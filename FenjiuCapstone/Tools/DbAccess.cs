using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace sales_managers.Common
{
    /// <summary>
    /// 数据库集成方法 create by Zane Xu 2024-12-12 
    /// </summary>
    public class DbAccess
    {
        /// <summary>
        /// ConnectionString连接字对象
        /// </summary>
        public static string ConnectionString = "Server=localhost;Port=3306;Database=fenjiumanagementsystem;Uid=root;Pwd=1234;Pooling=true;Charset=utf8;SslMode=none;";
        
        /// <summary>
        /// 生成连接
        /// </summary>
        /// <returns></returns>
        public static MySqlConnection connectingMySql()
        {
            try
            {
                var connection = new MySqlConnection(ConnectionString);
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"连接失败：{ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="msc"></param>
        public static void con_close(MySqlConnection msc)
        {
            if (msc != null && msc.State == ConnectionState.Open)
            {
                msc.Close();
                msc.Dispose();
            }
        }

        public static MySqlCommand command(string sql, MySqlConnection connection, MySqlTransaction transaction = null)
        {
            MySqlCommand cmd = new MySqlCommand(sql, connection, transaction);
            return cmd;
        }

        /// <summary>
        /// 执行数据库
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int Execute(string sql)
        {
            using (var connection = connectingMySql())
            {
                using (var cmd = command(sql, connection))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // 返回查询结果，主要以 Reader 对象进行返回
        public static MySqlDataReader read(string sql)
        {
            var connection = connectingMySql();
            var cmd = command(sql, connection);
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public void My_Operate_db(string SQLstr)
        {
            using (var connection = connectingMySql())
            {
                using (var cmd = command(SQLstr, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static MySqlDataAdapter CreatAdapter(string sql)
        {
            using (var connection = connectingMySql())
            {
                using (var cmd = command(sql, connection))
                {
                    return new MySqlDataAdapter(cmd);
                }
            }
        }

        public static MySqlTransaction sqltransaction(MySqlConnection connection)
        {
            return connection.BeginTransaction();
        }

        public static MySqlCommand orderCmd(string sql, MySqlTransaction transaction)
        {
            return new MySqlCommand(sql, transaction.Connection, transaction);
        }
    }
}