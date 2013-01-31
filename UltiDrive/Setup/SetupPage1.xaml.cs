using FileManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UltiDrive.Dropbox;
using OAuthProtocol;
using UltiDrive.Dropbox.Api;
using UltiDrive.SkyDrive;
using UltiDrive.GoogleDrive;

namespace UltiDrive.Setup
{
    /// <summary>
    /// Interaction logic for SetupPage1.xaml
    /// </summary>
    public partial class SetupPage1 : Page
    {
        InitialSetup parent;
        public ObservableCollection<StorageServices> selectedServices { get; set; }

        public SetupPage1(InitialSetup Parent)
        {
            this.parent = Parent;
            selectedServices = new ObservableCollection<StorageServices>();
            this.DataContext = selectedServices;

            InitializeComponent();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<StorageInformation> services = new ObservableCollection<StorageInformation>();
            parent.DataContext = selectedServices;

            foreach (StorageServices service in selectedServices)
            {
                switch (service)
                {
                    case StorageServices.SkyDrive:
                        LoginPage.Site = StorageServices.SkyDrive;
                        LoginPage.SigningIn = true;
                        LoginPage newWindow = new LoginPage();
                        newWindow.ShowDialog();

                        SkyDrive.Utilities.WriteSkyDriveSettings();

                        if (SkyDrive.Properties.LoggedIn)
                        {
                            SkyDrive.Properties.Quota qt = SkyDrive.Api.GetQuota();
                            services.Add(new StorageInformation(StorageServices.SkyDrive,
                                qt.quota - qt.available, qt.quota));

                            //INITIALIZE SKYDRIVE
                            SkyDrive.Utilities.checkSkyDriveInitiatedStart(null, null);
                            //END INITIALIZE SKYDRIVE
                        }
                        break;
                    //case StorageServices.UbuntuOne:
                    //    new UbuntuOne.UbuntuOneSignInWindow().ShowDialog();

                    //    if (UbuntuOne.RefreshInfo.UbuntuOneInfo.LoggedIn)
                    //    {
                    //        UbuntuOne.Quota qt = UbuntuOne.Api.GetQuota();
                    //        services.Add(new StorageInformation(StorageServices.UbuntuOne,
                    //            qt.used, qt.total));
                    //    }
                    //    break;
                    case StorageServices.GoogleDrive:
                        LoginPage.Site = StorageServices.GoogleDrive;
                        LoginPage.SigningIn = true;
                        LoginPage newWindow2 = new LoginPage();
                        newWindow2.ShowDialog();

                        if (GoogleDriveRefreshInfo.Instance.LoggedIn)
                        {
                            GoogleDrive.Quota qt = GoogleDrive.Api.GetQuota();
                            services.Add(new StorageInformation(StorageServices.GoogleDrive,
                                qt.quotaBytesUsed, qt.quotaBytesTotal));
                        }
                        break;
                    case StorageServices.Dropbox:
                        //DropboxApi.Api.Login();
                        Request request = new Dropbox.Request();
                        OAuthToken accessToken = request.GetAccessToken();
                        DropboxApi.Api.AccessToken = accessToken;
                        Utilities.WriteDropboxSettings();

                        if (DropboxApi.Api.AccessToken != null)
                        {
                            Dropbox.Api.Quota qt = DropboxApi.Api.GetAccountInfo().Quota;
                            services.Add(new StorageInformation(StorageServices.Dropbox,
                                qt.Normal + qt.Shared, qt.Total));
                        }
                        break;
                    case StorageServices.Box:
                        Box.Api.Login(true);

                        if (Box.BoxProperties.LoggedIn)
                        {
                            Box.Quota qt = Box.Api.GetQuota();
                            services.Add(new StorageInformation(StorageServices.Box,
                                qt.space_used, qt.space_amount));
                        }
                        break;
                    default:
                        break;
                }
            }
            SetupPage2 next = new SetupPage2(parent, services);
            this.parent.ContentFrame.Navigate(next);
        }

        private void cbSkyDrive_Checked(object sender, RoutedEventArgs e)
        {
            if (cbSkyDrive.IsChecked==true)
                selectedServices.Add(StorageServices.SkyDrive);
            else
                selectedServices.Remove(StorageServices.SkyDrive);
        }

        private void cbGoogleDrive_Checked(object sender, RoutedEventArgs e)
        {
            if (cbGoogleDrive.IsChecked == true)
                selectedServices.Add(StorageServices.GoogleDrive);
            else
                selectedServices.Remove(StorageServices.GoogleDrive);
        }

        //private void cbUbuntuOne_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (cbUbuntuOne.IsChecked == true)
        //        selectedServices.Add(StorageServices.UbuntuOne);
        //    else
        //        selectedServices.Remove(StorageServices.UbuntuOne);
        //}

        private void cbDropbox_Checked(object sender, RoutedEventArgs e)
        {
            if (cbDropbox.IsChecked == true)
                selectedServices.Add(StorageServices.Dropbox);
            else
                selectedServices.Remove(StorageServices.Dropbox);
        }

        private void cbBox_Checked(object sender, RoutedEventArgs e)
        {
            if (cbBox.IsChecked == true)
                selectedServices.Add(StorageServices.Box);
            else
                selectedServices.Remove(StorageServices.Box);
        }
    }
}
