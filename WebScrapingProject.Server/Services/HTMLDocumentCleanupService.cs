using HtmlAgilityPack;

namespace WebScrapingProject.Server.Services
{
    public class HTMLDocumentCleanupService
    {

        public HtmlDocument ElementCleaning(HtmlDocument articleDoc)
        {
            articleDoc = asideTagCleaning(articleDoc);

            articleDoc = footerTagCleaning(articleDoc);

            articleDoc = commentClassCleaning(articleDoc);

            articleDoc = ilanClassCleaning(articleDoc);

            articleDoc = removingOtherNews(articleDoc);

            return articleDoc;
        }



        public HtmlDocument asideTagCleaning(HtmlDocument articleDoc)
        {
            var asideElements = articleDoc.DocumentNode.SelectNodes("//aside");
            if (asideElements != null)
            {
                foreach (var aside in asideElements)
                {
                    aside.Remove();
                }
            }
            return articleDoc;
        }

        public HtmlDocument commentClassCleaning(HtmlDocument articleDoc)
        {
            var commentNodes = articleDoc.DocumentNode.SelectNodes("//*[contains(@class, 'comment') or contains(@class, 'yorumlar') or contains(@class, 'yorum') or contains(@class, 'comments') ]");
            

            if (commentNodes != null)
            {
                foreach (var commentNode in commentNodes)
                {
                    commentNode.Remove();
                }
            }

            var yorumIdNode = articleDoc.GetElementbyId("yorum");
            if (yorumIdNode != null) yorumIdNode.Remove();

            return articleDoc;
        }

        public HtmlDocument ilanClassCleaning(HtmlDocument articleDoc)
        {
            var ilanNodes = articleDoc.DocumentNode.SelectNodes("//*[contains(@class, 'ilan')]");

            if (ilanNodes != null)
            {
                foreach (var ilanNode in ilanNodes)
                {
                    ilanNode.Remove();
                }
            }

            return articleDoc;
        }

        public HtmlDocument footerTagCleaning(HtmlDocument articleDoc)
        {
            var footerElements = articleDoc.DocumentNode.SelectNodes("//footer");
            if (footerElements != null)
            {
                foreach (var footer in footerElements)
                {
                    footer.Remove();
                }
            }
            return articleDoc;
        }

        public HtmlDocument removingOtherNews (HtmlDocument articleDoc)
        {
            var intrusiveNodes = articleDoc.DocumentNode.SelectNodes("//*[contains(@class, 'benzer') or contains(@class, 'anket') or contains(@class, 'google-auto-placed') or contains(@class, 'data-policy') ]");
            if (intrusiveNodes != null)
            {
                foreach (var node in intrusiveNodes)
                {
                    node.Remove();
                }
            }
            return articleDoc;
        }

        public HtmlDocument sesKocaeliCleanup (HtmlDocument articleDoc)
        {
            var intrusiveNodes = articleDoc.DocumentNode.SelectNodes("//*[contains(@class, 'postblock')]");
            if (intrusiveNodes != null)
            {
                foreach (var node in intrusiveNodes)
                {
                    node.Remove();
                }
            }
            return articleDoc;
        }

    }

}