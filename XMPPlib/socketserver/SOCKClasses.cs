/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using System.IO;
using System.Net.Sockets;

/// Classes used for parsing SOCKS4 and SOCKS5 messages


namespace xmedianet.socketserver
{
    public class SocksMessage
    {
        public SocksMessage()
        {
        }

        protected byte[] m_bBytes = new byte[] { };
        public virtual byte[] GetBytes()
        {
            return m_bBytes;
        }
        
        public virtual int ReadFromBytes(byte[] bBytes, int nAt)
        {
            return 0;
        }
    }

    public enum SockMethod : byte
    {
        NoAuthenticationRequired =0,
        GSSAPI = 1,
        UserNamePassword = 2,
        NoAcceptableMethods = 0xFF,
    }

    public class MethodSelectionsMessage : SocksMessage
    {
        public MethodSelectionsMessage()
        {
        }

        private byte m_bVersion = 0x05;

        public byte Version
        {
            get { return m_bVersion; }
            set { m_bVersion = value; }
        }

        private List<SockMethod> m_listMethods = new List<SockMethod>();

        public List<SockMethod> Methods
        {
            get { return m_listMethods; }
            set { m_listMethods = value; }
        }



        public override byte[] GetBytes()
        {
            m_bBytes = new byte[2 + Methods.Count];
            m_bBytes[0] = Version;
            m_bBytes[1] = (byte) Methods.Count;
            for (int i = 0; i < Methods.Count; i++)
                m_bBytes[2 + i] = (byte)Methods[i];

            return m_bBytes;
        }

        public override int ReadFromBytes(byte[] bBytes, int nAt)
        {
            if (bBytes.Length < (nAt + 2))
                return 0;

            m_bVersion = bBytes[nAt];
            byte nLength = bBytes[nAt + 1];
            if (bBytes.Length < (nAt + 2 + nLength))
                return 0;

            Methods.Clear();
            for (int i = 0; i < nLength; i++)
            {
                SockMethod method = (SockMethod)bBytes[nAt + i + 2];
                Methods.Add(method);
            }

            return 2 + nLength;
        }

    }

    public enum SOCKS4CommandCode : byte
    {
        TCPStream = 0x01,
        TCPBinding = 0x02,
    }

    public class MethodSelectionsVersionFourMessage : SocksMessage
    {
        public MethodSelectionsVersionFourMessage()
        {
        }

        private byte m_bVersion = 0x04;
        public byte Version
        {
            get { return m_bVersion; }
            set { m_bVersion = value; }
        }

        private SOCKS4CommandCode m_eSOCKS4CommandCode = SOCKS4CommandCode.TCPStream;
        public SOCKS4CommandCode SOCKS4CommandCode
        {
            get { return m_eSOCKS4CommandCode; }
            set { m_eSOCKS4CommandCode = value; }
        }

        private ushort m_nDestinationPort = 0;
        public ushort DestinationPort
        {
            get { return m_nDestinationPort; }
            set { m_nDestinationPort = value; }
        }

        private IPAddress m_objDestIPAddress = IPAddress.Parse("0.0.0.1");
        public IPAddress DestIPAddress
        {
            get { return m_objDestIPAddress; }
            set { m_objDestIPAddress = value; }
        }

        private string m_strUserId = "";
        public string UserId
        {
            get { return m_strUserId; }
            set { m_strUserId = value; }
        }

        private string m_strDomainName = null;
        public string DomainName
        {
            get { return m_strDomainName; }
            set { m_strDomainName = value; }
        }


        public override byte[] GetBytes()
        {
            int nLength = 8 + UserId.Length + 1;
            if (DomainName != null) /// Using SOCKS4a, Domain name is valid, ip is bad
            {
                DestIPAddress = IPAddress.Parse("0.0.0.1");
                nLength = 8 + UserId.Length + 1 + DomainName.Length + 1;
            }
                
            m_bBytes = new byte[nLength];
            m_bBytes[0] = Version;
            m_bBytes[1] = (byte)SOCKS4CommandCode;
            m_bBytes[2] = (byte)((DestinationPort & 0xFF00) >> 8);
            m_bBytes[3] = (byte)((DestinationPort & 0x00FF) >> 0);

            byte[] bIP = DestIPAddress.GetAddressBytes();
            m_bBytes[4] = bIP[0];
            m_bBytes[5] = bIP[1];
            m_bBytes[6] = bIP[2];
            m_bBytes[7] = bIP[3];

            byte[] bUser = System.Text.UTF8Encoding.UTF8.GetBytes(UserId);
            byte[] bDomain = System.Text.UTF8Encoding.UTF8.GetBytes(DomainName);

            Array.Copy(bUser, 0, m_bBytes, 8, m_bBytes.Length);
            
            if (DomainName != null)
                Array.Copy(bDomain, 0, m_bBytes, 8 + 1 + bUser.Length, bDomain.Length);

            return m_bBytes;
        }

