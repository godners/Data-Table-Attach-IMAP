using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CUAI
{
    /// <summary>IMAP服务器动作</summary>
    public static class ImapAction
    {
        private readonly static String[] InboxName = { "Inbox", "inbox", "INBOX", "收件箱" };
        private const String ArchiveName = "Archive";
        private static String GetException(Exception e) => $"{e.Message}\r\n{e.StackTrace}";
        private static ImapFolder RootFolder(ImapClient Client) => (ImapFolder)Client.Inbox;
        private static Boolean ExistFolder(String FolderName, ImapFolder ParentFolder, out Boolean Exists, out String Exception)
        {
            Exception = String.Empty; Exists = false;
            try { ParentFolder.Open(FolderAccess.ReadOnly); ParentFolder.GetSubfolder(FolderName); ParentFolder.Close(); Exists = true; return true; }
            catch (FolderNotFoundException) { ParentFolder.Close(); Exists = false; return true; }
            catch (Exception e) { if (!(e is FolderNotFoundException)) Exception = GetException(e); return false; }
        }
        private static Boolean GetFolder(ImapClient Client, String FolderName, ImapFolder ParentFolder, out ImapFolder Folder, out String Exception)
        {
            Exception = String.Empty; Folder = null;
            if (InboxName.Contains<String>(FolderName))
                try { Folder = (ImapFolder)Client.Inbox; return true; }
                catch (Exception e) { Exception = GetException(e); return false; }
            try { ParentFolder.Open(FolderAccess.ReadOnly); Folder = (ImapFolder)ParentFolder.GetSubfolder(FolderName); ParentFolder.Close(); return true; }
            catch (Exception e) { Exception = GetException(e); return false; }
        }
        private static Boolean CreateFolder(String FolderName, ImapFolder ParentFolder, out ImapFolder Folder, out String Exception)
        {
            Exception = String.Empty; Folder = null;
            try { ParentFolder.Open(FolderAccess.ReadWrite); Folder = (ImapFolder)ParentFolder.Create(FolderName, true); ParentFolder.Close(); return true; }
            catch (Exception e) { Exception = GetException(e); return false; }
        }
        private static Boolean SetFolder(ImapClient Client, String FolderName, ImapFolder ParentFolder, out ImapFolder Folder, out String Exception)
        {
            Exception = String.Empty; Folder = null;
            if (InboxName.Contains<String>(FolderName))
                try { Folder = (ImapFolder)Client.Inbox; return true; }
                catch (Exception e) { Exception = GetException(e); return false; }
            if (!ExistFolder(FolderName, ParentFolder, out Boolean HasFolder, out Exception)) return false;
            if (HasFolder) { GetFolder(Client, FolderName, ParentFolder, out Folder, out Exception); return true; }
            else return CreateFolder(FolderName, ParentFolder, out Folder, out Exception);
        }
        /// <summary>上传内容更新</summary>
        /// <param name="Config">IMAP服务器配置</param>
        /// <param name="UploadContent">上传内容</param>
        /// <param name="Exception">返回错误</param>
        /// <param name="UploadFolder">上传文件夹[默认=收件箱]</param>
        /// <returns>是否成功上传内容更新</returns>
        public static Boolean Upload(ImapConfig Config, Content UploadContent, out String Exception, String UploadFolder = "Inbox")
        {
            if (!Config.GetClient(out ImapClient Client)) { Exception = Config.Exception; return false; }
            if (!SetFolder(Client, UploadFolder, RootFolder(Client), out ImapFolder ContentFolder, out Exception)) return false;

            try
            {
                UploadContent.GetMessage(out MimeMessage ContentMessage);
                AppendRequest ContentRequest = new AppendRequest(ContentMessage, MessageFlags.None);
                ContentFolder.Append(ContentRequest); Client.Disconnect(true); return true;
            }
            catch (Exception e) { Exception = GetException(e); return false; }
        }
        /// <summary>列出内容更新清单</summary>
        /// <param name="Config">IMAP服务器配置</param>
        /// <param name="Title">内容更新标题</param>
        /// <param name="ContentList">返回内容更新清单</param>
        /// <param name="Exception">返回异常</param>
        /// <param name="SearchFolder">搜索文件夹[默认=收件箱]</param>
        /// <returns>是否成功列出清单</returns>
        public static Boolean List(ImapConfig Config, String Title, out Dictionary<UniqueId, Content> ContentList, out String Exception, String SearchFolder = "Inbox")
        {
            ContentList = new Dictionary<UniqueId, Content>();
            if (!Config.GetClient(out ImapClient Client)) { Exception = Config.Exception; return false; }
            if (!GetFolder(Client, SearchFolder, RootFolder(Client), out ImapFolder ContentFolder, out Exception)) return false;
            try
            {
                TextSearchQuery ContentQuery = SearchQuery.SubjectContains(Title);
                ContentFolder.Open(FolderAccess.ReadOnly);
                UniqueId[] ContentUniqueIds = ContentFolder.Search(ContentQuery).ToArray<UniqueId>();
                foreach (UniqueId ContentUniqueId in ContentUniqueIds)
                    ContentList.Add(ContentUniqueId, new Content(ContentFolder.GetMessage(ContentUniqueId)));
                ContentFolder.Close(); Client.Disconnect(true); return true;
            }
            catch (Exception e) { Exception = GetException(e); return false; }
        }
        /// <summary>获取最新内容更新</summary>
        /// <param name="ContentList">内容更新清单</param>
        /// <param name="NewestContent">输出最新内容更新</param>
        /// <param name="Exception">返回一场</param>
        /// <returns>是否成功获取最新内容更新</returns>
        public static Boolean Newest(Dictionary<UniqueId, Content> ContentList, out Content NewestContent, out String Exception)
        {
            NewestContent = null; Exception = String.Empty;
            try
            {
                DateTime NewestUpdate = DateTime.MinValue;
                foreach (UniqueId ContentUniqueId in ContentList.Keys)
                    if (ContentList[ContentUniqueId].TimeStampValue.CompareTo(NewestUpdate).Equals(1))
                    { NewestUpdate = ContentList[ContentUniqueId].TimeStampValue; NewestContent = ContentList[ContentUniqueId]; }
                return true;
            }
            catch (Exception e) { Exception = GetException(e); return false; }
        }
        /// <summary>获取最新内容更新</summary>
        /// <param name="Config">IMAP服务器配置</param>
        /// <param name="Title">内容更新标题</param>
        /// <param name="NewestContent">输出最新内容更新</param>
        /// <param name="Exception">返回异常</param>
        /// <param name="SearchFolder">搜索文件夹[默认=收件箱]</param>
        /// <returns>是否成功获取最新内容更新</returns>
        public static Boolean Newest(ImapConfig Config, String Title, out Content NewestContent, out String Exception, String SearchFolder = "Inbox")
        {
            NewestContent = null;
            if (!List(Config, Title, out Dictionary<UniqueId, Content> ContentList, out Exception, SearchFolder)) return false;
            if (!Newest(ContentList, out NewestContent, out Exception)) return false; ContentList.Clear(); return true;
        }
        /// <summary>归档内容更新</summary>
        /// <param name="Config">IMAP服务器配置</param>
        /// <param name="Title">内容更新标题</param>
        /// <param name="Exception">异常</param>
        /// <param name="SearchFolder">搜索文件夹[默认=收件箱]</param>
        /// <returns>是否成功归档内容更新</returns>
        public static Boolean Archive(ImapConfig Config, String Title, out String Exception, String SearchFolder = "Inbox")
        {
            if (!List(Config, Title, out Dictionary<UniqueId, Content> ContentList, out Exception, SearchFolder)) return false;
            if (!Newest(ContentList, out Content NewestContent, out Exception)) return false;
            if (!Config.GetClient(out ImapClient Client)) { Exception = Config.Exception; return false; }
            if (!GetFolder(Client, SearchFolder, RootFolder(Client), out ImapFolder ContentFolder, out Exception)) return false;
            if (!ExistFolder(ArchiveName, ContentFolder, out Boolean HasArchive, out Exception)) return false;
            ImapFolder ArchiveFolder;
            if (HasArchive) if (!GetFolder(Client, ArchiveName, ContentFolder, out ArchiveFolder, out Exception)) return false; else { }
            else if (!CreateFolder(ArchiveName, ContentFolder, out ArchiveFolder, out Exception)) return false; else { }
            try
            {
                UniqueId NewestUniqueId = new UniqueId();
                foreach (UniqueId ContentUniqueId in ContentList.Keys)
                    if (ContentList[ContentUniqueId].TimeStampValue.Equals(NewestContent.TimeStampValue)) NewestUniqueId = ContentUniqueId;
                ContentList.Remove(NewestUniqueId);
                ContentFolder.Open(FolderAccess.ReadWrite);
                ContentFolder.MoveTo(ContentList.Keys.ToList<UniqueId>(), ArchiveFolder);
                ContentFolder.Close(); Client.Disconnect(true); ContentList.Clear();
            }
            catch (Exception e) { Exception = GetException(e); return false; }
            return true;
        }
    }
}