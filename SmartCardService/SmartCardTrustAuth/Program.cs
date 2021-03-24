using Common;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCardTrustAuth
{
    class Program 
    {
        static DataManager serializer = new DataManager();
        public static BackupSmartCardProxy BackupSmartCardProxy = null;
        static void Main(string[] args)
        {
            #region Start

            try
            {
                serializer.DeSerializeObject<List<SmartCard>>("SmartCardList.xml").ForEach(x => { SmartCardDB.SmartCardList.Add(x.CreateCertificate, x); });
                serializer.DeSerializeObject<List<SmartCard>>("SmartCardRevocationList.xml").ForEach(x => { SmartCardDB.SmartCardRevocationList.Add(x.CreateCertificate, x); });
                serializer.DeSerializeObject<List<String>>("ATMList.xml").ForEach(x => { SmartCardDB.AvailATMs.Add(x); });
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR[Serialization]: " +e.Message);
            }
            Console.WriteLine("Current SmartCard Service admin is " + Formatter.ParseName(WindowsIdentity.GetCurrent().Name));

            #region BackupProxy
            NetTcpBinding binding1 = new NetTcpBinding();
            binding1.Security.Mode = SecurityMode.Transport;
            binding1.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding1.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            binding1.CloseTimeout = TimeSpan.MaxValue;
            binding1.OpenTimeout = TimeSpan.MaxValue;
            binding1.ReceiveTimeout = TimeSpan.MaxValue;
            binding1.SendTimeout = TimeSpan.MaxValue;

            EndpointAddress address1 = new EndpointAddress(new Uri("net.tcp://localhost:7000/BackupSmartCardService"));
            BackupSmartCardProxy = new BackupSmartCardProxy(binding1, address1);
            BackupSmartCardProxy.TestCommunication();
            #endregion

            #region host

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            binding.CloseTimeout = TimeSpan.MaxValue;
            binding.OpenTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.MaxValue;

            string address = "net.tcp://10.1.212.184:8000/SmartCardService";
            ServiceHost host = new ServiceHost(typeof(SmartCardService));
            host.AddServiceEndpoint(typeof(ISmartCardService), binding, address);
            #endregion
            #endregion

            #region UI

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
                    serializer.SerializeObject<List<SmartCard>>(SmartCardDB.SmartCardList.Values.ToList(), "SmartCardList.xml");
                    serializer.SerializeObject<List<SmartCard>>(SmartCardDB.SmartCardRevocationList.Values.ToList(), "SmartCardRevocationList.xml");

                    serializer.SerializeObject<List<String>>(SmartCardDB.AvailATMs, "ATMList.xml");
                }
                catch(Exception e)
                {
                    Console.WriteLine("ERROR[Serialization]: "+e.Message);
                }
                Console.WriteLine("Press any key to close console.");
                Console.ReadKey();
            }

            #endregion
        }
    }
}
