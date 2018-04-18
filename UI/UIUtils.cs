using System;
using SAPbobsCOM;
using SAPbouiCOM;
using System.Collections.Generic;
using SB1Util.Connection;
using SB1Util.Log;
using System.Xml;
using SB1Util.UI;
using System.Threading;



namespace SB1Util.UI
{


    public class UIUtils
    {

        public enum ItemType
        {
            Matrix, UserDataSource, DataTable, DBDataSource, Item, ColumnMatrix
        }

        public struct MatrixColumns
        {
            public string ColUID;
            public BoFormItemTypes ColumnType;
            public string Caption;
            public int Width;
            public bool Editable;
            public bool SetBound;
            public string DSTable;
            public string DSField;
        }

        private ProgressBar oProgBar;
        private Logger logger;
        private EventFilters oFilters;
        private SAPbobsCOM.Company oCompany;
        private Application oApp;
        


        public UIUtils(B1Connection connection)
        {
            try
            {
                this.oApp = connection.App;
                this.oCompany = connection.Company;
                this.oFilters = new EventFilters();
                this.logger = Logger.getInstance();
            }
            catch (Exception e)
            {
                logger.log(
                    "Erro ao iniciar utilitario de interface: " +
                    e.Message + " - " + e.StackTrace, Logger.LogType.FATAL, e);
            }
        }


        public UIUtils(SAPbobsCOM.Company company, Application app)
        {
            try
            {
                this.oApp = app;
                this.oCompany = company;
                this.oFilters = new EventFilters();
                this.logger = Logger.getInstance();
            }
            catch (Exception e)
            {
                logger.log(
                    "Erro ao iniciar utilitario de interface: " +
                    e.Message + " - " + e.StackTrace, Logger.LogType.FATAL, e);
            }
        }


        public void AddFilter(string containerUID, BoEventTypes eventType)
        {
            try
            {
                //caso nao exista filtro para container e evento, cria
                int pos;
                if (!FilterExists(containerUID, eventType, out pos))
                {
                    EventFilter oFilter = oFilters.Add(eventType);
                    oFilter.AddEx(containerUID);
                    oApp.SetFilter(oFilters);
                    //oApp.MessageBox(containerUID+ " ev: "+eventType.ToString() );
                }
                //ja existe filtro de evento, adicionar apenas o container
                else
                {

                    oFilters.Item(pos).AddEx(containerUID);
                    oApp.SetFilter(oFilters);
                }
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }


        public bool FilterExists(string containerUID, BoEventTypes eventType, out int pos)
        {
            bool ret = false;
            pos = -1;
            try
            {
                for (int i = 0; i < oFilters.Count; i++)
                {
                    EventFilter f = oFilters.Item(i);

                    if (eventType.Equals(f.EventType))
                    {
                        ret = true;
                        pos = i;
                        break;
                    }

                }
            }
            catch (Exception e)
            {
                ret = false;
            }
            return ret;
        }


        public void testPermissao()
        {
            try
            {
                Form form = oApp.Forms.GetForm("139", 0);
                Item item = form.Items.Item("54");
                //form.m
                item.Enabled = false;

                for (int i = 0; i < oApp.Forms.Count; i++)
                {
                    Form frm = oApp.Forms.Item(i);
                    
                    StatusBarMessage("Title:" + frm.Title + " Type:" + frm.Type + " TypeCount:" + frm.TypeCount + " UDFFormUID:" + frm.UDFFormUID, BoMessageTime.bmt_Medium, false);
                }

            }
            catch (Exception e)
            {
            }

        }






        /**
         * Adiciona um item de menu em um menu ja existente ou adicionado.
         * 
         * 
         * menuItemB1ID     Id do menu pai onde o item de menu devera ser inserido
         * menuItemDescr    Texto do item de menu
         * menuItemID       ID do item de menu que esta sendo criado
         * position         posicao na lista de itens dentro de um menu
         * type             tipo...
         * imagePath        caminho para o arquivo de imagem a ser utilizado como icone no menu
         * 
         */
        public void AddMenuItem(string menuItemB1ID, string menuItemDescr, string menuItemID
            , int position, BoMenuType type, string imagePath="", bool remove = true)
        {
            SAPbouiCOM.Menus oMenus = null;
            SAPbouiCOM.MenuItem oMenuItem = null;
            SAPbouiCOM.MenuCreationParams oCreationPackage = null;
            oCreationPackage = ((SAPbouiCOM.MenuCreationParams)(oApp.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)));

            oMenuItem = oApp.Menus.Item(menuItemB1ID);
            oMenus = oMenuItem.SubMenus;
            
            bool exist = (oMenus == null) ? false : oMenuItem.SubMenus.Exists(menuItemID);

            if (exist && remove)
            {
                oMenuItem.SubMenus.RemoveEx(menuItemID);
                exist = false;
            }
            else
            {
                exist = false;
            }


            if (!(exist && remove))
            {
                oCreationPackage.Type = type;
                oCreationPackage.UniqueID = menuItemID;
                oCreationPackage.String = menuItemDescr;
                oCreationPackage.Enabled = true;
                oCreationPackage.Position = position; //posição onde vai criar o modulo
                oCreationPackage.Image = imagePath;

                try
                {
                    if (oMenus == null)
                    {
                        oMenuItem.SubMenus.Add(menuItemID, menuItemDescr, type, position);
                        oMenus = oMenuItem.SubMenus;
                    }

                    oMenus.AddEx(oCreationPackage);
                }
                catch (Exception e)
                {
                    logger.log(e.Message, Logger.LogType.ERROR, e, false);
                }
            }

        }