        public static int FindNull(byte[] bBytes, int nStartAt)
        {
            for (int i = nStartAt + 8; i < bBytes.Length; i++)
            {
                if (bBytes[i] == 0x00)
                {
                    return i;
                }
            }
            return -1;
        }

        public override int ReadFromBytes(byte[] bBytes, int nAt)
        {
            if (bBytes.Length < (nAt + 8))
                return 0;

            m_bVersion = bBytes[nAt];
            SOCKS4CommandCode = (SOCKS4CommandCode) bBytes[nAt + 1];
            DestinationPort = (ushort)((bBytes[nAt + 2] << 8) | (bBytes[nAt + 3]));

            int nRead = 4;

            byte[] bIP = new byte[4];
            Array.Copy(bBytes, nAt + nRead, bIP, 0, 4);
            DestIPAddress = new IPAddress(bIP);
            nRead += 4;

            int nNullAt = FindNull(bBytes, nAt+nRead);
            if (nNullAt == -1)
                return 0;
            int nUseLen = nNullAt - (nAt + nRead);
            byte[] bUser = new byte[nUseLen];
            Array.Copy(bBytes, nAt + nRead, bUser, 0, nUseLen);
            UserId = System.Text.UTF8Encoding.UTF8.GetString(bUser, 0, bUser.Length);
            nRead += bUser.Length + 1;
            
            /// See if domain is valid
            /// 
            if ((bIP[0] == 0x00) && (bIP[1] == 0x00) && (bIP[2] == 0x00) && (bIP[3] != 0x00))
            {
                nNullAt = FindNull(bBytes, nAt + nRead);
                if (nNullAt == -1)
                    return 0;
                int nDomainLen = nNullAt - (nAt + nRead);
                byte[] bDomain = new byte[nDomainLen];
                Array.Copy(bBytes, nAt + nRead, bUser, 0, nDomainLen);
                DomainName = System.Text.UTF8Encoding.UTF8.GetString(bDomain, 0, bDomain.Length);
                nRead += DomainName.Length + 1;
            }
            else
            {
                DomainName = null;
            }
         

            return nRead;
        }

    }

    public class MethodSelectedMessage : SocksMessage
    {
        public MethodSelectedMessage()
        {
        }

        private byte m_bVersion = 0x05;

        public byte Version
        {
            get { return m_bVersion; }
            set { m_bVersion = value; }
        }

        private SockMethod m_eSockMethod = SockMethod.NoAcceptableMethods;

        public SockMethod SockMethod
        {
            get { return m_eSockMethod; }
            set { m_eSockMethod = value; }
        }


        public override byte[] GetBytes()
        {
            m_bBytes = new byte[2];
            m_bBytes[0] = Version;
            m_bBytes[1] = (byte)SockMethod;

            return m_bBytes;
        }

        public override int ReadFromBytes(byte[] bBytes, int nAt)
        {
            if (bBytes.Length < (nAt + 2))
                return 0;

            m_bVersion = bBytes[nAt];
            SockMethod = (SockMethod) bBytes[nAt + 1];

            return 2;
        }
    }

    public enum SOCKS4Status
    {
        RequestGranted = 0x5a,
        RequestRejected = 0x5b,
        RequestFailedBecauseNoInetd = 0x5c,
        RequestFailedIdentity = 0x5d,
    }

    public class MethodSelectedVersionFourMessage: SocksMessage
    {
        public MethodSelectedVersionFourMessage()
        {
        }

        private SOCKS4Status m_eSOCKS4Status = SOCKS4Status.RequestGranted;

