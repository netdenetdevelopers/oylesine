using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class FIRMA_DENETIM
    {
        public string LOGICALREF { get; set; }
        public string FIRMA_LOGICALREF { get; set; }
        public string DENETIM_LOGICALREF { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public bool kullanimDurumuBool { get; set; }
    }
}