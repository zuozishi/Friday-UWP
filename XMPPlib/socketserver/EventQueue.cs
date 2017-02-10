/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xmedianet.socketserver
{
    public class EventQueueWithNotification<T>
    {
        public EventQueueWithNotification()
        {
        }


        public void Enqueue(T msg)
        {
            lock (m_objLock)
            {
                m_msgQueue.Enqueue(msg);
                GotNewMessageEvent.Set();
            }
        }

        public void Enqueue(T[] msgs)
        {
            lock (m_objLock)
            {
                foreach (T nextt in msgs)
                    m_msgQueue.Enqueue(nextt);
                GotNewMessageEvent.Set();
            }
        }

        public void EnqueueSorted(T[] msgs, IComparer<T> comparer)
        {
            lock (m_objLock)
            {
                List<T> listnewsorted = new List<T>();
                T[] arrayexisting = m_msgQueue.ToArray();
                m_msgQueue.Clear();

                listnewsorted.AddRange(arrayexisting);
                listnewsorted.AddRange(msgs);
                listnewsorted.Sort(comparer);

                foreach (T nextt in listnewsorted)
                    m_msgQueue.Enqueue(nextt);

                GotNewMessageEvent.Set();
            }
        }

        public void Clear()
        {
            lock (m_objLock)
            {
                m_msgQueue.Clear();
                GotNewMessageEvent.Reset();
            }
        }

        public T WaitNext(int nTimeOutMs)
        {
            if (GotNewMessageEvent.WaitOne(nTimeOutMs) == false)
                return default(T);

            return PopNextMessage();
        }

        public T[] WaitAll(int nTimeOutMs)
        {
            GotNewMessageEvent.WaitOne(nTimeOutMs);
            return PopAllMessages();
        }

        private T[] PopAllMessages()
        {
            List<T> msglist = new List<T>();
            lock (m_objLock)
            {
                while (m_msgQueue.Count > 0)
                    msglist.Add(m_msgQueue.Dequeue());

                if (m_msgQueue.Count <= 0)  /// No more messages
                    GotNewMessageEvent.Reset();
            }
            return msglist.ToArray();

        }

        private T PopNextMessage()
        {
            T msgret = default(T);
            lock (m_objLock)
            {
                if (m_msgQueue.Count > 0)
                    msgret = m_msgQueue.Dequeue();

                if (m_msgQueue.Count <= 0)  /// No more messages
                    GotNewMessageEvent.Reset();
            }
            return msgret;
        }

        public int Count
        {
            get
            {
                return m_msgQueue.Count;
            }
        }

        public System.Threading.ManualResetEvent GotNewMessageEvent = new System.Threading.ManualResetEvent(false);
        private object m_objLock = new object();
        private System.Collections.Generic.Queue<T> m_msgQueue = new System.Collections.Generic.Queue<T>();

    }

    public class EventQueueWithNotificationMultipleWaits<T>
    {
        public EventQueueWithNotificationMultipleWaits()
        {
        }


        public void Enqueue(T msg)
        {
            lock (m_objLock)
            {
                m_msgQueue.Enqueue(msg);
                GotNewMessageEvent.Set();
            }
        }

        public void Enqueue(T[] msgs)
        {
            lock (m_objLock)
            {
                foreach (T nextt in msgs)
                    m_msgQueue.Enqueue(nextt);
                GotNewMessageEvent.Set();
            }
        }

        public void EnqueueSorted(T[] msgs, IComparer<T> comparer)
        {
            lock (m_objLock)
            {
                List<T> listnewsorted = new List<T>();
                T[] arrayexisting = m_msgQueue.ToArray();
                m_msgQueue.Clear();

                listnewsorted.AddRange(arrayexisting);
                listnewsorted.AddRange(msgs);
                listnewsorted.Sort(comparer);

                foreach (T nextt in listnewsorted)
                    m_msgQueue.Enqueue(nextt);

                GotNewMessageEvent.Set();
            }
        }

        public void Clear()
        {
            lock (m_objLock)
            {
                m_msgQueue.Clear();
                GotNewMessageEvent.Reset();
            }
        }

        public T WaitNext(int nTimeOutMs)
        {
            if (GotNewMessageEvent.WaitOne(nTimeOutMs) == false)
                return default(T);

            return PopNextMessage();
        }

        public T[] WaitAll(int nTimeOutMs)
        {
            GotNewMessageEvent.WaitOne(nTimeOutMs);
            return PopAllMessages();
        }

        private T PopNextMessage()
        {
            T msgret = default(T);
            lock (m_objLock)
            {
                if (m_msgQueue.Count > 0)
                    msgret = m_msgQueue.Dequeue();

                if (m_msgQueue.Count > 0)  /// mo Amessages
                    GotNewMessageEvent.Set(); /// signal more people
            }
            return msgret;
        }


        private T[] PopAllMessages()
        {
            List<T> msglist = new List<T>();
            lock (m_objLock)
            {
                while (m_msgQueue.Count > 0)
                    msglist.Add(m_msgQueue.Dequeue());

                if (m_msgQueue.Count <= 0)  /// No more messages
                    GotNewMessageEvent.Reset();
            }
            return msglist.ToArray();

        }

        public int Count
        {
            get
            {
                return m_msgQueue.Count;
            }
        }

        public System.Threading.AutoResetEvent GotNewMessageEvent = new System.Threading.AutoResetEvent(false);
        private object m_objLock = new object();
        private System.Collections.Generic.Queue<T> m_msgQueue = new System.Collections.Generic.Queue<T>();

    }
}
