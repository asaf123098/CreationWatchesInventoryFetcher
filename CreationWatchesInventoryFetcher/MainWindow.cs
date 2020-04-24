using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CreationWatchesInventoryFetcher
{
    public partial class MainWindow : Form
    {
        private XmlDocument xmlDoc;
        private const string XmlFileName = "listed_files.xml";

        public MainWindow()
        {
            InitializeComponent();
        }  

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.LoadXml();
            this.AddXmlItemsToList();
        }

        private void LoadXml()
        {
            try
            {
                this.xmlDoc = new XmlDocument();
                this.xmlDoc.Load(XmlFileName);
            }
            catch (FileNotFoundException)
            {
                Generals.RaiseAlert($"Can't find the Xml file!!! ({XmlFileName})");
                Close();
            }
        }

        private void AddXmlItemsToList()
        {

            foreach (XmlNode item in this.xmlDoc.GetElementsByTagName("item"))
            {
                this.AddXmlItemToList(item);
            }
        }

        private void AddXmlItemToList(XmlNode item)
        {
            string itemName = item.SelectSingleNode("./name").InnerText;
            string itemLink = item.SelectSingleNode("./link").InnerText;
            string itemId = item.SelectSingleNode("./id").InnerText;

            ListViewItem listItem = new ListViewItem(itemName);
            listItem.SubItems.Add(itemLink);
            listItem.SubItems.Add(itemId);

            this.ListedItemsList.Items.Add(listItem);
            this.ListedItemsList.Columns[0].Width = -1; // To make sure the column will be sized by the longest item.
        }

        private void ListedItemsList_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            ListViewItem item = this.ListedItemsList.GetItemAt(e.X, e.Y);
            string link = item.SubItems[1].Text;
            Generals.OpenLink(link);
        }

        private void GenerateHtmlButton_Click(object sender, EventArgs e)
        {
            List<string[]> ListedItems = this.GetListedItems();
            new StockHtmlBuilder(ListedItems).Show();
        }

        /// <summary>
        /// This method parses the listed items as list
        /// </summary>
        /// <returns> List of string arrays { itemName, itemLink, itemId }</returns>

        private List<string[]> GetListedItems()
        {
            List<string[]> ListedItems = new List<string[]>();
            foreach (XmlNode item in this.xmlDoc.GetElementsByTagName("item"))
            {
                string itemName = item.SelectSingleNode("./name").InnerText;
                string itemLink = item.SelectSingleNode("./link").InnerText;
                string itemId = item.SelectSingleNode("./id").InnerText;

                ListedItems.Add(new string[] { itemName, itemLink, itemId });
            }
            return ListedItems;
        }

        private void AddNewItemButton_Click(object sender, EventArgs e)
        {
            XmlNode item = this.xmlDoc.CreateElement("item");
            
            XmlNode name = this.xmlDoc.CreateElement("name");
            name.InnerText = this.NameTextBox.Text;
            this.NameTextBox.Text = "";

            XmlNode link = this.xmlDoc.CreateElement("link");
            link.InnerText = this.LinkTextBox.Text;
            this.LinkTextBox.Text = "";

            XmlNode id = this.xmlDoc.CreateElement("id");
            id.InnerText = this.IdTextBox.Text.Trim(new char[]  {' '});
            this.IdTextBox.Text = "";

            item.AppendChild(name);
            item.AppendChild(link);
            item.AppendChild(id);

            this.AddXmlItemToList(item);
            this.xmlDoc.GetElementsByTagName("items")[0].AppendChild(item);
            this.xmlDoc.Save(XmlFileName);
        }

        private void ListedItemsList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (ListedItemsList.FocusedItem.Bounds.Contains(e.Location))
                {
                    DeleteContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void ItemDeleteClicked(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedItems = ListedItemsList.SelectedItems;

            foreach (ListViewItem selectedItem in selectedItems)
            {
                this.ListedItemsList.Items.Remove(selectedItem);

                foreach (XmlNode item in this.xmlDoc.GetElementsByTagName("item"))
                {
                    string itemId = item.SelectSingleNode("./id").InnerText;

                    if (itemId == selectedItem.SubItems[2].Text)
                    {
                        this.xmlDoc.GetElementsByTagName("items")[0].RemoveChild(item);
                        break;
                    }
                }
            }            
            this.xmlDoc.Save(XmlFileName);
        }
    }
}
