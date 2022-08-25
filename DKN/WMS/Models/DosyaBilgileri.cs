using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class DosyaBilgileri
    {
        public string  dosyaAdi { get;set;}
        public string  dosyaId { get;set;}
        public List<DosyaBilgileri> dosyaBilgileriList { get; set; }


    }
}