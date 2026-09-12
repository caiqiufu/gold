using MySql.Data.MySqlClient;
using System.Text;

namespace AutoHorseRace.Utils
{
    /// <summary>
    /// 数据库操作类, 包含常规数据源与 EA 远程数据源。
    /// 统一采用连接池（Pooling=true）与局部 using 自动释放机制，防止连接泄漏。
    /// </summary>
    public class DBUtils
    {
        /// <summary>
        /// 默认常规数据库地址（一般是本地地址）
        /// </summary>
        public static string connAddress = "127.0.0.1";

        /// <summary>
        /// EA 信息地址（连接远程 EA 主数据库，从参数 Config.EAInfoAddress 中获取）
        /// </summary>
        public static string connEAInfoAddress = "127.0.0.1";

        /// <summary>
        /// 基础连接字符串模板（配置了连接池、超时和生命周期控制）
        /// </summary>
        public static string connCommConfigStr = "server={0};user id={1};password={2};database=slipper-admin-base;Pooling=true;Max Pool Size=200;Min Pool Size=5;Connection Timeout=10;Default Command Timeout=120;Connection Lifetime=0;";

        /// <summary>
        /// 常规数据库连接字符串
        /// </summary>
        public static string connStr = "server=127.0.0.1;user id=unieap;password=unieap;database=slipper-admin-base;Pooling=true;Max Pool Size=100;Min Pool Size=5;Connection Lifetime=1800;";

        /// <summary>
        /// EA 数据库连接字符串
        /// </summary>
        public static string eaConnStr = "";

        /// <summary>
        /// 动态构建或更新常规数据库连接字符串
        /// </summary>
        public static void createConn(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                address = connAddress;
            }
            StringBuilder connStrSb = new StringBuilder();
            connStrSb.AppendFormat(connCommConfigStr, address, "unieap", "unieap");
            connStr = connStrSb.ToString();
        }

