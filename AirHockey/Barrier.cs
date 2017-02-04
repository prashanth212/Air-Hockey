using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AirHockey
{
    public enum Orientation { Top, Bottom };
    public enum MoveDirection { Left, None, Right };
    public class Barrier
    {
        public Barrier()
        {
            _moveDirection = MoveDirection.None;
            _moveTimeOut = 0;
        }
        public Border Border { get; set; }
        public Orientation Orientation { get; set; }
        public MoveDirection MoveDirection
        {
            get { return _moveDirection; }
            set 
            {
                if (value != AirHockey.MoveDirection.None)
                    _moveTimeOut = ReleaseTimeOut;
                _moveDirection = value;
            }
        }
        public double X
        {
            get
            {
                return Canvas.GetLeft(Border);
            }
            set
            {
                Canvas.SetLeft(Border, value);
            }
        }
        public double Y
        {
            get
            {
                return Canvas.GetTop(Border);
            }
            set
            {
                Canvas.SetTop(Border, value);
            }

        }

        public const double AbsolutSpeed = 0.3;
        const int ReleaseTimeOut = 15; // in milliseconds 
        public double LeftBorder
        {
            get { return X; }
        }
        public double RightBorder
        {
            get { return X + Border.Width; }
        }
        public double FaceBorder
        {
            get 
            { 
                if(Orientation == AirHockey.Orientation.Top)
                {
                    return Y + Border.Height;
                }
                return Y;
            }
        }
        public double BackBorder
        {
            get
            {
                if (Orientation == AirHockey.Orientation.Top)
                {
                    return Y;
                }
                return Y + Border.Height;
            }
        }
        public void DecrementTime()
        {
            if(MoveDirection == AirHockey.MoveDirection.None) return;
            if (_moveTimeOut > 0)
                _moveTimeOut--;
            else _moveDirection = AirHockey.MoveDirection.None;
        }

        int _moveTimeOut;
        MoveDirection _moveDirection;
    }
}
