/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;


namespace xmedianet.socketserver
{

    ///  No server components in windows phone, just client for now until we need it
#if !WINDOWS_PHONE

    public enum ServerSessionState
    {
        None,
        WaitingForMethodSelections,
        WaitingForSocksRequestMessage,
        JustExisting,
        Forwarding
    }

    public enum SOCKSServerMode
    {
        NormalSOCKS5Server,
        XMPPSOCKS5ByteStream,
    }

    public class SOCKSServerSession : SocketClient
    {
        public SOCKSServerSession(Socket s, SOCKServer parent)
        {
            Init(s, null);
            Parent = parent;
            ConnectClient = new SocketClient();
            ConnectClient.DisconnectHandler += new SocketEventHandler(ConnectClient_DisconnectHandler);
            ConnectClient.ReceiveHandlerBytes += new SocketReceiveHandler(ConnectClient_ReceiveHandlerBytes);
        }

        private SOCKSServerMode m_eSOCKSServerMode = SOCKSServerMode.NormalSOCKS5Server;
        public SOCKSServerMode SOCKSServerMode
        {
            get { return m_eSOCKSServerMode; }
            set { m_eSOCKSServerMode = value; }
        }

        private string m_strRemoteHost = "";

        public string RemoteHost
        {
            get { return m_strRemoteHost; }
            set { m_strRemoteHost = value; }
        }


        void ConnectClient_ReceiveHandlerBytes(SocketClient client, byte[] bData, int nLength)
        {
            if (this.Connected == true)
            {
                Send(bData, nLength);
            }
        }

        void ConnectClient_DisconnectHandler(object sender, EventArgs e)
        {
            if (this.Connected == true)
            {
                this.Disconnect();
            }
        }

