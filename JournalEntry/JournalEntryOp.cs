using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SB1Util.DB;
using SB1Util.Connection;
using SB1Util.Log;


/**
 * Classe para operacoes com Lancamento Contabil Manual
 * 
 * Autor: Cristiano M Martins
 * Version: 
 * 
 */
namespace SB1Util.JournalEntry
{
    /// <summary>
    /// Classe para operacoes com Lancamento Contabil Manual
    /// </summary>
    ///
    public class JournalEntryOp
    {
        private Logger logger;
        private DBFacade dbFacade;
        private B1Connection connection;

        public struct LedgerAccount
        {
            public string provisionName;
            public string profitCode;
            public string creditAccount;
            public string debitAccount;
            public double value;
        }


        public JournalEntryOp()
        {
            this.logger = Logger.getInstance();
            this.dbFacade = DBFacade.getInstance();
            this.connection = B1Connection.getInstance();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="jdtNum"></param>
        /// <returns></returns>
        public JournalEntries reverseEntries(int jdtNum)
        {
            JournalEntries retJE = null;
            try
            {
                JournalEntries existsJe = (JournalEntries)connection.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
                existsJe.GetByKey(jdtNum);

                retJE = copyJE(existsJe, false);

                //verifica se existe alguma linha configurada para gerar estorno
                int count = 0;
                JournalEntries_Lines lines = existsJe.Lines;
                while (count < lines.Count)
                {
                    lines.SetCurrentLine(count++);
                    string opt = lines.UserFields.Fields.Item("U_RevShp").Value;

                    if (opt.Equals("Sim"))
                    {

                    }

                    addDebitJEL(retJE, lines.ContraAccount
                        , lines.AccountCode, lines.ShortName
                        , lines.Credit, lines.LineMemo + " - (Reverso)", lines.DueDate);

                    lines.SetCurrentLine(count++);
                    addCreditJEL(retJE, lines.ContraAccount
                        , lines.AccountCode, lines.ShortName
                        , lines.Debit, lines.LineMemo + " - (Reverso)", lines.DueDate);

                }


            }
            catch (Exception e)
            {
                retJE = null;
            }
            return retJE;
        }


        /// <summary>
        /// Insere um Lancamento Contabil de Remessa.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public int insertShipmentJE(Documents doc)
        {
            int ret = 0;
            try
            {
                logger.pushOperation("insertShipmentJE");

                JournalEntryOp jop = new JournalEntryOp();

                //busca o existente
                JournalEntries existsJe = (JournalEntries)connection.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
                existsJe.GetByKey(doc.TransNum);

                JournalEntries remJE = copyJE(existsJe, false);
                remJE.Memo = "Lanc. Cont. Remessa - " + doc.DocEntry;
                remJE.UserFields.Fields.Item("U_DocRef").Value = doc.DocEntry;

                double val = 0;
                string memo = "";
                bool flag = false;
                LedgerAccount[] accts = new LedgerAccount[doc.Lines.Count];

                Recordset rs = dbFacade.Query(
                    "SELECT OSTA.Code, OSTA.SalesTax, OSTA.ARExpAct, SUM(DLN4.TaxSum)[value], DLN1.OcrCode " +
                    "FROM OSTA INNER JOIN DLN4 ON OSTA.Code = DLN4.StaCode AND OSTA.Type = DLN4.staType " +
                    "INNER JOIN DLN1 ON DLN1.DocEntry = DLN4.DocEntry AND DLN1.LineNum = DLN4.LineNum " + 
                    "WHERE ExpnsCode = -1 AND DLN1.Usage IN (SELECT ID FROM OUSG WHERE U_Shipment = 1) AND DLN4.DocEntry = " + doc.DocEntry +
                    "GROUP BY OSTA.Code, OSTA.SalesTax, OSTA.ARExpAct, DLN1.OcrCode "
                    );

                while (!rs.EoF)
                {
                    LedgerAccount ledger;
                    ledger.creditAccount = rs.Fields.Item("SalesTax").Value;
                    ledger.debitAccount = rs.Fields.Item("ARExpAct").Value;
                    ledger.provisionName = rs.Fields.Item("Code").Value;
                    ledger.value = rs.Fields.Item("value").Value;
                    ledger.profitCode = rs.Fields.Item("OcrCode").Value;
                    //lanca credito e debito de acordo com o imposto
                    addJournalEntryLines(remJE, ledger, ledger.value, remJE.DueDate, ledger.provisionName + " - Remessa", ledger.profitCode, ledger.profitCode);

                    rs.MoveNext();
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
                rs = null;
                System.GC.Collect();


                //caso o documento seja de entrega, estorna custos:
                //O valor deve ser igual ao do número 1.
                if (doc.DocObjectCode == BoObjectTypes.oDeliveryNotes)
                {
                    logger.log("Documento de Entrega, caso a utilizacao esteja configurada, o Custo sera estornado.", Logger.LogType.INFO, null, false);
                    
                    //para cada linha do doc...
                    for (int i = 0; i < doc.Lines.Count; i++)
                    {
                        doc.Lines.SetCurrentLine(i);
                        Document_Lines docLine = doc.Lines;
                        if (jop.validateShmtNFUsage(Convert.ToInt32(docLine.Usage))== 0)
                        {
                            double vl = dbFacade.Query("select StockValue from DLN1 " +
                                "where DocEntry = " + docLine.DocEntry + " and LineNum = " + docLine.LineNum).Fields.Item("StockValue").Value;

                            //busca a conta de custo da utilizacao
                            NotaFiscalUsage usage = connection.Company.GetBusinessObject(BoObjectTypes.oNotaFiscalUsage);
                            usage.GetByKey(Convert.ToInt32(docLine.Usage));
                            accts[i].debitAccount = usage.UserFields.Fields.Item("U_CstAcctCode").Value;
                            accts[i].value = vl;
                            accts[i].profitCode = docLine.CostingCode;
                            val += vl;
                            flag = true;
                        }
                        else
                        {
                            flag = false;
                        }

                    }

                    if (flag)
                    {
                        //busca as contas do lancamento existente
                        //LedgerAccount accts;
                        int count = 0, pos = 0;
                        JournalEntries_Lines lines = existsJe.Lines;
                        while (count < lines.Count)
                        {
                            lines.SetCurrentLine(count);
                            if (lines.AccountCode.StartsWith("42"))
                            {
                                accts[pos].creditAccount = lines.AccountCode;
                                accts[pos++].provisionName = lines.LineMemo;
                            }
                            count++;
                        }
                        //ajusta as contas das linhas
                        if (pos < accts.Length)
                        {
                            pos = 0;
                            string act = accts[0].creditAccount;
                            memo = accts[0].provisionName;
                            for (int i = 0; i < accts.Length; i++)
                            {
                                accts[i].creditAccount = act;
                            }
                        }

                        //1) Uma linha a Crédito, com a finalidade de estornar o lançamento a Débito na conta de custo 
                        //originado pelo documento “Entrega”, com valor correspondente à multiplicação das quantidades pelo custo unitário 
                        //dos itens em que houver a marcação “Y” para o campo “TaxOnly” na utilização.

                        //lanca os custos para cada linha do doc
                        //2) Uma linha a Débito, na conta de custo parametrizada na tabela de Utilizações, através do campo “U_CstAcctCode”, 
                        //para as linhas do documento “Entrega” em que a utilização possuir a marcação “Y” para o campo “TaxOnly”. 
                        foreach (LedgerAccount lact in accts)
                        {
                            addCreditJEL(remJE, lact.creditAccount, lact.debitAccount, lact.creditAccount
                            , lact.value, memo + " - (Reverso)", lines.DueDate, lact.profitCode);

                            addDebitJEL(remJE, lact.debitAccount, lact.creditAccount, lact.debitAccount
                            , lact.value, "Lanc. Custos - " + lact.debitAccount, lines.DueDate, lact.profitCode);
                        }

                    }
                    else
                    {
                        logger.log("Configuracao da Utilizacao nao permite estorno de Custos.", Logger.LogType.INFO);
                    }

                }

                //insere o lanc.
                remJE = insertJE(remJE);
                if (remJE == null)
                {
                    ret = -1;
                    logger.log("Erro ao adicionar Lancamento Contabil para Reverter Custo e Contabilizar impostos: " +
                    "Lancamento não adicionado.", Logger.LogType.ERROR);
                }
                else
                {
                    logger.log("Adicionado Lancamento Contabil para Reverter Custo e Contabilizar impostos ("+ remJE.JdtNum +")."
                    , Logger.LogType.INFO);
                    
                    doc.UserFields.Fields.Item("U_ShmtJE").Value = ""+remJE.JdtNum;
                    ret = doc.Update();
                    logger.log("Chave de Lancamento de Remessa ajustada no Documento("+doc.DocEntry+"). Retorno:" + ret, Logger.LogType.INFO);
                }

            }
            catch (Exception e)
            {
                ret = -1;
                logger.log("Erro ao Reverter Custos para o Lancamento " + doc.TransNum, Logger.LogType.ERROR, e, true);
            }

            logger.releaseOperation();

            return ret;
        }

        ///// <summary>
        ///// Cancela um Lancamento Contabil de Remessa
        ///// </summary>
        ///// <param name="document">Documento que esta sendo cancelado</param>
        ///// <returns></returns>
        //public int reverseShipmentJE(Documents document)
        //{
        //    int ret = 0;
        //    try
        //    {
        //        int transId=0;
        //        if(!string.IsNullOrEmpty(document.UserFields.Fields.Item("U_ShmtJE").Value))
        //        {
        //            transId =  Convert.ToInt32(document.UserFields.Fields.Item("U_ShmtJE").Value);    
        //        }
                
        //        //valida o numero passado
        //        if (transId <= 0)
        //        {
        //            logger.log("Nao havia Lancamento de Remessa para ser Cancelado (" + transId + "). "+
        //            "Chave para Lancamento invalida: " +transId, Logger.LogType.WARNING, null, false);
        //            return ret;
        //        }

        //        //realiza o cancelamento e verifica se pode ajustar o doc
        //        if (insertShipmentReverseJE(document) == 0)
        //        {
        //            //remove o lancamento de remessa do doc
        //            document.UserFields.Fields.Item("U_ShmtJE").Value = "";
        //            ret = document.Update();
        //            if (ret == 0)
        //            {
        //                logger.log("Lancamento para Cancelamento de " +
        //                    "Lancamentos de Remessa adicionado. Documento Ajustado.", Logger.LogType.INFO);
        //            }
        //            else
        //            {
        //                logger.log("Erro ao atualizar Documento para Cancelamento de Lancamentos de Remessa." +
        //                    "Documento nao Ajustado." +
        //                "Retorno SAP: " + ret + " - " + connection.Company.GetLastErrorDescription(), Logger.LogType.ERROR);
        //            }
        //        }
        //        else
        //        {
        //            ret = -1;
        //            logger.log("Lancamento de Cancelamento de Remessa não adicionado.", Logger.LogType.WARNING);
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        ret = -1;
        //        logger.log("Erro ao Cancelar Lancamento de Remessa: " + e.Message, Logger.LogType.ERROR, e);
        //    }
        //    return ret;
        //}


        /// <summary>
        /// Insere um Lancamento Contabil Manual para estorno do Lancamento de Remessa anteriormente adicionado
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public int insertShipmentReverseJE(Documents doc)
        {
            int ret = 0;
            try
            {
                JournalEntryOp jop = new JournalEntryOp();

                //busca o existente
                JournalEntries existsJe = (JournalEntries)connection.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
                existsJe.GetByKey(doc.TransNum);

                JournalEntries remJE = copyJE(existsJe, false);
                remJE.Memo = "Lanc. Cont. de Estorno de Remessa - " + doc.DocEntry;
                remJE.UserFields.Fields.Item("U_DocRef").Value = doc.DocEntry;

                double val = 0;
                string memo = "";
                bool flag = false;
                LedgerAccount[] accts = new LedgerAccount[doc.Lines.Count];

                Recordset rs = dbFacade.Query(
                    "SELECT OSTA.Code, OSTA.SalesTax, OSTA.ARExpAct, SUM(RDN4.TaxSum)[value], RDN1.OcrCode " +
                    "FROM OSTA INNER JOIN RDN4 ON OSTA.Code = RDN4.StaCode AND OSTA.Type = RDN4.staType " +
                    "INNER JOIN RDN1 ON RDN1.DocEntry = RDN4.DocEntry AND RDN1.LineNum = RDN4.LineNum " +
                    "WHERE ExpnsCode = -1 AND RDN1.Usage IN (SELECT ID FROM OUSG WHERE U_Shipment = 1) AND RDN4.DocEntry = " + doc.DocEntry +
                    "GROUP BY OSTA.Code, OSTA.SalesTax, OSTA.ARExpAct, RDN1.OcrCode "
                    );

                while (!rs.EoF)
                {
                    LedgerAccount ledger;
                    ledger.creditAccount = rs.Fields.Item("ARExpAct").Value;
                    ledger.debitAccount = rs.Fields.Item("SalesTax").Value;
                    ledger.provisionName = rs.Fields.Item("Code").Value;
                    ledger.value = rs.Fields.Item("value").Value;
                    ledger.profitCode = rs.Fields.Item("OcrCode").Value;
                    //lanca credito e debito de acordo com o imposto
                    addJournalEntryLines(remJE, ledger, ledger.value, remJE.DueDate, ledger.provisionName + " - Estorno de Remessa", ledger.profitCode, ledger.profitCode);

                    rs.MoveNext();
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
                rs = null;
                System.GC.Collect();


                //caso o documento seja de devolucao, estorna lancamento de impostos e custos
                //O valor deve ser igual ao do número 1.
                if (doc.DocObjectCode == BoObjectTypes.oReturns)
                {
                    logger.log("Documento para Devolução de Entrega, caso a utilizacao esteja configurada, o Custo sera estornado."
                        , Logger.LogType.INFO, null, false);

                    //para cada linha do doc...
                    for (int i = 0; i < doc.Lines.Count; i++)
                    {
                        doc.Lines.SetCurrentLine(i);
                        Document_Lines docLine = doc.Lines;

                        //if (docLine.BaseEntry == 0 && docLine.BaseLine == 0)
                        //{
                        //    string msg = "Linha " + docLine.LineNum + " não possui referência base.";
                        //    logger.log(msg, Logger.LogType.INFO);
                        //    int rm = connection.App.MessageBox(msg + "\nDeseja adicionar o Documento?", 1, "Sim", "Não");
                        //    if (rm != 1)
                        //    {
                        //        logger.log("", Logger.LogType.INFO);
                        //    }
                        //}


                        if (jop.validateShmtNFUsage(Convert.ToInt32(docLine.Usage)) == 0)
                        {
                            double vl = dbFacade.Query("select StockValue from RDN1 " +
                                "where DocEntry = " + docLine.DocEntry + " and LineNum = " + docLine.LineNum).Fields.Item("StockValue").Value;

                            //busca a conta de custo da utilizacao
                            NotaFiscalUsage usage = connection.Company.GetBusinessObject(BoObjectTypes.oNotaFiscalUsage);
                            usage.GetByKey(Convert.ToInt32(docLine.Usage));
                            accts[i].debitAccount = usage.UserFields.Fields.Item("U_CstAcctCode").Value;
                            accts[i].value = vl;
                            accts[i].profitCode = docLine.CostingCode;
                            val += vl;
                            flag = true;
                        }
                        else
                        {
                            flag = false;
                        }

                    }

                    if (flag)
                    {
                        //busca as contas do lancamento existente
                        //LedgerAccount accts;
                        int count = 0, pos = 0;
                        JournalEntries_Lines lines = existsJe.Lines;
                        while (count < lines.Count)
                        {
                            lines.SetCurrentLine(count);
                            if (lines.AccountCode.StartsWith("42"))
                            {
                                accts[pos].creditAccount = lines.AccountCode;
                                accts[pos++].provisionName = lines.LineMemo;
                            }
                            count++;
                        }
                        //ajusta as contas das linhas
                        if (pos < accts.Length)
                        {
                            pos = 0;
                            string act = accts[0].creditAccount;
                            memo = accts[0].provisionName;
                            for (int i = 0; i < accts.Length; i++)
                            {
                                accts[i].creditAccount = act;
                            }
                        }

                        foreach (LedgerAccount lact in accts)
                        {
                            addDebitJEL(remJE, lact.creditAccount, lact.debitAccount, lact.creditAccount
                            , lact.value, memo + " - (Reverso)", lines.DueDate, lact.profitCode);

                            addCreditJEL(remJE, lact.debitAccount, lact.creditAccount, lact.debitAccount
                            , lact.value, "Lanc. Custos - " + lact.debitAccount, lines.DueDate, lact.profitCode);
                        }

                    }
                    else
                    {
                        logger.log("Configuracao da Utilizacao nao permite estorno de Custos.", Logger.LogType.INFO);
                    }

                }

                //insere o lanc.
                remJE = insertJE(remJE);
                if (remJE == null)
                {
                    ret = -1;
                    logger.log("Erro ao adicionar Lancamento Contabil de Estorno de Remessa: " +
                    "Lancamento não adicionado.", Logger.LogType.ERROR);
                }
                else
                {
                    logger.log("Adicionado Lancamento Contabil de Estorno de Remessa (" + remJE.JdtNum + ")."
                    , Logger.LogType.INFO);

                    doc.UserFields.Fields.Item("U_ShmtJE").Value = "" + remJE.JdtNum;
                    ret = doc.Update();
                    logger.log("Chave para Lancamento de Estorno de Remessa ajustada no Documento(" + doc.DocEntry + "). Retorno:" + ret, Logger.LogType.INFO);
                }

            }
            catch (Exception e)
            {
                ret = -1;
                logger.log("Erro ao Reverter Custos para o Lancamento de Estorno de Remessa " + doc.TransNum, Logger.LogType.ERROR, e, true);
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="usageCode"></param>
        /// <returns>
        /// -2: Utilizacao esta marcada mas nao possui conta relacionada
        /// -1: Erro ao validar
        /// 0: Valido, Utilizacao esta marcada e possui conta relacionada
        /// 1: Utilizacao nao configurada
        /// </returns>
        public int validateShmtNFUsage(int usageCode){
            int ret = 0;
            try{
                NotaFiscalUsage usage = connection.Company.GetBusinessObject(BoObjectTypes.oNotaFiscalUsage);
                usage.GetByKey(usageCode);
                int yn = usage.UserFields.Fields.Item("U_Shipment").Value;
                string costAccount = usage.UserFields.Fields.Item("U_CstAcctCode").Value;

                //caso seja Remessa e tenha conta para lancar
                if (yn == 1 )
                {
                    if (string.IsNullOrEmpty(costAccount))
                    {
                        ret = -2;
                    }
                    else
                    {
                        //tem conta
                        ret = 0;
                    }
                }
                else
                {
                    ret = 1;
                }

            }catch(Exception e)
            {
                ret = -1;
            }

            return ret;
        }


        /// <summary>
        /// Cria mas nao adiciona um Lancamento Manual.
        /// </summary>
        /// <param name="memo"></param>
        /// <param name="refDate"></param>
        /// <param name="dueDate"></param>
        /// <param name="taxDate"></param>
        /// <param name="autoVat"></param>
        /// <param name="ref1"></param>
        /// <param name="ref2"></param>
        /// <param name="ref3"></param>
        /// <returns></returns>
        public JournalEntries createJE(string memo, DateTime refDate, DateTime dueDate, DateTime taxDate
            , BoYesNoEnum autoVat, string ref1=null, string ref2=null, string ref3=null)
        {
            JournalEntries je = null;
            try
            {
                je = connection.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
                je.ReferenceDate = refDate;
                je.DueDate = dueDate;
                je.TaxDate = taxDate;
                je.Memo = memo.Length>50? memo.Substring(0,50): memo;
                je.Reference = ref1;
                je.Reference2 = ref2;
                je.Reference3 = ref3;
                je.AutoVAT = autoVat;
                
            }
            catch (Exception e)
            {
                je = null;
                logger.log("Erro ao criar Lancamento Contabil Manual: " + e.Message, Logger.LogType.ERROR, e);
            }
            return je;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="je"></param>
        /// <returns></returns>
        public JournalEntries insertJE(JournalEntries je)
        {
            int r = 0;
            string msgSAP = "";
            int jdtNum = 0;

            try
            {

                r = je.Add();
                msgSAP = connection.Company.GetLastErrorDescription();
                string st = connection.Company.GetNewObjectKey();
                if (st.Equals("") || st.Length>11)
                {
                    jdtNum = 0;
                    logger.log("Chave de Lancamento Manual nao retornada: " + st, Logger.LogType.WARNING, null, false);
                }
                else
                {
                    jdtNum = Convert.ToInt32(connection.Company.GetNewObjectKey());
                }
                //caso tenha inserido com sucesso carrega novamente
                //para ajustar o objeto
                if (r == 0)
                {
                    je.GetByKey(jdtNum);
                    logger.log("Lancamento Contabil manual inserido com sucesso "+
                        "("+jdtNum+"). " + msgSAP, Logger.LogType.INFO);
                }
                else
                {
                    je = null;
                    logger.log("Erro ao inserir Lancamento Contabil Manual: " + 
                        "\nRetorno SAP: " + r + " - " + msgSAP, Logger.LogType.ERROR, null, false);
                }

            }
            catch (Exception e)
            {
                je = null;
                logger.log("Erro ao inserir Lancamento Contabil Manual: " + 
                    e.Message + " Retorno SAP: " + msgSAP, Logger.LogType.ERROR, e);
            }

            return je;
        }


        /// <summary>
        /// Insere um Lancamento Manual para Cancelamento do Lancamento informado por parametro.
        /// </summary>
        /// <param name="transId"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        public JournalEntries reverseJE(int transId, string memo = null)
        {
            JournalEntries retJE = null;
            try
            {
                //busca o existente
                JournalEntries existsJe = (JournalEntries)connection.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
                existsJe.GetByKey(transId);

                retJE = copyJE(existsJe, false);
                retJE.Memo = "Lanc. Cont. (Reverso) - " + transId;

                reverseJELines(existsJe, retJE);

                retJE = insertJE(retJE);
                if (retJE == null)
                {
                    logger.log("Erro ao adicionar Lancamento Contabil Reverso para Lancamento (" + transId + "): " +
                    "Lancamento não adicionado.", Logger.LogType.ERROR);
                }
                else
                {
                    logger.log("Lancamento Contabil Manual " + 
                        "("+retJE.JdtNum+") para reverter Lancamento ("+transId+") adicionado com sucesso.", Logger.LogType.INFO);
                }
            }
            catch (Exception e)
            {
                retJE = null;
                logger.log("Erro ao adicionar Lancamento Contabil Reverso para Lancamento ("+transId+"): " +
                    e.Message, Logger.LogType.ERROR, e);
            }
            return retJE;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalJE"></param>
        /// <param name="destinJE"></param>
        /// <returns></returns>
        public JournalEntries reverseJELines(JournalEntries originalJE, JournalEntries destinJE)
        {
            try
            {
                //insere as linhas
                int count = 0;
                JournalEntries_Lines lines = originalJE.Lines;
                while (count < lines.Count)
                {
                    lines.SetCurrentLine(count++);
                    addDebitJEL(destinJE, lines.ContraAccount
                        , lines.AccountCode, lines.ShortName
                        , lines.Credit, lines.LineMemo + " - (Reverso)", lines.DueDate, lines.CostingCode);

                    lines.SetCurrentLine(count++);
                    addCreditJEL(destinJE, lines.ContraAccount
                        , lines.AccountCode, lines.ShortName
                        , lines.Debit, lines.LineMemo + " - (Reverso)", lines.DueDate, lines.CostingCode);

                }

            }
            catch (Exception e)
            {
                logger.log("Erro ao copiar Linhas do Lancamento Contabil Manual " + originalJE.JdtNum
                    , Logger.LogType.ERROR, e, false);
            }

            return destinJE;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="journalEntry"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public JournalEntries reverseJELine(JournalEntries journalEntry, JournalEntries_Lines line)
        {
            try
            {
                addDebitJEL(journalEntry, line.ContraAccount
                        , line.AccountCode, line.ShortName
                        , line.Credit, line.LineMemo + " - (Reverso)", line.DueDate);

                addCreditJEL(journalEntry, line.ContraAccount
                    , line.AccountCode, line.ShortName
                    , line.Debit, line.LineMemo + " - (Reverso)", line.DueDate);


            }
            catch (Exception e)
            {
                logger.log("Erro ao reverter Linha do Lancamento Contabil Manual " + journalEntry.JdtNum
                    , Logger.LogType.ERROR, e, false);
            }

            return journalEntry;
        }


        /// <summary>
        /// Cria e retorna Lancamento Contabil Manual baseado nas informacoes do Lancamento 
        /// informado por parametro. As linhas do Lancamento Manual sao copiadas
        /// por padrao, podendo ser desabilitada atraves do parametro copyLines
        /// </summary>
        /// <param name="originalJE">Lancamento Contabil Manual original que deseja-se criar uma copia</param>
        /// <param name="copyLines">Quando true indica que ira copiar as linhas do Lancamento Contabil Manual, 
        /// false caso contrario</param>
        /// <returns>Lancamento Cont. Manual criado a partir das informacoes disponiveis no 
        /// Lancamento Original ou null em caso de erro</returns>
        public JournalEntries copyJE(JournalEntries originalJE, bool copyLines = true)
        {

            JournalEntries retJE = null;
            try
            {
                retJE = createJE("", originalJE.ReferenceDate, originalJE.DueDate, originalJE.TaxDate,
                    originalJE.AutoVAT, originalJE.Reference, originalJE.Reference2, originalJE.Reference3);

                retJE.Series = originalJE.Series;
                retJE.Indicator = originalJE.Indicator;
                retJE.ProjectCode = originalJE.ProjectCode;
                retJE.TransactionCode = originalJE.TransactionCode;
                retJE.DocumentType = originalJE.DocumentType;

                //verifica se as linhas do lancameto tambem serao copiadas
                if (copyLines)
                {
                    logger.log("Copia de linhas nao implementada.", Logger.LogType.WARNING, null, false);
                }

            }
            catch (Exception e)
            {
                retJE = null;
                logger.log("Erro ao copiar Lancamento Contabil Manual: " + e.Message
                    , Logger.LogType.ERROR, e);
            }
            return retJE;
        }

        
        /// <summary>
        /// Adiciona Linhas de Debito e Credito ao Lancamento Manual informado
        /// </summary>
        /// <param name="journalEntries"></param>
        /// <param name="ledgerAccount"></param>
        /// <param name="value"></param>
        /// <param name="date"></param>
        /// <param name="obsLine"></param>
        /// <param name="profitCodeCdt"></param>
        /// <param name="profitCodeDbt"></param>
        /// <returns></returns>
        public int addJournalEntryLines(JournalEntries journalEntries, 
            LedgerAccount ledgerAccount, double value, DateTime date, string obsLine, string profitCodeCdt = "", string profitCodeDbt = "")
        {
            int ret = 0;
            try
            {
                if (ledgerAccount.creditAccount.Equals("")
                    || ledgerAccount.debitAccount.Equals("")
                    || value == 0)
                {
                    ret = -2;
                    logger.log("Contas para Lancamento nao configuradas corretamente ou valor esta zerado: " +
                        "\nConta Credito: " + ledgerAccount.creditAccount +
                        "\nConta Debito: " + ledgerAccount.creditAccount + " Valor: " + value, Logger.LogType.WARNING, null, false);
                    return ret;
                }

                ret = addCreditJEL(
                    journalEntries, ledgerAccount.creditAccount, ledgerAccount.debitAccount,
                    ledgerAccount.creditAccount, value, obsLine, date, profitCodeCdt
                    );
                ret = addDebitJEL(
                    journalEntries, ledgerAccount.debitAccount, ledgerAccount.creditAccount,
                    ledgerAccount.debitAccount, value, obsLine, date, profitCodeDbt
                    );

            }
            catch (Exception e)
            {
                ret = -1;
                logger.log("Erro ao adicionar linhas no Lançamento Contábil Manual: " + 
                    journalEntries.JdtNum, Logger.LogType.ERROR, e);
            }
            return ret;
        }

        
        /// <summary>
        /// Adiciona um Lancamento Contabil Manual de Credito.
        /// </summary>
        /// <param name="journalEntries"></param>
        /// <param name="accountCode"></param>
        /// <param name="contraAccount"></param>
        /// <param name="shortName"></param>
        /// <param name="creditValue"></param>
        /// <param name="lineMemo"></param>
        /// <param name="dueDate"></param>
        /// <param name="profitCode"></param>
        /// <returns></returns>
        public int addCreditJEL(JournalEntries journalEntries,
            string accountCode, string contraAccount, string shortName,
            double creditValue, string lineMemo, DateTime dueDate, string profitCode = "")
        {
            int ret = 0;
            string msgSAP = "";
            try
            {
                journalEntries.Lines.AccountCode = accountCode;
                journalEntries.Lines.ContraAccount = contraAccount;
                journalEntries.Lines.ControlAccount = shortName;
                journalEntries.Lines.ShortName = shortName;
                journalEntries.Lines.Debit = 0;
                journalEntries.Lines.Credit = creditValue;
                journalEntries.Lines.TaxDate = System.DateTime.Now;
                journalEntries.Lines.DueDate = dueDate;
                journalEntries.Lines.LineMemo = lineMemo;

                //caso tenha sido informado um centro de custo e a conta seja I ou E, informa o centro de custo na linha
                if (!string.IsNullOrEmpty(profitCode))
                {
                    ChartOfAccounts account = connection.Company.GetBusinessObject(BoObjectTypes.oChartOfAccounts);
                    account.GetByKey(accountCode);
                    if (account.AccountType != BoAccountTypes.at_Other)
                    {
                        journalEntries.Lines.CostingCode = profitCode;
                        logger.log("Informado Centro de Custo " + profitCode +
                        " para a Conta " + accountCode + " na linha de Credito.", Logger.LogType.INFO, null, false);
                    }
                }
                journalEntries.Lines.Add();
                msgSAP = connection.Company.GetLastErrorDescription();
                logger.log("Adicionada Linha para Lançamento Contábil de Crédito. " +
                    "\nNumber: " + journalEntries.JdtNum + " Value: " + creditValue + " - " + lineMemo + ": " + msgSAP, Logger.LogType.INFO, null, false);
            }
            catch (Exception e)
            {
                ret = -1;
                msgSAP = connection.Company.GetLastErrorDescription();
                logger.log("Erro ao adicionar Linha para Lançamento Contábil de Crédito. " +
                    "\nNumber: " + journalEntries.JdtNum + " - " + lineMemo + ": " + msgSAP, Logger.LogType.ERROR, e, false);
            }

            return ret;
        }

        
        /// <summary>
        /// Adiciona um Lancamento Contabil Manual de Debito.
        /// </summary>
        /// <param name="journalEntries"></param>
        /// <param name="accountCode"></param>
        /// <param name="contraAccount"></param>
        /// <param name="shortName"></param>
        /// <param name="debitValue"></param>
        /// <param name="lineMemo"></param>
        /// <param name="dueDate"></param>
        /// <param name="profitCode"></param>
        /// <returns></returns>
        public int addDebitJEL(JournalEntries journalEntries,
            string accountCode, string contraAccount, string shortName,
            double debitValue, string lineMemo, DateTime dueDate, string profitCode = "")
        {
            int ret = 0;
            string msgSAP = "";
            try
            {
                journalEntries.Lines.AccountCode = accountCode;
                journalEntries.Lines.ContraAccount = contraAccount;
                journalEntries.Lines.ControlAccount = shortName;
                journalEntries.Lines.ShortName = shortName;
                journalEntries.Lines.Debit = debitValue;
                journalEntries.Lines.Credit = 0;
                journalEntries.Lines.TaxDate = System.DateTime.Now;
                journalEntries.Lines.DueDate = dueDate;
                journalEntries.Lines.LineMemo = lineMemo;

                //caso tenha sido informado um centro de custo e a conta seja I ou E, informa o centro de custo na linha
                if (!string.IsNullOrEmpty(profitCode))
                {
                    ChartOfAccounts account = connection.Company.GetBusinessObject(BoObjectTypes.oChartOfAccounts);
                    account.GetByKey(accountCode);
                    if (account.AccountType != BoAccountTypes.at_Other)
                    {
                        journalEntries.Lines.CostingCode = profitCode;
                        logger.log("Informado Centro de Custo " + profitCode +
                        " para a Conta " + accountCode + " na linha de Debito.", Logger.LogType.INFO, null, false);
                    }
                }

                journalEntries.Lines.Add();
                msgSAP = connection.Company.GetLastErrorDescription();
                logger.log("Adicionada Linha para Lançamento Contábil de Débito. "+
                    "\nNumber: " + journalEntries.JdtNum + " Value: " + debitValue + " - " + lineMemo + ": " + msgSAP, Logger.LogType.INFO, null, false);
            }
            catch (Exception e)
            {
                ret = -1;
                msgSAP = connection.Company.GetLastErrorDescription();
                logger.log("Erro ao adicionar Linha para Lançamento Contábil de Débito. "+
                    "\nNumber: " + journalEntries.JdtNum + " - " + lineMemo + ": " + msgSAP, Logger.LogType.ERROR, e, false);
            }

            return ret;
        }



        public int addJournalVoucher(string memo, DateTime refDate, DateTime dueDate, DateTime taxDate, LedgerAccount ledgerAccount)
        {
            int ret = 0;
            logger.pushOperation("addJournalVoucher");
            try
            {
                JournalVouchers jVoucher = dbFacade.Connection.Company.GetBusinessObject(BoObjectTypes.oJournalVouchers);

                jVoucher.JournalEntries.Memo = memo;
                jVoucher.JournalEntries.ReferenceDate = refDate;
                jVoucher.JournalEntries.DueDate = dueDate;
                jVoucher.JournalEntries.TaxDate = taxDate;

                jVoucher.JournalEntries.Lines.SetCurrentLine(0);
                jVoucher.JournalEntries.Lines.AccountCode = ledgerAccount.creditAccount;
                jVoucher.JournalEntries.Lines.Credit = ledgerAccount.value;
                jVoucher.JournalEntries.Lines.Add();

                jVoucher.JournalEntries.Lines.AccountCode = ledgerAccount.debitAccount;
                jVoucher.JournalEntries.Lines.Debit = ledgerAccount.value;
                jVoucher.JournalEntries.Lines.Add();

                insertJV(jVoucher);

            }
            catch (Exception e)
            {
                ret = -1;
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e);
            }
            finally
            {
                logger.releaseOperation();
            }

            return ret;

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="jv"></param>
        /// <returns></returns>
        public JournalVouchers insertJV(JournalVouchers jv)
        {
            int r = 0;
            string msgSAP = "";
            int jdtNum = 0;
            logger.pushOperation("insertJV");
            try
            {

                r = jv.Add();
                msgSAP = connection.Company.GetLastErrorDescription();
                string st = connection.Company.GetNewObjectKey();
                if (st.Equals("") || st.Length > 11)
                {
                    jdtNum = 0;
                    logger.log("Chave de Pré Lancamento Manual nao retornada: " + st, Logger.LogType.WARNING, null, false);
                }
                else
                {
                    jdtNum = Convert.ToInt32(connection.Company.GetNewObjectKey());
                }
                //caso tenha inserido com sucesso carrega novamente
                //para ajustar o objeto
                if (r == 0)
                {
                    //jv. GetByKey(jdtNum);
                    logger.log("Pré Lancamento Contabil inserido com sucesso " +
                        "(" + jdtNum + "). " + msgSAP, Logger.LogType.INFO);
                }
                else
                {
                    jv = null;
                    logger.log("Erro ao inserir Pre-Lancamento Contabil: " +
                        "\nRetorno SAP: " + r + " - " + msgSAP, Logger.LogType.ERROR, null, false);
                }

            }
            catch (Exception e)
            {
                jv = null;
                logger.log("Erro ao inserir Lancamento Contabil Manual: " +
                    e.Message + " Retorno SAP: " + msgSAP, Logger.LogType.ERROR, e);
            }
            finally
            {
                logger.releaseOperation();
            }

            return jv;
        }



    }
}
