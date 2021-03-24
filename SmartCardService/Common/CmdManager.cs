using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class CmdManager
    {
        #region Execute Command in CMD

        public static void ExecuteCommand(String path, String arg)
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WorkingDirectory = path;
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;                
                startInfo.Arguments = @"/c cd " +path+" && " + arg;
                startInfo.FileName = "cmd.exe";
                startInfo.Verb = "runas";
                
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
        }
        #endregion
    }
}
