using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TestVSCode
{
    class Program
    {

        static string message = "hi";
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
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
                IPAddress multicastAddress = IPAddress.Parse("224.5.5.5");
                IPEndPoint endPoint = new IPEndPoint(multicastAddress, 4567);
                byte[] messageBytes = Encoding.Default.GetBytes(message);
                socket.SendTo(messageBytes, endPoint);
                socket.Close();
                Console.WriteLine($"Sent: {message}");
            }
        }
            
    }
}
