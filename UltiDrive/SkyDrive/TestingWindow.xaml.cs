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
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Mjollnir.Live;
using RestSharp;
using UltiDrive.SkyDrive;
using System.IO;

namespace UltiDrive.SkyDrive
{
    /// <summary>
    /// Interaction logic for Testing.xaml
    /// </summary>
    public partial class Testing : Window
    {
        public Testing()
        {
            InitializeComponent();
        }

        private void getAllFiles(Files.RootObject Data)
        {
        }

        private void btnListAllFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LiveConnectClient liveClient = new LiveConnectClient(Properties.session);
                liveClient.GetCompleted += this.ConnectClient_GetCompleted;
                liveClient.GetAsync("me/skydrive/files");
            }
            catch (LiveConnectException exception)
            {
                txtTestBox.Text = "Error getting folder info: " + exception.Message;
            }

        }

        private void btnDownloadAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog picker = new Microsoft.Win32.OpenFileDialog();
            picker.Filter = "All Files (*.*)|*.*";
            picker.ShowDialog();

            string filePath = picker.FileName;

            if (filePath != "")
            {
                LiveConnectClient liveClient = new LiveConnectClient(Properties.session);
                liveClient.UploadCompleted += this.ConnectClient_UploadCompleted;
                var stream = default(Stream);
                stream = File.OpenRead(filePath);
                liveClient.UploadAsync("folder.6f717ff8a3b1a691.6F717FF8A3B1A691!192/files", filePath, stream, stream);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("You didn't select a file.");
            }

        }

        private void ConnectClient_UploadCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            txtRaw.Text += "BOOM!";
        }

        private void ConnectClient_GetCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Files.RootObject data = serializer.Deserialize<Files.RootObject>(e.RawResult);

            foreach (Files.Datum listItem in data.data)
            {
                txtTestBox.Text += listItem.name + "\r\n";
                txtRaw.Text += listItem.id + "\r\n";
            }
        }

        private void btnUploadDirectory_Click(object sender, RoutedEventArgs e)
        {
            //Show folder picker...
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowDialog();
            //END Show folder picker...
            
            //Get a recusive list of all files...
            if (dialog.SelectedPath == "")
            {
                System.Windows.MessageBox.Show("No folder selected.");
                return;
            }
            string[] files = Directory.GetFiles(dialog.SelectedPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                txtRaw.Text += file + "\r\n";
                LiveConnectClient client = new LiveConnectClient(Properties.session);

                client.UploadCompleted += this.ConnectClient_UploadCompleted;
                var stream = default(Stream);
                stream = File.OpenRead(file);
                client.UploadAsync(Properties.UltiDriveFolderID + "/files", file, stream, stream);
                stream.Close();
                //System.Windows.MessageBox.Show(file);
            }
            //END Get a recursive list of all files...
        }
    }
}
