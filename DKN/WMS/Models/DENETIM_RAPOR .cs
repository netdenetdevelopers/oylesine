using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class DENETIM_RAPOR
    {
        public string LOGICALREF { get; set; }
        public string DENETIM_LOGICALREF { get; set; }
        public string RAPOR_LOGICALREF { get; set; }
        public int KULLANIM_DURUMU { get; set; }
    }
}