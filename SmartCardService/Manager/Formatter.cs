using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager
{
	public class Formatter
	{
        #region Methods
        
        //ComputerName/Username
        public static string ParseName(string winLogonName)
		{
			string[] parts = new string[] { };

			if (winLogonName.Contains("@"))
			{
				///UPN format
				parts = winLogonName.Split('@');
				return parts[0];
			}
			else if (winLogonName.Contains("\\"))
			{
				/// SPN format
				parts = winLogonName.Split('\\');
				return parts[1];
			}
			else
			{
				return winLogonName;
			}
		}

        //CN=Client,OU=Group
        public static String ParseCNWithOU(String CN)
        {
            return CN.Split(',')[0].Split('=')[1];
        }

        public static String ParseOU(String CN)
        {
            return CN.Split(',')[1].Split('=')[1];
        }

        public static String ParseCNWithoutOU(String CN)
        {
            return CN.Split('=')[1];
        }
        #endregion
    }
}
