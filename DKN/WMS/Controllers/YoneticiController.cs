using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using DKN.db;
using DKN.Models;
using DKN.parametreler;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace DKN.Controllers
{
    public class YoneticiController : Controller
    {
        // GET: Yonetici
        public ActionResult IndexYonetici()
        {
            return View();
        }
        [Authorize]
        public ActionResult KuralListesi()
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            List<DENETIM_KURALLARI> kuralistesi = new List<DENETIM_KURALLARI>();
            kuralistesi = CRUD.GetirDenetimKurallariListesi();
            return View(kuralistesi);
        }

        [Authorize]
        public ActionResult CreateDenetimKurallari()
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            DENETIM_KURALLARI kural = new DENETIM_KURALLARI();
            kural.kapsamList = CRUD.GetirKapsamListesi();
            kural.altKapsamList = CRUD.GetirAltKapsamListesi();
            //VD Hariç getiriyor
            //kural.kapsamList = CRUD.GetirKapsamListesiKuralIcin();
            kural.selectedKapsamList = new List<KAPSAM>();
            kural.selectedAltKapsamList = new List<ALT_KAPSAM>();
            return View(kural);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateDenetimKurallari(DENETIM_KURALLARI kural)
        {
            try
            {
                if (kural.MUSTERI_ACIKLAMA == null)
                {
                    kural.MUSTERI_ACIKLAMA = "";
                }
                if (kural.MUSTERI_ACIKLAMA2 == null)
                {
                    kural.MUSTERI_ACIKLAMA2 = "";
                }
                if (kural.MEVZUAT == null)
                {
                    kural.MEVZUAT = "";
                }
                if (kural.SQL_IFADE == null)
                {
                    kural.SQL_IFADE = "";
                }
                Guid obj = Guid.NewGuid();
                kural.LOGICALREF = obj.ToString();
                CRUD.DenetimKuraliEkle(kural);

                if (kural.postedIds != null)
                {
                    foreach (string kapsamId in kural.postedIds.kapsamIds)
                    {
                        DENETIM_KURALLARI_KAPSAM denetimKuralKapsam = new DENETIM_KURALLARI_KAPSAM();
                        Guid objYeni = Guid.NewGuid();
                        denetimKuralKapsam.LOGICALREF = objYeni.ToString();
                        denetimKuralKapsam.DENETIM_KURALLARI_LOGICALREF = kural.LOGICALREF;
                        denetimKuralKapsam.KAPSAM_LOGICALREF = kapsamId;
                        CRUD.DenetimKurallariKapsamEkle(denetimKuralKapsam);
                    }
                    foreach (string kapsamId in kural.postedIds.altKapsamIds)
                    {
                        DENETIM_KURALLARI_ALT_KAPSAM denetimKuralAltKapsam = new DENETIM_KURALLARI_ALT_KAPSAM();
                        Guid objYeni = Guid.NewGuid();
                        denetimKuralAltKapsam.LOGICALREF = objYeni.ToString();
                        denetimKuralAltKapsam.DENETIM_KURALLARI_LOGICALREF = kural.LOGICALREF;
                        denetimKuralAltKapsam.ALT_KAPSAM_LOGICALREF = kapsamId;
                        CRUD.DenetimKurallariAltKapsamEkle(denetimKuralAltKapsam);
                    }
                }

                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Kural Kaydı Başarılı." };
                return RedirectToAction("KuralListesi");
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Kural Kaydı Sırasında Hata Oluştu." };
                return RedirectToAction("KuralListesi");
            }
        }

        [Authorize]
        public ActionResult EditDenetimKurallari(string id)
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            DENETIM_KURALLARI kayitliDenetimKural = new DENETIM_KURALLARI();
            kayitliDenetimKural.kapsamList = new List<KAPSAM>();
            kayitliDenetimKural.altKapsamList = new List<ALT_KAPSAM>();
            kayitliDenetimKural.postedIds = new PostedIds();
            try
            {
                kayitliDenetimKural = CRUD.GetirDenetimKurallariSqlsizIdIle(id);
                if (kayitliDenetimKural.KULLANIM_DURUMU == 1)
                {
                    kayitliDenetimKural.kullanimDurumuBool = true;
                }
                else
                {
                    kayitliDenetimKural.kullanimDurumuBool = false;
                }
                kayitliDenetimKural.kapsamList = CRUD.GetirKapsamListesi();
                kayitliDenetimKural.altKapsamList = CRUD.GetirAltKapsamListesi();
                //VD Hariç getiriyor
                //kayitliDenetimKural.kapsamList = CRUD.GetirKapsamListesiKuralIcin();
                List<DENETIM_KURALLARI_KAPSAM> denetimKapsamList = CRUD.GetirDenetimKurallariKapsamListesiDenetimKuralIdIle(kayitliDenetimKural.LOGICALREF);
                kayitliDenetimKural.selectedKapsamList = new List<KAPSAM>();
                foreach (DENETIM_KURALLARI_KAPSAM denetimKapsam in denetimKapsamList)
                {
                    KAPSAM kapsam = CRUD.GetirKapsamIdIle(denetimKapsam.KAPSAM_LOGICALREF);
                    kayitliDenetimKural.selectedKapsamList.Add(kapsam);
                }

                List<DENETIM_KURALLARI_ALT_KAPSAM> denetimAltKapsamList = CRUD.GetirDenetimKurallariAltKapsamListesiDenetimKuralIdIle(kayitliDenetimKural.LOGICALREF);
                kayitliDenetimKural.selectedAltKapsamList = new List<ALT_KAPSAM>();
                foreach (DENETIM_KURALLARI_ALT_KAPSAM denetimAltKapsam in denetimAltKapsamList)
                {
                    ALT_KAPSAM altKapsam = CRUD.GetirAltKapsamIdIle(denetimAltKapsam.ALT_KAPSAM_LOGICALREF);
                    kayitliDenetimKural.selectedAltKapsamList.Add(altKapsam);
                }
            }
            catch (Exception e)
            {

            }
            return View("EditDenetimKurallari", kayitliDenetimKural);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditDenetimKurallari(string id, DENETIM_KURALLARI denetimKurallari)
        {
            denetimKurallari.LOGICALREF = id;
            if (denetimKurallari.MUSTERI_ACIKLAMA == null)
            {
                denetimKurallari.MUSTERI_ACIKLAMA = "";
            }
            if (denetimKurallari.MUSTERI_ACIKLAMA2 == null)
            {
                denetimKurallari.MUSTERI_ACIKLAMA2 = "";
            }
            if (denetimKurallari.MEVZUAT == null)
            {
                denetimKurallari.MEVZUAT = "";
            }
            try
            {
                if (denetimKurallari.kullanimDurumuBool)
                {
                    denetimKurallari.KULLANIM_DURUMU = 1;
                }
                else
                {
                    denetimKurallari.KULLANIM_DURUMU = 0;
                }
                CRUD.GuncelleDenetimKurallari(denetimKurallari);
                CRUD.SilDenetimKuralKapsamKuralLogicalRefIle(denetimKurallari.LOGICALREF);
                CRUD.SilDenetimKuralAltKapsamKuralLogicalRefIle(denetimKurallari.LOGICALREF);
                if (denetimKurallari.postedIds != null)
                {
                    foreach (string kapsamId in denetimKurallari.postedIds.kapsamIds)
                    {
                        DENETIM_KURALLARI_KAPSAM denetimKuralKapsam = new DENETIM_KURALLARI_KAPSAM();
                        Guid objYeni = Guid.NewGuid();
                        denetimKuralKapsam.LOGICALREF = objYeni.ToString();
                        denetimKuralKapsam.DENETIM_KURALLARI_LOGICALREF = denetimKurallari.LOGICALREF;
                        denetimKuralKapsam.KAPSAM_LOGICALREF = kapsamId;
                        CRUD.DenetimKurallariKapsamEkle(denetimKuralKapsam);
                    }
                    foreach (string altKapsamId in denetimKurallari.postedIds.altKapsamIds)
                    {
                        DENETIM_KURALLARI_ALT_KAPSAM denetimKuralAltKapsam = new DENETIM_KURALLARI_ALT_KAPSAM();
                        Guid objYeni = Guid.NewGuid();
                        denetimKuralAltKapsam.LOGICALREF = objYeni.ToString();
                        denetimKuralAltKapsam.DENETIM_KURALLARI_LOGICALREF = denetimKurallari.LOGICALREF;
                        denetimKuralAltKapsam.ALT_KAPSAM_LOGICALREF = altKapsamId;
                        CRUD.DenetimKurallariAltKapsamEkle(denetimKuralAltKapsam);
                    }
                }

                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Güncelleme İşlemi Başarılı." };
                return RedirectToAction("KuralListesi");
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Güncelleme İşlemi Sırasında Hata Oluştu." };
                return View();
            }
        }
        [Authorize]
        public ActionResult GuncelleDenetimKuralDurum(string id)
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            DENETIM_KURALLARI kural = new DENETIM_KURALLARI();
            try
            {
                if (id != null)
                {
                    kural = CRUD.GetirDenetimKurallariIdIle(id);
                }
                if (kural.KULLANIM_DURUMU == 1)
                {
                    kural.KULLANIM_DURUMU = 0;
                }
                else if (kural.KULLANIM_DURUMU == 0)
                {
                    kural.KULLANIM_DURUMU = 1;
                }

                CRUD.GuncelleDenetimKurallari(kural);
                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Güncelleme Başarılı." };
                return RedirectToAction("KuralListesi");
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Güncelleme Sırasında Hata Oluştu." };
                return RedirectToAction("KuralListesi");
            }

        }


        [Authorize]
        public ActionResult RaporTasarim()
        {
            try
            {
                if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                    if (Session["menuGoster"].ToString().Equals("hidden"))
                        return RedirectToAction("HomeIndex", "Home");
            }
            catch (Exception e)
            {

            }
            return View();
        }



        [Authorize]
        public ActionResult KullaniciList()
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            List<KULLANICI> kullaniciListesi = new List<KULLANICI>();
            try
            {
                kullaniciListesi = CRUD.GetirKullaniciListesi();
                //foreach (var kullanici in kullaniciListesi)
                //{
                //    kullanici.kullaniciRolList = CRUD.GetirKullaniciRolListKullaniciIdIle(kullanici.LOGICALREF);
                //}
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Kullanıcı Listesi Getirilirken Hata Oluştu." };

            }
            return View(kullaniciListesi);

        }

        [Authorize]
        public ActionResult Create()
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            return View();
        }

        // POST: Kullanicilar/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(KULLANICI kullanici)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int kullaniciVarmi = CRUD.KullaniciVarMi(kullanici.EMAIL);
                    if (kullaniciVarmi == 1)
                    {
                        Guid obj = Guid.NewGuid();
                        kullanici.LOGICALREF = obj.ToString();
                        CRUD.KullaniciEkle(kullanici);
                        TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Kullanıcı Kaydı Başarılı." };
                        return RedirectToAction("KullaniciList");
                    }
                    else
                        TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Kullanıcı Kaydı Mevcut." };

                }
                return View();
            }

            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Kullanıcı Kaydı Sırasında Hata Oluştu." };

                return View();
            }
        }

        [Authorize]
        public ActionResult Edit(string id)
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            KULLANICI kayitliKulanici = new KULLANICI();
            try
            {
                kayitliKulanici = CRUD.GetirKullaniciIdIle(id);
                if (kayitliKulanici.KULLANIM_DURUMU == 1)
                {
                    kayitliKulanici.kullanimDurumuBool = true;
                }
                else
                {
                    kayitliKulanici.kullanimDurumuBool = false;
                }
                if (kayitliKulanici.ADMIN_MI == 1)
                {
                    kayitliKulanici.adminMiBool = true;
                }
                else
                {
                    kayitliKulanici.adminMiBool = false;
                }
            }
            catch (Exception e)
            {

            }
            return View("Edit", kayitliKulanici);
        }

        // POST: Kullanicilar/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(string id, KULLANICI kullanici)
        {

            kullanici.LOGICALREF = id;
            try
            {
                if (kullanici.kullanimDurumuBool)
                {
                    kullanici.KULLANIM_DURUMU = 1;
                }
                else
                {
                    kullanici.KULLANIM_DURUMU = 0;
                }
                if (kullanici.adminMiBool)
                {
                    kullanici.ADMIN_MI = 1;
                }
                else
                {
                    kullanici.ADMIN_MI = 0;
                }
                CRUD.GuncelleKullanici(kullanici);
                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Güncelleme İşlemi Başarılı." };
                return RedirectToAction("KullaniciList");

            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Güncelleme İşlemi Sırasında Hata Oluştu." };
                return View();
            }

        }
        [Authorize]
        public ActionResult GuncelleKullaniciDurum(String id)
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            KULLANICI kullanici = new KULLANICI();

            //kullanici.LOGICALREF = id;
            try
            {
                if (id != null)
                {
                    kullanici = CRUD.GetirKullaniciIdIle(id);
                }
                if (kullanici.KULLANIM_DURUMU == 1)
                {
                    kullanici.KULLANIM_DURUMU = 0;
                }
                else if (kullanici.KULLANIM_DURUMU == 0)
                {
                    kullanici.KULLANIM_DURUMU = 1;
                }

                CRUD.GuncelleKullanici(kullanici);

                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Durum Güncelleme Başarılı." };

                return RedirectToAction("KullaniciList");
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Durum Güncelleme Sırasında Hata Oluştu." };

                return View("KullaniciList");
            }

        }
        [Authorize]
        public ActionResult SqlSifreModal(string denetimKuraliLogicalref)
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            try
            {
                if (denetimKuraliLogicalref == null)//şifre yanlış girilip bu metoda geldiğinde sqlTur kaybolmasın diye kullanıldı
                    denetimKuraliLogicalref = Session["denetimKuraliLogicalref"].ToString();
                else
                    Session["denetimKuraliLogicalref"] = denetimKuraliLogicalref;
                return View();

            }
            catch (Exception)
            {
                return View();

            }

        }

        [HttpPost]
        public ActionResult SqlGoster(string SQLsifre)
        {
            DENETIM_KURALLARI denetimKurali = new DENETIM_KURALLARI();
            try
            {
                if (SQLsifre.Equals(parametreler.Sabitler.SQL_PASS))
                {
                    Session["labelGoster"] = "0";//yanlış şifre girildiğinde şifre hatalı label gizleyip göstermek için kullanıldı
                    if (Session["denetimKuraliLogicalref"] != null)
                    {

                        denetimKurali = CRUD.GetirDenetimKurallariIdIle(Session["denetimKuraliLogicalref"].ToString());
                        //sql.SQL_TUR = 1;

                        return View(denetimKurali);
                    }
                }
                Session["labelGoster"] = "1";//yanlış şifre girildiğinde şifre hatalı label gizleyip göstermek için kullanıldı
                return RedirectToAction("SqlSifreModal");
            }
            catch (Exception)
            {
                return View();

            }

        }

        [HttpPost]
        public ActionResult KaydetGuncelleSql(DENETIM_KURALLARI denetimKurallari)
        {
            try
            {
                if (denetimKurallari.LOGICALREF != null)
                {
                    if (denetimKurallari.SQL_IFADE == null)
                        denetimKurallari.SQL_IFADE = "";
                    CRUD.GuncelleDenetimKurallariSqlLogicalRefIle(denetimKurallari.LOGICALREF, denetimKurallari.SQL_IFADE);
                    TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Güncelleme İşlemi Başarılı." };
                }

            }
            catch (Exception)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "SQL Güncelleme Sırasında Hata Oluştu." };

            }
            return RedirectToAction("KuralListesi");

        }

        [Authorize]
        public ActionResult KontorIslemleri()
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            List<KONTOR> kontorListesi = new List<KONTOR>();
            try
            {
                kontorListesi = CRUD.GetirKontorListesi();

            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Kontör Listesi Getirilirken Hata Oluştu." };

            }
            return View(kontorListesi);

        }

        [Authorize]
        public ActionResult EditKontor(string id)
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            KONTOR kayitliKontor = new KONTOR();
            try
            {
                kayitliKontor = CRUD.GetirKontorIdIle(id);
                if (kayitliKontor.KULLANIM_DURUMU == 1)
                {
                    kayitliKontor.kullanimDurumuBool = true;
                }
                else
                {
                    kayitliKontor.kullanimDurumuBool = false;
                }
            }
            catch (Exception e)
            {

            }
            return View("EditKontor", kayitliKontor);
        }

        // POST: Kullanicilar/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditKontor(string id, KONTOR kontor)
        {

            kontor.LOGICALREF = id;
            try
            {
                if (kontor.kullanimDurumuBool)
                {
                    kontor.KULLANIM_DURUMU = 1;
                }
                else
                {
                    kontor.KULLANIM_DURUMU = 0;
                }
                CRUD.GuncelleKontor(kontor);
                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Güncelleme İşlemi Başarılı." };
                return RedirectToAction("KontorIslemleri");

            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Güncelleme İşlemi Sırasında Hata Oluştu." };
                return View();
            }

        }

        public ActionResult CreateKontor()
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            return View();
        }

        // POST: Kullanicilar/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateKontor(KONTOR kontor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Guid obj = Guid.NewGuid();
                    kontor.LOGICALREF = obj.ToString();
                    CRUD.KontorEkle(kontor);
                    TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Kontör Kaydı Başarılı." };
                    return RedirectToAction("KontorIslemleri");

                }
                return View();
            }

            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Kontör Kaydı Sırasında Hata Oluştu." };

                return View();
            }
        }

        [Authorize]
        public ActionResult GuncelleKontorDurum(String id)
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            KONTOR kontor = new KONTOR();

            //kullanici.LOGICALREF = id;
            try
            {
                if (id != null)
                {
                    kontor = CRUD.GetirKontorIdIle(id);
                }
                if (kontor.KULLANIM_DURUMU == 1)
                {
                    kontor.KULLANIM_DURUMU = 0;
                }
                else if (kontor.KULLANIM_DURUMU == 0)
                {
                    kontor.KULLANIM_DURUMU = 1;
                }

                CRUD.GuncelleKontor(kontor);

                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Durum Güncelleme Başarılı." };

                return RedirectToAction("KontorIslemleri");
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Durum Güncelleme Sırasında Hata Oluştu." };

                return View("KontorIslemleri");
            }

        }

        public ActionResult CreateAltKapsam()
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            return View();
        }

        // POST: Kullanicilar/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateAltKapsam(ALT_KAPSAM altKapsam)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Guid obj = Guid.NewGuid();
                    altKapsam.LOGICALREF = obj.ToString();
                    CRUD.AltKapsamEkle(altKapsam);
                    TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Alt Kapsam Kaydı Başarılı." };
                    return RedirectToAction("AltKapsamList");

                }
                return View();
            }

            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Kontör Kaydı Sırasında Hata Oluştu." };

                return View();
            }
        }

        [Authorize]
        public ActionResult AltKapsamList()
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            List<ALT_KAPSAM> altKapsamListesi = new List<ALT_KAPSAM>();
            try
            {
                altKapsamListesi = CRUD.GetirAltKapsamListesi();
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Alt Kapsam Listesi Getirilirken Hata Oluştu." };

            }
            return View(altKapsamListesi);

        }

        [Authorize]
        public ActionResult GuncelleAltKapsamDurum(String id)
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            ALT_KAPSAM altKapsam = new ALT_KAPSAM();

            //kullanici.LOGICALREF = id;
            try
            {
                if (id != null)
                {
                    altKapsam = CRUD.GetirAltKapsamIdIle(id);
                }
                if (altKapsam.KULLANIM_DURUMU == 1)
                {
                    altKapsam.KULLANIM_DURUMU = 0;
                }
                else if (altKapsam.KULLANIM_DURUMU == 0)
                {
                    altKapsam.KULLANIM_DURUMU = 1;
                }

                CRUD.GuncelleAltKapsam(altKapsam);

                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Durum Güncelleme Başarılı." };

                return RedirectToAction("AltKapsamList");
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Durum Güncelleme Sırasında Hata Oluştu." };

                return View("AltKapsamList");
            }

        }

        [Authorize]
        public ActionResult EditAltKapsam(string id)
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            ALT_KAPSAM altKapsam = new ALT_KAPSAM();
            try
            {
                altKapsam = CRUD.GetirAltKapsamIdIle(id);
                if (altKapsam.KULLANIM_DURUMU == 1)
                {
                    altKapsam.kullanimDurumuBool = true;
                }
                else
                {
                    altKapsam.kullanimDurumuBool = false;
                }
            }
            catch (Exception e)
            {

            }
            return View("EditAltKapsam", altKapsam);
        }

        // POST: Kullanicilar/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditAltKapsam(string id, ALT_KAPSAM altKapsam)
        {

            altKapsam.LOGICALREF = id;
            try
            {
                if (ModelState.IsValid)
                {
                    if (altKapsam.kullanimDurumuBool)
                    {
                        altKapsam.KULLANIM_DURUMU = 1;
                    }
                    else
                    {
                        altKapsam.KULLANIM_DURUMU = 0;
                    }
                    CRUD.GuncelleAltKapsam(altKapsam);
                    TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Güncelleme İşlemi Başarılı." };
                    return RedirectToAction("AltKapsamList");
                }
                else
                    return EditAltKapsam(id);
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Güncelleme İşlemi Sırasında Hata Oluştu." };
                return View();
            }

        }

        //-----------------Yönetici denetimi ile ilgili sayfalar----------------------------

        [Authorize]
        public ActionResult CreateDenetim()
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            DENETIM denetim = new DENETIM();
            denetim.firmaList = new List<FIRMA>();
            denetim.denetimKurallariList = new List<DENETIM_KURALLARI>();
            denetim.yilList = CRUD.GetirYilListesi();
            denetim.firmaList = CRUD.GetirFirmaListesi();
            denetim.denetimKurallariList = CRUD.GetirDenetimKurallariLogicalrefVeKodListesi();

            return View(denetim);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateDenetim(DENETIM denetim)
        {
            try
            {

                if (!string.IsNullOrEmpty(denetim.AD))
                {
                    Guid obj = Guid.NewGuid();
                    denetim.LOGICALREF = obj.ToString();
                    denetim.TARIH = DateTime.Now;
                    CRUD.YoneticiDenetimEkle(denetim);

                    FIRMA_DENETIM firmaDenetim = new FIRMA_DENETIM();
                    Guid objYeni2 = Guid.NewGuid();
                    firmaDenetim.LOGICALREF = objYeni2.ToString();
                    firmaDenetim.DENETIM_LOGICALREF = denetim.LOGICALREF;
                    firmaDenetim.FIRMA_LOGICALREF = denetim.firmaLogicalref;
                    CRUD.FirmaDenetimEkle(firmaDenetim,9);//9 yönetici 1 kullanıcı denetimleri

                    foreach (string item in denetim.selectedKuralLogicalrefList)
                    {
                        DENETIM_DENETIM_KURALLARI denetimKurallari = new DENETIM_DENETIM_KURALLARI();
                        Guid objYeni3 = Guid.NewGuid();
                        denetimKurallari.LOGICALREF = objYeni3.ToString();
                        denetimKurallari.DENETIM_LOGICALREF = denetim.LOGICALREF;
                        denetimKurallari.DENETIM_KURALLARI_LOGICALREF = item;
                        CRUD.DenetimDenetimKuralllariEkle(denetimKurallari);
                    }

                    TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Denetim Kaydı Başarılı." };
                    return RedirectToAction("YoneticiDenetimList");

                }
                else
                {
                    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Denetim Adını Giriniz!" };

                    return RedirectToAction("CreateDenetim");
                }



            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Denetim Kaydı Sırasında Hata Oluştu." };
                return RedirectToAction("CreateDenetim");
            }
        }
        [Authorize]
        public ActionResult EditDenetim(string id, string firmaId)
        {
            List<DosyaBilgileri> yevmiyeDefteriListTemp = new List<DosyaBilgileri>();
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");

            Session["denetimBaslatBtnDisable"] = "disabled";
            Session["denetimId"] = id;
            Session["gecenSure"] = "0";
            Session["firmaId"] = firmaId;
            //sadeedevam eden bir denetim olduğunda mesaj verebilmek için
            Session["devamEdenDenetimVarMi"] = "0";
            string path = Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ ");
            DENETIM denetim = new DENETIM();

            if (id != null)
            {
                try
                {
                    FIRMA firma = new FIRMA();
                    firma = CRUD.GetirFirmaIdIle(firmaId);
                    Session["firmaUnvan"] = firma.UNVAN;
                    Session["firmaTcknVkn"] = firma.VKNTCKN;
                    if (Session["dosyalariSilme"] == null)
                    {
                        if (Directory.Exists(path))
                        {
                            string[] files = Directory.GetFiles(path);
                            foreach (string file in files)
                            {
                                System.IO.File.Delete(file);
                            }
                        }
                    }

                    denetim = CRUD.GetirYoneticiDenetimIdIle(id);
                    Session["denetimAd"] = denetim.AD;
                    denetim.yevmiyeDefteriList = new List<DosyaBilgileri>();

                    //iterating through multiple file collection   
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ "));
                        FileInfo[] files = di.GetFiles("*.xml");
                        if (files.Count() > 0)
                            Session["denetimBaslatBtnDisable"] = "";//Denetim başlatma butonu aktif ediliyor
                        if (Session["yevmiyeDefteriListYoneticiTemp"] != null)
                            yevmiyeDefteriListTemp = (List<DosyaBilgileri>)Session["yevmiyeDefteriListYoneticiTemp"];

                        foreach (var item in files)
                        {
                            DosyaBilgileri dosyaBilgileriTemp = new DosyaBilgileri();
                            dosyaBilgileriTemp.dosyaAdi = item.Name;
                            dosyaBilgileriTemp.dosyaId = item.Name;
                            denetim.yevmiyeDefteriList.Add(dosyaBilgileriTemp);
                        }
                        if (yevmiyeDefteriListTemp.Count > 0)
                        {
                            List<DosyaBilgileri> listTemp = new List<DosyaBilgileri>();

                            listTemp.AddRange(denetim.yevmiyeDefteriList);
                            foreach (var itemTemp in yevmiyeDefteriListTemp)
                            {
                                foreach (var item in denetim.yevmiyeDefteriList)
                                {
                                    if (item.dosyaAdi == itemTemp.dosyaAdi)
                                        listTemp.Remove(item);
                                }

                            }
                            denetim.yevmiyeDefteriList = new List<DosyaBilgileri>();
                            denetim.yevmiyeDefteriList.AddRange(listTemp);
                        }
                    }
                }
                catch (Exception e)
                {
                    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Güncelleme İşlemi Sırasında Hata Oluştu." };
                }
            }

            int devamEdenDenetimSayisi = CRUD.GetirDevamEdenDenetimSayisi();
            if (devamEdenDenetimSayisi==0)
            {
                Session["devamEdenDenetimVarMi"] = "0";
            }
            else
            {
                Session["devamEdenDenetimVarMi"] = "1";
            }

            return View(denetim);
        }


        //public ActionResult SilYevmiyeDefteriKlasorden(string id)
        //{
        //    try
        //    {
        //        if (id != null)
        //        {
        //            DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ "));
        //            System.IO.File.Delete(di + "/" + id);
        //        }
        //        TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = " Yevmiye Defteri Silme İşlemi Başarılı." };

        //    }
        //    catch
        //    {
        //        TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Yevmiye Defteri Silme İşleminde Hata Oluştu." };

        //    }
        //    //return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString() });
        //    return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString(), firmaId = Session["firmaId"].ToString() });
        //}

        public ActionResult SilYevmiyeDefteriKlasorden(string id)
        {
           
             try
            {
                if (id != null)
                {
                    List<DosyaBilgileri> yevmiyeDefteriListTemp = new List<DosyaBilgileri>();
                    if (Session["yevmiyeDefteriListYoneticiTemp"] != null)
                    {
                        List<DosyaBilgileri> yevmiyeDefteriListTemp1 = (List<DosyaBilgileri>)Session["yevmiyeDefteriListYoneticiTemp"];
                        yevmiyeDefteriListTemp.AddRange(yevmiyeDefteriListTemp1);
                    }

                    DosyaBilgileri dosyaBilgileriTemp = new DosyaBilgileri();
                    dosyaBilgileriTemp.dosyaAdi = id;
                    dosyaBilgileriTemp.dosyaId = id;
                    yevmiyeDefteriListTemp.Add(dosyaBilgileriTemp);
                    Session["yevmiyeDefteriListYoneticiTemp"] = yevmiyeDefteriListTemp;
                }

                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = " Yevmiye Defteri Silme İşlemi Başarılı." };

            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Yevmiye Defteri Silme İşleminde Hata Oluştu." };

            }
            //return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString() });
            return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString(), firmaId = Session["firmaId"].ToString() });
        }

        [Authorize]
        [HttpGet]
        public ActionResult DosyaIceriginiGoster(string dosyaId)
        {
            try
            {
                string path = Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ ");
                string xsltPath = Server.MapPath("~/yevmiyeler/");

                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(xsltPath + "//yevmiye.xslt");

                // Execute the transform and output the results to a file.
                var result = dosyaId.Split('.')[0];
                xslt.Transform(path + "//" + dosyaId, path + "//" + result + ".html");
                Session["htmlDosyaAdi"] = result;

                return View();

            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = " Dosyası Yükleme İşlemi Sırasında Hata Oluştu." };
                return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString() });
            }

        }

        [HttpPost]
        public ActionResult UploadFiles(HttpPostedFileBase[] files)
        {
            Session["dosyalariSilme"] = "1";
            Session["yevmiyeDefteriListYoneticiTemp"] = null;

            try
            {
               
                string path = Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ ");
                if (!Directory.Exists(path))
                {
                    DirectoryInfo folder = Directory.CreateDirectory(path);
                }
                foreach (HttpPostedFileBase file in files)
                {
                    if (file != null)
                    {
                        var InputFileName = Path.GetFileName(file.FileName);
                        var ServerSavePath = Path.Combine(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ ") + InputFileName);
                        //file.SaveAs(ServerSavePath);
                        //////////////////////
                        //Zipli ise...
                        if (InputFileName.Contains(".zip"))
                        {

                            using (ZipArchive archive = new ZipArchive(file.InputStream))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    if (!string.IsNullOrEmpty(Path.GetExtension(entry.FullName))) //make sure it's not a folder
                                    {
                                        if (entry.Name.Contains("-Y-"))//Zip içinde ne olursa olsun Sadece yevmiye defterlerini alıyorum
                                            entry.ExtractToFile(Path.Combine(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ ") + entry.Name));
                                    }
                                }
                            }
                        }

                        else if (InputFileName.Contains(".rar"))
                        {

                            using (var archive = RarArchive.Open(file.InputStream))
                            {
                                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                                {
                                    if (entry.Key.Contains("-Y-"))// Sadece yevmiye defterlerini yüklemek için
                                        entry.WriteToDirectory(Path.Combine(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString())));
                                }
                            }

                        }
                        //Zipli değilse...
                        else
                        {
                            if (file.FileName.Contains("-Y-"))// Sadece yevmiye defterlerini yüklemek için
                                file.SaveAs(ServerSavePath);
                        }

                        //////////////////////
                        TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = " Yevmiye Defterleri Başarıyla Yüklendi." };

                    }
                }
                Session["denetimBaslatBtnDisable"] = " ";//Denetim başlat butonunu aktif ediyorum
            }
            catch (Exception ex)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = " Dosyası Yükleme İşlemi Sırasında Hata Oluştu." };
            }
            return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString(), firmaId = Session["firmaId"].ToString() });
        }

        [Authorize]
        [HttpPost]
        public ActionResult DenetimBaslat(string denetimId)
        {
            KullaniciController kullaniciController = new KullaniciController();
            List<DosyaBilgileri> yevmiyeDefteriListTemp = new List<DosyaBilgileri>();
            try
            {
                DENETIM denetim = CRUD.GetirYoneticiDenetimIdIle(denetimId);

                //Sayacı başlatıyoruz...
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                //DataSet ds = DataSetOlustur();
                string path = Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ ");

                DataSet ds = new DataSet();

                DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString()));
                FileInfo[] files = di.GetFiles("*.xml");

                XElement xElem3 = new XElement("root");
                XElement xElem4 = new XElement("root");
                if (Session["yevmiyeDefteriListYoneticiTemp"] != null)
                    yevmiyeDefteriListTemp = (List<DosyaBilgileri>)Session["yevmiyeDefteriListYoneticiTemp"];
                List<DosyaBilgileri> yevmiyeDefteriList = new List<DosyaBilgileri>();
                foreach (var item in files)
                {
                    DosyaBilgileri dosyaBilgileriTemp = new DosyaBilgileri();
                    dosyaBilgileriTemp.dosyaAdi = item.Name;
                    dosyaBilgileriTemp.dosyaId = item.Name;
                    yevmiyeDefteriList.Add(dosyaBilgileriTemp);

                }
                //Yevmiye defterlerinin listelendiği sayfadan silinen dosya  varsa onları listeden çıkarmak için
                if (yevmiyeDefteriListTemp.Count > 0)
                {
                    List<DosyaBilgileri> listTemp = new List<DosyaBilgileri>();

                    listTemp.AddRange(yevmiyeDefteriList);
                    foreach (var itemTemp in yevmiyeDefteriListTemp)
                    {
                        foreach (var item in yevmiyeDefteriList)
                        {
                            if (item.dosyaAdi == itemTemp.dosyaAdi)
                                listTemp.Remove(item);
                        }

                    }
                    foreach (var itemName in listTemp)
                    {
                        XElement xElement = XElement.Load(path + "//" + itemName.dosyaAdi);
                        xElem3.Add(xElement);
                    }

                }
                else
                {
                    foreach (FileInfo file in files)
                    {
                        XElement xElement = XElement.Load(path + "//" + file.Name);
                        xElem3.Add(xElement);
                    }
                }

                XmlDocument xmlDoc = new XmlDocument();
                StringBuilder sb = new StringBuilder(xElem3.ToString());
                sb.Replace("edefter:", "");
                sb.Replace("xbrli:", "");
                sb.Replace("gl-cor:", "");
                sb.Replace("gl-bus:", "");
                sb.Replace("contextRef=\"journal_context\"", "");
                sb.Replace("xmlns:gl-bus=\"http://www.xbrl.org/int/gl/bus/2006-10-25\"", "");
                sb.Replace("xmlns:gl-cor=\"http://www.xbrl.org/int/gl/cor/2006-10-25\"", "");
                sb.ToString();

                xmlDoc.LoadXml(sb.ToString());

                XmlNode root = xmlDoc.DocumentElement;

                XmlNodeList results = root.SelectNodes("/root/defter/xbrl");
                foreach (XmlNode node in results)
                {
                    XElement x = XElement.Parse(node.OuterXml);
                    xElem4.Add(x);
                }

                XmlDocument xmlDoc1 = new XmlDocument();
                xmlDoc1.LoadXml(xElem4.ToString());
                XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc1);

                ds.ReadXml(xmlReader);

                kullaniciController.TablolariOlustur(ds);

                //Mizan tabloları oluşturuluyor...
                YIL yil = CRUD.GetirYilIdIle(denetim.YIL_LOGICALREF);
                kullaniciController.MizanlariTemizle();
                kullaniciController.MizanlariOlustur(yil.YIL_AD);
                CRUD.GuncelleDenetimDurumu(denetimId, 2);
                
                List<DENETIM_KURALLARI> denetimKurallariList = new List<DENETIM_KURALLARI>();

                List<string> denetimKuralKodList = CRUD.GetirDenetimKuralKodDenetimIdIle(denetim.LOGICALREF);
                foreach (string denetimKuralKod in denetimKuralKodList)
                {
                    denetimKurallariList.Add(CRUD.GetirDenetimKurallariKodIle(denetimKuralKod));
                }

                List<List<DENETIM_KURALLARI>> list = kullaniciController.ListeyiBol(denetimKurallariList);

                denetim.yilAd = yil.YIL_AD;
                kullaniciController.KuralUygula(denetimId, yil.YIL_AD, list);

                List<DENETIM_KURALLARI> denetimKurallariListCalismayanlar = CRUD.GetirDenetimKurallariListesiCalismayanlar(denetimId);
                while (denetimKurallariListCalismayanlar.Count > 0)
                {
                    CRUD.SilDenetimKuralCalismayanlarDenetimIdIle(denetimId);
                    List<List<DENETIM_KURALLARI>> listCalismayanlar = kullaniciController.ListeyiBol(denetimKurallariListCalismayanlar);
                    kullaniciController.KuralUygulaCalismayanlar(denetimId, yil.YIL_AD, listCalismayanlar);
                    denetimKurallariListCalismayanlar = new List<DENETIM_KURALLARI>();
                    denetimKurallariListCalismayanlar = CRUD.GetirDenetimKurallariListesiCalismayanlar(denetimId);
                }
                CRUD.GuncelleDenetimDurumu(denetimId, 3);

                //Denetime ait raporlar oluşturuluyor ve Pdf klasörüne kaydediliyor...
                RaporKaydetAdmin(denetim
                                , Sabitler.sunucuAdiW1
                                , Sabitler.veriTabaniAdiW1
                                , Sabitler.kullaniciAdiW1
                                , Sabitler.kullaniciSifreW1);

                CRUD.GuncelleDenetimDurumu(denetimId, 4);

                DefterIsimleriKaydet(denetimId);

                stopWatch.Stop();
                string gecenSure = Math.Floor((decimal)(stopWatch.ElapsedMilliseconds / 1000)).ToString();
                CRUD.GuncelleDenetimDurum(denetimId, gecenSure);

                //Denetim tamalandığında yevmiye defterleri siliniyor.
                //SilYevmiyeDefterleri(Session["firmaId"].ToString());

                //Denetim bittikten sonra denetim sonuçlarını siliyorum.Bir denetim yapılıyorken diğerini engelleyebilmek için
                CRUD.TemizleDenetimSonucList(Sabitler.sunucuAdiW1
                                            , Sabitler.veriTabaniAdiW1
                                            , Sabitler.kullaniciAdiW1
                                            , Sabitler.kullaniciSifreW1);
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = " Hata Oluştu. " + e.Message };
            }
            return RedirectToAction("YoneticiDenetimList");
        }

        public void RaporKaydetAdmin(DENETIM denetim
                                    , string sunucuAdi
                                    , string veriTabaniAdi
                                    , string kullaniciAdi
                                    , string kullaniciSifre)
        {
            try
            {
                string firmaUnvani = CRUD.GetirFirmaUnaviYevmiyeDefterinden(sunucuAdi
                                                                           , veriTabaniAdi
                                                                           , kullaniciAdi
                                                                           , kullaniciSifre);
                string yilBilgisi = CRUD.GetirYilBilgisiYevmiyeDefterinden(sunucuAdi
                                                                            , veriTabaniAdi
                                                                            , kullaniciAdi
                                                                            , kullaniciSifre);
                string raporAd = firmaUnvani + " " + yilBilgisi;
                denetim.yilAd = yilBilgisi;
                string denetimKapsamiUzun = "YÖNETİCİ DENETİMİ ("+ raporAd+")";
                string entryDetail_Id = "entryDetail_Id";
                //entryDetail_Id = entryDetail_Id + denetimKapsami;
                entryDetail_Id = entryDetail_Id + "MD"; // Şu anlık ara sayfalar için tek bir format var.
                                                        // Onun ismine de MD dedik. İlerde farklı formatlar olursa
                                                        // VD, HD, PMA diye şekillendirebiliriz...

                string mizan = "mizan";
                mizan = mizan + "MD"; // Şu anlık ara sayfalar için tek bir format var.
                                      // Onun ismine de MD dedik. İlerde farklı formatlar olursa
                                      // VD, HD, PMA diye şekillendirebiliriz...

                string detaysiz = "detaysiz";
                detaysiz = detaysiz + "MD"; // Şu anlık ara sayfalar için tek bir format var.
                                            // Onun ismine de MD dedik. İlerde farklı formatlar olursa
                                            // VD, HD, PMA diye şekillendirebiliriz...

                MsSqlConnectionParameters connectionParameters
                    = new MsSqlConnectionParameters(sunucuAdi
                                                   , veriTabaniAdi
                                                   , kullaniciAdi
                                                   , kullaniciSifre
                                                   , MsSqlAuthorizationType.SqlServer);

                
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                XtraReport report2 = XtraReport.FromFile(Server.MapPath("~/Reports/" + entryDetail_Id + ".repx"), true);
                SqlDataSource ds2 = report2.DataSource as SqlDataSource;
                ds2.ConnectionParameters = connectionParameters;

                CustomSqlQuery query1_1 = new CustomSqlQuery();
                CustomSqlQuery query1_2 = new CustomSqlQuery();
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                query1_1.Name = "CustomSqlQuery";
                string sql = @" select ed.entryDetail_Id,
                                lnc.lineNumberCounter_Text, 
                                case ed.debitCreditCode
                                when 'D' then 'BORÇ'
                                when 'C' then 'ALACAK'
                                end as debitCreditCode,
                                ed.documentDate, ed.detailComment,ed.documentNumber,
                                acc.accountMainID, acc.accountMainDescription,
                                acs.accountSubID, acs.accountSubDescription,
                                FORMAT(convert(float,amo.amount_Text), 'C', 'tr-TR') as tutar,
                                tablo.KURAL_KOD, 
                                tablo.MUSTERI_ACIKLAMA, 
                                tablo.TIP, 
                                tablo.MEVZUAT
                                from entryDetail ed
                                inner join account acc on acc.entryDetail_Id = ed.entryDetail_Id
                                inner join accountSub acs on acs.account_Id = acc.account_Id
                                inner join amount amo on amo.entryDetail_Id = ed.entryDetail_Id
                                inner join lineNumberCounter lnc on lnc.entryDetail_Id = ed.entryDetail_Id
                                inner join
                                (
                                 select value,KURAL_KOD,MUSTERI_ACIKLAMA,TIP,MEVZUAT " +
                              " from DENETIM_SONUC CROSS APPLY STRING_SPLIT(DETAY, ',') ";

                //sql = sql + " where TIP = 'entryDetail_Id' and KAPSAM like '%" + denetimKapsami + "%' ";

                sql = sql + " where TIP = 'entryDetail_Id' ";

                sql = sql + " )tablo on tablo.value = ed.entryDetail_Id ";

                query1_1.Sql = sql;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                query1_2.Name = "CustomSqlQuery_1";
                //query1_2.Sql = "select * from DENETIM_SONUC  where TIP = 'entryDetail_Id'" + " and KAPSAM like '%" + denetimKapsami + "%' ";
                query1_2.Sql = "select * from DENETIM_SONUC  where TIP = 'entryDetail_Id' ";

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                ds2.Queries.Clear();
                ds2.Queries.Add(query1_1);
                ds2.Queries.Add(query1_2);

                report2.CreateDocument(false);
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                XtraReport report3 = XtraReport.FromFile(Server.MapPath("~/Reports/" + mizan + ".repx"), true);
                SqlDataSource ds3 = report3.DataSource as SqlDataSource;
                ds3.ConnectionParameters = connectionParameters;

                CustomSqlQuery query2_1 = new CustomSqlQuery();
                CustomSqlQuery query2_2 = new CustomSqlQuery();

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///
                query2_1.Name = "CustomSqlQuery";
                //query2_1.Sql = "select * from DENETIM_SONUC where TIP = 'mizan' and KAPSAM like '%" + denetimKapsami + "%' ";
                query2_1.Sql = "select * from DENETIM_SONUC where TIP = 'mizan' ";

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                query2_2.Name = "CustomSqlQuery_1";
                string sqlNew = " select value,KURAL_KOD,MUSTERI_ACIKLAMA,MEVZUAT " +
                               " from DENETIM_SONUC CROSS APPLY STRING_SPLIT(DETAY, '_') ";

                //sqlNew = sqlNew + " where TIP = 'mizan' and KAPSAM like '%" + denetimKapsami + "%'";
                sqlNew = sqlNew + " where TIP = 'mizan' ";

                query2_2.Sql = sqlNew;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                ds3.Queries.Clear();
                ds3.Queries.Add(query2_1);
                ds3.Queries.Add(query2_2);
                report3.CreateDocument(false);
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                XtraReport report4 = XtraReport.FromFile(Server.MapPath("~/Reports/" + detaysiz + ".repx"), true);
                SqlDataSource ds4 = report4.DataSource as SqlDataSource;
                ds4.ConnectionParameters = connectionParameters;

                CustomSqlQuery query3_1 = new CustomSqlQuery();
                query3_1.Name = "CustomSqlQuery";
                //query3_1.Sql = "select KURAL_KOD, MUSTERI_ACIKLAMA, MEVZUAT from DENETIM_SONUC where TIP = '' and KAPSAM like '%" + denetimKapsami + "%'";
                query3_1.Sql = "select KURAL_KOD, MUSTERI_ACIKLAMA, MEVZUAT from DENETIM_SONUC where TIP = '' ";

                ds4.Queries.Clear();
                ds4.Queries.Add(query3_1);

                report4.CreateDocument(false);
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
               
                XRWatermark watermark2 = new XRWatermark();
                watermark2.CopyFrom(report2.Watermark);

                XRWatermark watermark3 = new XRWatermark();
                watermark3.CopyFrom(report3.Watermark);

                XRWatermark watermark4 = new XRWatermark();
                watermark4.CopyFrom(report4.Watermark);

                int mainPageCount = report2.Pages.Count;
                int pageCount3 = report3.Pages.Count;
                int pageCount4 = report4.Pages.Count;

                report2.Pages.AddRange(report3.Pages);
                report2.PrintingSystem.ContinuousPageNumbering = true;
                report2.Pages.AddRange(report4.Pages);
                report2.PrintingSystem.ContinuousPageNumbering = true;

                for (int i = mainPageCount; i < mainPageCount + pageCount3; i++)
                {
                    report2.Pages[i].AssignWatermark(watermark2);
                }

                for (int i = mainPageCount + pageCount3; i < mainPageCount + pageCount3 + pageCount4; i++)
                {
                    report2.Pages[i].AssignWatermark(watermark3);
                }

                string pdfExportFile = Server.MapPath("~/Pdf/" + denetimKapsamiUzun + ".pdf");
                PdfExportOptions pdfExportOptions = new PdfExportOptions() { PdfACompatibility = PdfACompatibility.PdfA1b };
                report2.ExportToPdf(pdfExportFile, pdfExportOptions);

                string fileName = Path.GetFileName(pdfExportFile);
                string fileExtension = Path.GetExtension(pdfExportFile);
                byte[] fileBytes = System.IO.File.ReadAllBytes(pdfExportFile);

                RAPOR rapor = new RAPOR();
                Guid objYeni = Guid.NewGuid();
                rapor.LOGICALREF = objYeni.ToString();
                rapor.AD = fileName;
                rapor.CONTENT = fileBytes;
                rapor.CONTENT_TYPE = fileExtension;
                rapor.KULLANIM_DURUMU = 1;
                CRUD.KaydetRapor(rapor);

                DENETIM_RAPOR denetimRapor = new DENETIM_RAPOR();
                Guid objYeni2 = Guid.NewGuid();
                denetimRapor.DENETIM_LOGICALREF = denetim.LOGICALREF;
                denetimRapor.RAPOR_LOGICALREF = objYeni.ToString();
                denetimRapor.LOGICALREF = objYeni2.ToString();
                denetimRapor.KULLANIM_DURUMU = 1;
                CRUD.KaydetDenetimRapor(denetimRapor);
            }
            catch (Exception ex)
            {
                CRUD.WriteLog(denetim.LOGICALREF, ex.Message.ToString(), "RaporKaydetAdmin");
            }
        }

        [Authorize]
        public ActionResult YoneticiDenetimList()
        {
            if (Session["menuGoster"] != null)//Admin değilse adres çubuğundan adminin menülerine ulaşmasın.
                if (Session["menuGoster"].ToString().Equals("hidden"))
                    return RedirectToAction("HomeIndex", "Home");
            Session["dosyalariSilme"] = "1";
            Session["yevmiyeDefteriListYoneticiTemp"] = null;//Dosyaları kalıcı olarak silmemek için kullandığımız değişken
            List<DENETIM> denetimList = new List<DENETIM>();
            try
            {
                denetimList = CRUD.GetirYoneticiDenetimList();             
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Yönetici Denetim Listesi Getirilirken  Hata Oluştu." };
            }
            return View(denetimList);
        }

        [Authorize]
        public ActionResult Rapor(string denetimId)
        {
            List<RAPOR> raporList = new List<RAPOR>();
            List<DENETIM_RAPOR> denetimRaporList = new List<DENETIM_RAPOR>();
            try
            {
                DENETIM denetim = CRUD.GetirYoneticiDenetimIdIle(denetimId);
                Session["denetimAd"] = denetim.AD;

                denetimRaporList = CRUD.GetirDenetimRaporListDenetimIdIle(denetimId);
                foreach (DENETIM_RAPOR denetimRapor in denetimRaporList)
                {
                    RAPOR rapor = new RAPOR();
                    rapor = CRUD.GetirRaporRaporIdIle(denetimRapor.RAPOR_LOGICALREF);
                    raporList.Add(rapor);
                }
                if (raporList.Count > 0)
                {
                    //Yevmiye defterlerini raporlar sayfasında göstermek için raporliste setliyoum
                    raporList[0].yevmiyeDefterleriList = new List<DENETIM_YEVMIYE_DEFTER_AD>();
                    raporList[0].yevmiyeDefterleriList = CRUD.GetirYevmiyDefterAdListesiDenetimIdIle(denetim.LOGICALREF);
                }
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Rapor Getirilirken Hata Oluştu." };
            }
            return View(raporList);
        }

        [HttpGet]
        public FileResult IndirRapor(string raporId)
        {
            try
            {
                RAPOR rapor = new RAPOR();
                rapor = CRUD.GetirRaporRaporIdIleIndirmekIcin(raporId);
                return File(rapor.CONTENT, "application/pdf", rapor.AD);
            }
            catch (Exception exc)
            {
                CRUD.WriteLog("denetimId", exc.Message.ToString(), "IndirRapor");
                return null;
            }
        }

        public void DefterIsimleriKaydet(string denetimId)
        {
            List<DosyaBilgileri> yevmiyeDefteriListTemp = new List<DosyaBilgileri>();
            try
            {
                string path = Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ ");
                if (Directory.Exists(path))
                {
                    DirectoryInfo dir = new DirectoryInfo(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ "));
                    FileInfo[] filess = dir.GetFiles("*.xml");

                    if (Session["yevmiyeDefteriListYoneticiTemp"] != null)
                        yevmiyeDefteriListTemp = (List<DosyaBilgileri>)Session["yevmiyeDefteriListYoneticiTemp"];
                    List<DosyaBilgileri> yevmiyeDefteriList = new List<DosyaBilgileri>();
                    foreach (var item in filess)
                    {
                        DosyaBilgileri dosyaBilgileriTemp = new DosyaBilgileri();
                        dosyaBilgileriTemp.dosyaAdi = item.Name;
                        dosyaBilgileriTemp.dosyaId = item.Name;
                        yevmiyeDefteriList.Add(dosyaBilgileriTemp);

                    }
                    //Yevmiye defterlerinin listelendiği sayfadan silinen dosya  varsa onları listeden çıkarmak için
                    if (yevmiyeDefteriListTemp.Count > 0)
                    {
                        List<DosyaBilgileri> listTemp = new List<DosyaBilgileri>();

                        listTemp.AddRange(yevmiyeDefteriList);
                        foreach (var itemTemp in yevmiyeDefteriListTemp)
                        {
                            foreach (var item in yevmiyeDefteriList)
                            {
                                if (item.dosyaAdi == itemTemp.dosyaAdi)
                                    listTemp.Remove(item);
                            }

                        }
                        foreach (var itemName in listTemp)
                        {
                            DENETIM_YEVMIYE_DEFTER_AD yevmiyeDefteriAd = new DENETIM_YEVMIYE_DEFTER_AD();
                            Guid objYeni3 = Guid.NewGuid();
                            yevmiyeDefteriAd.LOGICALREF = objYeni3.ToString();
                            yevmiyeDefteriAd.YEVMIYE_DEFTER_AD = itemName.dosyaAdi;
                            yevmiyeDefteriAd.DENETIM_LOGICALREF = denetimId;
                            CRUD.YevmiyeDefteriAdEkle(yevmiyeDefteriAd);
                        }

                    }
                    else
                    {
                       
                            foreach (var item in filess)
                            {
                                DENETIM_YEVMIYE_DEFTER_AD yevmiyeDefteriAd = new DENETIM_YEVMIYE_DEFTER_AD();
                                Guid objYeni3 = Guid.NewGuid();
                                yevmiyeDefteriAd.LOGICALREF = objYeni3.ToString();
                                yevmiyeDefteriAd.YEVMIYE_DEFTER_AD = item.Name;
                                yevmiyeDefteriAd.DENETIM_LOGICALREF = denetimId;
                                CRUD.YevmiyeDefteriAdEkle(yevmiyeDefteriAd);
                            }
                    }


               
                }
            }
            catch (Exception ex)
            {

            }
        }

        public ActionResult SilYevmiyeDefterleri()
        {
           
            try
            {
                string path = Server.MapPath("~/yevmiyeler/" + @Session["firmaId"].ToString() + "/ ");
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path);
                    foreach (string file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                    Session["yevmiyeDefteriListYoneticiTemp"] = null;
                }
            }
            catch (Exception e)
            {
            }
            return null;
        }
    }
}