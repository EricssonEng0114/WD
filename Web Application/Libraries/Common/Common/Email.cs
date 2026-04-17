using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace UBPC.Web.Common
{
    public class Email
    {
        private int ServerPortNo = 0;
        private bool isSSLEnabled = true;

        private string m_smtpAddress;
        public string SMTPAddress
        {
            get { return m_smtpAddress; }
            set { m_smtpAddress = value; }
        }

        private string m_emailFrom;
        public string EmailFrom
        {
            get { return m_emailFrom; }
            set { m_emailFrom = value; }
        }

        private string m_emailFromPassword;
        public string EmailFromPassword
        {
            get { return m_emailFromPassword; }
            set { m_emailFromPassword = value; }
        }

        private string m_emailTo;
        public string EmailTo
        {
            get { return m_emailTo; }
            set { m_emailTo = value; }
        }

        private string m_subject;
        public string Subject
        {
            get { return m_subject; }
            set { m_subject = value; }
        }

        private string m_bodyContent;
        public string Content
        {
            get { return m_bodyContent; }
            set { m_bodyContent = value; }
        }


        public Email(int portNo, bool enableSSL) 
        {
            ServerPortNo = portNo;
            isSSLEnabled = enableSSL;
        }

        public void SendEmail() 
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(m_emailFrom);
                mail.To.Add(m_emailTo);
                mail.Subject = m_subject;
                mail.Body = m_bodyContent;
                mail.IsBodyHtml = true;
                // Can set to false, if you are sending pure text.

                //mail.Attachments.Add(new Attachment("C:\\SomeFile.txt"));
                //mail.Attachments.Add(new Attachment("C:\\SomeZip.zip"));

                using (SmtpClient smtp = new SmtpClient(m_smtpAddress, ServerPortNo))
                {
                    smtp.Credentials = new NetworkCredential(m_emailFrom, m_emailFromPassword);
                    smtp.EnableSsl = isSSLEnabled;
                    smtp.Send(mail);
                }
            }
        }
    }
}
