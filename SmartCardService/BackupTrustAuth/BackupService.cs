using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackupTrustAuth
{
    class BackupService : IBackupService
    {
        public bool AddATM(string ATM)
        {
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();

            if (Thread.CurrentPrincipal.IsInRole("SmartCardServiceGroup"))
            {
                try
                {
                    // logovanje uspesnog dodavanja na replikatoru
                    scsLogger.WriteInformation("ATM successfully add in replicator DB.");

                    BackupDB.AvailATMs.Add(ATM);
                    return true;
                }
                catch (Exception e)
                {
                    // logovanje neuspesnog dodavanja na replikatoru
                    scsLogger.WriteError("ATM unsuccessfully add in replicator DB.");

                    Console.WriteLine("ERROR[AddATM]: " + e.Message);
                    return false;
                }
            }
            else
            {
                // logovanje neautorizovanog pristupa replikatoru
                scsLogger.WriteError("Unauthorized call of AddATM() method on replicator.");

                return false;
            }
        }

        public bool AddSmartCard(SmartCard smartCard)
        {
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();

            if (Thread.CurrentPrincipal.IsInRole("SmartCardServiceGroup"))
            {
                try
                {
                    // logovanje uspesnog dodavanja smart kartice na replikatoru
                    scsLogger.WriteInformation("Smart card successfully add in replicator DB.");

                    BackupDB.SmartCardList.Add(smartCard.CreateCertificate, smartCard);
                    return true;
                }
                catch (Exception e)
                {
                    // logovanje neuspesnog dodavanja smart kartice na replikatoru
                    scsLogger.WriteError("Smart card unsuccessfully add in replicator DB.");

                    Console.WriteLine("ERROR[AddATM]: " + e.Message);
                    return false;
                }
            }
            else
            {
                // logovanje neautorizovanog pristupa metodi replikatora
                scsLogger.WriteError("Unauthorized call of AddSmartCard() method on replicator.");

                return false;
            }
        
        }

        public bool PayIn(double amount, string thumbprint)
        {
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();

            if (Thread.CurrentPrincipal.IsInRole("SmartCardServiceGroup"))
            {
                if (BackupDB.SmartCardList.ContainsKey(thumbprint))
                {
                    scsLogger.WriteInformation("PayIn() successfully called on replicator.");

                    BackupDB.SmartCardList[thumbprint].Amount += amount;
                    return true;
                }
                else
                {
                    scsLogger.WriteError("PayIn() called by unauthentificated client on replicator.");
                }
            }
            else
            {
                scsLogger.WriteError("PayIn() called by unauthorized client on replicator.");
            }

            return false;
        }

        public bool PayOut(double amount, string thumbprint)
        {
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();

            if (Thread.CurrentPrincipal.IsInRole("SmartCardServiceGroup"))
            {
                if (BackupDB.SmartCardList.ContainsKey(thumbprint))
                {
                    scsLogger.WriteInformation("PayOut() successfully called on replicator.");

                    BackupDB.SmartCardList[thumbprint].Amount -= amount;
                    return true;
                }
                else
                {
                    scsLogger.WriteError("PayOut() called by unauthentificated client on replicator.");
                }
            }
            else
            {
                scsLogger.WriteError("Payout() called by unauthorized client on replicator.");
            }

            return false;
        }

        public bool RemoveATM(string ATM)
        {
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();

            if (Thread.CurrentPrincipal.IsInRole("SmartCardServiceGroup"))
            {
                try
                {
                    scsLogger.WriteInformation("RemoveATM() successfully called on replicator.");

                    return BackupDB.AvailATMs.Remove(ATM);
                }
                catch (Exception e)
                {
                    scsLogger.WriteError("RemoveATM() unsuccessfully called on replicator.");
                    Console.WriteLine("ERROR[AddATM]: " + e.Message);
                    return false;
                }
            }
            else
            {
                scsLogger.WriteError("RemoveATM() called by unauthorized client on replicator.");
                return false;
            }
        }

        public bool RemoveSmartCard(SmartCard smartCard)
        {
            SmartCardServiceLogger scsLogger = new SmartCardServiceLogger();

            if (Thread.CurrentPrincipal.IsInRole("SmartCardServiceGroup"))
            {
                try
                {
                    scsLogger.WriteInformation("RemoveSmartCard() successfully called on replicator.");

                    BackupDB.SmartCardList.Remove(smartCard.CreateCertificate);
                    BackupDB.SmartCardRevocationList.Add(smartCard.CreateCertificate, smartCard);
                    return true;
                }
                catch (Exception e)
                {
                    scsLogger.WriteError("RemoveSmartCard() unsuccessfully called on replicator.");

                    Console.WriteLine("ERROR[AddATM]: " + e.Message);
                    return false;
                }
            }
            else
            {
                scsLogger.WriteError("RemoveSmartCard() called by unauthorized client on replicator.");
                return false;
            }
        }

        public void TestCommunication()
        {
            Console.WriteLine("TestCommunication success.");
        }
    }
}
