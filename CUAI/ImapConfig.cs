using MailKit.Net.Imap;
using MailKit.Security;
using System;
using System.Text;

namespace CUAI
{
    /// <summary>IMAP服务器配置</summary>
    public class ImapConfig
    {
        private readonly String _Server;
        private readonly Int32 _Port;
        private readonly SecureSocketOptions _UseSSL;
        private readonly String _Username, _Password;
        private Exception _Exception = null;
        /// <summary>错误信息</summary>
        public String Exception => $"{_Exception.Message}\r\n{_Exception.StackTrace}";
        /// <summary>生成IMAP配置</summary>
        /// <param name="Server">服务器地址[:端口]</param>
        /// <param name="Username">用户名</param>
        /// <param name="Password">密码</param>
        /// <param name="TrySSL">尝试SSL</param>
        public ImapConfig(String Server, String Username, String Password, Boolean TrySSL = true)
        {
            _Username = Username; _Password = Password;
            _UseSSL = TrySSL ? SecureSocketOptions.Auto : SecureSocketOptions.None;
            if (Server.Split(':').Length == 1) { _Server = Server; _Port = TrySSL ? 993 : 143; }
            else { _Server = Server.Split(':')[0]; _Port = Int32.Parse(Server.Split(':')[1]); }
        }
        /// <summary>生成IMAP配置</summary>
        /// <param name="Server">服务器地址</param>
        /// <param name="Port">端口</param>
        /// <param name="Username">用户名</param>
        /// <param name="Password">密码</param>
        /// <param name="TrySSL">尝试SSL</param>
        public ImapConfig(String Server, Int32 Port, String Username, String Password, Boolean TrySSL = true)
        {
            _Username = Username; _Password = Password;
            _UseSSL = TrySSL ? SecureSocketOptions.Auto : SecureSocketOptions.None;
            _Server = _Server = Server; _Port = Port;
        }
        /// <summary>创建IMAP客户端</summary>
        /// <param name="ReturnClient">返回生成的客户端</param>
        /// <returns>是否成功生成客户端</returns>
        public Boolean GetClient(out ImapClient ReturnClient)
        {
            try
            {
                ImapClient NewClient = new ImapClient();
                NewClient.Connect(_Server, _Port, _UseSSL);
                NewClient.Authenticate(Encoding.UTF8, _Username, _Password);
                if (NewClient.IsAuthenticated)
                { ReturnClient = NewClient; return true; }
                else { ReturnClient = null; return false; }
            }
            catch (Exception e) { _Exception = e; ReturnClient = null; return false; }
        }
    }
}
