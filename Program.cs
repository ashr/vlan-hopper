using System;
using System.IO;
using System.Diagnostics;
using System.Timers;

namespace vlanhops
{
	class MainClass
	{
		const string networkInterface = "eth0";

		public static void Main (string[] args)
		{
			for (int i = 1; i < 903; i++) {
				if (createVlan (i)) {
					getIP (networkInterface + "." + i);
					removeVlan (i);					
				}
			}
		}

		private static bool createVlan(int vlanID){
			Console.WriteLine ("CREATE VLAN:" + vlanID);
			VLANConfig vlc = new VLANConfig ();
			return (vlc.add ("eth0", vlanID.ToString()));


			ProcessStartInfo psi = new ProcessStartInfo ();
			psi.FileName = "/opt/vlan/vconfig";
			psi.Arguments = "add " + networkInterface + " " + vlanID;

			Process p = CreateProcAndWait (psi, 1);
			Console.WriteLine (p.StandardOutput.ReadToEnd ());
			
			if (p.HasExited) {
				return true;
			}
			return false;
		}

		private static bool removeVlan(int vlanID){
			Console.WriteLine ("REMOVE VLAN:" + vlanID);
			VLANConfig vlc = new VLANConfig ();
			return (vlc.rem ("eth0." + vlanID));

			ProcessStartInfo psi = new ProcessStartInfo ();
			psi.FileName = "/opt/vlan/vconfig";
			psi.Arguments = "rem " + networkInterface + "." + vlanID;

			Process p = CreateProcAndWait (psi, 1);
			if (p.HasExited) {
				return true;
			}
			return false;		
		}

		private static bool getIP(string interfaceName){
			Console.WriteLine ("RUNNING DHCLIENT ON " + interfaceName);
			ProcessStartInfo psi = new ProcessStartInfo ();
			psi.FileName = "/sbin/dhclient";
			psi.Arguments = interfaceName;

			Process p = CreateProcAndWait (psi,10);
			if (p.HasExited) {
				//success
				Console.WriteLine(String.Format("Got IP on interface: {0}",interfaceName));
				getIPForInterfaceName(interfaceName);
				return true;
			}
			Console.WriteLine ("NO IP");
			return false;
		}

		private static void getIPForInterfaceName(string interfaceName){
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = "/sbin/ifconfig";

			Process p = CreateProcAndWait(psi,1);
			if (p.HasExited){
				Console.Write(p.StandardOutput.ReadToEnd());
			}
		}

		private static Process CreateProcAndWait(ProcessStartInfo psi, int sleeps){
			psi.RedirectStandardOutput = true;
			psi.UseShellExecute = false;

			Process p = new Process();
			p.StartInfo = psi;
			p.Start();
			int sleepCounter = 0;

			while (!p.HasExited) {
				System.Threading.Thread.Sleep (1000);
				if (sleepCounter < sleeps) {
					Console.Write ("*");
					sleepCounter++;
				} else {
					break;
				}
			}
			Console.WriteLine ("");
			return p;
		}

	}
}
