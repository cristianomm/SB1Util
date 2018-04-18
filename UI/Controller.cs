using System;
using SAPbobsCOM;
using SAPbouiCOM;
using SB1Util.Connection;
using SB1Util.DB;
using SB1Util.Misc;
using SB1Util.Log;
using System.Collections.Generic;


namespace SB1Util.UI
{
    public abstract class Controller
    {

        protected B1Connection oConnection;
        protected UIUtils uiUtils;
        protected DBFacade oDBFacade;
        private LinkedList<FormMode> modeMonitors;
        protected Properties properties;
        protected Log.Logger log;
        protected bool bubbleEvent;
        protected static Controller instance;


        public struct FormMode
        {
            public string formID;
            public int mode;
        };



        protected Controller()
        {
            this.oConnection = B1Connection.getInstance();
            this.oDBFacade = DBFacade.getInstance();
            this.uiUtils = new UIUtils(oConnection.Company, oConnection.App);
            this.modeMonitors = new LinkedList<FormMode>();
            this.properties = Properties.getInstance();
            this.log = Logger.getInstance();
            log.setProperties(properties);
            log.setUiUtils(uiUtils);
            initAddon();
            log.prepare();
            log.log("Addon iniciado", Logger.LogType.INFO,null, false);
        }

        private void initAddon()
        {
            try
            {
                //Cria tabela de configuracao do addon
                oDBFacade.CreateTable("SB1_ADDON_CONFIG", "Configuração", BoUTBTableType.bott_NoObject);
                oDBFacade.CreateField("@SB1_ADDON_CONFIG", "Value", "Valor", BoFieldTypes.db_Memo, 255);

                oDBFacade.CreateTable("SB1_ADDON_LOG", "Log", BoUTBTableType.bott_NoObject);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "LogType", "LogType", BoFieldTypes.db_Alpha, 50);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "Text", "Text", BoFieldTypes.db_Memo, 3000);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "Date", "Date", BoFieldTypes.db_Date, 12);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "Time", "Time", BoFieldTypes.db_Alpha, 12);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "User", "User", BoFieldTypes.db_Alpha, 20);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "CompanyVersion", "Company Version", BoFieldTypes.db_Alpha, 20);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "AddonVersion", "Addon Version", BoFieldTypes.db_Alpha, 20);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "AddonName", "Addon Name", BoFieldTypes.db_Alpha, 50);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "LibVersion", "Library Version", BoFieldTypes.db_Alpha, 20);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "FormID", "Form", BoFieldTypes.db_Alpha, 20);
                oDBFacade.CreateField("@SB1_ADDON_LOG", "XMLException", "Internal Exception", BoFieldTypes.db_Memo, sizeof(int));
                oDBFacade.CreateField("@SB1_ADDON_LOG", "Operation", "Operation", BoFieldTypes.db_Memo, 100);

                //Remessas
                //campo para configurar a conta do estorno na remessa
                oDBFacade.CreateField("ODLN", "ShmtJE", "Lanc. de Remessa", BoFieldTypes.db_Alpha, 11, BoFldSubTypes.st_None);
                oDBFacade.CreateField("OUSG", "CstAcctCode", "Conta Substituta ao CPV", BoFieldTypes.db_Alpha, 20, BoFldSubTypes.st_None);
                oDBFacade.CreateField("OUSG", "Shipment", "Remessa", BoFieldTypes.db_Numeric, 11, BoFldSubTypes.st_None, "0", new string[] { "0|Não", "1|Sim" });
                
            }
            catch (Exception e) 
            {
                //Logger.logCaos("Erro na inicializacao basica. ", e);
                SB1ControlException.SB1ControlException.Save(e); 
            }
        }

        public UIUtils UIUtils
        {
            get { return this.uiUtils; }
            set { uiUtils = value; }
        }

        public B1Connection OConnection
        {
            get { return this.oConnection; }
        }

        public DBFacade ODBFacade
        {
            get { return oDBFacade; }
        }

        public void AddFilter(string containerUID, SAPbouiCOM.BoEventTypes eventType)
        {
            uiUtils.AddFilter(containerUID, eventType);
        }

        //executa configuracoes de inicializacao do addon - criacao de menus, 
        //adicao de filtros de eventos...
        public virtual void Init(string addonName)
        {
            try
            {
                log.pushOperation("Addon Initialization");
                CreateMenus();

                CreateFilters();

                if (Properties.getInstance().getByKey("addonConfInit").Equals("1"))
                {
                    CreateAddonDB();

                    AddUserData();

                    AddFormattedSearches();
                }
                else
                {
                    string msg = "Configuracao de addon nao realizada";
                    log.log(msg, Logger.LogType.INFO);
                }

                if (addonName == null)
                {
                    addonName = "Add-on " + Properties.getInstance().getByKey("addonName") + " Iniciado";
                }
                
                log.log(addonName, Logger.LogType.INFO);
            }
            catch (Exception e)
            {
                SB1ControlException.SB1ControlException.Save(e); 
            }
            log.releaseOperation();
        }

        /*
         * Cria os menus do addon
         */ 
        public abstract void CreateMenus();

        /*
         * Adiciona os filtros
         */
        public abstract void CreateFilters();

        /*
         * Cria o banco de dados do addon
         */ 
        public abstract void CreateAddonDB();

        //metodo para adicionar informacao predefinida em tabelas de Usuario/Sistema
        public abstract void AddUserData();

        //adiciona consultas formatadas no sistema
        public abstract void AddFormattedSearches();

        //recebe os eventos do SAP, para que sejam tratados, eh necessario
        //adicionar os filtros para os eventos no metodo Init()
        public virtual bool EventReceiver(string container, string componentID,
            bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            bubbleEvent = true;

            try
            {
                log.pushOperation("EventReceiver");

                BoEventTypes evnt = BoEventTypes.et_MENU_CLICK;
                if (pVal == null && objectInfo != null)
                {
                    evnt = objectInfo.EventType;
                }
                else if (pVal != null)
                {
                    evnt = pVal.EventType;
                }

                #region events
                switch (evnt)
                {
                    case BoEventTypes.et_ALL_EVENTS:
                        if (before)
                        {
                            bubbleEvent = beforeALL_EVENTS(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterALL_EVENTS(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_ITEM_PRESSED:
                        if (before)
                        {
                            bubbleEvent = beforeITEM_PRESSED(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterITEM_PRESSED(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_KEY_DOWN:
                        if (before)
                        {
                            bubbleEvent = beforeKEY_DOWN(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterKEY_DOWN(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_GOT_FOCUS:
                        if (before)
                        {
                            bubbleEvent = beforeGOT_FOCUS(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterGOT_FOCUS(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_LOST_FOCUS:
                        if (before)
                        {
                            bubbleEvent = beforeLOST_FOCUS(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterLOST_FOCUS(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_COMBO_SELECT:
                        if (before)
                        {
                            bubbleEvent = beforeCOMBO_SELECT(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterCOMBO_SELECT(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_CLICK:
                        if (before)
                        {
                            bubbleEvent = beforeCLICK(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterCLICK(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_DOUBLE_CLICK:
                        if (before)
                        {
                            bubbleEvent = beforeDOUBLE_CLICK(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterDOUBLE_CLICK(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_MATRIX_LINK_PRESSED:
                        if (before)
                        {
                            bubbleEvent = beforeMATRIX_LINK_PRESSED(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterMATRIX_LINK_PRESSED(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_MATRIX_COLLAPSE_PRESSED:
                        if (before)
                        {
                            bubbleEvent = beforeMATRIX_COLLAPSE_PRESSED(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterMATRIX_COLLAPSE_PRESSED(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_VALIDATE:
                        if (before)
                        {
                            bubbleEvent = beforeVALIDATE(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterVALIDATE(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_MATRIX_LOAD:
                        if (before)
                        {
                            bubbleEvent = beforeMATRIX_LOAD(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterMATRIX_LOAD(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_DATASOURCE_LOAD:
                        if (before)
                        {
                            bubbleEvent = beforeDATASOURCE_LOAD(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterDATASOURCE_LOAD(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_LOAD:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_LOAD(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_LOAD(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_UNLOAD:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_UNLOAD(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_UNLOAD(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_ACTIVATE:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_ACTIVATE(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_ACTIVATE(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_DEACTIVATE:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_DEACTIVATE(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_DEACTIVATE(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_CLOSE:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_CLOSE(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_CLOSE(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_RESIZE:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_RESIZE(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_RESIZE(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_KEY_DOWN:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_KEY_DOWN(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_KEY_DOWN(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_MENU_HILIGHT:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_MENU_HILIGHT(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_MENU_HILIGHT(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_PRINT:
                        if (before)
                        {
                            bubbleEvent = beforePRINT(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterPRINT(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_PRINT_DATA:
                        if (before)
                        {
                            bubbleEvent = beforePRINT_DATA(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterPRINT_DATA(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_EDIT_REPORT:
                        if (before)
                        {
                            bubbleEvent = beforeEDIT_REPORT(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterEDIT_REPORT(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_CHOOSE_FROM_LIST:
                        if (before)
                        {
                            bubbleEvent = beforeCHOOSE_FROM_LIST(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterCHOOSE_FROM_LIST(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_RIGHT_CLICK:
                        if (before)
                        {
                            bubbleEvent = beforeRIGHT_CLICK(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterRIGHT_CLICK(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_MENU_CLICK:
                        if (before)
                        {
                            bubbleEvent = beforeMENU_CLICK(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterMENU_CLICK(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_DATA_ADD:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_DATA_ADD(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_DATA_ADD(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_DATA_UPDATE:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_DATA_UPDATE(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_DATA_UPDATE(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_DATA_DELETE:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_DATA_DELETE(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_DATA_DELETE(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_FORM_DATA_LOAD:
                        if (before)
                        {
                            bubbleEvent = beforeFORM_DATA_LOAD(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterFORM_DATA_LOAD(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_PICKER_CLICKED:
                        if (before)
                        {
                            bubbleEvent = beforePICKER_CLICKED(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterPICKER_CLICKED(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_GRID_SORT:
                        if (before)
                        {
                            bubbleEvent = beforeGRID_SORT(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterGRID_SORT(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_Drag:
                        if (before)
                        {
                            bubbleEvent = beforeDrag(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterDrag(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                    case BoEventTypes.et_PRINT_LAYOUT_KEY:
                        if (before)
                        {
                            bubbleEvent = beforePRINT_LAYOUT_KEY(container, componentID, before, pVal, objectInfo);
                        }
                        else
                        {
                            bubbleEvent = afterPRINT_LAYOUT_KEY(container, componentID, before, pVal, objectInfo);
                        }
                        break;

                }
                #endregion

            }catch(Exception e)
            {
                bubbleEvent = false;
                SB1ControlException.SB1ControlException.Save(e); 
            }

            log.releaseOperation();

            return bubbleEvent;
        }






        //gambiarra...
        public void addFormModeMonitor(string formID)
        {
            try
            {
                if (getFormMode(formID).formID.Equals(""))
                {
                    FormMode a;
                    a.formID = formID;
                    a.mode = -1;
                    modeMonitors.AddLast(a);
                }
            }catch(Exception e)
            {
            }
        }

        public int getMode(string formID)
        {
            return getFormMode(formID).mode;
        }

        public FormMode getFormMode(string formID)
        {
            FormMode r;
            r.formID = "";
            r.mode = -1;

            try
            {
                foreach (FormMode fm in modeMonitors)
                {
                    if (fm.formID.Equals(formID))
                    {
                        r.formID = fm.formID;
                        r.mode = fm.mode;
                    }
                }
            }
            catch (Exception e)
            {
                SB1ControlException.SB1ControlException.Save(e); 
            }
            return r;
        }

        public void setMode(string formID, int mode)
        {
            LinkedListNode<FormMode> elem = modeMonitors.First;
            FormMode fm;
            try
            {
                while (elem != null)
                {
                    if (elem.Value.formID.Equals(formID))
                    {
                        modeMonitors.Remove(elem.Value);
                        fm.formID = elem.Value.formID;
                        fm.mode = mode;
                        modeMonitors.AddLast(fm);
                        break;
                    }
                    elem = elem.Next;
                }
            }
            catch (Exception e)
            {
                SB1ControlException.SB1ControlException.Save(e); 
            }
        }

        public LinkedList<FormMode> getModeMonitors()
        {
            return modeMonitors;
        }



        public virtual bool afterALL_EVENTS(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeALL_EVENTS(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterITEM_PRESSED(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeITEM_PRESSED(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterKEY_DOWN(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeKEY_DOWN(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterGOT_FOCUS(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeGOT_FOCUS(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterLOST_FOCUS(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeLOST_FOCUS(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterCOMBO_SELECT(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeCOMBO_SELECT(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterCLICK(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeCLICK(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterDOUBLE_CLICK(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeDOUBLE_CLICK(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterMATRIX_LINK_PRESSED(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeMATRIX_LINK_PRESSED(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterMATRIX_COLLAPSE_PRESSED(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeMATRIX_COLLAPSE_PRESSED(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterVALIDATE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeVALIDATE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterMATRIX_LOAD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeMATRIX_LOAD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterDATASOURCE_LOAD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeDATASOURCE_LOAD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_LOAD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_LOAD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_UNLOAD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_UNLOAD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_ACTIVATE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_ACTIVATE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_DEACTIVATE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_DEACTIVATE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_CLOSE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_CLOSE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_RESIZE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_RESIZE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_KEY_DOWN(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_KEY_DOWN(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_MENU_HILIGHT(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_MENU_HILIGHT(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterPRINT(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforePRINT(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterPRINT_DATA(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforePRINT_DATA(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterEDIT_REPORT(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeEDIT_REPORT(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterCHOOSE_FROM_LIST(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeCHOOSE_FROM_LIST(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterRIGHT_CLICK(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeRIGHT_CLICK(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterMENU_CLICK(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeMENU_CLICK(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_DATA_ADD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_DATA_ADD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_DATA_UPDATE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_DATA_UPDATE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_DATA_DELETE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_DATA_DELETE(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterFORM_DATA_LOAD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeFORM_DATA_LOAD(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterPICKER_CLICKED(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforePICKER_CLICKED(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterGRID_SORT(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeGRID_SORT(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterDrag(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforeDrag(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool afterPRINT_LAYOUT_KEY(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }

        public virtual bool beforePRINT_LAYOUT_KEY(string container, string componentID, bool before, ItemEvent pVal = null, IBusinessObjectInfo objectInfo = null)
        {
            return true;
        }
        
    }
}
