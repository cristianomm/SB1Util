using System;
using System.Collections.Generic;
using SAPbobsCOM;
using SB1Util.DB;

namespace SB1Util.ApprovalProcedures
{
    public class ApprovalProcedures
    {

        public enum ApprovalStatus
        {
            APROVADO, NAO_APROVADO, PENDENTE
        }

        private DBFacade oDBFacade;

        public ApprovalProcedures()
        {
            this.oDBFacade = DBFacade.getInstance();
        }

        public void createTables()
        {
            try
            {
                //Procedimentos de Aprovacao
                oDBFacade.CreateTable("SB1_APPROVAL", "Approval Procedure", BoUTBTableType.bott_NoObject);
                //oDBFacade.CreateField("SB1_APPROVAL", "ApprovalCode", "Approval", BoFieldTypes.db_Numeric, 11);
                oDBFacade.CreateField("SB1_APPROVAL", "isActive", "Active", BoFieldTypes.db_Alpha, 5);
                oDBFacade.CreateField("SB1_APPROVAL", "ApprovalStatus", "Approval Status", BoFieldTypes.db_Alpha, 30);

                //Estágios de Aprovacao
                oDBFacade.CreateTable("SB1_APPROVAL_STAGES", "Approval Stages", BoUTBTableType.bott_NoObject);
                oDBFacade.CreateField("SB1_APPROVAL_STAGES", "ApprovalCode", "Approval Procedure", BoFieldTypes.db_Numeric, 11);
                oDBFacade.CreateField("SB1_APPROVAL_STAGES", "WstCode", "Stage", BoFieldTypes.db_Numeric, 11);
                oDBFacade.CreateField("SB1_APPROVAL_STAGES", "ApprovalStatus", "Approval Status", BoFieldTypes.db_Alpha, 30);

                //Condicoes de Aprovacao
                oDBFacade.CreateTable("SB1_APPROVAL_CONDITION", "Approval Conditions", BoUTBTableType.bott_NoObject);
                oDBFacade.CreateField("SB1_APPROVAL_CONDITION", "ApprovalCode", "Approval Procedure", BoFieldTypes.db_Numeric, 11);
                oDBFacade.CreateField("SB1_APPROVAL_CONDITION", "Condition", "Approval Condition", BoFieldTypes.db_Memo, 5000);

                //Documentos para aprovacao
                oDBFacade.CreateTable("SB1_APPROVAL_DOCS", "Approval Stages", BoUTBTableType.bott_NoObject);
                oDBFacade.CreateField("SB1_APPROVAL_DOCS", "DocEntry", "Document", BoFieldTypes.db_Numeric, 11);
                oDBFacade.CreateField("SB1_APPROVAL_DOCS", "ApprovalCode", "Approval Procedure", BoFieldTypes.db_Numeric, 11);
                oDBFacade.CreateField("SB1_APPROVAL_DOCS", "ApprovalStatus", "Approval Status", BoFieldTypes.db_Alpha, 30);


            }
            catch (Exception e)
            {
            }

        }



        public LinkedList<Object> getDocumentsByStatus(ApprovalStatus status)
        {
            LinkedList<Object> ret = new LinkedList<object>();
            try
            {
            }
            catch (Exception e)
            {
                ret = new LinkedList<object>();
            }
            return ret;
        }




        public int addApprovalProcedure()
        {
            int ret = 0;
            try
            {

            }
            catch (Exception e)
            {
                ret = -1;
            }
            return ret;
        }


    }
}
