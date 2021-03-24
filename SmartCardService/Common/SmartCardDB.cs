using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class SmartCardDB
    {
        #region DataBase

        public static Dictionary<String, SmartCard> SmartCardList = new Dictionary<String, SmartCard>();
        public static Dictionary<String, SmartCard> SmartCardRevocationList = new Dictionary<String, SmartCard>();
        public static List<String> AvailATMs = new List<string>();
        #endregion
    }
}
