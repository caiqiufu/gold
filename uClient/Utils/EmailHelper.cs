using System;
using System.Net.Mail;
using System.Net;
using System.Text;
using NLog;


namespace uClient.Comm
{
    public class EmailHelper
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="toAddressList">要发送的邮箱</param>
        /// <param name="subject">邮箱主题</param>
        /// <param name="content">邮箱内容</param>
        /// <returns>返回发送邮箱的结果</returns>
        public static bool SendEmail(string[] toAddressList,string subject,string content)
        {
            content = content+ "\r\n" + " From Location : " + uClient.Comm.Utils.GetLocalIP() + " Date : "+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // 邮件服务设置
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
            smtpClient.Host = "smtp.qq.com"; //指定SMTP服务器

            //默认端口为25,由于阿里云限制25端口，改使用587端口发送成功
            smtpClient.Port = 587;

            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            //不能使用qq密码，需要使用qq邮箱专门的授权密码
            smtpClient.Credentials = new NetworkCredential("744161932@qq.com", "fhlkvhagryqsbdjf");//用户名和密码
            // 发送邮件设置       
            //MailMessage mailMessage = new MailMessage("744161932@qq.com","caiqiufu@sohu.com"); // 发送人和收件人
            if (toAddressList == null || toAddressList.Length ==0)
            {
                toAddressList = new string[] { "caiqiufu@hotmail.com" };
            }
            foreach (string address in toAddressList)
            {
                string senderDisplayName = "深圳龙知易科技";//这个配置的是发件人的要显示在邮件的名称
                string recipientsDisplayName = address;//这个配置的是收件人的要显示在邮件的名称
                MailAddress mailfrom = new MailAddress("744161932@qq.com", senderDisplayName, Encoding.UTF8);//发件人邮箱地址，名称，编码UTF8
                MailAddress mailto = new MailAddress(address, recipientsDisplayName, Encoding.UTF8);//收件人邮箱地址，名称，编码UTF8
                                                                                              //创建mailMessage对象
                MailMessage mailMessage = new MailMessage(mailfrom, mailto);

                //MailMessage mailMessage = new MailMessage("744161932@qq.com",address); // 发送人和收件人
                mailMessage.Subject = subject;//主题
                mailMessage.Body = content;//内容
                mailMessage.BodyEncoding = Encoding.UTF8;//正文编码
                mailMessage.IsBodyHtml = true;//设置为HTML格式
                mailMessage.Priority = MailPriority.Low;//优先级
                try
                {
                    smtpClient.Send(mailMessage); // 发送邮件,
                }
                catch (SmtpException ex)
                {
                    _logger.Info(ex.Message);
                    return false;
                }
            }
            return true;
        }
    }
}
