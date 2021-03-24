using Common;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace ATM
{
    class Program
    {
        public static SmartCardProxy SmartCardProxy;
        static void Main(string[] args)
        {
            #region Start
            
            NetTcpBinding ATMBinding = null;
            String ATMAddress = String.Empty;
            ServiceHost ATMHost = null;
            int option=-1;
            string srvCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);
            bool hasCertificate = true;
            #region SmartCardProxy
            NetTcpBinding binding3 = new NetTcpBinding();
            binding3.Security.Mode = SecurityMode.Transport;
            binding3.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding3.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding3.CloseTimeout = TimeSpan.MaxValue;
            binding3.OpenTimeout = TimeSpan.MaxValue;
            binding3.ReceiveTimeout = TimeSpan.MaxValue;
            binding3.SendTimeout = TimeSpan.MaxValue;
            EndpointAddress address3 = new EndpointAddress(new Uri("net.tcp://localhost:8000/SmartCardService"));

            SmartCardProxy = new SmartCardProxy(binding3, address3);
            SmartCardProxy.TestCommunicationATM();
            #endregion
            #endregion
            
            #region Initial Check

            try
            {
                if (Manager.CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, Formatter.ParseName(WindowsIdentity.GetCurrent().Name)) == null)
                {
                    hasCertificate = false;
                }
                else
                {
                    hasCertificate = true;
                    #region ATMService
                    ATMBinding = new NetTcpBinding();
                    ATMBinding.Security.Mode = SecurityMode.Transport;
                    ATMBinding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
                    ATMBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
                    ATMBinding.CloseTimeout = TimeSpan.MaxValue;
                    ATMBinding.OpenTimeout = TimeSpan.MaxValue;
                    ATMBinding.ReceiveTimeout = TimeSpan.MaxValue;
                    ATMBinding.SendTimeout = TimeSpan.MaxValue;
                    ATMAddress = "net.tcp://localhost:9000/ATMService";
                    ATMHost = new ServiceHost(typeof(ATMService));
                    ATMHost.AddServiceEndpoint(typeof(IATMService), ATMBinding, ATMAddress);

                    ///Custom validation mode enables creation of a custom validator - CustomCertificateValidator
                    ATMHost.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;

                    ///If CA doesn't have a CRL associated, WCF blocks every client because it cannot be validated
                    ATMHost.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

                    ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
                    ATMHost.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

                    try
                    {
                        ATMHost.Open();
                        Console.WriteLine($"ATMService is started at {ATMHost.Description.Endpoints[0].Address} .\nPress <enter> to stop ...");

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[ERROR] {0}", e.Message);
                        Console.WriteLine("[StackTrace] {0}", e.StackTrace);
                    }

                    #endregion
                }
            }
            catch
            {
                hasCertificate = false;
            }
            #endregion

            #region ATM UI


            //ATM UI
            while (true)
            {
                Console.Clear();

                //UI for ATM with Certificate
                if (hasCertificate)
                {
                    option = UIManager.ATMHasCertUI();
                }
                //UI for ATM without Certificate
                else
                {
                    option = UIManager.ATMHasntCertUI();
                }

                if(option == 1)
                {
                    // Exit Gracefully
                    if (hasCertificate)
                    {
                        ATMHost.Close();
                        Console.WriteLine("ATM is closed press any key to exit.");
                        Console.ReadKey();
                        break;
                    }
                    //Publish Certificate
                    else
                    {
                        if (SmartCardProxy.PublishATMCertificate())
                        {
                            hasCertificate = true;
                            Console.WriteLine("Certificate published.");
                            Console.WriteLine("Install certificates and press enter");
                            Console.ReadLine();
                            #region ATMService
                            ATMBinding = new NetTcpBinding();
                            ATMBinding.Security.Mode = SecurityMode.Transport;
                            ATMBinding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
                            ATMBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

                            ATMAddress = "net.tcp://localhost:9000/ATMService";
                            ATMHost = new ServiceHost(typeof(ATMService));
                            ATMHost.AddServiceEndpoint(typeof(IATMService), ATMBinding, ATMAddress);

                            ///Custom validation mode enables creation of a custom validator - CustomCertificateValidator
                            ATMHost.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;

                            ///If CA doesn't have a CRL associated, WCF blocks every client because it cannot be validated
                            ATMHost.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

                            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
                            ATMHost.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

                            try
                            {
                                ATMHost.Open();
                                Console.WriteLine($"ATMService is started at {ATMHost.Description.Endpoints[0]} .\nPress <enter> to stop ...");

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("[ERROR] {0}", e.Message);
                                Console.WriteLine("[StackTrace] {0}", e.StackTrace);
                            }

                            #endregion
                            
                        }
                    }
                }
                //Exit Gracefully
                else if(option == 2 && !hasCertificate)
                {
                    Console.WriteLine("ATM is closed press any key to exit.");
                    Console.ReadKey();
                    break;
                }
            }
            #endregion
        }
    }
}
