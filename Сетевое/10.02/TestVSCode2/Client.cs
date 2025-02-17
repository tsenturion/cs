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
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 4567);
            socket.Bind(localEndPoint);

            IPAddress multicastAddress = IPAddress.Parse("224.5.5.5");
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddress, IPAddress.Any));
            byte[] buffer = new byte[1024];
            
            Console.WriteLine("client start");

            while (true)
            {
                int bytesReceived = socket.Receive(buffer);
                string message = Encoding.Default.GetString(buffer, 0, bytesReceived);
                Console.WriteLine($"Received: {message}");
            }
        }
    }
}
