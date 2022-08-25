using DKN.db;
using DKN.Models;
using DKN.parametreler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace DKN.Controllers
{
    public class GirisYapController : Controller
    {
        // GET: GirisYap
        public ActionResult Index()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(KULLANICI kullanici)
        {
            KULLANICI kayitliKullanici = new KULLANICI();
            String kullaniciSifresi = "";
            try
            {
                int count = CRUD.KullaniciVarMi(kullanici.EMAIL);
                if (count == 1)
                    kullaniciSifresi = CRUD.GetirKullaniciSifre(kullanici.EMAIL);

                if (count == 1 && (kullaniciSifresi.Equals(kullanici.SIFRE)))
                {
                    kayitliKullanici = CRUD.KullaniciBilgileriniDoldur(kullanici.EMAIL, kullanici.SIFRE);
                    Session["BAKIYE"] = CRUD.GetirKullaniciBakiye(kayitliKullanici.LOGICALREF);
                    Session["BILDIRIM_SAYISI"] = CRUD.GetirBildirimSayisi(kayitliKullanici.LOGICALREF);
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
                    Thread t = new Thread(() => BaglantiSinifi.UyandirSQLServer());
                    t.Start();

                    return RedirectToAction("HomeIndex", "Home");
                }
                else
                {
                    //CRUD.KullaniciGirisiBasarisiz(kullanici.EMAIL);
                    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Kullanıcı Adı veya Şifre Hatalı." };
                }
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Giriş İşleminde Hata Oluştu." };


            }
            return View();
        }

        public void UyandirWorker()
        {

        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult SifreDegistir()
        {

            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SifreDegistir(string eskiSifre, string yeniSifre)
        {
            try
            {
                if (!string.IsNullOrEmpty(yeniSifre) && !string.IsNullOrEmpty(eskiSifre))
                {
                    string kayitliSifre = CRUD.GetirKullaniciSifre(Session["EMAIL"].ToString());
                    if (kayitliSifre.Equals(eskiSifre))
                    {
                        CRUD.GuncelleKullaniciSifreEmailIle(yeniSifre, Session["EMAIL"].ToString());
                        TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "login", mesajAciklama = "Şifre Değiştirme İşlemi Başarılı." };

                    }
                    else
                    {
                        TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "login", mesajAciklama = "Eski Şifreniz Geçerli Değil." };

                    }

                }
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "login", mesajAciklama = "Şifre Değiştirme İşlemi Sırasında Hata Oluştu" };

            }


            return View();
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SifremiUnuttum(string email)
        {

            try
            {

                int count = CRUD.KullaniciVarMi(email);

                //Kullanıcıya varsa şifresi alınır ve mail atılır.
                if (count == 1)
                {
                    KULLANICI kullanici = CRUD.GetirKullaniciEmailIle(email);
                    string adSoyad = kullanici.AD + " " + kullanici.SOYAD;
                    string sifre = CRUD.GetirKullaniciSifre(email);
                    string themessage = @"Sayın " + adSoyad + @" <br>Bu e-posta hesabınıza ait kullanıcı adı ve şifreniz aşağıdaki gibidir:<br> <br>Kullanıcı Adı :" + email + "<br>Şifre\t\t: " + sifre + @"
                      <html>
                      <body>
                        <table width=""100%"">
                        <tr>
                            <td style=""font-style:arial; color:maroon; font-weight:bold"">
                    <img  src=""http://netdenet.azurewebsites.net/Content/adminlte/img/dkn96x96.png""/>
                         </td>
                        </tr>                           
                        </body>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              </body>
                        </html>";
                    //<img src=cid:myImageID>
                    //string path = Server.MapPath("~/Content/adminlte/img");

                    MailMessage ePosta = new MailMessage();
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(themessage, null, "text/html");
                    //LinkedResource theEmailImage = new LinkedResource(path + "/dkn96x96.png");
                    //theEmailImage.ContentId = "myImageID";
                    //htmlView.LinkedResources.Add(theEmailImage);
                    ePosta.AlternateViews.Add(htmlView);

                    ePosta.From = new MailAddress("zimmettakipsistemi@gmail.com", "NETDENET");
                    ePosta.To.Add(email);
                    ePosta.Subject = "NETDENET şifre hatırlatma";
                    ePosta.IsBodyHtml = true;


                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential("zimmettakipsistemi@gmail.com", "ehirbhiqubiauypl");
                    smtp.EnableSsl = true;
                    smtp.Send(ePosta);
                    TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "login", mesajAciklama = "Şifreniz e-mail adresinize gönderilmiştir.." };

                }
                else
                {
                    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "login", mesajAciklama = "Tanımlı Kullanıcı Bulunamadı." };

                }

            }
            catch (SmtpException e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "login", mesajAciklama = "İşlem Sırasında Hata Oluştu" };

            }

            return RedirectToAction("Index");
        }

        public ActionResult HesapOlustur()
        {
            KULLANICI kullanici = new KULLANICI();
            kullanici.turList = new List<SelectListItem>();
            kullanici.turList.Add(new SelectListItem { Text = "SMMM", Value = "1" });
            kullanici.turList.Add(new SelectListItem { Text = "Vergi Müfettişi", Value = "2" });
            kullanici.turList.Add(new SelectListItem { Text = "YMM", Value = "3" });
            kullanici.turList.Add(new SelectListItem { Text = "Bireysel ve Diğer", Value = "4" });
            return View(kullanici);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult HesapOlustur(KULLANICI kullanici)
        {

            try
            {
                if (kullanici.EMAIL != null && kullanici.AD != null && kullanici.SOYAD != null && kullanici.TELEFON != null)
                {
                    int kullaniciVarmi = CRUD.KullaniciVarMi(kullanici.EMAIL);
                    if (kullaniciVarmi == 0)
                    {
                        Guid obj = Guid.NewGuid();
                        kullanici.LOGICALREF = obj.ToString();
                        CRUD.KullaniciEkleYeniUye(kullanici);
                        SendEmailActivationLink(kullanici.EMAIL, kullanici.LOGICALREF);
                        TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "E-mail adresinize gönderilen linki tıklayarak hesabınızı aktif edebilirsiniz." };

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "login", mesajAciklama = "Bu kullanıcı sistemde kayıtlı. " };
                        return HesapOlustur();
                    }
                }
                else
                {
                    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "login", mesajAciklama = "Boş alan bırakmayınız." };
                    return HesapOlustur();
                }
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "login", mesajAciklama = "Kayıt İşlemi Sırasında Hata Oluştu" };

            }
            return View();
        }
        [Authorize]
        public ActionResult Hesabim()
        {
            KULLANICI kullanici = new KULLANICI();

            try
            {
                if (Session["EMAIL"] != null)
                {
                    kullanici = CRUD.GetirKullaniciEmailIle(Session["EMAIL"].ToString());
                    Session["RESIM"] = kullanici.resimSrc;//profil resmi güncellendikten sonra güncel resmi alıyorum
                    kullanici.turList = new List<SelectListItem>();
                    kullanici.turList.Add(new SelectListItem { Text = "SMMM", Value = "1" });
                    kullanici.turList.Add(new SelectListItem { Text = "Vergi Müfettişi", Value = "2" });
                    kullanici.turList.Add(new SelectListItem { Text = "YMM", Value = "3" });
                    kullanici.turList.Add(new SelectListItem { Text = "Bireysel ve Diğer", Value = "4" });
                }
            }
            catch (Exception)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "login", mesajAciklama = "Hesap Bilgileri Getirilirken Hata Oluştu" };

            }
            return View(kullanici);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Hesabim(HttpPostedFileBase[] uploadImages, KULLANICI kullanici)
        {
            int resimSize = 0;
            try
            {
                KULLANICI kullaniciTemp = CRUD.GetirKullaniciEmailIle(Session["EMAIL"].ToString());
                kullanici.KULLANIM_DURUMU = 1;
                kullanici.LOGICALREF = kullaniciTemp.LOGICALREF;
                if (uploadImages[0] != null)
                    foreach (var image in uploadImages)
                    {
                        if (image.ContentLength > 0)
                        {
                            resimSize = image.ContentLength;
                            byte[] imageData = null;
                            //using (var binaryReader = new BinaryReader(image.InputStream))
                            //{
                            //    imageData = binaryReader.ReadBytes(image.ContentLength);
                            //    kullanici.RESIM = imageData;
                            //}

                            WebImage img = new WebImage(image.InputStream);
                            if (img.Width > 200)
                                img.Resize(200, 200);
                            imageData = img.GetBytes();
                            kullanici.RESIM = imageData;
                            CRUD.GuncelleKullaniciHesabim(kullanici);
                        }
                    }
                else
                    kullanici.RESIM = kullaniciTemp.RESIM;
                CRUD.GuncelleKullaniciHesabim(kullanici);
                TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Güncelleme İşlemi Başarılı." };
                //if (resimSize <= 150 * 1024)//resim boyutu 50 KB sınırı koyuldu
                //{
                //    CRUD.GuncelleKullaniciHesabim(kullanici);
                //    TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Güncelleme İşlemi Başarılı." };
                //}
                //else
                //    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Resim boyutu 150 KB(KiloByte)'tan küçük olmalıdır." };


            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Güncelleme İşlemi Sırasında Hata Oluştu" };

            }
            return RedirectToAction("Hesabim");
        }



        public ActionResult SendEmailActivationLink(string email, string logicalref)
        {

            try
            {
                KULLANICI kullanici = CRUD.GetirPasifKullaniciEmailIle(email);
                string adSoyad = kullanici.AD + " " + kullanici.SOYAD;
                string aciklama = "";
                if (kullanici.HESAP_TURU == 2)
                    aciklama = "Mükelleflerinizin";
                else if (kullanici.HESAP_TURU == 1 || kullanici.HESAP_TURU == 3)
                    aciklama = "Müşterilerinizin";
                else if (kullanici.HESAP_TURU == 4)
                    aciklama = "İşletmenizin";
                string themessage = @"Sayın " + adSoyad + @"<br><br>NETDENET’e hoş geldiniz.<b> " + aciklama + @"</b> e-denetimini güven içinde yapabilirsiniz.
                                   <br>NETDENET’in size ne tür kolaylıklar sunabileceğini ücretsiz tecrübe edebilmeniz için 250 TL değerindeki 5 kontör hesabınıza yüklenmiştir. <br> 
                                    Aktivasyonu tamamlamak ve denetime devam etmek için lütfen aşağıda belirtilen linke tıklayınız. <br><br>" + @"
                                    https://netdenet.azurewebsites.net/GirisYap/EmailActivation?id=" + logicalref +
                        @"<html>
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
                        Bu e-posta üyelik işlemlerinin tamamlanması için NETDENET tarafından otomatik olarak gönderilmiştir. Şayet böyle bir girişiminiz olmadıysa, bu e-postanın yanlışlıkla gönderildiğini düşünüyorsanız, bu e-postayı yok sayabilir ya da silebilirsiniz.
                        </body>
                        </html>";

                //string path = Server.MapPath("~/Content/adminlte/img");

                MailMessage ePosta = new MailMessage();
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(themessage, null, "text/html");
                //LinkedResource theEmailImage = new LinkedResource(path + "/dkn96x96.png");
                //theEmailImage.ContentId = "myImageID";
                //htmlView.LinkedResources.Add(theEmailImage);
                ePosta.AlternateViews.Add(htmlView);

                ePosta.From = new MailAddress("zimmettakipsistemi@gmail.com", "NETDENET");
                ePosta.To.Add(email);
                ePosta.Subject = "NETDENET  Aktivasyonu";
                ePosta.IsBodyHtml = true;


                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("zimmettakipsistemi@gmail.com", "ehirbhiqubiauypl");
                smtp.EnableSsl = true;
                smtp.Send(ePosta);


            }
            catch (SmtpException e)
            {

            }
            return null;
        }


        public ActionResult EmailActivation(string id)
        {
            KULLANICI kullanici = new KULLANICI();

            try
            {
                kullanici = CRUD.GetirKullaniciIdIle(id);

                if (!string.IsNullOrEmpty(kullanici.EMAIL) && kullanici.KULLANIM_DURUMU == 0)//kullanım durumu 0 OLDUĞUNA BAKIYORUM. DAHA SONRE AYNI LİNKE TIKLARSA LİNK GEÇERSİZ DİYEBİLMEK İÇİN
                {
                    kullanici.KULLANIM_DURUMU = 1;
                    CRUD.GuncelleKullanici(kullanici);
                    // return RedirectToAction("Index", new RouteValueDictionary(
                    //new { controller = "GirisYap", action = "Index", kullanici = kullanici }));
                    return Index(kullanici);
                }
            }
            catch (Exception)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "login", mesajAciklama = "İşlem Sırasında Hata Oluştu" };

            }
            return RedirectToAction("Index");
        }

    }


}