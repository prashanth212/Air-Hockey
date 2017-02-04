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

using System.Windows.Media.Animation;

namespace AirHockey
{
    /// <summary>
    /// Interaction logic for WaitingWindow.xaml
    /// </summary>
    public partial class WaitingWindow : Window
    {
        public WaitingWindow()
        {
            InitializeComponent();
        }
        bool wasClosed = false;
        public bool fromNetwork = false;
        public bool IsClosed 
        {
            get { return wasClosed; }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!fromNetwork)
               wasClosed = true;
        }
        private void InfoBox_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation rotate = new DoubleAnimation();
            rotate.From = 0;
            rotate.To = 360;
            rotate.RepeatBehavior = RepeatBehavior.Forever;
            rotate.Duration = new Duration(TimeSpan.FromSeconds(2));
            RotateTransform trans = new RotateTransform();
            RefreshImage.RenderTransform = trans;
            trans.BeginAnimation(RotateTransform.AngleProperty, rotate);
        }
    }
}
