/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace xmedianet.socketserver.TLS
{
    public enum HandShakeMessageType : byte
    {
        HelloRequest = 0,
        ClientHello = 1,
        ServerHello = 2,
        Certificate = 11,
        ServerKeyExchange = 12,
        CertificateRequest = 13,
        ServerHelloDone = 14,
        CertificateVerify = 15,
        ClientKeyExchange = 16,
        Finished = 20,
    }


    public class RandomStruct
    {
        public UInt32 gmt_unix_time = (uint) GetUnixTime();
        public byte[] random_bytes = new byte[28];

        internal const long UNIX_BASE_TICKS = 621355968000000000;
        //  Stolen from mono
        public static int GetUnixTime()
        {
            DateTime now = DateTime.UtcNow;

            return (int)((now.Ticks - UNIX_BASE_TICKS) / TimeSpan.TicksPerSecond);
        }

        public void DebugDump()
        {
            System.Diagnostics.Debug.WriteLine("   RandomStruct -> gmt_unix_time: {0}, random_bytes: {1}\r\n", gmt_unix_time, ByteHelper.HexStringFromByte(random_bytes, true, int.MaxValue));
        }


        public byte[] Bytes
        {
            get
            {
                byte[] bRet = new byte[32];
                ByteHelper.WriteUInt32BigEndian(bRet, 0, gmt_unix_time);
                ByteHelper.WriteByteArray(bRet, 4, random_bytes);
                return bRet;
            }
        }

        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public uint ReadFromArray(byte[] bData, int nStartAt)
        {
            if (bData.Length < (nStartAt + 32))
                return 0;

            gmt_unix_time = ByteHelper.ReadUintBigEndian(bData, nStartAt);
            random_bytes = ByteHelper.ReadByteArray(bData, nStartAt + 4, 28);
            return 32;
        }
    }

    /// <summary>
    ///  Need additional messages for Handshakedata messages, including:
    ///  Client Hello
    ///  Server Hello
    ///  Certificates
    ///  Server Hello Done
    ///  Client Key Exchange
    ///  More...
    /// </summary>

    public class TLSHandShakeMessage : TLSMessage
    {
        public TLSHandShakeMessage()
        {
        }
        public override void DebugDump(bool bReceived)
        {
            System.Diagnostics.Debug.WriteLine("{0} TLSHandShakeMessage Type: {1}, Length {2}, Total Raw Data: \r\n{3}\r\n Parsed Content:\r\n", 
                bReceived ? "<--" : "-->", 
                HandShakeMessageType, 
                MessageLength, 
                ByteHelper.HexStringFromByte(this.Bytes, true, 32));

            if (HandShakeMessageType == TLS.HandShakeMessageType.ClientHello)
                HandShakeClientHello.DebugDump();
            else if (HandShakeMessageType == TLS.HandShakeMessageType.ServerHello)
                HandShakeServerHello.DebugDump();
            else if (HandShakeMessageType == TLS.HandShakeMessageType.ServerKeyExchange)
                HandShakeServerKeyExchange.DebugDump();
            else if (HandShakeMessageType == TLS.HandShakeMessageType.CertificateRequest)
                HandShakeCertificateRequest.DebugDump();
            else if (HandShakeMessageType == TLS.HandShakeMessageType.Certificate)
                HandShakeCertificateMessage.DebugDump();
            else if (HandShakeMessageType == TLS.HandShakeMessageType.ServerHelloDone)
                HandShakeServerHelloDone.DebugDump();
            else if (HandShakeMessageType == TLS.HandShakeMessageType.CertificateVerify)
                HandShakeCertificateVerify.DebugDump();
            else if (HandShakeMessageType == TLS.HandShakeMessageType.ClientKeyExchange)
                HandShakeClientKeyExchange.DebugDump();
            else if (HandShakeMessageType == TLS.HandShakeMessageType.Finished)
                HandShakeFinished.DebugDump();
        }


        private HandShakeMessageType m_eHandShakeMessageType = HandShakeMessageType.HelloRequest;

        public HandShakeMessageType HandShakeMessageType
        {
            get { return m_eHandShakeMessageType; }
            set { m_eHandShakeMessageType = value; }
        }
        private uint m_nMessageLength = 0;

        public uint MessageLength
        {
            get { return m_nMessageLength; }
            set { m_nMessageLength = value; }
        }

      
        public byte[] HandShakeData
        {
            get 
            {
                if (HandShakeMessageType == TLS.HandShakeMessageType.ClientHello)
                    return HandShakeClientHello.Bytes;
                else if (HandShakeMessageType == TLS.HandShakeMessageType.ServerHello)
                    return HandShakeServerHello.Bytes;
                else if (HandShakeMessageType == TLS.HandShakeMessageType.ServerKeyExchange)
                    return HandShakeServerKeyExchange.Bytes;
                else if (HandShakeMessageType == TLS.HandShakeMessageType.CertificateRequest)
                    return HandShakeCertificateRequest.Bytes;
                else if (HandShakeMessageType == TLS.HandShakeMessageType.Certificate)
                    return HandShakeCertificateMessage.Bytes;
                else if (HandShakeMessageType == TLS.HandShakeMessageType.ServerHelloDone)
                    return HandShakeServerHelloDone.Bytes;
                else if (HandShakeMessageType == TLS.HandShakeMessageType.CertificateVerify)
                    return HandShakeCertificateVerify.Bytes;
                else if (HandShakeMessageType == TLS.HandShakeMessageType.ClientKeyExchange)
                    return HandShakeClientKeyExchange.Bytes;
                else if (HandShakeMessageType == TLS.HandShakeMessageType.Finished)
                    return HandShakeFinished.Bytes;

                return null;
            }
            set 
            { 
                /// TODO.. build the appropriate message
                if (HandShakeMessageType == TLS.HandShakeMessageType.ClientHello)
                    HandShakeClientHello.ReadFromArray(value, 0);
                else if (HandShakeMessageType == TLS.HandShakeMessageType.ServerHello)
                    HandShakeServerHello.ReadFromArray(value, 0);
                else if (HandShakeMessageType == TLS.HandShakeMessageType.ServerKeyExchange)
                    HandShakeServerKeyExchange.ReadFromArray(value, 0);
                else if (HandShakeMessageType == TLS.HandShakeMessageType.CertificateRequest)
                    HandShakeCertificateRequest.ReadFromArray(value, 0);
                else if (HandShakeMessageType == TLS.HandShakeMessageType.Certificate)
                    HandShakeCertificateMessage.ReadFromArray(value, 0);
                else if (HandShakeMessageType == TLS.HandShakeMessageType.ServerHelloDone)
                    HandShakeServerHelloDone.ReadFromArray(value, 0);
                else if (HandShakeMessageType == TLS.HandShakeMessageType.CertificateVerify)
                    HandShakeCertificateVerify.ReadFromArray(value, 0);
                else if (HandShakeMessageType == TLS.HandShakeMessageType.ClientKeyExchange)
                    HandShakeClientKeyExchange.ReadFromArray(value, 0);
                else if (HandShakeMessageType == TLS.HandShakeMessageType.Finished)
                    HandShakeFinished.ReadFromArray(value, 0);
            }
        }

        // HandShakeData is a union of one of these:

        public HandShakeClientHello HandShakeClientHello = new HandShakeClientHello();
        public HandShakeServerHello HandShakeServerHello = new HandShakeServerHello();
        public HandShakeCertificateMessage HandShakeCertificateMessage = new HandShakeCertificateMessage();
        public HandShakeServerKeyExchange HandShakeServerKeyExchange = new HandShakeServerKeyExchange();
        public HandShakeCertificateRequest HandShakeCertificateRequest = new HandShakeCertificateRequest();
        public HandShakeServerHelloDone HandShakeServerHelloDone = new HandShakeServerHelloDone();
        public HandShakeCertificateVerify HandShakeCertificateVerify = new HandShakeCertificateVerify();
        public HandShakeClientKeyExchange HandShakeClientKeyExchange = new HandShakeClientKeyExchange();
        public HandShakeFinished HandShakeFinished = new HandShakeFinished();

        public override byte[] Bytes
        {
            get
            {
                byte[] bData = HandShakeData;
                MessageLength = (uint) bData.Length;

                byte[] bRet = new byte[bData.Length + 4];
                bRet[0] = (byte)HandShakeMessageType;
                ByteHelper.WriteUInt32BigEndian(bRet, 1, MessageLength, 3); // Only 24 bits
                ByteHelper.WriteByteArray(bRet, 4, bData);
                return bRet;
            }
        }


        /// <summary>
        /// The raw bytes that were read when "ReadFromArray" was called
        /// </summary>
        public byte [] m_bRawBytes = new byte [] {};
        public byte[] RawBytes
        {
            get
            {
                byte[] bData = m_bRawBytes;
                MessageLength = (uint)bData.Length;

                byte[] bRet = new byte[bData.Length + 4];
                bRet[0] = (byte)HandShakeMessageType;
                ByteHelper.WriteUInt32BigEndian(bRet, 1, MessageLength, 3); // Only 24 bits
                ByteHelper.WriteByteArray(bRet, 4, bData);
                return bRet;
            }
           
        }
        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public override uint ReadFromArray(byte[] bData, int nStartAt)
        {
            if (bData.Length < (nStartAt + 4))
                return 0;

            HandShakeMessageType = (HandShakeMessageType)bData[nStartAt + 0];
            MessageLength = ByteHelper.ReadUintBigEndian(bData, nStartAt + 1, 3); // 24 bits only

            /// See if we have enough data for our message length
            /// 
            if (bData.Length < (nStartAt + 4 + MessageLength))
                return 0;

            m_bRawBytes = ByteHelper.ReadByteArray(bData, nStartAt + 4, (int)MessageLength);
            HandShakeData = m_bRawBytes;// Parse out our sub-message

            return (4 + MessageLength);
        }

    }

    /// <summary>
    ///  Messages.... No Message Authentication Code (MAC) or padding is included in these, just the Protocol Message
    /// </summary>

    public class HandShakeClientHello
    {
        public HandShakeClientHello()
        {
            //TLSRecord.ContentType = TLSContentType.Handshake;
        }

        private ushort m_nVersion = 0x0301; /// TLS v1.1

        public ushort Version
        {
            get { return m_nVersion; }
            set { m_nVersion = value; }
        }


        public void DebugDump()
        {

            System.Diagnostics.Debug.WriteLine("HandShakeClientHello, Version: {0}", Version);
            RandomStruct.DebugDump();
            System.Diagnostics.Debug.WriteLine("   SessionId: {0}", ByteHelper.HexStringFromByte(SessionID, true, int.MaxValue));
            System.Diagnostics.Debug.WriteLine("   Cipher Suites:");
            foreach (CipherSuite suit in CipherSuites)
                System.Diagnostics.Debug.WriteLine("      {0}", suit);
            System.Diagnostics.Debug.WriteLine("   Compression Methods:");
            foreach (CompressionMethod method in CompressionMethods)
                System.Diagnostics.Debug.WriteLine("      {0}", method);
            System.Diagnostics.Debug.WriteLine("");

        }


        public RandomStruct RandomStruct = new RandomStruct();

        public byte SessionIDLength = 0;
        private byte[] m_bSessionID = new byte[] { };

        public byte[] SessionID
        {
            get { return m_bSessionID; }
            set
            {
                if (value.Length > 255)
                    throw new Exception("SessionID is too long, must be under 256 bytes");
                m_bSessionID = value; 
                SessionIDLength = (byte) m_bSessionID.Length; 
            }
        }
        
        ushort CipherSuitesLength = 0;

        public List<CipherSuite> CipherSuites = new List<CipherSuite>();

        byte CompressionMethodsLength = 0;
        public List<CompressionMethod> CompressionMethods = new List<CompressionMethod>();

        /// Todo... Add Extension methods

        public byte[] Bytes
        {
            get
            {
                SessionIDLength = 0;
                if ((SessionID != null) && (SessionID.Length > 0) )
                    SessionIDLength = (byte) SessionID.Length;

                CipherSuitesLength = (ushort) (CipherSuites.Count*2);
                CompressionMethodsLength = (byte) CompressionMethods.Count;


                int nLength = 2+ 32 + 1 + SessionIDLength + 2 + CipherSuites.Count * 2 + 1 + CompressionMethods.Count;
                byte[] bRet = new byte[nLength];

                int nAt = 0;
                ByteHelper.WriteUShortBigEndian(bRet, nAt, Version); nAt += 2;

                ByteHelper.WriteByteArray(bRet, nAt, RandomStruct.Bytes); nAt += 32;
                ByteHelper.WriteByte(bRet, nAt, SessionIDLength); nAt += 1;
                if (SessionIDLength > 0)
                    ByteHelper.WriteByteArray(bRet, nAt, SessionID); nAt += SessionIDLength;
               
                ByteHelper.WriteUShortBigEndian(bRet, nAt, CipherSuitesLength); nAt += 2;
                if (CipherSuitesLength > 0)
                {
                    foreach (ushort nNextCipher in CipherSuites)
                    {
                        ByteHelper.WriteUShortBigEndian(bRet, nAt, nNextCipher); nAt += 2;
                    }
                }

                ByteHelper.WriteByte(bRet, nAt, CompressionMethodsLength); nAt += 1;
                if (CompressionMethodsLength > 0)
                {
                    foreach (byte nNextCompressionMethod in CompressionMethods)
                    {
                        ByteHelper.WriteByte(bRet, nAt, nNextCompressionMethod); nAt += 1;
                    }
                }


                return bRet;
            }
        }

        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public uint ReadFromArray(byte[] bData, int nStartAt)
        {
            CipherSuites.Clear();
            CompressionMethods.Clear();

            uint nReadSoFar = 0;
            if (bData.Length < (nStartAt+35))
                return 0;

            Version = ByteHelper.ReadUshortBigEndian(bData, (int) (nStartAt + nReadSoFar)); nReadSoFar += 2;

            nReadSoFar += RandomStruct.ReadFromArray(bData, (int) (nStartAt+nReadSoFar));

            SessionIDLength = bData[nStartAt + nReadSoFar]; nReadSoFar += 1;
            if (SessionIDLength > 0)
            {
                if (bData.Length < (nStartAt + nReadSoFar + SessionIDLength)) return 0;
                this.SessionID = ByteHelper.ReadByteArray(bData, (int) (nStartAt + nReadSoFar), (int) SessionIDLength); nReadSoFar += SessionIDLength;
            }

            if (bData.Length < (nStartAt + nReadSoFar + 2)) return 0;
            CipherSuitesLength = ByteHelper.ReadUshortBigEndian(bData, (int)(nStartAt + nReadSoFar)); nReadSoFar += 2;

            if (CipherSuitesLength > 0)
            {
                if (bData.Length < (nStartAt + nReadSoFar + CipherSuitesLength)) return 0;
                for (int i = 0; i < CipherSuitesLength / 2; i++)
                {
                    CipherSuite nNextCipherSuite = (CipherSuite) ByteHelper.ReadUshortBigEndian(bData, (int)(nStartAt + nReadSoFar)); nReadSoFar += 2;
                    CipherSuites.Add(nNextCipherSuite);
                }
            }

            if (bData.Length < (nStartAt + nReadSoFar + 2)) return 0;
            CompressionMethodsLength = ByteHelper.ReadByte(bData, (int)(nStartAt + nReadSoFar)); nReadSoFar += 1;
            if (CompressionMethodsLength > 0)
            {
                if (bData.Length < (nStartAt + nReadSoFar + CompressionMethodsLength)) return 0;
                for (int i = 0; i < CipherSuitesLength / 2; i++)
                {
                    CompressionMethod bNextCompressionMethod = (CompressionMethod) ByteHelper.ReadByte(bData, (int)(nStartAt + nReadSoFar)); nReadSoFar += 1;
                    CompressionMethods.Add(bNextCompressionMethod);
                }
            }


            return nReadSoFar;
        }
    }


    public class HandShakeServerHello
    {
        public HandShakeServerHello()
        {
        }

        private ushort m_nVersion = 0x0301;

        public ushort Version
        {
            get { return m_nVersion; }
            set { m_nVersion = value; }
        }


        public void DebugDump()
        {

            System.Diagnostics.Debug.WriteLine("HandShakeServerHello, Version: {0}", Version);
            RandomStruct.DebugDump();
            System.Diagnostics.Debug.WriteLine("   SessionId: {0}", ByteHelper.HexStringFromByte(SessionID, true, int.MaxValue));
            System.Diagnostics.Debug.WriteLine("   Cipher Suite: {0}", CipherSuite);
            System.Diagnostics.Debug.WriteLine("   Compression Method: {0}", CompressionMethod);
            System.Diagnostics.Debug.WriteLine("");

        }

        public RandomStruct RandomStruct = new RandomStruct();

        public byte SessionIDLength = 0;
        private byte[] m_bSessionID = new byte[] { };

        public byte[] SessionID
        {
            get { return m_bSessionID; }
            set
            {
                if (value.Length > 255)
                    throw new Exception("SessionID is too long, must be under 256 bytes");
                m_bSessionID = value;
                SessionIDLength = (byte)m_bSessionID.Length;
            }
        }

        public CipherSuite CipherSuite = CipherSuite.TLS_NULL_WITH_NULL_NULL;
        public CompressionMethod CompressionMethod = CompressionMethod.null0;


        /// Todo... Add Extension methods

        public byte[] Bytes
        {
            get
            {
                SessionIDLength = 0;
                if ((SessionID != null) && (SessionID.Length > 0))
                    SessionIDLength = (byte)SessionID.Length;


                int nLength = 2 + 32 + 1 + SessionIDLength + 2 + 1;
                byte[] bRet = new byte[nLength];

                int nAt = 0;
                ByteHelper.WriteUShortBigEndian(bRet, nAt, Version); nAt += 2;

                ByteHelper.WriteByteArray(bRet, nAt, RandomStruct.Bytes); nAt += 32;
                ByteHelper.WriteByte(bRet, nAt, SessionIDLength); nAt += 1;
                if (SessionIDLength > 0)
                    ByteHelper.WriteByteArray(bRet, nAt, SessionID); nAt += SessionIDLength;

                ByteHelper.WriteUShortBigEndian(bRet, nAt, (ushort)CipherSuite); nAt += 2;
                ByteHelper.WriteByte(bRet, nAt, (byte)CompressionMethod); nAt += 1;

                return bRet;
            }
        }

        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public uint ReadFromArray(byte[] bData, int nStartAt)
        {
            uint nReadSoFar = 0;
            if (bData.Length < (nStartAt + 35))
                return 0;

            Version = ByteHelper.ReadUshortBigEndian(bData, (int)(nStartAt + nReadSoFar)); nReadSoFar += 2;

            nReadSoFar += RandomStruct.ReadFromArray(bData, (int) (nStartAt+nReadSoFar));

            SessionIDLength = bData[nStartAt + nReadSoFar]; nReadSoFar += 1;
            if (SessionIDLength > 0)
            {
                if (bData.Length < (nStartAt + nReadSoFar + SessionIDLength)) return 0;
                this.SessionID = ByteHelper.ReadByteArray(bData, (int)(nStartAt + nReadSoFar), (int)SessionIDLength); nReadSoFar += SessionIDLength;
            }

            CipherSuite = (CipherSuite) ByteHelper.ReadUshortBigEndian(bData, (int)(nStartAt + nReadSoFar)); nReadSoFar += 2;
            CompressionMethod = (CompressionMethod) ByteHelper.ReadByte(bData, (int)(nStartAt + nReadSoFar)); nReadSoFar += 1;

            return nReadSoFar;
        }
    }

    public enum KeyExchangeAlgorithm
    {
        rsa,
        diffie_hellman,
    }

    public enum SignatureAlgorithm
    {
        anonymous,
        rsa, 
        dsa
    }

    /// <summary> 
    /// 7.4.3
    ///  The server key exchange message is sent by the server only when
    ///  the server certificate message (if sent) does not contain enough
    ///  data to allow the client to exchange a premaster secret.  This is
    ///  true for the following key exchange methods:
    ///       DHE_DSS
    ///       DHE_RSA
    ///       DH_anon
    /// </summary>
    public class HandShakeServerKeyExchange
    {
        public HandShakeServerKeyExchange()
        {
            //TLSRecord.ContentType = TLSContentType.Handshake;
        }

        private KeyExchangeAlgorithm m_eKeyExchangeAlgorithm = KeyExchangeAlgorithm.rsa;

        /// <summary>
        /// Client must set this to determine how we parse our data
        /// </summary>
        public KeyExchangeAlgorithm KeyExchangeAlgorithm
        {
            get { return m_eKeyExchangeAlgorithm; }
            set { m_eKeyExchangeAlgorithm = value; }
        }

        private SignatureAlgorithm m_eSignatureAlgorithm = SignatureAlgorithm.anonymous;
        /// <summary>
        ///  Client must set this to determine who to parse signature data
        /// </summary>
        public SignatureAlgorithm SignatureAlgorithm
        {
            get { return m_eSignatureAlgorithm; }
            set { m_eSignatureAlgorithm = value; }
        }

        public byte[] RawData = new byte[] { };

        public void DebugDump()
        {
            System.Diagnostics.Debug.WriteLine("HandShakeServerKeyExchange:");
            System.Diagnostics.Debug.WriteLine("   RawData (may be interpreted as rsa params or dh params: {0}", ByteHelper.HexStringFromByte(RawData, true, 16));
            System.Diagnostics.Debug.WriteLine("");
        }

        /// <summary>
        /// Parses the raw data in the record into the appropriate structures using the keyexhange and signature algorithms
        /// provided.
        /// </summary>
        /// <param name="algo"></param>
        /// <param name="sig"></param>
        public void ParseRawData(KeyExchangeAlgorithm algo, SignatureAlgorithm sig)
        {
            KeyExchangeAlgorithm = algo;
            SignatureAlgorithm = sig;
            int nAt = 0;
            // Parse our RawData block into the structures below
            if (KeyExchangeAlgorithm == TLS.KeyExchangeAlgorithm.rsa)
            {

                RSAModulusLength = ByteHelper.ReadUshortBigEndian(RawData, nAt); nAt += 2;
                rsa_modulus = ByteHelper.ReadByteArray(RawData, nAt, RSAModulusLength); nAt += RSAModulusLength;

                RSAExponentLength = ByteHelper.ReadUshortBigEndian(RawData, nAt); nAt += 2;
                rsa_exponent = ByteHelper.ReadByteArray(RawData, nAt, RSAExponentLength); nAt += RSAExponentLength;
            }
            else if (KeyExchangeAlgorithm == TLS.KeyExchangeAlgorithm.rsa)
            {
                dh_p_length = ByteHelper.ReadUshortBigEndian(RawData, nAt); nAt += 2;
                dh_p = ByteHelper.ReadByteArray(RawData, nAt, dh_p_length); nAt += dh_p_length;

                dh_g_length = ByteHelper.ReadUshortBigEndian(RawData, nAt); nAt += 2;
                dh_g = ByteHelper.ReadByteArray(RawData, nAt, dh_g_length); nAt += dh_g_length;

                dh_Ys_length = ByteHelper.ReadUshortBigEndian(RawData, nAt); nAt += 2;
                dh_Ys = ByteHelper.ReadByteArray(RawData, nAt, dh_Ys_length); nAt += dh_Ys_length;

            }
            if ((SignatureAlgorithm == TLS.SignatureAlgorithm.dsa) || (SignatureAlgorithm == TLS.SignatureAlgorithm.rsa))
            {
                HashDigitalSignatureLength = ByteHelper.ReadUshortBigEndian(RawData, nAt); nAt += 2;
                DigitalSignatureOfHash = ByteHelper.ReadByteArray(RawData, nAt, HashDigitalSignatureLength); nAt += HashDigitalSignatureLength;
            }

        }




        /// ServerRSAParams
        /// 
        private ushort RSAModulusLength = 0;
        public byte[] rsa_modulus = new byte[] { }; //rsa mdulus n = p*q (p and q are prime numbers)

        private ushort RSAExponentLength = 0;
        public byte[] rsa_exponent = new byte[] { }; /// rsa public exponent, e


        /// Signature.  non-anonymous algorithms have these hashes for RSA 
        //public byte[] md5_hash = new byte[16]; ///MD5(ClientHello.random + ServerHello.random + ServerParams);
        //public byte[] sha_hash = new byte[20]; ///SHA(ClientHello.random + ServerHello.random + ServerParams);


        /// ServerDHParams
        /// See http://en.wikipedia.org/wiki/Diffie_Hellman
        private ushort dh_p_length = 0;
        public byte[] dh_p = new byte[] { }; // diffie helman shared prime
        
        private ushort dh_g_length = 0; /// diffie hellman shared base
        public byte[] dh_g = new byte[] { };
        
        private ushort dh_Ys_length = 0;
        public byte[] dh_Ys = new byte[] { }; // diffie hellman public share A=g^a mod p, where a is the server secret number

        /// Signature.  non-anonymous algorithms have these hashes for dsa
        //public byte[] sha_hash = new byte[20];

        /// <summary>
        /// SignatureAlgorithm rsa and dsa have a digitally signed element 
        /// of the md5_hash and sha_hash for rsa, or sha_hash for dsa.
        /// (See section 4.7 Cryptographic attributes)
        /// </summary>
        private ushort HashDigitalSignatureLength = 0;
        public byte[] DigitalSignatureOfHash = new byte[] { };

    
        public byte[] Bytes
        {
            get
            {
                int nLength = 0;

                if ( (SignatureAlgorithm == TLS.SignatureAlgorithm.dsa) || (SignatureAlgorithm == TLS.SignatureAlgorithm.rsa) )
                    nLength += 2 + DigitalSignatureOfHash.Length;

                if (KeyExchangeAlgorithm == TLS.KeyExchangeAlgorithm.rsa)
                {
                    nLength += 2 + rsa_modulus.Length + 2 + rsa_exponent.Length;
                }
                else if (KeyExchangeAlgorithm == TLS.KeyExchangeAlgorithm.rsa)
                {
                    nLength += 2 + dh_p.Length + 2 + dh_g.Length + 2 + dh_Ys.Length;
                }

                byte[] bRet = new byte[nLength];
                int nAt = 0;
                if (KeyExchangeAlgorithm == TLS.KeyExchangeAlgorithm.rsa)
                {
                    ByteHelper.WriteUShortBigEndian(bRet, nAt, (ushort) rsa_modulus.Length); nAt += 2;
                    ByteHelper.WriteByteArray(bRet, nAt, rsa_modulus); nAt += rsa_modulus.Length;
                    ByteHelper.WriteUShortBigEndian(bRet, nAt, (ushort)rsa_exponent.Length); nAt += 2;
                    ByteHelper.WriteByteArray(bRet, nAt, rsa_exponent); nAt += rsa_exponent.Length;
                }
                else if (KeyExchangeAlgorithm == TLS.KeyExchangeAlgorithm.rsa)
                {
                    ByteHelper.WriteUShortBigEndian(bRet, nAt, (ushort)dh_p.Length); nAt += 2;
                    ByteHelper.WriteByteArray(bRet, nAt, dh_p); nAt += dh_p.Length;
                    ByteHelper.WriteUShortBigEndian(bRet, nAt, (ushort)dh_g.Length); nAt += 2;
                    ByteHelper.WriteByteArray(bRet, nAt, dh_g); nAt += dh_g.Length;
                    ByteHelper.WriteUShortBigEndian(bRet, nAt, (ushort)dh_Ys.Length); nAt += 2;
                    ByteHelper.WriteByteArray(bRet, nAt, dh_Ys); nAt += dh_Ys.Length;
                }
                if ((SignatureAlgorithm == TLS.SignatureAlgorithm.dsa) || (SignatureAlgorithm == TLS.SignatureAlgorithm.rsa))
                {
                    ByteHelper.WriteUShortBigEndian(bRet, nAt, (ushort)DigitalSignatureOfHash.Length); nAt += 2;
                    ByteHelper.WriteByteArray(bRet, nAt, DigitalSignatureOfHash); nAt += DigitalSignatureOfHash.Length;
                }

                return bRet;
            }
        }


        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// Since this is called from the Handshake record, we know we get the whole array
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public uint ReadFromArray(byte[] bData, int nStartAt)
        {
            RawData = bData;
            return (uint) bData.Length;
        }
    }


    public class HandShakeCertificateMessage
    {
        public HandShakeCertificateMessage()
        {
            //TLSRecord.ContentType = TLSContentType.Handshake;
        }


        public void DebugDump()
        {
            System.Diagnostics.Debug.WriteLine("HandShakeCertificateMessage:\r\n");
            System.Diagnostics.Debug.WriteLine("  Certificates:\r\n");
            foreach (X509Certificate cert in Certificates)
               System.Diagnostics.Debug.WriteLine("     Value: {0}", ByteHelper.HexStringFromByte(cert.GetRawCertData(), true, int.MaxValue));
            System.Diagnostics.Debug.WriteLine("");
        }

        private uint CertificatesLength = 0;

        public List<X509Certificate> Certificates = new List<X509Certificate>();

        public byte[] Bytes
        {
            get
            {
                CertificatesLength = 0;
                foreach (X509Certificate bNextCert in Certificates)
                {
                    byte[] bCertData = bNextCert.GetRawCertData();
                    CertificatesLength += (uint)(3 + bCertData.Length);
                    
                }

                byte[] bRet = new byte[3+CertificatesLength];
                int nAt = 0;
                ByteHelper.WriteUInt32BigEndian(bRet, nAt, CertificatesLength, 3); nAt += 3;

                foreach (X509Certificate bNextCert in Certificates)
                {
                    byte [] bCertData = bNextCert.GetRawCertData();
                    ByteHelper.WriteUInt32BigEndian(bRet, nAt, (uint)bCertData.Length); nAt += 3;
                    ByteHelper.WriteByteArray(bRet, nAt, bCertData); nAt += bCertData.Length;
                }

                return bRet;
            }
        }

        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public uint ReadFromArray(byte[] bData, int nStartAt)
        {
            Certificates.Clear();
            uint nReadSoFar = 0;
            if (bData.Length < (nStartAt + 3))
                return 0;

            CertificatesLength = ByteHelper.ReadUintBigEndian(bData, (int)(nStartAt + nReadSoFar), 3); nReadSoFar += 3;

            if (bData.Length < (nStartAt + nReadSoFar+CertificatesLength))
                return 0;  //not enough data

            int nDataRemaining = (int) CertificatesLength;
            while (nDataRemaining > 0)
            {
                int nNextByteArrayLength = (int) ByteHelper.ReadUintBigEndian(bData, (int)(nStartAt + nReadSoFar), 3); nReadSoFar += 3;
                byte [] bNextCert = ByteHelper.ReadByteArray(bData, (int)(nStartAt + nReadSoFar), nNextByteArrayLength); nReadSoFar += (uint)nNextByteArrayLength;
                X509Certificate cert = new X509Certificate(bNextCert);

                Certificates.Add(cert);
                nDataRemaining -= (3 + nNextByteArrayLength);
            }

            return nReadSoFar;
        }
    }

    public enum CertificateType : byte
    {
        rsa_sign = 1,
        dss_sign = 2,
        rsa_fixed_dh = 3,
        dss_fixed_dh = 4,
        rsa_ephemeral_dh_RESERVED = 5,
        dss_ephemeral_dh_RESERVED = 6,
        fortezza_dms_RESERVED = 20,
        twofiftyfive = 255,
    }

    public class HandShakeCertificateRequest /// 7.4.4
    {
        public HandShakeCertificateRequest()
        {
        }

        public void DebugDump()
        {
            System.Diagnostics.Debug.WriteLine("HandShakeCertificateRequest:");
            System.Diagnostics.Debug.WriteLine("  CertificatesTypes:");
            foreach (CertificateType cert in CertificateTypes)
                System.Diagnostics.Debug.WriteLine("     {0}", cert);
            System.Diagnostics.Debug.WriteLine("  CertificateAuthorities:");
            foreach (byte[] certauth in CertificateAuthorities)
                System.Diagnostics.Debug.WriteLine("     {0}", ByteHelper.HexStringFromByte(certauth, true, int.MaxValue));
            System.Diagnostics.Debug.WriteLine("");
        }

        public List<CertificateType> CertificateTypes = new List<CertificateType>();

        ushort DistinguishedNamesLength = 0;
        public List<byte[]> CertificateAuthorities = new List<byte []>();


        public byte[] Bytes
        {
            get
            {
                DistinguishedNamesLength = 0;
                foreach (byte[] bNextDistinguishedName in CertificateAuthorities)
                    DistinguishedNamesLength += (ushort) (2 + bNextDistinguishedName.Length);

                int nLength = 1 + CertificateTypes.Count + 2 + DistinguishedNamesLength;
                byte[] bRet = new byte[nLength];

                int nAt = 0;

                ByteHelper.WriteByte(bRet, nAt, (byte)CertificateTypes.Count); nAt += 1;
                foreach (CertificateType type in CertificateTypes)
                {
                    ByteHelper.WriteByte(bRet, nAt, (byte)type); nAt += 1;
                }
                ByteHelper.WriteUShortBigEndian(bRet, nAt, DistinguishedNamesLength); nAt += 2;
                if (DistinguishedNamesLength > 0)
                {
                    foreach (byte[] bNextDN in CertificateAuthorities)
                    {
                        ByteHelper.WriteUShortBigEndian(bRet, nAt, (byte)bNextDN.Length); nAt += 2;
                        ByteHelper.WriteByteArray(bRet, nAt, bNextDN); nAt += bNextDN.Length;
                    }
                }

                return bRet;
            }
        }

        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public uint ReadFromArray(byte[] bData, int nStartAt)
        {
            int nRead =0;
            CertificateTypes.Clear();
            CertificateAuthorities.Clear();

            int nCertificateTypes = ByteHelper.ReadByte(bData, nStartAt + nRead); nRead += 1;
            for (int i = 0; i < nCertificateTypes; i++)
            {
                CertificateType type = (CertificateType)ByteHelper.ReadByte(bData, nStartAt + nRead); nRead += 1;
                CertificateTypes.Add(type);
            }
            DistinguishedNamesLength = ByteHelper.ReadUshortBigEndian(bData, nStartAt + nRead); nRead += 2;

            if (DistinguishedNamesLength > 0)
            {
                int nLeft = DistinguishedNamesLength;
                while (nLeft > 0)
                {
                    int nNextDNLen = ByteHelper.ReadUshortBigEndian(bData, (nStartAt + nRead)); nRead += 2;
                    byte[] bNextDN = ByteHelper.ReadByteArray(bData, nStartAt + nRead, nNextDNLen); nRead += nNextDNLen;
                    CertificateAuthorities.Add(bNextDN);
                    nLeft -= (nNextDNLen + 2);
                }
            }

            return (uint) nRead;
        }
    }


    public class HandShakeServerHelloDone
    {
        public HandShakeServerHelloDone()
        {
        }

        public byte[] Bytes
        {
            get
            {
                return new byte[] { };
            }
        }

        public void DebugDump()
        {
            System.Diagnostics.Debug.WriteLine("HandShakeServerHelloDone:");
            System.Diagnostics.Debug.WriteLine("");
        }

        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public uint ReadFromArray(byte[] bData, int nStartAt)
        {
            return 0;
        }
    }

    public class HandShakeCertificateVerify
    {
        public HandShakeCertificateVerify()
        {
            //TLSRecord.ContentType = TLSContentType.Handshake;
        }

        public void DebugDump()
        {
            System.Diagnostics.Debug.WriteLine("HandShakeCertificateVerify:");
            System.Diagnostics.Debug.WriteLine("   Signature: {0}", ByteHelper.HexStringFromByte(Signature, true, int.MaxValue));
            System.Diagnostics.Debug.WriteLine("");
        }

        public byte[] Signature = new byte[] { };

        public byte[] Bytes
        {
            get
            {
                byte[] bRet = new byte[2 + Signature.Length];
                ByteHelper.WriteUShortBigEndian(bRet, 0, (ushort) Signature.Length);
                ByteHelper.WriteByteArray(bRet, 2, Signature);
                return bRet;
            }
        }


        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public uint ReadFromArray(byte[] bData, int nStartAt)
        {
            ushort uLength = ByteHelper.ReadUshortBigEndian(bData, nStartAt);
            Signature = ByteHelper.ReadByteArray(bData, nStartAt + 2, uLength);
            return (uint) (Signature.Length + 2);
        }
    }

    public enum PublicValueEncoding
    {
        implicit_en,
        explicit_en,
    }

    /// <summary>
    /// 7.4.7
    /// Sends RSA-encrypted secret or Diffie-Hellman client parameters
    /// </summary>
    public class HandShakeClientKeyExchange
    {
        public HandShakeClientKeyExchange()
        {
            //TLSRecord.ContentType = TLSContentType.Handshake;
        }

        public KeyExchangeAlgorithm KeyExchangeAlgorithm = KeyExchangeAlgorithm.diffie_hellman;
        public PublicValueEncoding PublicValueEncoding = PublicValueEncoding.explicit_en;

        public void DebugDump()
        {
            System.Diagnostics.Debug.WriteLine("HandShakeClientKeyExchange:");
            System.Diagnostics.Debug.WriteLine("   EncryptedPreMasterSecret or DiffeHellmanPublicValueYc: {0}", ByteHelper.HexStringFromByte(EncryptedPreMasterSecret, true, int.MaxValue));
            System.Diagnostics.Debug.WriteLine("");
        }

        /// Parameters for KeyExchangeAlgorithm = KeyExchangeAlgorithm.rsa
        /// EncryptedPreMasterSecret
        /// 
        public ushort client_version = 0x0301;
        public byte[] random = new byte[46];
        public byte[] PreMasterSecret
        {
            get
            {
                byte[] bRet = new byte[48];
                ByteHelper.WriteUShortBigEndian(bRet, 0, client_version);
                ByteHelper.WriteByteArray(bRet, 2, random);
                return bRet;
            }
            set
            {
                if (value.Length != 48)
                    throw new Exception("PreMasterSEcret must be 48 bytes for rsa");
                client_version = ByteHelper.ReadUshortBigEndian(value, 0);
                random = ByteHelper.ReadByteArray(value, 2, 46);
            }
        }

        /// <summary>
        /// Client must encrypt the PreMasterSecret and set this EncryptedPreMasterSecret before sending this
        /// </summary>
        public byte[] EncryptedPreMasterSecret = new byte[] { };



        /// Parameters for KeyExchangeAlgorithm = KeyExchangeAlgorithm.diffie_hellman
        ///ClientDiffieHellmanPublic
        /// Must supply the Diffie Hellman Client calculation if using Diffie Hellman
        /// and it's not supplied in the certificate (explicit)
        public byte[] DiffeHellmanPublicValueYc = new byte[] { };


        public byte[] Bytes
        {
            get
            {
                if (KeyExchangeAlgorithm == TLS.KeyExchangeAlgorithm.rsa)
                {
                    byte[] bRet = new byte[2 + EncryptedPreMasterSecret.Length];
                    ByteHelper.WriteUShortBigEndian(bRet, 0, (ushort)(EncryptedPreMasterSecret.Length));
                    ByteHelper.WriteByteArray(bRet, 2, EncryptedPreMasterSecret);
                    return bRet;
                }
                else if (KeyExchangeAlgorithm == TLS.KeyExchangeAlgorithm.diffie_hellman)
                {
                    if (PublicValueEncoding == TLS.PublicValueEncoding.implicit_en)
                        return new byte[] { };

                    byte[] bRet = new byte[2 + DiffeHellmanPublicValueYc.Length];
                    ByteHelper.WriteUShortBigEndian(bRet, 0, (ushort)(DiffeHellmanPublicValueYc.Length));
                    ByteHelper.WriteByteArray(bRet, 2, DiffeHellmanPublicValueYc);
                    return bRet;

                }
                return null;
            }
        }

        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public uint ReadFromArray(byte[] bData, int nStartAt)
        {
            int nRead = 0;


            ushort nLength = ByteHelper.ReadUshortBigEndian(bData, (nStartAt + nRead)); nRead += 2;
            EncryptedPreMasterSecret = ByteHelper.ReadByteArray(bData, nStartAt + nRead, (int)nLength); nRead += nLength;
            DiffeHellmanPublicValueYc = EncryptedPreMasterSecret;
            return (uint)nRead;
        }
    }

    public class HandShakeFinished
    {
        public HandShakeFinished()
        {
            //TLSRecord.ContentType = TLSContentType.Handshake;
        }

        public void DebugDump()
        {
            System.Diagnostics.Debug.WriteLine("HandShakeFinished:");
            System.Diagnostics.Debug.WriteLine(ByteHelper.HexStringFromByte(verify_data, true, int.MaxValue));
        }

        public byte[] verify_data = new byte[12];
      

        public byte[] Bytes
        {
            get
            {
                return verify_data;
            }
        }


        /// <summary>
        /// Reads this object in from an array, returning how many bytes were read, or 0 if there is an error or not enough data
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="nStartAt"></param>
        /// <returns></returns>
        public uint ReadFromArray(byte[] bData, int nStartAt)
        {
            verify_data = ByteHelper.ReadByteArray(bData, nStartAt, 12);
            return 12;
        }
    }

}
