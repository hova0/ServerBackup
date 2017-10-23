using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWR.ServerBackup.Library
{
    public class email
    {

        //private string mailusername;
        //private string mailpassword;
        //private string mailhost;

        public static System.Net.Mail.SmtpException LastException { get; set; }

        public static bool Send(string toaddress, string fromaddress,string subject, string body)
        {
            System.Net.Mail.SmtpClient sc = new System.Net.Mail.SmtpClient();
            if(toaddress.Contains(';'))
                toaddress = toaddress.Replace(';', ',');
            try
            {
                sc.Send(fromaddress, toaddress, subject, body);
                sc.Dispose();
                return true;
            }catch(System.Net.Mail.SmtpException se)
            {
                LastException = se;
                sc.Dispose();
                return false;
            }
        }

        

        public static void SendAsync(string toaddress, string fromaddress, string subject, string body)
        {
            System.Net.Mail.SmtpClient sc = new System.Net.Mail.SmtpClient();
            try
            {
                sc.SendAsync(fromaddress, toaddress, subject, body, null);
                sc.Dispose();
            }
            catch (System.Net.Mail.SmtpException se)
            {
                LastException = se;
                sc.Dispose();
            }
        }


        //Returns whether credentials have been configured in machine.config
        public static bool IsEmailConfigured()
        {
            System.Net.Mail.SmtpClient sc = new System.Net.Mail.SmtpClient();
            if (sc.Credentials != null && sc.Host != "127.0.0.1")
            {
                
                sc.Dispose();



                return true;
            }
            sc.Dispose();
            return false;
        }

    }
}
