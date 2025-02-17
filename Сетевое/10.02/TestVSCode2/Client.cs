/*
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Collections.Generic;

namespace TestVSCode
{
    class Program
    {

        static string infoMessage = "infoMessage";
        static string wariningMessage = "wariningMessage";
        static string urgentMessage = "urgentMessage";
        enum MessageType 
        {
            Information,
            Warning,
            Urgent
        }
        static List<Socket> clients = new List<Socket>();
        static int interval = 5000;
        static void Main(string[] args)
        {
            Thread sendThread = new Thread(SendMulticastMessage);
            sendThread.IsBackground = true;
            sendThread.Start();
            Console.ReadKey();
        }
        
        static void SendMulticastMessage() {
            while (true)
            {
                Thread.Sleep(interval);
                string messageToSend = infoMessage;
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
                IPAddress multicastAddress = IPAddress.Parse("224.5.5.5");
                IPEndPoint endPoint = new IPEndPoint(multicastAddress, 4567);
                byte[] messageBytes = Encoding.Default.GetBytes(messageToSend);
                socket.SendTo(messageBytes, endPoint);
                socket.Close();
                Console.WriteLine($"Sent: {messageToSend}");
            }
        }

        static void SubscribeClient(Socket clientSocket, MessageType subsctiptionType)
        {
            clients.Add(clientSocket);
            if (subsctiptionType == MessageType.Urgent)
            {
                SendUrgentMessage(clientSocket);
            }
        }

        static void SendUrgentMessage(Socket clientSocket)
        {
            string urgentMessage = "urgentMessage";
            byte[] messageBytes = Encoding.Default.GetBytes(urgentMessage);
            foreach (var clint in clients)
            {
                clint.Send(messageBytes);
            }
            Console.WriteLine("Sent urgent message");
        }
            
    }
}

*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestVSCode2
{
    enum MessageType 
        {
            Information,
            Warning,
            Urgent
        }
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 4567);
            socket.Bind(localEndPoint);

            IPAddress multicastAddress = IPAddress.Parse("224.5.5.5");
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddress, IPAddress.Any));
            byte[] buffer = new byte[1024];
            
            Console.WriteLine("choose");
            Console.WriteLine("1 - subscribe info");
            Console.WriteLine("2 - subscribe warning");
            Console.WriteLine("3 - subscribe urgent");

            int choice = int.Parse(Console.ReadLine());
            MessageType selectedType = (MessageType)(choice - 1);

            while (true)
            {
                int bytesReceived = socket.Receive(buffer);
                string message = Encoding.Default.GetString(buffer, 0, bytesReceived);
                
                if (selectedType == MessageType.Urgent && message == "urgentMessage")
                {
                    Console.WriteLine("urgent message received");
                }
                else if (message == "infoMessage" && selectedType == MessageType.Information)
                {
                    Console.WriteLine("info message received");
                }
                else if (message == "wariningMessage" && selectedType == MessageType.Warning)
                {
                    Console.WriteLine("warning message received");
                }
                else
                {
                    Console.WriteLine("received message: " + message);
                }
            }
        }
    }
}
