using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace CreationWatchesInventoryFetcher
{
    class HtmlHandler
    {
        public const string HtmlFileName = @"xml_viewer.html";
        private readonly HtmlDocument document;

        public HtmlHandler()
        {
            this.document = new HtmlDocument();
            this.document.Load(HtmlFileName);
        }

        public void AddListedItems(List<string[]> ListedItems)
        {
            bool foundOutOfStockItems = false;
            string alertState;
            
            HtmlNode outOfStockContainer = Generals.FindNodes(this.document, "//div[@id='nav-out-of-stock']")[0];
            HtmlNode inStockContainer = Generals.FindNodes(this.document, "//div[@id='nav-in-stock']")[0];
            ref HtmlNode nodeToAppendTo = ref outOfStockContainer;

            outOfStockContainer.RemoveAllChildren();
            inStockContainer.RemoveAllChildren();

            foreach (string[] listedItem in ListedItems)
            {
                HtmlNodeCollection matchingIdItems = 
                    Generals.FindNodes(this.document, $"//div[@class='card' and @id='{listedItem[2]}']", false);

                if (matchingIdItems is null)
                {
                    alertState = "danger";
                    nodeToAppendTo = ref outOfStockContainer;
                    foundOutOfStockItems = true;
                }
                else
                {
                    alertState = "success";
                    nodeToAppendTo = ref inStockContainer;
                }

                HtmlNode div = HtmlNode.CreateNode($"<div class='alert alert-{alertState} mt-3' role='alert'>" +
                        $"<a href='{listedItem[1]}' class='alert-link'>Item</a> {listedItem[0]}</div>");
                nodeToAppendTo.AppendChild(div);
            }
            if (!foundOutOfStockItems)
            {
                outOfStockContainer.InnerHtml = "No items are out of stock.";
            }
            this.document.Save(HtmlFileName);
        }

        public void UpdateStock(List<string[]> itemsList)
        {
            HtmlNode accordion = Generals.FindNodes(this.document, "//div[@id='accordion']")[0];
            accordion.RemoveAllChildren();

            for (int i = 0; i < itemsList.Count; i++)
            {
                //Create the card with the header and collapsable part
                HtmlNode card = this.GetCardElement(i, itemsList[i]);
                accordion.AppendChild(card);
            }
            this.document.Save(HtmlFileName);
        }

        /// <summary>This method creates an HTML element by given properties</summary>
        /// <param name="index">The index for the card element inside the list</param>
        /// <param name="properties">Array of strings that should look like: 
        /// { title, id, condition, link, price, sale_price, quantity ,availability]</param>
        private HtmlNode GetCardElement(int index, string[] properties)
        {
            HtmlNode card = HtmlNode.CreateNode($"<div id='{properties[1]}' class='card'></div>");
            HtmlNode header = HtmlNode.CreateNode($"<div class='card-header' id='heading-{index}'></div>");
            HtmlNode collapsable = HtmlNode.CreateNode($"<div id='collapse-{index}' class='collapse' " +
                $"aria-labelledby='heading{index}' data-parent='#accordion'></div>");

            HtmlNode h2 = HtmlNode.CreateNode($"<h2></h2>");

            HtmlNode buttonTag = HtmlNode.CreateNode($"<button class='btn btn-link' type='button' data-toggle='collapse' " +
                $"data-target='#collapse-{index}' aria-expanded='false' aria-controls='collapse-{index}'>{properties[0]}</button>");

            //Set the condition badge
            string condition = properties[2];
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
                $"<h5>Price:</h5><del><strong>{properties[4]}</strong></del> {properties[5]}</li>");

            HtmlNode quantityTag = HtmlNode.CreateNode($"<li class='list-group-item'><h5>Quantity:</h5>" +
                $"{properties[6]}</li>");

            HtmlNode linkTag = HtmlNode.CreateNode($"<a href='{properties[3]}' type='button' class='btn btn-outline-info'>Product Info</a>");

            //Append all the children

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

            return card;
        }
    }
}
