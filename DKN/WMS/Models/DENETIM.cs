using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DKN.Models
{
    public class DENETIM
    {
        public string LOGICALREF { get; set; }

        [Required(ErrorMessage = "Lütfen ad giriniz!")]
        public string AD { get; set; }

        public string SURE { get; set; }

        public string YIL_LOGICALREF { get; set; }
        public int KURAL_SAYISI { get; set; }
        public int KONTOR_SAYISI { get; set; }
        public DateTime TARIH { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public string DONEM_AY_LOGICALREF { get; set; }//1.Çeyrek, 2. çeyrek...
        public string donemAy { get; set; }
        public string firmaUnvan { get; set; }
        public string firmaVkn { get; set; }
        public string raporLogicalref { get; set; }
        public string raporAd { get; set; }
        public byte[] LOGO { get; set; }
        public string resimSrc { get; set; }
        public int DURUM { get; set; }
        public bool kullanimDurumuBool { get; set; }
        public List<KAPSAM> kapsamList { get; set; }
        public List<RAPOR> raporList { get; set; }
        public List<KAPSAM> selectedKapsamList { get; set; }
        public List<ALT_KAPSAM> selectedAltKapsamList { get; set; }
        public List<ALT_KAPSAM> altKapsamList { get; set; }
        public List<string> kuralKodList { get; set; }
        public List<ALT_KAPSAM> altKapsamListAVD { get; set; }
        public List<ALT_KAPSAM> altKapsamListGVD { get; set; }
        public List<ALT_KAPSAM> altKapsamListHD { get; set; }
        public List<ALT_KAPSAM> altKapsamListPD { get; set; }
        public List<DENETIM_YEVMIYE_DEFTER_AD> yevmiyeDefterAdList { get; set; }
        public PostedIds postedIds { get; set; } //güncellenecek seçilmiş  id ler

        public List<DosyaBilgileri> yevmiyeDefteriList { get; set; }
       
        //[Required(ErrorMessage = "Lütfen Dosya Seçiniz")]
        //[Display(Name = "Dosya Seçiniz...")]
        public HttpPostedFileBase[] files { get; set; }

        public List<YIL> yilList { get; set; }

        public string yilAd { get; set; }
        public List<SelectListItem> donemTurList { get; set; }
        public IEnumerable<DONEM_AY> donemList { get; set; }
        public IEnumerable<DONEM_AY> ayList { get; set; }
        public IEnumerable<FIRMA> firmaList { get; set; }
        public IEnumerable<DENETIM_KURALLARI> denetimKurallariList { get; set; }
        public string firmaLogicalref { get; set; }
        public int toplamKontorSayisi { get; set; }
        public string[] selectedKuralLogicalrefList { get; set; }
    }
}