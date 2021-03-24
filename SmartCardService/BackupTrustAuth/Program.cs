using Common;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BackupTrustAuth
{
    class Program
    {
        static DataManager serializer = new DataManager();
        static void Main(string[] args)
        {
            try
            {
                serializer.DeSerializeObject<List<SmartCard>>("BackupSmartCardList.xml").ForEach(x => { BackupDB.SmartCardList.Add(x.CreateCertificate, x); });
                serializer.DeSerializeObject<List<SmartCard>>("BackupSmartCardRevocationList.xml").ForEach(x => { BackupDB.SmartCardRevocationList.Add(x.CreateCertificate, x); });
                serializer.DeSerializeObject<List<String>>("BackupATMList.xml").ForEach(x => { BackupDB.AvailATMs.Add(x); });
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR[Serializer]: "+ e.Message);
            }
            
            
            #region host

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            binding.CloseTimeout = TimeSpan.MaxValue;
            binding.OpenTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.MaxValue;

            string address = "net.tcp://localhost:7" +
                "000/BackupSmartCardService";
            ServiceHost host = new ServiceHost(typeof(BackupService));
            host.AddServiceEndpoint(typeof(IBackupService), binding, address);
            #endregion

            try
            {
                host.Open();
                do
                {
                    Console.WriteLine("SmartCardService is started.\nPress 'X' to exit.");

                } while (Console.ReadLine().Equals("x"));
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] {0}", e.Message);
                Console.WriteLine("[StackTrace] {0}", e.StackTrace);
            }
            finally
            {
                host.Close();
                Console.WriteLine("SmartCard Service host is closed.");
                try
                {
                    serializer.SerializeObject<List<SmartCard>>(BackupDB.SmartCardList.Values.ToList(), "BackupSmartCardList.xml");
                    serializer.SerializeObject<List<SmartCard>>(BackupDB.SmartCardRevocationList.Values.ToList(), "BackupSmartCardRevocationList.xml");

                    serializer.SerializeObject<List<String>>(BackupDB.AvailATMs, "BackupATMList.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR[Serialization]: " + e.Message);
                }
                Console.WriteLine("Press any key to close console.");
                Console.ReadKey();
            }
        }
    }
}
