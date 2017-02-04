using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Net;

namespace AirHockey
{
    class Client : MyNetWorkBase
    {

        string[] msgPatterns = new string[] 
        {
            @"(YOUARE:){1}(\d+.{1}\d+.{1}\d+.{1})" ,
            @"(PLAYERS:){1}(\w+){1}",
            "(GAMEOVER){1}",
            @"(START:){1}(VX:[-+]?\d+.?\d*){1}(VY:[-+]?\d+.?\d*){1}",
            @"(SYNC:){1}(VX:[-+]?\d+.?\d*){1}(VY:[-+]?\d+.?\d*){1}(X:\d+.?\d*){1}(Y:\d+.?\d*){1}",
            @"(MOVE:){1}(\w{1})",
            "(MISSED){1}"
        };

        bool connected = false;
        public override bool Connected { get { return connected; } }

        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        byte _number = 0;
        public override byte Number { get { return _number; } }

        string _name = "client";

        public override string Name
        {
            get { return _name; }
        }

        List<string> _players = new List<string>();

        public ReadOnlyCollection<string> PlayersNames { get { return _players.AsReadOnly(); } }

        IPEndPoint server;

        Thread clientThread;
        bool runningFlag = false;

        byte[] message = new byte[1024];

        public Client(string ServerIP, string name, string ownIP)
        {
            //SendingException += sendingException;
            localIP = ownIP;
            _name = name;
            if (SendIP(ServerIP))
            {
                listener.Bind(new IPEndPoint(IPAddress.Parse(localIP), MyNetWorkBase.PORT2));
                clientThread = new Thread(StartListening);
                clientThread.IsBackground = true;
                clientThread.Start();
                server = new IPEndPoint(IPAddress.Parse(ServerIP), MyNetWorkBase.PORT);
                if (!SendMessage("IAM:" + _name)) StopListening();
                else connected = true;
            }

        }

        void StartListening()
        {
            runningFlag = true;

            try
            {
                listener.Listen(2);

                Socket soc;
                while (runningFlag)
                {
                    soc = listener.Accept();
                    message = new byte[1024];
                    int bytes = soc.Receive(message, SocketFlags.None);
                    if (bytes > 0)
                    {
                        soc.Send(Encoding.Unicode.GetBytes("OK"));
                        string info = Encoding.Unicode.GetString(message);
                        info = info.Trim('\0');
                        switch (info)
                        {
                            case "STOP": runningFlag = false; break;
                            default: ServerMessageHasCome(info); break;
                        }

                    }
                    soc.Shutdown(SocketShutdown.Both);
                    soc.Close();
                    soc.Dispose();
                }
                soc = null;
                listener.Dispose();
                listener = null;
                connected = false;
            }
            catch
            {
                listener.Dispose();
                listener = null;
                OnWrongConnection();
            }
        }

        bool SendIP(string ServerIP)
        {
            Socket ownSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ownSocket.SendTimeout = 3000;
            ownSocket.ReceiveTimeout = 2000;
            try
            {
                //ownSocket.Connect(new IPEndPoint(IPAddress.Parse(ServerIP), MyNetWorkBase.PORT));
                IAsyncResult res = ownSocket.BeginConnect(IPAddress.Parse(ServerIP), MyNetWorkBase.PORT, null, null);
                bool success = res.AsyncWaitHandle.WaitOne(3000, true);
                if (!success) throw new Exception("TIMEOUT!");

                ownSocket.Send(Encoding.Unicode.GetBytes(localIP));
                int result = ownSocket.Receive(message);

                string msg = Encoding.Unicode.GetString(message);
                msg = msg.Trim('\0');

                if (ServerMessageHasCome(msg) == 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                ownSocket.Close();
            }
        }

        override public bool SendMessage(string msg, IPAddress addrss = null)
        {
            Socket sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sc.ReceiveTimeout = 2000;
            try
            {
                message = new byte[1024];
                sc.Connect(server);
                sc.Send(Encoding.Unicode.GetBytes(msg));
                int a = sc.Receive(message);
                int b = ServerMessageHasCome(Encoding.Unicode.GetString(message).Trim('\0'));
                if (b == -2) return false;
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
                sc.Connect(new IPEndPoint(IPAddress.Parse(localIP), MyNetWorkBase.PORT2));

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

        override public void Disconnect()
        {
            if (!Connected) return;
            bool a = false;
            if (SendMessage("LEAVED:" + Number))
                a = StopListening();
        }
        int ServerMessageHasCome(string msg)
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
                            localIP = msg;
                            num = 0;
                        } break;
                    case 1:
                        {
                            msg = rg.Replace(msg, "$2", 1);

                            string[] players = msg.Split('|');

                            if (players.Length == 1)
                            {
                                //return -2;
                            }

                            _number = 0;
                            _players.Clear();
                            for (int j = 0; j < players.Length; j++)
                            {
                                _players.Add(players[j]);
                                if (players[j] == _name) _number = (byte)i;
                            }


                            string m = "";
                            foreach (var n in _players) m += n + "\n";

                            num = 1;
                        } break;

                    case 2:
                        {
                            runningFlag = false;
                            num = 2;
                            OnGameEnd();
                        } break;
                    case 3:
                        double spX = 0, spY = 0;
                        double.TryParse(rg.Replace(msg, "$2", 1).Split(':')[1], out spX);
                        double.TryParse(rg.Replace(msg, "$3", 1).Split(':')[1], out spY);
                        OnGameStatring(this, new BallRunningEventArgs { VX = spX, VY = spY });                     
                        break;
                    case 4:
                        spX = spY = 0;
                        double x = 0, y = 0;
                        double.TryParse(rg.Replace(msg, "$2", 1).Split(':')[1], out spX);
                        double.TryParse(rg.Replace(msg, "$3", 1).Split(':')[1], out spY);
                        double.TryParse(rg.Replace(msg, "$4", 1).Split(':')[1], out x);
                        double.TryParse(rg.Replace(msg, "$5", 1).Split(':')[1], out y);
                        OnGameSync(this, new BallRunningEventArgs { VX = spX, VY = spY, X =x, Y = y });
                        break;
                    case 5:
                        char letter = rg.Replace(msg, "$2", 1)[0];
                        if (letter == 'L')
                            OnBarrierMoving(System.Windows.Input.Key.Left);
                        else if (letter == 'R')
                            OnBarrierMoving(System.Windows.Input.Key.Right);
                        break;
                    case 6:
                        OnAnotherPlayerMissed();
                        break;
                }
                break;
            }

            //if (!command) MessageBox.Show(msg);
            return num;
        }
    }
}
