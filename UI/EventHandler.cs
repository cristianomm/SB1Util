using System;
using SAPbobsCOM;
using SAPbouiCOM;
using System.Diagnostics;


namespace SB1Util.UI
{
    public class EventHandler 
    {
        private SAPbobsCOM.Company oCompany;
        private SAPbouiCOM.Application oApp;
        private Controller oController;
        
        
        public EventHandler(SAPbobsCOM.Company company, SAPbouiCOM.Application app, Controller controller)
        {
            if (company == null)
            {
                throw new Exception("Null Parameters during EventHandler creation: SAPbobsCOM.Company");
            }
            if (app == null)
            {
                throw new Exception("Null Parameters during EventHandler creation: SAPbouiCOM.Application");
            }
            this.oApp = app;
            this.oCompany = company;
            oController = controller;
            
        }

        public void Init()
        {
            try
            {
                oController.Init(null);
                oApp.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(AppEvent);
                oApp.ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(ItemEvent);
                oApp.FormDataEvent += new SAPbouiCOM._IApplicationEvents_FormDataEventEventHandler(FormDataEvent);
                oApp.MenuEvent += new SAPbouiCOM._IApplicationEvents_MenuEventEventHandler(MenuEvent);
            }
            catch (Exception e)
            {
                ItsControlException.ItsControlException.Save(e); 
                oApp.SetStatusBarMessage(e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, true);
            }
        }

        public void Init(string addonName)
        {
            try
            {
                oController.Init(addonName);
                oApp.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(AppEvent);
                oApp.ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(ItemEvent);
                oApp.FormDataEvent += new SAPbouiCOM._IApplicationEvents_FormDataEventEventHandler(FormDataEvent);
                oApp.MenuEvent += new SAPbouiCOM._IApplicationEvents_MenuEventEventHandler(MenuEvent);
            }
            catch (Exception e)
            {
                ItsControlException.ItsControlException.Save(e); 
                oApp.SetStatusBarMessage(e.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, true);
            }
        }

        public void ExitAddon()
        {
            try
            {
                //Process p = Process.GetProcessesByName(AppDomain);
                Log.Logger.getInstance().Prepared = false;
                oCompany.Disconnect();
                oApp = null;
                oCompany = null;
            }
            catch (Exception e)
            {
                ItsControlException.ItsControlException.Save(e); 
            }
        }



        public void AppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case BoAppEventTypes.aet_CompanyChanged:
                    Log.Logger.getInstance().log("Compania alterada, terminado execucao do addon.", Log.Logger.LogType.INFO, null, false);
                    ExitAddon();
                    
                    break;

                case BoAppEventTypes.aet_FontChanged:
                    break;

                case BoAppEventTypes.aet_LanguageChanged:
                    //oApp.SetStatusBarMessage("Linguagem do sistema modificada.", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    Log.Logger.getInstance().log("Linguagem do sistema modificada.", Log.Logger.LogType.INFO, null, false);
                    break;

                case BoAppEventTypes.aet_ServerTerminition:
                    Log.Logger.getInstance().log("Servidor encerrado, terminado execucao do addon.", Log.Logger.LogType.INFO, null, false);
                    ExitAddon();
                    break;

                case BoAppEventTypes.aet_ShutDown:
                    Log.Logger.getInstance().log("Sistema finalizado, terminando execucao do addon.", Log.Logger.LogType.INFO, null, false);
                    ExitAddon();
                    break;
            }
        }

        public void ItemEvent(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            try
            {
                BubbleEvent = true;
                
                foreach (Controller.FormMode fm in oController.getModeMonitors())
                {
                    try
                    {
                        Form oForm = (Form)oApp.Forms.GetForm(fm.formID, 0);
                        if (oForm != null)
                        {
                            oController.setMode(fm.formID, Convert.ToInt32(oForm.Mode));
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        ItsControlException.ItsControlException.Save(e); 
                    }
                }
                BubbleEvent = oController.EventReceiver(pVal.FormTypeEx, pVal.ItemUID, pVal.BeforeAction, pVal);
            }catch(Exception e)
            {
                BubbleEvent = false;
                ItsControlException.ItsControlException.Save(e); 
            }
        }

        public void FormDataEvent(ref SAPbouiCOM.BusinessObjectInfo BusinessObjectInfo, out bool BubbleEvent)
        {
            try{

                BubbleEvent = true;

                foreach (Controller.FormMode fm in oController.getModeMonitors())
                {
                    try
                    {
                        Form oForm = (Form)oApp.Forms.GetForm(fm.formID, 0);
                        if (oForm != null)
                        {
                            oController.setMode(fm.formID, Convert.ToInt32(oForm.Mode));
                            break;
                        }
                    }
                    catch (Exception e) 
                    {
                        ItsControlException.ItsControlException.Save(e); 
                    }
                }

                BubbleEvent = oController.EventReceiver(BusinessObjectInfo.FormTypeEx, ""
                    , BusinessObjectInfo.BeforeAction, null, BusinessObjectInfo);

            }catch(Exception e)
            {
                BubbleEvent = false;
                ItsControlException.ItsControlException.Save(e); 
            }
        }

        public void MenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try{
            //System.Windows.Forms.MessageBox.Show(pVal.MenuUID);
                foreach (Controller.FormMode fm in oController.getModeMonitors())
                {
                    try
                    {
                        Form oForm = (Form)oApp.Forms.GetForm(fm.formID, 0);
                        if (oForm != null)
                        {
                            oController.setMode(fm.formID, Convert.ToInt32(oForm.Mode));
                            break;
                        }
                    }
                    catch (Exception e) 
                    {
                        ItsControlException.ItsControlException.Save(e); 
                    }
                }

                BubbleEvent = oController.EventReceiver("Menu", pVal.MenuUID, pVal.BeforeAction, null);

            }catch(Exception e)
            {
                BubbleEvent = false;
                ItsControlException.ItsControlException.Save(e); 
            }
        }


        public void AddFilter(string FormUID, SAPbouiCOM.BoEventTypes eventType)
        {
            oController.AddFilter(FormUID, eventType);
        }

    }
}
