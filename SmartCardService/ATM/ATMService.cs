using Common;
using Manager;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ATM
{
    public class ATMService : IATMService
    {
        #region Methods

        public List<String> ListAllValidUsers()
        {
            X509Certificate2 clientCertificate = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            if (Formatter.ParseOU(clientCertificate.Subject).Equals("Manager"))
            {
                return Program.SmartCardProxy.ListAllValidUsers();
            }
            else
            {
                return null;
            }
        }

        public double PayIn(double amount)
        {
            var clientCertificate = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            Console.WriteLine("Client executed PayIn command.");
            return Program.SmartCardProxy.PayIn(amount, clientCertificate);
        }

        public double PayOut(double amount)
        {
            var clientCertificate = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            Console.WriteLine("Client executed PayOut command.");
            return Program.SmartCardProxy.PayOut(amount, clientCertificate);
        }

        public bool SendPin(byte[] sign)
        {
            var clientCertificate = ((X509CertificateClaimSet)OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;
            // upit ka trustu
            bool retval = Program.SmartCardProxy.CheckCertificateAndPin(clientCertificate, sign);
            Console.WriteLine("Client executed SendPin command.");
            return retval;
        }

        public void TestCommunicationATM()
        {
            Console.WriteLine("TestCommunication success.");
        }
        #endregion
    }
}
