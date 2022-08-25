using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DKN.Models
{
    public class BILDIRIM
    {
        public string LOGICALREF { get; set; }
        public string KULLANICI_LOGICALREF { get; set; }
        public string ACIKLAMA_KISA { get; set; }
        public string ACIKLAMA_UZUN { get; set; }
        public DateTime TARIH { get; set; }
        public int DURUMU { get; set; }
        public int KULLANIM_DURUMU { get; set; }

    }
}
