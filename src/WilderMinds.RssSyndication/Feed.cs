using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Rfc822;
using DateTime = System.DateTime;

namespace WilderMinds.RssSyndication
{
    public class Feed
    {
        public static readonly string[] Rfc822DateTimeFormatStrings =
        {
            "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'",
            "ddd, dd MMM yyyy HH':'mm':'ss 'UT'",
            "ddd, dd MMM yyyy HH':'mm':'ss 'EST'",
            "ddd, dd MMM yyyy HH':'mm':'ss 'EDT'",
            "ddd, dd MMM yyyy HH':'mm':'ss 'CST'",
            "ddd, dd MMM yyyy HH':'mm':'ss 'CDT'",
            "ddd, dd MMM yyyy HH':'mm':'ss 'MST'",
            "ddd, dd MMM yyyy HH':'mm':'ss 'MDT'",
            "ddd, dd MMM yyyy HH':'mm':'ss 'PST'",
            "ddd, dd MMM yyyy HH':'mm':'ss 'PDT'",
        };

        public string Description { get; set; }
        public Uri Link { get; set; }
        public string Title { get; set; }
        public string Copyright { get; set; }

        public ICollection<Item> Items { get; set; } = new List<Item>();

        public static Feed GetFromXml()
        {
            var feed = new Feed();
            return feed;
        }

        public string Serialize()
        {
            var doc = new XDocument(new XElement("rss"));
            doc.Root.Add(new XAttribute("version", "2.0"));

            var channel = new XElement("channel");
            channel.Add(new XElement("title", this.Title));
            channel.Add(new XElement("link", this.Link.AbsoluteUri));
            channel.Add(new XElement("description", this.Description));
            channel.Add(new XElement("copyright", this.Copyright));
            doc.Root.Add(channel);

            foreach (var item in Items)
            {
                var itemElement = new XElement("item");
                itemElement.Add(new XElement("title", item.Title));
                itemElement.Add(new XElement("link", item.Link.AbsoluteUri));
                itemElement.Add(new XElement("description", item.Body));
                if (item.Author != null) itemElement.Add(new XElement("author", $"{item.Author.Email} ({item.Author.Name})"));
                foreach (var c in item.Categories) itemElement.Add(new XElement("category", c));
                if (item.Comments != null) itemElement.Add(new XElement("comments", item.Comments.AbsoluteUri));
                if (!string.IsNullOrWhiteSpace(item.Permalink)) itemElement.Add(new XElement("guid", item.Permalink));
                var dateFmt = item.PublishDate.ToString("r");
                if (item.PublishDate != DateTime.MinValue) itemElement.Add(new XElement("pubDate", dateFmt));
                channel.Add(itemElement);
            }

            return doc.ToString();

        }

        public static Feed Deserialize(XDocument xmlDocument)
        {
            var feed = new Feed();

            var channelNode = xmlDocument.Root?.Elements().FirstOrDefault(node => node.Name == "channel");
            if (channelNode != null)
            {
                foreach (var channelChildNode in channelNode.Elements())
                {
                    if (channelChildNode.Name.NamespaceName != string.Empty)
                    {
                        continue;
                    }

                    switch (channelChildNode.Name.LocalName)
                    {
                        case "title":
                            feed.Title = channelChildNode.Value;
                            break;
                        case "link":
                            feed.Link = new Uri(channelChildNode.Value);
                            break;
                        case "description":
                            feed.Description = channelChildNode.Value;
                            break;
                        case "copyright":
                            feed.Copyright = channelChildNode.Value;
                            break;
                        case "item":
                            var item = new Item();

                            foreach (var itemChildNode in channelChildNode.Elements())
                            {
                                switch (itemChildNode.Name.LocalName)
                                {
                                    case "title":
                                        item.Title = itemChildNode.Value;
                                        break;
                                    case "link":
                                        item.Link = new Uri(itemChildNode.Value);
                                        break;
                                    case "description":
                                        item.Body = itemChildNode.Value;
                                        break;
                                    case "pubDate":
                                        item.PublishDate = new Rfc822.DateTime(itemChildNode.Value, DateTimeSyntax.FourDigitYear | DateTimeSyntax.WithDayName | DateTimeSyntax.WithSeconds | DateTimeSyntax.WithDayName).Instant.UtcDateTime;
                                        break;
                                }
                            }

                            feed.Items.Add(item);
                            break;
                    }
                }
            }

            return feed;
        }

    }
}
