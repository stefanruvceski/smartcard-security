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
    public interface ISmartCardServiceClient
    {
        [OperationContract]
        List<String> TestCommunicationClient();

        /// <summary>
        /// Metoda kreira novi sertifikat, ciji SubjectName ce odgovarati korisnickom imenu korisnika, a Key korisnickom PIN-u
        /// </summary>
        [OperationContract]
        bool PublishNewSmartCard(String userGroup);

        /// <summary>
        /// Proverava da li je korisnik dobio pin i sertifikat i stavlja ga u listu aktivnih kartica
        /// </summary>
        [OperationContract]
        bool ConfirmPin(X509Certificate2 certificate, X509Certificate2 certificateSign, byte[] sign, byte[] hash);

        /// <summary>
        /// Metoda menja Key postojeceg sertifikata. Ako to nije moguce uraditi, treba probati obrisati postojeci i napraviti novi sertifikat sa novim Key-em
        /// </summary>
        [OperationContract]
        bool ResetPinCode(byte[] pin, X509Certificate2 certificate);
        
        /// <summary>
        /// Povlacenje smart kartice, tj. sertifikata i dodavanje sertifikata u listu povucenih
        /// </summary>
        [OperationContract]
        bool WithdrawSmartCardClient(byte[] pin);

        
    }
}
