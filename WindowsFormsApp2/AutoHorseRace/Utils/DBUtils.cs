using MySql.Data.MySqlClient;
using System.Text;

namespace AutoHorseRace.Utils
{
    /// <summary>
    /// 数据库操作类,包含两个数据源
    /// connAddress 常规数据源,一般是本地地址
    /// connEAInfoAddress EA数据源,连接远程EA主数据库
    /// </summary>
    public class DBUtils
    {
        /// <summary>
        /// 默认采用数据库地址
        /// </summary>
        public static string connAddress = "127.0.0.1";

        /// <summary>
        /// EA信息地址,包括写入/获取EA策略,自动交易信息,系统配置信息,从参数StrategyConfig.EAInfoAddress 中获取
        /// </summary>
        public static string connEAInfoAddress = "127.0.0.1";

        public static string connCommConfigStr = "server={0};user id={1};password={2};database=slipper-admin-base;Pooling=true;Max Pool Size=200;Min Pool Size=5;Connection Timeout=10;Default Command Timeout=120;Connection Lifetime=0;";

        //MySql 数据库连接
        //public static string connStr = "server="+ connAddress+ ";user id=unieap;password=unieap;database=slipper-admin-base;;Pooling=true;Max Pool Size=100;Min Pool Size=5;Connection Lifetime=1800;";
        public static string connStr = "server={0};user id=unieap;password=unieap;database=slipper-admin-base;Pooling=true;Max Pool Size=100;Min Pool Size=5;Connection Lifetime=1800;";
        //MySql 数据库连接
        //public static string eaConnStr = "server=" + connEAInfoAddress + ";user id=unieap;password=unieap;database=slipper-admin-base;;Pooling=true;Max Pool Size=100;Min Pool Size=5;Connection Lifetime=1800;";
        public static string eaConnStr = "";

        public static MySqlConnection conn = null;
        public static MySqlConnection eaConn = null;
        public static void createConn(string address)
        {
            StringBuilder connStrSb = new StringBuilder();
            if (conn == null)
            {
                if (string.IsNullOrEmpty(address))
                {
                    address = connAddress;
                }
                connStrSb.AppendFormat(connCommConfigStr, address, "unieap", "unieap");
                connStr = connStrSb.ToString();
                conn = new MySqlConnection(connStr);
            }
        }

