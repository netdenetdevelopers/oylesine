using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class DENETIM_KURALLARI_KAPSAM
    {
        public string LOGICALREF { get; set; }
        public string DENETIM_KURALLARI_LOGICALREF { get; set; }
        public string KAPSAM_LOGICALREF { get; set; }
        public int KULLANIM_DURUMU { get; set; }

        public bool kullanimDurumuBool { get; set; }
    }
}