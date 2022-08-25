using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DKN.Models
{
    public class KONTOR_HAREKET
    {
        public string LOGICALREF { get; set; }
        public string KULLANICI_LOGICALREF { get; set; }
        public int ISLEM_TIPI { get; set; }
        public int KONTOR_MIKTARI { get; set; }
        public string ISLEM_ACIKLAMA { get; set; }
        public DateTime ISLEM_TARIHI { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public string tipAciklama { get; set; }

    }
}
