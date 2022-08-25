using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DKN.Models;
using DKN.db;
using System.IO;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.Text;
using DKN.parametreler;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using System.Drawing.Printing;
using System.IO.Compression;
using System.Drawing;
using System.Web.Helpers;
using Iyzipay.Request;
using Iyzipay.Model;
using Iyzipay;
using System.Web.Security;
using System.Web.Routing;
using System.Net.Mail;
using RestSharp;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace DKN.Controllers
{

    public class KullaniciController : Controller
    {

        // GET: Kullanici
        [Authorize]
        public ActionResult IndexKullanici()
        {
            List<FIRMA> firmaListesi = new List<FIRMA>();
            //firmaListesi = CRUD.GetirFirmaListesi();
            firmaListesi = CRUD.GetirKullaniciFirmaListKullaniciIdIle(Session["LOGICALREF"].ToString());

            /*
            foreach (var firma in firmaListesi)
            {
                firma.denetimList = new List<DENETIM>();

                List<FIRMA_DENETIM> firmaDenetimListesi = CRUD.GetirFirmaDenetimListesiFirmaIdIle(firma.LOGICALREF);
                foreach (FIRMA_DENETIM firmaDenetim in firmaDenetimListesi)
                {
                    DENETIM denetim = CRUD.GetirDenetimIdIle(firmaDenetim.DENETIM_LOGICALREF);             
                    YIL yil = CRUD.GetirYilIdIle(denetim.YIL_LOGICALREF);
                    denetim.yilAd = yil.YIL_AD;
                    firma.denetimList.Add(denetim);
                }
            }
            */
            return View(firmaListesi);
        }

        [Authorize]
        public ActionResult CreateFirma()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateFirma(HttpPostedFileBase[] uploadImages, FIRMA firma)
        {
            int resimSize = 0;
            try
            {
                if (ModelState.IsValid)
                {
                    Guid obj = Guid.NewGuid();
                    firma.LOGICALREF = obj.ToString();

                    if (uploadImages[0] != null)
                    {
                        foreach (var image in uploadImages)
                        {
                            if (image.ContentLength > 0)
                            {
                                //resimSize = image.ContentLength;
                                byte[] imageData = null;
                                //using (var binaryReader = new BinaryReader(image.InputStream))
                                //{
                                //    imageData = binaryReader.ReadBytes(image.ContentLength);

                                //    firma.LOGO = imageData;
                                //}
                                WebImage img = new WebImage(image.InputStream);
                                if (img.Width > 200)
                                    img.Resize(200, 200);
                                imageData = img.GetBytes();
                                firma.LOGO = imageData;
                            }
                            //if (resimSize <= 150 * 1024)
                            //{
                            //CRUD.FirmaEkle(firma);
                            //Guid obj2 = Guid.NewGuid();
                            //string kullaniciFirmaLogicalref = obj2.ToString();
                            //CRUD.KullaniciFirmaEkle(firma.LOGICALREF, kullaniciFirmaLogicalref, Session["LOGICALREF"].ToString());
                            //TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Firma Kaydı Başarılı." };
                            //return RedirectToAction("IndexKullanici");
                            //}
                            //else
                            //{
                            //    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Resim boyutu 150 KB(KiloByte)'tan küçük olmalıdır." };
                            //    return CreateFirma();
                            //}
                        }
                    }
                    //else
                    //{
                    CRUD.FirmaEkle(firma);
                    Guid obj2 = Guid.NewGuid();
                    string kullaniciFirmaLogicalref = obj2.ToString();
                    CRUD.KullaniciFirmaEkle(firma.LOGICALREF, kullaniciFirmaLogicalref, Session["LOGICALREF"].ToString());
                    TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Firma Kaydı Başarılı." };
                    return RedirectToAction("IndexKullanici");
                    //}

                }
                else
                    return View();

            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Firma Kaydı Sırasında Hata Oluştu." };
                return RedirectToAction("IndexKullanici");
            }
            return View();
        }

        [Authorize]
        public ActionResult CreateDenetim()
        {
            DENETIM denetim = new DENETIM();
            denetim.altKapsamList = new List<ALT_KAPSAM>();
            denetim.donemList = CRUD.GetirDonemAyListesiTurIle(1);//1 Dönem
            denetim.ayList = CRUD.GetirDonemAyListesiTurIle(2);//2 Ay
            denetim.kapsamList = CRUD.GetirKapsamListesiDenetimIcin();
            denetim.selectedKapsamList = new List<KAPSAM>();
            //denetim.altKapsamList = CRUD.GetirAltKapsamListesi();
            //denetim.selectedAltKapsamList = new List<ALT_KAPSAM>();
            denetim.altKapsamListAVD = CRUD.GetirAltKapsamListesiKapsamIdIle(CRUD.GetirKapsamKısaAdIle("AVD").LOGICALREF);
            denetim.altKapsamListGVD = CRUD.GetirAltKapsamListesiKapsamIdIle(CRUD.GetirKapsamKısaAdIle("GVD").LOGICALREF);
            denetim.altKapsamListHD = CRUD.GetirAltKapsamListesiKapsamIdIle(CRUD.GetirKapsamKısaAdIle("HD").LOGICALREF);
            denetim.altKapsamListPD = CRUD.GetirAltKapsamListesiKapsamIdIle(CRUD.GetirKapsamKısaAdIle("PD").LOGICALREF);
            denetim.yilList = CRUD.GetirYilListesi();

            return View(denetim);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateDenetim(DENETIM denetim, string gvd, string avd, string hd, string pma)
        {
            try
            {
                KAPSAM kapsam = new KAPSAM();
                List<KAPSAM> kapsamList = new List<KAPSAM>();
                Guid obj = Guid.NewGuid();
                denetim.LOGICALREF = obj.ToString();
                denetim.TARIH = DateTime.Now;
                var errors = ModelState
                              .Where(x => x.Value.Errors.Count > 0)
                              .Select(x => new { x.Key, x.Value.Errors })
                              .ToArray();


                if (!string.IsNullOrEmpty(denetim.AD))
                {

                    if (gvd != null || avd != null)
                    {
                        if (!string.IsNullOrEmpty(denetim.DONEM_AY_LOGICALREF))
                        {
                            if (gvd != null)
                            {
                                 kapsam = CRUD.GetirKapsamKısaAdIle("GVD");
                                kapsamList.Add(kapsam);
                                foreach (ALT_KAPSAM altKapsam in denetim.altKapsamListGVD)
                                {
                                    if (altKapsam.IsCheck)//seçilen altkapsamları alıyorum
                                    {
                                        DENETIM_ALT_KAPSAM denetimAltKapsam = new DENETIM_ALT_KAPSAM();
                                        Guid objYeni = Guid.NewGuid();
                                        denetimAltKapsam.LOGICALREF = objYeni.ToString();
                                        denetimAltKapsam.DENETIM_LOGICALREF = denetim.LOGICALREF;
                                        denetimAltKapsam.ALT_KAPSAM_LOGICALREF = altKapsam.LOGICALREF;
                                        CRUD.DenetimAltKapsamEkle(denetimAltKapsam);
                                    }

                                }
                            }
                            else if (avd != null)
                            {
                                 kapsam = CRUD.GetirKapsamKısaAdIle("AVD");
                                kapsamList.Add(kapsam);
                                foreach (ALT_KAPSAM altKapsam in denetim.altKapsamListAVD)
                                {
                                    if (altKapsam.IsCheck)//seçilen altkapsamları alıyorum
                                    {
                                        DENETIM_ALT_KAPSAM denetimAltKapsam = new DENETIM_ALT_KAPSAM();
                                        Guid objYeni = Guid.NewGuid();
                                        denetimAltKapsam.LOGICALREF = objYeni.ToString();
                                        denetimAltKapsam.DENETIM_LOGICALREF = denetim.LOGICALREF;
                                        denetimAltKapsam.ALT_KAPSAM_LOGICALREF = altKapsam.LOGICALREF;
                                        CRUD.DenetimAltKapsamEkle(denetimAltKapsam);
                                    }

                                }
                            }
                        }
                        else
                        {
                            TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Denetimin Dönem/Ay Bilgisini Seçiniz." };

                            return RedirectToAction("CreateDenetim");
                        }
                    }

                    if (hd != null)
                    {
                         kapsam = CRUD.GetirKapsamKısaAdIle("HD");
                        kapsamList.Add(kapsam);
                        foreach (ALT_KAPSAM altKapsam in denetim.altKapsamListHD)
                        {
                            if (altKapsam.IsCheck)//seçilen altkapsamları alıyorum
                            {
                                DENETIM_ALT_KAPSAM denetimAltKapsam = new DENETIM_ALT_KAPSAM();
                                Guid objYeni = Guid.NewGuid();
                                denetimAltKapsam.LOGICALREF = objYeni.ToString();
                                denetimAltKapsam.DENETIM_LOGICALREF = denetim.LOGICALREF;
                                denetimAltKapsam.ALT_KAPSAM_LOGICALREF = altKapsam.LOGICALREF;
                                CRUD.DenetimAltKapsamEkle(denetimAltKapsam);
                            }

                        }
                        denetim.DONEM_AY_LOGICALREF = "";
                    }
                    else if (pma != null)
                    {
                         kapsam = CRUD.GetirKapsamKısaAdIle("PD");
                        kapsamList.Add(kapsam);
                        foreach (ALT_KAPSAM altKapsam in denetim.altKapsamListPD)
                        {
                            if (altKapsam.IsCheck)//seçilen altkapsamları alıyorum
                            {
                                DENETIM_ALT_KAPSAM denetimAltKapsam = new DENETIM_ALT_KAPSAM();
                                Guid objYeni = Guid.NewGuid();
                                denetimAltKapsam.LOGICALREF = objYeni.ToString();
                                denetimAltKapsam.DENETIM_LOGICALREF = denetim.LOGICALREF;
                                denetimAltKapsam.ALT_KAPSAM_LOGICALREF = altKapsam.LOGICALREF;
                                CRUD.DenetimAltKapsamEkle(denetimAltKapsam);
                            }

                        }
                        denetim.DONEM_AY_LOGICALREF = "";
                    }

                    CRUD.DenetimEkle(denetim);
                    foreach (KAPSAM kapsam1 in kapsamList)
                    {
                        DENETIM_KAPSAM denetimKapsam = new DENETIM_KAPSAM();
                        Guid objYeni = Guid.NewGuid();
                        denetimKapsam.LOGICALREF = objYeni.ToString();
                        denetimKapsam.DENETIM_LOGICALREF = denetim.LOGICALREF;
                        denetimKapsam.KAPSAM_LOGICALREF = kapsam1.LOGICALREF;
                        CRUD.DenetimKapsamEkle(denetimKapsam);
                    }
                   
                    //foreach (ALT_KAPSAM altKapsam in denetim.altKapsamList)
                    //{
                    //    if (altKapsam.IsCheck)//seçilen altkapsamları alıyorum
                    //    {
                    //        DENETIM_ALT_KAPSAM denetimAltKapsam = new DENETIM_ALT_KAPSAM();
                    //        Guid objYeni = Guid.NewGuid();
                    //        denetimAltKapsam.LOGICALREF = objYeni.ToString();
                    //        denetimAltKapsam.DENETIM_LOGICALREF = denetim.LOGICALREF;
                    //        denetimAltKapsam.ALT_KAPSAM_LOGICALREF = altKapsam.LOGICALREF;
                    //        CRUD.DenetimAltKapsamEkle(denetimAltKapsam);
                    //    }

                    //}
                    FIRMA_DENETIM firmaDenetim = new FIRMA_DENETIM();
                    Guid objYeni2 = Guid.NewGuid();
                    firmaDenetim.LOGICALREF = objYeni2.ToString();
                    firmaDenetim.DENETIM_LOGICALREF = denetim.LOGICALREF;
                    firmaDenetim.FIRMA_LOGICALREF = Session["firmaId"].ToString();
                    CRUD.FirmaDenetimEkle(firmaDenetim, 1);//1 kullanıcı firma denetimi 9 yönetici denetimi

                    //DENETİM KAYDEDİP SONRASINDA BU CENETİMDE KAÇ KURAL ÇALIŞTIĞI BİLGİSİ ÇEKİLİP dENETİM TABLOSU GÜNCELLENİYOR
                    denetim.KURAL_SAYISI = CRUD.GetirToplamKuralSayisiSayisiDenetimIdveLKapsamIle(denetim.LOGICALREF,kapsam.LOGICALREF);
                    CRUD.GuncelleDenetimKuralSayisi(denetim);

                    TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Denetim Kaydı Başarılı." };
                    return RedirectToAction("FirmaDenetimleri", new { firmaId = Session["firmaId"].ToString() });

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
                return RedirectToAction("FirmaDenetimleri", new { firmaId = Session["firmaId"].ToString() });
            }
        }

        [Authorize]
        public ActionResult EditFirma(string id)
        {
            Session["firmaId"] = id;
            FIRMA kayitliFirma = new FIRMA();
            try
            {
                kayitliFirma = CRUD.GetirFirmaIdIle(id);

                if (kayitliFirma.KULLANIM_DURUMU == 1)
                {
                    kayitliFirma.kullanimDurumuBool = true;
                }
                else
                {
                    kayitliFirma.kullanimDurumuBool = false;
                }
            }
            catch (Exception e)
            {

            }
            return View("EditFirma", kayitliFirma);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditFirma(HttpPostedFileBase[] uploadImages, string id, FIRMA firma)
        {
            firma.LOGICALREF = id;
            int resimSize = 0;
            try
            {
                if (firma.kullanimDurumuBool)
                {
                    firma.KULLANIM_DURUMU = 1;
                }
                else
                {
                    firma.KULLANIM_DURUMU = 0;
                }
                if (uploadImages[0] != null)
                    foreach (var image in uploadImages)
                    {
                        if (image.ContentLength > 0)
                        {
                            //resimSize = image.ContentLength;
                            byte[] imageData = null;
                            //using (var binaryReader = new BinaryReader(image.InputStream))
                            //{
                            //    imageData = binaryReader.ReadBytes(image.ContentLength);

                            //    firma.LOGO = imageData;
                            //}

                            WebImage img = new WebImage(image.InputStream);
                            if (img.Width > 200)
                                img.Resize(200, 200);
                            imageData = img.GetBytes();
                            firma.LOGO = imageData;

                        }
                    }
                //if (resimSize <= 150 * 1024)
                //{
                CRUD.GuncelleFirma(firma);
                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Güncelleme İşlemi Başarılı." };
                return RedirectToAction("IndexKullanici");
                //}
                //else
                //{
                //    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Resim boyutu 150 KB(KiloByte)'tan küçük olmalıdır." };
                //    return EditFirma(id);
                //}


            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Güncelleme İşlemi Sırasında Hata Oluştu." };
                return EditFirma(id);
            }

        }

        [Authorize]
        public ActionResult EditDenetim(string id)
        {
            List<DosyaBilgileri> yevmiyeDefteriListTemp = new List<DosyaBilgileri>();
            Session["denetimBaslatBtnDisable"] = "disabled";
            Session["denetimId"] = id;
            Session["gecenSure"] = "0";
            //sadeedevam eden bir denetim olduğunda mesaj verebilmek için
            Session["devamEdenDenetimVarMi"] = "0";
            string path = Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ ");
            DENETIM denetim = new DENETIM();

            try
            {
                if (id != null)
                {

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

                    denetim = CRUD.GetirDenetimIdIle(id);
                    YIL yil = CRUD.GetirYilIdIle(denetim.YIL_LOGICALREF);
                    denetim.yilAd = yil.YIL_AD;
                    //denetim.kapsamList = CRUD.GetirKapsamListesi();
                    //denetim.kapsamList = CRUD.GetirKapsamListesiDenetimIcin();
                    List<DENETIM_KAPSAM> denetimKapsamList = CRUD.GetirDenetimKapsamListesiDenetimIdIle(id);
                    denetim.selectedKapsamList = new List<KAPSAM>();
                    denetim.selectedAltKapsamList = new List<ALT_KAPSAM>();
                    denetim.yevmiyeDefteriList = new List<DosyaBilgileri>();
                    foreach (DENETIM_KAPSAM denetimKapsam in denetimKapsamList)
                    {
                        KAPSAM kapsam = CRUD.GetirKapsamIdIle(denetimKapsam.KAPSAM_LOGICALREF);
                        denetim.selectedKapsamList.Add(kapsam);
                    }
                    List<DENETIM_ALT_KAPSAM> denetimAltKapsamList = CRUD.GetirDenetimAltKapsamListesiDenetimIdIle(id);
                    foreach (DENETIM_ALT_KAPSAM denetimAltKapsam in denetimAltKapsamList)
                    {
                        ALT_KAPSAM altKapsam = CRUD.GetirAltKapsamIdIle(denetimAltKapsam.ALT_KAPSAM_LOGICALREF);
                        denetim.selectedAltKapsamList.Add(altKapsam);
                    }
                    //iterating through multiple file collection   

                    if (Directory.Exists(path))
                    {
                        DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ "));
                        FileInfo[] files = di.GetFiles("*.xml");
                        if (files.Count() > 0)
                            Session["denetimBaslatBtnDisable"] = "";//Denetim başlatma butonu aktif ediliyor
                                                                    // IEnumerable<string> yevmiyeList = Directory.GetFiles(Server.MapPath("~/yevmiyeler"), "*.xml");
                        if (Session["yevmiyeDefteriListTemp"] != null)
                            yevmiyeDefteriListTemp = (List<DosyaBilgileri>)Session["yevmiyeDefteriListTemp"];

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

                int devamEdenDenetimSayisi = CRUD.GetirDevamEdenDenetimSayisi();

                if (devamEdenDenetimSayisi == 0)
                {
                    Session["devamEdenDenetimVarMi"] = "0";
                }
                else
                {
                    Session["devamEdenDenetimVarMi"] = "1";
                }
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Güncelleme İşlemi Sırasında Hata Oluştu." };
            }

            return View(denetim);
        }

        [Authorize]
        public ActionResult GuncelleFirmaDurum(string id)
        {
            FIRMA firma = new FIRMA();
            int kullaniciFirmaKullanimDurumu = 0;
            try
            {
                if (id != null)
                {
                    firma = CRUD.GetirFirmaIdIle(id);
                }
                if (firma.kullanimDurumuBool)
                {
                    firma.KULLANIM_DURUMU = 1;
                    kullaniciFirmaKullanimDurumu = 1;
                }
                else if (!firma.kullanimDurumuBool)
                {
                    firma.KULLANIM_DURUMU = 0;
                    kullaniciFirmaKullanimDurumu = 0;
                }

                CRUD.GuncelleFirma(firma);
                CRUD.GuncelleKullaniciFirmaKullanimDurumu(firma.LOGICALREF, Session["LOGICALREF"].ToString(), kullaniciFirmaKullanimDurumu);
                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Durum Güncelleme Başarılı." };
                return RedirectToAction("IndexKullanici");
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Durum Güncelleme Sırasında Hata Oluştu." };
                return RedirectToAction("IndexKullanici");
            }

        }

        [Authorize]
        public ActionResult GuncelleDenetimDurum(string id)
        {
            DENETIM denetim = new DENETIM();
            try
            {
                if (id != null)
                {
                    denetim = CRUD.GetirDenetimIdIle(id);
                }
                if (denetim.KULLANIM_DURUMU == 1)
                {
                    denetim.KULLANIM_DURUMU = 0;
                    CRUD.GuncelleFirmaDenetim(denetim.LOGICALREF, 0);
                }
                else if (denetim.KULLANIM_DURUMU == 0)
                {
                    denetim.KULLANIM_DURUMU = 1;
                    CRUD.GuncelleFirmaDenetim(denetim.LOGICALREF, 1);
                }

                CRUD.GuncelleDenetim(denetim);

                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Denetim Güncelleme Başarılı." };
                return RedirectToAction("IndexKullanici");
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Denetim Güncelleme Sırasında Hata Oluştu." };
                return RedirectToAction("IndexKullanici");
            }

        }

        [Authorize]
        public ActionResult FirmaDenetimleri(string firmaId)
        {
            Session["yevmiyeDefteriListTemp"] = null;//Dosyaları kalıcı olarak silmemek için kullandığımız değişken

            //Session["dosyalariSilme"] = null;
            Session["dosyalariSilme"] = "1";// Dosyalar silinmek istemediği için bu şeklide yaptık
            Session["firmaId"] = firmaId;
            FIRMA firma = new FIRMA();
            try
            {
                firma = CRUD.GetirFirmaIdIle(firmaId);
                firma.denetimList = new List<DENETIM>();
                Session["firmaUnvan"] = firma.UNVAN;
                Session["firmaTcknVkn"] = firma.VKNTCKN;
                List<FIRMA_DENETIM> firmaDenetimListesi = CRUD.GetirFirmaDenetimListesiFirmaIdIle(firmaId);
                foreach (FIRMA_DENETIM firmaDenetim in firmaDenetimListesi)
                {
                    DENETIM denetim = CRUD.GetirDenetimIdIle(firmaDenetim.DENETIM_LOGICALREF);
                    if (!string.IsNullOrEmpty(denetim.SURE))
                    {
                        decimal dakika = Math.Floor(Convert.ToDecimal(Convert.ToInt32(denetim.SURE) / 60));
                        int saniye = Convert.ToInt32(denetim.SURE) % 60;

                        var saniyeString = "";
                        if (saniye < 10)
                        { saniyeString = "0" + saniye.ToString(); }
                        else
                        { saniyeString = saniye.ToString(); }

                        denetim.SURE = dakika.ToString() + ":" + saniyeString.ToString();
                    }
                    YIL yil = CRUD.GetirYilIdIle(denetim.YIL_LOGICALREF);
                    denetim.yilAd = yil.YIL_AD;
                    denetim.kapsamList = new List<KAPSAM>();
                    denetim.altKapsamList = new List<ALT_KAPSAM>();
                    denetim.yevmiyeDefterAdList = new List<DENETIM_YEVMIYE_DEFTER_AD>();
                    //denetim.yevmiyeDefterAdList = CRUD.GetirYevmiyDefterAdListesiDenetimIdIle(denetim.LOGICALREF);
                    if (denetim.LOGICALREF != null)
                    {
                        List<DENETIM_KAPSAM> denetimKapsamList = CRUD.GetirDenetimKapsamListesiDenetimIdIle(denetim.LOGICALREF);
                        foreach (DENETIM_KAPSAM denetimKapsam in denetimKapsamList)
                        {
                            KAPSAM kapsam = CRUD.GetirKapsamIdIle(denetimKapsam.KAPSAM_LOGICALREF);
                            denetim.kapsamList.Add(kapsam);
                        }
                        List<DENETIM_ALT_KAPSAM> denetimAltKapsamList = CRUD.GetirDenetimAltKapsamListesiDenetimIdIle(denetim.LOGICALREF);
                        foreach (DENETIM_ALT_KAPSAM denetimAltKapsam in denetimAltKapsamList)
                        {
                            ALT_KAPSAM altKapsam = CRUD.GetirAltKapsamIdIle(denetimAltKapsam.ALT_KAPSAM_LOGICALREF);
                            denetim.altKapsamList.Add(altKapsam);
                        }
                        //kapsamList çekilecek...
                        firma.denetimList.Add(denetim);
                    }
                }
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Firma Denetimleri Getirme İşleminde Hata Oluştu." };
            }
            return View(firma);
        }

        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public ActionResult EditDenetim(string id, DENETIM denetim)
        //{
        //    denetim.LOGICALREF = id;
        //    try
        //    {

        //        denetim.TARIH = DateTime.Now;
        //        if (denetim.KULLANIM_DURUMU == 1)
        //        {
        //            denetim.KULLANIM_DURUMU = 0;
        //        }
        //        else if (denetim.KULLANIM_DURUMU == 0)
        //        {
        //            denetim.KULLANIM_DURUMU = 1;
        //        }
        //        CRUD.GuncelleDenetim(denetim);
        //        CRUD.SilDenetimKapsamDenetimLogicalRefIle(denetim.LOGICALREF);
        //        foreach (string kapsamId in denetim.postedIds.kapsamIds)
        //        {
        //            DENETIM_KAPSAM denetimKapsam = new DENETIM_KAPSAM();
        //            Guid objYeni = Guid.NewGuid();
        //            denetimKapsam.LOGICALREF = objYeni.ToString();
        //            denetimKapsam.DENETIM_LOGICALREF = denetim.LOGICALREF;
        //            denetimKapsam.KAPSAM_LOGICALREF = kapsamId;
        //            CRUD.DenetimKapsamEkle(denetimKapsam);
        //        }

        //        TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Güncelleme İşlemi Başarılı." };
        //        return RedirectToAction("FirmaDenetimleri", new { firmaId = Session["firmaId"].ToString() });

        //    }
        //    catch
        //    {
        //        TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Güncelleme İşlemi Sırasında Hata Oluştu." };
        //        return RedirectToAction("FirmaDenetimleri", new { firmaId = Session["firmaId"].ToString() });
        //    }
        //}

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
        //    return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString() });
        //}


        public ActionResult SilYevmiyeDefteriKlasorden(string id)
        {
            try
            {
                if (id != null)
                {
                    List<DosyaBilgileri> yevmiyeDefteriListTemp = new List<DosyaBilgileri>();
                    if (Session["yevmiyeDefteriListTemp"] != null)
                    {
                        List<DosyaBilgileri> yevmiyeDefteriListTemp1 = (List<DosyaBilgileri>)Session["yevmiyeDefteriListTemp"];
                        yevmiyeDefteriListTemp.AddRange(yevmiyeDefteriListTemp1);
                    }

                    DosyaBilgileri dosyaBilgileriTemp = new DosyaBilgileri();
                    dosyaBilgileriTemp.dosyaAdi = id;
                    dosyaBilgileriTemp.dosyaId = id;
                    yevmiyeDefteriListTemp.Add(dosyaBilgileriTemp);
                    Session["yevmiyeDefteriListTemp"] = yevmiyeDefteriListTemp;
                }

                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = " Yevmiye Defteri Silme İşlemi Başarılı." };

            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Yevmiye Defteri Silme İşleminde Hata Oluştu." };

            }
            return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString() });
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
                return RedirectToAction("EditDenetim", new { firmaId = Session["denetimId"].ToString() });
            }

        }

        [HttpPost]
        public ActionResult YevmiyeDosyasiKaydet(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    string path = Path.Combine(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ "),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Yevmiye Dosyası Yükleme Başarılı." };
                }
                catch (Exception ex)
                {
                    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Veri Getirilirken Hata Oluştu." };
                }
            }
            else
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = " Dosyası Yükleme İşlemi Sırasında Hata Oluştu." };

            }

            return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString() });
        }

        [HttpPost]
        public ActionResult UploadFiles(HttpPostedFileBase[] files)
        {
            Session["dosyalariSilme"] = "1";
            Session["yevmiyeDefteriListTemp"] = null;
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
                            if (file.FileName.Contains("-Y-"))
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
            return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString() });
        }

        [Authorize]
        [HttpPost]
        public ActionResult DenetimBaslat(string denetimId)
        {
            List<DosyaBilgileri> yevmiyeDefteriListTemp = new List<DosyaBilgileri>();
            string raporAd = "";
            try
            {
                DENETIM denetim = CRUD.GetirDenetimIdIle(denetimId);

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

                if (Session["yevmiyeDefteriListTemp"] != null)
                    yevmiyeDefteriListTemp = (List<DosyaBilgileri>)Session["yevmiyeDefteriListTemp"];
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

                TablolariOlustur(ds);

                //XML'ler; veri tabanı tablolarına dönüşüyor...

                //Eski
                #region

                /*
                DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString()));
                FileInfo[] files = di.GetFiles("*.xml");

                XElement xElem3 = new XElement("root");
                XElement xElem4 = new XElement("root");

                foreach (FileInfo file in files)
                {
                    XElement xElement = XElement.Load(path + "//" + file.Name);
                    xElem3.Add(xElement);
                }

                XmlDocument xmlDoc = new XmlDocument();

                //////////////////////////////////////////////////////////////////
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

                //xmlDoc.LoadXml(xElem3.ToString().Replace("edefter:", "")
                //                        .Replace("xbrli:", "")
                //                        .Replace("gl-cor:", "")
                //                        .Replace("gl-bus:", "")
                //                        .Replace("contextRef=\"journal_context\"", "")
                //                        .Replace("xmlns:gl-bus=\"http://www.xbrl.org/int/gl/bus/2006-10-25\"", "")
                //                        .Replace("xmlns:gl-cor=\"http://www.xbrl.org/int/gl/cor/2006-10-25\"", "")
                //                        );
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

                //dataset olarak okuma örneği...
                DataSet ds = new DataSet();
                ds.ReadXml(xmlReader);              

                TablolariOlustur(ds);
                */
                #endregion

                //Eski
                #region
                /*
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    SqlConnection conn = BaglantiSinifi.BaglantiAcDKNSQLServer();

                    //Eğer tablo varsa silecek...
                    SqlCommand cmd2 = new SqlCommand("IF OBJECT_ID('dbo." + ds.Tables[i].TableName + "', 'U') IS NOT NULL DROP TABLE dbo." + ds.Tables[i].TableName + ";", conn);
                    cmd2.ExecuteNonQuery();
                    //tablo oluşturuluyor...
                    string Query = CreateTableQuery(ds.Tables[i]);
                    SqlCommand cmd = new SqlCommand(Query);
                    cmd = new SqlCommand(Query, conn);
                    int check = cmd.ExecuteNonQuery();

                    BaglantiSinifi.BaglantiKapatDKNSQLServer(conn);

                    using (var bulkCopy = new SqlBulkCopy(BaglantiSinifi.DKNBaglantiAyarlariniOkuSQLServer(), SqlBulkCopyOptions.KeepIdentity))
                    {
                        // my DataTable column names match my SQL Column names, so I simply made this loop. However if your column names don't match, just pass in which datatable name matches the SQL column name in Column Mappings
                        foreach (DataColumn col in ds.Tables[i].Columns)
                        {
                            bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                        }
                        bulkCopy.BulkCopyTimeout = 600;
                        bulkCopy.DestinationTableName = ds.Tables[i].TableName;
                        bulkCopy.WriteToServer(ds.Tables[i]);
                    }
                }
                */
                #endregion


                //Mizan tabloları oluşturuluyor...
                YIL yil = CRUD.GetirYilIdIle(denetim.YIL_LOGICALREF);
                MizanlariTemizle();
                MizanlariOlustur(yil.YIL_AD);
                CRUD.GuncelleDenetimDurumu(denetimId, 2);

                //Denetim kuralları getirilir...
                #region
                KAPSAM kapsam = CRUD.GetirDenetimKapsamDenetimIdIle(denetim.LOGICALREF);
                List<DENETIM_KURALLARI> denetimKurallariList = CRUD.GetirDenetimKurallariListesiDenetimIdIle(denetimId, kapsam.LOGICALREF);
                List<List<DENETIM_KURALLARI>> list = ListeyiBol(denetimKurallariList);

                denetim.yilAd = yil.YIL_AD;
                //List<DENETIM_SONUC> denetimSonucList = new List<DENETIM_SONUC>();
                KuralUygula(denetimId, yil.YIL_AD, list);

                List<DENETIM_KURALLARI> denetimKurallariListCalismayanlar = CRUD.GetirDenetimKurallariListesiCalismayanlar(denetimId);
                while (denetimKurallariListCalismayanlar.Count > 0)
                {
                    CRUD.SilDenetimKuralCalismayanlarDenetimIdIle(denetimId);
                    List<List<DENETIM_KURALLARI>> listCalismayanlar = ListeyiBol(denetimKurallariListCalismayanlar);
                    KuralUygulaCalismayanlar(denetimId, yil.YIL_AD, listCalismayanlar);
                    denetimKurallariListCalismayanlar = new List<DENETIM_KURALLARI>();
                    denetimKurallariListCalismayanlar = CRUD.GetirDenetimKurallariListesiCalismayanlar(denetimId);
                }
                CRUD.GuncelleDenetimDurumu(denetimId, 3);
                #endregion

                //Eski
                #region  
                /*
                foreach (DENETIM_KURALLARI denetimKurallari in denetimKurallariList)
                {
                    //SQL boş olduğunda denetime dahil edilmesin...
                    if (!string.IsNullOrEmpty(denetimKurallari.SQL_IFADE.Trim()))
                    {
                        CalisanKuralSonuc calisanKuralSonuc = CRUD.CalistirKural(denetimKurallari.SQL_IFADE.Replace("@yil", denetim.yilAd));
                        denetimKurallari.calisanKuralSonuc = calisanKuralSonuc;
                        if (denetimKurallari.calisanKuralSonuc != null)
                        {
                            DENETIM_SONUC denetimSonuc = new DENETIM_SONUC();
                            denetimSonuc.KURAL_KOD = denetimKurallari.KOD;
                            denetimSonuc.MEVZUAT = denetimKurallari.MEVZUAT;

                            //Demek ki hiç sonuç dönmemiştir...
                            if (calisanKuralSonuc.entryDetail_IdList.Equals("") && calisanKuralSonuc.mizanList.Equals(""))
                            {
                                denetimSonuc.MUSTERI_ACIKLAMA = denetimKurallari.MUSTERI_ACIKLAMA2;
                                denetimSonuc.TIP = "";
                                denetimSonuc.DETAY = "";
                            }
                            //Demek ki en az 1 kayıt dönmüştür...
                            else
                            {
                                denetimSonuc.MUSTERI_ACIKLAMA = denetimKurallari.MUSTERI_ACIKLAMA;

                                if (!calisanKuralSonuc.entryDetail_IdList.Equals(""))
                                {
                                    denetimSonuc.TIP = "entryDetail_Id";
                                    denetimSonuc.DETAY = calisanKuralSonuc.entryDetail_IdList;
                                }
                                else if (!calisanKuralSonuc.mizanList.Equals(""))
                                {
                                    denetimSonuc.TIP = "mizan";
                                    denetimSonuc.DETAY = calisanKuralSonuc.mizanList;
                                }
                            }
                            denetimSonucList.Add(denetimSonuc);
                        }
                    }
                }
                */
                #endregion

                //Eski
                #region  
                /*
                foreach (DENETIM_KURALLARI denetimKurallari in denetimKurallariList)
                {
                    //SQL boş olduğunda denetime dahil edilmesin...
                    if (!string.IsNullOrEmpty(denetimKurallari.SQL_IFADE.Trim()))
                    {
                        //string ifade = denetimKurallari.SQL_IFADE.Trim().Replace("@yil", denetim.yilAd).Trim('\r', '\n');
                        string replace = denetimKurallari.SQL_IFADE.Trim().Replace("@yil", denetim.yilAd);
                        string replacement = Regex.Replace(replace, @"\t", "\\t");
                        string replacement1 = Regex.Replace(replacement, @"\n", "\\n");
                        string replacement2 = Regex.Replace(replacement1, @"\r", "\\r");
                        threadList.Add(
                                        new Thread(() => KuralServisiCagir(replacement2
                                                                        , denetimKurallari.KOD
                                                                        , denetimKurallari.MEVZUAT
                                                                        , denetimKurallari.MUSTERI_ACIKLAMA
                                                                        , denetimKurallari.MUSTERI_ACIKLAMA2
                                                                        , Sabitler.sunucuAdiW1
                                                                        , Sabitler.sunucuPortNoW1
                                                                        , Sabitler.veriTabaniAdiW1
                                                                        , Sabitler.kullaniciAdiW1
                                                                        , Sabitler.kullaniciSifreW1
                                                                        , Sabitler.sunucuAdiR
                                                                        , Sabitler.sunucuPortNoR
                                                                        , Sabitler.veriTabaniAdiR
                                                                        , Sabitler.kullaniciAdiR
                                                                        , Sabitler.kullaniciSifreR))
                                     );
                        //new Thread(() => KuralServisiCagir(replacement2
                        //                                    , denetimKurallari.KOD
                        //                                    , denetimKurallari.MEVZUAT
                        //                                    , denetimKurallari.MUSTERI_ACIKLAMA
                        //                                    , denetimKurallari.MUSTERI_ACIKLAMA2)).Start();
                        //BackgroundJob.Enqueue(() => KuralServisiCagir(replacement
                        //                                            , denetimKurallari.KOD
                        //                                            , denetimKurallari.MEVZUAT
                        //                                            , denetimKurallari.MUSTERI_ACIKLAMA
                        //                                            , denetimKurallari.MUSTERI_ACIKLAMA2));
                    }
                }
                */
                #endregion

                //Denetime ait raporlar oluşturuluyor ve Pdf klasörüne kaydediliyor...
                raporAd= RaporOlustur(denetim
                            , Sabitler.sunucuAdiW1
                            , Sabitler.veriTabaniAdiW1
                            , Sabitler.kullaniciAdiW1
                            , Sabitler.kullaniciSifreW1);

                CRUD.GuncelleDenetimDurumu(denetimId, 4);
                //Eski
                #region
                /*
                PdfExportOptions pdfExportOptions = new PdfExportOptions() { PdfACompatibility = PdfACompatibility.PdfA1b };

                XtraReport report1 = XtraReport.FromFile(Server.MapPath("~/Reports/" + "giris" + ".repx"), true);
                report1.CreateDocument(false);                       
                XtraReport report2 = XtraReport.FromFile(Server.MapPath("~/Reports/" + "entryDetail_Id" + ".repx"), true);
                report2.CreateDocument(false);
                XtraReport report3 = XtraReport.FromFile(Server.MapPath("~/Reports/" + "mizan" + ".repx"), true);
                report3.CreateDocument(false);
                XtraReport report4 = XtraReport.FromFile(Server.MapPath("~/Reports/" + "detaysiz" + ".repx"), true);
                report4.CreateDocument(false);

                report1.Pages.AddRange(report2.Pages);
                report1.Pages.AddRange(report3.Pages);
                report1.Pages.AddRange(report4.Pages);

                string pdfExportFile = Server.MapPath("~/Pdf/" + denetim.AD + ".pdf");
                report1.ExportToPdf(pdfExportFile, pdfExportOptions);
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
                denetimRapor.DENETIM_LOGICALREF = denetimId;
                denetimRapor.RAPOR_LOGICALREF = objYeni.ToString();
                denetimRapor.LOGICALREF = objYeni2.ToString();
                denetimRapor.KULLANIM_DURUMU = 1;
                CRUD.KaydetDenetimRapor(denetimRapor);

                 */
                #endregion

                //Defter isimleri veri tabanına yazılıyor...

                //Eski
                #region
                /*
                string path = Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ ");
                if (Directory.Exists(path))
                {
                    DirectoryInfo dir = new DirectoryInfo(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ "));
                    FileInfo[] filess = dir.GetFiles("*.xml");

                    // IEnumerable<string> yevmiyeList = Directory.GetFiles(Server.MapPath("~/yevmiyeler"), "*.xml");
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
                */
                #endregion

                DefterIsimleriKaydet(denetimId);

                stopWatch.Stop();
                string gecenSure = Math.Floor((decimal)(stopWatch.ElapsedMilliseconds / 1000)).ToString();
                CRUD.GuncelleDenetimDurum(denetimId, gecenSure);
                KontorHareketKaydet(denetim, yil);

                int dakika = Convert.ToInt32(gecenSure)/60;
                int saniye = Convert.ToInt32(gecenSure) % 60;
                BILDIRIM bildirim = new BILDIRIM();
                Guid obj = Guid.NewGuid();
                bildirim.LOGICALREF = obj.ToString();
                bildirim.KULLANICI_LOGICALREF = Session["LOGICALREF"].ToString();
                bildirim.ACIKLAMA_KISA = " Denetiminiz Tamamlandı.("+ raporAd+")";
                bildirim.ACIKLAMA_UZUN = "Denetiminizde <b>"+ denetim.KURAL_SAYISI+ "</b> kural çalıştı. Maliyeti toplamda <b>"+ denetim.KONTOR_SAYISI+"</b> kontör, <b>"+ dakika + ":"+saniye+" </b> sn sürdü." +
                                         "<p>Denetime ait raporlara ulaşmak için <a href=\"/Kullanici/SonRaporlar\">tıklayınız.</a></p> ";
                bildirim.KULLANIM_DURUMU = 1;
                bildirim.DURUMU = 1;

                BildirimKaydet(bildirim);

                int bakiye = Convert.ToInt32(CRUD.GetirKullaniciBakiye(Session["LOGICALREF"].ToString()));
                if (bakiye < 20)
                {

                    BILDIRIM bildirim1 = new BILDIRIM();
                    Guid obj1 = Guid.NewGuid();
                    bildirim1.LOGICALREF = obj1.ToString();
                    bildirim1.KULLANICI_LOGICALREF = Session["LOGICALREF"].ToString();
                    bildirim1.ACIKLAMA_KISA =  "Kontör bakiyeniz azalmıştır.";
                    bildirim1.ACIKLAMA_UZUN = "Kontör satın almak için <a href=\"/Kullanici/KontorSatinAl\">tıklayınız.</a>";
                    bildirim1.KULLANIM_DURUMU = 1;
                    bildirim1.DURUMU = 1;

                    BildirimKaydet(bildirim1);
                }
                //Denetim tamalandığında kullanıcıya mail gönderiyorum.
                SendEmailDenetimDone(denetimId,raporAd);

                //Denetim tamalandığında kullanıcıya whatsaap mesaj gönderiyorum.
                //SendWhatsappDenetimDone(denetimId, Session["EMAIL"].ToString());

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
                //CRUD.WriteLog(e.Message.ToString(), "Denetim Başlat");
            }
            return RedirectToAction("FirmaDenetimleri", new { firmaId = Session["firmaId"].ToString() });
        }

        public List<List<DENETIM_KURALLARI>> ListeyiBol(List<DENETIM_KURALLARI> denetimKurallariList)
        {
            List<List<DENETIM_KURALLARI>> list = new List<List<DENETIM_KURALLARI>>();
            try
            {
                int sayi = denetimKurallariList.Count / 16;
                int kalan = denetimKurallariList.Count % 16;

                //Eğer tam olarak 16'e bölünmüyorsa kurallar, yukarıya yuvarlıyoruz...
                if (kalan != 0)
                {
                    sayi = denetimKurallariList.Count / 16 + 1;
                }

                for (int i = 0; i < denetimKurallariList.Count; i += sayi)
                {
                    list.Add(denetimKurallariList.GetRange(i, Math.Min(sayi, denetimKurallariList.Count - i)));
                }
            }
            catch (Exception e)
            {
            }
            return list;
        }

        public string CreateTableQuery(DataTable table)
        {
            string sqlsc = "CREATE TABLE " + table.TableName + "(";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sqlsc += "[" + table.Columns[i].ColumnName + "]";
                string columnType = table.Columns[i].DataType.ToString();
                switch (columnType)
                {
                    case "System.Int32":
                        sqlsc += " int ";
                        break;
                    case "System.Int64":
                        sqlsc += " bigint ";
                        break;
                    case "System.Int16":
                        sqlsc += " smallint";
                        break;
                    case "System.Byte":
                        sqlsc += " tinyint";
                        break;
                    case "System.Decimal":
                        sqlsc += " decimal ";
                        break;
                    case "System.DateTime":
                        sqlsc += " datetime ";
                        break;
                    case "System.String":
                    default:
                        sqlsc += string.Format(" nvarchar({0}) ", table.Columns[i].MaxLength == -1 ? "max" : table.Columns[i].MaxLength.ToString());
                        break;
                }
                if (table.Columns[i].AutoIncrement)
                    sqlsc += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
                if (!table.Columns[i].AllowDBNull)
                    sqlsc += " NOT NULL ";
                sqlsc += ",";
            }
            return sqlsc.Substring(0, sqlsc.Length - 1) + "\n)";
        }

        [Authorize]
        public ActionResult Rapor(string denetimId)
        {
            List<RAPOR> raporList = new List<RAPOR>();
            List<DENETIM_RAPOR> denetimRaporList = new List<DENETIM_RAPOR>();
            try
            {
                DENETIM denetim = CRUD.GetirDenetimIdIle(denetimId);
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

        [Authorize]
        public ActionResult ResimSil(string logiclRef)
        {
            FIRMA firma = CRUD.GetirFirmaIdIle(logiclRef);
            string path = Server.MapPath("~/Content/adminlte/img/noImage4.png");
            firma.LOGO = System.IO.File.ReadAllBytes(path);
            //firma.LOGO = new byte[0];
            try
            {
                if (logiclRef != null)
                {
                    CRUD.GuncelleFirma(firma);
                }

                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Resim Silme İşlemi Başarılı." };

            }
            catch (Exception)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Resim Silme İşleminde Hata  Oluştu." };


            }
            return RedirectToAction("EditFirma", new { id = logiclRef });

        }

        public List<Thread> KurallariUygula(List<DENETIM_KURALLARI> denetimList
                                            , string yil
                                            , string sunucuAdi
                                            , string veriTabaniAdi
                                            , string kullaniciAdi
                                            , string kullaniciSifre
                                            , string kuralServisEndpoint
                                            , string denetimId)
        {
            List<Thread> threadList = new List<Thread>();

            try
            {
                foreach (DENETIM_KURALLARI denetimKurallari in denetimList)
                {
                    //SQL boş olduğunda denetime dahil edilmesin...
                    if (!string.IsNullOrEmpty(denetimKurallari.SQL_IFADE.Trim()))
                    {
                        string replace = denetimKurallari.SQL_IFADE.Trim().Replace("@yil", yil);
                        string replacement = Regex.Replace(replace, @"\t", "\\t");
                        string replacement1 = Regex.Replace(replacement, @"\n", "\\n");
                        string replacement2 = Regex.Replace(replacement1, @"\r", "\\r");

                        string kapsamString = "";
                        List<DENETIM_KURALLARI_KAPSAM> denetimKapsamList = CRUD.GetirDenetimKurallariKapsamListesiDenetimKuralIdIle(denetimKurallari.LOGICALREF);

                        foreach (DENETIM_KURALLARI_KAPSAM denetimKapsam in denetimKapsamList)
                        {
                            KAPSAM kapsam = CRUD.GetirKapsamIdIle(denetimKapsam.KAPSAM_LOGICALREF);
                            kapsamString = kapsamString + " " + kapsam.KISA_AD;
                        }

                        threadList.Add(
                                        new Thread(() => KuralServisiCagir(replacement2
                                                                        , denetimKurallari.KOD
                                                                        , kapsamString
                                                                        , denetimKurallari.MEVZUAT
                                                                        , denetimKurallari.MUSTERI_ACIKLAMA
                                                                        , denetimKurallari.MUSTERI_ACIKLAMA2
                                                                        , sunucuAdi
                                                                        , veriTabaniAdi
                                                                        , kullaniciAdi
                                                                        , kullaniciSifre
                                                                        , Sabitler.sunucuAdiR
                                                                        , Sabitler.veriTabaniAdiR
                                                                        , Sabitler.kullaniciAdiR
                                                                        , Sabitler.kullaniciSifreR
                                                                        , kuralServisEndpoint
                                                                        , denetimId))
                                     );
                    }
                }

                return threadList;
            }
            catch (Exception ex)
            {
                CRUD.WriteLog(denetimId, ex.Message.ToString(), "KurallariUygula");
                return threadList;
            }
        }

        public void KuralServisiCagir(string SQL, string KOD, string kapsam, string MEVZUAT, string MUSTERI_ACIKLAMA, string MUSTERI_ACIKLAMA2,
                                      string sunucuAdiW, string veriTabaniAdiW, string kullaniciAdiW, string kullaniciSifreW,
                                      string sunucuAdiR, string veriTabaniAdiR, string kullaniciAdiR, string kullaniciSifreR,
                                      string endpoint, string denetimId)
        {
            try
            {
                string result = "";
                //string parametreler = "{\"istekModel\": [";
                //parametreler = parametreler + "{\"SQL\":\"" + SQL + "\"" +
                string parametreler = " {\"SQL\":\"" + SQL + "\"" +
                                        ",\"KOD\":\"" + KOD + "\"" +
                                        ",\"KAPSAM\":\"" + kapsam + "\"" +
                                        ",\"MEVZUAT\":\"" + MEVZUAT + "\"" +
                                        ",\"MUSTERI_ACIKLAMA\":\"" + MUSTERI_ACIKLAMA + "\"" +
                                        ",\"MUSTERI_ACIKLAMA2\":\"" + MUSTERI_ACIKLAMA2 + "\"" +
                                        ",\"sunucuAdiW\":\"" + sunucuAdiW + "\"" +
                                        ",\"sunucuPortNoW\":\"" + "" + "\"" +
                                        ",\"veriTabaniAdiW\":\"" + veriTabaniAdiW + "\"" +
                                        ",\"kullaniciAdiW\":\"" + kullaniciAdiW + "\"" +
                                        ",\"kullaniciSifreW\":\"" + kullaniciSifreW + "\"" +
                                        ",\"sunucuAdiR\":\"" + sunucuAdiR + "\"" +
                                        ",\"sunucuPortNoR\":\"" + "" + "\"" +
                                        ",\"veriTabaniAdiR\":\"" + veriTabaniAdiR + "\"" +
                                        ",\"kullaniciAdiR\":\"" + kullaniciAdiR + "\"" +
                                        ",\"kullaniciSifreR\":\"" + kullaniciSifreR + "\"" +
                                        "}";

                HttpWebRequest req = WebRequest.Create(new Uri(endpoint)) as HttpWebRequest;
                req.Timeout = 900000;
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Accept = "application/json";
                byte[] formData = UTF8Encoding.UTF8.GetBytes(parametreler);
                req.ContentLength = formData.Length;
                using (Stream post = req.GetRequestStream())
                {
                    post.Write(formData, 0, formData.Length);
                }
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                CRUD.WriteLog(denetimId, "KuralServisiCagir(Ana Uygulama) " + ex.Message.ToString(), KOD);
                CRUD.CalismayanKuralEkle(denetimId, KOD, ex.Message.ToString());
            }
        }

        public List<Thread> XMLdenTabloOlustur(DataSet ds, string sunucuAdi, string sunucuPortNo,
                                       string veriTabaniAdi, string kullaniciAdi, string kullaniciSifre)
        {
            List<Thread> threadList = new List<Thread>();
            try
            {
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    //Eski
                    #region
                    /*
                    //SqlConnection conn = BaglantiSinifi.BaglantiAcDKNSQLServer();
                    SqlConnection conn = new SqlConnection("Server=" + sunucuAdi
                                                        + ";Database=" + veriTabaniAdi
                                                        + ";User ID=" + kullaniciAdi
                                                        + ";Password=" + kullaniciSifre
                                                        + "; Trusted_Connection=False");
                    conn.Open();
                    //Eğer tablo varsa silecek...
                    SqlCommand cmd2 = new SqlCommand("IF OBJECT_ID('dbo." + ds.Tables[i].TableName + "', 'U') IS NOT NULL DROP TABLE dbo." + ds.Tables[i].TableName + ";", conn);
                    cmd2.ExecuteNonQuery();
                    //tablo oluşturuluyor...
                    string Query = CreateTableQuery(ds.Tables[i]);
                    SqlCommand cmd = new SqlCommand(Query);
                    cmd = new SqlCommand(Query, conn);
                    int check = cmd.ExecuteNonQuery();

                    BaglantiSinifi.BaglantiKapatDKNSQLServer(conn);

                    using (var bulkCopy = new SqlBulkCopy("Server=" + sunucuAdi
                                                        + ";Database=" + veriTabaniAdi
                                                        + ";User ID=" + kullaniciAdi
                                                        + ";Password=" + kullaniciSifre
                                                        + "; Trusted_Connection=False", SqlBulkCopyOptions.KeepIdentity))
                    {
                        // my DataTable column names match my SQL Column names, so I simply made this loop. However if your column names don't match, just pass in which datatable name matches the SQL column name in Column Mappings
                        foreach (DataColumn col in ds.Tables[i].Columns)
                        {
                            bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                        }
                        bulkCopy.BulkCopyTimeout = 1000;
                        bulkCopy.DestinationTableName = ds.Tables[i].TableName;
                        bulkCopy.WriteToServer(ds.Tables[i]);
                    }
                     */
                    #endregion

                    threadList.Add(new Thread(() => XMLdenTabloOlusturThread(sunucuAdi, veriTabaniAdi, kullaniciAdi, kullaniciSifre, ds.Tables[i])));

                    //XMLdenTabloOlusturThread(sunucuAdi, veriTabaniAdi,kullaniciAdi,kullaniciSifre, ds.Tables[i]);
                }
                return threadList;
                //foreach (Thread threadSiradaki in threadList)
                //{
                //    threadSiradaki.Start();
                //}
                //do
                //{
                //    Thread.Sleep(500);
                //} while (CalisanThreadVarMi(threadList));
            }
            catch (Exception ex)
            {
                return threadList;
            }
        }

        public void XMLdenTabloOlusturThread(string sunucuAdi, string veriTabaniAdi,
                                             string kullaniciAdi, string kullaniciSifre,
                                             DataTable dataTable)
        {
            SqlConnection conn = new SqlConnection("Server=" + sunucuAdi
                                                    + ";Database=" + veriTabaniAdi
                                                    + ";User ID=" + kullaniciAdi
                                                    + ";Password=" + kullaniciSifre
                                                    + ";Pooling=false");

            SqlBulkCopy bulkCopy = new SqlBulkCopy("Server=" + sunucuAdi
                                                    + ";Database=" + veriTabaniAdi
                                                    + ";User ID=" + kullaniciAdi
                                                    + ";Password=" + kullaniciSifre
                                                    + ";Pooling=false", SqlBulkCopyOptions.KeepIdentity);

            try
            {
                conn.Open();
                //Eğer tablo varsa silecek...
                SqlCommand cmd2 = new SqlCommand("IF OBJECT_ID('dbo." + dataTable.TableName + "', 'U') IS NOT NULL DROP TABLE dbo." + dataTable.TableName + ";", conn);
                cmd2.ExecuteNonQuery();
                //tablo oluşturuluyor...
                string Query = CreateTableQuery(dataTable);
                SqlCommand cmd = new SqlCommand(Query);
                cmd = new SqlCommand(Query, conn);
                int check = cmd.ExecuteNonQuery();

                BaglantiSinifi.BaglantiKapatDKNSQLServer(conn);
                foreach (DataColumn col in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }

                bulkCopy.BulkCopyTimeout = 1000;
                bulkCopy.DestinationTableName = dataTable.TableName;
                bulkCopy.WriteToServer(dataTable);
                bulkCopy.Close();
            }
            catch (Exception ex)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(conn);
                bulkCopy.Close();
            }
        }

        public void MizanServisiCagir(string yil, string query, string endpoint, string sunucuAdi,
                                      string veriTabaniAdi, string kullaniciAdi, string kullaniciSifre)
        {
            #region
            ////Mizan1 oluşturuluyor...
            ////önce varsa 4 tane mizan talosunu temizliyoruz...
            //string query1 = @"      DELETE FROM MIZAN1;
            //                        DELETE FROM MIZAN2;
            //                        DELETE FROM MIZAN3;
            //                        DELETE FROM MIZAN4;
            //                       INSERT INTO MIZAN1
            //                        (accountSubID
            //                        ,debitSum
            //                        ,creditSum
            //                        ,fark)
            //                        select  
            //                        isnull(TABLO1.debitAccountSubID,TABLO2.creditAccountSubID) as accountSubID,
            //                        isnull(TABLO1.debitSum,0) as debitSum, 
            //                        isnull(TABLO2.creditSum,0) as creditSum,
            //                        isnull(TABLO1.debitSum,0)-isnull(TABLO2.creditSum,0) as fark 
            //                        from
            //                        (
            //                        select  acs.accountSubID as debitAccountSubID,
            //                        SUM(convert(DECIMAL(20,4),amt.amount_Text)) as debitSum 
            //                        from accountSub acs 
            //                        inner join account ac on ac.account_Id = acs.account_Id 
            //                        inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'D' 
            //                        inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-03-31')  
            //                        inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
            //                        group by acs.accountSubID
            //                        )TABLO1
            //                        full join 
            //                        (
            //                        select  acs.accountSubID as creditAccountSubID, 
            //                        SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
            //                        from accountSub acs 
            //                        inner join account ac on ac.account_Id = acs.account_Id 
            //                        inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
            //                        inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-03-31') 
            //                        inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
            //                        group by acs.accountSubID
            //                        )TABLO2 on TABLO1.debitAccountSubID = TABLO2.creditAccountSubID
            //                        order by TABLO1.debitAccountSubID 
            //                    ";
            //query1 = query1.Replace("@yil", yil);

            ////Mizan2 oluşturuluyor...
            //string query2 = @"  INSERT INTO MIZAN2
            //                    (accountSubID
            //                    ,debitSum
            //                    ,creditSum
            //                    ,fark)
            //                    select  
            //                    isnull(TABLO1.debitAccountSubID,TABLO2.creditAccountSubID) as accountSubID,
            //                    isnull(TABLO1.debitSum,0) as debitSum, 
            //                    isnull(TABLO2.creditSum,0) as creditSum,
            //                    isnull(TABLO1.debitSum,0)-isnull(TABLO2.creditSum,0) as fark 
            //                    from
            //                    (
            //                    select  acs.accountSubID as debitAccountSubID,
            //                    SUM(convert(DECIMAL(20,4),amt.amount_Text)) as debitSum 
            //                    from accountSub acs 
            //                    inner join account ac on ac.account_Id = acs.account_Id 
            //                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'D' 
            //                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-06-30')  
            //                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
            //                    group by acs.accountSubID
            //                    )TABLO1
            //                    full join 
            //                    (
            //                    select  acs.accountSubID as creditAccountSubID, 
            //                    SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
            //                    from accountSub acs 
            //                    inner join account ac on ac.account_Id = acs.account_Id 
            //                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
            //                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-06-30') 
            //                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
            //                    group by acs.accountSubID
            //                    )TABLO2 on TABLO1.debitAccountSubID = TABLO2.creditAccountSubID
            //                    order by TABLO1.debitAccountSubID
            //            ";
            //query2 = query2.Replace("@yil", yil);

            ////Mizan3 oluşturuluyor...
            //string query3 = @" INSERT INTO MIZAN3
            //                    (accountSubID
            //                    ,debitSum
            //                    ,creditSum
            //                    ,fark)

            //                    select  
            //                    isnull(TABLO1.debitAccountSubID,TABLO2.creditAccountSubID) as accountSubID,
            //                    isnull(TABLO1.debitSum,0) as debitSum, 
            //                    isnull(TABLO2.creditSum,0) as creditSum,
            //                    isnull(TABLO1.debitSum,0)-isnull(TABLO2.creditSum,0) as fark 
            //                    from
            //                    (
            //                    select  acs.accountSubID as debitAccountSubID,
            //                    SUM(convert(DECIMAL(20,4),amt.amount_Text)) as debitSum 
            //                    from accountSub acs 
            //                    inner join account ac on ac.account_Id = acs.account_Id 
            //                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'D' 
            //                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-09-30')  
            //                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
            //                    group by acs.accountSubID
            //                    )TABLO1
            //                    full join 
            //                    (
            //                    select  acs.accountSubID as creditAccountSubID, 
            //                    SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
            //                    from accountSub acs 
            //                    inner join account ac on ac.account_Id = acs.account_Id 
            //                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
            //                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-09-30') 
            //                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
            //                    group by acs.accountSubID
            //                    )TABLO2 on TABLO1.debitAccountSubID = TABLO2.creditAccountSubID
            //                    order by TABLO1.debitAccountSubID ";
            //query3 = query3.Replace("@yil", yil);

            ////Mizan4 oluşturuluyor...
            ////mizan4 ü hesap ederken son yevmiye fişini hesaba katmıyoruz(kapanış fişi)...
            //string query4 = @"  INSERT INTO MIZAN4
            //                    (accountSubID
            //                    ,debitSum
            //                    ,creditSum
            //                    ,fark)

            //                    select  
            //                    isnull(TABLO1.debitAccountSubID,TABLO2.creditAccountSubID) as accountSubID,
            //                    isnull(TABLO1.debitSum,0) as debitSum, 
            //                    isnull(TABLO2.creditSum,0) as creditSum,
            //                    isnull(TABLO1.debitSum,0)-isnull(TABLO2.creditSum,0) as fark 
            //                    from
            //                    (
            //                    select  acs.accountSubID as debitAccountSubID,
            //                    SUM(convert(DECIMAL(20,4),amt.amount_Text)) as debitSum 
            //                    from accountSub acs 
            //                    inner join account ac on ac.account_Id = acs.account_Id 
            //                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'D' 
            //                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-12-31')  
            //                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
            //                    inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id and enc.entryNumberCounter_Text < (select MAX(convert(int,enc.entryNumberCounter_Text))
            //															                                   from entryHeader eh
            //															                                   inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id)
            //                    group by acs.accountSubID
            //                    )TABLO1
            //                    full join 
            //                    (
            //                    select acs.accountSubID as creditAccountSubID, 
            //                    SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
            //                    from accountSub acs 
            //                    inner join account ac on ac.account_Id = acs.account_Id 
            //                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
            //                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-12-31') 
            //                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
            //                    inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id and enc.entryNumberCounter_Text < (select MAX(convert(int,enc.entryNumberCounter_Text))
            //															                                   from entryHeader eh
            //															                                   inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id)
            //                    group by acs.accountSubID
            //                    )TABLO2 on TABLO1.debitAccountSubID = TABLO2.creditAccountSubID
            //                    order by TABLO1.debitAccountSubID ";
            //query4 = query4.Replace("@yil", yil);
            #endregion
            try
            {
                string replacement = Regex.Replace(query, @"\t", "\\t");
                string replacement1 = Regex.Replace(replacement, @"\n", "\\n");
                string replacement2 = Regex.Replace(replacement1, @"\r", "\\r");
                string result = "";

                string parametreler = " {\"SQL\":\"" + replacement2 + "\"" +
                                        ",\"sunucuAdi\":\"" + sunucuAdi + "\"" +
                                        ",\"sunucuPortNo\":\"" + "" + "\"" +
                                        ",\"veriTabaniAdi\":\"" + veriTabaniAdi + "\"" +
                                        ",\"kullaniciAdi\":\"" + kullaniciAdi + "\"" +
                                        ",\"kullaniciSifre\":\"" + kullaniciSifre + "\"" +
                                        "} ";

                HttpWebRequest req = WebRequest.Create(new Uri(endpoint)) as HttpWebRequest;
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Accept = "application/json";
                byte[] formData = UTF8Encoding.UTF8.GetBytes(parametreler);
                req.ContentLength = formData.Length;
                using (Stream post = req.GetRequestStream())
                {
                    post.Write(formData, 0, formData.Length);
                }
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }

                //List<Thread> thradList = new List<Thread>();
                //thradList.Add(t1);
                //thradList.Add(t2);
                //thradList.Add(t3);
                //thradList.Add(t4);

                //foreach (Thread thread in thradList)
                //{thread.Start();}

                //foreach (Thread thread in thradList)
                //{ thread.Join(); }

                //while (t1.IsAlive || t2.IsAlive || t3.IsAlive || t4.IsAlive)
                //{
                //    Thread.Sleep(500);
                //}

            }
            catch (Exception ex)
            {
                CRUD.WriteLog("denetimId", ex.Message.ToString(), "MizanServisiCagir");
            }
        }

        public void MizanlariTemizle()
        {
            try
            {
                MizanServisiCagirTemizle("DELETE FROM MIZAN1;DELETE FROM MIZAN2;DELETE FROM MIZAN3;DELETE FROM MIZAN4;",
                                         Sabitler.mizanServisEndpoint1,
                                         Sabitler.sunucuAdiW1,
                                         Sabitler.veriTabaniAdiW1,
                                         Sabitler.kullaniciAdiW1,
                                         Sabitler.kullaniciSifreW1);
                //MizanServisiCagirTemizle("DELETE FROM MIZAN1;DELETE FROM MIZAN2;DELETE FROM MIZAN3;DELETE FROM MIZAN4;",
                //                         Sabitler.mizanServisEndpoint2,
                //                         Sabitler.sunucuAdiW2,
                //                         Sabitler.sunucuPortNoW2,
                //                         Sabitler.veriTabaniAdiW2,
                //                         Sabitler.kullaniciAdiW2,
                //                         Sabitler.kullaniciSifreW2);
                //MizanServisiCagirTemizle("DELETE FROM MIZAN1;DELETE FROM MIZAN2;DELETE FROM MIZAN3;DELETE FROM MIZAN4;",
                //                         Sabitler.mizanServisEndpoint3,
                //                         Sabitler.sunucuAdiW3,
                //                         Sabitler.sunucuPortNoW3,
                //                         Sabitler.veriTabaniAdiW3,
                //                         Sabitler.kullaniciAdiW3,
                //                         Sabitler.kullaniciSifreW3);
                //MizanServisiCagirTemizle("DELETE FROM MIZAN1;DELETE FROM MIZAN2;DELETE FROM MIZAN3;DELETE FROM MIZAN4;",
                //                         Sabitler.mizanServisEndpoint4,
                //                         Sabitler.sunucuAdiW4,
                //                         Sabitler.sunucuPortNoW4,
                //                         Sabitler.veriTabaniAdiW4,
                //                         Sabitler.kullaniciAdiW4,
                //                         Sabitler.kullaniciSifreW4);

            }
            catch (Exception ex)
            {
                CRUD.WriteLog("denetimId", ex.Message, "MizanlariTemizle");
            }
        }

        public DataSet DataSetOlustur()
        {
            string path = Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString() + "/ ");
            DataSet ds = new DataSet();

            try
            {
                DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/yevmiyeler/" + Session["firmaId"].ToString()));
                FileInfo[] files = di.GetFiles("*.xml");

                XElement xElem3 = new XElement("root");
                XElement xElem4 = new XElement("root");

                foreach (FileInfo file in files)
                {
                    XElement xElement = XElement.Load(path + "//" + file.Name);
                    xElem3.Add(xElement);
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
                return ds;
            }
            catch (Exception ex)
            {
                return ds;
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
                    if (Session["yevmiyeDefteriListTemp"] != null)
                        yevmiyeDefteriListTemp = (List<DosyaBilgileri>)Session["yevmiyeDefteriListTemp"];
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
                    // IEnumerable<string> yevmiyeList = Directory.GetFiles(Server.MapPath("~/yevmiyeler"), "*.xml");

                }
            }
            catch (Exception ex)
            {

            }
        }

        public void RaporKaydet(DENETIM denetim
                                , string raporAd
                                , string firmaUnvan
                                , string denetimKapsami
                                , string ceyrekAdi
                                , string altKapsamList
                                , string lowHigh
                                , string sunucuAdi
                                , string veriTabaniAdi
                                , string kullaniciAdi
                                , string kullaniciSifre)
        {
            try
            {
                string denetimKapsamiUzun = "";
                string denetimKapsamiUzunAdIcin = "";
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

                string giris = "giris";
                giris = giris + denetimKapsami;
                string arkaKapak = "arkaKapak";
                arkaKapak = arkaKapak + denetimKapsami;


                switch (denetimKapsami)
                {
                    case "AVD":
                        denetimKapsamiUzun = "AYLIK VERGİ DENETİMİ";
                        break;
                    case "GVD":
                        denetimKapsamiUzun = "GEÇİCİ VERGİ DENETİMİ";
                        break;
                    case "HD":
                        denetimKapsamiUzun = "HİLE DENETİMİ";
                        break;
                    case "PD":
                        denetimKapsamiUzun = "PERFORMANS DENETİMİ";
                        break;
                        //case "VR":
                        //    denetimKapsamiUzun = "VERGİ RİSKİ";
                        //    break;
                        //case "VA":
                        //    denetimKapsamiUzun = "VERGİ AVANTAJI";
                        //    break;
                }
                denetimKapsamiUzunAdIcin = denetimKapsamiUzun;
                //ÜNAL PEYNİRCİLİK 2020 AYLIK VERGİ DENETİMİ 1 - 9 Aylar(Kısıtlı Detay).pdf
                if (lowHigh.Equals("low"))
                {
                    denetimKapsamiUzun = denetimKapsamiUzun + " (Kısıtlı Detay)";
                    entryDetail_Id = entryDetail_Id + "_low";
                }

                //MsSqlConnectionParameters connectionParameters
                //    = new MsSqlConnectionParameters(parametreler.Sabitler.sunucuAdiR
                //                                   , parametreler.Sabitler.veriTabaniAdiR
                //                                   , parametreler.Sabitler.kullaniciAdiR
                //                                   , parametreler.Sabitler.kullaniciSifreR
                //                                   , MsSqlAuthorizationType.SqlServer);

                MsSqlConnectionParameters connectionParameters
                    = new MsSqlConnectionParameters(sunucuAdi
                                                   , veriTabaniAdi
                                                   , kullaniciAdi
                                                   , kullaniciSifre
                                                   , MsSqlAuthorizationType.SqlServer);

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                XtraReport report1 = XtraReport.FromFile(Server.MapPath("~/Reports/" + giris + ".repx"), true);
                report1.Parameters["parameter1"].Value = denetim.yilAd;
                report1.Parameters["parameter1"].Visible = false;
                report1.Parameters["parameter2"].Value = ceyrekAdi;
                report1.Parameters["parameter2"].Visible = false;
                //report1.Parameters["parameter3"].Value = CRUD.GetirFirmaIdIle(Session["firmaId"].ToString()).UNVAN;
                report1.Parameters["parameter3"].Value = firmaUnvan;
                report1.Parameters["parameter3"].Visible = false;
                report1.Parameters["parameter4"].Value = denetimKapsamiUzun;
                report1.Parameters["parameter4"].Visible = false;
                report1.Parameters["parameter5"].Value = denetim.TARIH.Date.ToShortDateString();
                report1.Parameters["parameter5"].Visible = false;
                report1.Parameters["parameter6"].Value = altKapsamList;
                report1.Parameters["parameter6"].Visible = false;
                report1.CreateDocument(false);
                string deneme = denetim.TARIH.Date.ToShortDateString();
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

                //Band detailreportBand = report2.Bands[BandKind.DetailReport].BeforePrint += OwnReport_BeforePrint;
                //detailreportBand.

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

                XtraReport report5 = XtraReport.FromFile(Server.MapPath("~/Reports/" + arkaKapak + ".repx"), true);
                report5.CreateDocument(false);

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //report1.Pages.AddRange(report2.Pages);
                //report1.Pages.AddRange(report3.Pages);
                //report1.Pages.AddRange(report4.Pages);
                //report1.Pages.AddRange(report5.Pages);

                XRWatermark watermark2 = new XRWatermark();
                watermark2.CopyFrom(report2.Watermark);

                XRWatermark watermark3 = new XRWatermark();
                watermark3.CopyFrom(report3.Watermark);

                XRWatermark watermark4 = new XRWatermark();
                watermark4.CopyFrom(report4.Watermark);

                XRWatermark watermark5 = new XRWatermark();
                watermark5.CopyFrom(report5.Watermark);

                int mainPageCount = report1.Pages.Count;
                int pageCount2 = report2.Pages.Count;
                int pageCount3 = report3.Pages.Count;
                int pageCount4 = report4.Pages.Count;
                int pageCount5 = report5.Pages.Count;

                report1.Pages.AddRange(report2.Pages);
                report1.PrintingSystem.ContinuousPageNumbering = true;
                report1.Pages.AddRange(report3.Pages);
                report1.PrintingSystem.ContinuousPageNumbering = true;
                report1.Pages.AddRange(report4.Pages);
                report1.PrintingSystem.ContinuousPageNumbering = true;
                report1.Pages.AddRange(report5.Pages);

                for (int i = mainPageCount; i < mainPageCount + pageCount2; i++)
                {
                    report1.Pages[i].AssignWatermark(watermark2);
                }

                for (int i = mainPageCount + pageCount2; i < mainPageCount + pageCount2 + pageCount3; i++)
                {
                    report1.Pages[i].AssignWatermark(watermark3);
                }

                for (int i = mainPageCount + pageCount2 + pageCount3; i < mainPageCount + pageCount2 + pageCount3 + pageCount4; i++)
                {
                    report1.Pages[i].AssignWatermark(watermark4);
                }

                for (int i = mainPageCount + pageCount2 + pageCount3 + pageCount4; i < mainPageCount + pageCount2 + pageCount3 + pageCount4 + pageCount5; i++)
                {
                    report1.Pages[i].AssignWatermark(watermark5);
                }

                string pdfExportFile = Server.MapPath("~/Pdf/" + raporAd + " " + denetimKapsamiUzunAdIcin + " " + ceyrekAdi + " (Kısıtlı Detay)" + ".pdf");
                PdfExportOptions pdfExportOptions = new PdfExportOptions() { PdfACompatibility = PdfACompatibility.PdfA1b };
                report1.ExportToPdf(pdfExportFile, pdfExportOptions);

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
                CRUD.WriteLog(denetim.LOGICALREF, ex.Message.ToString(), "RaporKaydet");
            }
        }

        //public void OwnReport_BeforePrint(object sender, PrintEventArgs e)
        //{
        //    DetailBand db = (DetailBand)sender;
        //    db.r
        //}

        public string RaporOlustur(DENETIM denetim
                                , string sunucuAdi
                                , string veriTabaniAdi
                                , string kullaniciAdi
                                , string kullaniciSifre)
        {
            string raporAd = "";
            try
            {
                //List<DENETIM_KAPSAM> denetimKapsamList = CRUD.GetirDenetimKapsamListesiDenetimIdIle(denetim.LOGICALREF);
                //foreach (DENETIM_KAPSAM denetimKapsam in denetimKapsamList)
                //{
                //    KAPSAM kapsam = CRUD.GetirKapsamIdIle(denetimKapsam.KAPSAM_LOGICALREF);
                //    denetim.kapsamList.Add(kapsam);
                //}


                //RaporKaydet metoduna dönem/ay bilgisini parametrik göndermek için denetimId ile bilgiler getirildi.
                //ÜNAL PEYNİRCİLİK 2020 AYLIK VERGİ DENETİMİ 1 - 9 Aylar(Kısıtlı Detay).pdf


                KAPSAM kapsam = CRUD.GetirDenetimKapsamDenetimIdIle(denetim.LOGICALREF);
                DONEM_AY donemAy = CRUD.GetirDonemAyDenetimIdIle(denetim.LOGICALREF);
                string altKapsamList = CRUD.GetirAltKapsamListesidDenetimIdIle(denetim.LOGICALREF);
                string firmaUnvani = CRUD.GetirFirmaUnaviYevmiyeDefterinden(sunucuAdi
                                                                            , veriTabaniAdi
                                                                            , kullaniciAdi
                                                                            , kullaniciSifre);
                string yilBilgisi = CRUD.GetirYilBilgisiYevmiyeDefterinden(sunucuAdi
                                                                            , veriTabaniAdi
                                                                            , kullaniciAdi
                                                                            , kullaniciSifre);
                 raporAd = firmaUnvani + " " + yilBilgisi;
                //Geniş kapsamlı (high)...
                //Dar kapsamlı (low)...
                //RaporKaydet(denetim, "MD", "4. ÇEYREK", "high");
                denetim.yilAd = yilBilgisi;
                RaporKaydet(denetim
                            , raporAd
                            , firmaUnvani
                            , kapsam.KISA_AD
                            , donemAy.ACIKLAMA
                            , altKapsamList
                            , "low"
                            , sunucuAdi
                            , veriTabaniAdi
                            , kullaniciAdi
                            , kullaniciSifre);

                ////RaporKaydet(denetim, "VD", "4. ÇEYREK", "high");
                //RaporKaydet(denetim, "VD", "4. ÇEYREK", "low");

                ////RaporKaydet(denetim, "HD", "4. ÇEYREK", "high");
                //RaporKaydet(denetim, "HD", "4. ÇEYREK", "low");

                ////RaporKaydet(denetim, "PMA", "4. ÇEYREK", "high");
                //RaporKaydet(denetim, "PMA", "4. ÇEYREK", "low");
                //Eski
                #region
                /*
                MsSqlConnectionParameters connectionParameters 
                    = new MsSqlConnectionParameters(parametreler.Sabitler.sunucuAdiR
                                                   , parametreler.Sabitler.veriTabaniAdiR
                                                   , parametreler.Sabitler.kullaniciAdiR
                                                   , parametreler.Sabitler.kullaniciSifreR
                                                   , MsSqlAuthorizationType.SqlServer);

                PdfExportOptions pdfExportOptions = new PdfExportOptions() { PdfACompatibility = PdfACompatibility.PdfA1b };

                //XtraReport report1 = XtraReport.FromFile(Server.MapPath("~/Reports/" + "giris" + ".repx"), true);
                XtraReport report1 = XtraReport.FromFile(Server.MapPath("~/Reports/" + "girisYeni2" + ".repx"), true);
                report1.Parameters["parameter1"].Value = denetim.yilAd;
                report1.Parameters["parameter1"].Visible = false;
                report1.Parameters["parameter2"].Value = "4. Çeyrek";
                report1.Parameters["parameter2"].Visible = false;
                report1.Parameters["parameter3"].Value = CRUD.GetirFirmaIdIle(Session["firmaId"].ToString()).UNVAN;
                report1.Parameters["parameter3"].Visible = false;
                report1.Parameters["parameter4"].Value = "MUHASEBE DENETİMİ";
                report1.Parameters["parameter4"].Visible = false;
                report1.Parameters["parameter5"].Value = denetim.TARIH;
                report1.Parameters["parameter5"].Visible = false;
                report1.CreateDocument(false);
                XtraReport report2 = XtraReport.FromFile(Server.MapPath("~/Reports/" + "entryDetail_Id" + ".repx"), true);
                SqlDataSource ds2 = report2.DataSource as SqlDataSource;
                ds2.ConnectionParameters = connectionParameters;
                report2.CreateDocument(false);
                XtraReport report3 = XtraReport.FromFile(Server.MapPath("~/Reports/" + "mizan" + ".repx"), true);
                SqlDataSource ds3 = report3.DataSource as SqlDataSource;
                ds3.ConnectionParameters = connectionParameters;
                report3.CreateDocument(false);
                XtraReport report4 = XtraReport.FromFile(Server.MapPath("~/Reports/" + "detaysiz" + ".repx"), true);
                SqlDataSource ds4 = report4.DataSource as SqlDataSource;
                ds4.ConnectionParameters = connectionParameters;
                report4.CreateDocument(false);

                report1.Pages.AddRange(report2.Pages);
                report1.Pages.AddRange(report3.Pages);
                report1.Pages.AddRange(report4.Pages);

                string pdfExportFile = Server.MapPath("~/Pdf/" + denetim.AD + ".pdf");
                report1.ExportToPdf(pdfExportFile, pdfExportOptions);

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
                */
                #endregion

            }
            catch (Exception ex)
            {
                CRUD.WriteLog(denetim.LOGICALREF, ex.Message.ToString(), "RaporOlustur");
            }
            return raporAd;
        }


        public void TablolariOlustur(DataSet ds)
        {
            List<Thread> threadList = new List<Thread>();
            List<Thread> threadListExtra = new List<Thread>();

            List<Thread> threadListOne = new List<Thread>();
            //List<Thread> threadListTwo = new List<Thread>();
            //List<Thread> threadListThree = new List<Thread>();
            //List<Thread> threadListFour = new List<Thread>();

            List<Thread> threadListOneExtra = new List<Thread>();
            //List<Thread> threadListTwoExtra = new List<Thread>();
            //List<Thread> threadListThreeExtra = new List<Thread>();
            //List<Thread> threadListFourExtra = new List<Thread>();

            try
            {
                foreach (DataTable tablo in ds.Tables)
                {
                    if (!tablo.TableName.Equals("entryDetail")
                        && !tablo.TableName.Equals("accountSub")
                        && !tablo.TableName.Equals("account")
                        && !tablo.TableName.Equals("lineNumberCounter")
                        && !tablo.TableName.Equals("amount")
                        && !tablo.TableName.Equals("entryHeader")
                        && !tablo.TableName.Equals("entryNumberCounter")
                        && !tablo.TableName.Equals("totalCredit")
                        && !tablo.TableName.Equals("totalDebit"))
                    {
                        threadListOne.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW1
                                                                                    , Sabitler.veriTabaniAdiW1
                                                                                    , Sabitler.kullaniciAdiW1
                                                                                    , Sabitler.kullaniciSifreW1, tablo)));
                        //threadListTwo.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW2
                        //                                                            , Sabitler.veriTabaniAdiW2
                        //                                                            , Sabitler.kullaniciAdiW2
                        //                                                            , Sabitler.kullaniciSifreW2, tablo)));
                        //threadListThree.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW3
                        //                                                            , Sabitler.veriTabaniAdiW3
                        //                                                            , Sabitler.kullaniciAdiW3
                        //                                                            , Sabitler.kullaniciSifreW3, tablo)));
                        //threadListFour.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW4
                        //                                                            , Sabitler.veriTabaniAdiW4
                        //                                                            , Sabitler.kullaniciAdiW4
                        //                                                            , Sabitler.kullaniciSifreW4, tablo)));
                    }
                }

                threadList.AddRange(threadListOne);
                //threadList.AddRange(threadListTwo);
                //threadList.AddRange(threadListThree);
                //threadList.AddRange(threadListFour);

                foreach (Thread threadSiradaki in threadList)
                { threadSiradaki.Start(); }

                foreach (Thread threadSiradaki in threadList)
                { threadSiradaki.Join(); }

                ////////////////////////////////////////////////////////////////////////////////////

                //Uzun sürecek tabloları ayrıca çalıştırıyoruz...
                foreach (DataTable tablo in ds.Tables)
                {
                    if (tablo.TableName.Equals("entryDetail")
                        || tablo.TableName.Equals("accountSub")
                        || tablo.TableName.Equals("account")
                        || tablo.TableName.Equals("lineNumberCounter")
                        || tablo.TableName.Equals("amount")
                        || tablo.TableName.Equals("entryHeader")
                        || tablo.TableName.Equals("entryNumberCounter")
                        || tablo.TableName.Equals("totalCredit")
                        || tablo.TableName.Equals("totalDebit"))
                    {
                        threadListOneExtra.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW1
                                                                                        , Sabitler.veriTabaniAdiW1
                                                                                        , Sabitler.kullaniciAdiW1
                                                                                        , Sabitler.kullaniciSifreW1, tablo)));

                        //threadListTwoExtra.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW2
                        //                                                            , Sabitler.veriTabaniAdiW2
                        //                                                            , Sabitler.kullaniciAdiW2
                        //                                                            , Sabitler.kullaniciSifreW2, tablo)));
                        //threadListThreeExtra.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW3
                        //                                                            , Sabitler.veriTabaniAdiW3
                        //                                                            , Sabitler.kullaniciAdiW3
                        //                                                            , Sabitler.kullaniciSifreW3, tablo)));
                        //threadListFourExtra.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW4
                        //                                                            , Sabitler.veriTabaniAdiW4
                        //                                                            , Sabitler.kullaniciAdiW4
                        //                                                            , Sabitler.kullaniciSifreW4, tablo)));
                    }
                }

                threadListExtra.AddRange(threadListOneExtra);
                //threadListExtra.AddRange(threadListTwoExtra);
                //threadListExtra.AddRange(threadListThreeExtra);
                //threadListExtra.AddRange(threadListFourExtra);

                foreach (Thread threadSiradaki in threadListExtra)
                { threadSiradaki.Start(); }

                foreach (Thread threadSiradaki in threadListExtra)
                { threadSiradaki.Join(); }

                //Eski...
                #region
                /*
                threadList.AddRange(threadListOne);

                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    threadListTwo.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW2
                                                                            , Sabitler.veriTabaniAdiW2
                                                                            , Sabitler.kullaniciAdiW2
                                                                            , Sabitler.kullaniciSifreW2, ds.Tables[i])));
                }
                threadList.AddRange(threadListTwo);

                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    threadListThree.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW3
                                                                            , Sabitler.veriTabaniAdiW3
                                                                            , Sabitler.kullaniciAdiW3
                                                                            , Sabitler.kullaniciSifreW3, ds.Tables[i])));
                }
                threadList.AddRange(threadListThree);

                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    threadListFour.Add(new Thread(() => XMLdenTabloOlusturThread(Sabitler.sunucuAdiW4
                                                                            , Sabitler.veriTabaniAdiW4
                                                                            , Sabitler.kullaniciAdiW4
                                                                            , Sabitler.kullaniciSifreW4, ds.Tables[i])));
                }
                threadList.AddRange(threadListFour);
                */
                #endregion

                //Eski
                #region
                /*
                foreach (Thread threadSiradaki in threadListOne)
                {
                    threadSiradaki.Start();
                }
                foreach (Thread threadSiradaki in threadListTwo)
                {
                    threadSiradaki.Start();
                }
                foreach (Thread threadSiradaki in threadListThree)
                {
                    threadSiradaki.Start();
                }
                foreach (Thread threadSiradaki in threadListFour)
                {
                    threadSiradaki.Start();
                }
                */
                #endregion

            }
            catch (Exception ex)
            {
                CRUD.WriteLog("denetimId", ex.Message.ToString(), "TablolariOlustur");
            }
        }

        public void MizanlariOlustur(string yil)
        {
            //Mizan1 oluşturuluyor...
            //önce varsa 4 tane mizan talosunu temizliyoruz...
            string query1 = @"      DELETE FROM MIZAN1;
                                    DELETE FROM MIZAN2;
                                    DELETE FROM MIZAN3;
                                    DELETE FROM MIZAN4;
                                   INSERT INTO MIZAN1
                                    (accountSubID
                                    ,debitSum
                                    ,creditSum
                                    ,fark)
                                    select  
                                    isnull(TABLO1.debitAccountSubID,TABLO2.creditAccountSubID) as accountSubID,
                                    isnull(TABLO1.debitSum,0) as debitSum, 
                                    isnull(TABLO2.creditSum,0) as creditSum,
                                    isnull(TABLO1.debitSum,0)-isnull(TABLO2.creditSum,0) as fark 
                                    from
                                    (
                                    select  acs.accountSubID as debitAccountSubID,
                                    SUM(convert(DECIMAL(20,4),amt.amount_Text)) as debitSum 
                                    from accountSub acs 
                                    inner join account ac on ac.account_Id = acs.account_Id 
                                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'D' 
                                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-03-31')  
                                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                    group by acs.accountSubID
                                    )TABLO1
                                    full join 
                                    (
                                    select  acs.accountSubID as creditAccountSubID, 
                                    SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
                                    from accountSub acs 
                                    inner join account ac on ac.account_Id = acs.account_Id 
                                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
                                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-03-31') 
                                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                    group by acs.accountSubID
                                    )TABLO2 on TABLO1.debitAccountSubID = TABLO2.creditAccountSubID
                                    order by TABLO1.debitAccountSubID 
                                ";
            query1 = query1.Replace("@yil", yil);

            //Mizan2 oluşturuluyor...
            string query2 = @"  INSERT INTO MIZAN2
                                (accountSubID
                                ,debitSum
                                ,creditSum
                                ,fark)
                                select  
                                isnull(TABLO1.debitAccountSubID,TABLO2.creditAccountSubID) as accountSubID,
                                isnull(TABLO1.debitSum,0) as debitSum, 
                                isnull(TABLO2.creditSum,0) as creditSum,
                                isnull(TABLO1.debitSum,0)-isnull(TABLO2.creditSum,0) as fark 
                                from
                                (
                                select  acs.accountSubID as debitAccountSubID,
                                SUM(convert(DECIMAL(20,4),amt.amount_Text)) as debitSum 
                                from accountSub acs 
                                inner join account ac on ac.account_Id = acs.account_Id 
                                inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'D' 
                                inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-06-30')  
                                inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                group by acs.accountSubID
                                )TABLO1
                                full join 
                                (
                                select  acs.accountSubID as creditAccountSubID, 
                                SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
                                from accountSub acs 
                                inner join account ac on ac.account_Id = acs.account_Id 
                                inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
                                inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-06-30') 
                                inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                group by acs.accountSubID
                                )TABLO2 on TABLO1.debitAccountSubID = TABLO2.creditAccountSubID
                                order by TABLO1.debitAccountSubID
                        ";
            query2 = query2.Replace("@yil", yil);

            //Mizan3 oluşturuluyor...
            string query3 = @" INSERT INTO MIZAN3
                                (accountSubID
                                ,debitSum
                                ,creditSum
                                ,fark)

                                select  
                                isnull(TABLO1.debitAccountSubID,TABLO2.creditAccountSubID) as accountSubID,
                                isnull(TABLO1.debitSum,0) as debitSum, 
                                isnull(TABLO2.creditSum,0) as creditSum,
                                isnull(TABLO1.debitSum,0)-isnull(TABLO2.creditSum,0) as fark 
                                from
                                (
                                select  acs.accountSubID as debitAccountSubID,
                                SUM(convert(DECIMAL(20,4),amt.amount_Text)) as debitSum 
                                from accountSub acs 
                                inner join account ac on ac.account_Id = acs.account_Id 
                                inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'D' 
                                inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-09-30')  
                                inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                group by acs.accountSubID
                                )TABLO1
                                full join 
                                (
                                select  acs.accountSubID as creditAccountSubID, 
                                SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
                                from accountSub acs 
                                inner join account ac on ac.account_Id = acs.account_Id 
                                inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
                                inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-09-30') 
                                inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                group by acs.accountSubID
                                )TABLO2 on TABLO1.debitAccountSubID = TABLO2.creditAccountSubID
                                order by TABLO1.debitAccountSubID ";
            query3 = query3.Replace("@yil", yil);

            //Mizan4 oluşturuluyor...
            //mizan4 ü hesap ederken son yevmiye fişini hesaba katmıyoruz(kapanış fişi)...
            string query4 = @"  INSERT INTO MIZAN4
                                (accountSubID
                                ,debitSum
                                ,creditSum
                                ,fark)

                                select  
                                isnull(TABLO1.debitAccountSubID,TABLO2.creditAccountSubID) as accountSubID,
                                isnull(TABLO1.debitSum,0) as debitSum, 
                                isnull(TABLO2.creditSum,0) as creditSum,
                                isnull(TABLO1.debitSum,0)-isnull(TABLO2.creditSum,0) as fark 
                                from
                                (
                                select  acs.accountSubID as debitAccountSubID,
                                SUM(convert(DECIMAL(20,4),amt.amount_Text)) as debitSum 
                                from accountSub acs 
                                inner join account ac on ac.account_Id = acs.account_Id 
                                inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'D' 
                                inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-12-31')  
                                inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id and enc.entryNumberCounter_Text < (select MAX(convert(int,enc.entryNumberCounter_Text))
																											                                   from entryHeader eh
																											                                   inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id)
                                group by acs.accountSubID
                                )TABLO1
                                full join 
                                (
                                select acs.accountSubID as creditAccountSubID, 
                                SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
                                from accountSub acs 
                                inner join account ac on ac.account_Id = acs.account_Id 
                                inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
                                inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-12-31') 
                                inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id and enc.entryNumberCounter_Text < (select MAX(convert(int,enc.entryNumberCounter_Text))
																											                                   from entryHeader eh
																											                                   inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id)
                                group by acs.accountSubID
                                )TABLO2 on TABLO1.debitAccountSubID = TABLO2.creditAccountSubID
                                order by TABLO1.debitAccountSubID ";
            query4 = query4.Replace("@yil", yil);

            try
            {
                //4 tane veri tabanına mizan oluşturuyoruz...
                Thread t1 = new Thread(() => MizanServisiCagir(yil,
                                                               query1,
                                                               Sabitler.mizanServisEndpoint1,
                                                               Sabitler.sunucuAdiW1,
                                                               Sabitler.veriTabaniAdiW1,
                                                               Sabitler.kullaniciAdiW1,
                                                               Sabitler.kullaniciSifreW1));

                Thread t2 = new Thread(() => MizanServisiCagir(yil,
                                                               query2,
                                                               Sabitler.mizanServisEndpoint2,
                                                               Sabitler.sunucuAdiW1,
                                                               Sabitler.veriTabaniAdiW1,
                                                               Sabitler.kullaniciAdiW1,
                                                               Sabitler.kullaniciSifreW1));

                Thread t3 = new Thread(() => MizanServisiCagir(yil,
                                                               query3,
                                                               Sabitler.mizanServisEndpoint3,
                                                               Sabitler.sunucuAdiW1,
                                                               Sabitler.veriTabaniAdiW1,
                                                               Sabitler.kullaniciAdiW1,
                                                               Sabitler.kullaniciSifreW1));

                Thread t4 = new Thread(() => MizanServisiCagir(yil,
                                                               query4,
                                                               Sabitler.mizanServisEndpoint4,
                                                               Sabitler.sunucuAdiW1,
                                                               Sabitler.veriTabaniAdiW1,
                                                               Sabitler.kullaniciAdiW1,
                                                               Sabitler.kullaniciSifreW1));

                t1.Start();
                t2.Start();
                t3.Start();
                t4.Start();

                t1.Join();
                t2.Join();
                t3.Join();
                t4.Join();
            }
            catch (Exception ex)
            {
                CRUD.WriteLog("denetimId", ex.Message.ToString(), "MizanlariOlustur");
            }
        }

        public void KuralUygula(string denetimId, string yil, List<List<DENETIM_KURALLARI>> list)
        {
            try
            {
                CRUD.TemizleDenetimSonucList(Sabitler.sunucuAdiW1
                                            , Sabitler.veriTabaniAdiW1
                                            , Sabitler.kullaniciAdiW1
                                            , Sabitler.kullaniciSifreW1);

                CRUD.SilDenetimKuralCalismayanlarDenetimIdIle(denetimId);

                List<Thread> threadList = new List<Thread>();
                //4 defa yapılacak...
                if (list.Count > 0)
                {
                    List<Thread> threadListOne = KurallariUygula(list[0]
                                                            , yil
                                                            , Sabitler.sunucuAdiW1
                                                            , Sabitler.veriTabaniAdiW1
                                                            , Sabitler.kullaniciAdiW1
                                                            , Sabitler.kullaniciSifreW1
                                                            , Sabitler.kuralServisEndpoint1
                                                            , denetimId);
                    threadList.AddRange(threadListOne);
                }
                if (list.Count > 1)
                {
                    List<Thread> threadListTwo = KurallariUygula(list[1]
                                                            , yil
                                                            , Sabitler.sunucuAdiW1
                                                            , Sabitler.veriTabaniAdiW1
                                                            , Sabitler.kullaniciAdiW1
                                                            , Sabitler.kullaniciSifreW1
                                                            , Sabitler.kuralServisEndpoint2
                                                            , denetimId);
                    threadList.AddRange(threadListTwo);

                }
                if (list.Count > 2)
                {
                    List<Thread> threadListThree = KurallariUygula(list[2]
                                                            , yil
                                                            , Sabitler.sunucuAdiW1
                                                            , Sabitler.veriTabaniAdiW1
                                                            , Sabitler.kullaniciAdiW1
                                                            , Sabitler.kullaniciSifreW1
                                                            , Sabitler.kuralServisEndpoint3
                                                            , denetimId);
                    threadList.AddRange(threadListThree);

                }
                if (list.Count > 3)
                {
                    List<Thread> threadListFour = KurallariUygula(list[3]
                                                            , yil
                                                            , Sabitler.sunucuAdiW1
                                                            , Sabitler.veriTabaniAdiW1
                                                            , Sabitler.kullaniciAdiW1
                                                            , Sabitler.kullaniciSifreW1
                                                            , Sabitler.kuralServisEndpoint4
                                                            , denetimId);
                    threadList.AddRange(threadListFour);

                }
                if (list.Count > 4)
                {
                    List<Thread> threadListFive = KurallariUygula(list[4]
                                                            , yil
                                                            , Sabitler.sunucuAdiW1
                                                            , Sabitler.veriTabaniAdiW1
                                                            , Sabitler.kullaniciAdiW1
                                                            , Sabitler.kullaniciSifreW1
                                                            , Sabitler.kuralServisEndpoint5
                                                            , denetimId);
                    threadList.AddRange(threadListFive);

                }
                if (list.Count > 5)
                {
                    List<Thread> threadListSix = KurallariUygula(list[5]
                                                        , yil
                                                        , Sabitler.sunucuAdiW1
                                                        , Sabitler.veriTabaniAdiW1
                                                        , Sabitler.kullaniciAdiW1
                                                        , Sabitler.kullaniciSifreW1
                                                        , Sabitler.kuralServisEndpoint6
                                                        , denetimId);
                    threadList.AddRange(threadListSix);

                }
                if (list.Count > 6)
                {
                    List<Thread> threadListSeven = KurallariUygula(list[6]
                                                            , yil
                                                            , Sabitler.sunucuAdiW1
                                                            , Sabitler.veriTabaniAdiW1
                                                            , Sabitler.kullaniciAdiW1
                                                            , Sabitler.kullaniciSifreW1
                                                            , Sabitler.kuralServisEndpoint7
                                                            , denetimId);
                    threadList.AddRange(threadListSeven);

                }
                if (list.Count > 7)
                {
                    List<Thread> threadListEight = KurallariUygula(list[7]
                                                             , yil
                                                             , Sabitler.sunucuAdiW1
                                                             , Sabitler.veriTabaniAdiW1
                                                             , Sabitler.kullaniciAdiW1
                                                             , Sabitler.kullaniciSifreW1
                                                             , Sabitler.kuralServisEndpoint8
                                                             , denetimId);
                    threadList.AddRange(threadListEight);

                }
                if (list.Count > 8)
                {
                    List<Thread> threadListNine = KurallariUygula(list[8]
                                                              , yil
                                                              , Sabitler.sunucuAdiW1
                                                              , Sabitler.veriTabaniAdiW1
                                                              , Sabitler.kullaniciAdiW1
                                                              , Sabitler.kullaniciSifreW1
                                                              , Sabitler.kuralServisEndpoint9
                                                              , denetimId);
                    threadList.AddRange(threadListNine);

                }
                if (list.Count > 9)
                {
                    List<Thread> threadListTen = KurallariUygula(list[9]
                                                              , yil
                                                              , Sabitler.sunucuAdiW1
                                                              , Sabitler.veriTabaniAdiW1
                                                              , Sabitler.kullaniciAdiW1
                                                              , Sabitler.kullaniciSifreW1
                                                              , Sabitler.kuralServisEndpoint10
                                                              , denetimId);
                    threadList.AddRange(threadListTen);

                }
                if (list.Count > 10)
                {
                    List<Thread> threadListEleven = KurallariUygula(list[10]
                                                              , yil
                                                              , Sabitler.sunucuAdiW1
                                                              , Sabitler.veriTabaniAdiW1
                                                              , Sabitler.kullaniciAdiW1
                                                              , Sabitler.kullaniciSifreW1
                                                              , Sabitler.kuralServisEndpoint11
                                                              , denetimId);
                    threadList.AddRange(threadListEleven);

                }
                if (list.Count > 11)
                {
                    List<Thread> threadListTwelve = KurallariUygula(list[11]
                                                              , yil
                                                              , Sabitler.sunucuAdiW1
                                                              , Sabitler.veriTabaniAdiW1
                                                              , Sabitler.kullaniciAdiW1
                                                              , Sabitler.kullaniciSifreW1
                                                              , Sabitler.kuralServisEndpoint12
                                                              , denetimId);
                    threadList.AddRange(threadListTwelve);

                }
                if (list.Count > 12)
                {
                    List<Thread> threadListThirteen = KurallariUygula(list[12]
                                                              , yil
                                                              , Sabitler.sunucuAdiW1
                                                              , Sabitler.veriTabaniAdiW1
                                                              , Sabitler.kullaniciAdiW1
                                                              , Sabitler.kullaniciSifreW1
                                                              , Sabitler.kuralServisEndpoint13
                                                              , denetimId);
                    threadList.AddRange(threadListThirteen);

                }
                if (list.Count > 13)
                {
                    List<Thread> threadListFourteen = KurallariUygula(list[13]
                                                            , yil
                                                            , Sabitler.sunucuAdiW1
                                                            , Sabitler.veriTabaniAdiW1
                                                            , Sabitler.kullaniciAdiW1
                                                            , Sabitler.kullaniciSifreW1
                                                            , Sabitler.kuralServisEndpoint14
                                                            , denetimId);
                    threadList.AddRange(threadListFourteen);

                }
                if (list.Count > 14)
                {
                    List<Thread> threadListFifteen = KurallariUygula(list[14]
                                                            , yil
                                                            , Sabitler.sunucuAdiW1
                                                            , Sabitler.veriTabaniAdiW1
                                                            , Sabitler.kullaniciAdiW1
                                                            , Sabitler.kullaniciSifreW1
                                                            , Sabitler.kuralServisEndpoint15
                                                            , denetimId);
                    threadList.AddRange(threadListFifteen);

                }
                if (list.Count > 15)
                {
                    List<Thread> threadListSixteen = KurallariUygula(list[15]
                                                              , yil
                                                              , Sabitler.sunucuAdiW1
                                                              , Sabitler.veriTabaniAdiW1
                                                              , Sabitler.kullaniciAdiW1
                                                              , Sabitler.kullaniciSifreW1
                                                              , Sabitler.kuralServisEndpoint16
                                                              , denetimId);
                    threadList.AddRange(threadListSixteen);
                }


                foreach (Thread threadSiradaki in threadList)
                { threadSiradaki.Start(); }

                foreach (Thread threadSiradaki in threadList)
                { threadSiradaki.Join(); }
            }
            catch (Exception ex)
            {
                CRUD.WriteLog(denetimId, ex.Message.ToString(), "KuralUygula");
            }
        }

        public void KuralUygulaCalismayanlar(string denetimId, string yil, List<List<DENETIM_KURALLARI>> list)
        {
            try
            {
                List<Thread> threadList = new List<Thread>();

                if (list.Count > 0)
                {
                    List<Thread> threadListOne = KurallariUygula(list[0]
                                                                , yil
                                                                , Sabitler.sunucuAdiW1
                                                                , Sabitler.veriTabaniAdiW1
                                                                , Sabitler.kullaniciAdiW1
                                                                , Sabitler.kullaniciSifreW1
                                                                , Sabitler.kuralServisEndpoint1
                                                                , denetimId);
                    threadList.AddRange(threadListOne);
                }
                if (list.Count > 1)
                {
                    List<Thread> threadListTwo = KurallariUygula(list[1]
                                                            , yil
                                                            , Sabitler.sunucuAdiW1
                                                            , Sabitler.veriTabaniAdiW1
                                                            , Sabitler.kullaniciAdiW1
                                                            , Sabitler.kullaniciSifreW1
                                                            , Sabitler.kuralServisEndpoint2
                                                            , denetimId);
                    threadList.AddRange(threadListTwo);
                }
                if (list.Count > 2)
                {
                    List<Thread> threadListThree = KurallariUygula(list[2]
                                                        , yil
                                                        , Sabitler.sunucuAdiW1
                                                        , Sabitler.veriTabaniAdiW1
                                                        , Sabitler.kullaniciAdiW1
                                                        , Sabitler.kullaniciSifreW1
                                                        , Sabitler.kuralServisEndpoint3
                                                        , denetimId);
                    threadList.AddRange(threadListThree);
                }
                if (list.Count > 3)
                {
                    List<Thread> threadListFour = KurallariUygula(list[3]
                                                    , yil
                                                    , Sabitler.sunucuAdiW1
                                                    , Sabitler.veriTabaniAdiW1
                                                    , Sabitler.kullaniciAdiW1
                                                    , Sabitler.kullaniciSifreW1
                                                    , Sabitler.kuralServisEndpoint4
                                                    , denetimId);
                    threadList.AddRange(threadListFour);
                }
                if (list.Count > 4)
                {
                    List<Thread> threadListFive = KurallariUygula(list[4]
                                                , yil
                                                , Sabitler.sunucuAdiW1
                                                , Sabitler.veriTabaniAdiW1
                                                , Sabitler.kullaniciAdiW1
                                                , Sabitler.kullaniciSifreW1
                                                , Sabitler.kuralServisEndpoint5
                                                , denetimId);
                    threadList.AddRange(threadListFive);
                }
                if (list.Count > 5)
                {
                    List<Thread> threadListSix = KurallariUygula(list[5]
                                            , yil
                                            , Sabitler.sunucuAdiW1
                                            , Sabitler.veriTabaniAdiW1
                                            , Sabitler.kullaniciAdiW1
                                            , Sabitler.kullaniciSifreW1
                                            , Sabitler.kuralServisEndpoint6
                                            , denetimId);
                    threadList.AddRange(threadListSix);
                }
                if (list.Count > 6)
                {
                    List<Thread> threadListSeven = KurallariUygula(list[6]
                                                , yil
                                                , Sabitler.sunucuAdiW1
                                                , Sabitler.veriTabaniAdiW1
                                                , Sabitler.kullaniciAdiW1
                                                , Sabitler.kullaniciSifreW1
                                                , Sabitler.kuralServisEndpoint7
                                                , denetimId);
                    threadList.AddRange(threadListSeven);
                }
                if (list.Count > 7)
                {
                    List<Thread> threadListEight = KurallariUygula(list[7]
                                                 , yil
                                                 , Sabitler.sunucuAdiW1
                                                 , Sabitler.veriTabaniAdiW1
                                                 , Sabitler.kullaniciAdiW1
                                                 , Sabitler.kullaniciSifreW1
                                                 , Sabitler.kuralServisEndpoint8
                                                 , denetimId);
                    threadList.AddRange(threadListEight);
                }
                if (list.Count > 8)
                {
                    List<Thread> threadListNine = KurallariUygula(list[8]
                                                  , yil
                                                  , Sabitler.sunucuAdiW1
                                                  , Sabitler.veriTabaniAdiW1
                                                  , Sabitler.kullaniciAdiW1
                                                  , Sabitler.kullaniciSifreW1
                                                  , Sabitler.kuralServisEndpoint9
                                                  , denetimId);
                    threadList.AddRange(threadListNine);
                }
                if (list.Count > 9)
                {
                    List<Thread> threadListTen = KurallariUygula(list[9]
                                                  , yil
                                                  , Sabitler.sunucuAdiW1
                                                  , Sabitler.veriTabaniAdiW1
                                                  , Sabitler.kullaniciAdiW1
                                                  , Sabitler.kullaniciSifreW1
                                                  , Sabitler.kuralServisEndpoint10
                                                  , denetimId);
                    threadList.AddRange(threadListTen);
                }
                if (list.Count > 10)
                {
                    List<Thread> threadListEleven = KurallariUygula(list[10]
                                                  , yil
                                                  , Sabitler.sunucuAdiW1
                                                  , Sabitler.veriTabaniAdiW1
                                                  , Sabitler.kullaniciAdiW1
                                                  , Sabitler.kullaniciSifreW1
                                                  , Sabitler.kuralServisEndpoint11
                                                  , denetimId);
                    threadList.AddRange(threadListEleven);
                }
                if (list.Count > 11)
                {
                    List<Thread> threadListTwelve = KurallariUygula(list[11]
                                                  , yil
                                                  , Sabitler.sunucuAdiW1
                                                  , Sabitler.veriTabaniAdiW1
                                                  , Sabitler.kullaniciAdiW1
                                                  , Sabitler.kullaniciSifreW1
                                                  , Sabitler.kuralServisEndpoint12
                                                  , denetimId);
                    threadList.AddRange(threadListTwelve);
                }
                if (list.Count > 12)
                {
                    List<Thread> threadListThirteen = KurallariUygula(list[12]
                                                  , yil
                                                  , Sabitler.sunucuAdiW1
                                                  , Sabitler.veriTabaniAdiW1
                                                  , Sabitler.kullaniciAdiW1
                                                  , Sabitler.kullaniciSifreW1
                                                  , Sabitler.kuralServisEndpoint13
                                                  , denetimId);
                    threadList.AddRange(threadListThirteen);
                }
                if (list.Count > 13)
                {
                    List<Thread> threadListFourteen = KurallariUygula(list[13]
                                                , yil
                                                , Sabitler.sunucuAdiW1
                                                , Sabitler.veriTabaniAdiW1
                                                , Sabitler.kullaniciAdiW1
                                                , Sabitler.kullaniciSifreW1
                                                , Sabitler.kuralServisEndpoint14
                                                , denetimId);
                    threadList.AddRange(threadListFourteen);
                }
                if (list.Count > 14)
                {
                    List<Thread> threadListFifteen = KurallariUygula(list[14]
                                                , yil
                                                , Sabitler.sunucuAdiW1
                                                , Sabitler.veriTabaniAdiW1
                                                , Sabitler.kullaniciAdiW1
                                                , Sabitler.kullaniciSifreW1
                                                , Sabitler.kuralServisEndpoint15
                                                , denetimId);
                    threadList.AddRange(threadListFifteen);
                }
                if (list.Count > 15)
                {
                    List<Thread> threadListSixteen = KurallariUygula(list[15]
                                                  , yil
                                                  , Sabitler.sunucuAdiW1
                                                  , Sabitler.veriTabaniAdiW1
                                                  , Sabitler.kullaniciAdiW1
                                                  , Sabitler.kullaniciSifreW1
                                                  , Sabitler.kuralServisEndpoint16
                                                  , denetimId);
                    threadList.AddRange(threadListSixteen);
                }



                foreach (Thread threadSiradaki in threadList)
                { threadSiradaki.Start(); }

                foreach (Thread threadSiradaki in threadList)
                { threadSiradaki.Join(); }
            }
            catch (Exception ex)
            {

            }
        }

        public void MizanServisiCagirThread(string SQL, string sunucuAdi, string veriTabaniAdi,
                                            string kullaniciAdi, string kullaniciSifre, string endpoint)
        {
            try
            {
                string replacement = Regex.Replace(SQL, @"\t", "\\t");
                string replacement1 = Regex.Replace(replacement, @"\n", "\\n");
                string replacement2 = Regex.Replace(replacement1, @"\r", "\\r");
                string result = "";

                string parametreler = " {\"SQL\":\"" + replacement2 + "\"" +
                                        ",\"sunucuAdi\":\"" + sunucuAdi + "\"" +
                                        ",\"sunucuPortNo\":\"" + "" + "\"" +
                                        ",\"veriTabaniAdi\":\"" + veriTabaniAdi + "\"" +
                                        ",\"kullaniciAdi\":\"" + kullaniciAdi + "\"" +
                                        ",\"kullaniciSifre\":\"" + kullaniciSifre + "\"" +
                                        "} ";

                HttpWebRequest req = WebRequest.Create(new Uri(endpoint)) as HttpWebRequest;
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Accept = "application/json";
                byte[] formData = UTF8Encoding.UTF8.GetBytes(parametreler);
                req.ContentLength = formData.Length;
                using (Stream post = req.GetRequestStream())
                {
                    post.Write(formData, 0, formData.Length);
                }
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                CRUD.WriteLog("denetimId", ex.Message.ToString(), "MizanServisiCagirThread");
            }
        }

        public void MizanServisiCagirTemizle(string SQL, string sunucuAdi, string veriTabaniAdi,
                                             string kullaniciAdi, string kullaniciSifre, string endpoint)
        {
            try
            {
                string replacement = Regex.Replace(SQL, @"\t", "\\t");
                string replacement1 = Regex.Replace(replacement, @"\n", "\\n");
                string replacement2 = Regex.Replace(replacement1, @"\r", "\\r");
                string result = "";
                string parametreler = " {\"SQL\":\"" + replacement2 + "\"" +
                                        ",\"sunucuAdi\":\"" + sunucuAdi + "\"" +
                                        ",\"sunucuPortNo\":\"" + "" + "\"" +
                                        ",\"veriTabaniAdi\":\"" + veriTabaniAdi + "\"" +
                                        ",\"kullaniciAdi\":\"" + kullaniciAdi + "\"" +
                                        ",\"kullaniciSifre\":\"" + kullaniciSifre + "\"" +
                                        "}";
                HttpWebRequest req = WebRequest.Create(new Uri(endpoint)) as HttpWebRequest;
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Accept = "application/json";
                byte[] formData = UTF8Encoding.UTF8.GetBytes(parametreler);
                req.ContentLength = formData.Length;
                using (Stream post = req.GetRequestStream())
                {
                    post.Write(formData, 0, formData.Length);
                }
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public bool CalisanThreadVarMi(List<Thread> threadList)
        {
            bool deger = false;
            try
            {
                foreach (Thread threadSiradaki in threadList)
                {
                    if (threadSiradaki.IsAlive)
                    {
                        deger = true;
                        break;
                    }
                }

                return deger;
            }
            catch (Exception ex)
            {
                return deger;
            }
        }

        [Authorize]
        public ActionResult SonRaporlar()
        {
            try
            {
                List<DENETIM> denetimList = new List<DENETIM>();
                try
                {
                    denetimList = CRUD.GetirSonRaporList(Session["LOGICALREF"].ToString());
                }
                catch (Exception e)
                {
                    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Son Raporlar Getirilirken  Hata Oluştu." };
                }
                return View(denetimList);
            }
            catch (Exception)
            {

                throw;
            }
            return View();
        }

        [Authorize]
        public ActionResult SonDenetimler()
        {
            List<DENETIM> denetimList = new List<DENETIM>();
            try
            {
                denetimList = CRUD.GetirSonDenetimList(Session["LOGICALREF"].ToString());
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Son Denetimler Getirilirken  Hata Oluştu." };
            }
            return View(denetimList);
        }

        [Authorize]
        public ActionResult KontorSatinAl()
        {
            List<KONTOR> kontorList = new List<KONTOR>();
            try
            {
                kontorList = CRUD.GetirAktifKontorListesi();
            }
            catch (Exception e)
            {

                throw;
            }
            return View(kontorList);
        }

        [Authorize]
        public ActionResult Iyzico(string id)
        {
            try
            {
                KONTOR kontor = CRUD.GetirKontorIdIle(id);
                KULLANICI kullanıcı = CRUD.GetirKullaniciEmailIle(Session["EMAIL"].ToString());
                Options options = new Options();
                options.ApiKey = "sandbox-OcGZiXuCxqgHsZjnNfgV5ft3i4Y55tHE";
                options.SecretKey = "sandbox-VMMs894oagmzwq8N2lhx2aBMOblWkUFJ";
                options.BaseUrl = "https://sandbox-api.iyzipay.com";

                CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest();
                request.Locale = Locale.TR.ToString();
                request.ConversationId = "123456789";
                request.Price = kontor.PAKET_FIYATI;
                request.PaidPrice = kontor.PAKET_FIYATI;
                request.Currency = Currency.TRY.ToString();
                request.BasketId = "B67832";
                request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
                request.CallbackUrl = "https://netdenet.azurewebsites.net/Kullanici/ResultPay?logicalref=" + kullanıcı.LOGICALREF + "&id=" + kontor.LOGICALREF;

                List<int> enabledInstallments = new List<int>();
                enabledInstallments.Add(2);
                enabledInstallments.Add(3);
                enabledInstallments.Add(6);
                enabledInstallments.Add(9);
                request.EnabledInstallments = enabledInstallments;

                Buyer buyer = new Buyer();
                buyer.Id = "BY789";
                buyer.Name = kullanıcı.AD;
                buyer.Surname = kullanıcı.SOYAD;
                buyer.GsmNumber = "+90" + kullanıcı.TELEFON;
                buyer.Email = kullanıcı.EMAIL;
                buyer.IdentityNumber = "74300864791";
                buyer.LastLoginDate = "2015-10-05 12:43:35";
                buyer.RegistrationDate = "2013-04-21 15:12:09";
                buyer.RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
                buyer.Ip = "85.34.78.112";
                buyer.City = "Istanbul";
                buyer.Country = "Turkey";
                buyer.ZipCode = "34732";
                request.Buyer = buyer;

                Address shippingAddress = new Address();
                shippingAddress.ContactName = kullanıcı.AD + " " + kullanıcı.SOYAD;
                shippingAddress.City = "Istanbul";
                shippingAddress.Country = "Turkey";
                shippingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
                shippingAddress.ZipCode = "34742";
                request.ShippingAddress = shippingAddress;

                Address billingAddress = new Address();
                billingAddress.ContactName = kullanıcı.AD + " " + kullanıcı.SOYAD;
                billingAddress.City = "Istanbul";
                billingAddress.Country = "Turkey";
                billingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
                billingAddress.ZipCode = "34742";
                request.BillingAddress = billingAddress;

                List<BasketItem> basketItems = new List<BasketItem>();
                BasketItem basketItem = new BasketItem();
                basketItem.Id = "BI103";
                basketItem.Name = "Kontör";
                basketItem.Category1 = "Paket";
                basketItem.Category2 = "Kontör";
                basketItem.ItemType = BasketItemType.VIRTUAL.ToString();
                basketItem.Price = kontor.PAKET_FIYATI;
                basketItems.Add(basketItem);
                request.BasketItems = basketItems;

                CheckoutFormInitialize checkoutFormInitialize = CheckoutFormInitialize.Create(request, options);
                ViewBag.Iyzico = checkoutFormInitialize.CheckoutFormContent;
                ViewBag.KontorMıktar = kontor.KONTOR_ADET;
                ViewBag.Paket = kontor.PAKET_FIYATI;

                return View();
            }
            catch (Exception e)
            {

                throw;
            }

        }

        public ActionResult ResultPay(RetrieveCheckoutFormRequest model, string logicalref, string id)
        {
            KONTOR_HAREKET kontorHareket = new KONTOR_HAREKET();
            try
            {
                sessionDoldur(logicalref);
                string data = "";
                Options options = new Options();
                options.ApiKey = "sandbox-OcGZiXuCxqgHsZjnNfgV5ft3i4Y55tHE";
                options.SecretKey = "sandbox-VMMs894oagmzwq8N2lhx2aBMOblWkUFJ";
                options.BaseUrl = "https://sandbox-api.iyzipay.com";
                data = model.Token;
                RetrieveCheckoutFormRequest request = new RetrieveCheckoutFormRequest();
                request.Token = data;
                CheckoutForm checkoutForm = CheckoutForm.Retrieve(request, options);
                string bilgiler = checkoutForm.ToString();
                if (checkoutForm.PaymentStatus == "SUCCESS")
                {

                    KONTOR kontor = CRUD.GetirKontorIdIle(id);
                    Guid obj = Guid.NewGuid();
                    kontorHareket.LOGICALREF = obj.ToString();
                    kontorHareket.KULLANICI_LOGICALREF = logicalref;
                    kontorHareket.ISLEM_ACIKLAMA = "Iyzico Ödeme Id : " + checkoutForm.PaymentId.ToString() +
                        //" | İşlem Kimliği : " + checkoutForm.ConversationId.ToString() +
                        " | Paket Tutarı : " + checkoutForm.PaidPrice.ToString();
                    kontorHareket.KONTOR_MIKTARI = kontor.KONTOR_ADET;
                    kontorHareket.ISLEM_TIPI = 0;//0 satın alma 1 harcama
                    kontorHareket.KULLANIM_DURUMU = 1;
                    CRUD.KontorHareketEkle(kontorHareket);
                    Session["BAKIYE"] = CRUD.GetirKullaniciBakiye(logicalref);
                }
                return RedirectToAction("Confirm", new RouteValueDictionary(
                                new { controller = "Kullanici", action = "Confirm", status = checkoutForm.PaymentStatus }));

            }
            catch (Exception e)
            {
                ViewBag.ConfMessage = "Ödeme İşlemi Sırasında Hata Oluştu";
            }
            return View();
        }

        public void sessionDoldur(string logicalref)
        {
            KULLANICI kayitliKullanici = new KULLANICI();
            try
            {
                kayitliKullanici = CRUD.KullaniciBilgileriniDoldurEmailIle(logicalref);
                FormsAuthentication.SetAuthCookie(kayitliKullanici.EMAIL, false);
                Session["LOGICALREF"] = kayitliKullanici.LOGICALREF;
                Session["EMAIL"] = kayitliKullanici.EMAIL;
                Session["AD"] = kayitliKullanici.AD;
                Session["SOYAD"] = kayitliKullanici.SOYAD;
                Session["TELEFON"] = kayitliKullanici.TELEFON;
                Session["RESIM"] = kayitliKullanici.resimSrc;
                if (kayitliKullanici.ADMIN_MI == 1)
                    Session["menuGoster"] = "";
                else
                    Session["menuGoster"] = "hidden";

            }
            catch (Exception e)
            {

            }
        }

        [Authorize]
        public ActionResult Confirm(string status)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(status))
                {
                    if (status == "SUCCESS")
                        ViewBag.ConfMessageSuccess = "Ödeme İşlemi Başarılı";
                    else
                        ViewBag.ConfMessage = "Ödeme İşlemi Sırasında Hata Oluştu";
                }
                else
                    return RedirectToAction("HomeIndex", "Home");
            }
            catch (Exception e)
            {

                throw;
            }
            return View();
        }

        public void KontorHareketKaydet(DENETIM denetim, YIL yil)
        {
            KONTOR_HAREKET kontorHareket = new KONTOR_HAREKET();
            DONEM_AY donemAy = CRUD.GetirDonemAyIdIle(denetim.DONEM_AY_LOGICALREF);
            try
            {
                List<DENETIM_KAPSAM> denetimKapsamList = CRUD.GetirDenetimKapsamListesiDenetimIdIle(denetim.LOGICALREF);
                string kapsam = "";
                foreach (DENETIM_KAPSAM item in denetimKapsamList)
                {
                    KAPSAM kapsamItem = CRUD.GetirKapsamIdIle(item.KAPSAM_LOGICALREF);
                    kapsam = kapsam + " " + kapsamItem.AD;
                }
                FIRMA firma = CRUD.GetirFirmaDenetimIdIle(denetim.LOGICALREF);
                Guid obj = Guid.NewGuid();
                kontorHareket.LOGICALREF = obj.ToString();
                kontorHareket.KULLANICI_LOGICALREF = Session["LOGICALREF"].ToString();
                kontorHareket.ISLEM_ACIKLAMA = "Firma Bilgileri : (" + firma.UNVAN + "-" + firma.VKNTCKN + ")" +
                    //" | İşlem Kimliği : " + checkoutForm.ConversationId.ToString() +
                    " | Denetim bilgileri : (" + denetim.AD + "-" + yil.YIL_AD + "/" + donemAy.ACIKLAMA +
                    "-" + denetim.TARIH + "-" + kapsam + ")";
                int toplamKontor = CRUD.GetirDenetimeHarcananKontorDenetimIdIle(denetim.LOGICALREF);
                kontorHareket.KONTOR_MIKTARI = -toplamKontor;
                kontorHareket.ISLEM_TIPI = 1;//0 satın alma 1 harcama
                kontorHareket.KULLANIM_DURUMU = 1;
                CRUD.KontorHareketEkle(kontorHareket);
                Session["BAKIYE"] = CRUD.GetirKullaniciBakiye(Session["LOGICALREF"].ToString());
            }
            catch (Exception e)
            {

                throw;
            }
        }
        public void BildirimKaydet(BILDIRIM bildirim)
        {

            try
            {
                CRUD.BildirimEkle(bildirim);
                Session["BILDIRIM_SAYISI"] = CRUD.GetirBildirimSayisi(Session["LOGICALREF"].ToString());
            }
            catch (Exception e)
            {

                throw;
            }
        }
        [Authorize]
        public ActionResult SonDenetimlerRapor(string denetimId)
        {
            List<RAPOR> raporList = new List<RAPOR>();
            List<DENETIM_RAPOR> denetimRaporList = new List<DENETIM_RAPOR>();
            try
            {
                DENETIM denetim = CRUD.GetirDenetimIdIle(denetimId);

                denetimRaporList = CRUD.GetirDenetimRaporListDenetimIdIle(denetimId);
                foreach (DENETIM_RAPOR denetimRapor in denetimRaporList)
                {
                    RAPOR rapor = new RAPOR();
                    rapor = CRUD.GetirRaporRaporIdIle(denetimRapor.RAPOR_LOGICALREF);
                    raporList.Add(rapor);
                }
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Rapor Getirilirken Hata Oluştu." };

            }
            return View(raporList);
        }

        [Authorize]
        public ActionResult KontorGecmisi()
        {
            List<KONTOR_HAREKET> kontorHareketList = new List<KONTOR_HAREKET>();
            try
            {
                kontorHareketList = CRUD.GetirKontorHareketListesi(Session["LOGICALREF"].ToString());
            }
            catch (Exception)
            {

                throw;
            }
            return View(kontorHareketList);
        }

        [Authorize]
        public ActionResult Bildirim()
        {
            List<BILDIRIM> bildirimList = new List<BILDIRIM>();
            try
            {
              
                bildirimList = CRUD.GetirBildirimListesi(Session["LOGICALREF"].ToString());
            }
            catch (Exception)
            {

                throw;
            }
            return View(bildirimList);
        }

        public ActionResult SendEmailDenetimDone(string denetimId, string raporAd)
        {
            string email = Session["EMAIL"].ToString();
            try
            {
                KULLANICI kullanici = CRUD.GetirKullaniciEmailIle(email);
                string adSoyad = kullanici.AD + " " + kullanici.SOYAD;
                string themessage = @"Sayın " + adSoyad + @"<br><br>Başlatmış olduğunuz "+raporAd+@" denetiminiz başarılı bir şekilde tamamlanmıştır.
                                   <br>Denetim raporlarına ulaşmak için <a href=""https://netdenet.azurewebsites.net/Kullanici/SonDenetimler""> tıklayınız</a> <br> 
                        <html>
                        <body>

                        <table width=""100%"">
                        <tr>
                            <td style=""font-style:arial; color:maroon; font-weight:bold"">
                            <br>
                            <img  src=""https://netdenet.azurewebsites.net/Content/adminlte/img/dkn96x96.png""/>
                            </td>
                        </tr>
                        </tr>
                        </table>
                        </body>
                        </html>";

                //string path = Server.MapPath("~/Content/adminlte/img");

                MailMessage ePosta = new MailMessage();
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(themessage, null, "text/html");
                ePosta.AlternateViews.Add(htmlView);

                ePosta.From = new MailAddress("zimmettakipsistemi@gmail.com", "NETDENET");
                ePosta.To.Add(email);
                ePosta.Subject = "NETDENET(Denetiminiz Tamamlandı.)";
                ePosta.IsBodyHtml = true;


                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("zimmettakipsistemi@gmail.com", "ehirbhiqubiauypl"); // ehirbhiqubiauypl iki adımlı doğrulamadan sonra gmail hesabına erişim için verilen pasword
                smtp.EnableSsl = true;
                smtp.Send(ePosta);


            }
            catch (SmtpException e)
            {

            }
            return null;
        }

        public static void SendWhatsappDenetimDone(string denetimId, string email)
        {
            string mesaj = "NETDENET!  Başlatmış olduğunuz denetiminiz başarılı bir şekilde sonuçlanmıştır." +
                            " Denetim raporlarına sisteme giriş yaparak ulaşabilirsiniz..";
            try
            {
                KULLANICI kullanici = CRUD.GetirKullaniciEmailIle(email);

                var client = new RestClient(parametreler.Sabitler.ultraMessageLink + "/chat");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("undefined", "token=" + parametreler.Sabitler.ultraMessageToken + "&to=" + kullanici.TELEFON + "&body=" + mesaj + "&priority=10&referenceId=", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);



            }
            catch (Exception e)
            {

                throw;
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
                    Session["yevmiyeDefteriListTemp"] = null;
                }
                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = " Yevmiye Defterlerii Silme İşlemi Başarılı." };
            }
            catch
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Yevmiye Defteri Silme İşleminde Hata Oluştu." };

            }
            return RedirectToAction("EditDenetim", new { id = Session["denetimId"].ToString() });
        }

        [Authorize]
        public ActionResult BildirimDetay(string id)
        {

            BILDIRIM kayitliBildirim = new BILDIRIM();
            try
            {
                kayitliBildirim = CRUD.GetirBildirimIdIle(id);
                kayitliBildirim.DURUMU = 0;//bİLDİRİMİ OKUNDU OLARAK İŞARETLİYORUM
                CRUD.GuncelleBildirim(kayitliBildirim);
                Session["BILDIRIM_SAYISI"] = CRUD.GetirBildirimSayisi(Session["LOGICALREF"].ToString());
            }
            catch (Exception e)
            {

            }
            return View(kayitliBildirim);
        }

        [Authorize]
        public ActionResult SilBildirim(string id)
        {

            BILDIRIM kayitliBildirim = new BILDIRIM();
            try
            {
                kayitliBildirim = CRUD.GetirBildirimIdIle(id);
                kayitliBildirim.KULLANIM_DURUMU = 0;//bİLDİRİMİ OKUNDU OLARAK İŞARETLİYORUM
                CRUD.GuncelleBildirim(kayitliBildirim);

            }
            catch (Exception e)
            {

            }
            return RedirectToAction("Bildirim");
        }

    }
}