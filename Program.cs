using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;


namespace IpSubnetScanner
{
    class Program
    {
        static void Main(string[] args)
        {
			SubnetScanner ss = new SubnetScanner();
			ss.PrintInterfaces();
			
			// Adapter Sellection
			int sc = -1;
			while(true)
			{
				try
				{
					Console.Write("Select Adapter => ");
					sc = Int32.Parse(Console.ReadLine());
					if (sc<ss.Count && sc>-1) break;
					else Console.WriteLine("Out of range");
				}
				catch{ Console.WriteLine("Invalid number");} 
			}
			
		// Start Scan
		Console.WriteLine("\n==============================================================\n");
			ss.Scan(sc);
        }
    }
	
	public class SubnetScanner
	{
		
		private List<string> Subnets;
		private List<string> IPaddresses;
		private NetworkInterface[] Interfaces;
		
		//Adapters Length
		public int Count;
		
		public SubnetScanner()
		{
			Subnets = new List<string>();
			IPaddresses = new List<string>();
			Interfaces = NetworkInterface.GetAllNetworkInterfaces();
			Count = Interfaces.Length;
			
			foreach(NetworkInterface Interface in Interfaces)
			{
				if(Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
				IPaddresses.Add(NetInfo.GetIPv4(Interface));
				Subnets.Add(NetInfo.GetSubnetMask(Interface));
			}
		}
		
		public void PrintInterfaces()
		{
			for(int i=0; i<Interfaces.Length; i++)
			{
								
				if(Interfaces[i].NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
				Console.WriteLine("Adapter {0} => {1} => {2} =>{3}",i,Interfaces[i].Description, IPaddresses[i], Subnets[i]);
			}
		}
		
		public void Scan(int index)
		{
			byte[] subnet = IPAddress.Parse(Subnets[index]).GetAddressBytes();
			byte[] ip = IPAddress.Parse(IPaddresses[index]).GetAddressBytes();
			
			int iteration_parallel = 256-subnet[3];
			
			for(int j=0;j<256-subnet[0];j++)
			{
				for(int k=0;k<256-subnet[1];k++)
				{
					for(int l=0;l<256-subnet[2];l++)
					{
						Parallel.For(0,iteration_parallel, h =>
						{
							string ipad = "";
							if(subnet[0] == 255 && subnet[1] == 255 && subnet[2] == 255) 
							{
								ipad = $"{ip[0]}.{ip[1]}.{ip[2]}.{h}";
								startping(ipad);
							}
							else if(subnet[0] == 255 && subnet[1] == 255)
							{
								ipad = $"{ip[0]}.{ip[1]}.{l}.{h}";
								startping(ipad);
								
							}
							else if(subnet[0] == 255)
							{
								ipad = $"{ip[0]}.{k}.{l}.{h}";
								startping(ipad);
								
							}
							//Console.WriteLine(ipad);
						});
					}
				}
			}
		}
		
		private void startping(string ip)
		{
			try{Ping p = new Ping();
			PingReply pr = p.Send(ip,10);
			if(pr.Status == IPStatus.Success)
			{
				string z = "connected to => "+ip+" => ";
				try{Console.WriteLine(z+Dns.GetHostEntry(ip).HostName);}
				catch{ Console.WriteLine(z+"UNKNOW");}
			}}catch{}
		}
		
	}
	
}
