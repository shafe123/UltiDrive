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
        private System.Windows.Forms.NotifyIcon nIcon;
        
        public FancyMainWindow()
        {
            InitializeComponent();

            nIcon = new System.Windows.Forms.NotifyIcon();
            nIcon.Icon = new System.Drawing.Icon("..\\..\\Looks\\IDImg.ico");
            nIcon.DoubleClick += nIcon_DoubleClick;
            this.ShowInTaskbar = true;
        }

        private void nIcon_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Normal;
            this.Show();
        }

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

        #endregion

        private void ShowUnmanagedFiles_Click(object sender, RoutedEventArgs e)
        {
            UnmanagedFiles.ItemsSource = FileStructure.Index.UnManagedFiles;
        }
    }
}
