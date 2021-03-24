using Common;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ATM
{
    public class SmartCardProxy : ChannelFactory<ISmartCardServiceATM>, ISmartCardServiceATM, IDisposable
    {
        ISmartCardServiceATM factory;
        string username = WindowsPrincipal.Current.Identity.Name;

        public SmartCardProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        public bool CheckCertificateAndPin(X509Certificate2 certificate, byte[] pin)
        {
            try
            {
                return factory.CheckCertificateAndPin(certificate, pin);
            }
            catch (Exception e)
            {
                Console.WriteLine("[CheckCertificateAndPin] ERROR = {0}", e.Message);
                return false;
            }
        } 

        public List<string> ListAllValidUsers()
        {
            try
            {
                return factory.ListAllValidUsers();
            }
            catch (Exception e)
            {
                Console.WriteLine("[ListAllValidUsers] ERROR = {0}", e.Message);
                return null;
            }
        }

        public double PayIn(double amount, X509Certificate2 certificate)
        {
            try
            {
                return factory.PayIn(amount,certificate);
            }
            catch (Exception e)
            {
                Console.WriteLine("[PayIn] ERROR = {0}", e.Message);
                return 0;
            }
        }

        public double PayOut(double amount, X509Certificate2 certificate)
        {
            try
            {
                return factory.PayOut(amount, certificate);
            }
            catch (Exception e)
            {
                Console.WriteLine("[PayOut] ERROR = {0}", e.Message);
                return 0;
            }
        }

        public bool PublishATMCertificate()
        {
            try
            {
                return factory.PublishATMCertificate();
            }
            catch (Exception e)
            {
                Console.WriteLine("[PublishATMCertificate] ERROR = {0}", e.Message);
                return false;
            }
        }

        public void TestCommunicationATM()
        {
            try
            {
                Console.WriteLine(WindowsIdentity.GetCurrent().Name.Split('\\')[1]);
                factory.TestCommunicationATM();
            }
            catch (Exception e)
            {
                Console.WriteLine("[TestCommunication] ERROR = {0}", e.Message);
            }
        }
    }
}
