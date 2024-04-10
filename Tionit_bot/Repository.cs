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
    internal class Repository
    {
        public string FileName = "Chats.xml";
        public Repository() { }
        public void Save(List<Info> chats)
        {
            // Создание документа XML
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("Strings");
            xmlDoc.AppendChild(root);

            foreach (Info chat in chats)
            {
                XmlElement pairElement = xmlDoc.CreateElement("Pair");

                XmlElement stringElement = xmlDoc.CreateElement("Username");
                stringElement.InnerText = chat.Username;
                pairElement.AppendChild(stringElement);
                Console.WriteLine("Created " + stringElement.InnerText);

                XmlElement intElement = xmlDoc.CreateElement("TheshId");
                intElement.InnerText = chat.TheshId.ToString();
                pairElement.AppendChild(intElement);
                Console.WriteLine("Created " + intElement.InnerText);

                XmlElement IdElement = xmlDoc.CreateElement("ChatId");
                IdElement.InnerText = chat.ChatId.ToString();
                pairElement.AppendChild(IdElement);


                Console.WriteLine("Created " + IdElement.InnerText);

                root.AppendChild(pairElement);
            }
            xmlDoc.Save(FileName);
        }
        public List<Info> GetChats()
        {
            XDocument xmlDoc = XDocument.Load(FileName);

            List<Info> pairs = new List<Info>();
            // Извлекаем данные из XML-документа
            foreach (XElement pairElement in xmlDoc.Root.Elements("Pair"))
            {
                string Name = pairElement.Element("Username").Value;
                int Theshid = int.Parse(pairElement.Element("TheshId").Value);
                int ChatId= int.Parse(pairElement.Element("ChatId").Value);
                pairs.Add(new Info { TheshId = Theshid, ChatId = ChatId, Username = Name });
            }
            return pairs;

        }
    }
}