        public SOCKS4Status SOCKS4Status
        {
            get { return m_eSOCKS4Status; }
            set { m_eSOCKS4Status = value; }
        }


        public override byte[] GetBytes()
        {
            m_bBytes = new byte[8];
            m_bBytes[1] = (byte) SOCKS4Status;
            return m_bBytes;
        }

        public override int ReadFromBytes(byte[] bBytes, int nAt)
        {
            if (bBytes.Length < (nAt + 8))
                return 0;

            SOCKS4Status = (SOCKS4Status)bBytes[nAt + 1];
            return 8;
        }
    }

    public enum SOCKSCommand : byte
    {
        Connect = 0x01,
        Bind = 0x02,
        UDPAssociate = 0x03,
    }

    public enum AddressType : byte
    {
        IPV4 = 0x01,
        DomainName = 0x03,
        IPV6 = 0x04
    }


    public class SocksRequestMessage : SocksMessage
    {
        public SocksRequestMessage()
        {
        }

        private byte m_bVersion = 0x05;

        public byte Version
        {
            get { return m_bVersion; }
            set { m_bVersion = value; }
        }

        private SOCKSCommand m_eSOCKSCommand = SOCKSCommand.Connect;

        public SOCKSCommand SOCKSCommand
        {
            get { return m_eSOCKSCommand; }
            set { m_eSOCKSCommand = value; }
        }

        private byte m_bReserved = 0x00;

        public byte Reserved
        {
            get { return m_bReserved; }
            set { m_bReserved = value; }
        }

        private AddressType m_eAddressType = AddressType.IPV4;

        public AddressType AddressType
        {
            get { return m_eAddressType; }
            set { m_eAddressType = value; }
        }


        private IPAddress m_objDestinationAddress = IPAddress.Any;
        /// <summary>
        /// If address type is IPV4 or IPV6, this field should be populated
        /// </summary>
        public IPAddress DestinationAddress
        {
            get { return m_objDestinationAddress; }
            set { m_objDestinationAddress = value; }
        }

        private string m_strDestinationDomain = "";
        /// <summary>
        ///  If address type is domain, this field should be populate
        /// </summary>
        public string DestinationDomain
        {
            get { return m_strDestinationDomain; }
            set { m_strDestinationDomain = value; }
        }


        private ushort m_nDestinationPort = 0;

        public ushort DestinationPort
        {
            get { return m_nDestinationPort; }
            set { m_nDestinationPort = value; }
        }

        public override byte[] GetBytes()
        {
            int nLength = 6;
            if (AddressType == AddressType.IPV4)
                nLength += 4;
            else if (AddressType == AddressType.IPV6)
                nLength += 16;
            else if (AddressType == AddressType.DomainName)
                nLength += (1 + DestinationDomain.Length);

            m_bBytes = new byte[nLength];
            m_bBytes[0] = Version;
            m_bBytes[1] = (byte)SOCKSCommand;
            m_bBytes[2] = (byte)Reserved;
            m_bBytes[3] = (byte)AddressType;
            if (AddressType == AddressType.IPV4)
            {
                byte[] bAddr = DestinationAddress.GetAddressBytes();
                Array.Copy(bAddr, 0, m_bBytes, 4, bAddr.Length);

                m_bBytes[4 + bAddr.Length] = (byte)((DestinationPort & 0xFF00) >> 8);
                m_bBytes[4 + bAddr.Length + 1] = (byte)((DestinationPort & 0xFF) >> 0);
            }
            else if (AddressType == AddressType.IPV6)
            {
                byte[] bAddr = DestinationAddress.GetAddressBytes();
                Array.Copy(bAddr, 0, m_bBytes, 4, bAddr.Length);

                m_bBytes[4 + bAddr.Length] = (byte)((DestinationPort & 0xFF00) >> 8);
                m_bBytes[4 + bAddr.Length + 1] = (byte)((DestinationPort & 0xFF) >> 0);
            }
            else if (AddressType == AddressType.DomainName)
            {
                byte[] bAddr = System.Text.UTF8Encoding.UTF8.GetBytes(DestinationDomain);
                m_bBytes[4] = (byte) bAddr.Length;
                Array.Copy(bAddr, 0, m_bBytes, 4 + 1, bAddr.Length);

                m_bBytes[4 + 1 + bAddr.Length] = (byte)((DestinationPort & 0xFF00) >> 8);
                m_bBytes[4 + 1 + bAddr.Length + 1] = (byte)((DestinationPort & 0xFF) >> 0);
            }


            return m_bBytes;
        }

