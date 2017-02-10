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
    public class TLSApplicationMessage : TLSMessage
    {
        public TLSApplicationMessage()
        {
        }

        public override void DebugDump(bool bReceived)
        {
            System.Diagnostics.Debug.WriteLine("{0} TLSApplicationMessage ApplicationData Encrypted: \r\n{1}", bReceived ? "<--" : "-->", ByteHelper.HexStringFromByte(ApplicationData, true, 16));
        }



        byte[] m_bApplicationData = new byte[] { };
        public byte[] ApplicationData
        {
            get { return m_bApplicationData; }
            set { m_bApplicationData = value; }
        }

        public override byte[] Bytes
        {
            get
            {
                return ApplicationData;
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
            ApplicationData = bData;
            return (uint)bData.Length;
        }
    }
}
