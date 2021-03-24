using Common;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class ATMProxy : ChannelFactory<IATMService>, IATMService, IDisposable
    {
        #region Fields
        IATMService factory = null;
        string username = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);
        #endregion

        #region Methods

        
        public ATMProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            /// cltCertCN.SubjectName should be set to the client's username. .NET WindowsIdentity class provides information about Windows user running the given process
            string cltCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust;
            //this.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator = new ClientCertValidator();
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            /// Set appropriate client's certificate on the channel. Use CertManager class to obtain the certificate based on the "cltCertCN"
            this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);

            factory = this.CreateChannel();
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

        public double PayIn(double amount)
        {
            try
            {
                return factory.PayIn(amount);
            }
            catch(Exception e)
            {
                Console.WriteLine("[PayIn] ERROR = {0}", e.Message);
                return 0;
            }
        }

        public double PayOut(double amount)
        {
            try
            {
                return factory.PayOut(amount);
            }
            catch (Exception e)
            {
                Console.WriteLine("[PayOut] ERROR = {0}", e.Message);
                return 0;
            }
        }
        
        public bool SendPin(SecureString pin)
        {
            try
            {
                string signCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name) + "Sign";

                /// Create a signature based on the "signCertCN"
                X509Certificate2 signCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, signCertCN);
                // hash klijentskog PINa
                byte[] hash = DigitalSignature.CreateHash(new System.Net.NetworkCredential(string.Empty, pin).Password, "SHA1");
                /// Create a signature using SHA1 hash algorithm
                byte[] signature = DigitalSignature.Create(new System.Net.NetworkCredential(string.Empty, pin).Password, "SHA1", signCert);
                return SendPin(signature);
            }
            catch(Exception e)
            {
                Console.WriteLine("[SendPin] ERROR = {0}", e.Message);
                return false;
            }
        }

        public bool SendPin(byte[] signature)
        {
            try
            {
                return factory.SendPin(signature);
            }
            catch (Exception e)
            {
                Console.WriteLine("[SendPin] ERROR = {0}", e.Message);
                return false;
            }
        }

        public void TestCommunicationATM()
        {
            try
            {
                factory.TestCommunicationATM();
            }
            catch(Exception e)
            {
                Console.WriteLine("[TestCommunication] ERROR = {0}", e.Message);
            }
        }
        #endregion
    }
}
