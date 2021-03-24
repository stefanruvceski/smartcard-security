using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface ISmartCardServiceATM
    {
        [OperationContract]
        void TestCommunicationATM();

        /// <summary>
        /// Metoda kreira novi sertifikat, ciji SubjectName ce odgovarati korisnickom imenu korisnika, a Key korisnickom PIN-u
        /// </summary>
        [OperationContract]
        bool PublishATMCertificate();

        /// <summary>
        /// Provera validnosti pina i sertifikata
        /// </summary>
        [OperationContract]
        bool CheckCertificateAndPin(X509Certificate2 certificate, byte[] pin);

        /// <summary>
        /// povratna vrednost je koliko je novca ostalo na racunu
        /// </summary>
        [OperationContract]
        double PayIn(double amount, X509Certificate2 certificate);

        /// <summary>
        /// povratna vrednost je koliko je novca ostalo na racnunu
        /// </summary>
        [OperationContract]
        double PayOut(double amount, X509Certificate2 certificate);

        /// <summary>
        /// povratna vrednost je imena korisnika koji su validni
        /// </summary>
        [OperationContract]
        List<String> ListAllValidUsers();
    }
}
