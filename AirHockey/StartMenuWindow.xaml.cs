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

using System.Text.RegularExpressions;

namespace AirHockey
{
    /// <summary>
    /// Interaction logic for StartMenuWindow.xaml
    /// </summary>
    public partial class StartMenuWindow : Window
    {
        public StartMenuWindow()
        {
            InitializeComponent();
            foreach (var ip in MyNetWorkBase.GetClientIPAddresses())
                IPList.Items.Add(ip);
        }
        public string ServerIP { get { return ServerIpBox.Text; } }
        public bool IsServer { get { return (bool)IsServerRadio.IsChecked; } }
        public event Action GameStartingEvent;

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ServerIPlabel.Visibility = System.Windows.Visibility.Visible;
            ServerIpBox.Visibility = System.Windows.Visibility.Visible;
        }

        private void IsServerRadio_Checked(object sender, RoutedEventArgs e)
        {
            ServerIPlabel.Visibility = System.Windows.Visibility.Collapsed;
            ServerIpBox.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (IPList.SelectedItem == null)
            {
                IPList.BorderThickness = new System.Windows.Thickness(2);
                IPList.BorderBrush = System.Windows.Media.Brushes.Red;
                return;
            }
            IPList.BorderThickness = new System.Windows.Thickness(0);
            if (!IsServer)
            {
                Regex rg = new Regex(@"\d{1,3}.{1}\d{1,3}.{1}\d{1,3}.{1}\d{1,3}");
                if (rg.IsMatch(ServerIP))
                {
                    ServerIpBox.BorderThickness = new Thickness(0);
                    if (GameStartingEvent != null)
                    {
                        ShowInTaskbar = false;
                        WindowState = System.Windows.WindowState.Minimized;
                        GameStartingEvent();
                        return;
                    }                 
                }
                else
                {
                    ServerIpBox.BorderThickness = new Thickness(2);
                    ServerIpBox.BorderBrush = Brushes.Red;
                }
            }
            if (GameStartingEvent != null)
                GameStartingEvent();
        }
    }
}
