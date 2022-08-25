using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class FIRMA
    {
        public string LOGICALREF { get; set; }
        [MaxLength((11), ErrorMessage ="En falza 11 karakter giriniz!")]
        [MinLength((10), ErrorMessage ="En az 10 karakter giriniz!")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage ="Lütfen sadece rakam giriniz!")]
        [Required(ErrorMessage = "Lütfen VKN/TCKN giriniz!")]
        public string VKNTCKN { get; set; }
        [Required(ErrorMessage = "Lütfen unvan giriniz!")]
        public string UNVAN { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public bool kullanimDurumuBool { get; set; }
        public byte[] LOGO { get; set; }
        public string resimSrc { get; set; }
        public List<DENETIM> denetimList { get; set; }
       
    }
}