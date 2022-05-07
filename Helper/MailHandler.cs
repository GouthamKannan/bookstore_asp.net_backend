using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net;
using System.Net.Mail;

using BookStore.Models;

namespace BookStore.Helper
{
    public class MailHandler
    {
        static string AllChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

        // Create an SMTP client to sent email
        private static SmtpClient CreateSmtpClient()
        {
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["FromMail"],
                                                       ConfigurationManager.AppSettings["FromPassword"]);
            client.Host = "smtp.gmail.com";

            return client;
        }

        static SmtpClient client = CreateSmtpClient();
        
        // Generate random string for verification and password reset code
        private static string GenerateRandomString(int Length)
        {
            var random = new Random();
            string Token = new string(
                Enumerable.Repeat(AllChars, Length)
                .Select(token => token[random.Next(token.Length)]).ToArray());

            return Token.ToString();
        }

        // Send the given mail body and subject to ToMail
        private static bool SendMail(string ToMail, string Subject, string MailBody)
        {
            System.Diagnostics.Debug.WriteLine(ConfigurationManager.AppSettings["FromMail"]);
            System.Diagnostics.Debug.WriteLine(ConfigurationManager.AppSettings["FromPassword"]);

            // Create a new mail message
            MailMessage message = new MailMessage();
            message.From = new MailAddress(ConfigurationManager.AppSettings["FromMail"]);
            message.To.Add(new MailAddress(ToMail));

            message.IsBodyHtml = true;
            message.Body = MailBody;
            message.Subject = Subject;

            try
            {
                // Send mail
                client.Send(message);
                System.Diagnostics.Debug.WriteLine("Sent Mail");
                return true;
            }

            // Handle Smtp Exception            
            catch (System.Net.Mail.SmtpException ex)
            {
                System.Diagnostics.Debug.WriteLine("Smtp Exception");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return false;
            }

            // Handle other execptions
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return false;
            }


        }

        // Generate and send verification mail to users Email address
        public static bool SendVerificationCode(string UserName, string Email, string Password, string RequestUrl)
        {
            // Initialize the mail contents
            string ver_code = GenerateRandomString(8);
            string link = RequestUrl + "/user/verify_email/" + ver_code;
            string body = "Hello, Please Click on the link to verify your email. <a href=" + link + ">Click here to verify</a>";
            string subject = "Email verification link";

            // Send the email
            bool sent = SendMail(Email, subject, body);
            //bool sent = false;

            if (sent)
            {
                // Store the user details in the database
                BookStoreEntities dbContext = new BookStoreEntities();
                user User = new user
                {
                    user_name = UserName,
                    email = Email,
                    password = Password,
                    ver_code = ver_code,
                    status = "inactive",
                    reset_code = null,
                    jwt_token = null
                };
                dbContext.users.Add(User);
                dbContext.SaveChanges();

                return true;
            }
            else
            {
                return false;
            }
        }
        
        // Send Password reset link to the users Email address
        public static bool SendResetCode(string email, string RequestUrl)
        {
            // Initialize Email contents
            string reset_code = GenerateRandomString(8);
            string link = RequestUrl + "/user/get_reset_form/" + reset_code;
            string body = "Hello, Please Click on the link to reset your password. <a href=" + link + ">Click here to reset</a>";
            string subject = "Password reset link";

            // Send the Email
            bool sent = SendMail(email, subject, body);

            if (sent)
            {
                // Save reset_code in database
                BookStoreEntities dbContext = new BookStoreEntities();
                var user = dbContext.users.FirstOrDefault(cur_user => cur_user.email == email);
                if (user != null)
                {
                    user.reset_code = reset_code;
                    dbContext.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}