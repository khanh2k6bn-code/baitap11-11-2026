using MySql.Data.MySqlClient;

namespace HRMSystem.DAL
{
    public static class DatabaseHelper
    {
        // Chuỗi kết nối chuẩn tới MySQL trên XAMPP
        private static readonly string connectionString = 
            "Server=localhost;Port=3306;Database=HRMDB;Uid=root;Pwd=;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
