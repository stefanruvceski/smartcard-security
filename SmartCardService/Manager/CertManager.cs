using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security;

namespace Manager
{
	public class CertManager
	{
        #region Get certificate from storage

        public static X509Certificate2 GetCertificateFromStorage(StoreName storeName, StoreLocation storeLocation, String subjectName)
		{
			X509Store store = new X509Store(storeName, storeLocation);
			store.Open(OpenFlags.ReadOnly);
   
			X509Certificate2Collection certCollection = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, true);
            X509Certificate2 cert = null;
			/// Check whether the subjectName of the certificate is exactly the same as the given "subjectName"
			foreach (X509Certificate2 c in certCollection)
			{
                
                string CN = null;
                try
                {
                    CN = c.SubjectName.Name.Split(',')[0];
                }
                catch(Exception e)
                {
                    Console.WriteLine("ERROR[Parsing]: "+ e.Message);
                    CN = c.SubjectName.Name;
                }

				if (CN.Equals(String.Format("CN={0}", subjectName)))
				{
                    cert = c;
                    break;
				}
			}
            return cert;
        }
        #endregion
    }
}
