using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    class Account
    {
        /// <summary>
        ///  MT4 帐号
        /// </summary>
        public string UserCode { set; get; }

        /// <summary>
        ///  MT4 密码
        /// </summary>
        public string Password { set; get; }

        /// <summary>
        /// 交易品种
        /// </summary>
        public string[] Symbol { set; get; }
        /**
         * IP
         */
        public string IP { set; get; }
        /**
         * Port
         */
        public string Port { set; get; }

    }
}
