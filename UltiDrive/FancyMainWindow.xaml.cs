using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Input;
using FileManagement;
using Mjollnir.Live;
using UltiDrive.Dropbox;
using UltiDrive.SkyDrive;
using OAuthProtocol;
using UltiDrive.Dropbox.Api;
using Microsoft.Win32;
using UltiDrive.FileManagement;
using UltiDrive.GoogleDrive;
using System.IO;

namespace UltiDrive
{
    /// <summary>
    /// Interaction logic for FancyMainWindow.xaml
    /// </summary>
    public partial class FancyMainWindow : Window
    {
        public static StorageServices Site;
        private System.Windows.Forms.NotifyIcon nIcon;

        public FancyMainWindow()
        {
            InitializeComponent();
            //InitializeUbuntu();
            InitializeSkyDrive();

            List<string> directories = new List<string>();
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\UltiDrive\\Watcher.xml"))
            {
                string xml = System.IO.File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\UltiDrive\\Watcher.xml");
                StringReader reader = new StringReader(xml);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
                directories = (List<string>)serializer.Deserialize(reader);
            }

            InitializeWatchers(directories);

            nIcon = new System.Windows.Forms.NotifyIcon();
            nIcon.Icon = new System.Drawing.Icon("..\\..\\Looks\\IDImg.ico");
            nIcon.DoubleClick += nIcon_DoubleClick;
            this.ShowInTaskbar = true;
        }

        public FancyMainWindow(List<string> directories)
        {
            InitializeComponent();
            //InitializeUbuntu();
            InitializeSkyDrive();
            InitializeWatchers(directories);

            nIcon = new System.Windows.Forms.NotifyIcon();
            nIcon.Icon = new System.Drawing.Icon("..\\..\\Looks\\IDImg.ico");
            nIcon.DoubleClick += nIcon_DoubleClick;
            this.ShowInTaskbar = true;
        }

        #region FileSystemWatcher
        private List<FileSystemWatcher> _watchers;

        private void LoadWatchers()
        {
            _watchers = new List<FileSystemWatcher>();
        }

        private void InitializeWatchers(List<string> directories)
        {
            _watchers = new List<FileSystemWatcher>();

            foreach (string directory in directories)
            {
                AddWatcher(directory);
            }
        }

        private void AddWatcher(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.Created += new FileSystemEventHandler(watcher_Created);
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);
            watcher.Renamed += new RenamedEventHandler(watcher_Renamed);

            watcher.Path = @path;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            _watchers.Add(watcher);
        }

        private void RemoveWatcher(string path)
        {
            foreach (FileSystemWatcher watcher in _watchers)
            {
                if (watcher.Path == path)
                {
                    _watchers.Remove(watcher);
                }
            }
        }

        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            bool success = Unity.RenameFile(e.OldFullPath, e.FullPath);
            if (success)
            {
                System.Windows.Forms.MessageBox.Show("A file has been renamed");
            }

