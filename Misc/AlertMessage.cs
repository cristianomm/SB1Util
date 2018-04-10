using System;
using System.Collections.Generic;
using System.Text;
using SAPbobsCOM;
using SB1Util.Log;
using SB1Util.DB;



namespace SB1Util.Misc
{
    public class AlertMessage
    {


        public struct RecipientInfo
        {
            public string CellularNumber;
            public string EmailAddress;
            public string FaxNumber;
            public string NameTo;
            public BoYesNoEnum SendEmail;
            public BoYesNoEnum SendFax;
            public BoYesNoEnum SendInternal;
            public BoYesNoEnum SendSMS;
            public string UserCode;
            public BoMsgRcpTypes UserType;
        }



        private Logger logger;
        private DBFacade dbFacade;

        public AlertMessage()
        {
            this.logger = Logger.getInstance();
            this.dbFacade = DBFacade.getInstance();
        }




        public void sendMessage(string subject, string msgText, 
            List<RecipientInfo> recipients,
            BoMsgPriorities priority = BoMsgPriorities.pr_Normal)
        {
            logger.pushOperation("sendMessage");
            try
            {
                Message message = null;
                MessagesService serv = dbFacade.Connection.Company.GetCompanyService().GetBusinessService(ServiceTypes.MessagesService);
                MessageHeader header = null;

                message = serv.GetDataInterface(MessagesServiceDataInterfaces.msdiMessage);

                message.Subject = subject;
                message.Text = msgText;
                message.Priority = priority;
                
                int count = 0;
                foreach (RecipientInfo info in recipients)
                {

                    message.RecipientCollection.Add();
                    message.RecipientCollection.Item(count).CellularNumber = info.CellularNumber;
                    message.RecipientCollection.Item(count).EmailAddress = info.EmailAddress;
                    message.RecipientCollection.Item(count).FaxNumber = info.FaxNumber;
                    message.RecipientCollection.Item(count).NameTo = info.NameTo;
                    message.RecipientCollection.Item(count).SendEmail = info.SendEmail;
                    message.RecipientCollection.Item(count).SendFax = info.SendFax;
                    message.RecipientCollection.Item(count).SendInternal = info.SendInternal;
                    message.RecipientCollection.Item(count).SendSMS = info.SendSMS;
                    message.RecipientCollection.Item(count).UserCode = info.UserCode;
                    message.RecipientCollection.Item(count).UserType = info.UserType;

                    count++;
                }

                header = serv.SendMessage(message);

                logger.log("Mensagem enviada por em " + 
                    header.SentDate.ToShortDateString() + " às " + header.SentTime.ToShortTimeString(), 
                    Logger.LogType.INFO, null, false);

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

        public RecipientInfo createRecipientInfo( 
            string NameTo = "",
            string UserCode = "",
            BoYesNoEnum SendInternal = BoYesNoEnum.tNO,
            BoMsgRcpTypes UserType = BoMsgRcpTypes.rt_InternalUser,
            BoYesNoEnum SendEmail = BoYesNoEnum.tNO,
            string EmailAddress = "",
            BoYesNoEnum SendFax = BoYesNoEnum.tNO,
            string FaxNumber = "",
            BoYesNoEnum SendSMS = BoYesNoEnum.tNO,
            string CellularNumber = ""           
        )
        {
            RecipientInfo rf;
            rf.CellularNumber = CellularNumber;
            rf.EmailAddress = EmailAddress;
            rf.FaxNumber = FaxNumber;
            rf.NameTo = NameTo;
            rf.SendEmail = SendEmail;
            rf.SendFax = SendFax;
            rf.SendInternal = SendInternal;
            rf.SendSMS = SendSMS;
            rf.UserCode = UserCode;
            rf.UserType = UserType;

            return rf;
        }





    }
}
