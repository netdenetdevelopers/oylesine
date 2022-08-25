using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class KAPSAM
    {
        public string LOGICALREF { get; set; }
        public string AD { get; set; }
        public string KISA_AD { get; set; }
        public string ACIKLAMA { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public int KONTOR { get; set; }

        public bool kullanimDurumuBool { get; set; }
    }
}