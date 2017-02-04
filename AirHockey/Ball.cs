using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace AirHockey
{

   public class Ball
    {
        public Ellipse Ellipse { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double X
        {
            get
            {
                return Canvas.GetLeft(Ellipse);
            }
            set
            {
                Canvas.SetLeft(Ellipse, value);
            }
        }
        public double Y
        {
            get
            {
                return Canvas.GetTop(Ellipse);
            }
            set
            {
                Canvas.SetTop(Ellipse, value);
            }

        }
        public double LeftBorder
        {
            get { return X; }
        }
        public double RightBorder
        {
            get { return X + Ellipse.ActualWidth; }
        }
        public double TopBorder
        {
            get { return Y; }
        }
        public double BottomBorder
        {
            get { return Y + Ellipse.ActualHeight; }
        }
        public void Move()
        {
            X += Vx;
            Y += Vy;
        }
    }
}
