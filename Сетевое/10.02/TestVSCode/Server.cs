using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestVSCode
{
    class Program
    {
        static Dictionary<string, DateTime> clientLastActivity = new Dictionary<string, DateTime>();
        static TimeSpan inactivityLimit = TimeSpan.FromMinutes(10);
        static int maxConcurrentClients = 5;
        static Dictionary<string, List<DateTime>> clientsRequests = new Dictionary<string, List<DateTime>>();
        static int maxRequestedPerHour = 10;
        static TimeSpan timeFrame = TimeSpan.FromHours(1);
        static void Main(string[] args)
        {
            int port = 11000;
            UdpClient udpServer = new UdpClient(port);
            Console.WriteLine("server active");
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            Thread inactivityCheckThread = new Thread(CheckForInactiveClients);
            inactivityCheckThread.IsBackground = true;
            inactivityCheckThread.Start();
            while (true)
            {
                try{
                    string clientAddress = remoteEndPoint.ToString();
                    byte[] recivedBytes = udpServer.Receive(ref remoteEndPoint);
                    string recivedData = Encoding.UTF8.GetString(recivedBytes);
                    Console.WriteLine("Received from {0}:{1}: {2}", remoteEndPoint.Address, remoteEndPoint.Port, recivedData);
                    
                    if (!IsRequestAllowed(remoteEndPoint.ToString()))
                    {
                        string response = "limit is out of range";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        udpServer.Send(responseBytes, responseBytes.Length, remoteEndPoint);
                        Console.WriteLine("Sent to {0}:{1}: {2}", remoteEndPoint.Address, remoteEndPoint.Port, response);
                    }
                    else if (!IsClentAllowed(clientAddress)) {
                        string response = "limit is exceeded for this client";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        udpServer.Send(responseBytes, responseBytes.Length, remoteEndPoint);
                        Console.WriteLine("Sent to {0}:{1}: {2}", remoteEndPoint.Address, remoteEndPoint.Port, response);
                    }
                    else{
                        string response = GetComponentPrice(recivedData);
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        udpServer.Send(responseBytes, responseBytes.Length, remoteEndPoint);
                        Console.WriteLine("Sent to {0}:{1}: {2}", remoteEndPoint.Address, remoteEndPoint.Port, response);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        
        static bool IsClentAllowed(string clientAddress)
        {
            if (!clientLastActivity.ContainsKey(clientAddress))
            {
                clientLastActivity[clientAddress] = DateTime.Now;
                return true;
            }
            if (clientLastActivity.Count >= maxConcurrentClients)
            {
                return false;
            }
            return true;
        }

        static void CheckForInactiveClients()
        {
            while (true)
            {
                try {
                    foreach (var clint in new List<string>(clientLastActivity.Keys))
                    {
                        if (DateTime.Now - clientLastActivity[clint] > inactivityLimit)
                        {
                            clientLastActivity.Remove(clint);
                            clientsRequests.Remove(clint);
                            Console.WriteLine($"Client {clint} inactive. Removed from list.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Thread.Sleep(10000); // check every 10 seconds
            }
        }

        static bool IsRequestAllowed(string clientAddress)
        {
            if (!clientsRequests.ContainsKey(clientAddress))
            {
                clientsRequests[clientAddress] = new List<DateTime>();
            }
            clientsRequests[clientAddress].RemoveAll(time => DateTime.Now - time > timeFrame);
            if (clientsRequests[clientAddress].Count >= maxRequestedPerHour)
            {
                return false;
            }
            clientsRequests[clientAddress].Add(DateTime.Now);
            return true;
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
