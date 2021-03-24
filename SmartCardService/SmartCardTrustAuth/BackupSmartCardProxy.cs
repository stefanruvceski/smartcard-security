using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SmartCardTrustAuth
{
    public class BackupSmartCardProxy : ChannelFactory<IBackupService>, IBackupService, IDisposable
    {
        IBackupService factory;

        public BackupSmartCardProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }


        public bool AddATM(string ATM)
        {
            try
            {
                return factory.AddATM(ATM);
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR[AddATM]: "+e.Message);
                return false;
            }
        }

        public bool AddSmartCard(SmartCard smartCard)
        {
            try
            {
                return factory.AddSmartCard(smartCard);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR[AddATM]: " + e.Message);
                return false;
            }
        }

        public bool PayIn(double amount, string thumbprint)
        {
            try
            {
                return factory.PayIn(amount, thumbprint);
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR[PayIn]: "+e.Message);
                return false;
            }
        }

        public bool PayOut(double amount, string thumbprint)
        {
            try
            {
                return factory.PayOut(amount, thumbprint);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR[PayIn]: " + e.Message);
                return false;
            }
        }

        public bool RemoveATM(string ATM)
        {
            try
            {
                return factory.RemoveATM(ATM);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR[AddATM]: " + e.Message);
                return false;
            }
        }

        public bool RemoveSmartCard(SmartCard smartCard)
        {
            try
            {
                return factory.RemoveSmartCard(smartCard);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR[AddATM]: " + e.Message);
                return false;
            }
        }

        public void TestCommunication()
        {
            try
            {
                factory.TestCommunication();
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR[TestCommunication] " + e.Message);
            }
        }
    }
}
