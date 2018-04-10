using System;
using System.Collections.Generic;
using SB1Util.Misc;

namespace SB1Util.ApprovalProcedures
{
    public class ApprovalCondition: SAPObject
    {
        private string condition;
        private ApprovalProcedure approvalProcedure;







        public ApprovalProcedure ApprovalProcedure
        {
            get { return approvalProcedure; }
            set { approvalProcedure = value; }
        }

        public string Condition
        {
            get { return condition; }
            set { condition = value; }
        }

        public override long add()
        {
            
            userTable.UserFields.Fields.Item("U_Condition").Value = condition;
            userTable.UserFields.Fields.Item("U_ApprovalCode").Value = approvalProcedure.Code;

            return base.add();
        }

    }
}
