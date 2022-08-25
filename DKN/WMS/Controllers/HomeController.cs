using DKN.db;
using DKN.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace DKN.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult HomeIndex()
        {
           // DosyaBilgileri dosyaBilgileri = new DosyaBilgileri();
           // dosyaBilgileri.dosyaBilgileriList = new List<DosyaBilgileri>();

           // DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/yevmiyeler"));
           // FileInfo[] files = di.GetFiles("*.xml");

           //// IEnumerable<string> yevmiyeList = Directory.GetFiles(Server.MapPath("~/yevmiyeler"), "*.xml");
           // foreach (var item in files)
           // {
           //     DosyaBilgileri dosyaBilgileriTemp = new DosyaBilgileri();
           //     dosyaBilgileriTemp.dosyaAdi = item.Name;
           //     dosyaBilgileriTemp.dosyaId = item.Name;
           //     dosyaBilgileri.dosyaBilgileriList.Add(dosyaBilgileriTemp);
           // }
            //return View(dosyaBilgileri);
            return View();
        }

        [HttpPost]
        public ActionResult HomeIndex(HttpPostedFileBase file)
        {
            //if (file != null && file.ContentLength > 0)
            //    try
            //    {
            //        string path = Path.Combine(Server.MapPath("~/yevmiyeler"),
            //                                   Path.GetFileName(file.FileName));
            //        file.SaveAs(path);
            //        TempData["mesaj"] = new Message() { CssClassName = "alert-info", mesajBaslik = "Bilgi Mesajı!", mesajAciklama = "Yevmiye Dosyası Yükleme Başarılı." };

            //    }
            //    catch (Exception ex)
            //    {
            //        TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = "Veri Getirilirken Hata Oluştu." };

            //    }
            //else
            //{
            //    TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = " Dosyası Yükleme İşlemi Sırasında Hata Oluştu." };

            //}
            //DosyaBilgileri dosyaBilgileri = new DosyaBilgileri();
            //dosyaBilgileri.dosyaBilgileriList = new List<DosyaBilgileri>();

            //DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/yevmiyeler"));
            //FileInfo[] files = di.GetFiles("*.xml");

            //// IEnumerable<string> yevmiyeList = Directory.GetFiles(Server.MapPath("~/yevmiyeler"), "*.xml");
            //foreach (var item in files)
            //{
            //    DosyaBilgileri dosyaBilgileriTemp = new DosyaBilgileri();
            //    dosyaBilgileriTemp.dosyaAdi = item.Name;
            //    dosyaBilgileriTemp.dosyaId = item.Name;
            //    dosyaBilgileri.dosyaBilgileriList.Add(dosyaBilgileriTemp);
            //}


            //return View(dosyaBilgileri);
            return View();
        }


        [Authorize]
        [HttpPost]
        public ActionResult DosyaIceriginiGoster(DosyaBilgileri dosyaBilgileri)
        {
            try {
                string path = Server.MapPath("~/yevmiyeler");
               
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(path+ "//yevmiye.xslt");

                // Execute the transform and output the results to a file.
                var result = dosyaBilgileri.dosyaId.Split('.')[0];
                xslt.Transform(path+"//"+ dosyaBilgileri.dosyaId, path + "//"+result+".html");
                Session["htmlDosyaAdi"] = result;

                #region
                /*
                //herbir yevmiye xml'ini listeye atıyorum...
                List<XmlNode> liste = new List<XmlNode>(); 

                DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/yevmiyeler"));
                FileInfo[] files = di.GetFiles("*.xml");

                //Bütün xmlLler bu xml'de birleşiyor
                XElement xElem3 = new XElement("root");

                foreach (FileInfo file in files)
                {
                    XElement xElement = XElement.Load(path + "//" + file.Name);

                    xElem3.Add(xElement);

                    //xml'in sadece accountingEntries kısmını alabilmek için
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(XElement.Load(path + "//" + file.Name).ToString().Replace("gl-cor:", "")
                                             .Replace("gl-bus:", "")
                                             .Replace("contextRef=\"journal_context\"", "")
                                             .Replace("xmlns:gl-bus=\"http://www.xbrl.org/int/gl/bus/2006-10-25\"", "")
                                             .Replace("xmlns:gl-cor=\"http://www.xbrl.org/int/gl/cor/2006-10-25\"", "")
                                             );
                    XmlNode root = xmlDoc.DocumentElement;
                    XmlNode accountingEntries = root.FirstChild.LastChild;

                    liste.Add(accountingEntries);


                    //dataset olarak okuma örneği...
                    DataSet ds = new DataSet();
                    ds.ReadXml(path + "//" + file.Name);
                }


                
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Tüm dosyaların birleşmiş halini yapabilmek için...
                XmlDocument xmlDoc1 = new XmlDocument();
                xmlDoc1.LoadXml(xElem3.ToString().Replace("edefter:", "")
                                         .Replace("xbrli:", "")
                                         .Replace("gl-cor:", "")
                                         .Replace("gl-bus:", "")                                         
                                         .Replace("contextRef=\"journal_context\"", "")
                                         .Replace("xmlns:gl-bus=\"http://www.xbrl.org/int/gl/bus/2006-10-25\"", "")
                                         .Replace("xmlns:gl-cor=\"http://www.xbrl.org/int/gl/cor/2006-10-25\"", "")
                                         );

                XmlNode root1 = xmlDoc1.DocumentElement;
                XmlNodeList results = root1.SelectNodes("descendant::defter[xbrl/accountingEntries/entryHeader[entryDetail[account/accountSub/accountSubDescription='HALKBANK TAHTAKALE TL HESABI']]]");
                */
                #endregion


                #region

                

                DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/yevmiyeler"));
                FileInfo[] files = di.GetFiles("*.xml");

                XElement xElem3 = new XElement("root");
                XElement xElem4 = new XElement("root");

                foreach (FileInfo file in files)
                {
                    XElement xElement = XElement.Load(path + "//" + file.Name);
                    xElem3.Add(xElement);
                }
                
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xElem3.ToString().Replace("edefter:", "")
                                        .Replace("xbrli:", "")
                                        .Replace("gl-cor:", "")
                                        .Replace("gl-bus:", "")
                                        .Replace("contextRef=\"journal_context\"", "")
                                        .Replace("xmlns:gl-bus=\"http://www.xbrl.org/int/gl/bus/2006-10-25\"", "")
                                        .Replace("xmlns:gl-cor=\"http://www.xbrl.org/int/gl/cor/2006-10-25\"", "")
                                        );
                XmlNode root = xmlDoc.DocumentElement;
                //XmlNode accountingEntries = root.FirstChild;


                
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
                //ds.ReadXml(path + "//" + file.Name);



                for (int i = 0; i< ds.Tables.Count; i++)
                {
                    SqlConnection conn = BaglantiSinifi.BaglantiAcDKNSQLServer();

                    //Eğer tablo varsa silecek...
                    SqlCommand cmd2 = new SqlCommand("IF OBJECT_ID('dbo." + ds.Tables[i].TableName + "', 'U') IS NOT NULL DROP TABLE dbo." + ds.Tables[i].TableName + ";", conn);
                    cmd2.ExecuteNonQuery();
                    //Tbalo oluşturuluyor...
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
                 
                #endregion


                return View();
            }
            catch (Exception e)
            {
                TempData["mesaj"] = new Message() { CssClassName = "alert-danger", mesajBaslik = "Hata Mesajı!", mesajAciklama = " Dosyası Yükleme İşlemi Sırasında Hata Oluştu." };

            }
            return RedirectToAction("HomeIndex");
        }

        [Authorize]
        [HttpGet]
        public ActionResult DosyaIceriginiGoster()
        {
            return View();
        }


        static protected void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        static protected void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " +
            attr.Name + "='" + attr.Value + "'");
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


    }
}