using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Native;
using DevExpress.DataAccess.Web;


namespace DKN.db
{
    public class MyDataSourceWizardConnectionStringsProvider : IDataSourceWizardConnectionStringsProvider
    {
        public Dictionary<string, string> GetConnectionDescriptions()
        {
            Dictionary<string, string> connections = AppConfigHelper.GetConnections().Keys.ToDictionary(x => x, x => x);

            // Customize the loaded connections list. 
            connections.Remove("LocalSqlServer");
            connections.Add("DKN", "DKN Bağlantısı");
            return connections;
        }

        public DataConnectionParametersBase GetDataConnectionParameters(string name)
        {
            // Return custom connection parameters for the custom connection(s). 
            if (name == "DKN")
            {
                //db.BaglantiSinifi.DKNBaglantiAyarlariniOkuSQLServer();
                //devexpress report çalışırken ip kabul etmedi. O yüzden yeni bir değişken tanımladık...
                //return new MsSqlConnectionParameters(db.BaglantiSinifi.DKNServer, db.BaglantiSinifi.DKNDatabase, db.BaglantiSinifi.DKNUser, db.BaglantiSinifi.DKNPassword, MsSqlAuthorizationType.SqlServer);
                return new MsSqlConnectionParameters(parametreler.Sabitler.sunucuAdiR
                                                   , parametreler.Sabitler.veriTabaniAdiR
                                                   , parametreler.Sabitler.kullaniciAdiR
                                                   , parametreler.Sabitler.kullaniciSifreR
                                                   , MsSqlAuthorizationType.SqlServer);

            }
            return AppConfigHelper.LoadConnectionParameters(name);
        }
    }

}