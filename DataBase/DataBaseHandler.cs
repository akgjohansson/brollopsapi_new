using ChamberOfSecrets;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public abstract class DataBaseHandler : IDisposable
    {
        public static string ConnectionString { get; set; }
        public static string Databasename { get; set; }
        public ISession Session { get; set; }
        public string RootPath { get; set; }
        private static ISessionFactory _sessionFactory;
        public SqlCredential Credentials { get; set; }


        public DataBaseHandler(string connectionString)
        {
            Databasename = "DB_A2AF2E_brollop";
            if (string.IsNullOrWhiteSpace(connectionString))
                ConnectionString = @"Data Source=SQL6003.site4now.net;Initial Catalog=DB_A2AF2E_brollop;";
                //ConnectionString = @"Server = (localdb)\mssqllocaldb; Database = Brollop;";
            else
                ConnectionString = connectionString;
            Session = OpenSession();
            RootPath = System.IO.Path.GetFullPath(".");// "C:\\Users\\Andreas.Johansson\\Documents\\Visual Studio 2017\\Projects\\gotaelves.se";

        }

        private static ISession OpenSession()
        {
            var dbCredentials = new SecretContainer("brollop_db");
            bool doesDBExist = false;
            //var constr = ConfigurationManager.ConnectionStrings["MyDbConn"].ToString();
            var password = new SecureString();
            foreach (char character in dbCredentials.Password)
            {
                password.AppendChar(character);
            }
            password.MakeReadOnly();
            var credentials = new SqlCredential(dbCredentials.UserName, password);
            using (SqlConnection con = new SqlConnection(ConnectionString, credentials))
            {
                using (SqlCommand com = new SqlCommand($"select name from sys.databases where name = '{Databasename}';", con))
                {
                    con.Open();
                    //con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    if (reader.Read())
                        doesDBExist = true;
                    reader.Close();
                }
            }
            if (doesDBExist)

            {
                if (_sessionFactory == null)
                {
                    _sessionFactory = Configure().BuildSessionFactory();
                }
                ISession session = _sessionFactory.OpenSession();
                session.BeginTransaction();
                return session;
            }
            else
                return null;
        }

        public static Configuration Configure()
        {
            var dbCredentials = new SecretContainer("brollop_db");
            Configuration cfg = new Configuration()
                           .DataBaseIntegration(db =>
                           {

                               //db.ConnectionString = ConnectionString + " Trusted_Connection = True;";
                               db.ConnectionString = ConnectionString + $"User Id={dbCredentials.UserName};Password={dbCredentials.Password};";// + " Trusted_Connection = True;";
                               db.Dialect<MsSql2008Dialect>();
                           });

            var mapper = new ModelMapper();
            Type[] myTypes = Assembly.GetExecutingAssembly().GetExportedTypes();
            mapper.AddMappings(myTypes);

            HbmMapping mapping = new DataBase.Mapping.Mapping().Map();
            cfg.AddMapping(mapping);

            return cfg;
        }

        public void BuildDatabase()
        {
            bool dbExist = true;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand com = new SqlCommand("if db_id('Brollop') is null create database Brollop", con))
            {
                dbExist = false;
                con.Open();
                com.ExecuteNonQuery();

            }
            if (!dbExist)
            {
                var schema = new SchemaExport(Configure());
                schema.SetOutputFile($"{RootPath}\\Brollop.sql");
                schema.Create(true, false);
                schema.Create(true, true);
            }
        }

        public void Dispose()
        {
            if (Session != null)
                Session.Close();
        }

    }
}