        /// <summary>
        /// 动态构建或更新 EA 数据库连接字符串
        /// </summary>
        public static void createEaConn(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                address = connEAInfoAddress;
            }
            StringBuilder connStrSb = new StringBuilder();
            connStrSb.AppendFormat(connCommConfigStr, address, "unieap", "unieap");
            eaConnStr = connStrSb.ToString();
        }

        /// <summary>
        /// 查询数据（常规库），返回 List&lt;IDictionary&lt;string, string&gt;&gt;
        /// </summary>
        public static List<IDictionary<string, string>> query(string sql)
        {
            return query(sql, null);
        }

        /// <summary>
        /// 执行带参数的 SQL 查询（常规库）
        /// </summary>
        public static List<IDictionary<string, string>> query(
            string sql,
            Dictionary<string, object> parameters)
        {
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        List<IDictionary<string, string>> datas = new List<IDictionary<string, string>>();
                        while (reader.Read())
                        {
                            IDictionary<string, string> data = new Dictionary<string, string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                data[columnName] = value?.ToString();
                            }
                            datas.Add(data);
                        }
                        return datas;
                    }
                }
            }
        }

        /// <summary>
        /// EA 数据源查询
        /// </summary>
        public static List<IDictionary<string, string>> eaQuery(string sql)
        {
            using (MySqlConnection connection = new MySqlConnection(eaConnStr))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    List<IDictionary<string, string>> datas = new List<IDictionary<string, string>>();
                    while (reader.Read())
                    {
                        IDictionary<string, string> data = new Dictionary<string, string>();
                        int fieldCount = reader.FieldCount;
                        for (int i = 0; i < fieldCount; i++)
                        {
                            data.Add(reader.GetName(i), reader.GetValue(i)?.ToString());
                        }
                        datas.Add(data);
                    }
                    return datas;
                }
            }
        }

        /// <summary>
        /// 执行写操作 SQL（常规库）
        /// </summary>
        public static int executeSql(string sql)
        {
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// EA 数据连接执行写操作 SQL
        /// </summary>
        public static int eaExecuteSql(string sql)
        {
            using (MySqlConnection connection = new MySqlConnection(eaConnStr))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// EA 数据连接执行带参数的写操作 SQL
        /// </summary>
        public static void ExecuteParameterizedSql(string sql, Dictionary<string, object> parameters)
        {
            using (MySqlConnection connection = new MySqlConnection(eaConnStr))
            {
                connection.Open();
                using (var command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 插入数据（支持数组参数）
        /// </summary>
        public static void insert(string sql, Array parameters = null)
        {
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 批量插入数据（事务支持）
        /// </summary>
        public static void bulkInsert(string tableName, List<Dictionary<string, object>> data)
        {
            if (data == null || data.Count == 0) return;

            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        List<string> valueRows = new List<string>();
                        List<MySqlParameter> parameters = new List<MySqlParameter>();

                        int index = 0;
                        foreach (var row in data)
                        {
                            List<string> values = new List<string>();
                            foreach (var kvp in row)
                            {
                                string paramName = $"@{kvp.Key}{index}";
                                values.Add(paramName);
                                parameters.Add(new MySqlParameter(paramName, kvp.Value ?? DBNull.Value));
                            }
                            valueRows.Add($"({string.Join(",", values)})");
                            index++;
                        }

                        string columns = string.Join(",", data[0].Keys);
                        string query = $"INSERT INTO {tableName} ({columns}) VALUES {string.Join(",", valueRows)};";

                        using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行插入并返回自增 ID（EA 库）
        /// </summary>
        public static int ExecuteInsertAndGetId(string sql, IDictionary<string, object> parameters)
        {
            using (var connection = new MySqlConnection(eaConnStr))
            {
                connection.Open();
                using (var command = new MySqlCommand(sql + "; SELECT LAST_INSERT_ID();", connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }
                    object result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        public static void update(string sql, Array parameters = null)
        {
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        public static void delete(string sql, Array parameters = null)
        {
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 获取数据库序列号
        /// </summary>
        public static int getSeq()
        {
            string sql = "select nextval('unieap',1) as seq";
            List<IDictionary<string, string>> result = query(sql);
            if (result != null && result.Count > 0)
            {
                return Convert.ToInt32(result[0]["seq"]);
            }
            return -1;
        }

        /// <summary>
        /// 获取数据库日期时间（格式：yyyy-MM-dd HH:mm:ss）
        /// </summary>
        public static string getDateTime()
        {
            string sql = "select DATE_FORMAT(SYSDATE(), '%Y-%m-%d %H:%i:%s') as currentTime";
            List<IDictionary<string, string>> result = query(sql);
            if (result != null && result.Count > 0)
            {
                return result[0]["currentTime"];
            }
            return null;
        }

        /// <summary>
        /// 获取数据库日期时间并进行分钟偏移
        /// </summary>
        public static string GetOffsetTime(int offsetMinutes)
        {
            string sql = $@"
            SELECT DATE_FORMAT(
                DATE_ADD(SYSDATE(), INTERVAL {offsetMinutes} MINUTE),
                '%Y-%m-%d %H:%i:%s'
            ) AS currentTime";

            List<IDictionary<string, string>> result = query(sql);
            if (result != null && result.Count > 0)
            {
                return result[0]["currentTime"];
            }
            return null;
        }

        /// <summary>
        /// 获取数据库时间（格式：HH:mm:ss）
        /// </summary>
        public static string getTime()
        {
            string currentTime = getDateTime();
            if (!string.IsNullOrEmpty(currentTime) && currentTime.Contains(" "))
            {
                return currentTime.Split(' ')[1];
            }
            return currentTime;
        }

        private static readonly object _lockObj = new object();
        private static string _lastSecond = "";
        private static int _sequence = 0;

        /// <summary>
        /// 获取唯一时间序列号（基于北京时间，本地生成，支持高并发每秒 999 个）
        /// </summary>
        public static string GetUniqueSerialNumber()
        {
            TimeZoneInfo chinaTimeZone;
            try
            {
                chinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            }
            catch
            {
                chinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
            }

            DateTime chinaNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, chinaTimeZone);
            string currentSecond = chinaNow.ToString("yyyyMMddHHmmss");

            lock (_lockObj)
            {
                if (currentSecond != _lastSecond)
                {
                    _lastSecond = currentSecond;
                    _sequence = 1;
                }
                else
                {
                    _sequence++;
                }

                if (_sequence > 999)
                {
                    _sequence = 1;
                }

                return $"{currentSecond}{_sequence:000}";
            }
        }
    }
}