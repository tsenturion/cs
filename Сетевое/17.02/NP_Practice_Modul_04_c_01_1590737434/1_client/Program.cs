using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MulticastClient
{
	public class MulticastClient
	{
		void Listener()
		{
			while (true)
			{
				Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 4567);
				soc.Bind(ipep);
				IPAddress ip = IPAddress.Parse("224.5.5.5");
				soc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));
				soc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));
				byte[] buff = new byte[1024];
				soc.Receive(buff);
				Console.WriteLine(Encoding.UTF8.GetString(buff) + "\n");
				soc.Close();
			}
		}
		Thread listen;
		public MulticastClient()
		{
			listen = new Thread(new ThreadStart(Listener));
			listen.IsBackground = true;
			listen.Start();
		}
		static void Main()
		{
			MulticastClient client = new();
			Thread.Sleep(120000);
		}
	}
}
