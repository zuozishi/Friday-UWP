/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;
using System.Threading;

namespace xmedianet.socketserver
{
    public class ByteBuffer
    {
        public ByteBuffer()
        {
        }

        public int Size
        {
            get
            {
               lock (BufferLock)
               {
                  return m_nSize;
               }
            }
           private set   /// must have a lock before calling this
           {
              m_nSize = value;
              SizeEvent.Set();
           }
        }

       /// <summary>
       /// Waits for the number of elements in the buffer to reach a certain size, or until the timeout is reached or the other wait handle is signaled
       /// </summary>
       /// <param name="nSizeInBuffer"></param>
       /// <param name="nTimeout"></param>
       /// <param name="otherhandletowaiton"></param>
       /// <returns></returns>
        public bool WaitForSize(int nSizeInBuffer, int nTimeout, System.Threading.WaitHandle otherhandletowaiton)
        {
           lock (BufferLock)
           {
              if (m_nSize >= nSizeInBuffer)
                 return true;
              SizeEvent.Reset();
           }
           
           TimeSpan tsElapsed;
           DateTime dtStart = DateTime.Now;
           WaitHandle[] handles = new WaitHandle[] { SizeEvent, otherhandletowaiton };
           if (otherhandletowaiton == null)
              handles = new WaitHandle[] { SizeEvent};
           do
           {
              int nWait = WaitHandle.WaitAny(handles, nTimeout);
              if (nWait == 1)
              {
                 return false;
              }
              else if (nWait == 0)
              {
                 lock (BufferLock)
                 {
                    SizeEvent.Reset();
                    if (m_nSize >= nSizeInBuffer)
                       return true;
                 }
              }
              else
              {
                 return false;
              }

              tsElapsed = DateTime.Now - dtStart;
              if (nTimeout != Timeout.Infinite)
              {
                 nTimeout = (int)tsElapsed.TotalMilliseconds;
                 if (nTimeout <= 0)
                    break;
              }
              
           }
           while (true);

           return false;
        }

        AutoResetEvent SizeEvent = new AutoResetEvent(false);
        object BufferLock = new object();

        /// <summary>
        /// Retrieve N samples from our buffer   
        /// </summary>
        /// <param name="nSamples"></param>
        /// <returns></returns>
        public byte[] GetNSamples(int nSamples)
        {
            if (nSamples <= 0)
                return new byte[] { };


           lock (BufferLock)
           {
              byte[] Samples = new byte[nSamples];
              if ((OutgoingBuffer != null) && (OutgoingBuffer.Length > 0))
              {
                 int nCopyLength = (m_nSize > nSamples) ? nSamples : m_nSize;
                 Array.Copy(OutgoingBuffer, 0, Samples, 0, nCopyLength);

                 CompactBuffer(nCopyLength);
              }

              return Samples;
           }
        }

        public byte[] GetAllSamples()
        {
           lock (BufferLock)
           {
              byte[] Samples = new byte[this.m_nSize];
              if ((OutgoingBuffer != null) && (OutgoingBuffer.Length > 0))
              {
                 Array.Copy(OutgoingBuffer, 0, Samples, 0, m_nSize);

                 CompactBuffer(m_nSize);
              }

              return Samples;
           }
        }

        public byte[] PeekAllSamples()
        {
            lock (BufferLock)
            {
                byte[] Samples = new byte[this.m_nSize];
                if ((OutgoingBuffer != null) && (OutgoingBuffer.Length > 0))
                {
                    Array.Copy(OutgoingBuffer, 0, Samples, 0, m_nSize);
                }

                return Samples;
            }
        }


        public int GetNSamplesIntoBuffer(byte[] Samples, int nSamples)
        {
           lock (BufferLock)
           {
              if ((OutgoingBuffer != null) && (OutgoingBuffer.Length > 0))
              {
                 int nCopyLength = (nSamples > this.m_nSize) ? this.m_nSize : nSamples;
                 Array.Copy(OutgoingBuffer, 0, Samples, 0, nCopyLength);

                 CompactBuffer(nCopyLength);
                 return nCopyLength;
              }
              return 0;
           }
        }

