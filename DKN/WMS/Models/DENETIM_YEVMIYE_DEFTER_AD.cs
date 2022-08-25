using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class DENETIM_YEVMIYE_DEFTER_AD
    {
        public string LOGICALREF { get; set; }
        public string DENETIM_LOGICALREF { get; set; }
        public string YEVMIYE_DEFTER_AD { get; set; }
        public int KULLANIM_DURUMU { get; set; }

        public bool kullanimDurumuBool { get; set; }
    }
}