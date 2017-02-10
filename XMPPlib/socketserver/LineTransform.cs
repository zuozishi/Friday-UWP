/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Text;


namespace xmedianet.socketserver
{
    /// <summary>
    /// When added as a filter to a socket client, parses out messages line by line
    /// </summary>
   public class LineTransform : IMessageFilter
   {
      public LineTransform()
      {
      }

      public string csCurrentLine = "";

      #region IMessageFilter Members

      public byte[] TransformSendData(byte[] bSend)
      {
         return bSend; /// Assume data is already correctly delimited
      }

      public List<byte[]> TransformReceiveData(byte[] bSend)
      {
         List<byte[]> FoundLines = new List<byte[]>();

         /// Don't return data until we get a full line
         /// 
         string strData = System.Text.Encoding.UTF8.GetString(bSend, 0, bSend.Length);

         bool bLineFeed = false;
         string csGotLine = "";
         csCurrentLine += strData;

         do
         {
            // copy 
            bLineFeed = false;
            csGotLine = "";

            int nLineFeedAt = csCurrentLine.IndexOf("\r\n");
            if (nLineFeedAt >= 0)
            {
               csGotLine = csCurrentLine.Substring(0, nLineFeedAt); // don't include the /r/n
               string csRight = "";
               csRight = csCurrentLine.Substring(nLineFeedAt + 2);
               csCurrentLine = csRight;

               if (csGotLine.Length > 0)
               {
                   FoundLines.Add(System.Text.Encoding.UTF8.GetBytes(csGotLine));
               }
               bLineFeed = true;
            }
         } while ((bLineFeed) && (csCurrentLine.Length > 0));


         return FoundLines;
      }

      #endregion

      #region IMessageFilter Members


      public bool IsFilterActive
      {
          get { return true;  }
      }

      #endregion
   }
}
