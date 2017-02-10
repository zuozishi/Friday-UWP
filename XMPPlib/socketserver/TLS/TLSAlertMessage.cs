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

namespace xmedianet.socketserver.TLS
{

    public enum AlertLevel : byte
    {
        warning = 1,
        fatal = 2,
    }

    public enum AlertDescription : byte
    {
        CloseNotify = 0,
        UnexpectedMessage = 10,
        BadRecordMAC = 20,
        DecryptionFailed = 21,
        RecordOverflow = 22,
        DecompressionFailure = 30,
        HandshakeFailure = 40,
        NoCertificate = 41,
        BadCertificate = 42,
        UnsupportedCertificate = 43,
        CertificateRevoked = 44,
        CertificateExpired = 45,
        CertificateUnknown = 46,
        IllegalParameter = 47,
        UnknownCA = 48,
        AccessDenied = 49,
        DecodeError = 50,
        DecryptError = 51,
        ExportRestriction = 60,
        ProtocolVersion = 70,
        InsufficientSecurity = 71,
        InternalError = 80,
        UserCancelled = 90,
        NoRenegotiation = 100,
        UnsupportedExtension = 110,
        CertificateUnobtainable = 111,
        UnrecognizedName = 112,
        BadCertificateStatusResponse = 113,
        BadCertificateHashValue = 114,
        UnknownPSKIdentity = 115,
    }

    public class TLSAlertMessage : TLSMessage
    {
        public TLSAlertMessage()
        {
        }

        public override void DebugDump(bool bReceived)
        {
            System.Diagnostics.Debug.WriteLine("{0} TLSAlertMessage AlertLevel: {1}, AlertDescription: {2}", bReceived ? "<--" : "-->", AlertLevel, AlertDescription);
        }

        private AlertLevel m_eAlertLevel = AlertLevel.warning;
        public AlertLevel AlertLevel
        {
            get { return m_eAlertLevel; }
            set { m_eAlertLevel = value; }
        }

        private AlertDescription m_eAlertDescription = AlertDescription.CloseNotify;
        public AlertDescription AlertDescription
        {
            get { return m_eAlertDescription; }
            set { m_eAlertDescription = value; }
        }


        public override byte[] Bytes
        {
            get
            {
                byte[] bRet = new byte[2];
                bRet[0] = (byte)AlertLevel;
                bRet[1] = (byte)AlertDescription;

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
            if (bData.Length < (nStartAt + 2))
                return 0;

            if (bData.Length > (nStartAt + 2))
                throw new Exception("Alert Length too long, something bad");

            AlertLevel = (AlertLevel)bData[nStartAt + 0];
            AlertDescription = (AlertDescription)bData[nStartAt + 1];

            return 2;
        }
    }

   
}
