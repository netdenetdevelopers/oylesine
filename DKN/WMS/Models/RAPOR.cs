using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class RAPOR
    {
        public string LOGICALREF { get; set; }
        public string AD { get; set; }
        public byte[] CONTENT { get; set; }
        public string CONTENT_TYPE { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public List<DENETIM_YEVMIYE_DEFTER_AD> yevmiyeDefterleriList { get; set; }
    }
}