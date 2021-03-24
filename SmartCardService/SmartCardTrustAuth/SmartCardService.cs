using Common;
using Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCardTrustAuth
{
    public class SmartCardService : ISmartCardService
    {
        #region Methods

        public bool CheckCertificateAndPin(X509Certificate2 certificate, byte[] pin)
        {
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();
            if (Thread.CurrentPrincipal.IsInRole("ATMGroup"))
            {
                if (SmartCardDB.SmartCardList.ContainsKey(certificate.Thumbprint))
                {
                    for (int i = 0; i < pin.Length; i++)
                    {
                        if (pin[i] != SmartCardDB.SmartCardList[certificate.Thumbprint].PinCode[i])
                        {
                            scsLogger.WriteError($"Client {SmartCardDB.SmartCardList[certificate.Thumbprint].Username} entered wrong PIN code.");

                            // povlacenje kartice iz upotrebe ako tri puta pogresi PIN
                            if (++SmartCardDB.SmartCardList[certificate.Thumbprint].InvalidCnt >= 3)
                            {
                                Console.WriteLine($"Client {SmartCardDB.SmartCardList[certificate.Thumbprint].Username} entered wrong PIN code three times. His smart card is withdrawn.");
                                WithdrawSmartCardATM(SmartCardDB.SmartCardList[certificate.Thumbprint].Username);
                            }
                            return false;
                        }
                    }

                    // kad pogodi PIN, a nije promasio tri puta za redom, brojac se resetuje
                    SmartCardDB.SmartCardList[certificate.Thumbprint].InvalidCnt = 0;

                    scsLogger.WriteInformation($"Client {SmartCardDB.SmartCardList[certificate.Thumbprint].Username} successfully entered his PIN code.");
                    return true;
                }
            }
            else
            {
                scsLogger.WriteInformation($"Client not valid.");
                return false;
            }

            return false;
        }

        public bool ConfirmPin(X509Certificate2 certificate, X509Certificate2 certificateSign, byte[] sign, byte[] hash)
        {
            if (Thread.CurrentPrincipal.IsInRole("SmartCardUser"))
            {
                RSACryptoServiceProvider csp = (RSACryptoServiceProvider)certificateSign.PublicKey.Key;
                bool verified = csp.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), sign);
                if (verified)
                {
                    SmartCardDB.SmartCardList.Add(certificate.Thumbprint, new SmartCard(certificate.Thumbprint, certificateSign.Thumbprint, sign, Formatter.ParseCNWithOU(certificate.Subject)) { Amount = amount });
                    Program.BackupSmartCardProxy.AddSmartCard(new SmartCard(certificate.Thumbprint, certificateSign.Thumbprint, sign, Formatter.ParseCNWithOU(certificate.Subject)) { Amount = amount });
                    return true;
                }
            }
            return false;
        }

        public List<String> ListAllValidUsers()
        {
            if (Thread.CurrentPrincipal.IsInRole("ATMGroup"))
            {
                List<String> retVal = new List<String>();

                SmartCardDB.SmartCardList.Values.ToList().ForEach(x => retVal.Add(x.Username));

                return retVal;
            }
            else
            {
                return null;
            }
        }

        public double PayIn(double amount, X509Certificate2 certificate)
        {
            AtmServiceLogger atmLogger = new AtmServiceLogger();
            if (Thread.CurrentPrincipal.IsInRole("ATMGroup"))
            {
                if (SmartCardDB.SmartCardList.ContainsKey(certificate.Thumbprint))
                {
                    // logovanje uplate na racun           
                    atmLogger.WriteInformation($"Client {certificate.Subject.Split('=')[1]} has successfully paid in {amount} RSD.");

                    SmartCardDB.SmartCardList[certificate.Thumbprint].Amount += amount;
                    Program.BackupSmartCardProxy.PayIn(amount, certificate.Thumbprint);
                    return SmartCardDB.SmartCardList[certificate.Thumbprint].Amount;
                }
                else
                {
                    // logovanje pokusaja uplate
                    atmLogger.WriteError($"Client {Formatter.ParseCNWithOU(certificate.Subject)} isn't user of this ATM service.");
                    throw new Exception($"Client {Formatter.ParseCNWithOU(certificate.Subject)} isn't user of this ATM service.");
                }
            }
            atmLogger.WriteError($"ATM not valid.");
            throw new Exception($"ATM not valid.");
        }

        public double PayOut(double amount, X509Certificate2 certificate)
        {
            AtmServiceLogger atmLogger = new AtmServiceLogger();
            if (Thread.CurrentPrincipal.IsInRole("ATMGroup"))
            {
                if (SmartCardDB.SmartCardList.ContainsKey(certificate.Thumbprint))
                {
                    if (SmartCardDB.SmartCardList[certificate.Thumbprint].Amount >= amount)
                    {
                        // logovanje uspesne isplate sa racuna
                        atmLogger.WriteInformation($"Client {Formatter.ParseCNWithOU(certificate.Subject)} has successfully paid out {amount} RSD.");

                        SmartCardDB.SmartCardList[certificate.Thumbprint].Amount -= amount;
                        Program.BackupSmartCardProxy.PayOut(amount, certificate.Thumbprint);
                        return SmartCardDB.SmartCardList[certificate.Thumbprint].Amount;
                    }
                    else
                    {
                        atmLogger.WriteError($"Client {Formatter.ParseCNWithOU(certificate.Subject)} try to pay out {amount} RSD, but he hasn't got enough money on balance.");
                        throw new Exception($"Client {Formatter.ParseCNWithOU(certificate.Subject)} try to pay out {amount} RSD, but he hasn't got enough money on balance.");
                    }
                }
                else
                {
                    atmLogger.WriteError($"Client {Formatter.ParseCNWithOU(certificate.Subject)} isn't user of this ATM service.");
                    throw new Exception($"Client {Formatter.ParseCNWithOU(certificate.Subject)} isn't user of this ATM service.");
                }
            }
            atmLogger.WriteError($"ATM not valid.");
            throw new Exception($"ATM not valid.");
        }

        public bool PublishATMCertificate()
        {
            string username = Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
            if (Thread.CurrentPrincipal.IsInRole("ATMGroup"))
            {
                // generisanje pin koda za klijenta
                Random rand = new Random();
                string password = (rand.Next(1000, 9999)).ToString();
                SecureString pin = new NetworkCredential("", password).SecurePassword;

                string CA = "SmartCardCA";

                Console.WriteLine($"Generated password is: {password}. Please use it for registration.\nPress any key to continue");

                // klijentski sertifikat
                //string path = @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.17134.0\x86";
                string path = @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.16299.0\x86";

                string cmd = $"makecert -sv {username}.pvk -iv {CA}.pvk -n \"CN={username}\" -pe -ic {CA}.cer {username}.cer -sr localmachine -ss My -sky exchange";
                cmd = cmd.Replace(@"\", "");
                CmdManager.ExecuteCommand(path, cmd);
                CmdManager.ExecuteCommand(path, $"pvk2pfx.exe /pvk {username}.pvk /pi {password} /spc {username}.cer /pfx {username}.pfx");
                //CmdManager.ExecuteCommand(path, $"CertMgr.exe /add {username}.cer /s /r localmachine personal");   // DA LI TREBA. ovo personal treba menjati TRUSTED PEOPLE KAD BUDE NA VISE KOMPOVA

                // klijentski sertifikat za potpis
                CmdManager.ExecuteCommand(path, $"makecert -sv {username}Sign.pvk -iv {CA}.pvk -n \"CN = {username}Sign\" -pe -ic {CA}.cer {username}Sign.cer -sr localmachine -ss My -sky signature");
                CmdManager.ExecuteCommand(path, $"pvk2pfx.exe /pvk {username}Sign.pvk /pi {password} /spc {username}Sign.cer /pfx {username}Sign.pfx");
                //CmdManager.ExecuteCommand(path, $"CertMgr.exe /add {username}Sign.cer /s /r localmachine personal");   // DA LI TREBA. ovo personal treba menjati

                path = @"C:\Program Files (x86)\Windows Resource Kits\Tools";
                CmdManager.ExecuteCommand(path, $@"winhttpcertcfg -g -c LOCAL_MACHINE\My -s {username} -a {username}");
                CmdManager.ExecuteCommand(path, $@"winhttpcertcfg -g -c LOCAL_MACHINE\My -s {username}Sign -a {username}");

                // logovanje uspesnog izdavanja nove smart kartice
                SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();
                scsLogger.WriteInformation($"SCS has successfully published new smart card for client {username}.");

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool PublishNewSmartCard(String userGroup)
        {
            string username = Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
            if (Thread.CurrentPrincipal.IsInRole("SmartCardUser"))
            {
                // generisanje pin koda za klijenta
                Random rand = new Random();
                string password = (rand.Next(1000, 9999)).ToString();
                SecureString pin = new NetworkCredential("", password).SecurePassword;

                string CA = "SmartCardCA";

                Console.WriteLine($"Generated password is: {password}. Please use it for registration.\nPress any key to continue");

                // klijentski sertifikat
                //string path = @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.17134.0\x86";
                string path = @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.16299.0\x86";

                string cmd = $"makecert -sv {username}.pvk -iv {CA}.pvk -n \"CN={username},OU={userGroup}\" -pe -ic {CA}.cer {username}.cer -sr localmachine -ss My -sky exchange";
                cmd = cmd.Replace(@"\", "");
                CmdManager.ExecuteCommand(path, cmd);
                CmdManager.ExecuteCommand(path, $"pvk2pfx.exe /pvk {username}.pvk /pi {password} /spc {username}.cer /pfx {username}.pfx");
                //CmdManager.ExecuteCommand(path, $"CertMgr.exe /add {username}.cer /s /r localmachine personal");   // DA LI TREBA. ovo personal treba menjati TRUSTED PEOPLE KAD BUDE NA VISE KOMPOVA

                // klijentski sertifikat za potpis
                CmdManager.ExecuteCommand(path, $"makecert -sv {username}Sign.pvk -iv {CA}.pvk -n \"CN={username}Sign,OU={userGroup}\" -pe -ic {CA}.cer {username}Sign.cer -sr localmachine -ss My -sky signature");
                CmdManager.ExecuteCommand(path, $"pvk2pfx.exe /pvk {username}Sign.pvk /pi {password} /spc {username}Sign.cer /pfx {username}Sign.pfx");
                //CmdManager.ExecuteCommand(path, $"CertMgr.exe /add {username}Sign.cer /s /r localmachine personal");   // DA LI TREBA. ovo personal treba menjati

                // logovanje uspesnog izdavanja nove smart kartice
                SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();
                scsLogger.WriteInformation($"SCS has successfully published new smart card for client {username}.");

                return true;
            }
            else
            {
                return false;
            }
        }
        #region TestComm    
        double amount;
        #endregion
        public bool ResetPinCode(byte[] pin, X509Certificate2 certificate)
        {
            // ovde ce isto trebati logovanje scsLogger-om
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();
            string username = SmartCardDB.SmartCardList[certificate.Thumbprint].Username;
            if (Thread.CurrentPrincipal.IsInRole("SmartCardUser"))
            {
                amount = SmartCardDB.SmartCardList[certificate.Thumbprint].Amount;
                if (WithdrawSmartCardClient(pin, certificate.Thumbprint))
                {

                    scsLogger.WriteInformation($"Client {username} successfully reset PIN code.");

                    String userGroup = Formatter.ParseOU(certificate.Subject);
                    Console.WriteLine("Delete old certificate and press any key to continue.");
                    return PublishNewSmartCard(userGroup);
                }

                scsLogger.WriteInformation($"Client {username} unsuccessfully reset PIN code.");
            }
            else
            {
                scsLogger.WriteInformation($"Client {username} unsuccessfully reset PIN code.");
            }

            return false;
        }

        public void TestCommunicationATM()
        {
            if (Thread.CurrentPrincipal.IsInRole("ATMGroup"))
            {
                Console.WriteLine("ATM name is " + Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name));
                Console.WriteLine("TestCommunication success.");
                if (!SmartCardDB.AvailATMs.Contains(Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name)))
                {
                    SmartCardDB.AvailATMs.Add(Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name));
                    Program.BackupSmartCardProxy.AddATM(Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name));
                }
            }
 
        }

        public List<String> TestCommunicationClient()
        {
            if (Thread.CurrentPrincipal.IsInRole("SmartCardUser"))
            {
                Console.WriteLine("Client username is " + Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name));
                Console.WriteLine("TestCommunication success.");

                return SmartCardDB.AvailATMs;
            }
            else
            {
                return null;
            }
        }

        public bool WithdrawSmartCardATM(String clientUsername)
        {
            X509Certificate2 clientCertificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, clientUsername);
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();
            if (Thread.CurrentPrincipal.IsInRole("ATMGroup"))
            {
                SmartCardDB.SmartCardRevocationList.Add(clientCertificate.Thumbprint, SmartCardDB.SmartCardList[clientCertificate.Thumbprint]);
                SmartCardDB.SmartCardList.Remove(clientCertificate.Thumbprint);

                // logovanje uspesnog povlacenja kartice
                scsLogger.WriteInformation($"Successful withdrawal of {clientUsername}'s smart card.");

                return true;
            }
            scsLogger.WriteInformation($"Unsuccessful withdrawal of {clientUsername}'s smart card.");

            return false;
        }

        public bool WithdrawSmartCardClient(byte[] pin)
        {
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();
            X509Certificate2 clientCertificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name));

            if (Thread.CurrentPrincipal.IsInRole("SmartCardUser"))
            {
                
                if (SmartCardDB.SmartCardList.ContainsKey(clientCertificate.Thumbprint))
                {
                    for (int i = 0; i < pin.Length; i++)
                    {
                        if (pin[i] != SmartCardDB.SmartCardList[clientCertificate.Thumbprint].PinCode[i])
                        {
                            // logovanje neuspesnog povlacenja kartice
                            scsLogger.WriteError($"Unsuccessful withdrawal of {Formatter.ParseCNWithOU(clientCertificate.Subject)}'s smart card.");
                            SmartCardDB.SmartCardList[clientCertificate.Thumbprint].InvalidCnt++;

                            if (SmartCardDB.SmartCardList[clientCertificate.Thumbprint].InvalidCnt == 3)
                            {
                                WithdrawSmartCardATM(Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name));
                            }
                            return false;
                        }
                        else
                        {
                            SmartCardDB.SmartCardList[clientCertificate.Thumbprint].InvalidCnt = 0;
                        }
                    }
                }
            }
            else
            {
                scsLogger.WriteError($"Wrong role. Unsuccessful withdrawal of {Formatter.ParseCNWithOU(clientCertificate.Subject)}'s smart card.");
                return false;
            }

            SmartCardDB.SmartCardRevocationList.Add(clientCertificate.Thumbprint, SmartCardDB.SmartCardList[clientCertificate.Thumbprint]);
            Program.BackupSmartCardProxy.RemoveSmartCard(SmartCardDB.SmartCardList[clientCertificate.Thumbprint]);
            SmartCardDB.SmartCardList.Remove(clientCertificate.Thumbprint);
            

            // logovanje uspesnog povlacenja kartice
            scsLogger.WriteInformation($"Successful withdrawal of {Formatter.ParseCNWithOU(clientCertificate.Subject)}'s smart card.");

            return true;
        }

        public bool WithdrawSmartCardClient(byte[] pin, String thumbprint)
        {
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();
            
            if (SmartCardDB.SmartCardList.ContainsKey(thumbprint))
            {
                for (int i = 0; i < pin.Length; i++)
                {
                    if (pin[i] != SmartCardDB.SmartCardList[thumbprint].PinCode[i])
                    {
                        // logovanje neuspesnog povlacenja kartice
                        scsLogger.WriteError($"Unsuccessful withdrawal of {SmartCardDB.SmartCardList[thumbprint].Username}'s smart card.");
                        SmartCardDB.SmartCardList[thumbprint].InvalidCnt++;

                        if (SmartCardDB.SmartCardList[thumbprint].InvalidCnt == 3)
                        {
                            WithdrawSmartCardATM(Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name));
                        }
                        return false;
                    }
                    else
                    {
                        SmartCardDB.SmartCardList[thumbprint].InvalidCnt = 0;
                    }
                }
            }

            // logovanje uspesnog povlacenja kartice
            scsLogger.WriteInformation($"Successful withdrawal of {SmartCardDB.SmartCardList[thumbprint].Username}'s smart card.");

            SmartCardDB.SmartCardRevocationList.Add(thumbprint, SmartCardDB.SmartCardList[thumbprint]);
            Program.BackupSmartCardProxy.RemoveSmartCard(SmartCardDB.SmartCardList[thumbprint]);
            SmartCardDB.SmartCardList.Remove(thumbprint);

            return true;
        }

        #endregion
    }
}
