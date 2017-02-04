using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace AirHockey
{
   
    public delegate void NetWorkException();
    public delegate void SomeBallEvent(object obj, BallRunningEventArgs  args);
    public delegate void BarrierMoveEvent(System.Windows.Input.Key key);

    public abstract class MyNetWorkBase
    {
        public const int PORT = 11000;
        public const int PORT2 = 11200;

        public event NetWorkException SendingException = null;
        public event BarrierMoveEvent BarrierMoving;
        public event SomeBallEvent GameStartingEvent;
        public event SomeBallEvent GameSyncEvent;
        public event Action GameStartedEvent;
        public event Action GameEnded;
        public event Action BallIsMissedEvent;
        public event Action GameRestartEvent;
        public event Action AnotherPlayerMissed;

        protected void OnAnotherPlayerMissed()
        {
            if (AnotherPlayerMissed != null)
                AnotherPlayerMissed();
        }
        public void OnGameRestart()
        {
            if (GameRestartEvent != null)
                GameRestartEvent();
        }
        public void OnBallIsMissed()
        {
            if(BallIsMissedEvent!=null)
                 BallIsMissedEvent();
        }
        protected void OnGameEnd()
        {
            if (GameEnded != null)
                GameEnded();
        }
        protected void OnBarrierMoving(System.Windows.Input.Key key)
        {
            if (BarrierMoving != null)
                BarrierMoving(key);
        }
        protected void OnGameStatring(object obj, BallRunningEventArgs args)
        {
            if (GameStartingEvent != null)
                GameStartingEvent(obj, args);
        }
        protected void OnGameSync(object obj, BallRunningEventArgs args)
        {
            if (GameSyncEvent != null)
                GameSyncEvent(obj, args);
        }
        protected void OnGameStart()
        {
            if (GameStartedEvent != null)
                GameStartedEvent();
        }

        protected string localIP;
        public string LocalIP { get { return localIP; } }
        public abstract byte Number { get; }
        public abstract bool Connected { get; }
        public abstract string Name { get; }


        public abstract bool SendMessage(string msg, IPAddress addrss = null);
        public abstract void Disconnect();


        public void OnWrongConnection()
        {
        }
        public void OnSendingException()
        {
            if (SendingException != null)
                SendingException();
        }
        public static string[] GetClientIPAddresses()
        {
            IPAddress[] host;           
            host = Dns.GetHostAddresses(Dns.GetHostName());
            List<string> lst = new List<string>();
            foreach (IPAddress ip in host)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    lst.Add(ip.ToString());
                }
            }
            return lst.ToArray();
        }
    }

    public class BallRunningEventArgs : EventArgs
    {
       public double VX { get; set; }
       public double VY { get; set; }
       public double X  { get; set; }
       public double Y  { get; set; }
    }
}
