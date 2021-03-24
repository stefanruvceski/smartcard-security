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
    public class Program
    {
        static List<String> AvailATMs = new List<string>();
        static void Main(string[] args)
        {
            #region Start
            Console.WriteLine("Hello " +  Formatter.ParseName(WindowsIdentity.GetCurrent().Name));

            bool hasCertificate = true;
            int option = -1;
            #region SmartCardProxy
            NetTcpBinding binding1 = new NetTcpBinding();
            binding1.Security.Mode = SecurityMode.Transport;
            binding1.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding1.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            binding1.CloseTimeout = TimeSpan.MaxValue;
            binding1.OpenTimeout = TimeSpan.MaxValue;
            binding1.ReceiveTimeout = TimeSpan.MaxValue;
            binding1.SendTimeout = TimeSpan.MaxValue;

            EndpointAddress address1 = new EndpointAddress(new Uri("net.tcp://localhost:8000/SmartCardService"));
            #endregion
            SmartCardProxy SmartCardProxy = new SmartCardProxy(binding1, address1);
            AvailATMs = SmartCardProxy.TestCommunicationClient();
            ATMProxy ATMProxy = null;
            X509Certificate2 certificate = null;
            #endregion

            #region Initial check
            
            //Checking for users certificate
            try
            {
                if ((certificate = Manager.CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, Formatter.ParseName(WindowsIdentity.GetCurrent().Name))) == null)
                {
                    hasCertificate = false;
                }
                else
                {
                    String ATM = Program.ClientATMUI();
                    #region ATMProxy
                    NetTcpBinding binding2 = new NetTcpBinding();
                    binding2.Security.Mode = SecurityMode.Transport;
                    binding2.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
                    binding2.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
                    binding2.CloseTimeout = TimeSpan.MaxValue;
                    binding2.OpenTimeout = TimeSpan.MaxValue;
                    binding2.ReceiveTimeout = TimeSpan.MaxValue;
                    binding2.SendTimeout = TimeSpan.MaxValue;
                    /// Use CertManager class to obtain the certificate based on the "srvCertCN" representing the expected service identity.
                    X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, ATM);
                    EndpointAddress address2 = new EndpointAddress(new Uri("net.tcp://localhost:9000/ATMService"), new X509CertificateEndpointIdentity(srvCert));

                    ATMProxy = new ATMProxy(binding2, address2);
                    ATMProxy.TestCommunicationATM();
                    #endregion

                }
            }
            catch
            {
                hasCertificate = false;
            }
            Console.WriteLine("Press any key for menu.");
            Console.ReadKey();
            #endregion

            #region UI
            //Users UI
            while (true)
            {
                Console.Clear();
                //UI for users with certificate
                if (hasCertificate)
                {
                    string group = Formatter.ParseOU(certificate.Subject);

                    if (group.Equals("SmartCardUser"))
                    {
                        option = UIManager.ClientHasCertUI();
                    }
                    else if(group.Equals("Manager"))
                    {
                        option = UIManager.ManagerHasCertUI();
                    }
                }
                //UI for users without certificate
                else
                {
                    option = UIManager.UserHasntCertUI();
                }

                //Switch for users commands
                switch (option)
                {
                    case 0:
                        //List All users (Only for Manager)
                        {
                            ATMProxy.ListAllValidUsers().ForEach(user => {
                                Console.WriteLine(user);
                            });
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }
                        break;
                    case 1:
                        {
                            // PayIn
                            if (hasCertificate)
                            {
                                bool valid = false;
                                int invalidCnt = 0;

                                do
                                {
                                    if (valid = ATMProxy.SendPin(UIManager.EnterPassword()))
                                    {
                                        double amount = -1;
                                        do
                                        {
                                            Console.WriteLine("Enter amount");
                                            try
                                            {
                                                amount = double.Parse(Console.ReadLine());
                                            }
                                            catch
                                            {
                                                amount = -1;
                                            }
                                        } while (amount == -1);
                                        Console.WriteLine("Your balance: " + ATMProxy.PayIn(amount));
                                        Console.WriteLine("Press any key to continue.");
                                        Console.ReadKey();
                                    }
                                    else
                                    {
                                        invalidCnt++;
                                    }
                                } while (!valid && invalidCnt < 3);

                                if (!valid)
                                {
                                    Console.WriteLine("Pay in unsuccess.");
                                    Console.WriteLine("Card has been blocked.");
                                    Console.WriteLine("Press any key to exit.");
                                    Console.ReadKey();
                                    option = 5;
                                }
                            }
                            // Publish New Smartcard
                            else
                            {
                                
                                int op = -1;
                                List<string> l = new List<string>() { "SmartCardUser", "Manager" };
                                do
                                {
                                    Console.WriteLine("Type of User:");
                                    Console.WriteLine("1. SmartCardUser");
                                    Console.WriteLine("2. Manager");
                                    try
                                    {
                                        op = int.Parse(Console.ReadLine());
                                    }
                                    catch
                                    {
                                        op = -1;
                                    }
                                } while (op == -1);
                                if (SmartCardProxy.PublishNewSmartCard(l[op-1]))
                                {
                                    Console.WriteLine("SmartCard published.");
                                    Console.ReadKey();

                                    if (SmartCardProxy.ConfirmPin(UIManager.EnterPassword()))
                                    {
                                        Console.WriteLine("Pin confirmed.");
                                        Console.ReadKey();
                                        String ATM = Program.ClientATMUI();
                                        #region ATMProxy
                                        NetTcpBinding binding2 = new NetTcpBinding();
                                        binding2.Security.Mode = SecurityMode.Transport;
                                        binding2.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
                                        binding2.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
                                        binding2.CloseTimeout = TimeSpan.MaxValue;
                                        binding2.OpenTimeout = TimeSpan.MaxValue;
                                        binding2.ReceiveTimeout = TimeSpan.MaxValue;
                                        binding2.SendTimeout = TimeSpan.MaxValue;
                                        /// Use CertManager class to obtain the certificate based on the "srvCertCN" representing the expected service identity.
                                        X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, ATM);
                                        EndpointAddress address2 = new EndpointAddress(new Uri("net.tcp://localhost:9000/ATMService"), new X509CertificateEndpointIdentity(srvCert));

                                        ATMProxy = new ATMProxy(binding2, address2);
                                        certificate = Manager.CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, Formatter.ParseName(WindowsIdentity.GetCurrent().Name));
                                        ATMProxy.TestCommunicationATM();
                                        #endregion

                                        hasCertificate = true;
                                    }
                                }
                            }
                        }
                        break;
                    case 2:
                        {
                            //PayOut
                            if (hasCertificate)
                            {
                                bool valid = false;
                                int invalidCnt = 0;

                                do
                                {
                                    if (valid = ATMProxy.SendPin(UIManager.EnterPassword()))
                                    {
                                        double amount = -1;
                                        do
                                        {
                                            Console.WriteLine("Enter amount");
                                            try
                                            {
                                                amount = double.Parse(Console.ReadLine());
                                            }
                                            catch
                                            {
                                                amount = -1;
                                            }
                                        } while (amount == -1);
                                        Console.WriteLine("Your balance: " + ATMProxy.PayOut(amount));
                                        Console.WriteLine("Press any key to continue.");
                                        Console.ReadKey();
                                    }
                                    else
                                    {
                                        invalidCnt++;
                                    }
                                } while (!valid && invalidCnt < 3);

                                if (!valid)
                                {
                                    Console.WriteLine("PayOut unsuccess.");
                                    Console.WriteLine("Card has been blocked.");
                                    Console.WriteLine("Press any key to exit.");
                                    Console.ReadKey();
                                    option = 5;
                                }
                            }
                            //Goodbye message for user without certificate
                            else
                            {
                                Console.WriteLine("Goodbye.");
                                Console.WriteLine("Press any key to exit.");
                                Console.ReadKey();
                                option = 5;
                            }
                        }
                        break;
                    case 3:
                        //Withdraw Smartcard
                        {
                            int invalidCnt = 0;
                            bool valid = false;
                            do
                            {
                                if ((valid = SmartCardProxy.WithdrawSmartCardClient(UIManager.EnterPassword())))
                                {
                                    Console.WriteLine("SmartCard has been withdrawn.");
                                    hasCertificate = false;
                                }
                                else
                                {
                                    invalidCnt++;
                                }
                            } while (!valid && invalidCnt <3);
                            if (valid)
                            {
                                Console.WriteLine("Press any key to continue.");
                                Console.ReadKey();
                            }
                            else
                            {
                                Console.WriteLine("Withdraw unsuccess.");
                                Console.WriteLine("Card has been blocked.");
                                Console.WriteLine("Press any key to exit.");
                                Console.ReadKey();
                                option = 5;
                            }
                            
                        }
                        break;
                    case 4:
                        //Reset Pincode
                        {
                            int invalidCnt = 0;
                            bool valid = false;
                            do
                            {
                                if (SmartCardProxy.ResetPinCode(UIManager.EnterPassword()))
                                {
                                    Console.WriteLine("Reset pin success.");
                                    Console.WriteLine("Press any key to continue.");
                                    Console.ReadKey();
                                    Console.WriteLine("Enter new password to confirm new certificate.");


                                    if (SmartCardProxy.ConfirmPin(UIManager.EnterPassword()))
                                    {
                                        Console.WriteLine("Pin confirmed.");
                                        #region ATMProxy
                                        NetTcpBinding binding2 = new NetTcpBinding();
                                        binding2.Security.Mode = SecurityMode.Transport;
                                        binding2.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
                                        binding2.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

                                        /// Use CertManager class to obtain the certificate based on the "srvCertCN" representing the expected service identity.
                                        X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, "ATMUser");
                                        EndpointAddress address2 = new EndpointAddress(new Uri("net.tcp://localhost:9000/ATMService"), new X509CertificateEndpointIdentity(srvCert));

                                        ATMProxy = new ATMProxy(binding2, address2);

                                        #endregion

                                        hasCertificate = true;
                                        break;
                                    }

                                    hasCertificate = false;
                                }
                                else
                                {
                                    invalidCnt++;
                                }
                            } while (!valid && invalidCnt < 3);
                        }
                        break;
                    case 5:
                        //Goodbye message for user with certificate
                        {
                            Console.WriteLine("Goodbye.");
                            Console.WriteLine("Press any key to exit.");
                            Console.ReadKey();
                        }
                        break;
                    default:
                        break;
                }

                //Exit Gracefully
                if (option.Equals(5))
                    break;
            }
            #endregion
        }

        #region Helper

        
        static string ClientATMUI()
        {
            int op = -1;
            do
            {
                Console.WriteLine("-----------------");
                Console.WriteLine("All available ATMs\n");

                for (int i = 0; i < AvailATMs.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {AvailATMs[i]}");
                }
                Console.WriteLine("\nEnter number of ATM you want.");

                try
                {
                    op = int.Parse(Console.ReadLine());
                }
                catch (Exception)
                {
                    op = -1;
                }
            } while (op < 1 || op > AvailATMs.Count());


            return AvailATMs[op - 1];
        }
        #endregion
    }
}
