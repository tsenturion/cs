using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestVSCode
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 11000;
            UdpClient udpServer = new UdpClient(port);
            Console.WriteLine("server active");
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            while (true)
            {
                try{
                    byte[] recivedBytes = udpServer.Receive(ref remoteEndPoint);
                    string recivedData = Encoding.UTF8.GetString(recivedBytes);
                    Console.WriteLine("Received from {0}:{1}: {2}", remoteEndPoint.Address, remoteEndPoint.Port, recivedData);
                    string response = GetComponentPrice(recivedData);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    udpServer.Send(responseBytes, responseBytes.Length, remoteEndPoint);
                    Console.WriteLine("Sent to {0}:{1}: {2}", remoteEndPoint.Address, remoteEndPoint.Port, response);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        
        private static string GetComponentPrice(string componentName)
        {
            switch (componentName.ToLower())
            {
                case "cpu":
                    return "CPU price: $1000";
                case "ram":
                    return "RAM price: $500";
                case "motherboard":
                    return "Motherboard price: $2000";
                default:
                    return "Component not found";
            }
        }
    }
}
