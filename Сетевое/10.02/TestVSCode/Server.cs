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
        /*
        Создайте приложение для отображения информации о 
        фильме. Пользователь вводит название фильма, приложение 
        отображает информацию о нём. Для поиска информации 
        о фильме используйте http://www.omdbapi.com/.
        Добавьте возможность отсылки 
        письма с полученными результатами поиска. Для отсылки 
        письма нужно будет указать адресата, заголовок, текст пись
        ма, результаты поиска прикрепляются отдельным файлом
        */
        private const string ApiKey = "f360ac11";

        private const string ApiUrl = "http://www.omdbapi.com/";
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter movie title:");
            string movieTitle = Console.ReadLine();
            if (!string.IsNullOrEmpty(movieTitle))
            {
                await GetMovieInfo(movieTitle);
            }
            else{
                Console.WriteLine("Invalid title!");
            }
        }
        
        private static async Task GetMovieInfo(string title)
        {
            try{
                using (HttpClient client = new HttpClient())
                {
                    string requstUrl = $"{ApiUrl}?t={title}&apikey={ApiKey}";
                    HttpResponseMessage response = await client.GetAsync(requstUrl);
                    response.EnsureSuccessStatusCode();
                    string jsonString = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(jsonString);
                    if(json["Response"].Value<string>() == "True")
                    {
                        Console.WriteLine($"Title: {json["Title"]}");
                        Console.WriteLine($"Year: {json["Year"]}");
                        Console.WriteLine($"Rated: {json["Rated"]}");
                        Console.WriteLine($"Genre: {string.Join(", ", json["Genre"])}");
                        Console.WriteLine($"Director: {json["Director"]}");
                        Console.WriteLine($"Actors: {json["Actors"]}");
                        Console.WriteLine($"Plot: {json["Plot"]}");
                        Console.WriteLine($"отправить письмо?");
                        string answer = Console.ReadLine();
                        if(answer.ToLower() == "да")
                        {
                            await SendEmail(json);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: {json["Error"]}");
                    }
    
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error connecting to the API: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        private static async Task SendEmail(JObject json)
        {
            try{
                Console.WriteLine("адрес получателя");
                string recipientEmail = Console.ReadLine();
                Console.WriteLine("заголовок письма");
                string subject = Console.ReadLine();
                Console.WriteLine("текст письма");
                string body = Console.ReadLine();
                string filePath = CreateResultFile(json);
                string senderEmail = "";
                string senderPassword = "";
                string smtpServer = "smpt.gmail.com";
                int smtpPort = 587;

                MailMessage message = new MailMessage();
                message.From = new MailAddress(senderEmail);
                message.To.Add(recipientEmail);
                message.Subject = subject;
                message.Body = body;
                message.Attachments.Add(new Attachment(filePath));

                SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
                smtpClient.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
                smtpClient.EnableSsl = true;
                smtpClient.Send(message);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
        
        private static string CreateResultFile(JObject json)
        {
            string fileName = $"{json["Title"]}_{DateTime.Now.Ticks}.txt";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            using (StreamWriter writer = File.CreateText(filePath))
            {
                writer.WriteLine($"Title: {json["Title"]}");
                writer.WriteLine($"Year: {json["Year"]}");
                writer.WriteLine($"Rated: {json["Rated"]}");
                writer.WriteLine($"Genre: {string.Join(", ", json["Genre"])}");
                writer.WriteLine($"Director: {json["Director"]}");
                writer.WriteLine($"Actors: {json["Actors"]}");
                writer.WriteLine($"Plot: {json["Plot"]}");
            }
            return filePath;
        }
    
    }
}