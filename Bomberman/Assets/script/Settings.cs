using System.Text;
using System.Collections.Generic;


public class IPManager
{
	public enum Address { ANTHONY, FAYE, JEFFREY, MYSQL, LOCALHOST };

	// Change the public IP Addresses here
	public static Dictionary<Address, string> IPAddresses
		= new Dictionary<Address, string>()
	{
		{ Address.ANTHONY,   "169.234.21.76"  },
		{ Address.FAYE,      "169.234.12.76"  },
		{ Address.JEFFREY,   "169.234.13.110" },
		{ Address.LOCALHOST, "127.0.0.1"      },
	};

	// Whose IP Address are we using for the server?
    public static Address Server = Address.LOCALHOST;

	public static string getServerIP()
	{
		return IPAddresses[Server];
	}
}
