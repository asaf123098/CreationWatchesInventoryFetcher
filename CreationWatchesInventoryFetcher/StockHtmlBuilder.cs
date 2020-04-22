using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CreationWatchesInventoryFetcher
{
    public partial class StockHtmlBuilder : Form
    {
        private HtmlHandler HtmlHandler;
        private List<string[]> ListedItems;

        public StockHtmlBuilder(List<string[]> ListedItems)
        {
            InitializeComponent();
            this.ListedItems = ListedItems;
        }

        private void FormShown(object sender, EventArgs e)
        {
            var th = new Thread(this.parseStock);
            th.Start();
            th.Join();
            Close();
        }

        private void parseStock()
        {
            string response = this.Get("http://www.creationwatches.com/products_googlebase.xml");
            List<string[]> xmlNodes = GetParsedXml(response);
            this.ParseHtml();
            this.HtmlHandler.UpdateStock(xmlNodes);
            this.HtmlHandler.AddListedItems(this.ListedItems);
            Generals.OpenLink(HtmlHandler.HtmlFileName);
        }

        private void ParseHtml()
        {
            try
            {
                this.HtmlHandler = new HtmlHandler();
            }
            catch (FileNotFoundException)
            {
                Generals.RaiseAlert($"Failed to find '{HtmlHandler.HtmlFileName}' in the directory!!!");
                this.Close();
            }
        }

        private string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// This method parses the xml into a list of strings
        /// </summary>
        /// <param name="xml">The stock xml as string</param>
        /// <returns>List of string arrays representing each item</returns>
        private List<string[]> GetParsedXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNodeList itemTags = doc.GetElementsByTagName("item");
            List<string[]> parsedItemsList = new List<string[]> {};

            for (int i=0; i<itemTags.Count; i++)
            {
                XmlNode itemProperties = itemTags[i];
                string title = itemProperties["title"].InnerText;
                string id = itemProperties["g:id"].InnerText;
                string condition = itemProperties["g:condition"].InnerText;
                string link = itemProperties["link"].InnerText;
                string price = itemProperties["g:price"].InnerText;
                string sale_price = itemProperties["g:sale_price"].InnerText;
                string quantity = itemProperties["g:quantity"].InnerText;
                string availability = itemProperties["g:availability"].InnerText;
                                                
                string[] arr = new string[] { title, id, condition, link, price, sale_price, quantity ,availability };
                parsedItemsList.Add(arr);
            }

            return parsedItemsList;
        }        
    }
}
