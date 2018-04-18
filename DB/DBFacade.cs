using System;
using SAPbobsCOM;
using SB1Util.Connection;
using SB1Util.Misc;
using SB1Util.Log;


namespace SB1Util.DB
{
    public class DBFacade
    {

        public enum OperationType
        {
            PERSIST, REMOVE, UPDATE
        }

        private int transLevel;
        private string logMessage;
        private Logger logger;
        private B1Connection oConnection;
        private static DBFacade instance;

        public DBFacade()
        {
            this.logger = Logger.getInstance();
            this.oConnection = B1Connection.getInstance();
        }

        
        public static DBFacade getInstance()
        {
            if (instance == null)
            {
                instance = new DBFacade();
            }
            return instance;
        }


        public B1Connection Connection
        {
            get { return this.oConnection; }
        }


        #region Operacoes

        private SAPObject persist(SAPObject obj)
        {
            obj.add();
            return obj;
        }

        private SAPObject remove(SAPObject obj)
        {
            obj.remove();
            return obj;
        }

        private SAPObject update(SAPObject obj)
        {
            obj.update();
            return obj;
        }

        #endregion


        public SAPObject execute(SAPObject obj, OperationType type)
        {
            switch(type)
            {
                case OperationType.PERSIST:
                    return persist(obj);

                case OperationType.REMOVE:
                    return remove(obj);

                case OperationType.UPDATE:
                    return update(obj);

                default:
                    return obj;
                    
            }
        }


        public Object LoadUserObject(ObjectMap map, string key, string userTable)
        {
            Object obj = null;
            try{
                //remove colchetes para nao dar erro
                userTable = userTable.Replace("[", "");
                userTable = userTable.Replace("]", "");
                userTable = userTable.Replace("@", "");
                UserTable usertable = oConnection.Company.UserTables.Item(userTable);

                obj = usertable.GetByKey(key);
                
            }catch(Exception e)
            {
                obj = null;                
                logMessage = "Erro ao carregar Objeto de usuario:" + userTable + ": " + e.Message;
                logger.log(logMessage, Logger.LogType.ERROR, e, false);
                SB1ControlException.SB1ControlException.Save(e);
            }

            return obj;
        }


        public UserTable getUserTable(string userTable)
        {
            UserTable usertable = null;
            try
            {
                //remove colchetes para nao dar erro
                userTable = userTable.Replace("[","");
                userTable = userTable.Replace("]","");
                userTable = userTable.Replace("@","");

                usertable = oConnection.Company.UserTables.Item(userTable);
            }
            catch (Exception e)
            {
                usertable = null;
                logMessage = "Erro buscando Tabela de usuario:" + usertable + ": " + e.Message;
                logger.log(logMessage, Logger.LogType.ERROR, e, false);
                SB1ControlException.SB1ControlException.Save(e);
            }

            return usertable;
        }

        public void removeUserTable(string tableName)
        {
            int err = 0;
            string errMsg = "";
            UserTablesMD userTable = null;
            try
            {
                userTable = ((UserTablesMD)(oConnection.Company.GetBusinessObject(BoObjectTypes.oUserTables)));
                
                System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                System.GC.Collect();

                userTable = ((UserTablesMD)(oConnection.Company.GetBusinessObject(BoObjectTypes.oUserTables)));
                userTable.GetByKey(tableName);

                if (userTable.Remove() == 0)
                {
                    logMessage = "Tabela de Usuário " + tableName + " removida.";
                    logger.log(logMessage, Logger.LogType.INFO);
                }
                else
                {
                    oConnection.Company.GetLastError(out err, out errMsg);
                    logMessage = "Erro ao remover Tabela de Usuário: '"
                        + tableName + "': " + errMsg + " : " + err;
                    logger.log(logMessage, Logger.LogType.ERROR, null, false);
                }
            }
            catch (Exception e)
            {
                oConnection.Company.GetLastError(out err, out errMsg);
                logMessage = "Erro ao remover Tabela de Usuário: '"
                    + tableName + "': " + errMsg + " : " + err + " : " + e.Message;
                logger.log(logMessage, Logger.LogType.ERROR, e);
                SB1ControlException.SB1ControlException.Save(e);
            }

        }

