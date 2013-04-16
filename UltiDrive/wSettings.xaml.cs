using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using UltiDrive.Dropbox.Api;
using FileManagement;
using System.IO;
using System.Windows.Forms;
using UltiDrive.Dropbox;
using OAuthProtocol;

namespace UltiDrive
{
    /// <summary>
    /// Interaction logic for wSettings.xaml
    /// </summary>
    public partial class wSettings : Window
    {
        public wSettings()
        {
            InitializeComponent();

            lstRootFolders.ItemsSource = App.FolderStructure.IndexRoots.Select(r => r.RootFolderName);

            cbSkydrive.IsChecked = App.FolderStructure.algo.Services.Count(s => s.service == StorageServices.SkyDrive) == 1;
            cbGoogleDrive.IsChecked = App.FolderStructure.algo.Services.Count(s => s.service == StorageServices.GoogleDrive) == 1;
            cbDropbox.IsChecked = App.FolderStructure.algo.Services.Count(s => s.service == StorageServices.Dropbox) == 1;
            cbBox.IsChecked = App.FolderStructure.algo.Services.Count(s => s.service == StorageServices.Box) == 1;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnResetAll_Click(object sender, RoutedEventArgs e)
        {
            if (FancyMsgBoxWindow.Show("Are you sure?", 
                "Are you sure you want to disconnect from all services?") == true)
            {
                //UbuntuOne.Api.Logout();
                SkyDrive.Api.Logout();
                GoogleDrive.Api.Logout();

                DropboxApi.Api.AccessToken = null;
                Directory.Delete(App.AppFolder, true);
            }
        }

        private void chkStartOnWSU_Checked(object sender, RoutedEventArgs e)
        {
            string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            vbAccelerator.Components.Shell.ShellLink link = new vbAccelerator.Components.Shell.ShellLink();
            link.Target = location;
            link.DisplayMode = vbAccelerator.Components.Shell.ShellLink.LinkDisplayMode.edmNormal;
            link.Save(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\UltiDrive.lnk");
        }

        private void chkStartOnWSU_Unchecked(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\UltiDrive.lnk"))
                System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\UltiDrive.lnk");
        }

        private void RemRoot_Click(object sender, RoutedEventArgs e)
        {
            if (lstRootFolders.SelectedItems.Count > 0)
            {
                foreach (string dir in lstRootFolders.SelectedItems)
                    App.FolderStructure.RemoveRootFolder(dir);
            }

            lstRootFolders.ItemsSource = App.FolderStructure.IndexRoots.Select(r => r.RootFolderName);
        }

        private void AddRoot_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            fb.RootFolder = Environment.SpecialFolder.MyComputer;

            if (fb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                App.FolderStructure.AddRootFolder(fb.SelectedPath);
            }

            lstRootFolders.ItemsSource = App.FolderStructure.IndexRoots.Select(r => r.RootFolderName);
        }

        private void cbSkydrive_Checked(object sender, RoutedEventArgs e)
        {
            SkyDrive.Api.Login(false);
            App.FolderStructure.algo.AddService(StorageServices.SkyDrive);
        }

        private void cbGoogleDrive_Checked(object sender, RoutedEventArgs e)
        {
            GoogleDrive.Api.Login(false);
            App.FolderStructure.algo.AddService(StorageServices.GoogleDrive);
        }

        private void cbDropbox_Checked(object sender, RoutedEventArgs e)
        {
            Request request = new Dropbox.Request();
            OAuthToken accessToken = request.GetAccessToken();
            DropboxApi.Api.AccessToken = accessToken;
            Utilities.WriteDropboxSettings();

            App.FolderStructure.algo.AddService(StorageServices.Dropbox);
        }

        private void cbBox_Checked(object sender, RoutedEventArgs e)
        {
            Box.Api.Login(false);
            App.FolderStructure.algo.AddService(StorageServices.Box);
        }

        private void cbSkydrive_Unchecked(object sender, RoutedEventArgs e)
        {
            SkyDrive.Api.Logout();
            App.FolderStructure.algo.RemoveService(StorageServices.SkyDrive);
        }

        private void cbGoogleDrive_Unchecked(object sender, RoutedEventArgs e)
        {
            GoogleDrive.Api.Logout();
            App.FolderStructure.algo.RemoveService(StorageServices.GoogleDrive);
        }

        private void cbDropbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Dropbox.DropboxProperties.Properties.accessToken = null;
            App.FolderStructure.algo.RemoveService(StorageServices.Dropbox);
        }

        private void cbBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Box.BoxProperties.refreshInfo.access_token = null;
            App.FolderStructure.algo.RemoveService(StorageServices.Box);
        }


    }
}
