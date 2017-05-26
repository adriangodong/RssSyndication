using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Rfc822;
using WilderMinds.RssSyndication;
using Xunit;
using DateTime = System.DateTime;

namespace RssSyndication.Tests
{
    public class FeedFacts
    {
        [Fact]
        public void FeedIsCreated()
        {
            var feed = new Feed()
            {
                Title = "Shawn Wildermuth's Blog",
                Description = "My Favorite Rants and Raves",
                Link = new Uri("http://wildermuth.com/feed"),
                Copyright = "(c) 2016"
            };

            Assert.NotNull(feed);
            Assert.True(feed.Title == "Shawn Wildermuth's Blog");
            Assert.True(feed.Description == "My Favorite Rants and Raves");
            Assert.True(feed.Link == new Uri("http://wildermuth.com/feed"));
            Assert.True(feed.Copyright == "(c) 2016");
        }

        Feed CreateTestFeed()
        {
            var feed = new Feed()
            {
                Title = "Shawn Wildermuth's Blog",
                Description = "My Favorite Rants and Raves",
                Link = new Uri("http://wildermuth.com/feed"),
                Copyright = "(c) 2016"
            };

            var item1 = new Item()
            {
                Title = "Foo Bar",
                Body = "<p>Foo bar</p>",
                Link = new Uri("http://foobar.com/item#1"),
                Permalink = "http://foobar.com/item#1",
                PublishDate = DateTime.ParseExact(DateTime.UtcNow.ToString("r"), "r", CultureInfo.InvariantCulture), // RFC 822 / 1123 is precise down to seconds
                Author = new Author() { Name = "Shawn Wildermuth", Email = "shawn@wildermuth.com" }
            };

            item1.Categories.Add("aspnet");
            item1.Categories.Add("foobar");

            item1.Comments = new Uri("http://foobar.com/item1#comments");

            feed.Items.Add(item1);

            var item2 = new Item()
            {
                Title = "Quux",
                Body = "<p>Quux</p>",
                Link = new Uri("http://quux.com/item#1"),
                Permalink = "http://quux.com/item#1",
                PublishDate = DateTime.ParseExact(DateTime.UtcNow.ToString("r"), "r", CultureInfo.InvariantCulture), // RFC 822 / 1123 is precise down to seconds
                Author = new Author() { Name = "Shawn Wildermuth", Email = "shawn@wildermuth.com" }
            };

            item1.Categories.Add("aspnet");
            item1.Categories.Add("quux");

            feed.Items.Add(item2);

            return feed;
        }

        [Fact]
        public void FeedAddsItems()
        {
            var feed = CreateTestFeed();

            Assert.NotNull(feed.Items.First());
            Assert.True(feed.Items.First().Title == "Foo Bar");
            Assert.True(feed.Items.ElementAt(1).Title == "Quux");
            Assert.True(feed.Items.First().Author.Name == "Shawn Wildermuth");
        }

        [Fact]
        public void CreatesValidRss()
        {
            var feed = CreateTestFeed();

            var rss = feed.Serialize();
            Debug.Write(rss);
            var doc = XDocument.Parse(rss);

            Assert.NotNull(doc);
            var item = doc.Descendants("item").FirstOrDefault();
            Assert.NotNull(item);
            Assert.True(item.Element("title").Value == "Foo Bar", "First Item was correct");

        }

        [Fact]
        public void DatesAreProperlyFormatted()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            var feed = CreateTestFeed();
            var rss = feed.Serialize();
            var doc = XDocument.Parse(rss);
            var pubDate = doc.Descendants("pubDate").First();

            var rfc822FormattedDate = feed.Items.First().PublishDate.ToString("r", CultureInfo.InvariantCulture);
            Assert.Equal(rfc822FormattedDate, pubDate.Value);
        }

        [Fact]
        public void ParsesValidRss()
        {
            var expectedFeed = CreateTestFeed();
            var xmlDocument = XDocument.Parse(expectedFeed.Serialize());

            var actualFeed = Feed.Deserialize(xmlDocument);

            Assert.Equal(expectedFeed.Title, actualFeed.Title);
            Assert.Equal(expectedFeed.Link, actualFeed.Link);
            Assert.Equal(expectedFeed.Description, actualFeed.Description);
            Assert.Equal(expectedFeed.Copyright, actualFeed.Copyright);

            Assert.Equal(expectedFeed.Items.Count, actualFeed.Items.Count);

            Assert.Equal(expectedFeed.Items.First().Title, actualFeed.Items.First().Title);
            Assert.Equal(expectedFeed.Items.First().Link, actualFeed.Items.First().Link);
            Assert.Equal(expectedFeed.Items.First().Body, actualFeed.Items.First().Body);
            Assert.Equal(expectedFeed.Items.First().PublishDate, actualFeed.Items.First().PublishDate);

            Assert.Equal(expectedFeed.Items.Last().Title, actualFeed.Items.Last().Title);
            Assert.Equal(expectedFeed.Items.Last().Link, actualFeed.Items.Last().Link);
            Assert.Equal(expectedFeed.Items.Last().Body, actualFeed.Items.Last().Body);
            Assert.Equal(expectedFeed.Items.Last().PublishDate, actualFeed.Items.Last().PublishDate);
        }

    }
}
