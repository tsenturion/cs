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
            string ipAddress = "127.0.0.1";
            int port = 12345;

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            serverSocket.Bind(ipEndPoint);

            serverSocket.Listen(1);
            Console.WriteLine("Ожидаю подключения клиента...");

            Socket clientSocket = serverSocket.Accept();
            Console.WriteLine("Клиент подключен: " + clientSocket.RemoteEndPoint.ToString());

            byte[] buffer = new byte[1024];
            int receivedBytes = clientSocket.Receive(buffer);
            string receivedMessage = Encoding.ASCII.GetString(buffer, 0, receivedBytes);

            Console.WriteLine($"Сервер:\nВ {DateTime.Now:HH:mm} от {clientSocket.RemoteEndPoint} получена строка: {receivedMessage}");

            string response = "hi, client!";
            clientSocket.Send(Encoding.ASCII.GetBytes(response));

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            serverSocket.Close();
        }
    }
}
