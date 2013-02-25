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

namespace UltiDrive
{
    /// <summary>
    /// Interaction logic for FancyMsgBoxWindow.xaml
    /// </summary>
    public partial class FancyMsgBoxWindow : Window
    {
        public FancyMsgBoxWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtbMsg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static bool? Show(string strHeader, string strMessage)
        {
            FancyMsgBoxWindow fmbw = new FancyMsgBoxWindow();
            fmbw.lblHeader.Content = strHeader;
            fmbw.txtbMsg.Text = strMessage;
            return fmbw.ShowDialog();
        }
    }
}
