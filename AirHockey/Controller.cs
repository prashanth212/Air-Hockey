using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Input;

namespace AirHockey
{
    class Controller
    {
        public MainWindow view;           // main screen class
        public MyNetWorkBase NetMember;   // network member - Client or Server
        Random rnd = new Random();

        public Controller()
        {
            view = new MainWindow();
            
        }

        public void NetMember_AnotherPlayerMissed()
        {
            view.myScore++;
            if (view.myScore >= 10)
            {
                view.myScore = view.hisScore = 0;
            }
            if (NetMember is Client)
            {
                view.Dispatcher.BeginInvoke(new Action(() => { view.clock.Stop(); }));
            }
        }

        public void Server_GameRestartEvent()
        {
            view.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!NetMember.Connected) return;
                    view.clock.Stop();
                    view.ScoreBox.Text = "Score " + view.myScore + ":" + view.hisScore;
                    view.SetBallToCenter();
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    StartGame();
                }));
        }

        public void NetMember_BallIsMissedEvent()
        {
            if (!NetMember.SendMessage("MISSED"))
            {
                view.clock.Stop();
                if (!NetMember.Connected) return;
                view.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(view.Close));
                System.Windows.MessageBox.Show("Cannot send message to another player", "Connection problems", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }

        public void NetMember_GameEnded()
        {
            System.Windows.MessageBox.Show("Game is ended", "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            view.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(view.Close));
        }

        public void Client_GameStartedEvent(object obj,BallRunningEventArgs args)
        {
            view.Dispatcher.BeginInvoke(new SomeBallEvent((object obj1, BallRunningEventArgs args1) => 
            {
                view.SetBallToCenter();
                //TransformMirrorCoordinates(args);
                view.ball.Vx = args.VX;
                view.ball.Vy = args.VY;
                view.ScoreBox.Text = "Score " + view.myScore + ":" + view.hisScore;
                view.clock.Start();
            }), obj, args);
        }

        private void TransformMirrorCoordinates(BallRunningEventArgs args)
        {
            args.VX = -args.VX;
            args.VY = -args.VY;
            double MirrX = Math.Abs(args.X - view.GameField.Width / 2);
            double MirrY = Math.Abs(args.Y - view.GameField.Height / 2);

            if (args.X - view.GameField.Width / 2 > 0)
            {
                args.X = view.GameField.Width / 2 - MirrX;
            }
            else
                args.X = view.GameField.Width / 2 + MirrX;

            if (args.Y - view.GameField.Height / 2 > 0)
            {
                args.Y = view.GameField.Height / 2 - MirrY;
            }
            else
                args.Y = view.GameField.Height / 2 + MirrY;
        }

        public void Client_GameSyncEvent(object obj, BallRunningEventArgs args)
        {
            view.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (BallIsNearSomeBarrier()) return;
                TransformMirrorCoordinates(args);
                view.ball.Vx = args.VX;
                view.ball.Vy = args.VY;
                view.ball.X = (args.X + view.ball.X) / 2; view.ball.Y = (args.Y + view.ball.Y) / 2;
                view.lastReflection = ReflectionType.None;
            }));
        }

        private bool BallIsNearSomeBarrier()
        {
            bool result1 = view.ball.LeftBorder < 4 || Math.Abs(view.ball.RightBorder - view.GameField.Width) < 4;
            return result1;
        }

        bool ClientIsLagging(BallRunningEventArgs argServ)
        {
            // view.ball on client and on server is moving in the same direction
            bool isSameDirection = view.ball.Vx / argServ.VX > 0 && view.ball.Vy / argServ.VY > 0;
            bool isLaggin;
            if (view.ball.Vy > 0)
            {
                isLaggin = (view.ball.Y + view.ball.Vy) < argServ.Y;
            }
            else
                isLaggin = (view.ball.Y - view.ball.Vy) < argServ.Y;
            return isSameDirection && isLaggin;
        }

        public void StartGame()
        {
            view.NetMember = NetMember;
            double Vx = rnd.NextDouble();
            Vx += rnd.Next(1, 1);
            double Vy = rnd.NextDouble();
            Vy += rnd.Next(2, 2);
            if (rnd.Next(0, 2) != 0) Vx = -Vx;
            if (rnd.Next(0, 2) != 0) Vy = -Vy;
            view.ball.Vx = Vx;
            view.ball.Vy = Vy;
            Vx = -Vx; // Mirror game
            Vy = -Vy; // Mirror game   
            view.clock.Start();
            bool result = NetMember.SendMessage("START:VX:" + Vx + "VY:" + Vy);       
            if (!result)
            {
                view.clock.Stop();
                view.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(view.Close));
                System.Windows.MessageBox.Show("Cannot start the game", "Connection problems", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }         
        }
    }
}
