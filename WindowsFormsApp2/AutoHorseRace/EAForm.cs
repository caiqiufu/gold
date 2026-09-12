
using AutoHorseRace.Utils;
using Newtonsoft.Json;
using NLog;

namespace AutoHorseRace
{
    public partial class EAForm : Form
    {

        /// <summary>
        /// 配置文件默认目录
        /// </summary>
        public string rootPath = "C:\\iAutoTrade";

        /// <summary>
        /// 系统配置文件
        /// </summary>
        public string _ConfigFile = "\\Config.json";
        /// <summary>
        /// 操作员配置文件
        /// </summary>
        public string _MyConfigFile = "\\MyConfig.json";
        /// <summary>
        /// 自动交易策略文件
        /// </summary>
        public string _StrategyFile = "\\Strategy.json";

        /// <summary>
        /// 系统日志
        /// </summary>
        public NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 全局配置参数
        /// </summary>
        public Config _Config;

        /// <summary>
        /// 个性化配置参数
        /// </summary>
        public MyConfig _MyConfig;

        /// <summary>
        /// 业务日志
        /// </summary>
        public Log _Log;

        /// <summary>
        /// 当前交易账户信息
        /// </summary>
        public Account _Account;

        public EAForm()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void LoadConfigFile()
        {
            string newMyConfigFile = rootPath + _MyConfigFile;
            string newConfigFile = rootPath + _ConfigFile;
            _logger.Info("加载初始化文件开始");
            if (File.Exists(newMyConfigFile))
            {
                _MyConfig = JsonConvert.DeserializeObject<MyConfig>(File.ReadAllText(newMyConfigFile));
            }
            else
            {
                _logger.Fatal("没有配置文件，系统初始化异常");
            }

            if (File.Exists(newConfigFile))
            {
                _Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(newConfigFile));
            }
            else
            {
                _logger.Fatal("没有初始化配置文件，系统初始化异常");
            }
            _logger.Info("加载初始化文件完成");
        }
        /// <summary>
        /// 加载配置参数
        /// </summary>
        public virtual void LoadConfig()
        {
            _logger.Info("开始加载配置参数开始");
            if (_Config != null)
            {
                //获取数据库配置数据
                _Config.SysConfig = DBHelper.getSysConfig();
            }
            else
            {
                _logger.Fatal("没有初始化配置文件，系统初始化异常");
            }
            //加载Account信息后初始化到Log对象
            _Log._Account = _Account;
            _logger.Info("开始加载配置参数完成");
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private async void buttonAccountLogin_Click(object sender, EventArgs e)
        {
            // 1. 获取输入框的值
            string user = "vcd666888";
            string pass = "aabb1122.m";
            string pin = "1111";
            Console.WriteLine("正在登录...");
            try
            {
                // 3. 实例化你的登录系统
                var loginSystem = new AutoHorseRace.Utils.CtbLoginSystem();

                // 4. 调用异步登录方法
                string result = await loginSystem.LoginAsync(user, pass, pin);

                // 5. 显示结果
                MessageBox.Show(result);
            }
            catch (Exception ex)
            {
                // 错误处理
                MessageBox.Show($"登录失败: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("登录执行完成");
            }
        }
    }
}
