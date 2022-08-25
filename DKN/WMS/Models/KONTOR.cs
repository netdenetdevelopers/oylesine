using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DKN.Models
{
    public class KONTOR
    {
        public String LOGICALREF { get; set; }
        [Required(ErrorMessage = "Lütfen kontor adedi giriniz!")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Sadece rakam giriniz.")]
        public int KONTOR_ADET { get; set; }
        [Required(ErrorMessage = "Lütfen indirim oranı giriniz!")]
        [RegularExpression("([0-9][0-9]*)", ErrorMessage = "Sadece rakam giriniz.")]
        public int INDIRIM_ORANI { get; set; }
        [Required(ErrorMessage = "Lütfen paket fiyatı giriniz!")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Sadece rakam giriniz.")]
        public string PAKET_FIYATI { get; set; }

        [Required(ErrorMessage = "Lütfen kontor birim fiyatı giriniz!")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Sadece rakam giriniz.")]
        public string BIRIM_FIYATI { get; set; }

        [Required(ErrorMessage = "Lütfen acıklama giriniz!")]
        public string ACIKLAMA { get; set; }

        public int KULLANIM_DURUMU { get; set; }
        public bool kullanimDurumuBool { get; set; }
       
    }
}