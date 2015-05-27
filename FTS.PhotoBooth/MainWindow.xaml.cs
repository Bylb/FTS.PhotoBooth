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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;

namespace FTS.PhotoBooth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
         
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var msg = new ViewModel.ExitMsg();
            Messenger.Default.Send<ViewModel.ExitMsg>(msg);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Space)
                Messenger.Default.Send<ViewModel.SnapshotMsg>(new ViewModel.SnapshotMsg());

        }
    }
}
