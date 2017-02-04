using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;


namespace AirHockey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public  enum ReflectionType {UserBarrier, LeftBorder, RightBarrier, None };

    public partial class MainWindow : Window
    {
        public Ball ball;
        public Barrier[] barriers = new Barrier[2]; // 0 - Bottom, 1 - Top
        public DispatcherTimer clock;
        bool borderIsKicked = false;
        public MyNetWorkBase NetMember;
        public int myScore { get; set; }
        public int hisScore { get; set; }



        int syncCounter = 0;
        const int SYNC_TIME = 100;
        public ReflectionType lastReflection;

        public MainWindow()
        {
            InitializeComponent();
            InitBall();
            InitBarriers();
            clock = new DispatcherTimer();
            clock.Interval = TimeSpan.FromMilliseconds(1);
            clock.Tick += clock_Tick;
            lastReflection = ReflectionType.None;
            myScore = 0;
            hisScore = 0;
        }

        void clock_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < barriers.Length; i++)
                barriers[i].DecrementTime();
            ball.Move();   
            if (NetMember != null && NetMember is Server && ++syncCounter%SYNC_TIME==0)
            {         
                syncCounter = 0;
                double Xcoor = ball.X, Ycoor = ball.Y;
                Task<bool> task = new Task<bool>(() =>
                { return NetMember.SendMessage("SYNC:VX:" + ball.Vx + "VY:" + ball.Vy + "X:" + Xcoor + "Y:" + Ycoor); });
                task.Start();
            }                
            if (ball.RightBorder >= GameField.Width && lastReflection!= ReflectionType.RightBarrier)
            {
                lastReflection = ReflectionType.RightBarrier;
                ball.Vx = -ball.Vx;
                borderIsKicked = false;
            }
            else if(ball.LeftBorder <= 0 && lastReflection != ReflectionType.LeftBorder)
            {
                lastReflection = ReflectionType.LeftBorder;
                ball.Vx = -ball.Vx;
                borderIsKicked = false;
            }
            else if (ball.BottomBorder >= GameField.Height )
            {
                hisScore++;
                if (hisScore >= 10)
                {
                    myScore = hisScore = 0;
                }
                NetMember.OnBallIsMissed();
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.5));
                if (NetMember is Client)
                {
                    clock.Stop();
                }
                else
                {
                    NetMember.OnGameRestart();                                 
                }
                return;
            }
            else if (ball.TopBorder <= 0)
            {
                ball.Vy = -ball.Vy;
            }
            else
                for (int i = 0; i < barriers.Length; ++i)
                {
                    if (BarrierReflection(barriers[i]))
                    {
                        lastReflection = ReflectionType.UserBarrier;
                        borderIsKicked = false;
                        return;
                    }
                }
            if (BallStrikesAnySide())
            {
                ball.Vx = -ball.Vx;
                lastReflection = ReflectionType.UserBarrier;
            }
        }

        public void SetBallToCenter()
        {
            Canvas.SetLeft(ball.Ellipse, (GameField.Width - ball.Ellipse.Width) / 2);
            Canvas.SetTop(ball.Ellipse, (GameField.Height - ball.Ellipse.Height) / 2);
        }

        void InitBall()
        {
            ball = new Ball();
            ball.Ellipse = new Ellipse();
            ball.Ellipse.Width  = 25;
            ball.Ellipse.Height = 25;
            ball.Ellipse.Fill = Brushes.Red;
            SetBallToCenter();
            GameField.Children.Add(ball.Ellipse);
        }
        void InitBarriers()
        {
            double TopCoor;
            for (int i = 0; i < barriers.Length; ++i )
            {
                if(i == 0)
                {
                    TopCoor = GameField.Height - 2 * (GameField.Height / 15);
                }
                else 
                    TopCoor = (GameField.Height / 15);

                barriers[i] = new Barrier();
                barriers[i].Border = new Border();
                barriers[i].Border.Width = GameField.Width / 3;
                barriers[i].Border.Height = GameField.Height / 15;
                barriers[i].Border.Background = Brushes.Brown;
                barriers[i].Orientation = i == 0 ? Orientation.Bottom: Orientation.Top;
                barriers[i].Y = TopCoor;
                barriers[i].X = (GameField.Width - barriers[i].Border.Width) / 2;
                GameField.Children.Add(barriers[i].Border);
            }
        }
        bool BallOnBarrierFace(Barrier barrier)
        {
            if(barrier.Orientation ==  Orientation.Bottom)
                return Math.Abs(barrier.FaceBorder - ball.BottomBorder) < 4 && ball.LeftBorder < barrier.RightBorder && ball.RightBorder> barrier.LeftBorder;
            return Math.Abs(barrier.FaceBorder - ball.TopBorder) < 4 && ball.LeftBorder < barrier.RightBorder && ball.RightBorder > barrier.LeftBorder;
        }
        bool BarrierReflection(Barrier bar)
        {
            if (BallOnBarrierFace(bar))
            {
                if (ball.Vy > 0 && bar.Orientation == Orientation.Bottom || ball.Vy < 0 && bar.Orientation == Orientation.Top)
                {
                    ball.Vy = -ball.Vy;
                    switch(bar.MoveDirection)
                    {
                        case MoveDirection.Left:
                            ball.Vx -= Barrier.AbsolutSpeed;
                            break;
                        case MoveDirection.Right:
                            ball.Vx += Barrier.AbsolutSpeed;
                            break;
                    }                  
                }
                return true;
            }
            return false;
        }
        void MoveBarrier(Barrier bar, Key key)
        {
            double border;
            // offset the current barrier on this value
            double offset = (GameField.Width - bar.Border.Width) / 10; 
            switch (key)
            {
                case Key.Left:
                    border = bar.X - offset;
                    if (border < 0) border = 0;
                    else
                    {
                        if (BarrierStrickesBall(bar, MoveDirection.Left, border))
                        {
                            if (!borderIsKicked)
                            {
                                border = ball.RightBorder;
                                ball.Vx += Barrier.AbsolutSpeed;
                                ball.Vx = -ball.Vx;
                                lastReflection = ReflectionType.UserBarrier;
                                borderIsKicked = true;
                            }
                            else
                                border = bar.X;
                        }
                        bar.MoveDirection = MoveDirection.Left;
                    }
                    bar.X = border;
                    break;
                case Key.Right:
                    border = bar.X + offset;
                    if (border + bar.Border.Width > GameField.Width)
                        border = GameField.Width - bar.Border.Width;
                    else
                    {
                        if (BarrierStrickesBall(bar, MoveDirection.Right, border))
                        {
                            if (!borderIsKicked)
                            {
                                border = ball.LeftBorder - bar.Border.Width;
                                ball.Vx += Barrier.AbsolutSpeed;
                                lastReflection = ReflectionType.UserBarrier;
                                borderIsKicked = true;
                            }
                            else
                                border = bar.X;
                        }
                        bar.MoveDirection = MoveDirection.Right;
                    }
                    bar.X = border;
                    break;
            }
        }

        private void MoveHisBarrier(Key key)
        {
            key = key == Key.Left ? Key.Right : Key.Left;
            MoveBarrier(barriers[1], key);
        }

        public void Sync_BarrierMoving(System.Windows.Input.Key key)
        {
            GameField.Dispatcher.BeginInvoke(new BarrierMoveEvent(MoveHisBarrier), key);
        }
        bool BarrierStrickesBall(Barrier bar, MoveDirection dir, double n_pos)
        {
            bool result1, result2;
            if(bar.Orientation == Orientation.Bottom)
            {
                result1 = bar.FaceBorder <= ball.BottomBorder && bar.BackBorder >= ball.TopBorder;
            }
            else
            {
                result1 = bar.FaceBorder >= ball.TopBorder && bar.BackBorder <= ball.BottomBorder;
            }  
         
            if(dir == MoveDirection.Left)
            {
                result2 = n_pos <= ball.RightBorder && n_pos + bar.Border.Width > ball.RightBorder;
            }
            else
            {
                result2 = n_pos + bar.Border.Width >= ball.LeftBorder && n_pos < ball.LeftBorder;
            }
            return result1 && result2;
        }
        bool BallStrikesAnySide()
        {
            if (ball.Vx > 0 && ball.Vy > 0 && barriers[0].MoveDirection == MoveDirection.Left)
                return BarrierStrickesBall(barriers[0], MoveDirection.Left, barriers[0].LeftBorder)
                    && Math.Abs(ball.RightBorder - barriers[0].LeftBorder) < 4;
            if (ball.Vx < 0 && ball.Vy > 0)
                return BarrierStrickesBall(barriers[0], MoveDirection.Right, barriers[0].LeftBorder)
                    && Math.Abs(ball.LeftBorder - barriers[0].RightBorder) < 4;
            if(ball.Vx > 0 && ball.Vy < 0)
                return BarrierStrickesBall(barriers[1], MoveDirection.Left, barriers[1].LeftBorder)
                    && Math.Abs(ball.RightBorder - barriers[1].LeftBorder) < 4;
            if (ball.Vx < 0 && ball.Vy < 0)
               return  BarrierStrickesBall(barriers[1], MoveDirection.Right, barriers[1].LeftBorder)
                   && Math.Abs(ball.LeftBorder - barriers[1].RightBorder) < 4;
            return false;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                if (!clock.IsEnabled)
                {
                    ball.X = 100;
                    ball.Y = 100;
                    ball.Vy = 5;
                    borderIsKicked = false;
                    clock.Start();                
                }
                else clock.Stop();
            MoveBarrier(barriers[0], e.Key);
            if (NetMember != null)
                NetMember.SendMessage("MOVE:" + e.Key.ToString()[0]);
        }
    }
}
