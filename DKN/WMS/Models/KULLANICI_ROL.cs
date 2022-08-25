using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class KULLANICI_ROL
    {
        public string LOGICALREF { get; set; }
        public string ROL_LOGICALREF { get; set; }
        public string KULLANICI_LOGICALREF { get; set; }
        public string OLUSTURAN_LOGICALREF { get; set; }
        public DateTime OLUSTURMA_TARIHI { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public string GUNCELLEYEN_LOGICALREF { get; set; }
        public DateTime GUNCELLEME_TARIHI { get; set; }
    }
}