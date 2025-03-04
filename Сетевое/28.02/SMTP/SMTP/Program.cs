using System.Net;
using System.Net.Mail;

namespace Program
{
	class Program
	{ 
		static void Main()
		{
			//HELO	EHLO	MAIL FROM	RCPT TO
			//DATA	QUIT	RSET	NOOP	VRFY	EXPN	HELP

			MailMessage message = new();
			message.From = new MailAddress("@gmail.com");
			message.To.Add("@gmail.com");

			message.Subject = "test email";
			message.Body = "this is a test email, my friend";

			message.IsBodyHtml = false;

			//Attachment att = new(@"C:\\file.jpg");
			//message.Attachments.Add

			SmtpClient sc = new("smtp.gmail.com", 587);
			sc.Credentials = new NetworkCredential("@gmail.com", "");
			sc.EnableSsl = true;

			try
			{
				sc.Send(message);
				Console.WriteLine("Отправлено");
			}
			catch(Exception e)
			{
				Console.WriteLine("Не отправлено: " + e.Message);
			}
		}
	}
}