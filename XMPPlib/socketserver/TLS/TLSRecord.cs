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

namespace xmedianet.socketserver.TLS
{
    public enum TLSContentType : byte
    {
        ChangeCipherSpec = 0x14,
        Alert = 0x15,
        Handshake = 0x16,
        Application = 0x17,
    }

    public abstract class TLSMessage
    {
        public TLSMessage()
        {
        }
        public virtual void DebugDump(bool bReceived)
        {
        }

        public virtual byte[] Bytes
        {
            get
            {
                return new byte[] { };
            }
        }

        public virtual uint ReadFromArray(byte[] bData, int nStartAt)
        {
            return 0;
        }
    }
    

    public class TLSRecord
    {
        public TLSRecord()
        {
        }

        public const int MaxUncompressedRecordSize = 16384;
        public const int MaxCompressedRecordSize = 16384+1024;
        public const int MaxCompressedEncryptedRecordSize = 16384 + 2048;

        public void DebugDump(bool bReceived)
        {
            System.Diagnostics.Debug.WriteLine("======================================================");
            System.Diagnostics.Debug.WriteLine("{0} TLSRecord ContentType: {1}, Version: {2}.{3}, Messages follow", bReceived ? "<--" : "-->", ContentType, MajorVersion, MinorVersion);
            foreach (TLSMessage msg in Messages)
                msg.DebugDump(bReceived);
            System.Diagnostics.Debug.WriteLine("======================================================");
        }


        private TLSContentType m_bContentType = TLSContentType.Application;

        public TLSContentType ContentType
        {
          get { return m_bContentType; }
          set { m_bContentType = value; }
        }
        private byte m_bMajorVersion = 3;

        public byte MajorVersion
        {
            get { return m_bMajorVersion; }
            set { m_bMajorVersion = value; }
        }
        private byte m_bMinorVersion = 1;

        public byte MinorVersion
        {
            get { return m_bMinorVersion; }
            set { m_bMinorVersion = value; }
        }

        public byte[] Content
        {
            get
            {
                int nLength = 0;
                List<byte[]> bMsgs = new List<byte[]>();
                foreach (TLSMessage msg in Messages)
                {
                    byte[] bMsg = msg.Bytes;
                    nLength += bMsg.Length;
                    bMsgs.Add(bMsg);
                }

                byte[] bRet = new byte[nLength];
                int nAt = 0;
                foreach (byte [] bNextMsg in bMsgs)
                {
                    ByteHelper.WriteByteArray(bRet, nAt, bNextMsg); nAt += bNextMsg.Length;
                }
                return bRet;
            }
            set
            {
                RawSetContent = value;
                ParseContentForRecords(RawSetContent);
            }
        }

        public List<TLSMessage> Messages = new List<TLSMessage>();

        void ParseContentForRecords(byte [] bContent)
        {
            Messages.Clear();

            uint nIndexAt = 0;
            while (nIndexAt < bContent.Length) // Read all the sub records of type ContentType in this TLSRecord
            {
                if (ContentType == TLSContentType.Handshake)
                {
                    // Determine next handshake step
                    TLSHandShakeMessage msg = new TLSHandShakeMessage();
                    uint nRead = msg.ReadFromArray(bContent, (int)nIndexAt);
                    if (nRead == 0)
                        break;
                    Messages.Add(msg);

                    nIndexAt += nRead;
                }
                else if (ContentType == TLSContentType.Alert)
                {
                    TLSAlertMessage msg = new TLSAlertMessage();
                    uint nRead = msg.ReadFromArray(bContent, (int)nIndexAt);
                    if (nRead == 0)
                        break;
                    Messages.Add(msg);
                    nIndexAt += nRead;
                }
                else if (ContentType == TLSContentType.ChangeCipherSpec)
                {
                    TLSChangeCipherSpecMessage msg = new TLSChangeCipherSpecMessage();
                    uint nRead = msg.ReadFromArray(bContent, (int)nIndexAt);
                    if (nRead == 0)
                        break;
                    Messages.Add(msg);
                    nIndexAt += nRead;
                }
                else if (ContentType == TLSContentType.Application)
                {
                    // decrypt, add to ApplicationDataReturned
                    TLSApplicationMessage msg = new TLSApplicationMessage();
                    uint nRead = msg.ReadFromArray(bContent, (int)nIndexAt);
                    if (nRead == 0)
                        break;
                    Messages.Add(msg);
                    nIndexAt += nRead;

                }
                else
                    break;
            }
        }



        public byte[] Bytes
        {
            get
            {
                int nLength = (ushort) Content.Length;

                byte[] bRet = new byte[5 + nLength];
                bRet[0] = (byte) ContentType;
                bRet[1] = MajorVersion;
                bRet[2] = MinorVersion;
                bRet[3] = (byte)((nLength & 0xFF00) >> 8);
                bRet[4] = (byte)((nLength & 0x00FF));

                ByteHelper.WriteByteArray(bRet, 5, Content);
                return bRet;
            }
        }

        public byte[] BytesFromRawContent
        {
            get
            {
                int nLength = (ushort)RawSetContent.Length;

                byte[] bRet = new byte[5 + nLength];
                bRet[0] = (byte)ContentType;
                bRet[1] = MajorVersion;
                bRet[2] = MinorVersion;
                bRet[3] = (byte)((nLength & 0xFF00) >> 8);
                bRet[4] = (byte)((nLength & 0x00FF));

                ByteHelper.WriteByteArray(bRet, 5, RawSetContent);
                return bRet;
            }
        }

        public byte[] GetBytesWithEncryptedContent(byte [] bEncryptedContent)
        {
            int nLength = (ushort)bEncryptedContent.Length;

            byte[] bRet = new byte[5 + nLength];
            bRet[0] = (byte)ContentType;
            bRet[1] = MajorVersion;
            bRet[2] = MinorVersion;
            bRet[3] = (byte)((nLength & 0xFF00) >> 8);
            bRet[4] = (byte)((nLength & 0x00FF));

            ByteHelper.WriteByteArray(bRet, 5, bEncryptedContent);
            return bRet;
        }

        public byte[] RawSetContent = null;

        public uint ReadFromArray(byte[] bData, int nStartAt, bool bRawContentOnly)
        {
            if (bData.Length < (nStartAt + 5))
                return 0;
            ContentType = (TLSContentType)bData[nStartAt+ 0];
            MajorVersion = bData[nStartAt + 1];
            MinorVersion = bData[nStartAt + 2];
            int nLength = (ushort)((bData[nStartAt + 3] << 8) | (bData[nStartAt + 4]));

            if (bData.Length < (nStartAt + 5 + nLength))
                return 0; /// not enough data yet
            if (bRawContentOnly == true)
                RawSetContent = ByteHelper.ReadByteArray(bData, 5 + nStartAt, nLength);
            else
               Content = ByteHelper.ReadByteArray(bData, 5 + nStartAt, nLength);
            return (uint)(5 + nLength);
        }


    }


    

}


