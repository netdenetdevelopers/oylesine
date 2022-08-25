using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class KullaniciRol
    {
        public KULLANICI kullanici { get; set; }
        public IEnumerable<ROL> kullaniciRolList { get; set; }// kayitli bütün roller
        public IEnumerable<ROL> selectedKullaniciRolList { get; set; } //kullanıcının seçilen roller
        public PostedIds postedIds { get; set; } //güncellenecek seçilmiş  id ler

    }
}