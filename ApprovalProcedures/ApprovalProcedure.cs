using System;
using System.Collections.Generic;
using SB1Util.Misc;
using SB1Util.DB;
using SAPbobsCOM;

namespace SB1Util.ApprovalProcedures
{
    public class ApprovalProcedure:SAPObject
    {

        private string isActive;
        private LinkedList<ApprovalCondition> conditions;
        private LinkedList<ApprovalStage> stages;


        public ApprovalProcedure()
        {
            this.conditions = new LinkedList<ApprovalCondition>();
            this.stages = new LinkedList<ApprovalStage>();
        }

        public LinkedList<ApprovalCondition> Conditions
        {
            get
            {
                return conditions;
            }
        }
        
        public LinkedList<ApprovalStage> Stages
        {
            get
            {
                return stages;
            }
        }


        public int addApprovalStage()
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


        public int addApprovalCondition()
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


        public override long add()
        {
            foreach (ApprovalCondition ac in conditions)
            {
                ac.add();
            }

            foreach (ApprovalStage aps in stages)
            {
                aps.add();
            }

            userTable.UserFields.Fields.Item("U_isActive").Value = isActive;
            //userTable.UserFields.Fields.Item("U_DocEntry").Value = docEntry;

            return base.add();
        }

        public override long loadFromDB(string key, UserObjectFactory oFactory)
        {
            long ret = 0;
            try
            {
                DBFacade db = DBFacade.getInstance();

                //busca os estagios
                Recordset rs = db.Query(
                    "SELECT APST.Code " +
                    "FROM [@SB1_APPROVAL] APRV " +
                    "INNER JOIN [@SB1_APPROVAL_STAGE] APST " +
                    "ON APRV.Code = APST.U_ApprovalCode " +
                    "WHERE APRV.Code = " + key
                    );
                while (!rs.EoF)
                {
                    ApprovalStage aps = new ApprovalStage();
                    aps.loadFromDB((string)rs.Fields.Item("Code").Value, oFactory);
                    stages.AddLast(new LinkedListNode<ApprovalStage>(aps));
                    rs.MoveNext();
                }


                //busca as condicoes
                rs = db.Query(
                    "SELECT APCD.Code " +
                    "FROM [@SB1_APPROVAL] APRV " +
                    "INNER JOIN [@SB1_APPROVAL_CONDITION] APCD " +
                    "ON APRV.Code = APCD.U_ApprovalCode " +
                    "WHERE APRV.Code =  " + key
                    );
                while (!rs.EoF)
                {
                    ApprovalCondition ac = new ApprovalCondition();
                    ac.loadFromDB((string)rs.Fields.Item("Code").Value, oFactory);
                    conditions.AddLast(new LinkedListNode<ApprovalCondition>(ac));
                    rs.MoveNext();
                }

                long r = base.loadFromDB(key, oFactory);
                isActive = (string)userTable.UserFields.Fields.Item("U_isActive").Value;
            }
            catch (Exception e)
            {
                ret = -1;
            }

            return ret;
        }



    }
}
