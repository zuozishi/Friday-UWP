/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

using System.Collections.Generic;


namespace xmedianet.socketserver
{
    public delegate void DelegateConnectFinish(SocketClient client, bool bSuccess, string strErrors);

    /// <summary>
    /// A TCP connected socket.  Supports SOCKS4/SOCKS5 proxy and some TLS (RSA128/256 - AES) for use with windows phone
    /// A bit of a hack since windows phone sockets don't support streams and windows does, but works
    /// </summary>
    public class SocketClient
    {
        public delegate void SocketEventHandler(object sender, System.EventArgs e);
        public delegate void SocketReceiveHandler(SocketClient client, byte[] bData, int nLength);

        public SocketClient()
            : base()
        {
            this.Client = null;

            /// Add our SOCKS proxy filter in case we decide we need it
            SOCKStrans = new SOCKSTransform(this);
        }


        public SocketClient(ILogInterface logmgr, string strGuid)
            : this()
        {
            m_Logger = logmgr;
            OurGuid = strGuid;
        }

        public SocketClient(Socket s)
            : this()
        {
            Client = s;
        }

        public static bool ShowDebug = false;

        protected bool m_bSocketClientDisposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (m_bSocketClientDisposed == true)
                return;
            m_bSocketClientDisposed = true;


            OnAsyncConnectFinished = null;
            ReceiveHandler = null;
            ReceiveHandlerBytes = null;
            DisconnectHandler = null;


            m_Logger = null;
            this.TransformList.Clear();
            this.TransformList = null;
            this.Client = null;
            this.m_bData = null;
        }


        public static IBufferPool m_BufferPool = null;


        /// <summary>
        /// set for logging
        /// </summary>
        public ILogInterface m_Logger = null;
        public string OurGuid = "SocketClient";

        public event DelegateConnectFinish OnAsyncConnectFinished = null;
        public event SocketEventHandler ReceiveHandler = null;
        public event SocketReceiveHandler ReceiveHandlerBytes = null;
        public event SocketEventHandler DisconnectHandler = null;

        protected bool m_bStartReadOnConnect = true;

        protected System.Net.Sockets.Socket Client;
        public System.Net.Sockets.Socket socket
        {
            get
            {
                return Client;
            }
        }

        public bool Connected
        {
            get
            {
                return Client.Connected;
            }
        }

        SOCKSTransform SOCKStrans = null;
        xmedianet.socketserver.TLS.TLSClientTransform TLStrans = null;
        public bool StartTLS(string strServer)
        {
            /// Negotiate TLS, don't return until it's done
            /// (We would use SslStream in normal .NET, but that's not available here)
            /// TLS is implement as a message filter 
            TLStrans = new xmedianet.socketserver.TLS.TLSClientTransform(this);
            TransformList.Add(TLStrans);
            TLStrans.Activate();

            return false;
        }

        public List<IMessageFilter> TransformList = new List<IMessageFilter>();
        public byte[] TransformSendData(byte[] bData)
        {
            if (this.m_bSocketClientDisposed == true)
                return bData;

            if (bData == null)
                return bData;
            byte[] bRet = bData;

            try
            {
                foreach (IMessageFilter nextfilter in TransformList)
                {
                    if (nextfilter == null)
                        continue;

                    if (nextfilter.IsFilterActive == false)
                        continue;

                    bRet = nextfilter.TransformSendData(bData);
                    if ((bRet == null) || (bRet.Length <= 0))
                        return new byte[] { };
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null) 
                    m_Logger.LogError(ToString(), MessageImportance.Highest, "Exception transforming data: {0}", ex);
            }

            return bRet;
        }

        // Parsed incoming data into a list of packets
        public List<byte[]> TransformReceiveData(byte[] bData)
        {
            List<byte[]> ReturnList = new List<byte[]>();
            ReturnList.Add(bData);

            if (this.m_bSocketClientDisposed == true)
                return ReturnList;

            if (bData == null)
                return ReturnList;

            try
            {
                foreach (IMessageFilter nextfilter in TransformList)
                {
                    if (nextfilter == null)
                        continue;

                    if (nextfilter.IsFilterActive == false)
                        continue;

                    List<byte[]> TempList = new List<byte[]>();
                    foreach (byte[] bNextData in ReturnList)
                        TempList.AddRange(nextfilter.TransformReceiveData(bNextData));

                    if (TempList.Count <= 0) /// no data was returned... not enough to make a message
                        return new List<byte[]>();

                    ReturnList = TempList;
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null) 
                    m_Logger.LogError(ToString(), MessageImportance.Highest, "TransformReceiveData", "Exception transforming data: {0}", ex);
            }


            return ReturnList;
        }

