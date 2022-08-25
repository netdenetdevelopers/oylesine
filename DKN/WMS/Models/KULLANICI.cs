using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DKN.Models
{
    public class KULLANICI
    {
        public String LOGICALREF { get; set; }
        [Required(ErrorMessage = "Lütfen ad giriniz!")]
        [StringLength(50, ErrorMessage = "En Fazla 50 Karakter olabilir.")]
        public string AD { get; set; }

        [Required(ErrorMessage = "Lütfen soyad giriniz!")]
        [StringLength(25, ErrorMessage = "En Fazla 25 Karakter olabilir.")]
        public string SOYAD { get; set; }

        [Required(ErrorMessage = "Lütfen email adresi giriniz!")]
        [EmailAddress(ErrorMessage = "Girdiğiniz email adresi geçersiz.")]
        [StringLength(50, ErrorMessage = "En Fazla 50 Karakter olabilir.")]
        public string EMAIL { get; set; }

        [Required(ErrorMessage = "Lütfen şifre giriniz!")]
        public string SIFRE { get; set; }

        [Required(ErrorMessage = "Lütfen telefon giriniz!")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Sadece rakam giriniz.")]
        public string TELEFON { get; set; }

        public int KULLANIM_DURUMU { get; set; }
        public int ADMIN_MI { get; set; }
        public int HESAP_TURU { get; set; }
        public byte[] RESIM { get; set; }
        public string resimSrc { get; set; }
        public bool kullanimDurumuBool { get; set; }
        public bool adminMiBool { get; set; }
        public string aktifPasif { get; set; }
        public string renk { get; set; }
        public List<SelectListItem> turList { get; set; }
        public string AdSoyad { get; set; }
    }
}