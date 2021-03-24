using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IBackupService
    {
        [OperationContract]
        void TestCommunication();

        [OperationContract]
        bool AddSmartCard(SmartCard smartCard);

        [OperationContract]
        bool RemoveSmartCard(SmartCard smartCard);

        [OperationContract]
        bool AddATM(String ATM);

        [OperationContract]
        bool RemoveATM(String ATM);
        [OperationContract]
        bool PayIn(double amount,String thumbprint);

        [OperationContract]
        bool PayOut(double amount, String thumbprint);
    }
}