        public bool StartReadOnConnect
        {
            get
            {
                return m_bStartReadOnConnect;
            }
            set
            {
                m_bStartReadOnConnect = value;
            }
        }

        public bool IsIPVersion6 = false;

        public void SetSOCKSProxy(int nSocksVersion, string strSocksHost, int nSocksport, string strUser)
        {
            SOCKStrans.SocksVersion = nSocksVersion;
            SOCKStrans.SocksHost = strSocksHost;
            SOCKStrans.SocksPort = nSocksport;
            SOCKStrans.User = strUser;
            SOCKStrans.IsFilterActive = true;
        }

        /// <summary>
        ///  Creates a tcp connection asyncronously.  The client muust call DoAsyncRead on connection completed
        /// </summary>
        /// <param name="ipaddr"></param>
        /// <param name="nport"></param>
        /// <returns></returns>
        public bool ConnectAsync(string ipaddr, int nport)
        {
            EndPoint EPhost = null;
            if (ipaddr.Length <= 0)
                return false;

            if (m_Logger != null)
                m_Logger.LogMessage(ToString(), MessageImportance.Medium, "Calling ConnectAsync to {0}:{1}", ipaddr, nport);

            if (SOCKStrans.IsFilterActive == true)
            {
                /// Set our proxy remote location just in case we've been activated
                SOCKStrans.RemoteHost = ipaddr;
                SOCKStrans.RemotePort = nport;
                ipaddr = SOCKStrans.SocksHost;
                nport = SOCKStrans.SocksPort;
            }

            try
            {
                EPhost = new DnsEndPoint(ipaddr, nport, AddressFamily.InterNetwork);
            }
            catch (Exception e) /// could not resolve host name
            {

                if (m_Logger != null) 
                    m_Logger.LogError(ToString(), MessageImportance.Highest, e.ToString());
                return false;
            }
            return ConnectAsync(EPhost);
        }

