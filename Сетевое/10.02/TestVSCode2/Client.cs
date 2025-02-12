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
            string ipAddress = "127.0.0.1";
            int port = 12345;

            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));
            Console.WriteLine("Подключено к серверу...");

            string greeting = "hi, client!";
            clientSocket.Send(Encoding.ASCII.GetBytes(greeting));

            byte[] buffer = new byte[1024];
            int receivedBytes = clientSocket.Receive(buffer);
            string response = Encoding.ASCII.GetString(buffer, 0, receivedBytes);

            Console.WriteLine($"Клиент:\nВ {DateTime.Now:HH:mm} от {clientSocket.RemoteEndPoint} получена строка: {response}");

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }
}
