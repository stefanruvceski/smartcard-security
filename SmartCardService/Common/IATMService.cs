using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IATMService
    {
        [OperationContract]
        void TestCommunicationATM();

        [OperationContract]
        bool SendPin(byte[] sign);

        [OperationContract]
        double PayIn(double amount);

        [OperationContract]
        double PayOut(double amount);

        [OperationContract]
        List<String> ListAllValidUsers();
    }
}