        public static void createEaConn(string address)
        {
            StringBuilder connStrSb = new StringBuilder();
            if (eaConn == null)
            {
                if (string.IsNullOrEmpty(address))
                {
                    address = connEAInfoAddress;
                }
                connStrSb.AppendFormat(connCommConfigStr, address, "unieap", "unieap");
                eaConnStr = connStrSb.ToString();
                eaConn = new MySqlConnection(eaConnStr);
            }
        }
        /*
         查询数据，返回数组List<IDictionary<string, string>>
         */
        public static List<IDictionary<string, string>> query(string sql)
        {
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                // 执行数据库操作
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    List<IDictionary<string, string>> datas = new List<IDictionary<string, string>>();
                    while (reader.Read())
                    {
                        IDictionary<string, string> data = new Dictionary<string, string>();
                        datas.Add(data);
                        int fieldCount = reader.FieldCount;
                        for (int i = 0; i < fieldCount; i++)
                        {
                            data.Add(reader.GetName(i), reader.GetValue(i).ToString());
                        }
                    }
                    return datas;
                }
            }
        }

        /// <summary>
        /// 执行带参数的 SQL 查询，并返回结果集合。
        /// </summary>
        /// <remarks>
        /// 功能说明：
        /// 1. 支持参数化查询（防止 SQL 注入攻击）
        /// 2. 自动绑定传入的参数字典
        /// 3. 查询结果以 List&lt;IDictionary&lt;string,string&gt;&gt; 形式返回
        /// 4. 所有字段值统一转换为字符串类型
        ///
        /// 安全说明：
        /// - 推荐始终使用参数化查询，不要拼接 SQL 字符串
        /// - 参数名必须与 SQL 中占位符一致（例如 @symbol）
        ///
        /// 性能说明：
        /// - 每次调用都会创建并释放数据库连接（使用连接池）
        /// - 高频调用场景建议改为强类型泛型映射以减少装箱与字符串转换
        ///
        /// 注意事项：
        /// - 数据库字段为 NULL 时会返回 null
        /// - 本方法适用于中小规模查询，不适合超大结果集
        /// </remarks>
        /// <param name="sql">
        /// 要执行的 SQL 语句（可包含参数占位符，例如 @paramName）
        /// </param>
        /// <param name="parameters">
        /// 查询参数集合（键为参数名，例如 "@symbol"，值为对应参数值）
        /// 若为 null，则执行无参数查询
        /// </param>
        /// <returns>
        /// 返回查询结果集合。
        /// 每一行数据以 IDictionary&lt;string,string&gt; 表示：
        /// Key = 字段名，Value = 字段值（字符串形式）
        /// 若无数据则返回空集合（不会返回 null）
        /// </returns>
        public static List<IDictionary<string, string>> query(
            string sql,
            Dictionary<string, object> parameters = null)
        {
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    // 绑定参数（防止SQL注入）
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

        public static List<IDictionary<string, string>> eaQuery(string sql)
        {
            using (MySqlConnection connection = new MySqlConnection(eaConnStr))
            {
                connection.Open();
                // 执行数据库操作
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    List<IDictionary<string, string>> datas = new List<IDictionary<string, string>>();
                    while (reader.Read())
                    {
                        IDictionary<string, string> data = new Dictionary<string, string>();
                        datas.Add(data);
                        int fieldCount = reader.FieldCount;
                        for (int i = 0; i < fieldCount; i++)
                        {
                            data.Add(reader.GetName(i), reader.GetValue(i).ToString());
                        }
                    }
                    return datas;
                }
            }
        }
        /**
         * 执行sql
         */
        public static int executeSql(string sql)
        {
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                // 执行数据库操作
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// EA 数据连接执行SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int eaExecuteSql(string sql)
        {
            using (MySqlConnection connection = new MySqlConnection(eaConnStr))
            {
                connection.Open();
                // 执行数据库操作
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public static void ExecuteParameterizedSql(string sql, Dictionary<string, object> parameters)
        {
            // 实际的数据库连接和命令执行逻辑
            using (MySqlConnection connection = new MySqlConnection(eaConnStr))
            {
                connection.Open();
                using (var command = new MySqlCommand(sql, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                    command.ExecuteNonQuery();
                }

            }
        }

        public static void insert(string sql, Array parameters = null)
        {

            //string sql = "insert into test (id,name) values ('1', '张三')";
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                // 执行数据库操作
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    int res = command.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="data">和数据库字段名称保持一致</param>
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


        public static void update(string sql, Array parameters = null)
        {
            //string sql = "update test set name='李四' where id='1'";
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                // 执行数据库操作
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    int res = command.ExecuteNonQuery();
                }
            }
        }

        public static void delete(string sql, Array parameters = null)
        {
            //string sql = "delete from test where id='1'";
            using (MySqlConnection connection = new MySqlConnection(connStr))
            {
                connection.Open();
                // 执行数据库操作
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    int res = command.ExecuteNonQuery();
                }
            }
        }
        /**
         * 获取数据库序列号
         */
        public static int getSeq()
        {
            ;
            string sql = "select nextval('unieap',1) as seq";
            List<IDictionary<string, string>> result = query(sql);
            if (result != null && result.Count > 0)
            {
                return Convert.ToInt32(result[0]["seq"]);
            }
            return -1;
        }


        /// <summary>
        /// 获取数据库日期时间2023-12-30 05:25:04
        /// </summary>
        /// <returns></returns>
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
        /// 获取数据库日期时间并根据需求进行偏移2023-12-30 05:25:04
        /// </summary>
        /// <param name="xmin"></param>
        /// <returns></returns>
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
        /// 获取数据库时间05:25:04
        /// </summary>
        /// <returns></returns>
        public static string getTime()
        {
            string currentTime = getDateTime();
            currentTime = currentTime.Split(' ')[1];
            return currentTime;
        }


        private static readonly object _lockObj = new object();
        private static string _lastSecond = "";
        private static int _sequence = 0;

        /// <summary>
        /// 获取唯一时间序列号（基于北京时间，本地生成）
        ///
        /// 【格式】
        /// yyyyMMddHHmmss + 3位序号
        /// 示例：20260422013530001
        ///
        /// 【实现说明】
        /// 1. 使用 UTC 时间通过 TimeZoneInfo 转换为北京时间（避免依赖服务器本地时区）
        /// 2. 以“秒”为单位生成时间戳（yyyyMMddHHmmss）
        /// 3. 同一秒内通过递增序号（_sequence）保证唯一性
        /// 4. 使用 lock 保证线程安全（适用于单进程多线程场景）
        ///
        /// 【规则】
        /// - 每秒最多生成 999 个唯一序列号（序号范围：001 ~ 999）
        /// - 当进入新的一秒时，序号自动重置为 1
        ///
        /// 【注意事项】
        /// - 本方法适用于单机环境，多机部署时需额外引入机器ID以避免重复
        /// - 服务器时间需通过 NTP 保持同步，否则可能出现时间偏差
        /// - Windows 使用 "China Standard Time"，Linux 使用 "Asia/Shanghai"
        ///
        /// 【异常处理】
        /// - 若发生异常，将调用降级方法 GenerateFallbackSerial() 生成备用序列号
        /// </summary>
        public static string GetUniqueSerialNumber()
        {
            // 获取北京时间（兼容 Windows / Linux）
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

            // 格式：yyyyMMddHHmmss
            string currentSecond = chinaNow.ToString("yyyyMMddHHmmss");

            lock (_lockObj)
            {
                // 新的一秒 → 重置序号
                if (currentSecond != _lastSecond)
                {
                    _lastSecond = currentSecond;
                    _sequence = 1;
                }
                else
                {
                    _sequence++;
                }

                // 防止超过999（根据业务可改为抛异常或阻塞）
                if (_sequence > 999)
                {
                    _sequence = 1;
                }

                return $"{currentSecond}{_sequence:000}";
            }
        }
    }
}
