using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class KullaniciFirma
    {

        public KULLANICI kullanici { get; set; }
        public IEnumerable<FIRMA> kullaniciFirmaList { get; set; }// kayitli bütün firmalar
        public IEnumerable<FIRMA> selectedKullaniciFirmaList { get; set; } //kullanıcının seçilen firmaları
        public PostedIds postedIds { get; set; } //güncellenecek seçilmiş  id ler

    }
}