        public void removeUserField(string tableName, string tableField)
        {
            int err = 0;
            string errMsg = "";
            UserFieldsMD oUserFields = null;
            try
            {
                //bool exist = UserFieldsExist(tableName, tableField);
                oUserFields = ((UserFieldsMD)(oConnection.Company.GetBusinessObject(BoObjectTypes.oUserFields)));
                
                tableField = tableField.Replace("U_", "");

                Recordset rs = Query(
                    "SELECT FieldID FROM CUFD " +
                    "WHERE TableID = '" + tableName + "' AND AliasID = '" + tableField + "'"
                );

                //se o campo existe, remove
                if (rs.RecordCount >0)
                {
                    oUserFields.GetByKey(tableName, rs.Fields.Item("FieldID").Value);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
                    System.GC.Collect();

                    err = oUserFields.Remove();
                    errMsg = oConnection.Company.GetLastErrorDescription();
                    if (err == 0)
                    {
                        logger.log("Campo de Usuário " + tableField + " da tabela " + 
                            tableName +  " Removido com sucesso.", Logger.LogType.WARNING);
                    }
                    else
                    {
                        logger.log("Erro ao remover Campo de Usuário " + tableField 
                            + " da tabela " + tableName + " Retorno SAP: " + err + " - " + errMsg, Logger.LogType.ERROR);
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserFields);
                System.GC.Collect();

            }
            catch(Exception e)
            {
                oUserFields = null;
                System.GC.Collect();
                logger.log("Erro ao remover Campo de Usuário " + tableField
                    + " da tabela " + tableName + " Retorno SAP: " + err + " - " + errMsg, Logger.LogType.ERROR, e);

                SB1ControlException.SB1ControlException.Save(e);
            }

        }



        /*
         * cria banco de dados de acordo com a especificacao 
         * no arquivo passado por parametro.
         */ 
        public void CreateUserDB(string fileName)
        {


        }

        //Cria tabela de usuario
        public void CreateTable(string tableName, string tableDescr, BoUTBTableType tableType)
        {
            int err = 0;
            string errMsg = "";
            UserTablesMD oUserTablesMD = null;

            try
            {
                oUserTablesMD = ((UserTablesMD)(oConnection.Company.GetBusinessObject(BoObjectTypes.oUserTables)));
                System.GC.Collect();

                //Verifica se a tabela já existe
                if (oUserTablesMD.GetByKey(tableName))
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserTablesMD);
                    System.GC.Collect();
                }
                else
                {
                    if (tableName.Length >= 20)
                    {
                        throw new Exception("Nome de tabela maior que 19 caracteres: " + tableName + " Len: " + tableName.Length);
                    }
                    if (tableDescr.Length >= 30)
                    {
                        throw new Exception("Descrição de tabela maior que 29 caracteres: " + tableDescr + " Len: " + tableDescr.Length);
                    }

                    oUserTablesMD.TableName = tableName;
                    oUserTablesMD.TableDescription = tableDescr;
                    oUserTablesMD.TableType = tableType;
                    

                    //mostra msg
                    logMessage = "Criando Tabela de Usuário: '" + tableName;
                    logger.log(logMessage, Logger.LogType.INFO);


                    if (oUserTablesMD.Add() != 0)
                    {
                        oConnection.Company.GetLastError(out err, out errMsg);
                        logMessage = "Erro ao criar Tabela de Usuário: '"
                            + tableName + "': " + errMsg + " : " + err;
                        logger.log(logMessage, Logger.LogType.ERROR, null, false);
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserTablesMD);
                    oUserTablesMD = null;
                    System.GC.Collect();
                }
            }
            catch (Exception e)
            {
                oConnection.Company.GetLastError(out err, out errMsg);
                logMessage = "Erro ao criar Tabela de Usuário: '" + tableName + "': " + errMsg + " : " + err;
                logger.log(logMessage, Logger.LogType.ERROR, e);

                //SB1ControlException.SB1ControlException.Save(e);
                if (oUserTablesMD != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserTablesMD);
                }
                oUserTablesMD = null;
                System.GC.Collect();
                if (oUserTablesMD != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserTablesMD);
                }
                oUserTablesMD = null;
                System.GC.Collect();
            }
        }

        //Cria campo de usuario
        public void CreateField(string tableName, string tableField, string fieldDescr, 
            BoFieldTypes type, int size, BoFldSubTypes subType = SAPbobsCOM.BoFldSubTypes.st_None, 
            string defaultValue="", string[] validValues=null, BoYesNoEnum mandatory = BoYesNoEnum.tNO)
        {

            int err = 0;
            string errMsg = "";
            UserFieldsMD oUserFieldsMD = null;

            try
            {

                bool exist = UserFieldsExist(tableName, tableField);
                oUserFieldsMD = ((UserFieldsMD)(oConnection.Company.GetBusinessObject(BoObjectTypes.oUserFields)));


                //se o campo nao existe, cria
                if (!exist)
                {
                    if (tableField.Length >= 20)
                    {
                        throw new Exception("Nome de campo maior que 19 caracteres: " + tableField + " Len: " + tableField.Length);
                    }
                    if (fieldDescr.Length >= 30)
                    {
                        throw new Exception("Descrição de campo maior que 29 caracteres: " + fieldDescr + " Len: " + fieldDescr.Length);
                    }
                    //Adiciona o campo
                    oUserFieldsMD.TableName = tableName;//.Length >= 20 ? tableName.Substring(0, 20) : tableName;
                    oUserFieldsMD.Name = tableField;
                    oUserFieldsMD.Description = fieldDescr;
                    oUserFieldsMD.Type = type;
                    oUserFieldsMD.Size = size;
                    oUserFieldsMD.EditSize = size;
                    oUserFieldsMD.SubType = subType;
                    oUserFieldsMD.DefaultValue = defaultValue;
                    oUserFieldsMD.Mandatory = mandatory;

                    if (validValues != null)
                    {
                        IValidValuesMD vals = oUserFieldsMD.ValidValues;
                        
                        for (int i = 0; i < validValues.Length; i++)
                        {
                            string[] dv = validValues[i].Trim().Split(new char[] { ',', '|', '=' });
                            vals.SetCurrentLine(i);
                            vals.Value = dv[0];
                            vals.Description = dv[1];
                            vals.Add();
                        }

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(vals);
                        vals = null;
                        System.GC.Collect();

                    }

                    if (oUserFieldsMD.Add() != 0)
                    {
                        oConnection.Company.GetLastError(out err, out errMsg);
                        logMessage = "Erro ao criar Campo de Usuário: '" +
                            tableField + "' em '" + tableName + "': " + errMsg + " : " + err;
                        logger.log(logMessage, Logger.LogType.ERROR, null, false);
                    }
                    else
                    {
                        //mostra msg
                        logMessage = "Criado campo de Usuário: '" + tableField + "' em '" + tableName;
                        logger.log(logMessage, Logger.LogType.INFO);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserFieldsMD);
                    oUserFieldsMD = null;
                    System.GC.Collect();
                }
            }
            catch (Exception e)
            {
                oConnection.Company.GetLastError(out err, out errMsg);
                logMessage = "Erro ao criar Campo de Usuário: '"
                    + tableField + "' em '" + tableName + "': " + errMsg + " : " + err;
                logger.log(logMessage, Logger.LogType.ERROR, e);

                //SB1ControlException.SB1ControlException.Save(e);

                if (oUserFieldsMD != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserFieldsMD);
                }
                oUserFieldsMD = null;
                System.GC.Collect();

                if (oUserFieldsMD != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserFieldsMD);
                }
                oUserFieldsMD = null;
                System.GC.Collect();
            }
            
        }


        /*
         * Verifica se um campo de usuario existe, retornando true, 
         * caso o campo ja exista na tabela ou em caso de erro, ou false caso contrario.
         * 
         */
        private bool UserFieldsExist(string tableName, string tableField)
        {
            bool ret = false;
            Recordset oRecordSet = ((Recordset)(oConnection.Company.GetBusinessObject(BoObjectTypes.BoRecordset)));
            
            string query = "SELECT  1 [result]"
            + " FROM SYS.OBJECTS so INNER JOIN SYS.COLUMNS sc "
            + " ON so.object_id = sc.object_id "
            + " WHERE so.schema_id = 1 "
            + " AND so.name like '%" + tableName + "%' and sc.name = 'U_" + tableField + "'";

            try
            {
                oRecordSet.DoQuery(query);

                int retF = (int)oRecordSet.Fields.Item("result").Value;
                
                ret = (retF == 1? true: false);

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet);
                oRecordSet = null;
                System.GC.Collect();

            }catch(Exception e)
            {
                logMessage = e.Message;
                //retorna true pois nao deve tentar adicionar em caso de erro
                ret = true;
                logger.log(logMessage, Logger.LogType.ERROR, e);
                SB1ControlException.SB1ControlException.Save(e);
                
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet);
                oRecordSet = null;
                System.GC.Collect();
            }
                        
            return ret;

        }


        public Recordset Query(string query)
        {
            Recordset oRecordSet = null;
            try
            {
                oRecordSet = ((Recordset)(oConnection.Company.GetBusinessObject(BoObjectTypes.BoRecordset)));
                oRecordSet.DoQuery(query);
            }
            catch (Exception e)
            {
                oRecordSet = null;
                logMessage = "Erro ao executar consulta:" + query + ": " + e.Message;
                logger.log(logMessage, Logger.LogType.ERROR, e,false);
                Exception ex = new Exception(logMessage, e);
                SB1ControlException.SB1ControlException.Save(ex);
            }

            return oRecordSet;
        }

        


        /*
         * retorna a maior chave na tabela passada por parametro, caso a tabela nao possua 
         * registros, retorna "0"(string), caso nao exista ou ocorra algum erro, retorna null
         */ 
        public string Max(string tableName, string fieldName = "Code", string where = "")
        {
            return MIN_MAX(tableName, "MAX", fieldName, where);
        }


        /*
         * retorna a menor chave na tabela passada por parametro, caso a tabela nao possua 
         * registros, retorna "0"(string), caso nao exista ou ocorra algum erro, retorna null
         */ 
        public string Min(string tableName, string fieldName = "Code", string where = "")
        {
            return MIN_MAX(tableName, "MIN", fieldName, where);
        }

        /*
         * realiza a consulta para as funcoes Min ou Max do banco
         */ 
        private string MIN_MAX(string tableName, string func, string fieldName = "Code", string where="")
        {
            string min_max = "0";
            
            try
            {
                Recordset rs = (Recordset)oConnection.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(
                    "SELECT ISNULL(" + func + "(CONVERT(INT, " + fieldName + ")), 0)[" + fieldName + 
                    "] FROM " + tableName + (where.Equals("") ? "" : " WHERE " + where) );
                min_max = Convert.ToString(rs.Fields.Item(fieldName).Value);
            }
            catch (Exception e)
            {
                min_max = null;
                //oConnection.App.SetStatusBarMessage(e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, true);
                logMessage = "Erro ao buscar "+ func + " de " + tableName + ": " + e.Message;
                logger.log(logMessage, Logger.LogType.ERROR, e);
                SB1ControlException.SB1ControlException.Save(e);
            }

            return min_max;
        }


        public string nextObjectKey(BoObjectTypes type, string subType, string seriesName)
        {
            string ret = "";
            try
            {

                Series oSeries;
                SeriesService oSeriesService;
                SeriesParams oSeriesParams;

                //string qry = "SELECT NextNumber,  FROM NNM1 WHERE ObjectCode = " + (int)type;
                //qry += bplID > 0 ? " and BPLid = " + bplID : "";
                //qry += !string.IsNullOrEmpty(seriesName) ? " and SeriesName = '" + seriesName + "'" : "";

                //ret = queryValue("NextNumber", qry, "").ToString();

                Recordset rs = Query("SELECT Series FROM NNM1 "+
                    " WHERE ObjectCode = " + (int)type +
                    " and DocSubType = '" + subType + "' and SeriesName = '" + seriesName + "'");
                                
                oSeriesService = oConnection.Company.GetCompanyService().GetBusinessService(ServiceTypes.SeriesService);
                oSeriesParams = oSeriesService.GetDataInterface(SeriesServiceDataInterfaces.ssdiSeriesParams);
                oSeriesParams. Series = rs.Fields.Item("Series").Value;
                oSeries = oSeriesService.GetSeries(oSeriesParams);
                
                ret = oSeries.Prefix + 
                    (oSeries.NextNumber + 
                    oSeries.Suffix).PadLeft(oSeries.DigitNumber, '0');
                    
            }
            catch (Exception e)
            {
                ret = "";
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e, false);
            }

            return ret;
        }



        public void truncateUserTable(string userTable)
        {
            try
            {
                userTable = userTable.Replace("[", "");
                userTable = userTable.Replace("]", "");
                userTable = userTable.Replace("@", "");

                userTable = "[@" + userTable + "]";

                UserTable us = getUserTable(userTable);
                Recordset rs = Query("SELECT Code FROM " + userTable);

                while (!rs.EoF)
                {
                    us.GetByKey(rs.Fields.Item("Code").Value);
                    us.Remove();
                    rs.MoveNext();
                }

            }
            catch (Exception e)
            {
                logger.log("Erro ao truncar tabela de usuario (" + userTable + "): " + e.Message, Logger.LogType.ERROR, e, false);
                SB1ControlException.SB1ControlException.Save(e);
            }

        }


        public Object queryValue(string column, string query, Object valueIfNull)
        {
            Object ret = valueIfNull;
            logger.pushOperation("DBFacade.queryValue");
            try
            {
                ret = Query(query).Fields.Item(column).Value;
                
                if (ret == null || string.IsNullOrEmpty(ret.ToString()))
                {
                    ret = valueIfNull;
                }

            }
            catch (Exception e)
            {
                ret = valueIfNull;
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            finally
            {
                logger.releaseOperation();
            }
            return ret;
        }


        public string formatDate(DateTime date)
        {

            string dtstr = "";
            logger.pushOperation("DBFacade.formatDate");
            try
            {
                Recordset rs = Query("sp_helplanguage @language = @@language");

                rs.MoveFirst();
                if (!rs.EoF)
                {
                    switch ((string)rs.Fields.Item("dateformat").Value)
                    {
                        //mdy, dmy, ymd, ydm, myde dym
                        case "mdy":
                            dtstr = date.ToString("MM-dd-yyyy");
                            break;

                        case "dmy":
                            dtstr = date.ToString("dd-MM-yyyy");
                            break;

                        case "ymd":
                            dtstr = date.ToString("yyyy-MM-dd");
                            break;

                        case "ydm":
                            dtstr = date.ToString("yyyy-dd-MM");
                            break;

                        case "myd":
                            dtstr = date.ToString("MM-yyyy-dd");
                            break;

                        case "dym":
                            dtstr = date.ToString("dd-yyyy-MM");
                            break;

                    }

                }

            }
            catch (Exception e)
            {
                dtstr = "";
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            finally
            {
                logger.releaseOperation();
            }

            return dtstr;
        }



        
        public bool startTransaction()
        {
            bool ret = false;
            logger.pushOperation("DBFacade.startTransaction");
            try
            {
                if (!oConnection.Company.InTransaction)
                {
                    oConnection.Company.StartTransaction();
                    ret = true;
                }
                else
                {
                    transLevel++;
                }
            }
            catch (Exception e)
            {
                ret = false;
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e);
            }
            finally
            {
                logger.releaseOperation();
            }
            return ret;

        }

        public bool commit()
        {
            bool ret = false;
            logger.pushOperation("DBFacade.commit");
            try
            {
                if (oConnection.Company.InTransaction && transLevel == 0)
                {
                    oConnection.Company.EndTransaction(BoWfTransOpt.wf_Commit);
                }
                else if (!oConnection.Company.InTransaction && transLevel >= 0)
                {
                    transLevel = 0;
                }
                else
                {
                    transLevel--;
                }
            }
            catch (Exception e)
            {
                ret = false;
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e);
            }
            finally
            {
                logger.releaseOperation();
            }
            return ret;

        }

        public bool rollback()
        {
            bool ret = false;
            logger.pushOperation("DBFacade.rollback");
            try
            {
                if (oConnection.Company.InTransaction)
                {
                    oConnection.Company.EndTransaction(BoWfTransOpt.wf_RollBack);
                    ret = true;
                }
            }
            catch (Exception e)
            {
                ret = false;
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e);
            }
            finally
            {
                logger.releaseOperation();
                transLevel = 0;
            }
            return ret;

        }

    }
}