        public override void OnDisconnect(string strReason)
        {
            base.OnDisconnect(strReason);
            try
            {
                if ((this.ConnectClient != null) && (this.ConnectClient.Connected == true))
                {
                    this.ConnectClient.Disconnect();
                    this.ConnectClient = null;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                this.ConnectClient = null;
            }
        }




        SOCKServer Parent = null;
        SocketClient ConnectClient = null;


        public void Start()
        {
            ServerSessionState = ServerSessionState.WaitingForMethodSelections;
            DoAsyncRead();
        }

        ByteBuffer ReceiveBuffer = new ByteBuffer();

        private ServerSessionState m_eServerSessionState = ServerSessionState.None;

        public ServerSessionState ServerSessionState
        {
            get { return m_eServerSessionState; }
            set { m_eServerSessionState = value; }
        }

        protected override void OnRecvData(byte[] bData, int nLen)
        {
            if (nLen == 0)
            {
                OnDisconnect("Normal closing");
                return;
            }

            if (ServerSessionState == ServerSessionState.Forwarding)
            {
                try
                {
                    if (ConnectClient != null) /// connected client may be null if we are acting as a socks5 bytestream proxy, where we don't actually forward anything
                        ConnectClient.Send(bData, nLen);

                    DoAsyncRead();
                }
                catch (Exception)
                { }

                return;
            }

            ReceiveBuffer.AppendData(bData, 0, nLen);

            if (ServerSessionState == ServerSessionState.JustExisting)
            {
                DoAsyncRead(); // go read some more
                return;
            }

            byte[] bCurData = ReceiveBuffer.PeekAllSamples();

            if (bCurData.Length <= 0)
            {
                DoAsyncRead();
                return;
            }

            if (ServerSessionState == ServerSessionState.WaitingForMethodSelections)
            {
                int nVersion = bCurData[0];

                if (nVersion == 5)
                {
                    MethodSelectionsMessage msg = new MethodSelectionsMessage();
                    int nRead = msg.ReadFromBytes(bData, 0);
                    if (nRead > 0)
                    {
                        ReceiveBuffer.GetNSamples(nRead);

                        /// Determine which method we support
                        /// 
                        bool bCanDoNoAuth = false;
                        foreach (SockMethod method in msg.Methods)
                        {
                            if (method == SockMethod.NoAuthenticationRequired)
                            {
                                bCanDoNoAuth = true;
                                break;
                            }
                        }

                        if (bCanDoNoAuth == false)
                        {
                            MethodSelectedMessage retmsg = new MethodSelectedMessage();
                            retmsg.Version = 5;
                            retmsg.SockMethod = SockMethod.NoAcceptableMethods;
                            this.Send(retmsg.GetBytes());
                            this.Disconnect();
                            return;
                        }
                        else
                        {
                            ServerSessionState = ServerSessionState.WaitingForSocksRequestMessage;
                            MethodSelectedMessage retmsg = new MethodSelectedMessage();
                            retmsg.Version = msg.Version;
                            retmsg.SockMethod = SockMethod.NoAuthenticationRequired;
                            this.Send(retmsg.GetBytes());
                        }
                    }
                }
                else if (nVersion == 4)
                {
                    MethodSelectionsVersionFourMessage msg = new MethodSelectionsVersionFourMessage();
                    int nRead = msg.ReadFromBytes(bData, 0);
                    if (nRead > 0)
                    {
                        ReceiveBuffer.GetNSamples(nRead);


                        MethodSelectedVersionFourMessage reply = new MethodSelectedVersionFourMessage();
                        /// See what the man wants.  It appears that mozilla immediately starts sending data if we return success here, so let's do it
                        /// 
                        this.ServerSessionState = ServerSessionState.Forwarding;
                        /// Let's try to connect
                        /// 
                        bool bConnected = false;
                        if (msg.DomainName != null)
                            bConnected = ConnectClient.Connect(msg.DomainName, msg.DestinationPort, true);
                        else
                            bConnected = ConnectClient.Connect(msg.DestIPAddress.ToString(), msg.DestinationPort, true);

                        if (bConnected == false)
                        {
                            reply.SOCKS4Status = SOCKS4Status.RequestRejected;
                            Send(reply.GetBytes());
                            Disconnect();
                        }
                        else
                        {
                            reply.SOCKS4Status = SOCKS4Status.RequestGranted;
                            Send(reply.GetBytes());
                        }



                    }
                }
                else
                {
                    Console.WriteLine("Version {0} not supported", nVersion);
                    MethodSelectedMessage retmsg = new MethodSelectedMessage();
                    retmsg.Version = 5;
                    retmsg.SockMethod = SockMethod.NoAcceptableMethods;
                    this.Send(retmsg.GetBytes());
                    this.Disconnect();
                    return;
                }
            }
            else if (ServerSessionState == ServerSessionState.WaitingForSocksRequestMessage)
            {
                /// Read in our SocksRequestMessage
                /// 
                SocksRequestMessage reqmsg = new SocksRequestMessage();
                int nRead = reqmsg.ReadFromBytes(bData, 0);
                if (nRead > 0)
                {
                    ReceiveBuffer.GetNSamples(nRead);

                    if (reqmsg.Version != 0x05)
                        Console.WriteLine("No version 5, client wants version: {0}", reqmsg.Version);


                    //Parent.HandleRequest(reqmsg, this);
                    if (reqmsg.SOCKSCommand == SOCKSCommand.Connect)
                    {
                        /// See what the man wants.  It appears that mozilla immediately starts sending data if we return success here, so let's do it
                        /// 
                        bool bConnected = false;
                        if (SOCKSServerMode == SOCKSServerMode.NormalSOCKS5Server)
                        {
                            this.ServerSessionState = ServerSessionState.Forwarding;
                            /// Let's try to connect
                            /// 
                            if (reqmsg.AddressType == AddressType.DomainName)
                                bConnected = ConnectClient.Connect(reqmsg.DestinationDomain, reqmsg.DestinationPort, true);
                            else
                                bConnected = ConnectClient.Connect(reqmsg.DestinationAddress.ToString(), reqmsg.DestinationPort, true);
                        }
                        else
                        {
                            Console.WriteLine("Incoming SOCKS5 Bytestream Connect command to domain: {0}, remote endppoint is: {1}", reqmsg.DestinationDomain, this.socket.RemoteEndPoint);
                            RemoteHost = reqmsg.DestinationDomain;
                            bConnected = true;
                            this.ServerSessionState = ServerSessionState.JustExisting;
                        }

                        SocksReplyMessage reply = new SocksReplyMessage();

                        if (bConnected == false)
                        {
                            reply.SOCKSReply = SOCKSReply.ConnectionRefused;
                        }
                        else
                        {
                            reply.SOCKSReply = SOCKSReply.Succeeded;
                        }

                        Send(reply.GetBytes());
                    }
                    else
                    {
                        SocksReplyMessage reply = new SocksReplyMessage();
                        reply.SOCKSReply = SOCKSReply.CommandNotSupported;
                        reply.AddressType = AddressType.IPV4;
                        Send(reply.GetBytes());
                    }
                }
            }


            DoAsyncRead(); // go read some more
        }


        public override int Send(byte[] bData)
        {
            int nRet = 0;
            try
            {
                //Console.WriteLine(string.Format("--> {0}", ByteSize.ByteUtils.HexStringFromByte(bData, true)));
                nRet = base.Send(bData);
            }
            catch (Exception)
            {
                this.Disconnect();
            }
            return nRet;
        }

        public override int Send(byte[] bData, int nLength)
        {
            int nRet = 0;
            try
            {
                //Console.WriteLine(string.Format("--> {0}", ByteSize.ByteUtils.HexStringFromByte(bData, true)));
                nRet = base.Send(bData, nLength);
            }
            catch (Exception)
            {
                this.Disconnect();
            }
            return nRet;
        }

    }



