/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xmedianet.socketserver
{
    public interface IBufferPool
    {
        byte[] Checkout(int nSize);
        void Checkin(byte[] buffer);
    }

    /// <summary>
    /// The .NET 1.0 and 1.1 frameworks pin memory during asyncronous reading.  Because of this,
    /// memory is pinned and fragmented.  A recommended solution is to reuse this from a buffer pool,
    /// and have our socketclients get their buffers from here instead.
    /// 
    /// also see http://blogs.msdn.com/yunjin/archive/2004/01/27/63642.aspx for why we need this.
    /// </summary>
    public class BufferPool : IBufferPool
    {
        private int m_nPoolSize = 512; // initial size of the pool
        private int m_nBufferSize = 1024; // size of the buffers

        // pool of buffers
        private Queue<byte[]> m_FreeBuffers;

        public BufferPool(int nBufferSize, int nPoolSize)
        {
            m_nPoolSize = nPoolSize;
            m_nBufferSize = nBufferSize;

            m_FreeBuffers = new Queue<byte[]>(m_nPoolSize);

            for (int i = 0; i < m_nPoolSize; i++)
            {
                m_FreeBuffers.Enqueue(new byte[m_nBufferSize]);
            }
        }
        object SyncRoot = new object();

        // check out a buffer
        public byte[] Checkout(int nSize)
        {
            if (m_FreeBuffers.Count > 0)
            {
                lock (SyncRoot)
                {
                    if (m_FreeBuffers.Count > 0)
                        return (byte[])m_FreeBuffers.Dequeue();
                }
            }

            // instead of creating new buffer, 
            // blocking waiting or refusing request may be better
            return new byte[m_nBufferSize + 1]; /// add one so we know if we've been newed
        }

        // check in a buffer
        public void Checkin(byte[] buffer)
        {
            lock (SyncRoot)
            {
                m_FreeBuffers.Enqueue(buffer);
            }
        }
    }

    public class DynamicBufferPool : IBufferPool
    {
        private int m_nPoolSize = 512; // initial size of the pool

        // pool of buffers
        private Dictionary<int, Queue<byte[]>> m_FreeBuffers = new Dictionary<int, Queue<byte[]>>();

        public DynamicBufferPool(int nInitialBufferSize)
        {
            m_nPoolSize = nInitialBufferSize;
        }
        object SyncRoot = new object();

        // check out a buffer
        public byte[] Checkout(int nSize)
        {
            if (m_FreeBuffers.Count > 0)
            {
                lock (SyncRoot)
                {
                    Queue<byte[]> bufferqueue = null;
                    if (m_FreeBuffers.ContainsKey(nSize) == false)
                    {
                        bufferqueue = new Queue<byte[]>();
                        m_FreeBuffers.Add(nSize, bufferqueue);
                        for (int i = 0; i < m_nPoolSize; i++)
                            bufferqueue.Enqueue(new byte[nSize]);
                    }
                    else
                    {
                        bufferqueue = m_FreeBuffers[nSize];
                    }


                    if (bufferqueue.Count > 0)
                        return bufferqueue.Dequeue();
                }
            }

            // instead of creating new buffer, 
            // blocking waiting or refusing request may be better
            return new byte[nSize]; /// add one so we know if we've been newed
        }

        // check in a buffer
        public void Checkin(byte[] buffer)
        {
            lock (SyncRoot)
            {
                Queue<byte[]> bufferqueue = null;
                if (m_FreeBuffers.ContainsKey(buffer.Length) == false)
                {
                    return;
                }
                else
                {
                    bufferqueue = m_FreeBuffers[buffer.Length];
                }

                bufferqueue.Enqueue(buffer);
            }
        }
    }
}
