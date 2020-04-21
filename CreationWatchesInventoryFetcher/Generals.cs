using System.Windows.Forms;
using HtmlAgilityPack;

namespace CreationWatchesInventoryFetcher
{
    class Generals
    { 
        public static void RaiseAlert(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static HtmlNodeCollection FindNodes(HtmlAgilityPack.HtmlDocument document, string xpath, bool raiseException = true)
        {
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes(xpath);

            if (nodes == null && raiseException)            
            {
                RaiseAlert($"Failed to find xpath: '{xpath}'");
                Application.Exit();
            }
            return nodes;
        }

        public static void OpenLink(string link)
        {
            System.Diagnostics.Process.Start(link);
        }
    }
}
