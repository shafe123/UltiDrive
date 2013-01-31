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
            SaveGetSettings(false, "SOWS");
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
            SaveGetSettings(true, "SOWS");
            this.Close();
        }

        private void SaveGetSettings(bool blnSave, string strKeyName)
        {
            RegistryKey RegKey;
            string strKeyVal = "";
            try
            {
                // Check to see if reg key exists
                RegKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Run", true);


                if (RegKey == null)
                {
                    return; // exit the procedure - It should be there!
                }


                // Do save or get
                //==============================================================
                switch (blnSave)
                {
                    case (true):
                        {
                            switch (strKeyName)
                            {
                                case ("SOWS"):
                                    {
                                        string AppExec = System.Reflection.Assembly.GetExecutingAssembly().Location;
                                        if (chkStartOnWSU.IsChecked == true)
                                        {
                                            RegKey.SetValue(strKeyName, AppExec);
                                        }
                                        else
                                        {
                                            RegKey.DeleteValue(strKeyName, false);
                                        }
                                        break;
                                    }
                            }
                            break;

                        }

                    case (false):
                        {
                            switch (strKeyName)
                            {
                                case ("SOWS"):
                                    {
                                        strKeyVal = Convert.ToString(RegKey.GetValue(strKeyName));
                                        if (strKeyVal == "")
                                        {
                                            chkStartOnWSU.IsChecked = false;
                                        }
                                        else
                                        {
                                            chkStartOnWSU.IsChecked = true;
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                }
                //    //==============================================================
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnResetAll_Click(object sender, RoutedEventArgs e)
        {
            if (FancyMsgBoxWindow.Show("Are you sure?", 
                "Are you sure you want to disconnect from all services?") == true)
            {
                //UbuntuOne.Api.Logout();

                SkyDrive.Api.Logout();

                GoogleDrive.GoogleDriveRefreshInfo.Instance = null;

                DropboxApi.Api.AccessToken = null;

                System.IO.Directory.Delete(App.AppFolder, true);
            }
        }
    }
}
