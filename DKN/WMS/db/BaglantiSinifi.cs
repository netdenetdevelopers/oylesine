using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using DKN.Models;

namespace DKN.db
{
    public class BaglantiSinifi
    {
        //DKN veri tabanı bilgileri
        public static string DKNServer;
        public static string DKNDatabase;
        public static string DKNPort;
        public static string DKNUser;
        public static string DKNPassword;

        //ERP veri tabanı bilgileri
        public static string ERPServer;
        public static string ERPDatabase;
        public static string ERPPort;
        public static string ERPUser;
        public static string ERPPassword;

        //LOGO Rest servis bilgileri
        public static string RestServisClientId;
        public static string RestServisClientSecret;
        public static string RestServisIp;
        public static string RestServisPort;
        public static string RestServisUser;
        public static string RestServisPassword;
        public static string RestServisFirmNo;
        public static string accessToken;
        public static string refreshToken;

        public static string connectionStringDKNSQLServer = DKNBaglantiAyarlariniOkuSQLServer();


        public static string DKNBaglantiAyarlariniOkuSQLServer()
        {
            string searchFor = "<Server>";
            string searchFor1 = "<Database>";
            string searchFor2 = "<User>";
            string searchFor3 = "<Password>";

            try
            {
                var path = System.Web.HttpContext.Current.Server.MapPath("/ayarlar/dbConfig.xml");

                string[] lines = File.ReadAllLines(path);
                for (int i = lines.Length - 1; i >= 0; i--)
                {
                    if (lines[i].Contains(searchFor))
                    {
                        DKNServer = RemoveBetween(lines[i], '<', '>');
                    }
                    if (lines[i].Contains(searchFor1))
                    {
                        DKNDatabase = RemoveBetween(lines[i], '<', '>');
                    }
                    if (lines[i].Contains(searchFor2))
                    {
                        DKNUser = RemoveBetween(lines[i], '<', '>');
                    }
                    if (lines[i].Contains(searchFor3))
                    {
                        DKNPassword = RemoveBetween(lines[i], '<', '>');
                    }
                }

                return "Server=" + DKNServer + ";Database=" + DKNDatabase + ";User ID=" + DKNUser + ";Password=" + DKNPassword + "; Trusted_Connection=False";
            }
            catch (Exception exc)
            {
                return null;
                throw;
            }
        }

        public static SqlConnection BaglantiAcDKNSQLServer()
        {

            SqlConnection sqlCon = new SqlConnection(DKNBaglantiAyarlariniOkuSQLServer());
            try
            {
                if (sqlCon.State == ConnectionState.Closed)
                {
                    sqlCon.Open();
                }
                return sqlCon;
            }
            catch (Exception exc)
            {
                return null;
                throw;
            }
        }


        public static SqlConnection BaglantiAcDKNSQLServerStatic()
        {

            SqlConnection sqlCon = new SqlConnection(
                                      "Server=" + parametreler.Sabitler.sunucuAdi
                                      + ";Database=" + parametreler.Sabitler.veriTabaniAdi
                                      + ";User ID=" + parametreler.Sabitler.kullaniciAdi
                                      + ";Password=" + parametreler.Sabitler.kullaniciSifre
                                      + "; Trusted_Connection=False");
            try
            {
                if (sqlCon.State == ConnectionState.Closed)
                {
                    sqlCon.Open();
                }
                return sqlCon;
            }
            catch (Exception exc)
            {
                return null;
                throw;
            }
        }

        public static SqlConnection BaglantiAcDKNRSQLServer()
        {

            SqlConnection sqlCon = new SqlConnection(
                "Server=" + parametreler.Sabitler.sunucuAdiR  
                + ";Database=" + parametreler.Sabitler.veriTabaniAdiR 
                + ";User ID=" + parametreler.Sabitler.kullaniciAdiR 
                + ";Password=" + parametreler.Sabitler.kullaniciSifreR 
                + "; Trusted_Connection=False");

            try
            {
                if (sqlCon.State == ConnectionState.Closed)
                {
                    sqlCon.Open();
                }
                return sqlCon;
            }
            catch (Exception exc)
            {

                return null;
                throw;
            }
        }

        public static SqlConnection BaglantiDondurDKNWSQLServer()
        {
            SqlConnection sqlCon = new SqlConnection(
                "Server=" + parametreler.Sabitler.sunucuAdiW1
                + ";Database=" + parametreler.Sabitler.veriTabaniAdiW1
                + ";User ID=" + parametreler.Sabitler.kullaniciAdiW1
                + ";Password=" + parametreler.Sabitler.kullaniciSifreW1);
                //+ "; Trusted_Connection=False");

            try
            {
                //if (sqlCon.State == ConnectionState.Closed)
                //{
                //    sqlCon.Open();
                //}
                return sqlCon;
            }
            catch (Exception exc)
            {
                return null;
                throw;
            }
        }

        public static void BaglantiKapatDKNSQLServer(SqlConnection sqlCon)
        {
            try
            {
                //Varsa veri tabanı bağlantısı kapatılır.
                if (sqlCon.State == ConnectionState.Open)
                {
                    sqlCon.Close();
                }
            }
            catch (Exception exc)
            {
                throw;
            }
        }

        public static void UyandirSQLServer()
        {
            try
            {
                SqlConnection sqlConTest = BaglantiDondurDKNWSQLServer();
                sqlConTest.Open();
                sqlConTest.Close();
            }
            catch (Exception exc)
            {
            }
        }

        public static string RemoveBetween(string s, char begin, char end)
        {
            Regex regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
            return regex.Replace(s, string.Empty);
        }

     


    }
}
