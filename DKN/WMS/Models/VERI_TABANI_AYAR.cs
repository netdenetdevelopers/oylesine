using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class VERI_TABANI_AYAR
    {
        public string LOGICALREF { get; set; }
        public int VERI_TABANI_AYAR_TUR { get; set; }
        public int VERI_TABANI_TUR { get; set; }

        [Required(ErrorMessage = "Lütfen bu alanı boş bırakmayınız!")]
        public string SUNUCU_ADI { get; set; }
        [Required(ErrorMessage = "Lütfen bu alanı boş bırakmayınız!")]
        public string SUNUCU_PORT_NUMARASI { get; set; }
        [Required(ErrorMessage = "Lütfen bu alanı boş bırakmayınız!")]
        public string VERI_TABANI_ADI { get; set; }
        [Required(ErrorMessage = "Lütfen bu alanı boş bırakmayınız!")]
        public string KULLANICI_ADI { get; set; }

        [Required(ErrorMessage = "Lütfen bu alanı boş bırakmayınız!")]
        [DataType(DataType.Password)]
        public string KULLANICI_SIFRE { get; set; }
        public string OLUSTURAN_LOGICALREF { get; set; }
        public DateTime OLUSTURMA_TARIHI { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public string GUNCELLEYEN_LOGICALREF { get; set; }
        public DateTime GUNCELLEME_TARIHI { get; set; }

        public string btnGuncelleKaydet { get; set; }
    }
}