        public override int ReadFromBytes(byte[] bBytes, int nAt)
        {
            if (bBytes.Length < (nAt + 7))
                return 0;

            m_bVersion = bBytes[nAt];
            SOCKSCommand = (SOCKSCommand) bBytes[nAt + 1];
            Reserved = bBytes[nAt + 2];
            AddressType = (AddressType) bBytes[nAt + 3];
            if (AddressType == AddressType.IPV4)
            {
                if (bBytes.Length < (nAt + 4+4+2))
                    return 0;
                byte[] bAddr = new byte[4];
                Array.Copy(bBytes, nAt+4, bAddr, 0, 4);
                m_objDestinationAddress = new IPAddress(bAddr);
                DestinationPort = (ushort)((bBytes[nAt + 8] << 8) | (bBytes[nAt + 9]));
                return 10;
            }
            else if (AddressType == AddressType.IPV6)
            {
                if (bBytes.Length < (nAt + 4+16+2))
                    return 0;
                byte[] bAddr = new byte[4];
                Array.Copy(bBytes, nAt + 4, bAddr, 0, 16);
                m_objDestinationAddress = new IPAddress(bAddr);
                DestinationPort = (ushort)((bBytes[nAt + 20] << 8) | (bBytes[nAt + 21]));
                return 22;
            }
            else if (AddressType == AddressType.DomainName)
            {
                // Read until we find a 0 character
                int nStringLen = bBytes[nAt+4];
                if (bBytes.Length < (nAt + 4 + nStringLen + 2))
                    return 0;

                byte[] bDomain = new byte[nStringLen];
                Array.Copy(bBytes, nAt + 4+1, bDomain, 0, nStringLen);
                this.DestinationDomain = System.Text.UTF8Encoding.UTF8.GetString(bDomain, 0, bDomain.Length);

                DestinationPort = (ushort)((bBytes[nAt + 4 + bDomain.Length + 1] << 8) | (bBytes[nAt + 4 + bDomain.Length + 2]));
                return (4 + bDomain.Length + 1 + 2);
            }

            return 0;
        }
    }

    public enum SOCKSReply
    {
        Succeeded = 0x00,
        GeneralServerFailure = 0x01,
        ConnectionNotAllowed = 0x02,
        NetworkUnreachable = 0x03,
        HostUnreachable = 0x04,
        ConnectionRefused = 0x05,
        TTLExpired = 0x06,
        CommandNotSupported = 0x07,
        AddressTypeNotSupported = 0x08,
    }

    public class SocksReplyMessage : SocksMessage
    {
        public SocksReplyMessage()
        {
        }

        private byte m_bVersion = 0x05;

        public byte Version
        {
            get { return m_bVersion; }
            set { m_bVersion = value; }
        }

        private SOCKSReply m_eSOCKSReply = SOCKSReply.Succeeded;

        public SOCKSReply SOCKSReply
        {
            get { return m_eSOCKSReply; }
            set { m_eSOCKSReply = value; }
        }

        private byte m_bReserved = 0x00;

        public byte Reserved
        {
            get { return m_bReserved; }
            set { m_bReserved = value; }
        }

        private AddressType m_eAddressType = AddressType.IPV4;

        public AddressType AddressType
        {
            get { return m_eAddressType; }
            set { m_eAddressType = value; }
        }


        private IPAddress m_objBindAddress = IPAddress.Any;
        /// <summary>
        /// If address type is IPV4 or IPV6, this field should be populated
        /// </summary>
        public IPAddress BindAddress
        {
            get { return m_objBindAddress; }
            set { m_objBindAddress = value; }
        }

        private string m_strBindDomain = "";
        /// <summary>
        ///  If address type is domain, this field should be populate
        /// </summary>
        public string BindDomain
        {
            get { return m_strBindDomain; }
            set { m_strBindDomain = value; }
        }


        private ushort m_nBindPort = 0;

