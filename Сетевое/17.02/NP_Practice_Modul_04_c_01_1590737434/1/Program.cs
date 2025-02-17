using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace MulticastServer
{
	public class MulticastServer
	{
		static string message = "Hello network!!!";
		static int Interval = 1000;
		static void multicastSend()
		{
			while (true)
			{
				Thread.Sleep(Interval);
				Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				soc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
				IPAddress dest = IPAddress.Parse("224.5.5.5");
				soc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(dest));
				IPEndPoint ipep = new IPEndPoint(dest, 4567);
				soc.Connect(ipep);
				soc.Send(Encoding.Default.GetBytes(message));
				soc.Close();
			}
		}
		void SendMulticastMessage(string text)
		{
			Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
			IPAddress destination = IPAddress.Parse("224.5.5.5");
			socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(destination));
			IPEndPoint IPEnd = new(destination, 4567);
			socket.Connect(IPEnd);
			socket.Send(Encoding.UTF8.GetBytes(text));
			socket.Close();
		}
		Thread Sender = new Thread(new ThreadStart(multicastSend));
		public MulticastServer()
		{
			//Sender.IsBackground = true;
			//Sender.Start();
		}

		static void Main()
		{
			MulticastServer server = new();

			string message = ""; 
			message = Console.ReadLine();
			server.SendMulticastMessage(message);
		}
	}
}
