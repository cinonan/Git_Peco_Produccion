using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearch.Core.Models.Settings
{
    public class MSSQLSetting
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public string GetConnectionString
        {
            get
            {
                return String.Format("Server={0}; Initial Catalog={1}; User id={2}; Password={3};", Server, Database, User, Password);
                //cnx = "Server=tcp:demo-pc-server.database.windows.net,1433;Database=DBCatalogo_20200826_DESA;User ID=wrojas@demo-pc-server;Password=H$fNMYcW;Trusted_Connection=False;Encrypt=True;";

            }
        }

    }
}
