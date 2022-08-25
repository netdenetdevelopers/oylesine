using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class ALT_KAPSAM
    {
        public string LOGICALREF { get; set; }
        [Required(ErrorMessage = "Lütfen Ad giriniz!")]
        public string AD { get; set; }
        [Required(ErrorMessage = "Lütfen kısa ad giriniz!")]
        public string KISA_AD { get; set; }
        public string ACIKLAMA { get; set; }
        [Required(ErrorMessage = "Lütfen kontor adedi giriniz!")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Sadece rakam giriniz.")]
        public int KONTOR { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public bool kullanimDurumuBool { get; set; }
        public bool IsCheck { get; set; }
        public int kuralSayisi { get; set; }
        public int toplamKontorSayisi { get; set; }
    }
}