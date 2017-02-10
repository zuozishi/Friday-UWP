/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Text;

namespace xmedianet.socketserver
{
    public interface IMessageFilter
   {
      /// <summary>
      /// Transforms data for sending.  This may add additional headers, etc.  If it returns a zero length
      /// array, the data will not be passed to the next message in the chain
      /// For messages, only one message must be sent a time if there are mutiple message filters in the list
      /// </summary>
      /// <param name="bSend"></param>
      /// <returns></returns>
      byte[] TransformSendData(byte[] bSend);

      /// <summary>
      /// Transforms data that was read.  May be used to find message boundaries, decompress, unencrypt etc.  
      /// Returns a list of messages broken on message boundaries.   Each of these arrays is passed to the 
      /// next filter in the chain
      /// </summary>
      /// <param name="bSend"></param>
      /// <returns></returns>
      List<byte[]> TransformReceiveData(byte[] bSend);

      bool IsFilterActive { get; }
   }
}
