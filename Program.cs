using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LivelockDemo01
{
    class MyResource
    {
        public MyResource()
        {
        }

        public bool Acquire()
        {
            if (Monitor.TryEnter(m_objLock) == true)
            {
                SomeThreadTriedToAcquireAndFailed = false;
                return true;
            }
            else
            {
                SomeThreadTriedToAcquireAndFailed = true;
                return false;
            }
        }

        public void Release()
        {
            Monitor.Exit(m_objLock);
        }

        public bool SomeThreadTriedToAcquireAndFailed
        {
            get
            {
                return m_bSomeThreadTriedToAcquireAndFailed;
            }

            set
            {
                m_bSomeThreadTriedToAcquireAndFailed = value;
            }
        }

        private object m_objLock = new object();
        private bool m_bSomeThreadTriedToAcquireAndFailed = false;
    }

    class Program
    {
        const ConsoleColor CONSOLE_COLOR_THREAD_01 = ConsoleColor.White;
        const ConsoleColor CONSOLE_COLOR_THREAD_02 = ConsoleColor.Cyan;

        static void StartThread01()
        {
            m_thread_01 = new Thread(new ThreadStart(ThreadMethod01));
            m_thread_01.Name = "Thread01";
            m_thread_01.Start();
        }

        static void StartThread02()
        {
            m_thread_02 = new Thread(new ThreadStart(ThreadMethod02));
            m_thread_02.Name = "Thread02";
            m_thread_02.Start();
        }

        static void WaitThreadEnd(Thread thread)
        {
            thread.Join();
        }

        static void ThreadMethod01()
        {
            bool bAcquired = false;

            try
            {
                while (true)
                {
                    // Attempt to acquire resource.
                    if (m_MyResource.Acquire() == false)
                    {
                        continue;
                    }

                    bAcquired = true;
                    ThreadOutputText("Acquired resource.", CONSOLE_COLOR_THREAD_01);

                    Thread.Sleep(1000);

                    // Once acquired, resource, check to see if another thread
                    // has tried to acquire resource.
                    if (m_MyResource.SomeThreadTriedToAcquireAndFailed == true)
                    {
                        // If so, release the resource.
                        ThreadOutputText("But someone else wants resource.", CONSOLE_COLOR_THREAD_01);
                        m_MyResource.Release();
                        bAcquired = false;
                        ThreadOutputText("I gave up resource.", CONSOLE_COLOR_THREAD_01);
                        // The amount of time taken for the thread
                        // to sleep will affect how fast this thread
                        // re-tries to acquire the resource.
                        //
                        // If it is too fast, this thread may acquire
                        // the resource without the other thread
                        // even attempting to try to acquire the resource.
                        //
                        // If it is too slow, the other thread may 
                        // successfully acquire the resource and this
                        // thread may not have enough time to try
                        // to acquire the resource. As a result,
                        // the other thread will not detect that this
                        // thread is trying to acquire the resource
                        // and hence it will think it need not release
                        // the resource. The other thread thus completes
                        // its task and there will be no Livelock.
                        Thread.Sleep(500);
                        continue;
                    }

                    break;
                }

                ThreadOutputText("Progress.", CONSOLE_COLOR_THREAD_01);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[{0:D}] Exception : [{1:S}].",
                    Thread.CurrentThread.ManagedThreadId,
                    ex.Message);
            }
            finally
            {
                if (bAcquired)
                {
                    m_MyResource.Release();
                    bAcquired = false;
                }

                ThreadOutputText("Exiting Thread.", CONSOLE_COLOR_THREAD_01);
            }
        }

        static void ThreadMethod02()
        {
            bool bAcquired = false;

            try
            {
                while (true)
                {
                    if (m_MyResource.Acquire() == false)
                    {
                        continue;
                    }

                    bAcquired = true;
                    ThreadOutputText("Acquired resource.", CONSOLE_COLOR_THREAD_02);

                    Thread.Sleep(1000);

                    if (m_MyResource.SomeThreadTriedToAcquireAndFailed == true)
                    {
                        ThreadOutputText("But someone else wants resource.", CONSOLE_COLOR_THREAD_02);
                        m_MyResource.Release();
                        bAcquired = false;
                        ThreadOutputText("I gave up resource.", CONSOLE_COLOR_THREAD_02);
                        Thread.Sleep(500);
                        continue;
                    }

                    break;
                }

                ThreadOutputText("Progress.", CONSOLE_COLOR_THREAD_02);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[{0:D}] Exception : [{1:S}].",
                    Thread.CurrentThread.ManagedThreadId,
                    ex.Message);
            }
            finally
            {
                if (bAcquired)
                {
                    m_MyResource.Release();
                    bAcquired = false;
                }

                ThreadOutputText("Exiting Thread.", CONSOLE_COLOR_THREAD_02);
            }
        }

        static void ThreadOutputText(string strText, ConsoleColor color)
        {
            lock (m_objTextOutput)
            {
                Console.ForegroundColor = color;
                Console.WriteLine("[{0:S}] {1:S}", Thread.CurrentThread.Name, strText);
            }
        }

        static void Main(string[] args)
        {
            m_MyResource = new MyResource();

            StartThread01();
            StartThread02();

            WaitThreadEnd(m_thread_01);
            WaitThreadEnd(m_thread_02);
        }

        static Thread m_thread_01 = null;
        static Thread m_thread_02 = null;
        static MyResource m_MyResource = null;
        static object m_objTextOutput = new object();
    }
}