        public ushort BindPort
        {
            get { return m_nBindPort; }
            set { m_nBindPort = value; }
        }

        public override byte[] GetBytes()
        {
            int nLength = 6;
            if (AddressType == AddressType.IPV4)
                nLength += 4;
            else if (AddressType == AddressType.IPV6)
                nLength += 16;
            else if (AddressType == AddressType.DomainName)
                nLength += (1 + BindDomain.Length);

            m_bBytes = new byte[nLength];
            m_bBytes[0] = Version;
            m_bBytes[1] = (byte)SOCKSReply;
            m_bBytes[2] = (byte)Reserved;
            m_bBytes[3] = (byte)AddressType;
            if (AddressType == AddressType.IPV4)
            {
                byte[] bAddr = BindAddress.GetAddressBytes();
                Array.Copy(bAddr, 0, m_bBytes, 4, bAddr.Length);

                m_bBytes[4 + bAddr.Length] = (byte)((BindPort & 0xFF00) >> 8);
                m_bBytes[4 + bAddr.Length + 1] = (byte)((BindPort & 0xFF) >> 0);

            }
            else if (AddressType == AddressType.IPV6)
            {
                byte[] bAddr = BindAddress.GetAddressBytes();
                Array.Copy(bAddr, 0, m_bBytes, 4, bAddr.Length);

                m_bBytes[4 + bAddr.Length] = (byte)((BindPort & 0xFF00) >> 8);
                m_bBytes[4 + bAddr.Length + 1] = (byte)((BindPort & 0xFF) >> 0);

            }
            else if (AddressType == AddressType.DomainName)
            {
                byte[] bAddr = System.Text.UTF8Encoding.UTF8.GetBytes(BindDomain);
                m_bBytes[4] = 0;
                Array.Copy(bAddr, 0, m_bBytes, 4 + 1, bAddr.Length);

                m_bBytes[4+1+bAddr.Length] = (byte) ((BindPort&0xFF00)>>8);
                m_bBytes[4 + 1 + bAddr.Length+1] = (byte)((BindPort & 0xFF) >> 0);
            }


            return m_bBytes;
        }

        public override int ReadFromBytes(byte[] bBytes, int nAt)
        {
            if (bBytes.Length < (nAt + 7))
                return 0;

            m_bVersion = bBytes[nAt];
            SOCKSReply = (SOCKSReply)bBytes[nAt + 1];
            Reserved = bBytes[nAt + 2];
            AddressType = (AddressType)bBytes[nAt + 3];
            if (AddressType == AddressType.IPV4)
            {
                if (bBytes.Length < (nAt + 4 + 4 + 2))
                    return 0;
                byte[] bAddr = new byte[4];
                Array.Copy(bBytes, nAt + 4, bAddr, 0, 4);
                m_objBindAddress = new IPAddress(bAddr);
                BindPort = (ushort)((bBytes[nAt + 8] << 8) | (bBytes[nAt + 9]));
                return 10;
            }
            else if (AddressType == AddressType.IPV6)
            {
                if (bBytes.Length < (nAt + 4 + 16 + 2))
                    return 0;
                byte[] bAddr = new byte[4];
                Array.Copy(bBytes, nAt + 4, bAddr, 0, 16);
                m_objBindAddress = new IPAddress(bAddr);
                BindPort = (ushort)((bBytes[nAt + 20] << 8) | (bBytes[nAt + 21]));
                return 22;
            }
            else if (AddressType == AddressType.DomainName)
            {
                // Read until we find a 0 character
                int nStringLen = bBytes[nAt + 4];
                if (bBytes.Length < (nAt + 4 + nStringLen + 2))
                    return 0;

                byte[] bDomain = new byte[nStringLen];
                Array.Copy(bBytes, nAt + 4 + 1, bDomain, 0, nStringLen);
                this.BindDomain = System.Text.UTF8Encoding.UTF8.GetString(bDomain, 0, bDomain.Length);

                BindPort = (ushort)((bBytes[nAt + 4 + bDomain.Length + 1] << 8) | (bBytes[nAt + 4 + bDomain.Length + 2]));

                return (4 + bDomain.Length + 1);
            }

            return 0;
        }
    }