        public bool ConnectAsync(EndPoint EPhost)
        {
            UserInitiatedDisconnect = false;
            //Creates the Socket for sending data over TCP.
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connects to the host using IPEndPoint.
            try
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.RemoteEndPoint = EPhost;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnClientConnected);
                Client.ConnectAsync(args);
            }
            catch (SocketException e2)
            {
                if (m_Logger != null) 
                    m_Logger.LogError(ToString(), MessageImportance.Highest, e2.ToString());
                return false;
            }

            return true;
        }


        private void OnClientConnected(object sender, SocketAsyncEventArgs e)
        {
            if (Client.Connected == false)
            {

                OnConnected(false, e.SocketError.ToString());
                if (OnAsyncConnectFinished != null)
                    OnAsyncConnectFinished(this, false, e.SocketError.ToString());
                return;
            }

            if (SOCKStrans.IsFilterActive == true)
            {
                if (m_Logger != null)
                    m_Logger.LogMessage(ToString(), MessageImportance.Medium, "Starting TLS negotiation");

                SOCKStrans.Start();
                // SOCKS object will trigger the connection finished event when it's done
                return;
            }

            NegotiationsFinishedFireConnected(true, "");
        }


        internal void NegotiationsFinishedFireConnected(bool bSuccess, string strErrors)
        {
            if (bSuccess == false)
            {
                if (m_Logger != null)
                    m_Logger.LogError(ToString(), MessageImportance.Medium, "Failed TLS negotiation");

                try
                {
                    Client.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                }
                catch (Exception)
                {
                }
            }

            if (m_Logger != null)
                m_Logger.LogMessage(ToString(), MessageImportance.Medium, "Connected");

            OnConnected(bSuccess, strErrors);
            if (OnAsyncConnectFinished != null)
                OnAsyncConnectFinished(this, true, "");

            if ((m_bStartReadOnConnect) && (this.Connected == true))
                DoAsyncRead();
        }

        protected virtual void OnConnected(bool bSuccess, string strErrors)
        {
        }

        protected void FireDisconnectHandler(System.EventArgs args)
        {
            if (DisconnectHandler != null)
                DisconnectHandler(this, args);
        }

        public virtual int Send(string strLine)
        {
            if (this.m_bSocketClientDisposed == true)   
                return -1;

            byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(strLine);
            int nRet = Send(sendBytes);
            return nRet;
        }

        void sendargs_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= new EventHandler<SocketAsyncEventArgs>(sendargs_Completed);
            if (e.SocketError == SocketError.Success)
                return;

            if (e.BytesTransferred <= 0)
            {
            }
        }

        public virtual int Send(byte[] bData)
        {
            if (this.m_bSocketClientDisposed == true)
                return -1;

            if (bData == null)
                return -1;

            return Send(bData, bData.Length);
        }

        public virtual int Send(byte[] bData, int nLength)
        {
            return Send(bData, nLength, true);
        }

        public virtual int Send(byte[] bData, int nLength, bool bTransform)
        {
            if (this.m_bSocketClientDisposed == true)
                return -1;

            int nRet = -1;
            lock (SyncRoot)
            {
                if (Client != null && Client.Connected)
                {
                    try
                    {
                        if (bTransform == true)
                            bData = TransformSendData(bData);

                        if ((bData == null) || (bData.Length <= 0))
                            return 0;

                        SocketAsyncEventArgs sendargs = new SocketAsyncEventArgs();
                        sendargs.SetBuffer(bData, 0, bData.Length);
                        sendargs.Completed += new EventHandler<SocketAsyncEventArgs>(sendargs_Completed);

                        Client.SendAsync(sendargs);
                    }
                    catch (System.Net.Sockets.SocketException ex)
                    {
                        if (m_Logger != null)
                            m_Logger.LogError(ToString(), MessageImportance.Highest, ex.ToString());

                        OnDisconnect("Send failed");
                        throw new Exception("Send Failed", ex);
                    }
                    catch (System.Exception ex2)
                    {
                        if (m_Logger != null)
                            m_Logger.LogError(ToString(), MessageImportance.Highest, ex2.ToString());

                        OnDisconnect(ex2.ToString());
                        throw new Exception("Send Failed", ex2);
                    }
                }
                else
                {
                    OnDisconnect("Client not connected");
                }
            }
            return nRet;
        }

        public virtual void OnDisconnect(string strReason)
        {
            lock (SyncRoot)
            {
                try
                {
                    if (Client != null)
                        Client.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                }
                catch (SocketException e) /// winso
                {
                    string strError = string.Format("{0} - {1}", e.ErrorCode, e.ToString());
                    if (m_Logger != null)
                       m_Logger.LogError(ToString(), MessageImportance.Highest, strError);
                }
                catch (ObjectDisposedException e2) // socket was closed
                {
                    string strError = e2.ToString();
                    if (m_Logger != null)
                       m_Logger.LogError(ToString(), MessageImportance.Highest, strError);
                }
                catch (System.Exception)
                { }

                FinalClose();

                FireDisconnectHandler(new System.EventArgs());

            }
        }

        public object SyncRoot = new object();
        bool UserInitiatedDisconnect = false;
        public virtual bool Disconnect()
        {
            UserInitiatedDisconnect = true;
            lock (SyncRoot)  // don't want this to be called by multiple people at the same time
            {
                if (Client == null)
                    return true;

                if (Client.Connected == false)
                {
                    return true;
                }

                try
                {
                    Client.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                }
                catch (SocketException e) /// winso
                {
                    string strError = string.Format("{0} - {1}", e.ErrorCode, e.ToString());
                    if (m_Logger != null)
                        m_Logger.LogError(ToString(), MessageImportance.Highest, strError);
                }
                catch (ObjectDisposedException e2) // socket was closed
                {
                    string strError = e2.ToString();
                    if (m_Logger != null) 
                        m_Logger.LogError(ToString(), MessageImportance.Highest, strError);
                }
                catch (Exception ex)
                {
                    string strError = ex.ToString();
                    if (m_Logger != null) 
                        m_Logger.LogError(ToString(), MessageImportance.Highest, strError);
                }

            }
            return true;
        }

        private void FinalClose()
        {
            try
            {
                if (Client != null)
                    Client.Close();
            }
            catch (SocketException e)
            {
                if (m_Logger != null) 
                    m_Logger.LogError(ToString(), MessageImportance.Highest, e.ToString());
                return;
            }
            catch (Exception)
            {
            }
        }

        private byte[] m_bData = new byte[8192];
        public void DoAsyncRead()
        {
            lock (SyncRoot)  // don't want this to be called by multiple people at the same time
            {
                if (Client != null && Client.Connected)
                {

                    try
                    {
                        byte[] bData = null;
                        if (m_BufferPool != null)
                            bData = m_BufferPool.Checkout(4096);
                        else
                            bData = new byte[4096];

                        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                        args.SetBuffer(bData, 0, bData.Length);
                        args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveComplete);
                        Client.ReceiveAsync(args);
                    }
                    catch (System.Net.Sockets.SocketException sock)
                    {
                        if (m_Logger != null) 
                            m_Logger.LogError(ToString(), MessageImportance.Highest, sock.ToString());
                        System.Threading.Thread.Sleep(0);
                        OnDisconnect(sock.ToString());
                    }
                    catch (System.IO.IOException e)
                    {
                        if (m_Logger != null) 
                            m_Logger.LogError(ToString(), MessageImportance.Highest, e.ToString());
                        System.Threading.Thread.Sleep(0);
                        OnDisconnect(e.ToString());
                    }
                    catch (System.ObjectDisposedException e2)
                    {
                        if (m_Logger != null) 
                            m_Logger.LogError(ToString(), MessageImportance.Highest, e2.ToString());
                        OnDisconnect(e2.ToString());
                    }
                    catch (System.InvalidOperationException e3)
                    {
                        if (m_Logger != null) 
                            m_Logger.LogError(ToString(), MessageImportance.Highest, e3.ToString());
                        OnDisconnect(e3.ToString());
                    }
                    catch (System.Exception e4)
                    {
                        if (m_Logger != null)
                            m_Logger.LogError(ToString(), MessageImportance.Highest, e4.ToString());
                        OnDisconnect(e4.ToString());
                    }
                }
            }
        }

        
    
        /// <summary>
        /// Function is called by our read threads whenever data is received.  This function calls the
        /// OnRecvData function, which is the one that should be overriden by the user.
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveComplete(object sender, SocketAsyncEventArgs e)
        {
            if (Client == null)
            {
                return;
            }

            if (e == null)
            {
                throw new Exception("SocketAsyncEventArgs was null");
            }

            byte[] bData = (byte[])e.Buffer;

            int nLen = e.BytesTransferred;

            if (nLen == 0)
            {
                if (UserInitiatedDisconnect == false)
                   OnDisconnect("Graceful Disconnect");
                return;
            }

            if (bData == null)
            {
                throw new Exception("bData is null in SocketClient.OnRecvDataAll");
            }

            try
            {
                byte[] bPassIn = new byte[nLen];
                Array.Copy(bData, 0, bPassIn, 0, nLen);

                /// Check in our buffer, prevent pinning
                if (m_BufferPool != null)
                    m_BufferPool.Checkin(bData);

                OnRecvData(bPassIn, nLen);
            }
            catch (System.NullReferenceException exnull)
            {
                throw new Exception("Something is null here... not sure what", exnull);
            }

        }

        protected void FireReceiveHandler(byte[] bData, int nLen)
        {
            FireReceiveHandler(new SocketEventArgs(bData, nLen));
        }

        protected void FireReceiveHandler(SocketEventArgs args)
        {
            if (ReceiveHandler != null)
            {
                ReceiveHandler(this, args);
            }
            if (ReceiveHandlerBytes != null)
            {
                ReceiveHandlerBytes(this, args.m_data, args.Length);
            }
        }

        protected virtual void OnRecvData(byte[] bData, int nLen)
		{

           FireReceiveHandler(bData, nLen);

           List<byte[]> ReturnList = TransformReceiveData(bData);
           foreach (byte[] bNextArray in ReturnList)
           {
               if ((bNextArray != null) && (bNextArray.Length > 0))
                  OnMessage(bNextArray);
           }

		   DoAsyncRead(); // go read some more
		}

        public event SocketReceiveHandler OnReceiveMessage = null;

        // An individual message (as defined by the transform filter list), has been received and extracted from the array received
        protected virtual void OnMessage(byte[] bMessage)
        {
            if (OnReceiveMessage != null)
               OnReceiveMessage(this, bMessage, bMessage.Length);
        }

    }

}
