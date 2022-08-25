using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DKN.Models
{
    public class DENETIM_KURALLARI
    {
        public string LOGICALREF { get; set; }
        public string KOD { get; set; }
        public string ACIKLAMA { get; set; }
        public string MUSTERI_ACIKLAMA { get; set; }
        public string MUSTERI_ACIKLAMA2 { get; set; }
        public string MEVZUAT { get; set; }
        public string SQL_IFADE { get; set; }
        public int KULLANIM_DURUMU { get; set; }
        public bool kullanimDurumuBool { get; set; }
        public List<KAPSAM> kapsamList { get; set; }
        public List<ALT_KAPSAM> altKapsamList { get; set; }
        public List<KAPSAM> selectedKapsamList { get; set; }
        public List<ALT_KAPSAM> selectedAltKapsamList { get; set; }
        public PostedIds postedIds { get; set; } //güncellenecek seçilmiş  id ler
        public CalisanKuralSonuc calisanKuralSonuc { get; set; }
    }
}