            else
            {
                System.Windows.Forms.MessageBox.Show("File \"" + e.Name + "\" failed to rename in database");
            }
        }

        void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            bool success = Unity.DeleteFile(e.FullPath);
            if (success)
            {
                System.Windows.Forms.MessageBox.Show("A file has been deleted");
            }

            else
            {
                System.Windows.Forms.MessageBox.Show("File \"" + e.Name + "\" failed to delete from service");
            }
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            bool success = Unity.UpdateFile(e.FullPath);
            if (success)
            {
                System.Windows.Forms.MessageBox.Show("A file has been changed");
            }

            else
            {
                System.Windows.Forms.MessageBox.Show("File \"" + e.Name + "\" failed to update on service");
            }
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            FileInfo info = new FileInfo(e.FullPath);
            string guid = Guid.NewGuid().ToString();
            Unity.UploadFile(guid, e.FullPath, FileStructure.algo.SortingHat(info.Length, e.FullPath, guid));

            System.Windows.Forms.MessageBox.Show("A file has been uploaded");
        }
        #endregion

        #region Looks

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            wSettings WS = new wSettings();
            WS.ShowDialog();
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ethanshafer.com");
        }

        void nIcon_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;

            nIcon.BalloonTipTitle = "Minimized";
            nIcon.BalloonTipText = "Now minimized";
            nIcon.ShowBalloonTip(400);
            nIcon.Visible = true;
            this.ShowInTaskbar = false;

            this.Hide();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //Application.Current.Shutdown();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            switch (this.WindowState)
            {
                case (WindowState.Maximized):
                    {
                        this.WindowState = System.Windows.WindowState.Normal;
                        break;
                    }
                case (WindowState.Normal):
                    {
                        this.WindowState = System.Windows.WindowState.Maximized;
                        break;
                    }
            }
        }

        private void window_MouseDown(object sender, MouseButtonEventArgs e)
        {

            try
            {
 DragMove();
            }
            catch (Exception)
            {
                
               //throw;
            }
       
           
        }

        private void btnDropbox_MouseEnter(object sender, MouseEventArgs e)
        {
            //bDropBoxLight.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnDropbox_MouseLeave(object sender, MouseEventArgs e)
        {
            //bDropBoxLight.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btnGoogleDrive_MouseEnter(object sender, MouseEventArgs e)
        {
            //bGoogleDriveLight.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnGoogleDrive_MouseLeave(object sender, MouseEventArgs e)
        {
           // bGoogleDriveLight.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btnUbuntu_MouseEnter(object sender, MouseEventArgs e)
        {
           // bUbuntuLight.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnUbuntu_MouseLeave(object sender, MouseEventArgs e)
        {
           // bUbuntuLight.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btnSkyDrive_MouseEnter(object sender, MouseEventArgs e)
        {
           // bSkyDriveLight.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnSkyDrive_MouseLeave(object sender, MouseEventArgs e)
        {
           // bSkyDriveLight.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btnDropbox_Click(object sender, RoutedEventArgs e)
        {
            ChangeRow2Height(0);
        }

        private void ChangeRow2Height(int intButton)
        {
            btnMinimizenav2.Visibility = System.Windows.Visibility.Visible;
            gMain.RowDefinitions[1].Height = new GridLength(120);
            switch (intButton)
            {
                case (0): // Dropbox
                    {
                        gDropBoxNav1.Visibility = System.Windows.Visibility.Visible;
                        gGoogleDriveNav1.Visibility = System.Windows.Visibility.Collapsed;
                        //gUbuntuNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gSkyDriveNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gBoxNET.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    }
                case (1): // Google Drive
                    {
                        gDropBoxNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gGoogleDriveNav1.Visibility = System.Windows.Visibility.Visible;
                        //gUbuntuNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gSkyDriveNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gBoxNET.Visibility = System.Windows.Visibility.Collapsed;
                        bFileTree.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    }
                case (2): // Ununtu
                    {
                        gDropBoxNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gGoogleDriveNav1.Visibility = System.Windows.Visibility.Collapsed;
                        //gUbuntuNav1.Visibility = System.Windows.Visibility.Visible;
                        gSkyDriveNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gBoxNET.Visibility = System.Windows.Visibility.Collapsed;
                        bFileTree.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    }
                case (3): // SkyDrive
                    {
                        gDropBoxNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gGoogleDriveNav1.Visibility = System.Windows.Visibility.Collapsed;
                        //gUbuntuNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gSkyDriveNav1.Visibility = System.Windows.Visibility.Visible;
                        gBoxNET.Visibility = System.Windows.Visibility.Collapsed;
                        bFileTree.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    }

                case (4): // Box.NET
                    {
                        gDropBoxNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gGoogleDriveNav1.Visibility = System.Windows.Visibility.Collapsed;
                        //gUbuntuNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gSkyDriveNav1.Visibility = System.Windows.Visibility.Collapsed;
                        gBoxNET.Visibility = System.Windows.Visibility.Visible;
                        bFileTree.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    }
            }
        }

        private void btnMinimizenav2_Click(object sender, RoutedEventArgs e)
        {
            gMain.RowDefinitions[1].Height = new GridLength(75);
            btnMinimizenav2.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btnGoogleDrive_Click(object sender, RoutedEventArgs e)
        {
            ChangeRow2Height(1);
        }

        private void btnUbuntu_Click(object sender, RoutedEventArgs e)
        {
            ChangeRow2Height(2);
        }

        private void btnSkyDrive_Click(object sender, RoutedEventArgs e)
        {
            ChangeRow2Height(3);
        }

 private void btnBOX_Click(object sender, RoutedEventArgs e)
        {
            ChangeRow2Height(4);
        }


        #endregion

        //#region Ubuntu
        //private void InitializeUbuntu()
        //{
        //    if (UbuntuOne.RefreshInfo.UbuntuOneInfo.LoggedIn)
        //    {
        //        btnUbuntu_signin.Content = "Sign-out";
        //    }
        //    else
        //    {
        //        btnUbuntu_signin.Content = "Sign-in";
        //    }

        //    btnUbuntu_info.IsEnabled = UbuntuOne.RefreshInfo.UbuntuOneInfo.LoggedIn;
        //    btnUbuntu_3.IsEnabled = UbuntuOne.RefreshInfo.UbuntuOneInfo.LoggedIn;
        //    btnUbuntu_4.IsEnabled = UbuntuOne.RefreshInfo.UbuntuOneInfo.LoggedIn;
        //    //set username/password textboxes to ""
        //}

        //private void btnUbuntu_signin_Click(object sender, RoutedEventArgs e)
        //{
        //    if (btnUbuntu_signin.Content.ToString() == "Sign-out")
        //    {
        //        UbuntuOne.Api.Logout();
        //    }
        //    else
        //    {
        //        new UbuntuOneSignInWindow().ShowDialog();
        //    }
        //    InitializeUbuntu();
        //}

        //private void Upload_Test_Click(object sender, RoutedEventArgs e)
        //{
        //    if (UbuntuOne.RefreshInfo.UbuntuOneInfo.LoggedIn)
        //    {
        //        System.Windows.Forms.FileDialog fd = new System.Windows.Forms.OpenFileDialog();
        //        fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //        if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //        {
        //            UbuntuOne.Api.UploadFile(Guid.NewGuid().ToString(), fd.FileName);
        //        }
        //    }
        //}

        //private void ShowFiles(object sender, RoutedEventArgs e)
        //{
        //    if (UbuntuOne.RefreshInfo.UbuntuOneInfo.LoggedIn)
        //    {
        //        DirectoryListing files = UbuntuOne.Api.GetFileStructure();

        //        DownloadAllFiles(files);
        //    }
        //}

        //private void DownloadAllFiles(DirectoryListing listing)
        //{
        //    if (listing.rootFolder.has_children)
        //    {
        //        foreach (Item itm in listing.items)
        //            UbuntuOne.Api.DownloadFile(itm.content_path, itm.key);
        //        foreach (UbuntuOne.Folder folder in listing.subFolders)
        //            DownloadFolder(folder);
        //    }
        //}

        //private void DownloadFolder(UbuntuOne.Folder folder)
        //{
        //    foreach (Item itm in folder.items)
        //        UbuntuOne.Api.DownloadFile(itm.content_path, itm.key);
        //    foreach (UbuntuOne.Folder subfolder in folder.subFolders)
        //        DownloadFolder(subfolder);
        //}

        //#endregion

        #region Google Drive

        private void btnGoogleDriveLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!GoogleDriveRefreshInfo.Instance.LoggedIn)
            {
                GoogleDrive.Api.Login(true);

                if (GoogleDriveRefreshInfo.Instance.LoggedIn)
                {
                    GoogleDriveLogin.Content = "Sign-out";
                }

                GoogleDrive.Api.GetQuota();
            }
            else
            {
                GoogleDriveRefreshInfo.Instance = null;
                GoogleDriveLogin.Content = "Sign-in";
            }
        }

        private void ListFiles(object sender, RoutedEventArgs e)
        {
            string dir = "C:\\UltiDrive\\GoogleDrive";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            GoogleDrive.Api.DownloadAllFiles("C:\\UltiDrive\\GoogleDrive");
        }

        private void UploadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dg = new OpenFileDialog();
            if (dg.ShowDialog() == true)
            {
                string[] names = dg.FileName.Split(new string[]{ "\\" }, StringSplitOptions.None);

                GoogleDrive.Api.UploadFile(names[names.Length - 1], dg.FileName);
            }
        }
        #endregion

        #region Skydrive

        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            string path;
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            path = file.FileName;
            if (path != "")
            {
                string guid = Guid.NewGuid().ToString();
                Unity.UploadFile(guid, path, StorageServices.SkyDrive);
            }
            else
            {
                MessageBox.Show("You didn't select a file.");
            }
        }

        private void ConnectClient_UploadCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            System.Windows.MessageBox.Show("Boom!");
            //txtRaw.Text += "BOOM!";
        }

        private void InitializeSkyDrive()
        {
            if (SkyDrive.Properties.LoggedIn)
            {
                btnSkyDriveLogIn.Content = "Login";
            }
            else
            {
                btnSkyDriveLogIn.Content = "Sign out";
            }
        }

        private void SkyDriveLogIn_Click(object sender, RoutedEventArgs e)
        {
            if (SkyDrive.Properties.LoggedIn) //Check to see if there's an existing session...
            {
                /*Check for exisiting session file...
                if (System.IO.File.Exists(App.AppFolder + "\\SkyDrive.json"))
                {
                }
                else
                {
                }

                //END Check for exisiting session file...*/
                SkyDrive.Api.Login(true);

                if (SkyDrive.Properties.LoggedIn)
                {
                    SkyDrive.Properties.Quota quota = SkyDrive.Api.GetQuota();
                    double available = Convert.ToDouble(quota.available.ToString());
                    double totalStorage = Convert.ToDouble(quota.quota.ToString());
                    double percentageFull = (totalStorage - available) / totalStorage;
                    double gbAvailable = available / 1024 / 1024 / 1024;
                    double gbTotalStorage = totalStorage / 1024 / 1024 / 1024;
                    double gbUsed = gbTotalStorage - gbAvailable;

                    //SkyDriveLabel.Content += " - " + percentageFull.ToString("0.00") + "% full (" + gbUsed.ToString("0.00") + "GB of " + gbTotalStorage.ToString("0.00") + "GB used)";
                    btnSkyDriveLogIn.Content = "Sign out";
                    TEST_Click(null, null);
                }
                    //END Get storage quota
                
            }
            else //SIGN OUT of SKYDRIVE
            {
                //SkyDriveLabel.Content = "SkyDrive";
                btnSkyDriveLogIn.Content = "Login";
            }
        }

        private void SkyDriveAccountInto_Click(object sender, RoutedEventArgs e)
        {

        }

        private void skydrive_checkInitiated(object sender, LiveOperationCompletedEventArgs e)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            Files.RootObject data = ser.Deserialize<Files.RootObject>(e.RawResult);

            bool foundFolder = false;
            foreach (Files.Datum listItem in data.data)
            {
                if (listItem.name == "UltiDrive") //Look for UltiDrive folder
                {
                    //System.Windows.MessageBox.Show("Found UltiDrive folder");
                    SkyDrive.Properties.UltiDriveFolderID = listItem.id; //Assign the UltiDrive folder ID to the SkyDrive Properties class.
                    foundFolder = true;
                }
            }

            if (foundFolder == false) //Not found, so create UltiDrive folder.
            {
                LiveConnectClient client = new LiveConnectClient(SkyDrive.Properties.session);
                var folderData = new Dictionary<string, object> { { "name", "UltiDrive" } };
                client.PostCompleted += skydrive_setUltiDriveFolderID;
                //client.PostAsync("me/skydrive", folderData);
                System.Windows.MessageBox.Show("Created UltiDrive folder!");
            }
        }

        private void skydrive_setUltiDriveFolderID(object sender, LiveOperationCompletedEventArgs e)
        {
            LiveConnectClient client = new LiveConnectClient(SkyDrive.Properties.session);
            client.GetCompleted += skydrive_checkInitiated;
            client.GetAsync("me/skydrive/files");
        }

        private void TEST_Click(object sender, RoutedEventArgs e)
        {
            //Check to see if user is logged in
            if (SkyDrive.Properties.session != null)
            {
                //Check to see if SkyDrive is initiated for UltiDrive
                LiveConnectClient client = new LiveConnectClient(SkyDrive.Properties.session);
                client.GetCompleted += this.skydrive_checkInitiated;
                client.GetAsync("me/skydrive/files"); //Get list of files in root directory of SkyDrive.
                //END Check to see if SkyDrive is initiated for UltiDrive
            }
        }
        #endregion

        #region Dropbox
        private void btnDropboxLogin_Click(object sender, RoutedEventArgs e)
        {
            Request request = new Dropbox.Request();

            OAuthToken accessToken = request.GetAccessToken();

            DropboxApi.Api.AccessToken = accessToken;

            Utilities.WriteDropboxSettings();

            InitializeDropboxButtons();
        }

        private void btnDropboxAuto_Click(object sender, RoutedEventArgs e)
        {
            DropboxProperties.Properties.accessToken = "39pmbdbq1c6fe8v";
            DropboxProperties.Properties.accessTokenSecret = "hqsvtxlfr41ad7j";

            OAuthToken accessToken = new OAuthToken(DropboxProperties.Properties.accessToken, DropboxProperties.Properties.accessTokenSecret);

            DropboxApi.Api.AccessToken = accessToken;

            Utilities.WriteDropboxSettings();

            InitializeDropboxButtons();
        }

        private void btnDropboxLogout_Click(object sender, RoutedEventArgs e)
        {
            DropboxApi.Api.AccessToken = null;
            System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\UltiDrive\\Dropbox.json");

            InitializeDropboxButtons();

            bFileTree.Visibility = System.Windows.Visibility.Collapsed;
            treeFileList.Items.Clear();
        }

        private void btnDropboxAccount_Click(object sender, RoutedEventArgs e)
        {
            var account = DropboxApi.Api.GetAccountInfo();
            
            FancyMsgBoxWindow.Show(account.DisplayName + " Account Info", "ID: " + account.Id + "\t Email: " + account.Email + "\t Quota: " + account.Quota.Total);
        }

        private void btnDropboxFiles_Click(object sender, RoutedEventArgs e)
        {
            treeFileList.Items.Clear();

            var appFolder = DropboxApi.Api.GetFiles("sandbox", "");
            foreach (var file in appFolder.Contents)
            {
                treeFileList.Items.Add(file.Path);
            }

            bFileTree.Visibility = System.Windows.Visibility.Visible;
            btnDropboxFiles.Visibility = System.Windows.Visibility.Hidden;
            btnDropboxDownload.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnDropboxDownload_Click(object sender, RoutedEventArgs e)
        {
            string filePath = treeFileList.SelectedItem.ToString();

            Unity.DownloadFile(filePath, "C:\\UltiDrive");
        }

        private void treeFileList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            btnDropboxDownload.IsEnabled = true;
        }

        private void btnDropboxUpload_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "UltiDrive";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = dialog.FileName;
                Unity.UploadFile(Guid.NewGuid().ToString(), filePath, StorageServices.Dropbox);

                btnDropboxFiles_Click(null, null);
            }
        }

        private void gDropBoxNav1_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            InitializeDropboxButtons();
        }

        private void InitializeDropboxButtons()
        {
            //DropboxApi.LoggedIn
            if (DropboxApi.Api.AccessToken == null)
            {
                btnDropboxLogin.Visibility = System.Windows.Visibility.Visible;
                btnDropboxLogout.Visibility = System.Windows.Visibility.Hidden;
                btnDropboxAccount.Visibility = System.Windows.Visibility.Hidden;
                btnDropboxFiles.Visibility = System.Windows.Visibility.Hidden;
                btnDropboxDownload.Visibility = System.Windows.Visibility.Hidden;
                btnDropboxUpload.Visibility = System.Windows.Visibility.Hidden;
                btnDropboxAuto.Visibility = System.Windows.Visibility.Visible;

                btnDropboxLogin.IsEnabled = true;
                btnDropboxLogout.IsEnabled = false;
                btnDropboxAccount.IsEnabled = false;
                btnDropboxFiles.IsEnabled = false;
                btnDropboxDownload.IsEnabled = false;
                btnDropboxUpload.IsEnabled = false;
                btnDropboxAuto.IsEnabled = true;
            }

            else
            {
                btnDropboxLogin.Visibility = System.Windows.Visibility.Hidden;
                btnDropboxLogout.Visibility = System.Windows.Visibility.Visible;
                btnDropboxAccount.Visibility = System.Windows.Visibility.Visible;
                btnDropboxFiles.Visibility = System.Windows.Visibility.Visible;
                btnDropboxDownload.Visibility = System.Windows.Visibility.Hidden;
                btnDropboxUpload.Visibility = System.Windows.Visibility.Visible;
                btnDropboxAuto.Visibility = System.Windows.Visibility.Hidden;

                btnDropboxLogin.IsEnabled = false;
                btnDropboxLogout.IsEnabled = true;
                btnDropboxAccount.IsEnabled = true;
                btnDropboxFiles.IsEnabled = true;
                btnDropboxDownload.IsEnabled = false;
                btnDropboxUpload.IsEnabled = true;
                btnDropboxAuto.IsEnabled = false;
            }
        }
        #endregion        

        private void btnBoxLogin_Click(object sender, RoutedEventArgs e)
        {
            Box.Api.Login(true);
            //if (Box.BoxProperties.UserIsLoggedIn)
            //{
            //    btnBoxAccount.IsEnabled = true;
            //}
            //else
            //{
            //    btnBoxAccount.IsEnabled = false;
            //}
        }

        private void btnBoxAccount_Click(object sender, RoutedEventArgs e)
        {
            //wBoxAccount BA = new wBoxAccount();
            //BA.ShowDialog();
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<string> directories = new List<string>();

            if (_watchers != null)
            {
                foreach (FileSystemWatcher watcher in _watchers)
                {
                    directories.Add(watcher.Path);
                }

                StringWriter stringWriter = new StringWriter(new System.Text.StringBuilder());
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
                writer.Serialize(stringWriter, directories);
                System.IO.File.WriteAllText(App.AppFolder + "\\Watcher.dat", stringWriter.ToString());
            }
        }

        //private void btnBoxAccount_Click(object sender, RoutedEventArgs e)
        //{
           

        //    Box.BoxProperties.AFC.LoadXml("<?xml version='1.0' encoding='UTF-8' ?>\r\n<response><status>listing_ok</status><tree><folder id=\"0\" name=\"\" description=\"\" user_id=\"186900666\" shared=\"\" shared_link=\"\" permissions=\"douv\" size=\"261954\" file_count=\"\" created=\"\" updated=\"\" ><tags></tags><files><file id=\"4043512110\" file_name=\"h1.jpg\" shared=\"0\" sha1=\"80483d1158dfeabaef6090b76face72a92bb7991\" created=\"1353252996\" updated=\"1353253000\" size=\"261954\" shared_link=\"\" description=\"\" user_id=\"186900666\" thumbnail=\"https://www.box.net/thumbnail/f_4043512110/a18c7b80185baffb588deec80aed8c58/2012-11-18/small_thumb.gif\" small_thumbnail=\"https://www.box.net/thumbnail/f_4043512110/a18c7b80185baffb588deec80aed8c58/2012-11-18/small_thumb.gif\" large_thumbnail=\"https://www.box.net/thumbnail/f_4043512110/a18c7b80185baffb588deec80aed8c58/2012-11-18/large_thumb.jpg\" larger_thumbnail=\"https://www.box.net/thumbnail/f_4043512110/a18c7b80185baffb588deec80aed8c58/2012-11-18/larger_thumb.jpg\" preview_thumbnail=\"https://www.box.net/thumbnail/f_4043512110/a18c7b80185baffb588deec80aed8c58/2012-11-18/preview_thumb.jpg\" permissions=\"gdcenopstuvh\"  ><tags></tags></file></files></folder></tree></response>");
        //    Box.BoxProperties.AFC.Save("C:\\1.xml");
        //    Box.BoxMethods.ParseXMLContent();
        //    wBoxAccount BA = new wBoxAccount();
        //    BA.ShowDialog();
        //}
    }
}
