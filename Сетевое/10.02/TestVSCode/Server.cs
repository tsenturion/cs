using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Mail;


namespace TestVSCode
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //теория
            //команды почты

            // отправка письма с использованием SmtpClient
            // mail from <адрес отправителя> отправитель указывается в формате email
            // rcpt to:<адрес получателя> получатель указывается в формате email
            // data начинается письмом и заканчивается двумя пустыми строками


            // hello/helo индентификация smtp клиента
            // quit завершает работу с smtp клиентом


            // body текст письма

            //практика
            MailMessage message = new MailMessage();

            message.From = new MailAddress("sender@example.com");
            message.To.Add("receiver@example.com");

            message.Subject = "Test email";
            message.Body = "This is a test email.";

            message.IsBodyHtml = false;

            Attachment attachment = new Attachment(@"C:\file.jpg");
            message.Attachments.Add(attachment);

            //SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.Port = 587;

            smtpClient.Credentials = new System.Net.NetworkCredential("sender@example.com", "password");
            smtpClient.EnableSsl = true;

            try {
                smtpClient.Send(message);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex) {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}