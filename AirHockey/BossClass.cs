using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;

namespace AirHockey
{
    class BossClass
    {
        public Controller cntrl;
        public StartMenuWindow startWindow;
        WaitingWindow wnd;

        public BossClass()
        {
            startWindow = new StartMenuWindow();
            startWindow.GameStartingEvent += startWindow_GameStartingEvent;
        }
        void startWindow_GameStartingEvent()
        {
            cntrl = new Controller();            
            if (startWindow.IsServer)
            {
                cntrl.NetMember = new Server("Server", startWindow.IPList.SelectedItem.ToString());
                if (!cntrl.NetMember.Connected)
                {
                    System.Windows.MessageBox.Show("Cannot create server.\nSome error occured.", "Connection problems", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    startWindow.ShowInTaskbar = true;
                    startWindow.WindowState = System.Windows.WindowState.Normal;
                    return;
                }
                cntrl.NetMember.GameStartedEvent += NetMember_GameStartedEvent;
            }
            else
            {
                cntrl.NetMember = new Client(startWindow.ServerIP, "Client", startWindow.IPList.SelectedItem.ToString());
                if (!cntrl.NetMember.Connected)
                {
                    System.Windows.MessageBox.Show("Cannot connect to the " + startWindow.ServerIP + " server", "Connection problems", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    startWindow.ShowInTaskbar = true;
                    startWindow.WindowState = System.Windows.WindowState.Normal;
                    return;
                }
                cntrl.NetMember.GameStartingEvent += cntrl.Client_GameStartedEvent;
                cntrl.NetMember.GameSyncEvent += cntrl.Client_GameSyncEvent;
                cntrl.NetMember.BarrierMoving += cntrl.view.Sync_BarrierMoving;
                cntrl.NetMember.GameEnded += cntrl.NetMember_GameEnded;
                cntrl.NetMember.BallIsMissedEvent += cntrl.NetMember_BallIsMissedEvent;
                cntrl.NetMember.AnotherPlayerMissed += cntrl.NetMember_AnotherPlayerMissed;
                cntrl.view.NetMember = cntrl.NetMember;
                cntrl.view.Title = "Client Window";
                cntrl.view.ShowDialog();
                startWindow.ShowInTaskbar = true;
                startWindow.WindowState = System.Windows.WindowState.Normal;
                cntrl.NetMember.Disconnect();
                return;
            }
            startWindow.WindowState = System.Windows.WindowState.Minimized;
            startWindow.ShowInTaskbar = false;
            wnd = new WaitingWindow();
            wnd.InfoBox.Text = "Please wait while someone will connect to your game.";
            wnd.ShowDialog();
            if (wnd.IsClosed)
            {
                startWindow.ShowInTaskbar = true;
                startWindow.WindowState = System.Windows.WindowState.Normal;
                cntrl.NetMember.Disconnect();
            }
            else
            {
                cntrl.view.Title = "Server Window";
                cntrl.NetMember.BarrierMoving += cntrl.view.Sync_BarrierMoving;
                cntrl.NetMember.GameEnded += cntrl.NetMember_GameEnded;
                cntrl.NetMember.GameRestartEvent += cntrl.Server_GameRestartEvent;
                cntrl.NetMember.BallIsMissedEvent += cntrl.NetMember_BallIsMissedEvent;
                cntrl.NetMember.AnotherPlayerMissed += cntrl.NetMember_AnotherPlayerMissed;
                cntrl.view.Dispatcher.BeginInvoke(new Action(() => { cntrl.StartGame(); }));
                cntrl.view.ShowDialog();
                startWindow.ShowInTaskbar = true;
                startWindow.WindowState = System.Windows.WindowState.Normal;
                cntrl.NetMember.Disconnect();
            }
        }
        void NetMember_GameStartedEvent()
        {
            wnd.fromNetwork = true;
            wnd.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(wnd.Close));
        }
    }
}
