using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileManagement;

namespace UltiDrive.Setup
{
    /// <summary>
    /// Interaction logic for SetupPage3.xaml
    /// </summary>
    public partial class SetupPage3 : Page
    {
        InitialSetup parent;
        List<string> directories;

        public SetupPage3(InitialSetup parent,
                        ObservableCollection<string> Directories, 
                        ObservableCollection<StorageInformation> Services)
        {
            this.parent = parent;
            InitializeComponent();

            directories = Directories.ToList<string>();
            List<StorageInformation> info = Services.ToList<StorageInformation>();

            App.initialize = Task.Factory.StartNew(() => new FileStructure(directories, info));

            foreach (StorageInformation service in Services)
            {
                App.TotalAvailable += service.storageTotal;
                App.TotalUsed += service.storageTotal - service.storageLeft;
            }

            App.initialize.Wait();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new wSettings().Show();
            this.parent.Close();
        }
    }
}
