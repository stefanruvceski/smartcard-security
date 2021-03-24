using Common;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class SmartCardProxy : ChannelFactory<ISmartCardServiceClient>, ISmartCardServiceClient, IDisposable
    {
        #region Fields
        
        ISmartCardServiceClient factory = null;
        string username = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);
        #endregion

        #region Methods
        
        public SmartCardProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        public bool ConfirmPin(SecureString pin)
        {
            try
            {
                X509Certificate2 certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username);
                X509Certificate2 certificateSign = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username + "Sign");

                // hash klijentskog PINa
                byte[] hash = DigitalSignature.CreateHash(new System.Net.NetworkCredential(string.Empty, pin).Password, "SHA1");

                /// Create a signature using SHA1 hash algorithm
                byte[] signature = DigitalSignature.Create(new System.Net.NetworkCredential(string.Empty, pin).Password, "SHA1", certificateSign);

                return ConfirmPin(certificate, certificateSign, signature, hash);
            }
            catch(Exception e)
            {
                Console.WriteLine("[ConfirmPin] ERROR = {0}", e.Message);
                return false;
            }
        }

        public bool ConfirmPin(X509Certificate2 certificate, X509Certificate2 certificateSign, byte[] pin, byte[] hash)
        {
            try
            {
                return factory.ConfirmPin(certificate, certificateSign, pin, hash);
            }
            catch (Exception e)
            {
                Console.WriteLine("[ConfirmPin] ERROR = {0}", e.Message);
                return false;
            }
        }

        public bool PublishNewSmartCard(String userGroup)
        {
            try
            {
                return factory.PublishNewSmartCard(userGroup);
            }
            catch (Exception e)
            {
                Console.WriteLine("[PublishNewSmartCard] ERROR = {0}", e.Message);
                return false;
            }
        }

        public List<String> TestCommunicationClient()
        {
            try
            {
                return factory.TestCommunicationClient();
            }
            catch (Exception e)
            {
                Console.WriteLine("[TestCommunication] ERROR = {0}", e.Message);
                return null;
            }
        }

        public bool ResetPinCode(SecureString pin)
        {
            try
            {
                X509Certificate2 certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username);

                X509Certificate2 certificateSign = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username + "Sign");

                // hash klijentskog PINa
                byte[] hash = DigitalSignature.CreateHash(new System.Net.NetworkCredential(string.Empty, pin).Password, "SHA1");

                /// Create a signature using SHA1 hash algorithm
                byte[] signature = DigitalSignature.Create(new System.Net.NetworkCredential(string.Empty, pin).Password, "SHA1", certificateSign);

                Console.WriteLine("Delete all cert files and press any key");
                Console.ReadKey();

                return ResetPinCode(signature, certificate);
            }
            catch(Exception e)
            {
                Console.WriteLine("[ResetPinCode] ERROR = {0}", e.Message);
                return false;
            }
        }

        public bool ResetPinCode(byte[] pin, X509Certificate2 certificate)
        {
            try
            {
                return factory.ResetPinCode(pin, certificate);
            }
            catch(Exception e)
            {
                Console.WriteLine("[ResetPinCode] ERROR = {0}", e.Message);
                return false;
            }
            
        }

        public bool WithdrawSmartCardClient(SecureString pin)
        {
            try
            {
                X509Certificate2 certificateSign = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username + "Sign");

                // hash klijentskog PINa
                byte[] hash = DigitalSignature.CreateHash(new System.Net.NetworkCredential(string.Empty, pin).Password, "SHA1");

                /// Create a signature using SHA1 hash algorithm
                byte[] signature = DigitalSignature.Create(new System.Net.NetworkCredential(string.Empty, pin).Password, "SHA1", certificateSign);

                return factory.WithdrawSmartCardClient(signature);
            }
            catch(Exception e)
            {
                Console.WriteLine("[WithdrawSmartCardClient] ERROR = {0}", e.Message);
                return false;
            }
        }

        public bool WithdrawSmartCardClient(byte[] pin)
        {
            try
            {
                return factory.WithdrawSmartCardClient(pin);
            }
            catch(Exception e)
            {
                Console.WriteLine("[WithdrawSmartCardClient] ERROR = {0}", e.Message);
                return false;
            }
        }
        #endregion
    }
}
