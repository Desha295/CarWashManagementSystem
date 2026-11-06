using System.Data.SqlClient;

namespace CarWashManagementSystem
{
    internal static class DatabaseInitializer
    {
        public static void EnsureCustomerEmailColumn()
        {
            dbConnect db = new dbConnect();
            try
            {
                db.open();
                using (SqlCommand command = new SqlCommand(
                    "IF COL_LENGTH('dbo.tbCustomer', 'email') IS NULL " +
                    "ALTER TABLE dbo.tbCustomer ADD email NVARCHAR(254) NULL;",
                    db.connect()))
                {
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                db.close();
            }
        }

        public static void EnsureWashWorkflowColumns()
        {
            dbConnect db = new dbConnect();
            try
            {
                db.open();
                using (SqlCommand command = new SqlCommand(
                    "IF COL_LENGTH('dbo.tbCash', 'washstatus') IS NULL " +
                    "ALTER TABLE dbo.tbCash ADD washstatus NVARCHAR(20) NOT NULL " +
                    "CONSTRAINT DF_tbCash_washstatus DEFAULT ('Waiting') WITH VALUES; " +
                    "IF COL_LENGTH('dbo.tbCash', 'washstartedat') IS NULL " +
                    "ALTER TABLE dbo.tbCash ADD washstartedat DATETIME NULL; " +
                    "IF COL_LENGTH('dbo.tbCash', 'washreadyat') IS NULL " +
                    "ALTER TABLE dbo.tbCash ADD washreadyat DATETIME NULL; " +
                    "IF COL_LENGTH('dbo.tbCash', 'washdeliveredat') IS NULL " +
                    "ALTER TABLE dbo.tbCash ADD washdeliveredat DATETIME NULL;",
                    db.connect()))
                {
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                db.close();
            }
        }
    }
}
