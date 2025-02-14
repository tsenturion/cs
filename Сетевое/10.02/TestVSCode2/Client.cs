using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestVSCode2
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverAdress = "127.0.0.1";
            int port = 11000;
            UdpClient udpClient = new UdpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAdress), port);
            
            Console.WriteLine("client active, enter query");
            while (true)
            {
                Console.Write("query");
                string query = Console.ReadLine();
                if (query == "exit")
                {
                    break;
                }
                byte[] requestedBytes = Encoding.UTF8.GetBytes(query);
                udpClient.Send(requestedBytes, requestedBytes.Length, serverEndPoint);

                byte[] responseBytes = udpClient.Receive(ref serverEndPoint);
                string response = Encoding.UTF8.GetString(responseBytes);
                Console.WriteLine($"Server response: {response}");
            }
            udpClient.Close();
        }
    }
}
