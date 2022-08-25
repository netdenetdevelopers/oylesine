using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class ROL
    {
        public string LOGICALREF { get; set; }
        public int ROL_ID { get; set; }

        [Required(ErrorMessage = "Lütfen rol açıklaması giriniz!")]
        [StringLength(25, ErrorMessage = "En Fazla 25 Karakter olabilir.")]
        public string ACIKLAMA { get; set; }

        public string OLUSTURAN_LOGICALREF { get; set; }
        public DateTime OLUSTURMA_TARIHI { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public string GUNCELLEYEN_LOGICALREF { get; set; }
        public DateTime GUNCELLEME_TARIHI { get; set; }
        public Boolean kullanimDurumuBool { get; set; }
        public string aktifPasif { get; set; }
        public string renk { get; set; }


    }
}