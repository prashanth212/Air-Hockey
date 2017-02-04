using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Threading;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Net;

namespace AirHockey
{
    class Server : MyNetWorkBase
    {

        string[] msgPatterns = new string[] { @"(LEAVED:){1}(\d){1}", @"(IAM:){1}(\w+)", "(STOP){1}", @"(MOVE:){1}(\w{1})", "(MISSED){1}" };

        List<IPAddress> addresses = new List<IPAddress>();

        public override byte Number { get { return 0; } }

        bool connected = false;
        public override bool Connected { get { return connected; } }

        string _name = "server";

        public override string Name
        {
            get { return _name; }
        }

        List<string> _players = new List<string>();

        public ReadOnlyCollection<string> PlayersNames { get { return _players.AsReadOnly(); } }

        public ReadOnlyCollection<IPAddress> ClientsAddresses
        {
            get
            {
                return addresses.AsReadOnly();
            }
        }

        Thread listening;

        byte[] message;
        bool runningFlag = false;

        Socket ownSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public Server(string name, string ownIP)
        {
            _name = name;
            localIP = ownIP;
            ownSocket.ReceiveTimeout = 2000;
            if (localIP.Length > 0)
            {
                try
                {
                    ownSocket.Bind(new IPEndPoint(IPAddress.Parse(localIP), MyNetWorkBase.PORT));
                    listening = new Thread(StartListening);
                    listening.IsBackground = true;
                    listening.Start();
                    connected = true;
                }
                catch
                { connected = false; }
            }
        }

        void StartListening()
        {
            runningFlag = true;
            bool flag = false;
            try
            {
                ownSocket.Listen(100);
                Socket soc;
                while (runningFlag)
                {
                    soc = ownSocket.Accept();
                    message = new byte[128];
                    int bytes = soc.Receive(message, SocketFlags.None);
                    if (bytes > 0)
                    {
                        string info = Encoding.Unicode.GetString(message);
                        info = info.Trim('\0');
                        IPAddress adress = IPAddress.Any;
                        if (IPAddress.TryParse(info, out adress) && info.Contains('.'))
                        {

                            IPEndPoint point = soc.RemoteEndPoint as IPEndPoint;
                            if (point == null) point = new IPEndPoint(addresses[0], PORT2);
                            IPAddress IP = IPAddress.Parse(point.Address.ToString());
                            foreach (var m in addresses) flag |= m.ToString() == IP.ToString();

                            if (!flag || addresses.Count >= 6)
                            {
                                addresses.Add(IP);
                                soc.Send(Encoding.Unicode.GetBytes(String.Format("YOUARE:" + (addresses[addresses.Count - 1]))));
                            }
                            else
                            {
                                SendMessage("GOAWAY", IP);
                            }
                            IP = null;
                        }
                        else
                        {
                            string request = "OK";
                            int k = -1;
                            #region
                            switch (k = ClientMessageHasCome(info))
                            {
                                case -2: request = "PLAYERS:0";
                                    {
                                        IPEndPoint point = soc.RemoteEndPoint as IPEndPoint;
                                        foreach (var ad in addresses) if (ad.ToString() == ((IPEndPoint)point).Address.ToString())
                                            {
                                                addresses.Remove(ad);
                                                break;
                                            }
                                    }
                                    break;

                                case 1:
                                    request = "PLAYERS:";
                                    foreach (var m in _players) request += m + "|";
                                    request = request.Substring(0, request.Length - 1);

                                    for (int i = 0; i < addresses.Count - 1; i++)
                                    {
                                        if (i == 0) continue;
                                        SendMessage(request, addresses[i]);
                                    }
                                    break;
                                case 2:
                                    {
                                        IPEndPoint point = soc.RemoteEndPoint as IPEndPoint;
                                        if (point.Address.ToString() == LocalIP) runningFlag = false;
                                    } break;
                            }
                            #endregion
                            soc.Send(Encoding.Unicode.GetBytes(request));
                            //if (k == -1) MessageBox.Show(info);
                        }
                        soc.Shutdown(SocketShutdown.Both);
                        soc.Close();
                        soc.Dispose();
                    }

                }
                soc = null;
                ownSocket.Dispose();
                ownSocket = null;
                connected = false;
            }
            catch
            {
                ownSocket.Dispose();
                ownSocket = null;
                OnWrongConnection();
            }
        }

        public override bool SendMessage(string msg, IPAddress addrss)
        {
            if (_players.Count == 0) return false;
            Socket sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sc.ReceiveTimeout = 2000;
            try
            {
                sc.Connect(new IPEndPoint(ClientsAddresses[0], MyNetWorkBase.PORT2));
                message = new byte[msg.Length];
                sc.Send(Encoding.Unicode.GetBytes(msg));
                int a = sc.Receive(message);
                if (a > 0) return true;
                return false;
            }
            catch
            {
                OnSendingException();
                return false;
            }
            finally
            {
                sc.Close();
            }
        }

        bool StopListening() 
        {
            Socket sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                if (!runningFlag) return true;
                sc.Connect(new IPEndPoint(IPAddress.Parse(localIP), MyNetWorkBase.PORT));

                int a = sc.Send(Encoding.Unicode.GetBytes("STOP"));

                if (a > 0)
                {
                    Thread.Sleep(200);
                    return !runningFlag;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (sc.Connected)
                {
                    sc.Close();
                }
            }
        }

        void PlayerHasLeft(byte number)
        {
            addresses.RemoveAt(number);
            _players.RemoveAt(number);

            string pl = "PLAYERS:";
            foreach (var m in _players) pl += m + "|";
            pl = pl.Substring(0, pl.Length - 1);

            for (int i = 0; i < addresses.Count; i++)
            {
                if (i == 0) continue;
                SendMessage(pl, addresses[i]);
            }

        }

        int ClientMessageHasCome(string msg)
        {
            Regex rg;

            int num = -1;
            for (int i = 0; i < msgPatterns.Length; i++)
            {
                rg = new Regex(msgPatterns[i]);

                if (!rg.IsMatch(msg)) continue;

                switch (i)
                {
                    case 0:
                        {
                            msg = rg.Replace(msg, "$2", 1);
                            byte numb = byte.Parse(msg);
                            OnGameEnd();
                            //PlayerHasLeft(numb);
                            num = 0;
                        } break;
                    case 1:
                        {
                            msg = rg.Replace(msg, "$2");
                            if (!_players.Contains(msg))
                            {
                                _players.Add(msg);
                                num = 1;
                                OnGameStart();
                            }
                            else { num = -2; }
                        } break;
                    case 2:
                        {
                            num = 2;
                        } break;
                    case 3:
                        char letter = rg.Replace(msg, "$2", 1)[0];
                        if (letter == 'L')
                            OnBarrierMoving(System.Windows.Input.Key.Left);
                        else if (letter == 'R')
                            OnBarrierMoving(System.Windows.Input.Key.Right);
                        break;
                    case 4:
                        OnAnotherPlayerMissed();
                        OnGameRestart();
                        break;
                }
                break;
            }

            return num;
        }

        public override void Disconnect()
        {
            if (!Connected) return;
            for (int i = 0; i < addresses.Count; i++)
            {
                SendMessage("GAMEOVER", addresses[i]);
            }
            StopListening(); 
        }
    }
}