    public enum SOCKS5ClientSessionState
    {
        None,
        WaitingForMethodSelected,
        WaitingForFinalResponse,
    }
    /// <summary>
    /// Filter on our socket client.  Nothing is passed to the upper layer until this filter is satisfied
    /// This will be the top most filter, then TLS if applicable, then user protocols
    /// </summary>
    public class SOCKSTransform
    {
        public SOCKSTransform(SocketClient client)
        {
            Client = client;
        }

        SocketClient Client = null;
        int m_nSocksVersion = 5;
        public int SocksVersion
        {
            get { return m_nSocksVersion; }
            set { m_nSocksVersion = value; }
        }

        private string m_strSocksHost = "";
        public string SocksHost
        {
            get { return m_strSocksHost; }
            set { m_strSocksHost = value; }
        }

        private int m_nSocksPort = 8080;
        public int SocksPort
        {
            get { return m_nSocksPort; }
            set { m_nSocksPort = value; }
        }

        private string m_strRemoteHost = "";
        public string RemoteHost
        {
            get { return m_strRemoteHost; }
            set { m_strRemoteHost = value; }
        }

        private int m_nRemotePort = 0;
        public int RemotePort
        {
            get { return m_nRemotePort; }
            set { m_nRemotePort = value; }
        }

        private string m_strUser = "User";
        public string User
        {
            get { return m_strUser; }
            set { m_strUser = value; }
        }

        SOCKS5ClientSessionState SOCKS5ClientSessionState = SOCKS5ClientSessionState.None;

        private bool m_bFilterActive = false;

        public bool IsFilterActive
        {
            get { return m_bFilterActive; }
            set { m_bFilterActive = value; }
        }
   

        private byte[] m_bData = new byte[8192];
        void DoAsyncRead()
        {

#if WINDOWS_PHONE
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(m_bData, 0, m_bData.Length);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveComplete);
            Client.socket.ReceiveAsync(args);
#else
            Client.socket.BeginReceive(m_bData, 0, m_bData.Length, System.Net.Sockets.SocketFlags.None, new AsyncCallback(OnRecvDataAll), m_bData);
#endif

        }

#if WINDOWS_PHONE        
        private void ReceiveComplete(object sender, SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                throw new Exception("SocketAsyncEventArgs was null");
            }

            byte[] bData = (byte[])e.Buffer;

            int nLen = e.BytesTransferred;

            if (nLen == 0)
            {
                Client.NegotiationsFinishedFireConnected(false, "Disconnected");
                return;
            }

            if (bData == null)
            {
                Client.NegotiationsFinishedFireConnected(false, "No data passed to receive callback");
            }

                byte[] bPassIn = new byte[nLen];
                Array.Copy(bData, 0, bPassIn, 0, nLen);

