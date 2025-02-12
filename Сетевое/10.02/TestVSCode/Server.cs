using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestVSCode
{
    class Program
    {
        private static int MaxQuotesCount = 5;

        private const int MaxConnections = 3;
        private static int currentConnections = 0;
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Server started on port 5000...");
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                if (currentConnections >= MaxConnections)
                {
                    NetworkStream stream = client.GetStream();
                    StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) {AutoFlush = true};
                    writer.WriteLine("Server is full. Please try again later.");
                    client.Close();
                    continue;
                }
                Interlocked.Increment(ref currentConnections);

                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
    
        }
        
        private static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            string clientEndPoint = client.Client.RemoteEndPoint.ToString();
            DateTime connectedTime = DateTime.Now;
            lock (logLock)
            {
                connectionLogs.Add($"[{connectedTime.ToShortTimeString()}] {clientEndPoint} connected.");
                Console.WriteLine($"[{connectedTime.ToShortTimeString()}] {clientEndPoint} connected.");
            }
            
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);

            int quoteCount = 0;

            try{
                string request;
                while ((request = reader.ReadLine())!= null)
                {
                    if (request.ToLower() == "exit")
                    {
                        break;
                    }
                    if (quoteCount >= MaxQuotesCount)
                    {
                        writer.WriteLine("Quotes limit reached. Please try again later.");
                        break;
                    }
                    Random rand = new Random();
                    string quote = quotes[rand.Next(quotes.Count)];
                    writer.WriteLine(quote);
                    quoteCount++;
                }
            }
            finally
            {
                DateTime disconnectedTime = DateTime.Now;
                lock (logLock)
                {
                    connectionLogs.Add($"[{disconnectedTime.ToShortTimeString()}] {clientEndPoint} disconnected.");
                    Console.WriteLine($"[{disconnectedTime.ToShortTimeString()}] {clientEndPoint} disconnected.");
                }
                Interlocked.Decrement(ref currentConnections);
                Console.WriteLine("Client disconnected...");
                client.Close();
            }
        }
        
        private static List<string> quotes = new List<string>
        {
            "The future belongs to those who believe in the beauty of their dreams.",
            "Believe you can and you're halfway there.",
            "Success is not final, failure is not fatal: It is the courage to continue that counts."
        };

        private static List<string> connectionLogs = new List<string>();
        
        private static readonly object logLock = new object();
    }
}
