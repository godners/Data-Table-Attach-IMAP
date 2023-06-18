using MimeKit;
using MimeKit.Text;
using System;
using System.Linq;
using System.Text;

namespace CUAI
{
    /// <summary>内容更新</summary>
    public class Content
    {
        /// <summary>内容标题</summary>
        public String Title;
        /// <summary>内容正文</summary>
        public String Body;
        /// <summary>内容时间戳</summary>
        public String TimeStamp;
        /// <summary>内容时间戳</summary>
        public DateTime TimeStampValue => DateTime.Parse(TimeStamp);
        /// <summary>邮件主题</summary>
        public String Subject => $"{Title} | {TimeStamp}";
        /// <summary>内容更新收发地址</summary>
        public String Address;
        /// <summary>内容更新收发显示名</summary>
        public String AddressName;
        /// <summary>内容更新启用备份</summary>
        public Boolean IsBackup;
        /// <summary>内容更新备份地址</summary>
        public String Backup;
        /// <summary>内容更新备份显示名</summary>
        public String BackupName;
        private MimeMessage _Message;
        private Exception _Exception = null;
        /// <summary>异常信息</summary>
        public String Exception => _Exception is null ? "No Exception" : $"{_Exception.Message}\r\n{_Exception.StackTrace}";
        /// <summary>生成内容更新</summary>
        /// <param name="ContentTitle">标题</param>
        /// <param name="ContentBody">内容</param>
        public Content(String ContentTitle, String ContentBody)
        {
            Title = ContentTitle; Body = ContentBody;
            TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        /// <summary>生成内容更新</summary>
        /// <param name="ContentTitle">标题</param>
        /// <param name="ContentBody">内容</param>
        /// <param name="ContentTimeStamp">时间戳</param>
        public Content(String ContentTitle, String ContentBody, DateTime ContentTimeStamp)
        {
            Title = ContentTitle; Body = ContentBody;
            TimeStamp = ContentTimeStamp.ToString("yyyyMMddHHmmssfff");
        }
        /// <summary>生成内容更新</summary>
        /// <param name="ContentMessage"></param>
        public Content(MimeMessage ContentMessage)
        {
            _Message = ContentMessage;
            Title = ContentMessage.Subject.Split('|')[0].Trim();
            Body = ((TextPart)ContentMessage.Body).Text;
            TimeStamp = ContentMessage.Subject.Split('|')[1].Trim();
            DateTime.Parse(TimeStamp);
            Address = ContentMessage.Sender.Address;
            AddressName = ContentMessage.Sender.Name;
            if (ContentMessage.Cc.ToArray().Length > 0)
            {
                Backup = ((MailboxAddress)ContentMessage.Cc.First<InternetAddress>()).Address;
                Backup = ((MailboxAddress)ContentMessage.Cc.First<InternetAddress>()).Name;
            }
        }
        /// <summary>设置内容更新收发备份</summary>
        /// <param name="ContentAddress">内容更新收发地址</param>
        /// <param name="ContentAddressName">内容更新收发显示名</param>
        /// <param name="ContentBackup">内容更新备份地址</param>
        /// <param name="ContentBackupName">内容更新备份显示名</param>
        /// <returns></returns>
        public void SetAddress(String ContentAddress, String ContentAddressName = "", String ContentBackup = "", String ContentBackupName = "")
        {
            Address = ContentAddress; AddressName = ContentAddressName == "" ? ContentAddress : ContentAddressName;
            if (ContentBackup != "")
            { IsBackup = true; Backup = ContentBackup; BackupName = ContentBackupName == "" ? ContentBackup : ContentBackupName; }
        }
        /// <summary>生成IMAP消息（不含收件人、发件人）</summary>        
        /// <returns>是否成功生成消息</returns>
        public Boolean GetMessage(out MimeMessage ResultMessage)
        {
            try
            {
                TextPart NewTextPart = new TextPart(TextFormat.Plain);
                NewTextPart.SetText(Encoding.UTF8, Body);
                _Message = new MimeMessage
                {
                    Body = NewTextPart,
                    Subject = Subject,
                    Importance = MessageImportance.Normal,
                    Priority = MessagePriority.Normal
                };
                MailboxAddress NewAddress = new MailboxAddress(Encoding.UTF8, AddressName, Address);
                _Message.Sender = NewAddress; _Message.From.Add(NewAddress); _Message.To.Add(NewAddress);
                if (IsBackup) _Message.Cc.Add(new MailboxAddress(Encoding.UTF8, BackupName, Backup));
                ResultMessage = _Message; return true;
            }
            catch (Exception e) { ResultMessage = null; _Exception = e; return false; }
        }
    }
}