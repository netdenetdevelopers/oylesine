using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Windows;
using System.Data.SqlClient;
using DKN.Models;
using DKN.db;
using DKN.parametreler;
using System.Web.Mvc;
using System.Threading;

namespace DKN.db
{
    class CRUD
    {
        public static KULLANICI GetirKullanici(KULLANICI kullanici)
        {
            KULLANICI kayitliKullanici = new KULLANICI();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " SELECT * FROM KULLANICI " +
                                " WHERE EMAIL=@EMAIL " +
                                "   AND SIFRE=@SIFRE " +
                                "   AND KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@EMAIL", kullanici.EMAIL);
                sqlCmd2.Parameters.AddWithValue("@SIFRE", kullanici.SIFRE);
                //sqlCmd2.Parameters.AddWithValue("@SIFRE", Dogrulama.GenerateSaltedHash(Encoding.UTF8.GetBytes(sifre), Encoding.UTF8.GetBytes("connector")));

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                sqlDataReader1.Read();
                kayitliKullanici.EMAIL = sqlDataReader1["EMAIL"].ToString();
                kayitliKullanici.SIFRE = sqlDataReader1["SIFRE"].ToString();
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }

            return kayitliKullanici;
        }
        
        public static int KullaniciVarMi(string kullaniciAdi)
        {
            //Veri Tabanına Bağlantı Açıyoruz...
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            int count = 0;

            try
            {
                //Bu kısımda sadece kullanıcının varlığının testi yapılmaktadır.
                string query1 = "SELECT COUNT(1) FROM KULLANICI " +
                                " WHERE EMAIL=@EMAIL " +
                                "   AND KULLANIM_DURUMU=1";

                SqlCommand sqlCmd1 = new SqlCommand(query1, sqlCon);
                sqlCmd1.CommandType = CommandType.Text;
                sqlCmd1.Parameters.AddWithValue("@EMAIL", kullaniciAdi);
                count = Convert.ToInt32(sqlCmd1.ExecuteScalar());

            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return count;
        }
        
        public static string GetirKullaniciSifre(string kullaniciAdi)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            string query = "";
            string sifre = "";

            try
            {
                query = " SELECT SIFRE " +
                        " FROM KULLANICI " +
                        " WHERE EMAIL=@EMAIL " +
                        "   AND KULLANIM_DURUMU=1";

                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.Parameters.AddWithValue("@EMAIL", kullaniciAdi);
                SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();
                sqlDataReader.Read();
                sifre = sqlDataReader["SIFRE"].ToString();

                sqlDataReader.Close();
                return Dogrulama.Decrypt(sifre, parametreler.Dogrulama.saltValue);
            }
            catch (Exception exception)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
        }
        
        public static void KullaniciGirisiBasarisiz(string kullaniciAdi)
        {
            //Veri Tabanına Bağlantı Açıyoruz...
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                //Eğer böyle bir e-mail varsa başarısız giriş sayısını bir arttırıyoruz...
                string query6 = " SELECT COUNT(1) FROM KULLANICI WHERE EMAIL=@EMAIL AND KULLANIM_DURUMU=1 ";
                SqlCommand sqlCmd6 = new SqlCommand(query6, sqlCon);
                sqlCmd6.CommandType = CommandType.Text;
                sqlCmd6.Parameters.AddWithValue("@EMAIL", kullaniciAdi);

                int sayi = Convert.ToInt32(sqlCmd6.ExecuteScalar());
                //Demek ki böyle bir kullanıcı varmış ama şifreyi yanlış girmiş...
                //Dolayısıyla başarısız giriş sayısını bir arttıracağız...
                if (sayi == 1)
                {
                    string query7 = " SELECT " +
                                    " BASARISIZ_DENEME_SAYISI, LOGICALREF " +
                                    " FROM KULLANICI " +
                                    " WHERE EMAIL=@EMAIL " +
                                    "   AND KULLANIM_DURUMU=1";
                    SqlCommand sqlCmd7 = new SqlCommand(query7, sqlCon);
                    sqlCmd7.CommandType = CommandType.Text;
                    sqlCmd7.Parameters.AddWithValue("@EMAIL", kullaniciAdi);
                    SqlDataReader sqlDataReader4 = sqlCmd7.ExecuteReader();
                    sqlDataReader4.Read();

                    string basrisizDenemeSayisi = sqlDataReader4["BASARISIZ_DENEME_SAYISI"].ToString();
                    basrisizDenemeSayisi = Convert.ToString(Convert.ToInt32(basrisizDenemeSayisi) + 1);
                    string logicalRef = sqlDataReader4["LOGICALREF"].ToString();
                    sqlDataReader4.Close();

                    string query8 = " UPDATE KULLANICI " +
                                    " SET BASARISIZ_DENEME_SAYISI=@BASARISIZDENEMESAYISI " +
                                    " WHERE LOGICALREF=@LOGICALREF " +
                                    "   AND KULLANIM_DURUMU=1 ";
                    SqlCommand sqlCmd8 = new SqlCommand(query8, sqlCon);
                    sqlCmd8.CommandType = CommandType.Text;
                    sqlCmd8.Parameters.AddWithValue("@BASARISIZDENEMESAYISI", basrisizDenemeSayisi);
                    sqlCmd8.Parameters.AddWithValue("@LOGICALREF", logicalRef);
                    sqlCmd8.ExecuteScalar();
                }

            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;

            }
            finally
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
        }
        
        public static KULLANICI KullaniciBilgileriniDoldur(string kullaniciAdi, string sifre)
        {
            //Veri Tabanına Bağlantı Açıyoruz...
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            KULLANICI kullanici = new KULLANICI();
            
            try
            {
                //Session boyunca bilgilerini tutacağımız kullanıcıyı create ediyoruz.
                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " SELECT * FROM KULLANICI WHERE EMAIL=@EMAIL AND KULLANIM_DURUMU=1 ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@EMAIL", kullaniciAdi);
                //sqlCmd2.Parameters.AddWithValue("@SIFRE", Dogrulama.GenerateSaltedHash(Encoding.UTF8.GetBytes(sifre), Encoding.UTF8.GetBytes("connector")));

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                sqlDataReader1.Read();

                kullanici.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kullanici.AD = sqlDataReader1["AD"].ToString();
                kullanici.SOYAD = sqlDataReader1["SOYAD"].ToString();
                kullanici.EMAIL = sqlDataReader1["EMAIL"].ToString();
                kullanici.ADMIN_MI = Convert.ToInt32(sqlDataReader1["ADMIN_MI"].ToString());
                kullanici.TELEFON = sqlDataReader1["TELEFON"].ToString();
                
                if (sqlDataReader1["RESIM"].GetType() != typeof(DBNull))
                {
                    kullanici.RESIM = (byte[])sqlDataReader1["RESIM"];
                }
                else
                    kullanici.RESIM = null;

                if (kullanici.RESIM != null)
                {
                    string urunresimString = Convert.ToBase64String(kullanici.RESIM, 0, kullanici.RESIM.Length);
                    kullanici.resimSrc = "data:image/jpeg;base64," + urunresimString;
                }
                else
                {
                    kullanici.resimSrc = "";
                }
                sqlDataReader1.Close();
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kullanici;
        }
        
        public static string GetirKullaniciBakiye(string kullaniciLogicalref)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            string toplamKontor = "0";

            try
            {
                string query = @" select ISNULL(SUM(KONTOR_MIKTARI),0) AS TOPLAM_KONTOR from KONTOR_HAREKET 
                                  WHERE KULLANICI_LOGICALREF=@kullaniciLogicalref
                                  GROUP BY KULLANICI_LOGICALREF ";

                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.Parameters.AddWithValue("@kullaniciLogicalref", kullaniciLogicalref);
                SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    toplamKontor = sqlDataReader["TOPLAM_KONTOR"].ToString();
                }
                sqlDataReader.Close();
                return toplamKontor;
            }
            catch (Exception exception)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static string GetirBildirimSayisi(string kullaniciLogicalref)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            string toplamBildirm = "0";

            try
            {
                string query = @"SELECT COUNT(*) AS TOPLAM FROM BILDIRIM
                                WHERE KULLANICI_LOGICALREF=@kullaniciLogicalref
                                AND DURUMU=1
                                AND KULLANIM_DURUMU=1 ";

                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.Parameters.AddWithValue("@kullaniciLogicalref", kullaniciLogicalref);
                SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    toplamBildirm = sqlDataReader["TOPLAM"].ToString();
                }
                sqlDataReader.Close();
                return toplamBildirm;
            }
            catch (Exception exception)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }


        //public static int GetirDenetimeHarcananKontorDenetimIdIle(string denetimLogicalref)
        //{
        //    SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
        //    int toplamKontor = 0;

        //    try
        //    {
        //        string query = @"SELECT SUM(K.KONTOR) AS TOPLAM_KONTOR FROM DENETIM_KAPSAM DK
        //                        INNER JOIN KAPSAM K ON K.LOGICALREF=DK.KAPSAM_LOGICALREF
        //                        WHERE DK.DENETIM_LOGICALREF=@denetimLogicalref
        //                        GROUP BY DK.DENETIM_LOGICALREF";

        //        SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
        //        sqlCmd.CommandType = CommandType.Text;
        //        sqlCmd.Parameters.AddWithValue("@denetimLogicalref", denetimLogicalref);
        //        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();
        //        while (sqlDataReader.Read())
        //        {
        //            toplamKontor = Convert.ToInt32(sqlDataReader["TOPLAM_KONTOR"].ToString());
        //        }
        //        sqlDataReader.Close();
        //        return toplamKontor;
        //    }
        //    catch (Exception exception)
        //    {

        //        BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
        //        throw;
        //    }
        //    finally
        //    {

        //        BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

        //    }
        //}

        public static int GetirDenetimeHarcananKontorDenetimIdIle(string denetimLogicalref)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            int toplamKontor = 0;

            try
            {
                string query = @" SELECT SUM(AK.KONTOR) AS TOPLAM_KONTOR FROM DENETIM_ALT_KAPSAM DAK
                                  INNER JOIN ALT_KAPSAM AK ON AK.LOGICALREF=DAK.ALT_KAPSAM_LOGICALREF
                                  WHERE DAK.DENETIM_LOGICALREF=@denetimLogicalref
                                  GROUP BY DAK.DENETIM_LOGICALREF";

                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.Parameters.AddWithValue("@denetimLogicalref", denetimLogicalref);
                SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    toplamKontor = Convert.ToInt32(sqlDataReader["TOPLAM_KONTOR"].ToString());
                }
                sqlDataReader.Close();
                return toplamKontor;
            }
            catch (Exception exception)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
        }
        
        public static KULLANICI KullaniciBilgileriniDoldurEmailIle(string logicalref)
        {
            //Veri Tabanına Bağlantı Açıyoruz...
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            KULLANICI kullanici = new KULLANICI();
            try
            {
                //Session boyunca bilgilerini tutacağımız kullanıcıyı create ediyoruz.
                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " SELECT * FROM KULLANICI WHERE LOGICALREF=@logicalref AND KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@LOGICALREF", logicalref);
                //sqlCmd2.Parameters.AddWithValue("@SIFRE", Dogrulama.GenerateSaltedHash(Encoding.UTF8.GetBytes(sifre), Encoding.UTF8.GetBytes("connector")));

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                sqlDataReader1.Read();

                kullanici.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kullanici.AD = sqlDataReader1["AD"].ToString();
                kullanici.SOYAD = sqlDataReader1["SOYAD"].ToString();
                kullanici.EMAIL = sqlDataReader1["EMAIL"].ToString();
                kullanici.ADMIN_MI = Convert.ToInt32(sqlDataReader1["ADMIN_MI"].ToString());
                kullanici.TELEFON = sqlDataReader1["TELEFON"].ToString();
                if (sqlDataReader1["RESIM"].GetType() != typeof(DBNull))
                {
                    kullanici.RESIM = (byte[])sqlDataReader1["RESIM"];
                }
                else
                    kullanici.RESIM = null;

                if (kullanici.RESIM != null)
                {
                    string urunresimString = Convert.ToBase64String(kullanici.RESIM, 0, kullanici.RESIM.Length);
                    kullanici.resimSrc = "data:image/jpeg;base64," + urunresimString;
                }
                else
                {
                    kullanici.resimSrc = "";
                }

                sqlDataReader1.Close();


            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kullanici;
        }

        public static KULLANICI GetirKullaniciEmailIle(string kullaniciAdi)
        {
            //Veri Tabanına Bağlantı Açıyoruz...
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            KULLANICI kullanici = new KULLANICI();
            try
            {
                //Session boyunca bilgilerini tutacağımız kullanıcıyı create ediyoruz.
                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " SELECT * FROM KULLANICI WHERE EMAIL=@EMAIL AND KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@EMAIL", kullaniciAdi);
                //sqlCmd2.Parameters.AddWithValue("@SIFRE", Dogrulama.GenerateSaltedHash(Encoding.UTF8.GetBytes(sifre), Encoding.UTF8.GetBytes("connector")));

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                sqlDataReader1.Read();

                kullanici.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kullanici.AD = sqlDataReader1["AD"].ToString();
                kullanici.SOYAD = sqlDataReader1["SOYAD"].ToString();
                kullanici.EMAIL = sqlDataReader1["EMAIL"].ToString();
                kullanici.TELEFON = sqlDataReader1["TELEFON"].ToString();
                kullanici.HESAP_TURU = Convert.ToInt32(sqlDataReader1["HESAP_TURU"].ToString());
                if (sqlDataReader1["RESIM"].GetType() != typeof(DBNull))
                {
                    kullanici.RESIM = (byte[])sqlDataReader1["RESIM"];
                }
                else
                    kullanici.RESIM = null;

                if (kullanici.RESIM != null)
                {
                    string urunresimString = Convert.ToBase64String(kullanici.RESIM, 0, kullanici.RESIM.Length);
                    kullanici.resimSrc = "data:image/jpeg;base64," + urunresimString;
                }
                else
                {
                    kullanici.resimSrc = "";
                }
                sqlDataReader1.Close();


            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kullanici;
        }
        
        public static KULLANICI GetirPasifKullaniciEmailIle(string kullaniciAdi)
        {
            //Veri Tabanına Bağlantı Açıyoruz...
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            KULLANICI kullanici = new KULLANICI();
            try
            {
                //Session boyunca bilgilerini tutacağımız kullanıcıyı create ediyoruz.
                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " SELECT * FROM KULLANICI WHERE EMAIL=@EMAIL AND KULLANIM_DURUMU=0 ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@EMAIL", kullaniciAdi);
                //sqlCmd2.Parameters.AddWithValue("@SIFRE", Dogrulama.GenerateSaltedHash(Encoding.UTF8.GetBytes(sifre), Encoding.UTF8.GetBytes("connector")));

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                sqlDataReader1.Read();

                kullanici.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kullanici.AD = sqlDataReader1["AD"].ToString();
                kullanici.SOYAD = sqlDataReader1["SOYAD"].ToString();
                kullanici.EMAIL = sqlDataReader1["EMAIL"].ToString();
                kullanici.TELEFON = sqlDataReader1["TELEFON"].ToString();
                kullanici.HESAP_TURU = Convert.ToInt32(sqlDataReader1["HESAP_TURU"].ToString());
                if (sqlDataReader1["RESIM"].GetType() != typeof(DBNull))
                {
                    kullanici.RESIM = (byte[])sqlDataReader1["RESIM"];
                }
                else
                    kullanici.RESIM = null;

                if (kullanici.RESIM != null)
                {
                    string urunresimString = Convert.ToBase64String(kullanici.RESIM, 0, kullanici.RESIM.Length);
                    kullanici.resimSrc = "data:image/jpeg;base64," + urunresimString;
                }
                else
                {
                    kullanici.resimSrc = "";
                }
                sqlDataReader1.Close();
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kullanici;
        }
        
        public static List<KULLANICI> GetirKullaniciListesi()
        {
            KULLANICI kayitliKullanici = new KULLANICI();
            List<KULLANICI> kayitliKullaniciList = new List<KULLANICI>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " SELECT * FROM KULLANICI ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                //sqlCmd2.Parameters.AddWithValue("@SIFRE", Dogrulama.GenerateSaltedHash(Encoding.UTF8.GetBytes(sifre), Encoding.UTF8.GetBytes("connector")));

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                while (sqlDataReader1.Read())
                {
                    kayitliKullanici = new KULLANICI();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliKullanici.aktifPasif = "AKTİF";
                        kayitliKullanici.renk = "red";
                        kayitliKullanici.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliKullanici.aktifPasif = "PASİF";
                        kayitliKullanici.renk = "blue";
                        kayitliKullanici.kullanimDurumuBool = false;
                    }
                    if (Convert.ToInt32(sqlDataReader1["ADMIN_MI"]) == 1)
                    {
                        kayitliKullanici.adminMiBool = true;
                    }
                    else
                    {
                        kayitliKullanici.adminMiBool = false;
                    }
                    kayitliKullanici.EMAIL = sqlDataReader1["EMAIL"].ToString();
                    kayitliKullanici.SIFRE = sqlDataReader1["SIFRE"].ToString();
                    kayitliKullanici.AD = sqlDataReader1["AD"].ToString();
                    kayitliKullanici.SOYAD = sqlDataReader1["SOYAD"].ToString();
                    kayitliKullanici.TELEFON = sqlDataReader1["TELEFON"].ToString();
                    kayitliKullanici.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKullanici.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliKullanici.AdSoyad = sqlDataReader1["AD"].ToString() + " " + sqlDataReader1["SOYAD"].ToString();
                    if (sqlDataReader1["RESIM"].GetType() != typeof(DBNull))
                    {
                        kayitliKullanici.RESIM = (byte[])sqlDataReader1["RESIM"];
                    }
                    else
                        kayitliKullanici.RESIM = null;

                    if (kayitliKullanici.RESIM != null)
                    {
                        string urunresimString = Convert.ToBase64String(kayitliKullanici.RESIM, 0, kayitliKullanici.RESIM.Length);
                        kayitliKullanici.resimSrc = "data:image/jpeg;base64," + urunresimString;
                    }
                    else
                    {
                        kayitliKullanici.resimSrc = "";
                    }
                    kayitliKullaniciList.Add(kayitliKullanici);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKullaniciList;
        }

        public static void KullaniciEkle(KULLANICI kullanici)
        {
            kullanici.ADMIN_MI = 0;
            String query = "INSERT INTO KULLANICI " +
                " (AD," +
                " LOGICALREF," +
                " SOYAD," +
                " EMAIL," +
                " TELEFON," +
                " SIFRE," +
                " KULLANIM_DURUMU)" +
                " VALUES" +
                " (@ad," +
                " @logicalRef," +
                " @soyad," +
                " @email," +
                " @telefon," +
                " @sifre," +
                " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    String hashSifre = Dogrulama.Encrypt(kullanici.SIFRE, parametreler.Dogrulama.saltValue);

                    command.Parameters.AddWithValue("@ad", kullanici.AD);
                    command.Parameters.AddWithValue("@logicalRef", kullanici.LOGICALREF);
                    command.Parameters.AddWithValue("@soyad", kullanici.SOYAD);
                    command.Parameters.AddWithValue("@email", kullanici.EMAIL);
                    command.Parameters.AddWithValue("@telefon", kullanici.TELEFON);
                    command.Parameters.AddWithValue("@sifre", hashSifre);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void KullaniciEkleYeniUye(KULLANICI kullanici)
        {
            kullanici.ADMIN_MI = 0;
            String query = " INSERT INTO KULLANICI " +
                " (AD," +
                " LOGICALREF," +
                " SOYAD," +
                " EMAIL," +
                " TELEFON," +
                " HESAP_TURU," +
                " SIFRE," +
                " KULLANIM_DURUMU)" +
                " VALUES" +
                " (@ad," +
                " @logicalRef," +
                " @soyad," +
                " @email," +
                " @telefon," +
                " @hesapTuru," +
                " @sifre," +
                " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    String hashSifre = Dogrulama.Encrypt(kullanici.SIFRE, parametreler.Dogrulama.saltValue);

                    command.Parameters.AddWithValue("@ad", kullanici.AD);
                    command.Parameters.AddWithValue("@logicalRef", kullanici.LOGICALREF);
                    command.Parameters.AddWithValue("@soyad", kullanici.SOYAD);
                    command.Parameters.AddWithValue("@email", kullanici.EMAIL);
                    command.Parameters.AddWithValue("@telefon", kullanici.TELEFON);
                    command.Parameters.AddWithValue("@hesapTuru", kullanici.HESAP_TURU);
                    command.Parameters.AddWithValue("@sifre", hashSifre);
                    command.Parameters.AddWithValue("@kullanimDurumu", 0);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static KULLANICI GetirKullaniciIdIle(string logicalRef)
        {
            KULLANICI kayitliKullanici = new KULLANICI();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM KULLANICI WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kayitliKullanici.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kayitliKullanici.AD = sqlDataReader1["AD"].ToString();
                kayitliKullanici.SOYAD = sqlDataReader1["SOYAD"].ToString();
                kayitliKullanici.EMAIL = sqlDataReader1["EMAIL"].ToString();
                kayitliKullanici.TELEFON = sqlDataReader1["TELEFON"].ToString();
                kayitliKullanici.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                kayitliKullanici.ADMIN_MI = Convert.ToInt32(sqlDataReader1["ADMIN_MI"]);
                kayitliKullanici.SIFRE = Dogrulama.Decrypt(sqlDataReader1["SIFRE"].ToString(), parametreler.Dogrulama.saltValue);
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKullanici;
        }

        public static void GuncelleKullanici(KULLANICI kullanici)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query = " UPDATE KULLANICI" +
                                  " SET " +
                                  " AD=@ad," +
                                  " SOYAD=@soyad," +
                                  " ADMIN_MI=@adminMi," +
                                  " TELEFON=@telefon," +
                                  " KULLANIM_DURUMU=@kullanimDurumu" +
                                  " WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@ad", kullanici.AD);
                sqlCmd2.Parameters.AddWithValue("@soyad", kullanici.SOYAD);
                sqlCmd2.Parameters.AddWithValue("@telefon", kullanici.TELEFON);
                sqlCmd2.Parameters.AddWithValue("@kullanimDurumu", kullanici.KULLANIM_DURUMU);
                sqlCmd2.Parameters.AddWithValue("@adminMi", kullanici.ADMIN_MI);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", kullanici.LOGICALREF);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void GuncelleKullaniciHesabim(KULLANICI kullanici)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query = " UPDATE KULLANICI SET AD=@ad, SOYAD=@soyad, HESAP_TURU=@hesapTuru, TELEFON=@telefon, ";

                if (kullanici.RESIM != null)                  //ilk hesap ya da kullanıcı tanımlamalarda resim değeri null olduğu için. güncellemede sıkıntı çıkmasın diye
                    query = query + " RESIM=@resim,";
                query = query + " KULLANIM_DURUMU=@kullanimDurumu WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@ad", kullanici.AD);
                sqlCmd2.Parameters.AddWithValue("@soyad", kullanici.SOYAD);
                sqlCmd2.Parameters.AddWithValue("@telefon", kullanici.TELEFON);
                sqlCmd2.Parameters.AddWithValue("@hesapTuru", kullanici.HESAP_TURU);
                if (kullanici.RESIM != null)
                    sqlCmd2.Parameters.AddWithValue("@resim", kullanici.RESIM);
                sqlCmd2.Parameters.AddWithValue("@kullanimDurumu", kullanici.KULLANIM_DURUMU);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", kullanici.LOGICALREF);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static List<KONTOR> GetirKontorListesi()
        {
            KONTOR kayitliKontor = new KONTOR();
            List<KONTOR> kayitliKontorList = new List<KONTOR>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " SELECT * FROM KONTOR ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                //sqlCmd2.Parameters.AddWithValue("@SIFRE", Dogrulama.GenerateSaltedHash(Encoding.UTF8.GetBytes(sifre), Encoding.UTF8.GetBytes("connector")));

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                while (sqlDataReader1.Read())
                {
                    kayitliKontor = new KONTOR();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliKontor.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliKontor.kullanimDurumuBool = false;
                    }
                    kayitliKontor.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKontor.KONTOR_ADET = Convert.ToInt32(sqlDataReader1["KONTOR_ADET"].ToString());
                    kayitliKontor.BIRIM_FIYATI = sqlDataReader1["BIRIM_FIYATI"].ToString();
                    kayitliKontor.PAKET_FIYATI = sqlDataReader1["PAKET_FIYATI"].ToString();
                    kayitliKontor.INDIRIM_ORANI = Convert.ToInt32(sqlDataReader1["INDIRIM_ORANI"].ToString());
                    kayitliKontor.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                    kayitliKontor.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliKontorList.Add(kayitliKontor);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKontorList;
        }

        public static List<KONTOR> GetirAktifKontorListesi()
        {
            KONTOR kayitliKontor = new KONTOR();
            List<KONTOR> kayitliKontorList = new List<KONTOR>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " SELECT * FROM KONTOR WHERE KULLANIM_DURUMU=1 ORDER BY BIRIM_FIYATI DESC ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                //sqlCmd2.Parameters.AddWithValue("@SIFRE", Dogrulama.GenerateSaltedHash(Encoding.UTF8.GetBytes(sifre), Encoding.UTF8.GetBytes("connector")));

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                while (sqlDataReader1.Read())
                {
                    kayitliKontor = new KONTOR();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliKontor.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliKontor.kullanimDurumuBool = false;
                    }
                    kayitliKontor.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKontor.KONTOR_ADET = Convert.ToInt32(sqlDataReader1["KONTOR_ADET"].ToString());
                    kayitliKontor.BIRIM_FIYATI = sqlDataReader1["BIRIM_FIYATI"].ToString();
                    kayitliKontor.PAKET_FIYATI = sqlDataReader1["PAKET_FIYATI"].ToString();
                    kayitliKontor.INDIRIM_ORANI = Convert.ToInt32(sqlDataReader1["INDIRIM_ORANI"].ToString());
                    kayitliKontor.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                    kayitliKontor.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliKontorList.Add(kayitliKontor);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKontorList;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public static List<FIRMA> GetirFirmaListesi()
        {
            List<FIRMA> kayitliFirmaList = new List<FIRMA>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM FIRMA ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    FIRMA kayitliFirma = new FIRMA();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliFirma.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliFirma.kullanimDurumuBool = false;
                    }
                    kayitliFirma.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliFirma.VKNTCKN = sqlDataReader1["VKNTCKN"].ToString();
                    kayitliFirma.UNVAN = sqlDataReader1["UNVAN"].ToString();
                    kayitliFirma.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliFirmaList.Add(kayitliFirma);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliFirmaList;
        }

        public static List<KAPSAM> GetirKapsamListesi()
        {
            List<KAPSAM> kayitliKapsamList = new List<KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = " SELECT * FROM KAPSAM WHERE KULLANIM_DURUMU=1 ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    KAPSAM kayitliKapsam = new KAPSAM();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliKapsam.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliKapsam.kullanimDurumuBool = false;
                    }

                    kayitliKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKapsam.AD = sqlDataReader1["AD"].ToString();
                    kayitliKapsam.KISA_AD = sqlDataReader1["KISA_AD"].ToString();
                    kayitliKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliKapsamList.Add(kayitliKapsam);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKapsamList;
        }

        public static List<ALT_KAPSAM> GetirAltKapsamListesi()
        {
            List<ALT_KAPSAM> kayitliKapsamList = new List<ALT_KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = @"SELECT AK.*, 
                                    COUNT(DKAK.ALT_KAPSAM_LOGICALREF) AS KURAL_SAYISI 
                                    FROM ALT_KAPSAM   AK 
                                    LEFT JOIN DENETIM_KURALLARI_ALT_KAPSAM DKAK ON AK.LOGICALREF=DKAK.ALT_KAPSAM_LOGICALREF 
                                    WHERE AK.KULLANIM_DURUMU=1 
                                    GROUP BY DKAK.ALT_KAPSAM_LOGICALREF, 
                                    AK.LOGICALREF,
                                    AK.AD,
                                    AK.KISA_AD,
                                    AK.KONTOR,
                                    AK.ACIKLAMA,
                                    AK.KULLANIM_DURUMU 
                                   ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    ALT_KAPSAM kayitliKapsam = new ALT_KAPSAM();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliKapsam.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliKapsam.kullanimDurumuBool = false;
                    }

                    kayitliKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKapsam.AD = sqlDataReader1["AD"].ToString();
                    kayitliKapsam.KISA_AD = sqlDataReader1["KISA_AD"].ToString();
                    kayitliKapsam.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                    kayitliKapsam.KONTOR = Convert.ToInt32(sqlDataReader1["KONTOR"]);
                    kayitliKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliKapsam.kuralSayisi = Convert.ToInt32(sqlDataReader1["KURAL_SAYISI"]);
                    kayitliKapsamList.Add(kayitliKapsam);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKapsamList;
        }

        public static List<ALT_KAPSAM> GetirAltKapsamListesiKapsamIdIle(string kapsamLogicalref)
        {
            List<ALT_KAPSAM> kayitliKapsamList = new List<ALT_KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = @"SELECT AK.*, 
                                    COUNT(DKAK.ALT_KAPSAM_LOGICALREF) AS KURAL_SAYISI 
                                    FROM ALT_KAPSAM   AK 
                                    LEFT JOIN DENETIM_KURALLARI_ALT_KAPSAM DKAK ON AK.LOGICALREF=DKAK.ALT_KAPSAM_LOGICALREF 
                                    LEFT JOIN DENETIM_KURALLARI_KAPSAM DKK ON DKK.DENETIM_KURALLARI_LOGICALREF=DKAK.DENETIM_KURALLARI_LOGICALREF 
                                    WHERE AK.KULLANIM_DURUMU=1 
                                    AND DKK.KAPSAM_LOGICALREF= @kapsamLogicalref
                                    GROUP BY DKAK.ALT_KAPSAM_LOGICALREF, 
                                    AK.LOGICALREF,
                                    AK.AD,
                                    AK.KISA_AD,
                                    AK.KONTOR,
                                    AK.ACIKLAMA,
                                    AK.KULLANIM_DURUMU  
                                   ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kapsamLogicalref", kapsamLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                int toplamKontor = 0;
                while (sqlDataReader1.Read())
                {
                    ALT_KAPSAM kayitliKapsam = new ALT_KAPSAM();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliKapsam.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliKapsam.kullanimDurumuBool = false;
                    }

                    kayitliKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKapsam.AD = sqlDataReader1["AD"].ToString();
                    kayitliKapsam.KISA_AD = sqlDataReader1["KISA_AD"].ToString();
                    kayitliKapsam.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                    kayitliKapsam.KONTOR = Convert.ToInt32(sqlDataReader1["KONTOR"]);
                    kayitliKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliKapsam.kuralSayisi = Convert.ToInt32(sqlDataReader1["KURAL_SAYISI"]);
                    toplamKontor += Convert.ToInt32(sqlDataReader1["KONTOR"]);
                    kayitliKapsam.toplamKontorSayisi = toplamKontor;
                    kayitliKapsam.IsCheck = true;
                    kayitliKapsamList.Add(kayitliKapsam);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kayitliKapsamList;
        }

        public static List<KAPSAM> GetirKapsamListesiDenetimIcin()
        {
            List<KAPSAM> kayitliKapsamList = new List<KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //string query2 = " SELECT * FROM KAPSAM WHERE KISA_AD NOT IN ('VR','VA')";
                string query2 = " SELECT * FROM KAPSAM WHERE KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    KAPSAM kayitliKapsam = new KAPSAM();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliKapsam.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliKapsam.kullanimDurumuBool = false;
                    }

                    kayitliKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKapsam.AD = sqlDataReader1["AD"].ToString();
                    kayitliKapsam.KISA_AD = sqlDataReader1["KISA_AD"].ToString();
                    kayitliKapsam.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                    kayitliKapsam.KONTOR = Convert.ToInt32(sqlDataReader1["KONTOR"].ToString());
                    kayitliKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliKapsamList.Add(kayitliKapsam);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKapsamList;
        }

        public static List<KAPSAM> GetirKapsamListesiKuralIcin()
        {
            List<KAPSAM> kayitliKapsamList = new List<KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = " SELECT * FROM KAPSAM WHERE KISA_AD != 'VD' ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    KAPSAM kayitliKapsam = new KAPSAM();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliKapsam.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliKapsam.kullanimDurumuBool = false;
                    }

                    kayitliKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKapsam.AD = sqlDataReader1["AD"].ToString();
                    kayitliKapsam.KISA_AD = sqlDataReader1["KISA_AD"].ToString();
                    kayitliKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliKapsamList.Add(kayitliKapsam);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKapsamList;
        }

        public static List<YIL> GetirYilListesi()
        {
            List<YIL> kayitliYilList = new List<YIL>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = " SELECT * FROM YIL WHERE KULLANIM_DURUMU = 1 ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    YIL kayitliYil = new YIL();

                    kayitliYil.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliYil.YIL_AD = sqlDataReader1["YIL_AD"].ToString();
                    kayitliYil.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliYilList.Add(kayitliYil);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliYilList;
        }

        public static List<FIRMA_DENETIM> GetirFirmaDenetimListesiFirmaIdIle(string firmaLogicalref)
        {
            List<FIRMA_DENETIM> kayitliFirmaDenetimList = new List<FIRMA_DENETIM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = " SELECT * FROM FIRMA_DENETIM WHERE FIRMA_LOGICALREF = @firmaLogicalref AND KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@firmaLogicalref", firmaLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    FIRMA_DENETIM kayitliFirmaDenetim = new FIRMA_DENETIM();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliFirmaDenetim.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliFirmaDenetim.kullanimDurumuBool = false;
                    }

                    kayitliFirmaDenetim.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliFirmaDenetim.FIRMA_LOGICALREF = sqlDataReader1["FIRMA_LOGICALREF"].ToString();
                    kayitliFirmaDenetim.DENETIM_LOGICALREF = sqlDataReader1["DENETIM_LOGICALREF"].ToString();
                    kayitliFirmaDenetim.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliFirmaDenetimList.Add(kayitliFirmaDenetim);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliFirmaDenetimList;
        }

        public static List<DENETIM> GetirSonDenetimList(string kullaniciLogicalref)
        {
            List<DENETIM> kayitliDenetimList = new List<DENETIM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = @"SELECT TOP(10) D.LOGICALREF,F.VKNTCKN,
                                    F.UNVAN,
                                    F.LOGO,
                                    D.AD,
                                    D.TARIH,
                                    Y.YIL_AD,
                                    DA.ACIKLAMA , 
                                    D.SURE  
                                    FROM KULLANICI_FIRMA KF 
                                    INNER JOIN FIRMA F ON F.LOGICALREF=KF.FIRMA_LOGICALREF
                                    INNER JOIN FIRMA_DENETIM FD ON FD.FIRMA_LOGICALREF=F.LOGICALREF
                                    INNER JOIN DENETIM D ON D.LOGICALREF=FD.DENETIM_LOGICALREF AND D.DURUM=1 AND D.KULLANIM_DURUMU=1
                                    INNER JOIN YIL Y ON Y.LOGICALREF=D.YIL_LOGICALREF
                                    INNER JOIN DONEM_AY DA ON DA.LOGICALREF=D.DONEM_AY_LOGICALREF
                                    WHERE KF.KULLANICI_LOGICALREF=@kullaniciLogicalref
                                    ORDER BY D.TARIH DESC
                                                            ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kullaniciLogicalref", kullaniciLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM kayitliDenetim = new DENETIM();
                    kayitliDenetim.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliDenetim.firmaVkn = sqlDataReader1["VKNTCKN"].ToString();
                    kayitliDenetim.firmaUnvan = sqlDataReader1["UNVAN"].ToString();
                    kayitliDenetim.AD = sqlDataReader1["AD"].ToString();
                    kayitliDenetim.TARIH = Convert.ToDateTime(sqlDataReader1["TARIH"].ToString());
                    kayitliDenetim.yilAd = sqlDataReader1["YIL_AD"].ToString();
                    kayitliDenetim.donemAy = sqlDataReader1["ACIKLAMA"].ToString();
                    kayitliDenetim.SURE = sqlDataReader1["SURE"].ToString();
                    if (!string.IsNullOrEmpty(kayitliDenetim.SURE))
                    {
                        decimal dakika = Math.Floor(Convert.ToDecimal(Convert.ToInt32(kayitliDenetim.SURE) / 60));
                        int saniye = Convert.ToInt32(kayitliDenetim.SURE) % 60;

                        var saniyeString = "";
                        if (saniye < 10)
                        { saniyeString = "0" + saniye.ToString(); }
                        else
                        { saniyeString = saniye.ToString(); }

                        kayitliDenetim.SURE = dakika.ToString() + ":" + saniyeString.ToString();
                    }

                    if (sqlDataReader1["LOGO"].GetType() != typeof(DBNull))
                    {
                        kayitliDenetim.LOGO = (byte[])sqlDataReader1["LOGO"];
                    }
                    else
                        kayitliDenetim.LOGO = null;

                    if (kayitliDenetim.LOGO != null)
                    {
                        string urunresimString = Convert.ToBase64String(kayitliDenetim.LOGO, 0, kayitliDenetim.LOGO.Length);
                        kayitliDenetim.resimSrc = "data:image/jpeg;base64," + urunresimString;
                    }
                    else
                    {
                        kayitliDenetim.resimSrc = "";
                    }

                    List<DENETIM_KAPSAM> denetimKapsamList = GetirDenetimKapsamListesiDenetimIdIle(kayitliDenetim.LOGICALREF);
                    kayitliDenetim.kapsamList = new List<KAPSAM>();
                    foreach (DENETIM_KAPSAM denetimKapsam in denetimKapsamList)
                    {
                        KAPSAM kapsam = CRUD.GetirKapsamIdIle(denetimKapsam.KAPSAM_LOGICALREF);
                        kayitliDenetim.kapsamList.Add(kapsam);
                    }
                    List<DENETIM_ALT_KAPSAM> denetimAltKapsamList = CRUD.GetirDenetimAltKapsamListesiDenetimIdIle(kayitliDenetim.LOGICALREF);
                    kayitliDenetim.altKapsamList = new List<ALT_KAPSAM>();
                    foreach (DENETIM_ALT_KAPSAM denetimAltKapsam in denetimAltKapsamList)
                    {
                        ALT_KAPSAM altKapsam = CRUD.GetirAltKapsamIdIle(denetimAltKapsam.ALT_KAPSAM_LOGICALREF);
                        kayitliDenetim.altKapsamList.Add(altKapsam);
                    }
                    //List<DENETIM_RAPOR> denetimRaporList= GetirDenetimRaporListDenetimIdIle(kayitliDenetim.LOGICALREF);
                    //kayitliDenetim.raporList = new List<RAPOR>();
                    //foreach (DENETIM_RAPOR denetimRapor in denetimRaporList)
                    //{
                    //    RAPOR rapor = new RAPOR();
                    //    rapor = CRUD.GetirRaporRaporIdIle(denetimRapor.RAPOR_LOGICALREF);
                    //    kayitliDenetim.raporList.Add(rapor);
                    //}
                    kayitliDenetimList.Add(kayitliDenetim);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliDenetimList;
        }

        public static List<DENETIM> GetirYoneticiDenetimList()
        {
            List<DENETIM> kayitliDenetimList = new List<DENETIM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = @"SELECT 
                                        D.LOGICALREF,
                                        F.VKNTCKN,
                                        F.UNVAN,
                                        F.LOGO,
                                        D.AD,
                                        F.LOGICALREF as FIRMA_LOGICALREF,
                                        D.TARIH,
                                        Y.YIL_AD, 
                                        D.SURE,
                                        D.DURUM
                                 FROM DENETIM D
                                 INNER JOIN FIRMA_DENETIM FD ON D.LOGICALREF=FD.DENETIM_LOGICALREF
                                 INNER JOIN FIRMA F ON F.LOGICALREF=FD.FIRMA_LOGICALREF
                                 INNER JOIN YIL Y ON Y.LOGICALREF=D.YIL_LOGICALREF
                                 WHERE D.KULLANIM_DURUMU=9 ";

                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM kayitliDenetim = new DENETIM();
                    kayitliDenetim.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliDenetim.firmaVkn = sqlDataReader1["VKNTCKN"].ToString();
                    kayitliDenetim.firmaUnvan = sqlDataReader1["UNVAN"].ToString();
                    kayitliDenetim.AD = sqlDataReader1["AD"].ToString();
                    kayitliDenetim.TARIH = Convert.ToDateTime(sqlDataReader1["TARIH"].ToString());
                    kayitliDenetim.yilAd = sqlDataReader1["YIL_AD"].ToString();
                    kayitliDenetim.firmaLogicalref = sqlDataReader1["FIRMA_LOGICALREF"].ToString();
                    kayitliDenetim.SURE = sqlDataReader1["SURE"].ToString();
                    kayitliDenetim.DURUM = Convert.ToInt32(sqlDataReader1["DURUM"].ToString());

                    if (!string.IsNullOrEmpty(kayitliDenetim.SURE))
                    {
                        decimal dakika = Math.Floor(Convert.ToDecimal(Convert.ToInt32(kayitliDenetim.SURE) / 60));
                        int saniye = Convert.ToInt32(kayitliDenetim.SURE) % 60;

                        var saniyeString = "";
                        if (saniye < 10)
                        { saniyeString = "0" + saniye.ToString(); }
                        else
                        { saniyeString = saniye.ToString(); }

                        kayitliDenetim.SURE = dakika.ToString() + ":" + saniyeString.ToString();
                    }

                    if (sqlDataReader1["LOGO"].GetType() != typeof(DBNull))
                    {
                        kayitliDenetim.LOGO = (byte[])sqlDataReader1["LOGO"];
                    }
                    else
                        kayitliDenetim.LOGO = null;

                    if (kayitliDenetim.LOGO != null)
                    {
                        string urunresimString = Convert.ToBase64String(kayitliDenetim.LOGO, 0, kayitliDenetim.LOGO.Length);
                        kayitliDenetim.resimSrc = "data:image/jpeg;base64," + urunresimString;
                    }
                    else
                    {
                        kayitliDenetim.resimSrc = "";
                    }
                    List<string> denetimKuralKodList = CRUD.GetirDenetimKuralKodDenetimIdIle(kayitliDenetim.LOGICALREF);
                    kayitliDenetim.kuralKodList = new List<string>();
                    foreach (string denetimKuralKod in denetimKuralKodList)
                    {
                        kayitliDenetim.kuralKodList.Add(denetimKuralKod);
                    }

                    kayitliDenetimList.Add(kayitliDenetim);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliDenetimList;
        }

        public static List<DENETIM> GetirSonRaporList(string kullaniciLogicalref)
        {
            List<DENETIM> kayitliDenetimList = new List<DENETIM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = @"SELECT TOP(10) 
                                    F.VKNTCKN,
                                    F.UNVAN,
                                    F.LOGO,
                                    D.AD,
                                    D.TARIH,
                                    Y.YIL_AD,
                                    DA.ACIKLAMA , 
                                    R.AD AS RAPOR_AD,
                                    R.LOGICALREF AS RAPOR_LOGICALREF
                                    FROM KULLANICI_FIRMA KF 
                                    INNER JOIN FIRMA F ON F.LOGICALREF=KF.FIRMA_LOGICALREF
                                    INNER JOIN FIRMA_DENETIM FD ON FD.FIRMA_LOGICALREF=F.LOGICALREF
                                    INNER JOIN DENETIM D ON D.LOGICALREF=FD.DENETIM_LOGICALREF AND D.DURUM=1 AND D.KULLANIM_DURUMU=1
                                    INNER JOIN YIL Y ON Y.LOGICALREF=D.YIL_LOGICALREF
                                    INNER JOIN DONEM_AY DA ON DA.LOGICALREF=D.DONEM_AY_LOGICALREF
                                    INNER JOIN DENETIM_RAPOR DR ON DR.DENETIM_LOGICALREF=D.LOGICALREF
                                    INNER JOIN RAPOR R ON R.LOGICALREF=DR.RAPOR_LOGICALREF
                                    WHERE KF.KULLANICI_LOGICALREF=@kullaniciLogicalref
                                    ORDER BY D.TARIH DESC
                                                            ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kullaniciLogicalref", kullaniciLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM kayitliDenetim = new DENETIM();
                    kayitliDenetim.firmaVkn = sqlDataReader1["VKNTCKN"].ToString();
                    kayitliDenetim.firmaUnvan = sqlDataReader1["UNVAN"].ToString();
                    kayitliDenetim.AD = sqlDataReader1["AD"].ToString();
                    kayitliDenetim.TARIH = Convert.ToDateTime(sqlDataReader1["TARIH"].ToString());
                    kayitliDenetim.yilAd = sqlDataReader1["YIL_AD"].ToString();
                    kayitliDenetim.donemAy = sqlDataReader1["ACIKLAMA"].ToString();
                    kayitliDenetim.raporLogicalref = sqlDataReader1["RAPOR_LOGICALREF"].ToString();
                    kayitliDenetim.raporAd = sqlDataReader1["RAPOR_AD"].ToString();

                    if (sqlDataReader1["LOGO"].GetType() != typeof(DBNull))
                    {
                        kayitliDenetim.LOGO = (byte[])sqlDataReader1["LOGO"];
                    }
                    else
                        kayitliDenetim.LOGO = null;

                    if (kayitliDenetim.LOGO != null)
                    {
                        string urunresimString = Convert.ToBase64String(kayitliDenetim.LOGO, 0, kayitliDenetim.LOGO.Length);
                        kayitliDenetim.resimSrc = "data:image/jpeg;base64," + urunresimString;
                    }
                    else
                    {
                        kayitliDenetim.resimSrc = "";
                    }
                    kayitliDenetimList.Add(kayitliDenetim);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliDenetimList;
        }

        public static List<DENETIM_KAPSAM> GetirDenetimKapsamListesiDenetimIdIle(string denetimLogicalref)
        {
            List<DENETIM_KAPSAM> kayitlidenetimKapsamList = new List<DENETIM_KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM DENETIM_KAPSAM WHERE DENETIM_LOGICALREF = @denetimLogicalref";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimLogicalref", denetimLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_KAPSAM kayitliDenetimKapsam = new DENETIM_KAPSAM();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliDenetimKapsam.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliDenetimKapsam.kullanimDurumuBool = false;
                    }

                    kayitliDenetimKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliDenetimKapsam.KAPSAM_LOGICALREF = sqlDataReader1["KAPSAM_LOGICALREF"].ToString();
                    kayitliDenetimKapsam.DENETIM_LOGICALREF = sqlDataReader1["DENETIM_LOGICALREF"].ToString();
                    kayitliDenetimKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitlidenetimKapsamList.Add(kayitliDenetimKapsam);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitlidenetimKapsamList;
        }

        public static List<DENETIM_ALT_KAPSAM> GetirDenetimAltKapsamListesiDenetimIdIle(string denetimLogicalref)
        {
            List<DENETIM_ALT_KAPSAM> kayitlidenetimAltKapsamList = new List<DENETIM_ALT_KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM DENETIM_ALT_KAPSAM WHERE DENETIM_LOGICALREF = @denetimLogicalref";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimLogicalref", denetimLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_ALT_KAPSAM kayitliDenetimAltKapsam = new DENETIM_ALT_KAPSAM();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliDenetimAltKapsam.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliDenetimAltKapsam.kullanimDurumuBool = false;
                    }

                    kayitliDenetimAltKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliDenetimAltKapsam.ALT_KAPSAM_LOGICALREF = sqlDataReader1["ALT_KAPSAM_LOGICALREF"].ToString();
                    kayitliDenetimAltKapsam.DENETIM_LOGICALREF = sqlDataReader1["DENETIM_LOGICALREF"].ToString();
                    kayitliDenetimAltKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitlidenetimAltKapsamList.Add(kayitliDenetimAltKapsam);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitlidenetimAltKapsamList;
        }

        public static List<string> GetirDenetimKuralKodDenetimIdIle(string denetimLogicalref)
        {
            List<string> kayitliKuralList = new List<string>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = @" SELECT DK.KOD FROM DENETIM_KURALLARI DK 
                                    INNER JOIN  DENETIM_DENETIM_KURALLARI DDK ON DK.LOGICALREF = DDK.DENETIM_KURALLARI_LOGICALREF
                                    AND DDK.DENETIM_LOGICALREF = @denetimLogicalref ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimLogicalref", denetimLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    string kuralKod = "";

                    kuralKod = sqlDataReader1["KOD"].ToString();
                    kayitliKuralList.Add(kuralKod);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKuralList;
        }

        public static List<DENETIM_YEVMIYE_DEFTER_AD> GetirYevmiyDefterAdListesiDenetimIdIle(string denetimLogicalref)
        {
            List<DENETIM_YEVMIYE_DEFTER_AD> kayitliList = new List<DENETIM_YEVMIYE_DEFTER_AD>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM DENETIM_YEVMIYE_DEFTER_AD WHERE DENETIM_LOGICALREF = @denetimLogicalref";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimLogicalref", denetimLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_YEVMIYE_DEFTER_AD kayitliYevmiyeDefterAd = new DENETIM_YEVMIYE_DEFTER_AD();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliYevmiyeDefterAd.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliYevmiyeDefterAd.kullanimDurumuBool = false;
                    }

                    kayitliYevmiyeDefterAd.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliYevmiyeDefterAd.YEVMIYE_DEFTER_AD = sqlDataReader1["YEVMIYE_DEFTER_AD"].ToString();
                    kayitliYevmiyeDefterAd.DENETIM_LOGICALREF = sqlDataReader1["DENETIM_LOGICALREF"].ToString();
                    kayitliYevmiyeDefterAd.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliList.Add(kayitliYevmiyeDefterAd);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliList;
        }

        public static void FirmaEkle(FIRMA firma)
        {

            String query = "INSERT INTO FIRMA " +
                " (VKNTCKN," +
                " LOGICALREF," +
                " UNVAN,";
            if (firma.LOGO != null)
                query = query + " LOGO,";
            query = query + " KULLANIM_DURUMU)" +
                " VALUES" +
                " (@vkntckn," +
                " @logicalRef," +
                " @unvan,";
            if (firma.LOGO != null)
                query = query + " @resim,";
            query = query + " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", firma.LOGICALREF);
                    command.Parameters.AddWithValue("@vkntckn", firma.VKNTCKN);
                    command.Parameters.AddWithValue("@unvan", firma.UNVAN);
                    if (firma.LOGO != null)
                        command.Parameters.AddWithValue("@resim", firma.LOGO);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void DenetimEkle(DENETIM denetim)
        {
            String query = " INSERT INTO DENETIM (AD, LOGICALREF, YIL_LOGICALREF, TARIH, DURUM, DONEM_AY_LOGICALREF,KONTOR_SAYISI, KULLANIM_DURUMU)" +
                           " VALUES " +
                           " (@ad, @logicalRef, @yilLogicalRef, @tarih, @durum, @donemTur,@kontorSayisi, @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", denetim.LOGICALREF);
                    command.Parameters.AddWithValue("@yilLogicalRef", denetim.YIL_LOGICALREF);
                    command.Parameters.AddWithValue("@ad", denetim.AD);
                    command.Parameters.AddWithValue("@kontorSayisi", denetim.KONTOR_SAYISI);
                    command.Parameters.AddWithValue("@tarih", denetim.TARIH);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.Parameters.AddWithValue("@donemTur", denetim.DONEM_AY_LOGICALREF);
                    command.Parameters.AddWithValue("@durum", 0);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void YoneticiDenetimEkle(DENETIM denetim)
        {
            String query = " INSERT INTO DENETIM (AD, LOGICALREF, YIL_LOGICALREF, TARIH, DURUM, DONEM_AY_LOGICALREF, KULLANIM_DURUMU)" +
                           " VALUES " +
                           " (@ad, @logicalRef, @yilLogicalRef, @tarih, @durum, @donemTur, @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", denetim.LOGICALREF);
                    command.Parameters.AddWithValue("@yilLogicalRef", denetim.YIL_LOGICALREF);
                    command.Parameters.AddWithValue("@ad", denetim.AD);
                    command.Parameters.AddWithValue("@tarih", denetim.TARIH);
                    command.Parameters.AddWithValue("@kullanimDurumu", 9);//9 Yönetici Denetimi
                    command.Parameters.AddWithValue("@donemTur", "");
                    command.Parameters.AddWithValue("@durum", 0);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void FirmaDenetimEkle(FIRMA_DENETIM firmaDenetim, int kullanımDurumu)
        {
            String query =  " INSERT INTO FIRMA_DENETIM " +
                            " (LOGICALREF, FIRMA_LOGICALREF, DENETIM_LOGICALREF, KULLANIM_DURUMU) " +
                            " VALUES" +
                            " (@logicalRef, @firmaLogicalRef, @denetimLogicalRef, @kullanimDurumu) ";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", firmaDenetim.LOGICALREF);
                    command.Parameters.AddWithValue("@firmaLogicalRef", firmaDenetim.FIRMA_LOGICALREF);
                    command.Parameters.AddWithValue("@denetimLogicalRef", firmaDenetim.DENETIM_LOGICALREF);
                    command.Parameters.AddWithValue("@kullanimDurumu", kullanımDurumu);//9 Yönetici FirmaDenetim, 1 KullanıcıDenetimi
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        
        public static void DenetimDenetimKuralllariEkle(DENETIM_DENETIM_KURALLARI denetimKurallari)
        {

            String query = "INSERT INTO DENETIM_DENETIM_KURALLARI " +
                " (LOGICALREF, DENETIM_KURALLARI_LOGICALREF, DENETIM_LOGICALREF)" +
                " VALUES" +
                " (@logicalRef, @denetimKurallariLogicalref, @denetimLogicalRef)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", denetimKurallari.LOGICALREF);
                    command.Parameters.AddWithValue("@denetimKurallariLogicalref", denetimKurallari.DENETIM_KURALLARI_LOGICALREF);
                    command.Parameters.AddWithValue("@denetimLogicalRef", denetimKurallari.DENETIM_LOGICALREF);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void DenetimKapsamEkle(DENETIM_KAPSAM denetimKapsam)
        {
            String query = "INSERT INTO DENETIM_KAPSAM " +
                " (LOGICALREF, DENETIM_LOGICALREF, KAPSAM_LOGICALREF, KULLANIM_DURUMU)" +
                " VALUES " +
                " (@logicalRef, @denetimLogicalRef, @kapsamLogicalRef, @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", denetimKapsam.LOGICALREF);
                    command.Parameters.AddWithValue("@denetimLogicalRef", denetimKapsam.DENETIM_LOGICALREF);
                    command.Parameters.AddWithValue("@kapsamLogicalRef", denetimKapsam.KAPSAM_LOGICALREF);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void DenetimAltKapsamEkle(DENETIM_ALT_KAPSAM denetimAltKapsam)
        {
            String query = "INSERT INTO DENETIM_ALT_KAPSAM " +
                " (LOGICALREF," +
                " DENETIM_LOGICALREF," +
                " ALT_KAPSAM_LOGICALREF," +
                " KULLANIM_DURUMU)" +
                " VALUES" +
                " (@logicalRef," +
                " @denetimLogicalRef," +
                " @altKapsamLogicalRef," +
                " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", denetimAltKapsam.LOGICALREF);
                    command.Parameters.AddWithValue("@denetimLogicalRef", denetimAltKapsam.DENETIM_LOGICALREF);
                    command.Parameters.AddWithValue("@altKapsamLogicalRef", denetimAltKapsam.ALT_KAPSAM_LOGICALREF);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static FIRMA GetirFirmaIdIle(string logicalRef)
        {
            FIRMA kayitliFirma = new FIRMA();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM FIRMA " +
                                " WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kayitliFirma.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kayitliFirma.UNVAN = sqlDataReader1["UNVAN"].ToString();
                kayitliFirma.VKNTCKN = sqlDataReader1["VKNTCKN"].ToString();
                kayitliFirma.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                if (sqlDataReader1["LOGO"].GetType() != typeof(DBNull))
                {
                    kayitliFirma.LOGO = (byte[])sqlDataReader1["LOGO"];
                }
                else
                    kayitliFirma.LOGO = null;

                if (kayitliFirma.LOGO != null)
                {
                    string urunresimString = Convert.ToBase64String(kayitliFirma.LOGO, 0, kayitliFirma.LOGO.Length);
                    kayitliFirma.resimSrc = "data:image/jpeg;base64," + urunresimString;
                }
                else
                {
                    kayitliFirma.resimSrc = "";
                }
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliFirma;
        }

        public static BILDIRIM GetirBildirimIdIle(string logicalRef)
        {
            BILDIRIM kayitlİBildirim = new BILDIRIM();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM BILDIRIM " +
                                " WHERE LOGICALREF=@logicalRef AND KULLANIM_DURUMU=1 ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kayitlİBildirim.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kayitlİBildirim.ACIKLAMA_KISA = sqlDataReader1["ACIKLAMA_KISA"].ToString();
                kayitlİBildirim.ACIKLAMA_UZUN = sqlDataReader1["ACIKLAMA_UZUN"].ToString();
                kayitlİBildirim.TARIH = Convert.ToDateTime(sqlDataReader1["TARIH"].ToString());
                kayitlİBildirim.DURUMU = Convert.ToInt32(sqlDataReader1["DURUMU"].ToString());
                kayitlİBildirim.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
             
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitlİBildirim;
        }
        public static FIRMA GetirFirmaDenetimIdIle(string denetimId)
        {
            FIRMA kayitliFirma = new FIRMA();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            
            try
            {
                string query2 = @"SELECT F.LOGICALREF, F.UNVAN,F.VKNTCKN FROM FIRMA_DENETIM FD
                                INNER JOIN  FIRMA F ON F.LOGICALREF=FD.FIRMA_LOGICALREF
                                WHERE DENETIM_LOGICALREF=@denetimId";

                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimId", denetimId);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kayitliFirma.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kayitliFirma.UNVAN = sqlDataReader1["UNVAN"].ToString();
                kayitliFirma.VKNTCKN = sqlDataReader1["VKNTCKN"].ToString();
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliFirma;
        }

        public static DENETIM GetirDenetimIdIle(string logicalRef)
        {
            DENETIM denetim = new DENETIM();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM DENETIM WHERE LOGICALREF=@logicalRef AND KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                denetim.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                denetim.YIL_LOGICALREF = sqlDataReader1["YIL_LOGICALREF"].ToString();
                denetim.AD = sqlDataReader1["AD"].ToString();
                denetim.SURE = sqlDataReader1["SURE"].ToString();
                denetim.TARIH = Convert.ToDateTime(sqlDataReader1["TARIH"].ToString());
                denetim.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                denetim.KONTOR_SAYISI = Convert.ToInt32(sqlDataReader1["KONTOR_SAYISI"]);
                denetim.KURAL_SAYISI = Convert.ToInt32(sqlDataReader1["KURAL_SAYISI"]);
                denetim.DURUM = Convert.ToInt32(sqlDataReader1["DURUM"]);
                denetim.DONEM_AY_LOGICALREF = sqlDataReader1["DONEM_AY_LOGICALREF"].ToString();
                
                if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                {
                    denetim.kullanimDurumuBool = true;
                }
                else
                {
                    denetim.kullanimDurumuBool = false;
                }

                if (!string.IsNullOrEmpty(denetim.DONEM_AY_LOGICALREF))
                {
                    DONEM_AY donemAy = GetirDonemAyIdIle(denetim.DONEM_AY_LOGICALREF);
                    if (donemAy != null)
                        denetim.donemAy = donemAy.ACIKLAMA;
                    else
                        denetim.donemAy = "";
                }
                else
                    denetim.donemAy = "";

            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return denetim;
        }

        public static DENETIM GetirYoneticiDenetimIdIle(string logicalRef)
        {
            DENETIM denetim = new DENETIM();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM DENETIM WHERE LOGICALREF=@logicalRef AND KULLANIM_DURUMU=9";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                denetim.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                denetim.YIL_LOGICALREF = sqlDataReader1["YIL_LOGICALREF"].ToString();
                denetim.AD = sqlDataReader1["AD"].ToString();
                denetim.SURE = sqlDataReader1["SURE"].ToString();
                denetim.TARIH = Convert.ToDateTime(sqlDataReader1["TARIH"].ToString());
                denetim.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                denetim.DURUM = Convert.ToInt32(sqlDataReader1["DURUM"]);
                denetim.DONEM_AY_LOGICALREF = sqlDataReader1["DONEM_AY_LOGICALREF"].ToString();

                if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 9)
                {
                    denetim.kullanimDurumuBool = true;
                }
                else
                {
                    denetim.kullanimDurumuBool = false;
                }
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return denetim;
        }

        public static DONEM_AY GetirDonemAyIdIle(string logicalRef)
        {
            DONEM_AY donemAy = new DONEM_AY();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            
            try
            {
                string query2 = " SELECT * FROM DONEM_AY WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                donemAy.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                donemAy.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                donemAy.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
            }
            catch (Exception exp)
            {
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return donemAy;
        }

        public static YIL GetirYilIdIle(string logicalRef)
        {
            YIL yil = new YIL();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            
            try
            {
                string query2 = " SELECT * FROM YIL WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                yil.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                yil.YIL_AD = sqlDataReader1["YIL_AD"].ToString();
                yil.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return yil;
        }
        public static int GetirToplamKuralSayisiSayisiDenetimIdveLKapsamIle(string logicalRef, string kapsamLogicalref)
        {
            int toplamKuralSayisi = 0;
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();

            try
            {
                string query2 = @"SELECT ISNULL(COUNT(DAK.ALT_KAPSAM_LOGICALREF),0) AS KURAL_SAYISI FROM DENETIM_ALT_KAPSAM DAK
                                    LEFT JOIN DENETIM_KURALLARI_ALT_KAPSAM DKAK ON DKAK.ALT_KAPSAM_LOGICALREF=DAK.ALT_KAPSAM_LOGICALREF
                                    LEFT JOIN DENETIM_KURALLARI DK ON DK.LOGICALREF=DKAK.DENETIM_KURALLARI_LOGICALREF
                                    INNER JOIN DENETIM_KURALLARI_KAPSAM DKK ON DKK.DENETIM_KURALLARI_LOGICALREF=DK.LOGICALREF 
                                    AND DKK.KAPSAM_LOGICALREF=@kapsamLogicalref
                                    WHERE DAK.DENETIM_LOGICALREF=@logicalRef";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                sqlCmd2.Parameters.AddWithValue("@kapsamLogicalref", kapsamLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                toplamKuralSayisi = Convert.ToInt32(sqlDataReader1["KURAL_SAYISI"].ToString());
             
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return toplamKuralSayisi;
        }
        public static KAPSAM GetirKapsamIdIle(string logicalRef)
        {
            KAPSAM kapsam = new KAPSAM();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM KAPSAM WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kapsam.AD = sqlDataReader1["AD"].ToString();
                kapsam.KISA_AD = sqlDataReader1["KISA_AD"].ToString();
                kapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
            }
            catch (Exception exp)
            {
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kapsam;
        }

        public static ALT_KAPSAM GetirAltKapsamIdIle(string logicalRef)
        {
            ALT_KAPSAM altKapsam = new ALT_KAPSAM();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM ALT_KAPSAM WHERE LOGICALREF=@logicalRef AND KULLANIM_DURUMU=1 ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                altKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                altKapsam.AD = sqlDataReader1["AD"].ToString();
                altKapsam.KISA_AD = sqlDataReader1["KISA_AD"].ToString();
                altKapsam.KONTOR = Convert.ToInt32(sqlDataReader1["KONTOR"].ToString());
                altKapsam.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                altKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return altKapsam;
        }
        
        public static KAPSAM GetirKapsamKısaAdIle(string kisaAd)
        {
            KAPSAM kapsam = new KAPSAM();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM KAPSAM " +
                                " WHERE KISA_AD=@kisaAd AND KULLANIM_DURUMU=1 ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kisaAd", kisaAd);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kapsam.AD = sqlDataReader1["AD"].ToString();
                kapsam.KISA_AD = sqlDataReader1["KISA_AD"].ToString();
                kapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kapsam;
        }

        public static void GuncelleFirma(FIRMA firma)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query = " UPDATE FIRMA  SET UNVAN=@unvan, ";

                if (firma.LOGO != null)
                    query = query + " LOGO=@logo,";

                query = query + " KULLANIM_DURUMU=@kullanimDurumu WHERE LOGICALREF=@logicalRef ";

                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@unvan", firma.UNVAN);
                sqlCmd2.Parameters.AddWithValue("@kullanimDurumu", firma.KULLANIM_DURUMU);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", firma.LOGICALREF);
                if (firma.LOGO != null)
                    sqlCmd2.Parameters.AddWithValue("@logo", firma.LOGO);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        public static void GuncelleBildirim(BILDIRIM bildirim)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query =    @"UPDATE BILDIRIM  SET DURUMU=@durumu, 
                                    KULLANIM_DURUMU=@kullanimDurumu
                                    WHERE LOGICALREF=@logicalRef";

              
                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@durumu", bildirim.DURUMU);
                sqlCmd2.Parameters.AddWithValue("@kullanimDurumu", bildirim.KULLANIM_DURUMU);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", bildirim.LOGICALREF);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        public static void GuncelleDenetimKurallari(DENETIM_KURALLARI denetimKural)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query = " UPDATE DENETIM_KURALLARI" +
                                  " SET " +
                                  " KOD=@kod," +
                                  " ACIKLAMA=@aciklama," +
                                  " MUSTERI_ACIKLAMA=@musteriAciklama," +
                                  " MUSTERI_ACIKLAMA2=@musteriAciklama2," +
                                  " MEVZUAT=@mevzuat," +
                                  " KULLANIM_DURUMU=@kullanimDurumu" +
                                " WHERE LOGICALREF=@logicalRef ";

                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kod", denetimKural.KOD);
                sqlCmd2.Parameters.AddWithValue("@aciklama", denetimKural.ACIKLAMA);
                sqlCmd2.Parameters.AddWithValue("@musteriAciklama", denetimKural.MUSTERI_ACIKLAMA);
                sqlCmd2.Parameters.AddWithValue("@musteriAciklama2", denetimKural.MUSTERI_ACIKLAMA2);
                sqlCmd2.Parameters.AddWithValue("@mevzuat", denetimKural.MEVZUAT);
                sqlCmd2.Parameters.AddWithValue("@kullanimDurumu", denetimKural.KULLANIM_DURUMU);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", denetimKural.LOGICALREF);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void GuncelleDenetimKurallariSqlLogicalRefIle(string logicalRef, string sqlIfade)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query = " UPDATE DENETIM_KURALLARI SET SQL_IFADE=@sqlIfade WHERE LOGICALREF=@logicalRef ";

                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@sqlIfade", sqlIfade);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void GuncelleDenetim(DENETIM denetim)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query = " UPDATE DENETIM" +
                                  " SET " +
                                  " AD=@ad," +
                                  " TARIH=@tarih," +
                                  " KULLANIM_DURUMU=@kullanimDurumu" +
                                " WHERE LOGICALREF=@logicalRef ";

                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@ad", denetim.AD);
                sqlCmd2.Parameters.AddWithValue("@tarih", denetim.TARIH);
                sqlCmd2.Parameters.AddWithValue("@kullanimDurumu", denetim.KULLANIM_DURUMU);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", denetim.LOGICALREF);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        public static void GuncelleDenetimKuralSayisi(DENETIM denetim)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query = " UPDATE DENETIM" +
                                  " SET " +
                                  " KURAL_SAYISI=@kuralSayisi" +
                                " WHERE LOGICALREF=@logicalRef ";

                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kuralSayisi", denetim.KURAL_SAYISI);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", denetim.LOGICALREF);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        public static void GuncelleFirmaDenetim(string denetimLogicalref, int kullanimDurumu)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query = " UPDATE FIRMA_DENETIM" +
                                  " SET " +
                                  " KULLANIM_DURUMU=@kullanimDurumu" +
                                " WHERE DENETIM_LOGICALREF=@logicalRef ";

                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", denetimLogicalref);
                sqlCmd2.Parameters.AddWithValue("@kullanimDurumu", kullanimDurumu);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void DenetimKuraliEkle(DENETIM_KURALLARI kural)
        {
            String query = "INSERT INTO DENETIM_KURALLARI " +
                " (KOD," +
                " LOGICALREF," +
                " ACIKLAMA," +
                " MUSTERI_ACIKLAMA," +
                " MUSTERI_ACIKLAMA2," +
                " MEVZUAT," +
                " SQL_IFADE," +
                " KULLANIM_DURUMU)" +
                " VALUES" +
                " (@kod," +
                " @logicalRef," +
                " @aciklama," +
                " @musteriAciklama," +
                " @mustericiklama2," +
                " @mevzuat," +
                " @sqlIfade," +
                " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", kural.LOGICALREF);
                    command.Parameters.AddWithValue("@kod", kural.KOD);
                    command.Parameters.AddWithValue("@aciklama", kural.ACIKLAMA);
                    command.Parameters.AddWithValue("@musteriAciklama", kural.MUSTERI_ACIKLAMA);
                    command.Parameters.AddWithValue("@mustericiklama2", kural.MUSTERI_ACIKLAMA2);
                    command.Parameters.AddWithValue("@mevzuat", kural.MEVZUAT);
                    command.Parameters.AddWithValue("@sqlIfade", kural.SQL_IFADE);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void DenetimKurallariKapsamEkle(DENETIM_KURALLARI_KAPSAM denetimKuralKapsam)
        {
            String query = " INSERT INTO DENETIM_KURALLARI_KAPSAM " +
                " (LOGICALREF, DENETIM_KURALLARI_LOGICALREF, KAPSAM_LOGICALREF, KULLANIM_DURUMU)" +
                " VALUES" +
                " (@logicalRef, @denetimKurallariLogicalRef, @kapsamLogicalRef, @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();

            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", denetimKuralKapsam.LOGICALREF);
                    command.Parameters.AddWithValue("@denetimKurallariLogicalRef", denetimKuralKapsam.DENETIM_KURALLARI_LOGICALREF);
                    command.Parameters.AddWithValue("@kapsamLogicalRef", denetimKuralKapsam.KAPSAM_LOGICALREF);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        
        public static void DenetimKurallariAltKapsamEkle(DENETIM_KURALLARI_ALT_KAPSAM denetimKuralKapsam)
        {

            String query = "INSERT INTO DENETIM_KURALLARI_ALT_KAPSAM " +
                " (LOGICALREF, DENETIM_KURALLARI_LOGICALREF, ALT_KAPSAM_LOGICALREF, KULLANIM_DURUMU)" +
                " VALUES" +
                " (@logicalRef, @denetimKurallariLogicalRef, @altKapsamLogicalRef, @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", denetimKuralKapsam.LOGICALREF);
                    command.Parameters.AddWithValue("@denetimKurallariLogicalRef", denetimKuralKapsam.DENETIM_KURALLARI_LOGICALREF);
                    command.Parameters.AddWithValue("@altKapsamLogicalRef", denetimKuralKapsam.ALT_KAPSAM_LOGICALREF);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        
        public static List<DENETIM_KURALLARI> GetirDenetimKurallariListesi()
        {
            List<DENETIM_KURALLARI> kayitliKuralList = new List<DENETIM_KURALLARI>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = @"SELECT   
                                 LOGICALREF,   
                                 KOD, 
                                     CASE SQL_IFADE
                                     WHEN '' THEN 0
                                     WHEN NULL THEN 0
                                     ELSE 1  
                                     END AS SQL_IFADE,   
                                 ACIKLAMA,   
                                 MUSTERI_ACIKLAMA,   
                                 MEVZUAT,   
                                 MUSTERI_ACIKLAMA2,    
                                 KULLANIM_DURUMU    
                                 FROM DENETIM_KURALLARI WHERE KULLANIM_DURUMU=1 ORDER BY KOD";

                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_KURALLARI kayitliKural = new DENETIM_KURALLARI();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliKural.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliKural.kullanimDurumuBool = false;
                    }

                    kayitliKural.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKural.KOD = sqlDataReader1["KOD"].ToString();
                    kayitliKural.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                    kayitliKural.MUSTERI_ACIKLAMA = sqlDataReader1["MUSTERI_ACIKLAMA"].ToString();
                    kayitliKural.MUSTERI_ACIKLAMA2 = sqlDataReader1["MUSTERI_ACIKLAMA2"].ToString();
                    kayitliKural.MEVZUAT = sqlDataReader1["MEVZUAT"].ToString();
                    kayitliKural.SQL_IFADE = sqlDataReader1["SQL_IFADE"].ToString();
                    kayitliKural.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliKural.kapsamList = new List<KAPSAM>();
                    kayitliKural.altKapsamList = new List<ALT_KAPSAM>();
                    List<DENETIM_KURALLARI_KAPSAM> denetimKapsamList = CRUD.GetirDenetimKurallariKapsamListesiDenetimKuralIdIle(kayitliKural.LOGICALREF);
                    foreach (DENETIM_KURALLARI_KAPSAM denetimKapsam in denetimKapsamList)
                    {
                        KAPSAM kapsam = CRUD.GetirKapsamIdIle(denetimKapsam.KAPSAM_LOGICALREF);
                        kayitliKural.kapsamList.Add(kapsam);
                    }
                    List<DENETIM_KURALLARI_ALT_KAPSAM> denetimAltKapsamList = CRUD.GetirDenetimKurallariAltKapsamListesiDenetimKuralIdIle(kayitliKural.LOGICALREF);
                    foreach (DENETIM_KURALLARI_ALT_KAPSAM denetimAltKapsam in denetimAltKapsamList)
                    {
                        ALT_KAPSAM altKapsam = CRUD.GetirAltKapsamIdIle(denetimAltKapsam.ALT_KAPSAM_LOGICALREF);
                        kayitliKural.altKapsamList.Add(altKapsam);
                    }
                    kayitliKuralList.Add(kayitliKural);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKuralList;
        }
        
        public static List<DENETIM_KURALLARI> GetirDenetimKurallariLogicalrefVeKodListesi()
        {
            List<DENETIM_KURALLARI> kayitliKuralList = new List<DENETIM_KURALLARI>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = @"SELECT   
                                 LOGICALREF,   
                                 KOD   
                                 FROM DENETIM_KURALLARI WHERE KULLANIM_DURUMU=1 ORDER BY KOD";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_KURALLARI kayitliKural = new DENETIM_KURALLARI();
                    kayitliKural.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKural.KOD = sqlDataReader1["KOD"].ToString();
                    kayitliKuralList.Add(kayitliKural);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKuralList;
        }

        public static List<DENETIM_KURALLARI_KAPSAM> GetirDenetimKurallariKapsamListesiDenetimKuralIdIle(string denetimKuralLogicalref)
        {
            List<DENETIM_KURALLARI_KAPSAM> kayitlidenetimKapsamList = new List<DENETIM_KURALLARI_KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM DENETIM_KURALLARI_KAPSAM WHERE DENETIM_KURALLARI_LOGICALREF = @denetimKuralLogicalref AND KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimKuralLogicalref", denetimKuralLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_KURALLARI_KAPSAM kayitliDenetimKuraliKapsam = new DENETIM_KURALLARI_KAPSAM();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliDenetimKuraliKapsam.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliDenetimKuraliKapsam.kullanimDurumuBool = false;
                    }

                    kayitliDenetimKuraliKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliDenetimKuraliKapsam.KAPSAM_LOGICALREF = sqlDataReader1["KAPSAM_LOGICALREF"].ToString();
                    kayitliDenetimKuraliKapsam.DENETIM_KURALLARI_LOGICALREF = sqlDataReader1["DENETIM_KURALLARI_LOGICALREF"].ToString();
                    kayitliDenetimKuraliKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitlidenetimKapsamList.Add(kayitliDenetimKuraliKapsam);
                }
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kayitlidenetimKapsamList;
        }

        public static List<DENETIM_KURALLARI_ALT_KAPSAM> GetirDenetimKurallariAltKapsamListesiDenetimKuralIdIle(string denetimKuralLogicalref)
        {
            List<DENETIM_KURALLARI_ALT_KAPSAM> kayitlidenetimAltKapsamList = new List<DENETIM_KURALLARI_ALT_KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM DENETIM_KURALLARI_ALT_KAPSAM WHERE DENETIM_KURALLARI_LOGICALREF = @denetimKuralLogicalref AND KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimKuralLogicalref", denetimKuralLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_KURALLARI_ALT_KAPSAM kayitliDenetimKuraliAltKapsam = new DENETIM_KURALLARI_ALT_KAPSAM();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliDenetimKuraliAltKapsam.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliDenetimKuraliAltKapsam.kullanimDurumuBool = false;
                    }

                    kayitliDenetimKuraliAltKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliDenetimKuraliAltKapsam.ALT_KAPSAM_LOGICALREF = sqlDataReader1["ALT_KAPSAM_LOGICALREF"].ToString();
                    kayitliDenetimKuraliAltKapsam.DENETIM_KURALLARI_LOGICALREF = sqlDataReader1["DENETIM_KURALLARI_LOGICALREF"].ToString();
                    kayitliDenetimKuraliAltKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitlidenetimAltKapsamList.Add(kayitliDenetimKuraliAltKapsam);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitlidenetimAltKapsamList;
        }

        public static DENETIM_KURALLARI GetirDenetimKurallariIdIle(string logicalRef)
        {
            DENETIM_KURALLARI kayitliKural = new DENETIM_KURALLARI();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM DENETIM_KURALLARI WHERE LOGICALREF=@logicalRef ";

                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kayitliKural.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kayitliKural.KOD = sqlDataReader1["KOD"].ToString();
                kayitliKural.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                kayitliKural.MUSTERI_ACIKLAMA = sqlDataReader1["MUSTERI_ACIKLAMA"].ToString();
                kayitliKural.MUSTERI_ACIKLAMA2 = sqlDataReader1["MUSTERI_ACIKLAMA2"].ToString();
                kayitliKural.MEVZUAT = sqlDataReader1["MEVZUAT"].ToString();
                kayitliKural.SQL_IFADE = sqlDataReader1["SQL_IFADE"].ToString();
                kayitliKural.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKural;
        }

        public static DENETIM_KURALLARI GetirDenetimKurallariKodIle(string kod)
        {
            DENETIM_KURALLARI kayitliKural = new DENETIM_KURALLARI();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM DENETIM_KURALLARI WHERE KOD=@kod ";

                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kod", kod);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kayitliKural.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kayitliKural.KOD = sqlDataReader1["KOD"].ToString();
                kayitliKural.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                kayitliKural.MUSTERI_ACIKLAMA = sqlDataReader1["MUSTERI_ACIKLAMA"].ToString();
                kayitliKural.MUSTERI_ACIKLAMA2 = sqlDataReader1["MUSTERI_ACIKLAMA2"].ToString();
                kayitliKural.MEVZUAT = sqlDataReader1["MEVZUAT"].ToString();
                kayitliKural.SQL_IFADE = sqlDataReader1["SQL_IFADE"].ToString();
                kayitliKural.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKural;
        }

        public static DENETIM_KURALLARI GetirDenetimKurallariSqlsizIdIle(string logicalRef)
        {
            DENETIM_KURALLARI kayitliKural = new DENETIM_KURALLARI();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = " SELECT " +
                    " LOGICALREF, " +
                    " KOD, ACIKLAMA, " +
                    " MUSTERI_ACIKLAMA, " +
                    " MUSTERI_ACIKLAMA2, " +
                    " MEVZUAT, " +
                    " KULLANIM_DURUMU " +
                    " FROM DENETIM_KURALLARI " +
                    " WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kayitliKural.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kayitliKural.KOD = sqlDataReader1["KOD"].ToString();
                kayitliKural.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                kayitliKural.MUSTERI_ACIKLAMA = sqlDataReader1["MUSTERI_ACIKLAMA"].ToString();
                kayitliKural.MUSTERI_ACIKLAMA2 = sqlDataReader1["MUSTERI_ACIKLAMA2"].ToString();
                kayitliKural.MEVZUAT = sqlDataReader1["MEVZUAT"].ToString();
                kayitliKural.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitliKural;
        }

        public static List<DENETIM_KURALLARI> GetirDenetimKurallariListesiDenetimIdIle(string denetimLogicalRef, string kapsamLogicalref)
        {
            List<DENETIM_KURALLARI> kayitlidenetimKurallariList = new List<DENETIM_KURALLARI>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //string query2 = @" SELECT * FROM DENETIM_KURALLARI   WHERE LOGICALREF IN
                //                    (
                //                        SELECT
                //                        DISTINCT DENETIM_KURALLARI_KAPSAM.DENETIM_KURALLARI_LOGICALREF
                //                        FROM DENETIM_KURALLARI_KAPSAM
                //                        INNER JOIN DENETIM_KURALLARI ON DENETIM_KURALLARI.LOGICALREF = DENETIM_KURALLARI_KAPSAM.DENETIM_KURALLARI_LOGICALREF
                //                                                    AND DENETIM_KURALLARI.KULLANIM_DURUMU = 1
                //                        WHERE KAPSAM_LOGICALREF IN                                        (
                //                        SELECT KAPSAM_LOGICALREF FROM DENETIM_KAPSAM
                //                        WHERE DENETIM_LOGICALREF = @denetimLogicalRef
                //                        )
                //                    ) ";

                string query2 = @"  select dk.*
                                    from DENETIM_KURALLARI dk 
                                    inner join DENETIM_KURALLARI_KAPSAM dkk on dkk.DENETIM_KURALLARI_LOGICALREF = dk.LOGICALREF
										                                      and dkk.KAPSAM_LOGICALREF = @kapsamLogicalref
                                    where dk.LOGICALREF in
                                    (
	                                    select distinct dkak.DENETIM_KURALLARI_LOGICALREF 
	                                    from DENETIM_KURALLARI_ALT_KAPSAM dkak
	                                    inner join DENETIM_KURALLARI dk on dk.LOGICALREF = dkak.DENETIM_KURALLARI_LOGICALREF
									                                       and dk.KULLANIM_DURUMU = 1
	                                    where dkak.ALT_KAPSAM_LOGICALREF in
	                                    (
		                                    select dak.ALT_KAPSAM_LOGICALREF
		                                    from DENETIM_ALT_KAPSAM dak
		                                    where dak.DENETIM_LOGICALREF = @denetimLogicalRef
	                                    )
                                    ) ";



                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimLogicalRef", denetimLogicalRef);
                sqlCmd2.Parameters.AddWithValue("@kapsamLogicalref", kapsamLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_KURALLARI kayitliKural = new DENETIM_KURALLARI();
                    kayitliKural.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKural.KOD = sqlDataReader1["KOD"].ToString();
                    kayitliKural.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                    kayitliKural.MUSTERI_ACIKLAMA = sqlDataReader1["MUSTERI_ACIKLAMA"].ToString();
                    kayitliKural.MUSTERI_ACIKLAMA2 = sqlDataReader1["MUSTERI_ACIKLAMA2"].ToString();
                    kayitliKural.MEVZUAT = sqlDataReader1["MEVZUAT"].ToString();
                    kayitliKural.SQL_IFADE = sqlDataReader1["SQL_IFADE"].ToString();
                    kayitliKural.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitlidenetimKurallariList.Add(kayitliKural);
                }
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitlidenetimKurallariList;
        }

        public static void SilDenetimKuralKapsamKuralLogicalRefIle(string logicalRef)
        {

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " DELETE DENETIM_KURALLARI_KAPSAM " +
                                " WHERE DENETIM_KURALLARI_LOGICALREF=@logicalRef" +
                                " AND KULLANIM_DURUMU = 1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                sqlCmd2.ExecuteNonQuery();

            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }

        }

        public static void SilDenetimKuralAltKapsamKuralLogicalRefIle(string logicalRef)
        {

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " DELETE DENETIM_KURALLARI_ALT_KAPSAM " +
                                " WHERE DENETIM_KURALLARI_LOGICALREF=@logicalRef" +
                                " AND KULLANIM_DURUMU = 1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                sqlCmd2.ExecuteNonQuery();

            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }

        }
        
        public static void SilDenetimKapsamDenetimLogicalRefIle(string logicalRef)
        {

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " DELETE DENETIM_KAPSAM " +
                                " WHERE DENETIM_LOGICALREF=@logicalRef" +
                                " AND KULLANIM_DURUMU = 1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                sqlCmd2.ExecuteNonQuery();

            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }

        }

        public static void YevmiyeDefteriAdEkle(DENETIM_YEVMIYE_DEFTER_AD denetimYevmiyeDefteriAd)
        {

            String query = "INSERT INTO DENETIM_YEVMIYE_DEFTER_AD " +
                " (LOGICALREF," +
                " DENETIM_LOGICALREF," +
                " YEVMIYE_DEFTER_AD," +
                " KULLANIM_DURUMU)" +
                " VALUES" +
                " (@logicalRef," +
                " @denetimLogicalRef," +
                " @yevmiyeDefteriAd," +
                " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", denetimYevmiyeDefteriAd.LOGICALREF);
                    command.Parameters.AddWithValue("@denetimLogicalRef", denetimYevmiyeDefteriAd.DENETIM_LOGICALREF);
                    command.Parameters.AddWithValue("@yevmiyeDefteriAd", denetimYevmiyeDefteriAd.YEVMIYE_DEFTER_AD);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void GuncelleDenetimDurum(string denetimId, string gecenSure)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query = " UPDATE DENETIM" +
                                  " SET " +
                                  " DURUM=@durum, " +
                                  " SURE=@sure " +
                                " WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@durum", 1);
                sqlCmd2.Parameters.AddWithValue("@sure", gecenSure);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", denetimId);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static RAPOR GetirRaporRaporIdIle(string logicalRef)
        {
            RAPOR rapor = new RAPOR();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT AD,LOGICALREF,KULLANIM_DURUMU FROM RAPOR " +
                                " WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                while (sqlDataReader1.Read())
                {
                    rapor.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    rapor.AD = sqlDataReader1["AD"].ToString();
                    rapor.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                }
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return rapor;
        }

        public static RAPOR GetirRaporRaporIdIleIndirmekIcin(string logicalRef)
        {
            RAPOR rapor = new RAPOR();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM RAPOR " +
                                " WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                while (sqlDataReader1.Read())
                {
                    rapor.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    rapor.AD = sqlDataReader1["AD"].ToString();
                    rapor.CONTENT = (byte[])sqlDataReader1["CONTENT"];
                    rapor.CONTENT_TYPE = sqlDataReader1["CONTENT_TYPE"].ToString();
                    rapor.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                }
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return rapor;
        }

        public static List<DENETIM_RAPOR> GetirDenetimRaporListDenetimIdIle(string logicalRef)
        {
            List<DENETIM_RAPOR> denetimRaporList = new List<DENETIM_RAPOR>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query2 = " SELECT * FROM DENETIM_RAPOR " +
                                " WHERE DENETIM_LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                while (sqlDataReader1.Read())
                {
                    DENETIM_RAPOR denetimRapor = new DENETIM_RAPOR();
                    denetimRapor.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    denetimRapor.DENETIM_LOGICALREF = sqlDataReader1["DENETIM_LOGICALREF"].ToString();
                    denetimRapor.RAPOR_LOGICALREF = sqlDataReader1["RAPOR_LOGICALREF"].ToString();
                    denetimRapor.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    denetimRaporList.Add(denetimRapor);
                }
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return denetimRaporList;
        }

        public static List<DENETIM_KURALLARI_KAPSAM> GetirDenetimKurallariKapsamListesiKapsamIdIle(string kapsamLogicalref)
        {
            List<DENETIM_KURALLARI_KAPSAM> kayitlidenetimKapsamList = new List<DENETIM_KURALLARI_KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = " SELECT * FROM DENETIM_KURALLARI_KAPSAM WHERE KAPSAM_LOGICALREF = @kapsamLogicalref";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kapsamLogicalref", kapsamLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_KURALLARI_KAPSAM kayitliDenetimKuraliKapsam = new DENETIM_KURALLARI_KAPSAM();

                    kayitliDenetimKuraliKapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliDenetimKuraliKapsam.KAPSAM_LOGICALREF = sqlDataReader1["KAPSAM_LOGICALREF"].ToString();
                    kayitliDenetimKuraliKapsam.DENETIM_KURALLARI_LOGICALREF = sqlDataReader1["DENETIM_KURALLARI_LOGICALREF"].ToString();
                    kayitliDenetimKuraliKapsam.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitlidenetimKapsamList.Add(kayitliDenetimKuraliKapsam);
                }
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kayitlidenetimKapsamList;
        }

        public static List<DENETIM_KURALLARI_KAPSAM> GetirDenetimKurallariKapsamListesiDenetimIdIle(string denetimLogicalref)
        {
            List<DENETIM_KURALLARI_KAPSAM> kayitlidenetimKapsamList = new List<DENETIM_KURALLARI_KAPSAM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = @"  SELECT 
                                    DISTINCT DENETIM_KURALLARI_KAPSAM.DENETIM_KURALLARI_LOGICALREF 
                                    FROM DENETIM_KURALLARI_KAPSAM
                                    INNER JOIN DENETIM_KURALLARI ON DENETIM_KURALLARI.LOGICALREF  = DENETIM_KURALLARI_KAPSAM.DENETIM_KURALLARI_LOGICALREF 
							                                    AND DENETIM_KURALLARI.KULLANIM_DURUMU = 1
                                    WHERE KAPSAM_LOGICALREF IN 
                                    (
                                    SELECT KAPSAM_LOGICALREF FROM DENETIM_KAPSAM
                                    WHERE DENETIM_LOGICALREF = @denetimLogicalref
                                    ) ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimLogicalref", denetimLogicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_KURALLARI_KAPSAM kayitliDenetimKuraliKapsam = new DENETIM_KURALLARI_KAPSAM();
                    kayitliDenetimKuraliKapsam.DENETIM_KURALLARI_LOGICALREF = sqlDataReader1["DENETIM_KURALLARI_LOGICALREF"].ToString();
                    kayitlidenetimKapsamList.Add(kayitliDenetimKuraliKapsam);
                }
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kayitlidenetimKapsamList;
        }

        public static void DoldurMizanTablolari(string yil)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                //önce varsa 4 tane mizan talosunu temizliyoruz...
                SqlCommand cmd1 = new SqlCommand("DELETE FROM MIZAN1;DELETE FROM MIZAN2;DELETE FROM MIZAN3;DELETE FROM MIZAN4;", sqlCon);
                cmd1.ExecuteNonQuery();


                //Mizan1 oluşturuluyor...
                string query1 = @" INSERT INTO MIZAN1
                                   (accountSubID
                                   ,debitSum
                                   ,creditSum
                                   ,fark)
                                    select  
                                    TABLO1.debitAccountSubID,
                                    TABLO1.debitSum, 
                                    isnull(TABLO2.creditSum,0) as creditSum,
                                    TABLO1.debitSum-isnull(TABLO2.creditSum,0) as fark 
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
                                    left join 
                                    (
                                    select  acs.accountSubID as debitAccountSubID, 
                                    SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
                                    from accountSub acs 
                                    inner join account ac on ac.account_Id = acs.account_Id 
                                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
                                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-03-31') 
                                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                    group by acs.accountSubID
                                    )TABLO2 on TABLO1.debitAccountSubID = TABLO2.debitAccountSubID
                                    where TABLO1.debitSum-isnull(TABLO2.creditSum,0) > 0
                                    order by TABLO1.debitAccountSubID ";
                query1 = query1.Replace("@yil", yil);

                SqlCommand cmd5 = new SqlCommand(query1, sqlCon);
                cmd5.CommandType = CommandType.Text;
                cmd5.CommandTimeout = 600;
                //cmd5.ExecuteNonQuery();

                Thread t1 = new Thread(() => cmd5.ExecuteNonQuery());
                t1.Start();

                //Mizan2 oluşturuluyor...
                string query2 = @" INSERT INTO MIZAN2
                                   (accountSubID
                                   ,debitSum
                                   ,creditSum
                                   ,fark)
                                    select  
                                    TABLO1.debitAccountSubID,
                                    TABLO1.debitSum, 
                                    isnull(TABLO2.creditSum,0) as creditSum,
                                    TABLO1.debitSum-isnull(TABLO2.creditSum,0) as fark 
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
                                    left join 
                                    (
                                    select  acs.accountSubID as debitAccountSubID, 
                                    SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
                                    from accountSub acs 
                                    inner join account ac on ac.account_Id = acs.account_Id 
                                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
                                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-06-30') 
                                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                    group by acs.accountSubID
                                    )TABLO2 on TABLO1.debitAccountSubID = TABLO2.debitAccountSubID
                                    where TABLO1.debitSum-isnull(TABLO2.creditSum,0) > 0
                                    order by TABLO1.debitAccountSubID ";

                query2 = query2.Replace("@yil", yil);

                SqlCommand cmd6 = new SqlCommand(query2, sqlCon);
                cmd6.CommandType = CommandType.Text;
                cmd6.CommandTimeout = 600;
                //cmd6.ExecuteNonQuery();

                Thread t2 = new Thread(() => cmd6.ExecuteNonQuery());
                t2.Start();

                //Mizan3 oluşturuluyor...
                string query3 = @" INSERT INTO MIZAN3
                                   (accountSubID
                                   ,debitSum
                                   ,creditSum
                                   ,fark)
                                    select  
                                    TABLO1.debitAccountSubID,
                                    TABLO1.debitSum, 
                                    isnull(TABLO2.creditSum,0) as creditSum,
                                    TABLO1.debitSum-isnull(TABLO2.creditSum,0) as fark 
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
                                    left join 
                                    (
                                    select  acs.accountSubID as debitAccountSubID, 
                                    SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
                                    from accountSub acs 
                                    inner join account ac on ac.account_Id = acs.account_Id 
                                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
                                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-09-30') 
                                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                    group by acs.accountSubID
                                    )TABLO2 on TABLO1.debitAccountSubID = TABLO2.debitAccountSubID
                                    where TABLO1.debitSum-isnull(TABLO2.creditSum,0) > 0
                                    order by TABLO1.debitAccountSubID ";

                query3 = query3.Replace("@yil", yil);

                SqlCommand cmd7 = new SqlCommand(query3, sqlCon);
                cmd7.CommandType = CommandType.Text;
                cmd7.CommandTimeout = 600;
                //cmd7.ExecuteNonQuery();

                Thread t3 = new Thread(() => cmd7.ExecuteNonQuery());
                t3.Start();

                //Mizan4 oluşturuluyor...
                //mizan4 ü hesap ederken son yevmiye fişini hesaba katmıyoruz(kapanış fişi)...
                string query4 = @" INSERT INTO MIZAN4
                                   (accountSubID
                                   ,debitSum
                                   ,creditSum
                                   ,fark)
                                    select  
                                    TABLO1.debitAccountSubID,
                                    TABLO1.debitSum, 
                                    isnull(TABLO2.creditSum,0) as creditSum,
                                    TABLO1.debitSum-isnull(TABLO2.creditSum,0) as fark 
                                    from
                                    (
                                    select  acs.accountSubID as debitAccountSubID,
                                    SUM(convert(DECIMAL(20,4),amt.amount_Text)) as debitSum 
                                    from accountSub acs 
                                    inner join account ac on ac.account_Id = acs.account_Id 
                                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'D' 
                                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-12-31')  
                                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                    inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id and enc.entryNumberCounter_Text< (select MAX(convert(int,enc.entryNumberCounter_Text))
																							                                    from entryHeader eh
																							                                    inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id)
                                    group by acs.accountSubID
                                    )TABLO1
                                    left join 
                                    (
                                    select  acs.accountSubID as debitAccountSubID, 
                                    SUM(convert(DECIMAL(16,4),amt.amount_Text)) as creditSum 
                                    from accountSub acs 
                                    inner join account ac on ac.account_Id = acs.account_Id 
                                    inner join entryDetail ed on ac.entryDetail_Id = ed.entryDetail_Id and ed.debitCreditCode = 'C' 
                                    inner join entryHeader eh on eh.entryHeader_Id = ed.entryHeader_Id and (eh.enteredDate >= '@yil-01-01' and eh.enteredDate <= '@yil-12-31') 
                                    inner join amount amt on amt.entryDetail_Id = ed.entryDetail_Id 
                                    inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id and enc.entryNumberCounter_Text< (select MAX(convert(int,enc.entryNumberCounter_Text))
																							                                    from entryHeader eh
																							                                    inner join entryNumberCounter enc on enc.entryHeader_Id = eh.entryHeader_Id)
                                    group by acs.accountSubID
                                    )TABLO2 on TABLO1.debitAccountSubID = TABLO2.debitAccountSubID
                                    where TABLO1.debitSum-isnull(TABLO2.creditSum,0) > 0
                                    order by TABLO1.debitAccountSubID ";

                query4 = query4.Replace("@yil", yil);

                SqlCommand cmd8 = new SqlCommand(query4, sqlCon);
                cmd8.CommandType = CommandType.Text;
                cmd8.CommandTimeout = 600;
                //cmd8.ExecuteNonQuery();

                Thread t4 = new Thread(() => cmd8.ExecuteNonQuery());
                t4.Start();

                while (t1.IsAlive || t2.IsAlive || t3.IsAlive || t4.IsAlive)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static CalisanKuralSonuc CalistirKural(string sql)
        {
            CalisanKuralSonuc calisanKuralSonuc = new CalisanKuralSonuc();
            string sonucTip = "";
            string entryDetail_IdList = "";
            string mizanList = "";


            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            int count = 0;

            try
            {
                string query1 = sql;

                SqlCommand sqlCmd1 = new SqlCommand(query1, sqlCon);
                sqlCmd1.CommandTimeout = 600;
                sqlCmd1.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd1.ExecuteReader();

                for (int i = 0; i < sqlDataReader1.FieldCount; i++)
                {
                    if (sqlDataReader1.GetName(i).Equals("entryDetail_Id"))
                    {
                        sonucTip = "entryDetail_Id";
                        break;
                    }
                    else if (sqlDataReader1.GetName(i).Equals("mizan"))
                    {
                        sonucTip = "mizan";
                        break;
                    }

                }

                while (sqlDataReader1.Read())
                {
                    if (sonucTip.Equals("entryDetail_Id"))
                    {
                        string entryDetail_Id = sqlDataReader1["entryDetail_Id"].ToString();
                        entryDetail_IdList = entryDetail_IdList + "," + entryDetail_Id + "";
                    }
                    else if (sonucTip.Equals("mizan"))
                    {
                        string mizan = sqlDataReader1["mizan"].ToString() + " - " + sqlDataReader1["toplam"].ToString();
                        mizanList = mizanList + "_" + mizan + "";
                    }

                }

                //Birinci sıradaki virgülü silmek için...
                if (!entryDetail_IdList.Equals(""))
                    entryDetail_IdList = entryDetail_IdList.Substring(1);

                if (!mizanList.Equals(""))
                    mizanList = mizanList.Substring(1);

                calisanKuralSonuc.entryDetail_IdList = entryDetail_IdList;
                calisanKuralSonuc.mizanList = mizanList;

            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return calisanKuralSonuc;
        }

        public static void EkleDenetimSonucList(List<DENETIM_SONUC> denetimSonucList)
        {
            String query = "INSERT INTO DENETIM_SONUC " +
                " (KURAL_KOD," +
                " MUSTERI_ACIKLAMA," +
                " MEVZUAT," +
                " DETAY," +
                " TIP)" +
                " VALUES" +
                " (@kuralKod," +
                " @musteriAciklama," +
                " @mevzuat," +
                " @detay," +
                " @tip)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                //önce varsa denetim sonuç tablosundaki kayıtları temizliyoruz...
                SqlCommand cmd1 = new SqlCommand("DELETE FROM DENETIM_SONUC;", sqlCon);
                cmd1.ExecuteNonQuery();

                foreach (DENETIM_SONUC denetimSonuc in denetimSonucList)
                {
                    using (SqlCommand command = new SqlCommand(query, sqlCon))
                    {
                        command.Parameters.AddWithValue("@kuralKod", denetimSonuc.KURAL_KOD);
                        command.Parameters.AddWithValue("@musteriAciklama", denetimSonuc.MUSTERI_ACIKLAMA);
                        command.Parameters.AddWithValue("@mevzuat", denetimSonuc.MEVZUAT);
                        command.Parameters.AddWithValue("@detay", denetimSonuc.DETAY);
                        command.Parameters.AddWithValue("@tip", denetimSonuc.TIP);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void TemizleDenetimSonucList(string sunucuAdi
                                                 , string veriTabaniAdi
                                                 , string kullaniciAdi
                                                 , string kullaniciSifre)
        {
            //SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNRSQLServer();

            SqlConnection sqlCon = new SqlConnection(
                "Server=" + sunucuAdi
                + ";Database=" + veriTabaniAdi
                + ";User ID=" + kullaniciAdi
                + ";Password=" + kullaniciSifre
                + "; Trusted_Connection=False");

            

            try
            {
                sqlCon.Open();
                //önce varsa denetim sonuç tablosundaki kayıtları temizliyoruz...
                SqlCommand cmd1 = new SqlCommand("DELETE FROM DENETIM_SONUC;DELETE FROM DENETIM_LOG;", sqlCon);
                cmd1.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void KaydetRapor(RAPOR rapor)
        {
            String query = "INSERT INTO RAPOR " +
                 " (LOGICALREF," +
                 " AD," +
                 " CONTENT," +
                 " CONTENT_TYPE," +
                 " KULLANIM_DURUMU)" +
                 " VALUES" +
                 " (@logicalref," +
                 " @ad," +
                 " @content," +
                 " @contentType," +
                 " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();

            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.CommandTimeout = 600;
                    command.Parameters.AddWithValue("@logicalref", rapor.LOGICALREF);
                    command.Parameters.AddWithValue("@ad", rapor.AD);
                    command.Parameters.AddWithValue("@content", rapor.CONTENT);
                    command.Parameters.AddWithValue("@contentType", rapor.CONTENT_TYPE);
                    command.Parameters.AddWithValue("@kullanimDurumu", rapor.KULLANIM_DURUMU);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void KaydetDenetimRapor(DENETIM_RAPOR denetimRapor)
        {
            String query = "INSERT INTO DENETIM_RAPOR " +
                 " (LOGICALREF," +
                 " RAPOR_LOGICALREF," +
                 " DENETIM_LOGICALREF," +
                 " KULLANIM_DURUMU)" +
                 " VALUES" +
                 " (@logicalref," +
                 " @raporLogicalref," +
                 " @denetimLogicalref," +
                 " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                Guid objYeni = Guid.NewGuid();

                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalref", denetimRapor.LOGICALREF);
                    command.Parameters.AddWithValue("@raporLogicalref", denetimRapor.RAPOR_LOGICALREF);
                    command.Parameters.AddWithValue("@denetimLogicalref", denetimRapor.DENETIM_LOGICALREF);
                    command.Parameters.AddWithValue("@kullanimDurumu", denetimRapor.KULLANIM_DURUMU);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static List<FIRMA> GetirKullaniciFirmaListKullaniciIdIle(string logicalRef)
        {
            List<FIRMA> kayitliFirmaList = new List<FIRMA>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = @"SELECT KF.FIRMA_LOGICALREF, 
                                    F.UNVAN,
                                    F.LOGO,
                                    F.VKNTCKN, 
                                    F.KULLANIM_DURUMU FROM  KULLANICI_FIRMA KF 
                                    INNER JOIN FIRMA F ON F.LOGICALREF=KF.FIRMA_LOGICALREF AND KF.KULLANIM_DURUMU=1
                                    WHERE KF.KULLANICI_LOGICALREF=@logicalRef
                                    AND F.KULLANIM_DURUMU=1
                                    ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                while (sqlDataReader1.Read())
                {
                    FIRMA kayitliFirma = new FIRMA();
                    if (Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]) == 1)
                    {
                        kayitliFirma.kullanimDurumuBool = true;
                    }
                    else
                    {
                        kayitliFirma.kullanimDurumuBool = false;
                    }
                    kayitliFirma.LOGICALREF = sqlDataReader1["FIRMA_LOGICALREF"].ToString();
                    kayitliFirma.UNVAN = sqlDataReader1["UNVAN"].ToString();
                    kayitliFirma.VKNTCKN = sqlDataReader1["VKNTCKN"].ToString();
                    kayitliFirma.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"].ToString());

                    if (sqlDataReader1["LOGO"].GetType() != typeof(DBNull))
                    {
                        kayitliFirma.LOGO = (byte[])sqlDataReader1["LOGO"];
                    }
                    else
                        kayitliFirma.LOGO = null;

                    if (kayitliFirma.LOGO != null)
                    {
                        string urunresimString = Convert.ToBase64String(kayitliFirma.LOGO, 0, kayitliFirma.LOGO.Length);
                        kayitliFirma.resimSrc = "data:image/jpeg;base64," + urunresimString;
                    }
                    else
                    {
                        kayitliFirma.resimSrc = "";
                    }
                    kayitliFirmaList.Add(kayitliFirma);
                }
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kayitliFirmaList;
        }

        public static void SilKullaniciFirmaKullaniciIdIle(string logicalRef)
        {

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " DELETE KULLANICI_FIRMA " +
                                " WHERE KULLANICI_LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                sqlCmd2.ExecuteNonQuery();

            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }

        }

        public static void KullaniciFirmaEkle(string firmaLogicalRef, string LOGICALREF, string kullanicLogicalRef)
        {

            String query = "INSERT INTO KULLANICI_FIRMA " +
                " (LOGICALREF," +
                " KULLANICI_LOGICALREF," +
                " KULLANIM_DURUMU," +
                " FIRMA_LOGICALREF)" +
                " VALUES" +
                " (@logicalRef," +
                " @kullaniciLogicalRef," +
                " @kullanimDurumu," +
                " @firmaLogicalRef)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {


                    command.Parameters.AddWithValue("@logicalRef", LOGICALREF);
                    command.Parameters.AddWithValue("@kullaniciLogicalRef", kullanicLogicalRef);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.Parameters.AddWithValue("@firmaLogicalRef", firmaLogicalRef);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void GuncelleKullaniciSifreEmailIle(string yeniSifre, string email)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query = " UPDATE KULLANICI" +
                                  " SET " +
                                  " SIFRE = @sifre" +
                                  " WHERE EMAIL=@email ";
                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                String hashSifre = Dogrulama.Encrypt(yeniSifre, parametreler.Dogrulama.saltValue);
                sqlCmd2.Parameters.AddWithValue("@sifre", hashSifre);
                sqlCmd2.Parameters.AddWithValue("@email", email);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
        }

        public static void GuncelleKullaniciFirmaKullanimDurumu(string firmaLogicalRef, string kulllaniciLogicalRef, int kullanimDurumu)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query = " UPDATE KULLANICI_FIRMA" +
                                  " SET " +
                                  " KULLANIM_DURUMU=@kullanimDurumu" +
                                " WHERE KULLANICI_LOGICALREF=@kulllaniciLogicalRef " +
                                " AND FIRMA_LOGICALREF=@firmaLogicalRef";
                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@firmaLogicalRef", firmaLogicalRef);
                sqlCmd2.Parameters.AddWithValue("@kulllaniciLogicalRef", kulllaniciLogicalRef);
                sqlCmd2.Parameters.AddWithValue("@kullanimDurumu", kullanimDurumu);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        
        public static List<DONEM_AY> GetirDonemAyListesiTurIle(int tur)//1 Donem 2 Ay
        {
            DONEM_AY kayit = new DONEM_AY();

            List<DONEM_AY> kayitList = new List<DONEM_AY>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " SELECT * FROM DONEM_AY WHERE TUR=@tur";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@tur", tur);
                //sqlCmd2.Parameters.AddWithValue("@SIFRE", Dogrulama.GenerateSaltedHash(Encoding.UTF8.GetBytes(sifre), Encoding.UTF8.GetBytes("connector")));

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                while (sqlDataReader1.Read())
                {
                    kayit = new DONEM_AY();

                    kayit.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayit.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                    kayit.TUR = Convert.ToInt32(sqlDataReader1["TUR"].ToString());
                    kayit.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitList.Add(kayit);
                }
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kayitList;
        }
        public static string GetirFirmaUnaviYevmiyeDefterinden(string sunucuAdi
                                            , string veriTabaniAdi
                                            , string kullaniciAdi
                                            , string kullaniciSifre)//1 Donem 2 Ay
        {
            string kayit = "";

            SqlConnection sqlCon = new SqlConnection(
                                                    "Server=" + sunucuAdi
                                                    + ";Database=" + veriTabaniAdi
                                                    + ";User ID=" + kullaniciAdi
                                                    + ";Password=" + kullaniciSifre
                                                    + "; Trusted_Connection=False"  
                                                    );
            try
            {
                string query2 = " SELECT TOP 1 organizationIdentifier FROM organizationIdentifiers";
                sqlCon.Open();
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                while (sqlDataReader1.Read())
                {
                    kayit = sqlDataReader1["organizationIdentifier"].ToString();
                }
            }
            catch (Exception exp)
            {

                sqlCon.Close();
                throw;
            }
            finally
            {
                sqlCon.Close();

            }
            return kayit;
        }

        public static string GetirYilBilgisiYevmiyeDefterinden(string sunucuAdi
                                      , string veriTabaniAdi
                                      , string kullaniciAdi
                                      , string kullaniciSifre)//1 Donem 2 Ay
        {
            string kayit = "";

            SqlConnection sqlCon = new SqlConnection(
                                                    "Server=" + sunucuAdi
                                                    + ";Database=" + veriTabaniAdi
                                                    + ";User ID=" + kullaniciAdi
                                                    + ";Password=" + kullaniciSifre
                                                    + "; Trusted_Connection=False"
                                                    );
            try
            {
                string query2 = "SELECT TOP 1 LEFT(fiscalYearStart, 4) AS 'YIL' FROM entityInformation";
                sqlCon.Open();
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                while (sqlDataReader1.Read())
                {
                    kayit = sqlDataReader1["YIL"].ToString();
                }
            }
            catch (Exception exp)
            {

                sqlCon.Close();
                throw;
            }
            finally
            {
                sqlCon.Close();

            }
            return kayit;
        }

        public static KONTOR GetirKontorIdIle(string logicalRef)
        {
            KONTOR kayitliKontor = new KONTOR();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = " SELECT * FROM KONTOR " +
                                " WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kayitliKontor.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kayitliKontor.KONTOR_ADET = Convert.ToInt32(sqlDataReader1["KONTOR_ADET"].ToString());
                kayitliKontor.BIRIM_FIYATI = sqlDataReader1["BIRIM_FIYATI"].ToString();
                kayitliKontor.PAKET_FIYATI = sqlDataReader1["PAKET_FIYATI"].ToString();
                kayitliKontor.INDIRIM_ORANI = Convert.ToInt32(sqlDataReader1["INDIRIM_ORANI"].ToString());
                kayitliKontor.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                kayitliKontor.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
            }
            catch (Exception exp)
            {

            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kayitliKontor;
        }

        public static void GuncelleKontor(KONTOR kontor)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query = " UPDATE KONTOR" +
                                  " SET " +
                                  " KONTOR_ADET=@kontor," +
                                  " PAKET_FIYATI=@paketFiyat," +
                                  " BIRIM_FIYATI=@birimFiyat," +
                                  " INDIRIM_ORANI=@indirimOrani," +
                                  " ACIKLAMA=@aciklama," +
                                  " KULLANIM_DURUMU=@kullanimDurumu" +
                                  " WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kontor", kontor.KONTOR_ADET);
                sqlCmd2.Parameters.AddWithValue("@paketFiyat", kontor.PAKET_FIYATI);
                sqlCmd2.Parameters.AddWithValue("@birimFiyat", kontor.BIRIM_FIYATI);
                sqlCmd2.Parameters.AddWithValue("@kullanimDurumu", kontor.KULLANIM_DURUMU);
                sqlCmd2.Parameters.AddWithValue("@indirimOrani", kontor.INDIRIM_ORANI);
                sqlCmd2.Parameters.AddWithValue("@aciklama", kontor.ACIKLAMA);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", kontor.LOGICALREF);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
        }

        public static void KontorEkle(KONTOR kontor)
        {

            String query = "INSERT INTO KONTOR " +
                " (LOGICALREF," +
                " KONTOR_ADET," +
                " PAKET_FIYATI," +
                " BIRIM_FIYATI," +
                " INDIRIM_ORANI," +
                " ACIKLAMA," +
                " KULLANIM_DURUMU)" +
                " VALUES" +
                " (@logicalRef," +
                " @kontor," +
                " @paketFiyat," +
                " @birimFiyat," +
                " @indirimOrani," +
                " @aciklama," +
                " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@kontor", kontor.KONTOR_ADET);
                    command.Parameters.AddWithValue("@paketFiyat", kontor.PAKET_FIYATI);
                    command.Parameters.AddWithValue("@birimFiyat", kontor.BIRIM_FIYATI);
                    command.Parameters.AddWithValue("@kullanimDurumu", 0);
                    command.Parameters.AddWithValue("@indirimOrani", kontor.INDIRIM_ORANI);
                    command.Parameters.AddWithValue("@aciklama", kontor.ACIKLAMA);
                    command.Parameters.AddWithValue("@logicalRef", kontor.LOGICALREF);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        
        public static void KontorHareketEkle(KONTOR_HAREKET kontor)
        {
            String query = "INSERT INTO KONTOR_HAREKET " +
                    " (LOGICALREF," +
                    " KULLANICI_LOGICALREF," +
                    " ISLEM_TIPI," +
                    " KONTOR_MIKTARI," +
                    " ISLEM_ACIKLAMA," +
                    " ISLEM_TARIHI," +
                    " KULLANIM_DURUMU)" +
                    " VALUES" +
                    " (@logicalRef," +
                    " @kullaniciLogicalref," +
                    " @islemTipi," +
                    " @kontorMiktar," +
                    " @islemAciklama," +
                    " @islemTarihi," +
                    " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", kontor.LOGICALREF);
                    command.Parameters.AddWithValue("@kullaniciLogicalref", kontor.KULLANICI_LOGICALREF);
                    command.Parameters.AddWithValue("@islemTipi", kontor.ISLEM_TIPI);
                    command.Parameters.AddWithValue("@kontorMiktar", kontor.KONTOR_MIKTARI);
                    command.Parameters.AddWithValue("@islemAciklama", kontor.ISLEM_ACIKLAMA);
                    command.Parameters.AddWithValue("@islemTarihi", DateTime.Now);
                    command.Parameters.AddWithValue("@kullanimDurumu", kontor.KULLANIM_DURUMU);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        public static void BildirimEkle(BILDIRIM bildirim)
        {
            String query = "INSERT INTO BILDIRIM " +
                    " (LOGICALREF," +
                    " KULLANICI_LOGICALREF," +
                    " ACIKLAMA_KISA," +
                    " ACIKLAMA_UZUN," +
                    " DURUMU," +
                    " TARIH," +
                    " KULLANIM_DURUMU)" +
                    " VALUES" +
                    " (@logicalRef," +
                    " @kullaniciLogicalref," +
                    " @aciklamaKisa," +
                    " @aciklamaUzun," +
                    " @durumu," +
                    " @tarih," +
                    " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", bildirim.LOGICALREF);
                    command.Parameters.AddWithValue("@kullaniciLogicalref", bildirim.KULLANICI_LOGICALREF);
                    command.Parameters.AddWithValue("@aciklamaKisa", bildirim.ACIKLAMA_KISA);
                    command.Parameters.AddWithValue("@aciklamaUzun", bildirim.ACIKLAMA_UZUN);
                    command.Parameters.AddWithValue("@durumu", bildirim.DURUMU);
                    command.Parameters.AddWithValue("@tarih", DateTime.Now);
                    command.Parameters.AddWithValue("@kullanimDurumu", bildirim.KULLANIM_DURUMU);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }
        public static List<KONTOR_HAREKET> GetirKontorHareketListesi(string kullaniciLogicalref)
        {
            KONTOR_HAREKET kayitliKontor = new KONTOR_HAREKET();
            List<KONTOR_HAREKET> kayitliKontorList = new List<KONTOR_HAREKET>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " select * from KONTOR_HAREKET WHERE KULLANICI_LOGICALREF=@kullaniciLogicalref";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kullaniciLogicalref", kullaniciLogicalref);

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                while (sqlDataReader1.Read())
                {
                    kayitliKontor = new KONTOR_HAREKET();

                    kayitliKontor.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKontor.KONTOR_MIKTARI = Convert.ToInt32(sqlDataReader1["KONTOR_MIKTARI"].ToString());
                    kayitliKontor.ISLEM_ACIKLAMA = sqlDataReader1["ISLEM_ACIKLAMA"].ToString();
                    kayitliKontor.ISLEM_TARIHI = Convert.ToDateTime(sqlDataReader1["ISLEM_TARIHI"].ToString());
                    kayitliKontor.ISLEM_TIPI = Convert.ToInt32(sqlDataReader1["ISLEM_TIPI"]);
                    if (kayitliKontor.ISLEM_TIPI == 0)
                        kayitliKontor.tipAciklama = "SATIN ALMA";
                    else
                        kayitliKontor.tipAciklama = "DENETİM";
                    kayitliKontorList.Add(kayitliKontor);
                }
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kayitliKontorList;
        }

        public static List<BILDIRIM> GetirBildirimListesi(string kullaniciLogicalref)
        {
         
            List<BILDIRIM> kayitliBildirimList = new List<BILDIRIM>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                //Eğer kullanıcı varsa veri tabanından kullanıcı bilgileri getirilir.
                string query2 = " select * from BILDIRIM WHERE KULLANICI_LOGICALREF=@kullaniciLogicalref AND KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@kullaniciLogicalref", kullaniciLogicalref);

                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                //Bir tane data gelmesini beklediğimiz için sadece bir tane okuma yapıyoruz.
                while (sqlDataReader1.Read())
                {
                    BILDIRIM kayitliBildirim = new BILDIRIM();

                    kayitliBildirim.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliBildirim.KULLANICI_LOGICALREF = sqlDataReader1["KULLANICI_LOGICALREF"].ToString();
                    kayitliBildirim.ACIKLAMA_KISA = sqlDataReader1["ACIKLAMA_KISA"].ToString();
                    kayitliBildirim.ACIKLAMA_UZUN = sqlDataReader1["ACIKLAMA_UZUN"].ToString();
                    kayitliBildirim.TARIH = Convert.ToDateTime(sqlDataReader1["TARIH"].ToString());
                    kayitliBildirim.DURUMU = Convert.ToInt32(sqlDataReader1["DURUMU"]);
                    kayitliBildirim.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitliBildirimList.Add(kayitliBildirim);
                }
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kayitliBildirimList;
        }

        public static void WriteLog(string denetimId,string mesaj, string kod)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServerStatic();

            try
            {
                String query = " INSERT INTO DENETIM_LOG (DENETIM_ID,KURAL_KOD, HATA)" +
                                " VALUES (@denetimId,@kuralKod, @hata)";

                
                //SqlConnection sqlCon = new SqlConnection("Server=" + Sabitler.sunucuAdiR +
                //                                         ";Database=" + Sabitler.veriTabaniAdiR +
                //                                         ";User ID=" + Sabitler.kullaniciAdiR +
                //                                         ";Password=" + Sabitler.kullaniciSifreR +
                //                                         "; Trusted_Connection=False");

               
                sqlCon.Open();

                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@kuralKod", kod);
                    command.Parameters.AddWithValue("@hata", mesaj);
                    command.Parameters.AddWithValue("@denetimId", denetimId);
                    command.ExecuteNonQuery();
                }

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            catch (Exception exception)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void CalismayanKuralEkle(string denetimId,string kuralKod, string hata)
        {

            String query = "INSERT INTO DENETIM_CALISMAYAN_KURAL " +
                " (DENETIM_ID," +
                " KURAL_KOD," +
                " HATA)" +
                " VALUES" +
                " (@denetimId," +
                " @kuralKod," +
                " @hata)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServerStatic();
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@denetimId", denetimId);
                    command.Parameters.AddWithValue("@kuralKod", kuralKod);
                    command.Parameters.AddWithValue("@hata", hata);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                //WriteLog(e.Message.ToString(), "CalismayanKuralEkle");
                throw ;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static List<DENETIM_KURALLARI> GetirDenetimKurallariListesiCalismayanlar(string denetimID)
        {
            List<DENETIM_KURALLARI> kayitlidenetimKurallariList = new List<DENETIM_KURALLARI>();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = @" SELECT dk.* 
                                   FROM DENETIM_CALISMAYAN_KURAL dck
                                   inner join DENETIM_KURALLARI dk on dk.KOD = dck.KURAL_KOD AND dck.DENETIM_ID=@denetimID ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimID", denetimID);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();

                while (sqlDataReader1.Read())
                {
                    DENETIM_KURALLARI kayitliKural = new DENETIM_KURALLARI();
                    kayitliKural.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                    kayitliKural.KOD = sqlDataReader1["KOD"].ToString();
                    kayitliKural.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
                    kayitliKural.MUSTERI_ACIKLAMA = sqlDataReader1["MUSTERI_ACIKLAMA"].ToString();
                    kayitliKural.MUSTERI_ACIKLAMA2 = sqlDataReader1["MUSTERI_ACIKLAMA2"].ToString();
                    kayitliKural.MEVZUAT = sqlDataReader1["MEVZUAT"].ToString();
                    kayitliKural.SQL_IFADE = sqlDataReader1["SQL_IFADE"].ToString();
                    kayitliKural.KULLANIM_DURUMU = Convert.ToInt32(sqlDataReader1["KULLANIM_DURUMU"]);
                    kayitlidenetimKurallariList.Add(kayitliKural);
                }
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return kayitlidenetimKurallariList;
        }

        public static void SilDenetimKuralCalismayanlarDenetimIdIle(string denetimId)
        {

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServerStatic();

            try
            {
                string query2 = " DELETE DENETIM_CALISMAYAN_KURAL WHERE DENETIM_ID=@denetimId ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@denetimId",denetimId);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void GuncelleDenetimDurumu(string denetimId, int durum)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {
                string query = " UPDATE DENETIM" +
                                  " SET " +
                                  " DURUM=@durum " +
                                " WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@durum", durum);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", denetimId);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static int GetirDevamEdenDenetimSayisi()
        {
            int devamEdenDenetimSayisi=0;
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServerStatic();
            try
            {

                string query2 = @"select COUNT(*) AS SAYI from DENETIM WHERE DURUM!=0 AND DURUM!=1"; //DENETİM DURUMU 1 İSE DENETİM TAMAMLANMIŞ DEMEKTİR. 0 İSE YENİ TANIMLANMIŞ DAHA BAŞLATMAMIŞ DEMEKTİR.
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                while (sqlDataReader1.Read())
                {
                    devamEdenDenetimSayisi = Convert.ToInt32(sqlDataReader1["SAYI"].ToString());
                }
            }
            catch (Exception exp)
            {

            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
            return devamEdenDenetimSayisi;
        }

        public static void AltKapsamEkle(ALT_KAPSAM altKapsam)
        {

            String query = "INSERT INTO ALT_KAPSAM " +
                " (LOGICALREF," +
                " AD," +
                " KISA_AD," +
                " KONTOR," +
                " ACIKLAMA," +
                " KULLANIM_DURUMU)" +
                " VALUES" +
                " (@logicalRef," +
                " @ad," +
                " @kisaAd," +
                " @kontor," +
                " @aciklama," +
                " @kullanimDurumu)";

            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                using (SqlCommand command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("@logicalRef", altKapsam.LOGICALREF);
                    command.Parameters.AddWithValue("@ad", altKapsam.AD);
                    command.Parameters.AddWithValue("@kisaAd", altKapsam.KISA_AD);
                    command.Parameters.AddWithValue("@kontor", altKapsam.KONTOR);
                    command.Parameters.AddWithValue("@aciklama", altKapsam.ACIKLAMA);
                    command.Parameters.AddWithValue("@kullanimDurumu", 1);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
            }
        }

        public static void GuncelleAltKapsam(ALT_KAPSAM altKapsam)
        {
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query = " UPDATE ALT_KAPSAM" +
                                  " SET " +
                                  " AD=@ad," +
                                  " KISA_AD=@kisaAd," +
                                  " KONTOR=@kontor," +
                                  " ACIKLAMA=@aciklama," +
                                  " KULLANIM_DURUMU=@kullanimDurumu" +
                                  " WHERE LOGICALREF=@logicalRef ";
                SqlCommand sqlCmd2 = new SqlCommand(query, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@ad", altKapsam.AD);
                sqlCmd2.Parameters.AddWithValue("@kisaAd", altKapsam.KISA_AD);
                sqlCmd2.Parameters.AddWithValue("@kontor", altKapsam.KONTOR);
                sqlCmd2.Parameters.AddWithValue("@kullanimDurumu", altKapsam.KULLANIM_DURUMU);
                sqlCmd2.Parameters.AddWithValue("@aciklama", altKapsam.ACIKLAMA);
                sqlCmd2.Parameters.AddWithValue("@logicalRef", altKapsam.LOGICALREF);
                sqlCmd2.ExecuteNonQuery();
            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
        }

        public static KAPSAM GetirDenetimKapsamDenetimIdIle(string logicalRef)
        {
            KAPSAM kapsam = new KAPSAM();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = @"SELECT K.LOGICALREF, K.AD,K.KISA_AD FROM KAPSAM K
                                INNER JOIN DENETIM_KAPSAM DK ON DK.KAPSAM_LOGICALREF=K.LOGICALREF 
                                AND DK.DENETIM_LOGICALREF=@logicalRef
                                WHERE K.KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                kapsam.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                kapsam.AD = sqlDataReader1["AD"].ToString();
                kapsam.KISA_AD = sqlDataReader1["KISA_AD"].ToString();
            }
            catch (Exception exp)
            {

            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kapsam;
        }
        
        public static DONEM_AY GetirDonemAyDenetimIdIle(string logicalRef)
        {
            DONEM_AY donemAy = new DONEM_AY();
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = @"SELECT DA.LOGICALREF,DA.ACIKLAMA FROM DONEM_AY DA 
                                    INNER JOIN DENETIM D ON D.DONEM_AY_LOGICALREF=DA.LOGICALREF 
                                    AND D.LOGICALREF=@logicalRef
                                    WHERE DA.KULLANIM_DURUMU=1";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalRef", logicalRef);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                sqlDataReader1.Read();
                donemAy.LOGICALREF = sqlDataReader1["LOGICALREF"].ToString();
                donemAy.ACIKLAMA = sqlDataReader1["ACIKLAMA"].ToString();
            }
            catch (Exception exp)
            {

            }

            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return donemAy;
        }
        
        public static string GetirAltKapsamListesidDenetimIdIle(string logicalref)
        {
            string kapsamList = "";
            SqlConnection sqlCon = BaglantiSinifi.BaglantiAcDKNSQLServer();
            try
            {

                string query2 = @"SELECT  AK.AD FROM ALT_KAPSAM AK
                                    INNER JOIN DENETIM_ALT_KAPSAM DAK ON DAK.ALT_KAPSAM_LOGICALREF=AK.LOGICALREF 
                                    AND DAK.DENETIM_LOGICALREF=@logicalref
                                    WHERE AK.KULLANIM_DURUMU=1 
                                   ";
                SqlCommand sqlCmd2 = new SqlCommand(query2, sqlCon);
                sqlCmd2.CommandType = CommandType.Text;
                sqlCmd2.Parameters.AddWithValue("@logicalref", logicalref);
                SqlDataReader sqlDataReader1 = sqlCmd2.ExecuteReader();
                while (sqlDataReader1.Read())
                {

                    kapsamList += sqlDataReader1["AD"].ToString()+", ";
                }
                if (!string.IsNullOrEmpty(kapsamList))
                    kapsamList = kapsamList.Remove(kapsamList.Length - 2, 2);//Hem sondaki virgülü hem de boşluğu kaldırıyorum

            }
            catch (Exception exp)
            {

                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);
                throw;
            }
            finally
            {
                BaglantiSinifi.BaglantiKapatDKNSQLServer(sqlCon);

            }
            return kapsamList;
        }
    }

}