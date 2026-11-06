using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarWashManagementSystem
{
    //to get connection string between application and database   
    class dbConnect
    {
        private readonly SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["CarWashDb"].ConnectionString);
        public SqlConnection connect()
        {
            return cn;
        }

        public void open()
        {
            if (cn.State == System.Data.ConnectionState.Closed)
                cn.Open();
        }

        public void close()
        {
            if (cn.State == System.Data.ConnectionState.Open)
                cn.Close();
        }

        public void executeQuery(string sql)
        {
            using (SqlCommand cm = new SqlCommand(sql, connect()))
            {
                open();
                try { cm.ExecuteNonQuery(); }
                finally { close(); }
            }
        }
    }
}
