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

using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace xmedianet.socketserver.TLS
{
    /// <summary>
    /// When activated on a socket client, performs TLS negotiation and subsequently encrypts all data
    /// </summary>
    public class TLSClientTransform : IMessageFilter
    {

        public TLSClientTransform(SocketClient client)
        {
            Client = client;
        }

        protected SocketClient Client = null;

        private ClientTLSState m_eClientTLSState = ClientTLSState.None;

        public ClientTLSState ClientTLSState
        {
            get { return m_eClientTLSState; }
            set { m_eClientTLSState = value; }
        }


        public ManualResetEvent EventFinished = new ManualResetEvent(false);
        /// <summary>
        /// Tells this filter to start the TLS Client side negotiation.  By not automatically activating we can
        /// use this with XMPP which may need to start TLS after some TCP data has already been exchanged
        /// </summary>
        public void Activate()
        {
            EventFinished.Reset();
            m_bActive = true;

            SendClientHello();
        }

        bool m_bActive = false;
        public bool IsFilterActive 
        { 
            get
            {
                return m_bActive;
            }
        }

        void SendClientHello()
        {

            ClientTLSState = ClientTLSState.SentClientHello;
            /// Send out Client Hello
            /// 
            TLSHandShakeMessage msg = new TLSHandShakeMessage();
            msg.HandShakeMessageType = HandShakeMessageType.ClientHello;
            msg.HandShakeClientHello.CompressionMethods.Add(CompressionMethod.null0);
            //msg.HandShakeClientHello.CipherSuites.Add(CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA);
            msg.HandShakeClientHello.CipherSuites.Add(CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA);
            msg.HandShakeClientHello.Version = 0x0301;
            RNGCryptoServiceProvider.GetBytes(msg.HandShakeClientHello.RandomStruct.random_bytes);

            this.state.SecurityParameters.ClientRandom = msg.HandShakeClientHello.RandomStruct.Bytes;

            TLSRecord record = new TLSRecord();
            record.MajorVersion = 3;
            record.MinorVersion = 1;
            record.ContentType = TLSContentType.Handshake;
            record.Messages.Add(msg);

            SendTLSRecord(record, true);
        }

        public RNGCryptoServiceProvider RNGCryptoServiceProvider = new RNGCryptoServiceProvider();

        void SendTLSRecord(TLSRecord record, bool bAppend)
        {
            if (SocketClient.ShowDebug == true)
               record.DebugDump(false);

            if (bAppend == true)
            {
                AllHandShakeMessages.AppendData(record.Content);
                if (SocketClient.ShowDebug == true)
                   System.Diagnostics.Debug.WriteLine("AllHandShakeMessages Length is now {0}", AllHandShakeMessages.Size);
            }
            else
            {
            }

            /// Encrypt the record if we are in that state
            byte[] bEncryptedGenericBlockCipher = state.CompressEncryptOutgoingData(record);
            byte[] bSend = record.GetBytesWithEncryptedContent(bEncryptedGenericBlockCipher);


            Client.Send(bSend, bSend.Length, false);
        }
        

        #region IMessageFilter Members

        /// <summary>
        /// If we are negotiation TLS, no client data can be sent until we have finished
        /// </summary>
        /// <param name="bSend"></param>
        /// <returns></returns>
        public byte[] TransformSendData(byte[] bSend)
        {
            if (this.ClientTLSState != TLS.ClientTLSState.ApplicationData)
            {
                /// Not finished negotiating, queue our data
                /// 
                this.ClientSendQueue.AppendData(bSend);
                return null;
            }
            else
            {
                // fi, send the records here
                BuildAndSendApplicationRecords(bSend);
                return null;
            }
        }

        void BuildAndSendApplicationRecords(byte [] bData)
        {
            int nLengthRemaining = bData.Length;
            int nAt = 0;
            while (nLengthRemaining > 0)
            {
                int nLengthToSend = ((bData.Length - nAt) > TLSRecord.MaxUncompressedRecordSize) ? TLSRecord.MaxUncompressedRecordSize : bData.Length - nAt;

                TLSApplicationMessage msg = new TLSApplicationMessage();
                msg.ApplicationData = new byte[nLengthToSend];
                Array.Copy(bData, nAt, msg.ApplicationData, 0, nLengthToSend);

                TLSRecord record = new TLSRecord();
                record.ContentType = TLSContentType.Application;
                record.Messages.Add(msg);

                /// Encrypt the record if we are in that state
                byte[] bEncryptedGenericBlockCipher = state.CompressEncryptOutgoingData(record);
                byte[] bSend = record.GetBytesWithEncryptedContent(bEncryptedGenericBlockCipher);


                /// Leave this to the higer layer to send, though we could send it here
                Client.Send(bSend, bSend.Length, false);

                nAt += nLengthToSend;
                nLengthRemaining -= nLengthToSend;
            }

        }


        TLSRecord CurrentRecord = new TLSRecord();
        ByteBuffer Buffer = new ByteBuffer();

        /// <summary>
        /// Unencrypt received data, or perform a
        /// </summary>
        /// <param name="bReceived"></param>
        /// <returns></returns>
        public List<byte[]> TransformReceiveData(byte[] bReceived)
        {
            List<byte []> ListRet = new List<byte []>();
            if (m_bActive == false)
            {
                ListRet.Add(bReceived);
                return ListRet;
            }

            /// Parse out TLS packets, check our state, and decrypt or respond appropriately
            /// 
            /// TLS 1.0 used by spark doesn't encapsulate certain messages (like client hello)
            /// in a TLS Record, so it will need a different transform
            /// 
            /// Seems to work like this:
            /// --> Client Hello - Uses TLS Handshake protocol encapsulated in TLSRecord protocol messages (TLSHandShake class)

            /// Client Data - Uses TLS Record Protocol 

            uint nRead = 0;
            Buffer.AppendData(bReceived);
            do
            {
                byte[] bData = Buffer.PeekAllSamples();
                nRead = CurrentRecord.ReadFromArray(bData, 0, true);
                if (nRead > 0)
                {
                    Buffer.GetNSamples((int)nRead); /// advance the buffer
                    ListRet.AddRange(ParseAndHandleTLSRecords(CurrentRecord));
                }

            } while ( (nRead > 0) && (Buffer.Size > 0));


            return ListRet;
        }

        #endregion

        //SecurityParameters SecurityParameters = new SecurityParameters();
        internal ConnectionState state = new ConnectionState();
        //ConnectionState readstate = new ConnectionState();
        //ConnectionState writestate = new ConnectionState();
        //ConnectionState pendingreadstate = new ConnectionState();
        //ConnectionState pendingwritestate = new ConnectionState();

        internal void SendAlert(AlertLevel level, AlertDescription desc)
        {
            TLSRecord record = new TLSRecord();
            TLSAlertMessage msg = new TLSAlertMessage();
            msg.AlertLevel = level;
            msg.AlertDescription = desc;

            record.ContentType = TLSContentType.Alert;
            record.MajorVersion = 0x03;
            record.MinorVersion = 0x01;
            record.Messages.Add(msg);
            Client.Send(record.Bytes, record.Bytes.Length, false);
        }

        /// <summary>
        /// A new TLS record has been received... It may contain multiple messages, so these need to be parsed
        /// </summary>
        /// <param name="record"></param>
        List<byte[]> ParseAndHandleTLSRecords(TLSRecord record)
        {
            List<byte[]> ApplicationDataReturned = new List<byte[]>();

            /// Decrypt our message if we are at that stage
            /// 
            try
            {
                record = state.DecompressRecord(record);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("!!! Exception decompressing record: {0}", ex);

                SendAlert(AlertLevel.fatal, AlertDescription.BadRecordMAC);
                this.Client.Disconnect();
                return ApplicationDataReturned;
            }


            /// Let the record parse it's content - couldn't do that until now that it's decrypted 
            record.Content = record.RawSetContent;
            if (SocketClient.ShowDebug == true)
               record.DebugDump(true);

            foreach (TLSMessage tlsmsg in record.Messages)
            {
                if (record.ContentType == TLSContentType.Handshake)
                {

                    // Determine next handshake step
                    TLSHandShakeMessage msg = tlsmsg as TLSHandShakeMessage;

                    if (msg.HandShakeMessageType != HandShakeMessageType.Finished)
                    {
                        AllHandShakeMessages.AppendData(msg.RawBytes);
                        if (SocketClient.ShowDebug == true)
                           System.Diagnostics.Debug.WriteLine("AllHandShakeMessages, adding record of length {0}, Length is now {1}", record.RawSetContent.Length, AllHandShakeMessages.Size);
                    }
                    else
                    {
                    }

                    HandleHandshakeMessage(msg);
                }
                else if (record.ContentType == TLSContentType.Alert)
                {
                    TLSAlertMessage msg = tlsmsg as TLSAlertMessage;
                }
                else if (record.ContentType == TLSContentType.ChangeCipherSpec)
                {
                    TLSChangeCipherSpecMessage msg = tlsmsg as TLSChangeCipherSpecMessage;
                    HandleChangeCipherSpecMessage(msg);
                }
                else if (record.ContentType == TLSContentType.Application)
                {
                    // decrypt, add to ApplicationDataReturned
                    TLSApplicationMessage msg = tlsmsg as TLSApplicationMessage;
                    ApplicationDataReturned.Add(msg.ApplicationData);
                }
            }

            return ApplicationDataReturned;
        }

        ByteBuffer AllHandShakeMessages = new ByteBuffer();

        /// <summary>
        /// All the stuff the client wants to send, but can't because we're still handshaking.  We'll send it when we're done
        /// </summary>
        ByteBuffer ClientSendQueue = new ByteBuffer();
        bool m_bSendClientCertificate = false;
        void HandleHandshakeMessage(TLSHandShakeMessage msg)
        {
            /// Append all our handshake data because we'll need it later for some stupid reason

            if (msg.HandShakeMessageType == HandShakeMessageType.ServerHello)
            {
                ClientTLSState = TLS.ClientTLSState.ReceivedServerHello;
            }
            else if (msg.HandShakeMessageType == HandShakeMessageType.ServerHelloDone)
            {
                if (m_bSendClientCertificate == true) /// got a certificate request from the server, but we don't have any, so send an empty one
                {
                    TLSHandShakeMessage msgclientcert = new TLSHandShakeMessage();
                    msgclientcert.HandShakeMessageType = HandShakeMessageType.Certificate;
                    TLSRecord recordclientcert = new TLSRecord();
                    recordclientcert.MajorVersion = 3;
                    recordclientcert.MinorVersion = 1;
                    recordclientcert.ContentType = TLSContentType.Handshake;
                    recordclientcert.Messages.Add(msgclientcert);

                    SendTLSRecord(recordclientcert, true);
                }

                /// Server has sent the algorithm they want us to use, certificates, parameters, 
                /// and any request for certificates from us.  Now let's respond
                /// First generate, encrypt, and send the PreMasterSecret

                byte [] bKey = state.SecurityParameters.PeerCertificate.GetPublicKey();
                byte [] bPreMasterSecret = new byte[48];
                RNGCryptoServiceProvider.GetBytes(bPreMasterSecret);  /// Get some random bytes
                bPreMasterSecret[0] = 0x03; /// first two bytes get set to the version
                bPreMasterSecret[1] = 0x01;

                /// Openssl is showing our modulus as having an extra 00 on the front when using this command
                /// (ignore this, the modulus lower in the output doesn't show the 0)
                /// rsa -in test_key.pem -text

                if (SocketClient.ShowDebug == true)
                {
                    System.Diagnostics.Debug.WriteLine("Client Random: +++++++++++\r\n{0}\r\n++++++++++++", ByteHelper.HexStringFromByte(state.SecurityParameters.ClientRandom, true, 16));
                    System.Diagnostics.Debug.WriteLine("Server Random: +++++++++++\r\n{0}\r\n++++++++++++", ByteHelper.HexStringFromByte(state.SecurityParameters.ServerRandom, true, 16));
                    System.Diagnostics.Debug.WriteLine("PreMasterSecret: +++++++++++\r\n{0}\r\n++++++++++++", ByteHelper.HexStringFromByte(bPreMasterSecret, true, 16));
                }

                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                // Need to use this guys' class to parse the DER encoded ASN.1 described certificate to
                // extract the modulus and e from the certificate - these two are the public key
                RSAParameters rsaparams = Kishore.X509.Parser.X509PublicKeyParser.GetRSAPublicKeyParameters(bKey);
                provider.ImportParameters(rsaparams);

                //System.Diagnostics.Debug.WriteLine("Public key modulus: +++++++++++\r\n{0}\r\n++++++++++++", ByteHelper.HexStringFromByte(rsaparams.Modulus, true, 16));
                //System.Diagnostics.Debug.WriteLine("Public key exponent: +++++++++++\r\n{0}\r\n++++++++++++", ByteHelper.HexStringFromByte(rsaparams.Exponent, true, 16));
                //System.Diagnostics.Debug.WriteLine("Key Exchange Algorithm: {0}, KeySize: {1}", provider.KeyExchangeAlgorithm, provider.KeySize);
                byte[] EncryptedPreMasterSecret = provider.Encrypt(bPreMasterSecret, false);
                provider.Dispose();

                /// Send a Client Key Exchange method with our PreMasterSecret
                ///
                TLSHandShakeMessage msgsend = new TLSHandShakeMessage();
                msgsend.HandShakeMessageType = HandShakeMessageType.ClientKeyExchange;
                msgsend.HandShakeClientKeyExchange.EncryptedPreMasterSecret = EncryptedPreMasterSecret;
                msgsend.HandShakeClientKeyExchange.KeyExchangeAlgorithm = KeyExchangeAlgorithm.rsa;
                msgsend.HandShakeClientKeyExchange.PublicValueEncoding = PublicValueEncoding.explicit_en;
                TLSRecord record = new TLSRecord();
                record.MajorVersion = 3;
                record.MinorVersion = 1;
                record.ContentType = TLSContentType.Handshake;
                record.Messages.Add(msgsend);

                SendTLSRecord(record, true);




                /// Now generate all our keys, etc
                /// Section 8.1
                /// master_secret = PRF(pre_master_secret, "master secret",ClientHello.random + ServerHello.random) [0..47];

                // Combine ClientHello.Random and ServerHello.Random
                ByteBuffer buf = new ByteBuffer();
                buf.AppendData(state.SecurityParameters.ClientRandom);
                buf.AppendData(state.SecurityParameters.ServerRandom);
                byte [] bCSRandom = buf.GetAllSamples();

                // Do it in reverse order for different algorithms
                buf.AppendData(state.SecurityParameters.ServerRandom);
                buf.AppendData(state.SecurityParameters.ClientRandom);
                byte [] bSCRandom = buf.GetAllSamples();

                state.SecurityParameters.MasterSecret = state.PRF(bPreMasterSecret, "master secret", bCSRandom, 48);

                if (SocketClient.ShowDebug == true)
                   System.Diagnostics.Debug.WriteLine("MasterSecret: +++++++++++\r\n{0}\r\n++++++++++++", ByteHelper.HexStringFromByte(state.SecurityParameters.MasterSecret, true, 16));

                /// Verified that we are computing all our keys correctly by using the same input into the Mentalis library
                /// If we coded ours independently and get the same answers, it has to be correct, or we made the same mistakes :)
                state.ComputeKeys(state.SecurityParameters.MasterSecret, bSCRandom);

                

                /// Now send out a change cipher-spec, followed by a client finished message that is encrypted
                /// 
                TLSChangeCipherSpecMessage msgChangeCipherSpec = new TLSChangeCipherSpecMessage();
                msgChangeCipherSpec.CCSProtocolType = CCSProtocolType.Default;
                TLSRecord recordccs = new TLSRecord();
                recordccs.MajorVersion = 3;
                recordccs.MinorVersion = 1;
                recordccs.ContentType = TLSContentType.ChangeCipherSpec;
                recordccs.Messages.Add(msgChangeCipherSpec);
                SendTLSRecord(recordccs, false);

                state.SendEncryptionActive = true;
                state.WriteSequenceNumber = 0; /// reset on changecipherspec

                /// Now send a finished method, encrypted with our generated things
                /// RFC2246 section 7.4.9

                if (SocketClient.ShowDebug == true)
                   System.Diagnostics.Debug.WriteLine("FINAL:  AllHandShakeMessages Length is now {0}", AllHandShakeMessages.Size);

                SHA1Managed sha1 = new SHA1Managed();
                byte[] bAllHandshakeData = AllHandShakeMessages.PeekAllSamples();
                byte[] bmd5OfHandshakeMessages = MD5Core.GetHash(bAllHandshakeData);
                byte[] bsha1OfHandshakeMessages = sha1.ComputeHash(bAllHandshakeData);
                ByteBuffer bSum = new ByteBuffer();
                bSum.AppendData(bmd5OfHandshakeMessages);
                bSum.AppendData(bsha1OfHandshakeMessages);

                byte [] bCombinedHashes = bSum.GetAllSamples();

                /// No use in writing out this debug, only a few of the characters get seen, the rest are dropped
                ///System.Diagnostics.Debug.WriteLine("**** Start all handshake data ****\r\n{0}\r\n**** End all handshake data ****", ByteHelper.HexStringFromByte(bAllHandshakeData, true, 32));
                /// verify_data
                ///     PRF(master_secret, finished_label, MD5(handshake_messages) + SHA-1(handshake_messages)) [0..11];

                TLSHandShakeMessage msgFinished = new TLSHandShakeMessage();
                msgFinished.HandShakeMessageType = HandShakeMessageType.Finished;
                msgFinished.HandShakeFinished.verify_data = state.PRF(state.SecurityParameters.MasterSecret, "client finished", bCombinedHashes, 12);

                TLSRecord recordFinished = new TLSRecord();
                recordFinished.MajorVersion = 3;
                recordFinished.MinorVersion = 1;
                recordFinished.ContentType = TLSContentType.Handshake;
                recordFinished.Messages.Add(msgFinished);

                state.WriteSequenceNumber = 0;
                /// This record must now be encrypted before it can be sent
                SendTLSRecord(recordFinished, true);


                ClientTLSState = ClientTLSState.SentClientFinished;


            }
            else if (msg.HandShakeMessageType == HandShakeMessageType.Finished)
            {
                /// Got the server finished method, let's verify the data
                /// 
                SHA1Managed sha1 = new SHA1Managed();
                byte[] bAllHandshakeData = AllHandShakeMessages.GetAllSamples(); // no need to peek here, just get it all and clear it
                byte[] bmd5OfHandshakeMessages = MD5Core.GetHash(bAllHandshakeData);
                byte[] bsha1OfHandshakeMessages = sha1.ComputeHash(bAllHandshakeData);
                ByteBuffer bSum = new ByteBuffer();
                bSum.AppendData(bmd5OfHandshakeMessages);
                bSum.AppendData(bsha1OfHandshakeMessages);

                byte[] bCombinedHashes = bSum.GetAllSamples();

                /// No use in writing out this debug, only a few of the characters get seen, the rest are dropped
                ///System.Diagnostics.Debug.WriteLine("**** Start all handshake data ****\r\n{0}\r\n**** End all handshake data ****", ByteHelper.HexStringFromByte(bAllHandshakeData, true, 32));
                /// verify_data
                ///     PRF(master_secret, finished_label, MD5(handshake_messages) + SHA-1(handshake_messages)) [0..11];

                byte [] bComputedVerify = state.PRF(state.SecurityParameters.MasterSecret, "server finished", bCombinedHashes, 12);
                bool bMatch = ByteHelper.CompareArrays(msg.HandShakeFinished.verify_data, bComputedVerify);
                if (bMatch == false)
                {
                    SendAlert(AlertLevel.fatal, AlertDescription.HandshakeFailure);
                    Client.Disconnect();
                }
                else
                {
                    NegotiationFinished();
                }
            }
            else if (msg.HandShakeMessageType == HandShakeMessageType.CertificateRequest)
            {
                m_bSendClientCertificate = true;
            }



            if (ClientTLSState == TLS.ClientTLSState.ReceivedServerHello)
            {
                /// Gathering information
                /// 
                if (msg.HandShakeMessageType == HandShakeMessageType.ServerHello)
                {
                    state.SecurityParameters.Cipher = Cipher.FindCipher(msg.HandShakeServerHello.CipherSuite);
                    state.SecurityParameters.CompressionMethod = CompressionMethod.null0;
                    state.SecurityParameters.ConnectionEnd = ConnectionEnd.client;
                    state.SecurityParameters.ServerRandom = msg.HandShakeServerHello.RandomStruct.Bytes;
                }
                else if (msg.HandShakeMessageType == HandShakeMessageType.Certificate)
                {
                    if (msg.HandShakeCertificateMessage.Certificates.Count > 0)
                        state.SecurityParameters.PeerCertificate = msg.HandShakeCertificateMessage.Certificates[0];
                }
            }

           
        }

        void HandleChangeCipherSpecMessage(TLSChangeCipherSpecMessage msg)
        {
            /// Activate the new cipher protocol
            state.ReceiveEncryptionActive = true;
            state.ReadSequenceNumber = 0;
        }


        void NegotiationFinished()
        {
            this.ClientTLSState = TLS.ClientTLSState.ApplicationData;
            EventFinished.Set();

            /// Now send all data that we have queued up 
            /// 
            byte[] bQueuedData = ClientSendQueue.GetAllSamples();
            if ((bQueuedData != null) && (bQueuedData.Length > 0))
            {
                BuildAndSendApplicationRecords(bQueuedData);
            }

        }

      

    }

}
