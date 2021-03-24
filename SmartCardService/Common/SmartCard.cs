using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
   
    public class SmartCard
    {
        #region Fields

        private string createCertificate;
        private string signCertificate;
        private byte[] pinCode;
        private double amount = 0;
        private int invalidCnt = 0;
        private string username;
        
        public byte[] PinCode { get => pinCode; set => pinCode = value; }
        public double Amount { get => amount; set => amount = value; }
        public int InvalidCnt { get => invalidCnt; set => invalidCnt = value; }
        public string CreateCertificate { get => createCertificate; set => createCertificate = value; }
        public string SignCertificate { get => signCertificate; set => signCertificate = value; }
        public string Username { get => username; set => username = value; }
        #endregion

        #region Constructors

        public SmartCard()
        {
        }

        public SmartCard( string createCertificate, string signCertificate, byte[] pinCode,  string username, double amount=0)
        {
            this.createCertificate = createCertificate;
            this.signCertificate = signCertificate;
            this.pinCode = pinCode;
            this.amount = amount;
            this.invalidCnt = 0;
            this.username = username;
        }
        #endregion
    }
}
