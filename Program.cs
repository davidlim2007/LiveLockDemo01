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
                        Thread.Sleep(0);
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
                        Thread.Sleep(0);
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
