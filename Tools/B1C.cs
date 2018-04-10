using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;
using SB1Util.Connection;
using SB1Util.DB;
using SB1Util.UI;
using SB1Util.Log;

namespace SB1Util.Tools
{
    public class B1C
    {

        private B1Connection connection;
        private DBFacade dbFacade;
        private UIUtils uiUtils;


        public B1C()
        {
            this.connection = B1Connection.getInstance();
            this.dbFacade = DBFacade.getInstance();
            this.uiUtils = new UIUtils(connection);
        }

        public void removeBusinessPartners(string filter)
        {
            try
            {
                string query = "select CardCode FROM OCRD";

                if (filter.Length > 0)
                {
                    query += " WHERE " + filter;
                }


                Recordset rs = dbFacade.Query(query);
                int countRem = 0;

                if (rs.RecordCount > 0)
                {
                    int opt = uiUtils.MessageBox("Poderão ser removidos " + rs.RecordCount + " Parceiros de Negócio. Tem certeza que deseja continuar?", 1, "Remover", "Cancelar");

                    if (opt == 1)
                    {
                        //Start the progress bar
                        ProgressBar oProgBar = connection.App.StatusBar.CreateProgressBar("Progress Bar", rs.RecordCount, true);
                        BusinessPartners bp = connection.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners);


                        while (!rs.EoF)
                        {
                            bp.GetByKey(rs.Fields.Item("CardCode").Value);
                            string remText = bp.CardCode + " - " + bp.CardName;
                            int rem = bp.Remove();
                            //vai para o proximo registro
                            rs.MoveNext();

                            if (rem == 0)
                            {
                                countRem++;
                                Logger.getInstance().log("Remoção de BP: Removido: " + remText, Logger.LogType.INFO, null, false);
                                oProgBar.Text = "Removido: " + remText;
                                //avanca a progress bar
                                oProgBar.Value += 1;
                            }
                            else
                            {
                                remText += " Não pode ser removido: " + connection.Company.GetLastErrorCode() + " - " + connection.Company.GetLastErrorDescription();
                                Logger.getInstance().log("Remoção de BP: " + remText, Logger.LogType.WARNING, null, false);
                            }
                        }

                        oProgBar.Stop();

                        uiUtils.MessageBox("Foram Removidos " + countRem + " Parceiros de Negócio.");
                    }
                }
                else
                {
                    uiUtils.MessageBox("Sem PN para remover!");
                }


            }
            catch (Exception e)
            {
                Logger.getInstance().log("Erro ao remover Parceiros de Negócio", Logger.LogType.INFO, e);
            }
        }


        public void copyDocuments(BoDocumentTypes docType)
        {
            try
            {
                SAPbobsCOM.Company comp = new SAPbobsCOM.Company();
                comp.CompanyDB = "SBO_Digistar";
                comp.DbServerType = BoDataServerTypes.dst_MSSQL2008;
                comp.Server = "B1DEV";
                comp.UserName = "manager";
                comp.Password = "1234";
                comp.DbUserName = "sa";
                comp.DbPassword = "1234";
                comp.Connect();

                Documents doc = (Documents)comp.GetBusinessObject(BoObjectTypes.oInvoices);
                

            }
            catch (Exception e)
            {

            }
        }

    }
}