    public class ListenPortToRemoteEndpointForwardService
    {
        public ListenPortToRemoteEndpointForwardService(SOCKServer parent)
        {
            Parent = parent;
            RemoteClient = new SocketClient();
            Listener = new SocketListener();
            Listener.OnNewConnection += new SocketListener.DelegateNewConnectedSocket(Listener_OnNewConnection);
            RemoteClient.ReceiveHandlerBytes += new SocketClient.SocketReceiveHandler(RemoteClient_ReceiveHandlerBytes);
            RemoteClient.DisconnectHandler += new SocketClient.SocketEventHandler(RemoteClient_DisconnectHandler);
        }


        SOCKServer Parent = null;
        SocketClient RemoteClient = null;
        SocketClient IncomingClient = null;
        SocketListener Listener = null;

        public SocksReplyMessage Start(string strRemoteHost, int nRemotePort)
        {
            SocksReplyMessage reply = new SocksReplyMessage();
            reply.AddressType = AddressType.IPV4;

            bool bAccept = Listener.EnableAccept(0);
            if (bAccept == false)
            {
                reply.SOCKSReply = SOCKSReply.GeneralServerFailure;
                return reply;
            }

            IPEndPoint BoundEp = Listener.ListeningSocket.LocalEndPoint as IPEndPoint;

            bool bConnected = RemoteClient.Connect(strRemoteHost, nRemotePort, false);
            if (bConnected == false)
            {
                reply.SOCKSReply = SOCKSReply.ConnectionRefused;
                Listener.Close();
                Listener = null;
                return reply;
            }

            reply.SOCKSReply = SOCKSReply.Succeeded;
            reply.BindAddress = BoundEp.Address;
            reply.BindPort = (ushort)BoundEp.Port;
            reply.AddressType = AddressType.IPV4;

            return reply;
        }

        void Listener_OnNewConnection(Socket s)
        {
            IncomingClient = new SocketClient(s, null);
            IncomingClient.ReceiveHandlerBytes += new SocketClient.SocketReceiveHandler(IncomingClient_ReceiveHandlerBytes);
            IncomingClient.DisconnectHandler += new SocketClient.SocketEventHandler(IncomingClient_DisconnectHandler);
            IncomingClient.DoAsyncRead();
            RemoteClient.DoAsyncRead();
        }



        void IncomingClient_ReceiveHandlerBytes(SocketClient client, byte[] bData, int nLength)
        {
            if ((RemoteClient != null) && (RemoteClient.Connected == true))
                RemoteClient.Send(bData, nLength);
        }

        void RemoteClient_ReceiveHandlerBytes(SocketClient client, byte[] bData, int nLength)
        {
            if ((IncomingClient != null) && (IncomingClient.Connected == true))
                IncomingClient.Send(bData, nLength);
        }

        void IncomingClient_DisconnectHandler(object sender, EventArgs e)
        {
            Parent.RemoveService(this);
            if (RemoteClient != null)
            {
                RemoteClient.Disconnect();
                RemoteClient = null;
            }
            if (Listener != null)
            {
                Listener.Close();
                Listener = null;
            }
        }

        void RemoteClient_DisconnectHandler(object sender, EventArgs e)
        {
            if (IncomingClient != null)
            {
                IncomingClient.Disconnect();
                IncomingClient = null;
            }
        }


    }




