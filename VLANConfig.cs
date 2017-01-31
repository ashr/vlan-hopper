using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace vlanhops 
{

	/* Passed in vlan_ioctl_args structure to determine behaviour. */
	enum vlan_ioctl_cmds {
		ADD_VLAN_CMD,
		DEL_VLAN_CMD,
		SET_VLAN_INGRESS_PRIORITY_CMD,
		SET_VLAN_EGRESS_PRIORITY_CMD,
		GET_VLAN_INGRESS_PRIORITY_CMD,
		GET_VLAN_EGRESS_PRIORITY_CMD,
		SET_VLAN_NAME_TYPE_CMD,
		SET_VLAN_FLAG_CMD,
		GET_VLAN_REALDEV_NAME_CMD, /* If this works, you know it's a VLAN device, btw */
		GET_VLAN_VID_CMD /* Get the VID of this VLAN (specified by name) */
	};

	enum vlan_flags {
		VLAN_FLAG_REORDER_HDR	= 0x1,
		VLAN_FLAG_GVRP		= 0x2,
		VLAN_FLAG_LOOSE_BINDING	= 0x4,
		VLAN_FLAG_MVRP		= 0x8,
	};

	enum vlan_name_types {
		VLAN_NAME_TYPE_PLUS_VID, /* Name will look like:  vlan0005 */
		VLAN_NAME_TYPE_RAW_PLUS_VID, /* name will look like:  eth1.0005 */
		VLAN_NAME_TYPE_PLUS_VID_NO_PAD, /* Name will look like:  vlan5 */
		VLAN_NAME_TYPE_RAW_PLUS_VID_NO_PAD, /* Name will look like:  eth0.5 */
		VLAN_NAME_TYPE_HIGHEST
	};


	[StructLayout(LayoutKind.Sequential,Pack =1)]
	//[StructLayout(LayoutKind.Explicit)]
	struct vlan_ioctl_args {
		//[FieldOffset( <- for use with LayoutKind.Explicit
		//[MarshalAs(UnmanagedType.I4)]
		//[FieldOffset(0)]
		public int cmd; /* Should be one of the vlan_ioctl_cmds enum above. */
		//[FieldOffset(4)]
		[MarshalAs(UnmanagedType.ByValArray,SizeConst= 24)]
		public char[] device1;// = new char[24];
		//public char[] device2;//= new char[24];
		//[MarshalAs(UnmanagedType.I4)]
		//[FieldOffset(28)]
		public int VID;
		//public uint skb_priority;
		//public uint name_type;
		//public uint bind_type;
		//public uint flag; /* Matches vlan_dev_priv flags */
		//public short vlan_qos;
	};

	
	public class VLANConfig
	{
		const int SIOCSIFVLAN = 0x8983; 
		[DllImport("libc",EntryPoint="ioctl",CharSet=CharSet.Ansi)]
		static extern int ioctl (int __fd, int dunnoWTF, ref vlan_ioctl_args __request);


		[DllImport("libc",EntryPoint="socket")]
		static extern int socket (int domain, int type, int protocol);
			
		public VLANConfig ()
		{
		}

		public unsafe bool add(string interfaceName, string vlanid){
			vlan_ioctl_args ifrequest = new vlan_ioctl_args ();
			ifrequest.cmd = (int)vlan_ioctl_cmds.ADD_VLAN_CMD;
			ifrequest.device1 = new char[24];
			//ifrequest.device1 = (interfaceName/*+"\0"*/).ToCharArray() ;
			char[] interfaceNameCharArray = interfaceName.ToCharArray();
			Array.Copy (interfaceNameCharArray, ifrequest.device1, interfaceNameCharArray.Length);
			ifrequest.VID = int.Parse (vlanid);
			//AF_INET = 1, SOCK_STREAM = 1
			int fd = socket(1,1,1); //UnixAddressFamily.AF_INET,UnixSocketType.SOCK_STREAM,  UnixSocketProtocol.SOL_SOCKET);
			if (fd == -1)
				throw new Exception ("Fuckup couldnt open socket");

			//IntPtr requestPointer = Marshal.AllocHGlobal(Marshal.SizeOf(ifrequest));
			//Marshal.StructureToPtr (ifrequest, requestPointer, true);

			//if (ioctl (fd, SIOCSIFVLAN,requestPointer) < 0) {
			if (ioctl (fd, SIOCSIFVLAN, ref ifrequest) < 0) {
				Console.WriteLine ("Couldn't add vlan:" + vlanid + " to interface " + interfaceName);
				return false;
			} else {
				Console.WriteLine ("Added VLAN " + interfaceName + "." + vlanid);
				return true;
			}
		}

		public bool rem(string interfaceName){
			vlan_ioctl_args ifrequest = new vlan_ioctl_args ();
			ifrequest.cmd = (int)vlan_ioctl_cmds.DEL_VLAN_CMD;
			ifrequest.device1 = interfaceName.ToCharArray();
			//ifrequest.VID = int.Parse (vlanid);

			int fd = socket (1, 1, 1);//UnixAddressFamily.AF_INET,UnixSocketType.SOCK_STREAM,  UnixSocketProtocol.SOL_SOCKET);
			if (fd == -1)
				throw new Exception ("Fuckup couldnt open socket");

			//IntPtr requestPointer = new IntPtr ();
			//Marshal.StructureToPtr (ifrequest, requestPointer, true);

			if (ioctl (fd, SIOCSIFVLAN, ref ifrequest) < 0) {
				Console.WriteLine ("Couldn't rem vlan:" + interfaceName);
				return false;
			} else {
				Console.WriteLine ("Removed VLAN " + interfaceName);
				return true;
			}
		}
	}
}

