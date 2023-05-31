using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCL_Voucher.Utils
{
    public class SendBrandName
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        public static int SentMsg(string phone, string message)
        {
            return SendMsg(message, phone, "TCL VN", "TCL VN", "0");
        }
        static int SendMsg(string message, string phone, string commandcode,string cpnumber,string requestid)
        {
            string messageSent = EncodeTo64(message);
            string phoneSent = phone.StartsWith("+") ? phone.Replace("+", "") : phone;
            if (phoneSent.StartsWith("0"))
            {
                string test = phoneSent.Substring(1);
                phoneSent = string.Concat("84", test);
            }
            if (!phoneSent.StartsWith("84"))
            {

                phoneSent = string.Concat("84", phoneSent);
            }
            int sendMessageResult = -1;
            try
            {

                SendMsgReceiver smsSend = new SendMsgReceiver();
                smsSend.UserName = "tclvietnam";
                smsSend.Password = "TCLVN_2020#@*()";
                smsSend.PreAuthenticate = true;
                sendMessageResult = smsSend.sendMT(phoneSent, messageSent, cpnumber, commandcode, "1", requestid, "1", "1", "0", "0");
                logger.Info(sendMessageResult);
                return sendMessageResult;
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                return sendMessageResult;
            }
        }

        static private string EncodeTo64(string toEncode)
        {

            byte[] toEncodeAsBytes

                  = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);

            string returnValue

                  = System.Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;

        }
    }
}