    public class SOCKServer
    {
        public SOCKServer()
        {
            Listener.OnNewConnection += new SocketListener.DelegateNewConnectedSocket(Listener_OnNewConnection);
        }

        private SOCKSServerMode m_eSOCKSServerMode = SOCKSServerMode.NormalSOCKS5Server;
        public SOCKSServerMode SOCKSServerMode
        {
            get { return m_eSOCKSServerMode; }
            set
            {
                m_eSOCKSServerMode = value;

            }
        }

        private int m_nPort = 8080;
        /// <summary>
        /// The port to listen on.  Pass in 0 for any port
        /// </summary>
        public int Port
        {
            get { return m_nPort; }
            set { m_nPort = value; }
        }

        SocketListener Listener = new SocketListener();

        public void Start()
        {
            Console.WriteLine("SOCKS server listening on port {0}", Port);
            Listener.EnableAccept(Port);
            Port = Listener.PortListeningOn;
        }

        public List<SOCKSServerSession> Sessions = new List<SOCKSServerSession>();
        public object SessionLock = new object();

        /// <summary>
        /// Look for an incoming byte stream session with the specified host.  For SOCKS5 bytestreams.
        /// 
        /// </summary>
        /// <param name="strHost"></param>
        public SOCKSServerSession GetIncomingByteStreamSession(string strHost)
        {
            Console.WriteLine("Host looking for SOCKS5 Bytestream connection for: {0}", strHost);
            lock (SessionLock)
            {
                foreach (SOCKSServerSession nextsession in Sessions)
                {
                    if (nextsession.RemoteHost == strHost)
                        return nextsession;
                }
            }

            return null;
        }

        public void CloseAndRemoveSession(SOCKSServerSession session)
        {
            try
            {
                session.Disconnect();
            }
            catch (Exception ex)
            {
            }

            lock (SessionLock)
            {
                if (Sessions.Contains(session) == true)
                    Sessions.Remove(session);
            }

        }

        void Listener_OnNewConnection(System.Net.Sockets.Socket s)
        {
            Console.WriteLine("Session Connecting: {0}", s.RemoteEndPoint);
            SOCKSServerSession session = new SOCKSServerSession(s, this);
            session.SOCKSServerMode = SOCKSServerMode;
            lock (SessionLock)
            {
                Sessions.Add(session);
            }

            session.DisconnectHandler += new SocketClient.SocketEventHandler(session_DisconnectHandler);
            session.Start();
        }

        void session_DisconnectHandler(object sender, EventArgs e)
        {
            Console.WriteLine("Session Disconnecting: {0}", sender);
            SOCKSServerSession session = sender as SOCKSServerSession;
            lock (SessionLock)
            {
                if (Sessions.Contains(session) == true)
                    Sessions.Remove(session);
            }
        }


        public List<ListenPortToRemoteEndpointForwardService> ConnectServices = new List<ListenPortToRemoteEndpointForwardService>();
        public object LockConnectServices = new object();

        public void RemoveService(ListenPortToRemoteEndpointForwardService service)
        {
            lock (LockConnectServices)
            {
                if (ConnectServices.Contains(service) == true)
                    ConnectServices.Remove(service);
            }
        }

        //public void HandleRequest(SocksRequestMessage reqmsg, SOCKSServerSession session)
        //{
        //   /// See what the man wants.. The client above handles connect request, not us
        //    /// 
        //    if (reqmsg.SOCKSCommand == SOCKSCommand.Connect)
        //    {
        //        /// Let's try to connect
        //        /// 
        //        ListenPortToRemoteEndpointForwardService service = new ListenPortToRemoteEndpointForwardService(this);
        //        SocksReplyMessage reply = null;
        //        if (reqmsg.AddressType == AddressType.DomainName)
        //            reply = service.Start(reqmsg.DestinationDomain, reqmsg.DestinationPort);
        //        else
        //            reply = service.Start(reqmsg.DestinationAddress.ToString(), reqmsg.DestinationPort);

        //        reply.BindAddress = IPAddress.Parse("127.0.0.1"); 

        //        session.Send(reply.GetBytes());
        //    }
        //    else
        //    {
        //        SocksReplyMessage reply = new SocksReplyMessage();
        //        reply.SOCKSReply = SOCKSReply.CommandNotSupported;
        //        reply.AddressType = AddressType.IPV4;
        //        session.Send(reply.GetBytes());
        //    }
        //}



    }

#endif

}