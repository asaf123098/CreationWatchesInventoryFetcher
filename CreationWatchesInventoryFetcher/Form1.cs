using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace CreationWatchesInventoryFetcher
{
    public partial class Form1 : Form
    {
        private const string HtmlFileName = "xml_viewer.html";
        private HtmlAgilityPack.HtmlDocument document;

        public Form1()
        {
            InitializeComponent();
        }

        private void FormShown(object sender, EventArgs e)
        {
            var th = new Thread(this.parseStock);
            th.Start();
        }

        private void parseStock()
        {
            this.ParseHtml();
            string response = this.Get("http://www.creationwatches.com/products_googlebase.xml");
            List<string[]> xmlNodes = GetParsedXml(response);

            this.label1.Text = "Generating HTML...";
            this.UpdateHtml(xmlNodes);
            System.Diagnostics.Process.Start(HtmlFileName);
            Close();
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
                string condition = itemProperties["g:condition"].InnerText;
                string link = itemProperties["link"].InnerText;
                string price = itemProperties["g:price"].InnerText;
                string sale_price = itemProperties["g:sale_price"].InnerText;
                string quantity = itemProperties["g:quantity"].InnerText;
                string availability = itemProperties["g:availability"].InnerText;
                                                
                string[] arr = new string[] { title, condition, link, price, sale_price, quantity ,availability };
                parsedItemsList.Add(arr);
            }

            return parsedItemsList;
        }

        private void UpdateHtml(List<string[]> itemsList)
        {
            HtmlNode accordion = this.FindNodes("//div[@id='accordion']")[0];
            accordion.RemoveAllChildren();

            for (int i = 0; i < itemsList.Count; i++)
            {
                string[] properties = itemsList[i];
                //Create the card with the header and collapsable part
                HtmlNode card = HtmlNode.CreateNode("<div class='card'></div>");
                HtmlNode header = HtmlNode.CreateNode($"<div class='card-header' id='heading-{i}'></div>");
                HtmlNode collapsable = HtmlNode.CreateNode($"<div id='collapse-{i}' class='collapse' " +
                    $"aria-labelledby='heading{i}' data-parent='#accordion'></div>");

                HtmlNode h2 = HtmlNode.CreateNode($"<h2'></h2>");

                HtmlNode buttonTag = HtmlNode.CreateNode($"<button class='btn btn-link' type='button' data-toggle='collapse' " +
                    $"data-target='#collapse-{i}' aria-expanded='false' aria-controls='collapse-{i}'>{properties[0]}</button>");

                //Set the condition badge
                string condition = properties[1];
                string conditionState = condition == "new" ? "primary" : "warning";
                HtmlNode conditionTag = HtmlNode.CreateNode($"<span class='badge badge-{conditionState} ml-2'>{condition}</span>");

                //Set the availability badge
                string availability = properties.Last();
                string availabilityState = availability == "in stock" ? "success" : "danger";
                HtmlNode availabilityTag = HtmlNode.CreateNode($"<span class='badge badge-{availabilityState} ml-2'>{availability}</span>");

                //Set the item's body
                HtmlNode cardBodyTag = HtmlNode.CreateNode($"<div class='card-body'></div");
                HtmlNode listTag = HtmlNode.CreateNode($"<ul class='list-group list-group-horizontal-sm mb-4'></ul>");

                HtmlNode priceTag = HtmlNode.CreateNode($"<li class='list-group-item'>" +
                    $"<h5>Price:</h5><del><strong>{properties[3]}</strong></del> {properties[4]}</li>");

                HtmlNode quantityTag = HtmlNode.CreateNode($"<li class='list-group-item'><h5>Quantity:</h5>" +
                    $"{properties[5]}</li>");

                HtmlNode linkTag = HtmlNode.CreateNode($"<a href='{properties[2]}' type='button' class='btn btn-outline-info'>Product Info</a>");

                //Append all the children
                accordion.AppendChild(card);

                card.AppendChild(header);
                card.AppendChild(collapsable);

                header.AppendChild(h2);

                h2.AppendChild(buttonTag);
                buttonTag.AppendChild(conditionTag);
                buttonTag.AppendChild(availabilityTag);


                collapsable.AppendChild(cardBodyTag);
                cardBodyTag.AppendChild(listTag);
                cardBodyTag.AppendChild(linkTag);

                listTag.AppendChild(priceTag);
                listTag.AppendChild(quantityTag);
            }
            this.document.Save(HtmlFileName);
        }

        private void ParseHtml()
        {
            try
            {
                this.document = new HtmlAgilityPack.HtmlDocument();
                this.document.Load(HtmlFileName);
            }
            catch (FileNotFoundException)
            {
                this.RaiseAlert($"Failed to find '{HtmlFileName}' in the directory!!!");
                this.Close();
            }
        }
        private void RaiseAlert(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private HtmlNodeCollection FindNodes(string xpath, bool raiseException = true)
        {
            HtmlNodeCollection nodes = this.document.DocumentNode.SelectNodes(xpath);

            if (nodes != null)
                return nodes;
            else if (raiseException)
            {
                this.RaiseAlert($"Failed to find xpath: '{xpath}'");
                Application.Exit();
            }
            return null;
        }
    }


}
