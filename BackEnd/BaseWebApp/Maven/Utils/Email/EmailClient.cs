using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace BaseWebApp.Maven.Utils.Email
{
    public class EmailClient
    {
        private static MailAddress DEFAULT_FROM = new MailAddress(Config.GetConfig("EMAIL_FROM_EMAIL"), Config.GetConfig("EMAIL_FROM_NAME"));
        private static MailAddress DEFAULT_TO = new MailAddress(Config.GetConfig("EMAIL_TO_EMAIL"), Config.GetConfig("EMAIL_TO_NAME"));

        public static bool SendEmail(string email, string subject, string body)
        {
            return SendEmail(email, null, subject, body);
        }

        public static bool SendEmail(string email, string name, string subject, string body)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            return SendEmail(new MailAddress(email, name), subject, body);
        }

        public static bool SendEmail(MailAddress to, string subject, string body)
        {
            return SendEmail(null, to, subject, body, new List<Attachment>());
        }

        public static bool SendEmail(string email, string name, string subject, string body, Attachment attachment)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            return SendEmail(null, new MailAddress(email, name), subject, body, new List<Attachment> { attachment });
        }

        public static bool SendEmail(MailAddress from, MailAddress to, string subject, string body, List<Attachment> attachments)
        {
            try
            {
                if (from == null)
                {
                    from = DEFAULT_FROM;
                }

                if (to != null)
                {

                    if (!Config.SendEmail())
                    {
                        subject += "     [to:" + to.Address + "," + to.DisplayName + "]";
                        to = DEFAULT_TO;
                    }

                    MailMessage message = new MailMessage(from, to)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    foreach (Attachment att in attachments)
                    {
                        message.Attachments.Add(att);
                    }

                    Logger.Log("EmailClient Start :: F:" + from.Address + ",T:" + to.Address + "," + to.DisplayName + ",S:" + subject);
                    Task.Run(() => SendEmailAsync(message));

                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
            }

            return false;
        }

        public static void SendEmailAsync(MailMessage message)
        {
            SmtpClient client = GetClient();
            client.SendCompleted += SendCompletedCallback;
            client.SendAsync(message, message);
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                MailMessage msg = (MailMessage)e.UserState;

                string text = "F:" + msg.From.Address + ",T:" + msg.To[0].Address + "," + msg.To[0].DisplayName + ",S:" + msg.Subject;

                if (e.Cancelled)
                {
                    Logger.Log("EmailClient Cancelled :: " + text);
                }
                if (e.Error != null)
                {
                    Logger.Log("EmailClient Error :: " + text + " Error: " + e.Error);
                }
                else
                {
                    Logger.Log("EmailClient Done :: " + text);
                }

                if (msg != null)
                    msg.Dispose();
            }
            catch (Exception er)
            {
                Logger.Log(er.ToString());
            }
        }

        private static SmtpClient GetClient()
        {
            return new SmtpClient
            {
                Host = Config.GetConfig("EMAIL_SMTP_HOST"),
                Port = Config.GetSmtpPort(),
                EnableSsl = Config.UseSmtpSSL(),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Config.GetConfig("EMAIL_SMTP_USERNAME"), Config.GetConfig("EMAIL_SMTP_PASSWORD")),
            };
        }
    }
}