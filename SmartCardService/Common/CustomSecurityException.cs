using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class CustomSecurityException
    {
        #region Fields

        string message;

        [DataMember]
        public string Message { get => message; set => message = value; }
        #endregion

        #region Constructors

        public CustomSecurityException()
        {
            
        }
        public CustomSecurityException(string message)
        {
            this.message = message;
        }
        #endregion
    }
}
