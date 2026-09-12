using System;
using System.Collections.Generic;

namespace uClient.Comm
{
    public class Config
    {
        /// <summary>
        /// OEC数据源地址
        /// </summary>
        public String PublishAddressOEC { set; get; }
        /// <summary>
        /// T4数据源地址
        /// </summary>
        public String PublishAddressT4 { set; get; }
        /// <summary>
        /// V4报价数据源地址
        /// </summary>
        public String PublishAddressV4 { set; get; }

        /// <summary>
        /// EA策略数据源地址 like: tcp://127.0.0.1:5553
        /// </summary>
        public String PublishAddressEA { set; get; }

        /// <summary>
        /// 对锁窗口绑定IP
        /// </summary>
        public String LockSubscriberAddress { set; get; }

        /// <summary>
        /// 当前使用的数据源类型
        /// T4/OEC/EA
        /// </summary>
        public String DSType { set; get; }

        /// <summary>
        /// 系统启动默认的平台
        /// </summary>
        public String PlatformCode { set; get; }
        /// <summary>
        /// 平台列表
        /// </summary>
        public List<uClient.Comm.Platform> Platform { set; get; }
        /// <summary>
        /// 平台列表
        /// </summary>
        public Dictionary<String, Platform> PlatformList = new Dictionary<String, Platform>();
        /// <summary>
        /// 主菜单上的按钮
        /// </summary>
        public Dictionary<String, IList<string>> MenuButtonList = new Dictionary<String, IList<string>>();
        /// <summary>
        /// 客户端类型,EA,DS
        /// </summary>
        public string clientType { set; get; }

        /// <summary>
        /// 数据源数据库连接地址,如T4/OEC/情绪指数等
        /// </summary>
        public string PublishAddressDS { set; get; }

        /// <summary>
        ///EA信息地址,包括写入/获取EA策略,自动交易信息,系统配置信息,回写交易日志信息
        /// </summary>
        public string EAInfoAddress { set; get; }

        /// <summary>
        /// 默认数据库地址
        /// </summary>
        public string DefaultDBAddress { set; get; }
        /// <summary>
        /// 系统配置参数,从服务器读取数据
        /// </summary>
        public Dictionary<String, String> SysConfig = new Dictionary<String, String>();
    }
}