          try
            {
                bool bFinished = HandleReceiveData(bPassIn);
                if (bFinished == true)
                    Client.NegotiationsFinishedFireConnected(true, "");
                else
                    DoAsyncRead();
            }
            catch (Exception ex)
            {
                Client.NegotiationsFinishedFireConnected(false, ex.ToString());
            }
        }

        
        public void Send(byte[] bData)
        {
           try
           {

                SocketAsyncEventArgs sendargs = new SocketAsyncEventArgs();
                sendargs.SetBuffer(bData, 0, bData.Length);
                sendargs.Completed += new EventHandler<SocketAsyncEventArgs>(sendargs_Completed);

                Client.socket.SendAsync(sendargs);
            }
            catch (Exception ex)
            {
                Client.NegotiationsFinishedFireConnected(false, ex.ToString());
            }
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

#else
        private void OnRecvDataAll(IAsyncResult ar)
        {

            m_bData = (byte[])ar.AsyncState;

            int nLen = 0;
            try
            {
                nLen = Client.socket.EndReceive(ar);
            }
            catch (Exception)
            {
                Client.NegotiationsFinishedFireConnected(false, "Exception in receive callback");
                return;
            }

            if (nLen == 0)
            {
                Client.NegotiationsFinishedFireConnected(false, "No data passed to receive callback");
                return;
            }
            byte[] bData = new byte[nLen];
            Array.Copy(m_bData, 0, bData, 0, nLen);

            try
            {
                bool bFinished = HandleReceiveData(bData);
                if (bFinished == true)
                    Client.NegotiationsFinishedFireConnected(true, "");
                else
                    DoAsyncRead();
            }
            catch (Exception ex)
            {
                Client.NegotiationsFinishedFireConnected(false, ex.ToString());
            }

        }

        public void Send(byte[] bData)
        {
            try
            {
                Client.socket.Send(bData);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Client.NegotiationsFinishedFireConnected(false, ex.ToString());
            }
        }

#endif

        public void Start()
        {
            DoAsyncRead();
            if (SocksVersion == 4)
            {
                MethodSelectionsVersionFourMessage msg = new MethodSelectionsVersionFourMessage();
                msg.SOCKS4CommandCode = SOCKS4CommandCode.TCPStream;
                msg.DomainName = RemoteHost;
                msg.DestinationPort = (ushort)RemotePort;
                msg.UserId = User;
                msg.Version = 0x04;
                byte[] bMsg = msg.GetBytes();
                Send(bMsg);
            }
            else if (SocksVersion == 5)
            {
                SOCKS5ClientSessionState = SOCKS5ClientSessionState.WaitingForMethodSelected;
                MethodSelectionsMessage msg = new MethodSelectionsMessage();
                msg.Version = 0x05;
                msg.Methods.Add(SockMethod.NoAuthenticationRequired);
                byte[] bMsg = msg.GetBytes();
                Send(bMsg);
            }
         
            else
            {
                
            }
        }

        ByteBuffer ReceiveBuffer = new ByteBuffer();

        public bool HandleReceiveData(byte[] bRecv)
        {
            ReceiveBuffer.AppendData(bRecv);
            byte[] bAllData = ReceiveBuffer.PeekAllSamples();

            /// Recieve our response
            /// 
            if (SocksVersion == 4)
            {
                MethodSelectedVersionFourMessage msg = new MethodSelectedVersionFourMessage();
                int nRead = msg.ReadFromBytes(bAllData, 0);
                if (nRead > 0)
                {
                    ReceiveBuffer.GetNSamples(nRead);
                    if (msg.SOCKS4Status == SOCKS4Status.RequestGranted)
                    {
                       return true;
                    }
                    else
                    {
                        throw new Exception("Failed to negotiate a SOCKS4 session");
                    }
                }
                return false;
            }
            else if (SocksVersion == 5)
            {
                if (SOCKS5ClientSessionState == SOCKS5ClientSessionState.WaitingForMethodSelected)
                {
                    MethodSelectedMessage msg = new MethodSelectedMessage();
                    int nRead = msg.ReadFromBytes(bAllData, 0);
                    if (nRead > 0)
                    {
                        ReceiveBuffer.GetNSamples(nRead);
                        if (msg.SockMethod != SockMethod.NoAuthenticationRequired)
                        {
                            throw new Exception("Failed to negotiate a SOCKS5 session");
                        }

                        SOCKS5ClientSessionState = SOCKS5ClientSessionState.WaitingForFinalResponse;

                        /// We're happy, send our connect request
                        SocksRequestMessage req = new SocksRequestMessage();
                        req.AddressType = AddressType.DomainName;
                        req.DestinationDomain = RemoteHost;
                        req.DestinationPort = (ushort)RemotePort;
                        req.Version = 0x05;
                        req.SOCKSCommand = SOCKSCommand.Connect;
                        byte[] bMsg = req.GetBytes();
                        Send(bMsg);
                    }
                    return false;
                }
                else if (SOCKS5ClientSessionState == SOCKS5ClientSessionState.WaitingForFinalResponse)
                {
                    SocksReplyMessage msg = new SocksReplyMessage();
                    int nRead = msg.ReadFromBytes(bAllData, 0);
                    if (nRead > 0)
                    {
                        ReceiveBuffer.GetNSamples(nRead);
                        if (msg.SOCKSReply == SOCKSReply.Succeeded)
                        {
                            return true;
                        }
                        else
                        {
                            throw new Exception(string.Format("Failed to negotiate a SOCKS5 session, error: {0}", msg.SOCKSReply));
                        }

                    }
                    return false;
                }

                return false;
            }
            else
            {
                throw new Exception("Unsupported Version");
            }
        }

    }
}