        public void AddTab(Form oForm, string tabName, string tabNum, string tabCaption)
        {
            try
            {

                if (ItemExist(oForm, tabNum) && !ItemExist(oForm, tabName))
                {
                    Item oItPosition = oForm.Items.Item(tabNum);

                    Item oItTbLicencas = oForm.Items.Add(tabName, BoFormItemTypes.it_FOLDER);
                    oItTbLicencas.Top = oItPosition.Top;
                    oItTbLicencas.Height = oItPosition.Height;
                    oItTbLicencas.Width = oItPosition.Width;
                    oItTbLicencas.Left = (oItPosition.Left + oItPosition.Width) + 20;
                    oItTbLicencas.AffectsFormMode = true;
                    oItTbLicencas.Enabled = false;

                    Folder oTbLicencas = (Folder)oItTbLicencas.Specific;
                    oTbLicencas.Caption = tabCaption;
                    oTbLicencas.ValOff = "0";
                    oTbLicencas.ValOn = "1";
                    oTbLicencas.GroupWith(tabNum);
                }
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }


        public void AddMatrix(Form oForm, string itemID, string mtxName
            , int mtxWidth, int mtxHeight, int mtxTop, int mtxLeft, int mtxFromPane, int mtxToPane)
        {
            try
            {
                SAPbouiCOM.Item oItem;
                
                if (ItemExist(oForm, itemID) && !ItemExist(oForm, mtxName, ItemType.Matrix))
                {
                    Item oItPosition = oForm.Items.Item(itemID);

                    oItem = oForm.Items.Add(mtxName, SAPbouiCOM.BoFormItemTypes.it_MATRIX);
                    oItem.Width = mtxWidth;
                    oItem.Height = mtxHeight;
                    oItem.Top = oItPosition.Top + mtxTop;
                    oItem.Left = mtxLeft;
                    oItem.FromPane = mtxFromPane;
                    oItem.ToPane = mtxToPane;
                }
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }


        public void AddMatrix(Form oForm, string mtxName, 
            int height, int top, int left, int width, List<MatrixColumns> colunas)
        {

            try
            {
                Form form = oApp.Forms.ActiveForm;
                Matrix oMatrix = null;
                if (!ItemExist(oForm, mtxName, ItemType.Matrix))
                {
                    var oItem = form.Items.Add(mtxName, BoFormItemTypes.it_MATRIX);

                    oItem.Top = top;
                    oItem.Left = left;
                    oItem.Width = width;
                    oItem.Height = height;

                    oMatrix = (Matrix)oItem.Specific;
                }

                oMatrix = form.Items.Item(mtxName).Specific;

                oMatrix.SelectionMode = BoMatrixSelect.ms_Auto;

                var oColumns = oMatrix.Columns;

                foreach (var coluna in colunas)
                {
                    Column oColumn = null;
                    if (!ItemExist(oForm, mtxName, ItemType.ColumnMatrix, coluna.ColUID))
                    {
                        oColumn = oColumns.Add(coluna.ColUID, coluna.ColumnType);
                    }
                    else
                    {
                        oColumn = ((Matrix)form.Items.Item(mtxName).Specific).Columns.Item(coluna.ColUID);
                    }
                    oColumn.TitleObject.Caption = coluna.Caption;
                    oColumn.Width = coluna.Width;
                    oColumn.Editable = coluna.Editable;

                    if (coluna.SetBound)
                    {
                        form.DataSources.UserDataSources.Add(coluna.DSField, BoDataType.dt_LONG_TEXT);
                        oColumn.DataBind.SetBound(coluna.SetBound, coluna.DSTable, coluna.DSField);
                    }
                }
            }
            catch (Exception e)
            {
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            
        }



        public void AddColumnMtx(Form oForm, string mtxName, string columnName
            , string columnCaption, int columnWidth, bool editable, SAPbouiCOM.BoFormItemTypes columnType)
        {
            try
            {
                SAPbouiCOM.Columns oColumns;
                SAPbouiCOM.Column oColumn;

                if (ItemExist(oForm, mtxName, ItemType.Matrix) && !ItemExist(oForm, mtxName, ItemType.ColumnMatrix, columnName))
                {
                    oColumns = ((Matrix)oForm.Items.Item(mtxName).Specific).Columns;

                    oColumn = oColumns.Add(columnName, columnType);
                    oColumn.TitleObject.Caption = columnCaption;
                    oColumn.Width = columnWidth;
                    oColumn.Editable = editable;

                    columnName = columnName.Length > 7 ? columnName.Substring(0, 7) : columnName;
                    string dts = ("dts" + columnName);
                    oForm.DataSources.UserDataSources.Add(dts, BoDataType.dt_SHORT_TEXT);
                    oColumn.DataBind.SetBound(true, "", dts);

                }
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }


        public void AddItemsStaticText(Form oForm, string itemID, string itemName
            , string itemCaption, int itemWidth, int itemTop, int itemLeft, int itemFromPane, int itemToPane)
        {
            try
            {
                
                if (ItemExist(oForm, itemID) && !ItemExist(oForm, itemName))
                {
                    Item oItPosition = oForm.Items.Item(itemID);

                    Item oItem = oForm.Items.Add(itemName, SAPbouiCOM.BoFormItemTypes.it_STATIC);
                    oItem.Width = itemWidth;
                    oItem.Top = oItPosition.Top + itemTop;
                    oItem.Left = itemLeft;
                    oItem.FromPane = itemFromPane;
                    oItem.ToPane = itemToPane;

                    ((StaticText)oItem.Specific).Caption = itemCaption;
                }
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }


        public void AddItemsButton(Form oForm, string itemID, string itemName, string itemCaption,
            int itemWidth, int itemHeight, int itemTop, int itemLeft, int itemFromPane, int itemToPane, bool enabled = true)
        {
            try
            {
                
                if (ItemExist(oForm, itemID) && !ItemExist(oForm, itemName))
                {
                    Item oItPosition = oForm.Items.Item(itemID);

                    Item oItem = oForm.Items.Add(itemName, BoFormItemTypes.it_BUTTON);
                    oItem.Width = itemWidth;
                    oItem.Height = itemHeight;
                    oItem.Top = oItPosition.Top + itemTop;
                    oItem.Left = itemLeft;
                    oItem.FromPane = itemFromPane;
                    oItem.ToPane = itemToPane;
                    oItem.Enabled = enabled;

                    ((Button)oItem.Specific).Caption = itemCaption;
                }

            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }


        public void AddItemsRadioButton(Form oForm, string itemID, string itemName, string itemCaption,
            int itemWidth, int itemHeight, int itemTop, int itemLeft, int itemFromPane, int itemToPane,
            string group = null, bool enabled = true, string colName = null, string tableName = null)
        {
            try
            {
                
                if (ItemExist(oForm, itemID) && !ItemExist(oForm, itemName))
                {
                    Item oItPosition = oForm.Items.Item(itemID);

                    Item oItem = oForm.Items.Add(itemName, BoFormItemTypes.it_OPTION_BUTTON);
                    oItem.Width = itemWidth;
                    oItem.Height = itemHeight;
                    oItem.Top = oItPosition.Top + itemTop;
                    oItem.Left = itemLeft;
                    oItem.FromPane = itemFromPane;
                    oItem.ToPane = itemToPane;
                    oItem.Enabled = enabled;


                    OptionBtn radio = ((OptionBtn)(oItem.Specific));
                    radio.Caption = itemCaption;
                    //radio.ValOff = "0";
                    //radio.ValOn = "1";

                    if (group != null)
                    {
                        radio.GroupWith(group);
                    }

                    if (colName == null && tableName == null)
                    {
                        itemName = itemName.Length > 7 ? itemName.Substring(0, 7) : itemName;
                        string DSC = ("DSC" + itemName);
                        oForm.DataSources.UserDataSources.Add(DSC, SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                        radio.DataBind.SetBound(true, "", DSC);
                    }
                    else
                    {
                        itemName = itemName.Length > 7 ? itemName.Substring(0, 7) : itemName;
                        string DSC = ("DSC" + itemName);
                        oForm.DataSources.UserDataSources.Add(DSC, SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                        radio.DataBind.SetBound(true, tableName, colName);
                    }

                }

            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }


        public void AddItemsEditText(Form oForm, string itemID, string itemName
            , int itemWidth, int itemTop, int itemLeft, int itemFromPane, int itemToPane, 
            bool enabled = true, string tableName = "", string tableField = "")
        {
            try
            {
                
                if (ItemExist(oForm, itemID) && !ItemExist(oForm, itemName))
                {
                    Item oItPosition = oForm.Items.Item(itemID);

                    Item oItem = oForm.Items.Add(itemName, SAPbouiCOM.BoFormItemTypes.it_EDIT);
                    oItem.Width = itemWidth;
                    oItem.Top = oItPosition.Top + itemTop;
                    oItem.Left = itemLeft;
                    oItem.FromPane = itemFromPane;
                    oItem.ToPane = itemToPane;
                    oItem.Enabled = enabled;

                    if (tableName.Equals("") || tableField.Equals(""))
                    {
                        itemName = itemName.Length > 7 ? itemName.Substring(0, 7) : itemName;
                        string dts = ("dts" + itemName);
                        oForm.DataSources.UserDataSources.Add(dts, BoDataType.dt_LONG_TEXT);
                        ((EditText)oItem.Specific).DataBind.SetBound(true, "", dts);
                    }
                    else
                    {
                        ((EditText)oItem.Specific).DataBind.SetBound(true, tableName, tableField);
                    }

                }
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }


        /// <summary>
        /// Adiciona itens em um combobox
        /// </summary>
        /// <param name="oForm">O id do form que contém o combobox</param>
        /// <param name="itemID">O id do combobox</param>
        /// <param name="itemName"></param>
        /// <param name="itemWidth"></param>
        /// <param name="itemTop"></param>
        /// <param name="itemLeft"></param>
        /// <param name="itemFromPane"></param>
        /// <param name="itemToPane"></param>
        /// <param name="query"></param>
        /// <param name="valCol"></param>
        /// <param name="description"></param>
        /// <param name="colName"></param>
        /// <param name="tableName"></param>
        public void AddItemsComboBox(Form oForm, string itemID, string itemName
            , int itemWidth, int itemTop, int itemLeft, int itemFromPane, int itemToPane, string query
            , int valCol = 0, string description = "", string colName = null, string tableName = null)
        {
            try
            {
                if (ItemExist(oForm, itemID) && !ItemExist(oForm, itemName))
                {
                    Item oItPosition = oForm.Items.Item(itemID);
                    Item oItem = oForm.Items.Add(itemName, SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);

                    ComboBox oComboBox = ((ComboBox)(oItem.Specific));
                    SAPbobsCOM.Recordset oRecordSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);

                    oItem.Top = oItPosition.Top + itemTop;
                    oItem.Left = itemLeft;
                    oItem.Width = itemWidth;
                    oItem.FromPane = itemFromPane;
                    oItem.ToPane = itemToPane;
                    oItem.DisplayDesc = true;

                    itemName = itemName.Length > 7 ? itemName.Substring(0, 7) : itemName;
                    string DSC = ("DSC" + itemName);
                    CreateDataSource(oForm, itemID, DSC, ItemType.UserDataSource);

                    if (ItemExist(oForm, itemID, ItemType.UserDataSource))
                    {
                        oForm.DataSources.UserDataSources.Add(DSC, BoDataType.dt_SHORT_TEXT, 20);
                    }

                    if (colName == null && tableName == null)
                    {
                        oComboBox.DataBind.SetBound(true, "", DSC);
                    }
                    else
                    {
                        oComboBox.DataBind.SetBound(true, tableName, colName);
                    }

                    oRecordSet.DoQuery(query);

                    while (!oRecordSet.EoF)
                    {
                        //se o indice de valor nao estiver dentro do intervalo 
                        //dos campos da consulta, seleciona o 0
                        string value = "";
                        if (valCol < 0 || valCol > oRecordSet.Fields.Count)
                        {
                            value = "0";
                        }
                        else
                        {
                            value = Convert.ToString(oRecordSet.Fields.Item(valCol).Value);
                        }


                        //caso description nao exista seleciona vazio como descricao
                        string descr = "";
                        try
                        {
                            descr = (string)(description.Equals("") ? description : oRecordSet.Fields.Item(description).Value);
                        }
                        catch (Exception e)
                        {
                            descr = "";
                        }

                        oComboBox.ValidValues.Add(value, descr);
                        oRecordSet.MoveNext();
                    }

                }
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }
        

        public void AddItemsChkBox(Form oForm, string itemID, string itemName, string caption, string dtsName, ItemType dtsType,
            int itemWidth, int itemTop, int itemLeft, int itemFromPane, int itemToPane)
        {
            try
            {
                
                if (ItemExist(oForm, itemID) && !ItemExist(oForm, itemName))
                {
                    Item oItPosition = oForm.Items.Item(itemID);
                    itemName = itemName.Length > 7 ? itemName.Substring(0, 7) : itemName;

                    Item oItem = oForm.Items.Add(itemName, SAPbouiCOM.BoFormItemTypes.it_CHECK_BOX);
                    oItem.Top = oItPosition.Top + itemTop;
                    oItem.Left = itemLeft;
                    oItem.Width = itemWidth;
                    oItem.FromPane = itemFromPane;
                    oItem.ToPane = itemToPane;
                    oItem.DisplayDesc = false;

                    CheckBox oCheckBox = ((SAPbouiCOM.CheckBox)(oItem.Specific));

                    oCheckBox.Caption = caption;
                    oCheckBox.Checked = false;

                    string DSC = ("DSC" + itemName);
                    oForm.DataSources.UserDataSources.Add(DSC, SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                    oCheckBox.DataBind.SetBound(true, "", DSC);

                }

            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
                SB1ControlException.SB1ControlException.Save(e);
            }
        }


        public void AddItemsLinkedButton(Form oForm, string itemID, string itemName, string caption, string dtsName, ItemType dtsType,
            int itemWidth, int itemTop, int itemLeft, int itemFromPane, int itemToPane)
        {
            try
            {

            }
            catch (Exception e) 
            {
                SB1ControlException.SB1ControlException.Save(e);
            }
        }       

        /// <summary>
        ///Carrega um form a partir de um XMLDocument
        /// </summary>
        /// <param name="oSBuilder">a StringBuilder com o arquivo XML</param>
        /// <param name="uniqueID">O id do form</param>
        /// <returns>Flag com o status de processo. True se tudo ocorrer bem</returns>
        public bool LoadFormXML(System.Xml.XmlDocument oXmlDoc, string uniqueID, bool visible = true)
        {
            return LoadFormXML(oXmlDoc.InnerXml.ToString(), uniqueID, visible);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sFileName">Nome do formulário .srf</param>
        /// <returns>Status se o form foi ou nao carregado</returns>
        public bool LoadFormXMLFile(string sFileName, string uniqueID, bool visible = true)
        {
            var oXmlDoc = new XmlDocument();
            var sPath = string.Format("{0}\\Files\\{1}", AppDomain.CurrentDomain.BaseDirectory, sFileName);

            oXmlDoc.Load(sPath);

            return LoadFormXML(oXmlDoc, uniqueID, visible);
        }

        /// <summary>
        ///Carrega um form a partir de um XmlReader
        /// </summary>
        /// <param name="oSBuilder">a StringBuilder com o arquivo XML</param>
        /// <param name="uniqueID">O id do form</param>
        /// <returns>Flag com o status de processo. True se tudo ocorrer bem</returns>

        public bool LoadFormXML(System.Xml.XmlReader oXMLReader, string uniqueID, bool visible = true)
        {
            System.Xml.XmlDocument oXmlDoc = null;
            oXmlDoc = new System.Xml.XmlDocument();

            // Informa o caminho e carrega o arquivo XML
            oXmlDoc.Load(oXMLReader);

            return LoadFormXML(oXmlDoc, uniqueID, visible);
        }        
        /// <summary>
        ///Carrega um form partir do conteudo de um StringBuilder 
        /// </summary>
        /// <param name="oSBuilder">a StringBuilder com o arquivo XML</param>
        /// <param name="uniqueID">O id do form</param>
        /// <returns>Flag com o status de processo. True se tudo ocorrer bem</returns>
        public bool LoadFormXML(System.Text.StringBuilder oSBuilder, string uniqueID, bool visible = true)
        {
            return LoadFormXML(oSBuilder.ToString(), uniqueID, visible);
        }        

        /// <summary>
        /// Carrega um form a partir do conteudo XML em formato texto
        /// </summary>
        /// <param name="sXmlDoc">O Xml que vai ser processado</param>
        /// <param name="uniqueID"></param>
        /// <returns></returns>
        private bool LoadFormXML(string sXmlDoc, string uniqueID, bool visible = true)
        {
            bool ret = false;

            try
            {
                SAPbouiCOM.FormCreationParams oFormCreationParams;

                oFormCreationParams = ((SAPbouiCOM.FormCreationParams)(oApp.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams)));

                // Carrega o form para a aplicação SBO
                string sXML = sXmlDoc;
                                
                oFormCreationParams.XmlData = sXML;
                Form oForm = oApp.Forms.AddEx(oFormCreationParams);

                //CreateDataSource(oForm);
                MapEvents(oForm, uniqueID);

                oForm.Visible = visible;
                ret = true;

            }
            catch (Exception e)
            {
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e, false);
                SB1ControlException.SB1ControlException.Save(e);
            }


            return ret;
        }

        
        /// <summary>
        /// Carrega uma tab a partir do conteudo XML em formato texto
        /// </summary>
        /// <param name="sXmlDoc">o XmlDocument</param>
        /// <returns></returns>
        public bool LoadTabXML(string sXmlDoc)
        {
            bool ret = false;

            try
            {
                SAPbouiCOM.FormCreationParams oFormCreationParams;

                oFormCreationParams = ((SAPbouiCOM.FormCreationParams)(oApp.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams)));

                // Carrega o form para a aplicação SBO
                string sXML = sXmlDoc;

                oApp.LoadBatchActions(sXML);

                //oFormCreationParams.UniqueID = uniqueID;
                //oFormCreationParams.FormType = "";

                //oFormCreationParams.XmlData = sXML;
                //Form oForm = oApp.Forms.AddEx(oFormCreationParams);

                //CreateDataSource(oForm);
                //MapEvents(oForm, uniqueID);

                //oForm.Visible = true;
                ret = true;

            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }


            return ret;
        }



        public void LoadUDO(string name)
        {
            var oMenu =  oApp.Menus.Item("47616");

            if (oMenu.SubMenus.Count <= 0) return;

            for (var i = 0; i < oMenu.SubMenus.Count; i++)
            {
                if (oMenu.SubMenus.Item(i).String.Contains(name))
                {
                    oMenu.SubMenus.Item(i).Activate();
                }
            }
        }



        public void loadMatrixData(string query, string dtsName, string[] mapp, Form oForm, string matrixID)
        {
            logger.pushOperation("loadMatrixData");
            try
            {
                Matrix matrix = oForm.Items.Item(matrixID).Specific;
                if (!ItemExist(oForm, dtsName, SB1Util.UI.UIUtils.ItemType.DataTable))
                {
                    oForm.DataSources.DataTables.Add(dtsName);
                }

                oForm.DataSources.DataTables.Item(dtsName).ExecuteQuery(query);

                if (oForm.DataSources.DataTables.Item(dtsName).Rows.Count > 0)
                {
                    foreach (string s in mapp)
                    {
                        string[] sm = s.Split('=');
                        matrix.Columns.Item(sm[0]).DataBind.Bind(dtsName, sm[1]);
                    }
                }

                matrix.Clear();
                matrix.LoadFromDataSource();
                matrix.AutoResizeColumns();
            }
            catch (Exception e)
            {
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e);
            }
            finally
            {
                logger.releaseOperation();
            }

        }




        
        /// <summary>
        /// Salva o form em formato XML
        /// </summary>
        /// <param name="formUID">O id do form</param>
        /// <param name="pathName">O nome que o form será salvo</param>
        public void SaveFormXML(string formUID, string pathName)
        {
            try
            {
                Form oForm = oApp.Forms.GetForm(formUID, 0);

                System.Xml.XmlDocument oXmlDoc = new System.Xml.XmlDocument();
                string sXmlString = null;

                // get the form as an XML string
                sXmlString = oForm.GetAsXML();

                // load the form's XML string to the
                // XML document object
                oXmlDoc.LoadXml(sXmlString);

                // save the XML Document
                oXmlDoc.Save((pathName + formUID + ".xml"));
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
                SB1ControlException.SB1ControlException.Save(e);
            }

        }

        /// <summary>
        /// Serve para filtrar os eventos do form
        /// </summary>
        /// <param name="form">O objeto form</param>
        /// <param name="uniqueID">Filtra os componentes por ID</param>
        private void MapEvents(Form form, string uniqueID)
        {
            Dictionary<BoFormItemTypes, BoEventTypes> dic =
                new Dictionary<BoFormItemTypes, BoEventTypes>();

            //lista os itens do form
            foreach (IItem it in form.Items)
            {
                //adiciona os tipos de eventos a filtrar
                #region switch
                try
                {
                    switch (it.Type)
                    {
                        case BoFormItemTypes.it_ACTIVE_X:
                            //dic.Add(BoFormItemTypes.it_ACTIVE_X,BoEventTypes.et_CLICK);
                            break;
                        case BoFormItemTypes.it_BUTTON:
                            //dic.Add(BoFormItemTypes.it_BUTTON, BoEventTypes.et_ITEM_PRESSED);
                            dic.Add(BoFormItemTypes.it_BUTTON, BoEventTypes.et_CLICK);
                            break;
                        case BoFormItemTypes.it_BUTTON_COMBO:
                            dic.Add(BoFormItemTypes.it_BUTTON_COMBO, BoEventTypes.et_COMBO_SELECT);
                            break;
                        case BoFormItemTypes.it_CHECK_BOX:
                            dic.Add(BoFormItemTypes.it_CHECK_BOX, BoEventTypes.et_CLICK);
                            break;
                        case BoFormItemTypes.it_COMBO_BOX:
                            dic.Add(BoFormItemTypes.it_COMBO_BOX, BoEventTypes.et_COMBO_SELECT);
                            break;
                        case BoFormItemTypes.it_EDIT:
                            //dic.Add(BoFormItemTypes.it_EDIT, BoEventTypes);
                            break;
                        case BoFormItemTypes.it_EXTEDIT:
                            //dic.Add(BoFormItemTypes.it_EXTEDIT, BoEventTypes);
                            break;
                        case BoFormItemTypes.it_FOLDER:
                            //dic.Add(BoFormItemTypes.it_FOLDER, BoEventTypes);
                            break;
                        case BoFormItemTypes.it_GRID:
                            //dic.Add(BoFormItemTypes.it_GRID, BoEventTypes);
                            break;
                        case BoFormItemTypes.it_LINKED_BUTTON:
                            //dic.Add(BoFormItemTypes.it_LINKED_BUTTON, BoEventTypes);
                            break;
                        case BoFormItemTypes.it_MATRIX:
                            dic.Add(BoFormItemTypes.it_MATRIX, BoEventTypes.et_MATRIX_LINK_PRESSED);
                            break;
                        case BoFormItemTypes.it_OPTION_BUTTON:
                            dic.Add(BoFormItemTypes.it_OPTION_BUTTON, BoEventTypes.et_CLICK);
                            break;
                        case BoFormItemTypes.it_PANE_COMBO_BOX:
                            //dic.Add(BoFormItemTypes.it_PANE_COMBO_BOX, BoEventTypes);
                            break;
                        case BoFormItemTypes.it_PICTURE:
                            //dic.Add(BoFormItemTypes.it_PICTURE, BoEventTypes);
                            break;
                        case BoFormItemTypes.it_RECTANGLE:
                            //dic.Add(BoFormItemTypes.it_RECTANGLE, BoEventTypes);
                            break;
                        case BoFormItemTypes.it_STATIC:
                            //dic.Add(BoFormItemTypes.it_STATIC, BoEventTypes);
                            break;
                    }
                }
                catch (Exception e) { }
                #endregion

            }

            //adiciona os filtros
            foreach (BoEventTypes type in dic.Values)
            {
                AddFilter(uniqueID, type);
            }


        }
        
        /// <summary>
        /// Verifica a existencia de um componente em um form. 
        /// </summary>
        /// <param name="oForm">o Id do form que será pesquisado o componente</param>
        /// <param name="itemID">o id do Item</param>
        /// <param name="type">o Tipo do item</param>
        /// <param name="columnID">o Numero da coluna do item</param>
        /// <returns>Flag sinalizando true se o processo terminou corretamente ou não</returns>
        public bool ItemExist(Form oForm, string itemID, ItemType type = ItemType.Item, string columnID = null)
        {
            bool ret = false;
            
            try
            {
                
                //tenta acessar os itens, em caso de erro :O o item nao existe...
                switch (type)
                {
                    case ItemType.ColumnMatrix:
                        ((IMatrix)oForm.Items.Item(itemID).Specific).Columns.Item(columnID);
                        break;

                    case ItemType.DataTable:
                        oForm.DataSources.DataTables.Item(itemID);
                        break;

                    case ItemType.DBDataSource:
                        oForm.DataSources.DBDataSources.Item(itemID);
                        break;

                    case ItemType.Item:
                        oForm.Items.Item(itemID);
                        break;

                    case ItemType.Matrix:
                        oForm.Items.Item(itemID);
                        break;

                    case ItemType.UserDataSource:
                        oForm.DataSources.UserDataSources.Item(itemID);
                        break;

                }
                ret = true;
            }
            catch (Exception e)
            {
                ret = false;
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
                SB1ControlException.SB1ControlException.Save(e);
            }
            return ret;
        }


        public bool FormOpen(Form oForm)
        {
            bool ret = false;

            return ret;
        }


        public void CreateDataSource(Form oForm, string itemID, string dtsName, 
            ItemType dtsType, BoDataType dataType = BoDataType.dt_LONG_TEXT)
        {
            try
            {
                Item item = oForm.Items.Item(itemID);

                switch (dtsType)
                {
                    case ItemType.DataTable:

                        break;

                    case ItemType.DBDataSource:
                        string[] aux = dtsName.Split('.');
                        //((DBDataSource)item.Specific). .DataBind.SetBound(true, aux[0], aux[1]);
                        break;

                    case ItemType.UserDataSource:
                        oForm.DataSources.UserDataSources.Add(dtsName, dataType);
                        oForm.Items.Item(itemID).Specific.DataBind.SetBound(true, "", dtsName);

                        break;

                }
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }

        }

        private void AddUserDataSource(string dtsName, Form oForm, string itemID)
        {

        }

        private void AddDBDataSource(string dtsName, Form oForm, string itemID)
        {

        }

        private void AddTableDataSource(string dtsName, Form oForm, string itemID)
        {
        }


        /// <summary>
        /// Retorna o valor presente em um componente do container passado por parametro
        /// </summary>
        /// <param name="containerUID">O id do container contendo componentes</param>
        /// <param name="componentUID">O id do componente</param>
        /// <param name="columnIndex">O numero da coluna do componente</param>
        /// <param name="rowIndex">O número da linha do compoente</param>
        /// <returns></returns>
        public Object GetValue(string containerUID, string componentUID, int columnIndex = 0, int rowIndex = 0)
        {
            Object ret = null;

            try
            {
                Item item = oApp.Forms.Item(containerUID).Items.Item(componentUID);

                //de acordo com o tipo
                switch (item.Type)
                {
                    case BoFormItemTypes.it_EDIT:
                        ret = ((EditText)item.Specific).Value;
                        break;

                    case BoFormItemTypes.it_COMBO_BOX:
                        ret = ((ComboBox)item.Specific).Selected.Value;
                        break;

                    case BoFormItemTypes.it_MATRIX:
                        ret = ((Matrix)item.Specific).GetCellSpecific(columnIndex, rowIndex);
                        break;

                    case BoFormItemTypes.it_STATIC:
                        ret = ((StaticText)item.Specific).Caption;
                        break;

                    case BoFormItemTypes.it_OPTION_BUTTON:
                        ret = ((OptionBtn)item.Specific).Selected;
                        break;

                    case BoFormItemTypes.it_CHECK_BOX:
                        ret = ((CheckBox)item.Specific).Checked;
                        break;

                }

            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
                SB1ControlException.SB1ControlException.Save(e);
            }

            return ret;
        }
        
       
        /// <summary>
        /// Retorna o valor presente em um componente do container passado por parametro
        /// </summary>
        /// <param name="containerUID">O id do Container que contém componentes</param>
        /// <param name="componentUID">O id do componente</param>
        /// <param name="value">O valor que será setado no componente</param>
        public void SetValue(Form containerUID, string componentUID, string value)
        {
            try
            {
                Item item = containerUID.Items.Item(componentUID);

                //de acordo com o tipo
                switch (item.Type)
                {
                    case BoFormItemTypes.it_EDIT:
                        ((EditText)item.Specific).Value = value;
                        break;

                    case BoFormItemTypes.it_COMBO_BOX:
                        ((ComboBox)item.Specific).Select(value);
                        break;

                    case BoFormItemTypes.it_STATIC:
                        ((StaticText)item.Specific).Caption = value;
                        break;
                }

            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }



        /// <summary>
        //Retorna Coluna e Linha selecionada na matrix, a forma de selecao 
        //de uma linha pode ser nas formas CheckBox ou Matrix. Sendo CheckBox
        //a linha selecionada sera identificada de acordo com a coluna de checkBox 
        //contida na Matrix, sendo Matrix, a linha selecionada sera identificada 
        // atravez
        /// </summary>
        /// <param name="containerUID"></param>
        /// <param name="componentUID"></param>
        /// <param name="columnID"></param>
        /// <param name="itemType"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        public void getSelected(string containerUID, string componentUID, string columnID, BoFormItemTypes itemType, out int column, out int row)
        {
            try
            {
                row = 0;
                column = 0;

                Form oForm = oApp.Forms.ActiveForm;
                Matrix results = (Matrix)oForm.Items.Item(componentUID).Specific;
                Columns cols = results.Columns;
                for (int i = 1; i < cols.Count; i++)
                {
                    if (cols.Item(i).UniqueID.Equals(columnID))
                    {
                        column = i;
                        break;
                    }
                }


                if (itemType.Equals(BoFormItemTypes.it_CHECK_BOX))
                {
                    //para cada linha da matriz...
                    for (int i = 1; i <= results.RowCount; i++)
                    {
                        //verifica se a licenca foi selecionada           
                        CheckBox chk = (CheckBox)cols.Item(columnID).Cells.Item(i).Specific;
                        if (chk.Checked)
                        {
                            row = i;
                            break;
                        }
                    }

                }
                else if (itemType.Equals(BoFormItemTypes.it_MATRIX))
                {
                    row = results.GetNextSelectedRow();
                }

            }
            catch (Exception e)
            {
                row = 0;
                column = 0;
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }

        }


        /*
         * Realiza o mapeamento dos componentes do form para que
         * seu valor possa ser acesado atraves do metodo GetData()
         */
        private void MapComponents()
        {

        }


        public Matrix GetMatrix(int i, int j, string containerUID, string componentUID)
        {

            return null;
        }


        /// <summary>        
        /// Recupara o ID de uma tela de acordo com o prefixo passado por parametro.
        /// As telas de usuario possuem um id no formato [prefix + TblNum], geralmente
        /// o prefix eh 11000 entao o id da tabela de usuario para um TblNum=120
        /// ficaria 11120.
        /// </summary>
        /// <param name="tableName">O nome da tabela que vai ser efetuado a busca de dados</param>
        /// <param name="prefix">Um prefixo para diferenciar a tabela</param>
        /// <returns>O nome da tabela </returns>
        public string GetFormID(string tableName, int prefix)
        {
            string id = "";
            Recordset rs = null;
            try
            {
                rs = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery("SELECT TblNum FROM OUTB WHERE TableName = '" + tableName + "'");

                id = Convert.ToString(prefix + Convert.ToInt32(rs.Fields.Item("TblNum").Value));                
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
            rs = null;
            System.GC.Collect();

            return id;
        }

        /// <summary>
        /// Remove Totas as consultas formatadas existentes
        /// </summary> 
        public void ClearFormattedSearches()
        {
            try
            {
                DB.DBFacade db = DB.DBFacade.getInstance();
                int max = Convert.ToInt32(db.Max("CSHS", "IndexID"));
                for (int i = 1; i <= max; i++)
                {
                    RemoveFormattedSearch(i);
                }
            }
            catch (Exception e) 
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }

        }


        /// <summary>
        ///Remove a consulta formatada de acordo com o indice passado por parametro.
        ///Este indice é referente ao campo IndexID da tabela CSHS 
        /// </summary>
        /// <param name="index">O id da conslta</param>
        public void RemoveFormattedSearch(int index)
        {
            try
            {
                FormattedSearches fs = (FormattedSearches)oCompany.GetBusinessObject(BoObjectTypes.oFormattedSearches);
                fs.GetByKey(index);
                int ret = fs.Remove();
                string msg = oCompany.GetLastErrorDescription();
                logger.log("Removendo Consulta Formatada(" + index + "): " + ret + " - " + msg, Logger.LogType.INFO);

            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
        }


        /// <summary>
        ///Adiciona uma consulta formatada a uma tela. 
        /// </summary>
        /// <param name="formID">O Código do form que vai executar a consulta</param>
        /// <param name="itemID">O item que vai disparar a consulta</param>
        /// <param name="colID">O N° da coluna, caso for uma matrix, que vai receber o dado processado de volta</param>
        /// <param name="queryName">O nome da query que vai ser chamada para processamento</param>
        /// <param name="queryCategory">A categoria da query</param>
        /// <param name="fieldID">ID do campo que ira receber a consulta</param>
        /// <param name="refresh">O campo possui atualizacao automatica</param>
        /// <param name="byField"></param>
        /// <returns></returns>
        public bool AddFormattedSearch(string formID, string itemID, string colID, string queryName, string queryCategory
            , string fieldID = null, BoYesNoEnum refresh = BoYesNoEnum.tNO, BoYesNoEnum byField = BoYesNoEnum.tNO)
        {
            bool ret = false;
            try
            {
                FormattedSearches fs = (FormattedSearches)oCompany.GetBusinessObject(BoObjectTypes.oFormattedSearches);
                Recordset rs = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery("SELECT IntrnalKey " +
                    " FROM OUQR INNER JOIN OQCN " +
                    " ON OQCN.CategoryID = OUQR.QCategory " +
                    " WHERE OQCN.CatName = '" + queryCategory + "'" +
                    " AND OUQR.QName = '" + queryName + "'");

                //adiciona a consulta formatada
                fs.FormID = formID;
                fs.ItemID = itemID;
                fs.Action = BoFormattedSearchActionEnum.bofsaQuery;
                fs.ColumnID = colID.Equals("") ? "-1" : colID;
                fs.QueryID = (int)rs.Fields.Item("IntrnalKey").Value;
                fs.FieldID = fieldID;
                fs.Refresh = refresh;
                fs.ForceRefresh = refresh;
                fs.ByField = byField;
                if (fs.Add() == 0)
                {
                    ret = true;
                }
            }
            catch (Exception e)
            {
                ret = false;
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
            return ret;
        }
        /// <summary>
        /// Adiciona uma consulta que podera ser utilizada como consulta formatada.
        /// </summary>
        /// <param name="query"> a consulta sql que vai ser disparada</param>
        /// <param name="queryName">O nome que a consulta vai receber dentro do SAP</param>
        /// <param name="queryCategory">A categoria da consulta</param>
        /// <returns>Flag informando se o processo ocorreu com sucesso</returns>
        public bool AddSearchQuery(string query, string queryName, string queryCategory)
        {
            bool ret = false;

            try
            {
                UserQueries uq = (UserQueries)oCompany.GetBusinessObject(BoObjectTypes.oUserQueries);
                Recordset rs = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery("SELECT CategoryId FROM OQCN WHERE CatName = '" + queryCategory + "'");

                //adiciona a consulta
                uq.Query = query;
                uq.QueryCategory = (int)rs.Fields.Item("CategoryId").Value;
                uq.QueryDescription = queryName;
                if (uq.Add() == 0)
                {
                    ret = true;
                }
            }
            catch (Exception e)
            {
                ret = false;
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
            return ret;
        }

        /// <summary>
        ///Cria uma categoria para cadastrar as consultas 
        /// </summary>
        /// <param name="categoryName">O nome da categoria a ser cadastrada</param>
        /// <returns>Flag que informa se o cadastro foi efetuado com sucesso</returns>
        public bool AddQueryCategory(string categoryName)
        {
            bool ret = false;
            try
            {
                QueryCategories qc = (QueryCategories)oCompany.GetBusinessObject(BoObjectTypes.oQueryCategories);
                qc.Name = categoryName;
                if (qc.Add() == 0)
                {
                    ret = true;
                }
            }
            catch (Exception e)
            {
                ret = false;
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
            }
            return ret;
        }
        /// <summary>
        /// Dispara uma mensagem para o usuário
        /// </summary>
        /// <param name="message">A mensagem que será mostrada para o usuário</param>
        /// <param name="defaultBtn">Os botões selecionado por default</param>
        /// <param name="btn1Caption">O caption do botão 1</param>
        /// <param name="btn2Caption">O caption do botão 2</param>
        /// <param name="btn3Caption">O caption do botão 3</param>
        /// <returns>Retorna o id identificando o numero da janela criada</returns>
        public int MessageBox(string message, int defaultBtn = 1, string btn1Caption = "Ok", string btn2Caption = "", string btn3Caption = "")
        {
            return oApp.MessageBox(message, defaultBtn, btn1Caption, btn2Caption, btn3Caption);
        }
        /// <summary>
        /// Mostra alguma mensagem de SAP na barra de tarefas
        /// </summary>
        /// <param name="text">a mensagem do SAP</param>
        /// <param name="time">O tempo em que a mensagem será mostrada na barra de tarefas do SAP</param>
        /// <param name="isError">Informa se a mensagem é um erro</param>
        public void StatusBarMessage(string text, BoMessageTime time = BoMessageTime.bmt_Medium, bool isError = true)
        {
            oApp.SetStatusBarMessage(text, time, isError);
        }
        /// <summary>
        /// Mostra alguma mensagem de SAP na barra de tarefas
        /// </summary>
        /// <param name="text">a mensagem do SAP</param>
        /// <param name="LogType">O tipo de Mensagem </param>
        /// <param name="time">O tempo em que a mensagem será mostrada na barra de tarefas do SAP</param>
        public void StatusBarMessage(string text, SB1Util.Log.Logger.LogType LogType, BoMessageTime time = BoMessageTime.bmt_Medium)
        {
            try
            {
                BoStatusBarMessageType B1ErrorType = BoStatusBarMessageType.smt_None;
                switch (LogType)
                {
                    case Logger.LogType.DEBUG:
                        B1ErrorType = BoStatusBarMessageType.smt_None;
                        break;
                    case Logger.LogType.INFO:
                        B1ErrorType = BoStatusBarMessageType.smt_Success;
                        break;
                    case Logger.LogType.WARNING:
                        B1ErrorType = BoStatusBarMessageType.smt_Warning;
                        break;
                    case Logger.LogType.ERROR:
                        B1ErrorType = BoStatusBarMessageType.smt_Error;
                        break;
                    case Logger.LogType.FATAL:
                        B1ErrorType = BoStatusBarMessageType.smt_Error;
                        break;
                }

                oApp.StatusBar.SetSystemMessage(text, time, B1ErrorType);
            }
            catch (Exception e)
            {
                logger.log(e.Message, Logger.LogType.ERROR, e, false);
                SB1ControlException.SB1ControlException.Save(e);
            }
        }


        public void startProgressBar(int count, bool stoppeable=true)
        {
            try
            {
                if (oProgBar != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oProgBar);
                    oProgBar = null;
                    System.GC.Collect();
                }

                oProgBar = oApp.StatusBar.CreateProgressBar("Progress Bar", count, stoppeable);
            }
            catch (Exception e)
            {
            }
        }
        public void countProgress(string text="")
        {
            try
            {
                oProgBar.Text = (oProgBar.Value * 100/ oProgBar.Maximum) + "% " + text;
                oProgBar.Value += 1;
            }
            catch (Exception e)
            {
            }
        }
        public void stopProgress()
        {
            try
            {
                oProgBar.Stop();
                if (oProgBar != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oProgBar);
                    oProgBar = null;
                    System.GC.Collect();
                }
            }
            catch (Exception e)
            {
            }
        }
        

        public void refreshComboBox(ComboBox oComboBox, Recordset oRecordSet, int valCol, string description)
        {
            try
            {
                logger.pushOperation("UIUtils.refreshComboBox");

                System.Windows.Forms.Application.UseWaitCursor = true;

                int count = oComboBox.ValidValues.Count;
                for (int i = 0; i < count; i++)
                {
                    oComboBox.ValidValues.Remove(0, BoSearchKey.psk_Index);
                }


                if (oRecordSet != null)
                {
                    oRecordSet.MoveFirst();
                    while (!oRecordSet.EoF)
                    {
                        //se o indice de valor nao estiver dentro do intervalo 
                        //dos campos da consulta, seleciona o 0
                        string value = "";
                        if (valCol < 0 || valCol > oRecordSet.Fields.Count)
                        {
                            value = "0";
                        }
                        else
                        {
                            value = Convert.ToString(oRecordSet.Fields.Item(valCol).Value);
                        }


                        //caso description nao exista seleciona vazio como descricao
                        string descr = "";
                        try
                        {
                            descr = (string)(description.Equals("") ? description : oRecordSet.Fields.Item(description).Value);
                        }
                        catch (Exception e)
                        {
                            descr = "";
                        }

                        oComboBox.ValidValues.Add(value, descr);
                        oRecordSet.MoveNext();
                    }
                }
            }
            catch (Exception e)
            {
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            finally
            {
                logger.releaseOperation();
                System.Windows.Forms.Application.UseWaitCursor = false;
            }

        }


        public Form getOpenedForm(string formID)
        {
            Form oForm = null;
            try
            {
                Forms forms = oApp.Forms;
                int ant = 0;
                for (int i = 0; i < forms.Count; i++)
                {
                    Form aux = forms.Item(i);
                    if (aux.TypeEx.Equals(formID) && ant < aux.TypeCount)
                    {
                        ant = aux.TypeCount;
                        oForm = aux;
                    }

                }
            }
            catch (Exception e)
            {
                oForm = null;
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e);
            }

            return oForm;
        }


        public string addQueryFilter(string filter, string val, string column = "")
        {
            if (!filter.Equals("") && !val.Equals(""))
            {
                filter += " AND ";
            }

            if (!val.Equals(""))
            {
                filter += (column.Equals("") ? "" : column + " = ") + val;
            }

            return filter;

        }




        public void checkAll(Form oForm, string matrixID, string colChkID)
        {
            logger.pushOperation("checkAll");
            try
            {
                //oForm.Freeze(true);
                Column col = ((Matrix)oForm.Items.Item(matrixID).Specific).Columns.Item(colChkID);

                System.Windows.Forms.Application.UseWaitCursor = true;

                //seleciona tudo
                if (col.TitleObject.Caption.Equals("[  ]"))
                {
                    for (int i = 1; i <= col.Cells.Count; i++)
                    {
                        CheckBox chk = col.Cells.Item(i).Specific;
                        chk.Checked = true;
                    }

                    //ajusta o caption da coluna
                    col.TitleObject.Caption = "[X]";
                }

                //desmarca tudo
                else
                {
                    for (int i = 1; i <= col.Cells.Count; i++)
                    {
                        CheckBox chk = col.Cells.Item(i).Specific;
                        chk.Checked = false;
                    }

                    //ajusta o caption da coluna
                    col.TitleObject.Caption = "[  ]";
                }
            }
            catch (Exception e)
            {
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e);
            }
            finally
            {
                logger.releaseOperation();
                System.Windows.Forms.Application.UseWaitCursor = false;
            }
        }

        public int countMatrixLinesChecked(Form oForm, string matrixID, string colID)
        {

            int count = 0;
            logger.pushOperation("countMatrixLinesChecked");
            try
            {
                Column col = ((Matrix)oForm.Items.Item(matrixID).Specific).Columns.Item(colID);
                
                for (int i = 1; i <= col.Cells.Count; i++)
                {
                    CheckBox chk = col.Cells.Item(i).Specific;
                    if (chk.Checked == true)
                    {
                        count++;
                    }
                }
            }
            catch (Exception e)
            {
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e);
            }
            finally
            {
                logger.releaseOperation();
            }
            return count;
        }



        public string showOpenDialog(bool selectFile = true, bool multiSelect = false, bool showNewFolderButton = true, 
            Environment.SpecialFolder rootFolder = Environment.SpecialFolder.MyComputer)
        {

            string ret = "";
            logger.pushOperation("showOpenDialog");
            try
            {

                SetFilePathName oSetFilePathName = new SetFilePathName();
                oSetFilePathName.RootFolder = rootFolder;
                oSetFilePathName.ShowNewFolderButton = showNewFolderButton;

                Thread threadGetFile = null;

                if (!selectFile)
                {
                    threadGetFile = new Thread(new ThreadStart(oSetFilePathName.SetPathName));
                    threadGetFile.SetApartmentState(ApartmentState.STA);
                    try
                    {
                        threadGetFile.Start();
                        while (!threadGetFile.IsAlive)
                        {
                            System.Windows.Forms.Application.DoEvents(); // Wait for thread to get started
                        }
                        Thread.Sleep(1);  // Wait a sec more
                        threadGetFile.Join();    // Wait for thread to end
                        //// Use file name as you will here
                        ret = oSetFilePathName.SelectedPath.Trim();
                    }
                    catch (Exception ex)
                    {
                        ret = "";
                        logger.log("Erro: " + ex.Message, Logger.LogType.ERROR, ex);
                    }
                }
                else
                {
                    threadGetFile = new Thread(new ThreadStart(oSetFilePathName.SetFileName));
                    threadGetFile.SetApartmentState(ApartmentState.STA);
                    try
                    {
                        threadGetFile.Start();
                        while (!threadGetFile.IsAlive)
                        {
                            System.Windows.Forms.Application.DoEvents(); // Wait for thread to get started
                        } // Wait for thread to get started
                        Thread.Sleep(1);  // Wait a sec more
                        threadGetFile.Join();    // Wait for thread to end
                        //// Use file name as you will here
                        ret = oSetFilePathName.SelectedFile;
                    }
                    catch (Exception ex)
                    {
                        ret = "";
                        logger.log("Erro: " + ex.Message, Logger.LogType.ERROR, ex);
                    }
                }

                threadGetFile = null;
                oSetFilePathName = null;

            }
            catch (Exception e)
            {
                ret = "";
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e);
            }
            finally
            {
                logger.releaseOperation();
            }

            return ret;
        }


        //public string getObjectCode(BoObjectTypes type, string seriesName, string subType)
        //{
        //    logger.pushOperation("getObjectCode");
        //    string ret = "";
        //    try
        //    {
        //        SB1Util.DB.DBFacade db = SB1Util.DB.DBFacade.getInstance();
        //        string num = db.nextObjectKey(type, 0, seriesName);

        //        Recordset rs = db.Query("SELECT NumSize, LastNum, BeginStr, EndStr " +
        //        "FROM NNM1 WHERE SeriesName = '" + seriesName + "' and ObjectCode = " + (int)type + 
        //        " and DocSubType = '" + subType + "'");


        //        num = rs.Fields.Item("BeginStr").Value + 
        //            num.PadLeft(rs.Fields.Item("NumSize").Value) + 
        //            rs.Fields.Item("EndStr").Value;


        //    }
        //    catch (Exception e)
        //    {
        //        ret = "";
        //        logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e);
        //    }
        //    finally
        //    {
        //        logger.releaseOperation();
        //    }
        //    return ret;
        //}


    }

}
