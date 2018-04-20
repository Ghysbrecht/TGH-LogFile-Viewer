using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TGH_Log_Viewer
{
    public partial class IndexWindow : Window
    {
        List<IndexItem> indexItems, backup;
        LLQueryBuilder queryBuilder;

        public IndexWindow(LLQueryBuilder builder)
        {
            InitializeComponent();
            queryBuilder = builder;
            indexItems = queryBuilder.getAllIndexItems();
            backup = indexItems.ToList();
            indexItemGrid.ItemsSource = indexItems;
        }

        //Listener to right click remove a line
        private void removeLine_Click(object sender, RoutedEventArgs e)
        {
            if (indexItemGrid.SelectedItem != null)
            {
                IndexItem item = indexItemGrid.SelectedItem as IndexItem;
                if (item != null)
                {
                    Console.WriteLine("Removing index: " + item.index);
                    indexItems.Remove(item);
                    refreshIndexList();
                }
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {

            var list = backup.Except(indexItems).ToList();
            list = list.OrderBy(o => o.index).ToList();
            String messageText = "Are you sure you wan to delete these indices? The data will be UNRECOVERABLE!: \n\n";
            foreach (IndexItem item in list) messageText += "   - " + item.index + "\n";
            if (list.Count != 0)
            {
                MessageBoxResult result = MessageBox.Show(messageText, "Index Deletion", MessageBoxButton.OKCancel,MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.OK)
                {
                    deleteIndices(list);
                    this.Close();
                }
                else if(result == MessageBoxResult.Cancel)
                {
                    restoreBackup();
                }
            }
        }

        private void refreshIndexList()
        {
            indexItemGrid.ItemsSource = null;
            indexItemGrid.ItemsSource = indexItems;
        }

        private void clearChangesButton_Click(object sender, RoutedEventArgs e)
        {
            restoreBackup();
        }

        private void restoreBackup()
        {
            indexItems = backup.ToList();
            refreshIndexList();
        }

        private void deleteIndices(List<IndexItem> items)
        {
            foreach(IndexItem item in items)
            {
                Console.WriteLine("DELETING INDEX! -> " + item.index);
                queryBuilder.deleteIndex(item.index);
            }
        }
    }
}
