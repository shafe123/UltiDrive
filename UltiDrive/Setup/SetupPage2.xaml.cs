using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileManagement;

namespace UltiDrive.Setup
{
    /// <summary>
    /// Interaction logic for SetupPage2.xaml
    /// </summary>
    public partial class SetupPage2 : Page
    {
        public InitialSetup parent;
        public ObservableCollection<StorageInformation> Services { get; set; }
        public ObservableCollection<string> Directories { get; set; }
        public long available { get; set; }
        public long total { get; set; }

        public SetupPage2(InitialSetup parent, ObservableCollection<StorageInformation> services)
        {
            this.parent = parent;
            Directories = new ObservableCollection<string>();
            InitializeComponent();
            Services = services;

            available = 0;
            total = 0;
            foreach (StorageInformation si in services)
            {
                available += si.storageLeft;
                total += si.storageTotal;
            }

            System.Windows.Application.Current.Properties["TotalAvailable"] = available;
            System.Windows.Application.Current.Properties["TotalUsed"] = total-available;

            DataContext = this;
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            if (fb.ShowDialog() == DialogResult.OK)
            {
                Directories.Add(fb.SelectedPath);
            }
        }

        private void RemoveFolder(object sender, RoutedEventArgs e)
        {
            if (SelectedFolders.SelectedItems.Count > 0)
            {
                string[] vals = new string[SelectedFolders.SelectedItems.Count];
                SelectedFolders.SelectedItems.CopyTo(vals, 0);
                foreach (string val in vals)
                {
                    Directories.Remove(val);
                }
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            this.parent.ContentFrame.Navigate(new SetupPage3(parent, Directories, Services));
        }
    }
}
