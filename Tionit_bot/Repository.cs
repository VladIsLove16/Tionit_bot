using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Telegram.Bot.Types;

namespace Tionit_bot
{
    public static class Repository
    {
        public static string FileName = "D:\\Projects\\C#\\Tionit_bot\\Tionit_bot\\Chats.xml";
        private static string rootString = "Root";
        private static string chatsInfoRootString = "ChatsInfo";
        private static string usernameString = "Username";
        private static string threadIDString = "ThreadID";
        private static string chatIDString = "ChatID";
        public static void SaveChats(List<Info> chatsInfo)
        {
            try { 
            // Создание документа XML
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement(rootString);
            xmlDoc.AppendChild(root);

            foreach (Info chatInfo in chatsInfo)
            {
                XmlElement ChatsInfoElement = xmlDoc.CreateElement(chatsInfoRootString);

                XmlElement usernameElement = xmlDoc.CreateElement(usernameString);
                usernameElement.InnerText = chatInfo.Username;
                ChatsInfoElement.AppendChild(usernameElement);
                Console.WriteLine("Created " + usernameElement.InnerText);

                XmlElement ThreadIdElement = xmlDoc.CreateElement(threadIDString);
                ThreadIdElement.InnerText = chatInfo.ThreadID.ToString();
                ChatsInfoElement.AppendChild(ThreadIdElement);
                Console.WriteLine("Created " + ThreadIdElement.InnerText);

                XmlElement userIdElement = xmlDoc.CreateElement(chatIDString);
                userIdElement.InnerText = chatInfo.ChatID.ToString();
                ChatsInfoElement.AppendChild(userIdElement);
                Console.WriteLine("Created " + userIdElement.InnerText);

                 root.AppendChild(ChatsInfoElement);
            }
            xmlDoc.Save(FileName);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
        public static List<Info>? GetChats()
        {
            try
            {
                XDocument xmlDoc = XDocument.Load(FileName);
                List<Info> ChatsInfo = new List<Info>();
                // Извлекаем данные из XML-документа
                foreach (XElement ChatInfo in xmlDoc.Root.Elements(chatsInfoRootString))
                {
                    string username = ChatInfo.Element(usernameString).Value;
                    int threadID = int.Parse(ChatInfo.Element(threadIDString).Value);
                    int chatID= int.Parse(ChatInfo.Element(chatIDString).Value);
                    ChatsInfo.Add(new Info { ThreadID = threadID, ChatID = chatID, Username = username });
                }
                Console.WriteLine("Список чатов получен");

                return ChatsInfo;
            }
            catch  (Exception e)
            {
                Console.WriteLine("Неудачная попытка сохранения");
                Console.WriteLine(e.Message);
                return null;
            }


        }
    }
}
