/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace xmedianet.socketserver
{
   /// <summary>
   /// Summary description for Class1.
   /// </summary>
   /// 
   public class SocketEventArgs : System.EventArgs
   {
      public int Length = 0;
      public byte[] m_data = null;
      public SocketEventArgs( byte[] data, int nlen )
      {

         if ( m_data != null )
            m_data = null;
         m_data = new byte[nlen];
         System.Array.Copy( data, 0, m_data, 0, nlen);
         Length = nlen;
      }

      public SocketEventArgs()
      {
      }

      public byte[] GetData()
      {
         return m_data;
      }
      public string GetString()
      {
          return System.Text.Encoding.UTF8.GetString(m_data, 0, Length);
      }



   }

   public class TcpEventArgs : SocketEventArgs
   {
      public string strLine;
      public TcpEventArgs( string str ) : base()
      {
         strLine = str;
         m_data = System.Text.Encoding.UTF8.GetBytes(str);
         Length = m_data.Length;
      }

   }
}
