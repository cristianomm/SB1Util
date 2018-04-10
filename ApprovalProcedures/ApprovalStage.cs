using System;
using System.Collections.Generic;
using SB1Util.Misc;

namespace SB1Util.ApprovalProcedures
{
    public class ApprovalStage:SAPObject
    {

        private ApprovalProcedures.ApprovalStatus status;
        private ApprovalProcedure approvalProcedure;


        public ApprovalProcedure ApprovalProcedure
        {
            get { return approvalProcedure; }
            set { approvalProcedure = value; }
        }


        public override long add()
        {

            userTable.UserFields.Fields.Item("U_ApprovalStatus").Value = status.ToString();
            userTable.UserFields.Fields.Item("U_ApprovalCode").Value = approvalProcedure.Code;

            return base.add();
        }

    }
}