        /// <summary>
        /// Compact our buffer, move all usable data to index 0, mark the new length of our usable data, and zero out all unused data
        /// Data must be locked
        /// </summary>
        /// <param name="nIndexFrom"></param>
        protected void CompactBuffer(int nIndexFrom)
        {
            if (OutgoingBuffer == null) 
                return;

            if ((nIndexFrom > 0) && (nIndexFrom <= OutgoingBuffer.Length))
            {
                int nLengthRemaining = m_nSize - nIndexFrom;
                if (nLengthRemaining > 0)
                {
                    byte[] NewBuffer = new byte[nLengthRemaining];
                   Array.Copy(OutgoingBuffer, nIndexFrom, NewBuffer, 0, nLengthRemaining);
                   Array.Copy(NewBuffer, 0, OutgoingBuffer, 0, nLengthRemaining);
                }

                Size = nLengthRemaining;
                Array.Clear(OutgoingBuffer, m_nSize, OutgoingBuffer.Length - m_nSize);
            }
        }


        protected void ExpandBuffer(int nNewLength)
        {
            if (this.OutgoingBuffer == null)
            {
                this.OutgoingBuffer = new byte[nNewLength];
                return;
            }

            // First see if our buffer is big enough
            if (this.OutgoingBuffer.Length < nNewLength)
            {
               int nOldLength = this.OutgoingBuffer.Length;
               Array.Resize(ref OutgoingBuffer, nNewLength);
            }
        }

       /// <summary>
       /// Merge data into this array at the index specified.  Merging is a summation operation
       /// ... Can't do this with .NET generics.   It won't let you use the + operator
       /// </summary>
       /// <param name="aData"></param>
       /// <param name="nIndexAt"></param>
        public void MergeData(byte[] aData)
        {
           lock (BufferLock)
           {
              ExpandBuffer(aData.Length);
              if (aData.Length > m_nSize)
                 Size = aData.Length;
              for (int i = 0; i < aData.Length; i++)
              {

                 OutgoingBuffer[i] = (byte) (OutgoingBuffer[i] + aData[i]);
              }
           }
        }

       


       /// <summary>
       /// Append data onto the end of this array
       /// </summary>
       /// <param name="aData"></param>
        public int AppendData(byte[] aData)
        {
           int nRet = 0;
           lock (BufferLock)
           {
              
              ExpandBuffer(m_nSize + aData.Length);
              Array.Copy(aData, 0, OutgoingBuffer, m_nSize, aData.Length);
              Size += aData.Length;
              nRet = m_nSize;
           }

           return nRet;
        }

        public int AppendData(byte[] aData, int nOffset, int nLength)
        {
            int nRet = 0;
            lock (BufferLock)
            {
                ExpandBuffer(m_nSize + nLength);
                Array.Copy(aData, nOffset, OutgoingBuffer, m_nSize, nLength);
                Size += nLength;
                nRet = m_nSize;
            }

            return nRet;
        }

        public byte [] FindString(string strValue)
        {
            lock (BufferLock)
            {
                if ((OutgoingBuffer == null) || (OutgoingBuffer.Length <= 0) || (OutgoingBuffer.Length < strValue.Length) )
                    return null;

                //string strBuffer = System.Text.Encoding.UTF8.GetString(OutgoingBuffer, 0, m_nSize);

                //int nAt = strBuffer.IndexOf(strValue);
                int nAt = FindByteArray(OutgoingBuffer, 0, m_nSize, System.Text.Encoding.UTF8.GetBytes(strValue));
                if (nAt >= 0)
                {
                    int nCopyLength = nAt + strValue.Length;
                    byte[] Samples = new byte[nCopyLength];
                    Array.Copy(OutgoingBuffer, 0, Samples, 0, nCopyLength);
                    CompactBuffer(nCopyLength);
                    return Samples;
                }
            }
            return null;
        }


        public int FindBytes(byte [] bSearch)
        {
            lock (BufferLock)
            {
                if ((OutgoingBuffer == null) || (OutgoingBuffer.Length <= 0) || (OutgoingBuffer.Length < bSearch.Length))
                    return -1;

                int nAt = FindByteArray(OutgoingBuffer, 0, m_nSize, bSearch);
                return nAt;
            }
        }


        public static int FindByteArray(byte[] bSource, int nStartAt, int nLength, byte[] bSearch)
        {
            int nRet = -1;
            if (nLength < bSearch.Length)
                return nRet;

            int nSearchLen = bSearch.Length;
            for (int i = nStartAt; i <= nLength-nSearchLen; i++)
            {
                bool bFoundHere = true;
                for (int s = 0; s < nSearchLen; s++)
                {
                    if (bSource[i+s] != bSearch[s])
                    {
                        bFoundHere = false;
                        break;
                    }
                }
                if (bFoundHere == true)
                    return i;
            }

            return nRet;
        }

        protected object OutgoingBufferLock = new object();
        protected byte[] OutgoingBuffer = null;

       /// <summary>
       ///  Where the data ends in our outgoing buffer
       /// </summary>
        int m_nSize = 0;

    }
}
