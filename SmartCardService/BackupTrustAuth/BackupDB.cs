using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupTrustAuth
{
    public static class BackupDB
    {
        #region DataBase

        public static Dictionary<String, SmartCard> SmartCardList = new Dictionary<String, SmartCard>();
        public static Dictionary<String, SmartCard> SmartCardRevocationList = new Dictionary<String, SmartCard>();
        public static List<String> AvailATMs = new List<string>();
        #endregion
    }
}
