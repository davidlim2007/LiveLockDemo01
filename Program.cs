using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LivelockDemo01
{
    // The MyResource class represents a system resource,
    // the ownership of which is being contested between the
    // two threads.
    //
    // It makes use of the Monitor class to lock down
    // resources. The m_objLock object serves as the resource.
    class MyResource
    {
        public MyResource()
        {
        }

        public bool Acquire()
        {
            if (Monitor.TryEnter(m_objLock) == true)
            {
                // If a thread has managed to enter the
                // Monitor, SomeThreadTriedToAcquireAndFailed
                // will be false (as a thread has successfully
                // acquired the resource).
                SomeThreadTriedToAcquireAndFailed = false;

                // A true value is returned to signal a successful
                // acquisition.
                return true;
            }
            else
            {
                // If a thread has failed to enter the
                // Monitor, SomeThreadTriedToAcquireAndFailed
                // will be true (as a thread has unsuccessfully
                // acquired the resource).
                SomeThreadTriedToAcquireAndFailed = true;

                // A false value is returned to signal an unsuccessful
                // acquisition.
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

        // The object used for accessing the critical section
        // of code.
        private object m_objLock = new object();

        // A value representing whether or not a thread has
        // attempted to enter the Monitor (i.e. gain ownership
        // of the resource).
        //
        // Initially set to false.
        private bool m_bSomeThreadTriedToAcquireAndFailed = false;
    }

    class Program
    {
        // We color code the output of each thread so as to
        // distinguish between them.
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

        // The entry-point method for Thread 01.
        static void ThreadMethod01()
        {
            // A bool value representing whether
            // the resource is successfully acuqired
            // or not.
            //
            // Initially set to false.
            bool bAcquired = false;

            try
            {
                while (true)
                {
                    // Continually attempt to acquire the resource.
                    //
                    // If the thread fails to do so, the while-loop
                    // will reiterate and the thread will try again.
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
                        // to acquire the resource. 
                        //
                        // As a result, the other thread will not detect 
                        // that this thread is trying to acquire the resource
                        // and hence it will think it need not release
                        // the resource. The other thread thus completes
                        // its task and there will be no Livelock.
                        Thread.Sleep(500);

                        // Re-iterate the while loop.
                        continue;
                    }

                    // If the thread manages to get to this point,
                    // it is able to hold on to the resource 
                    // without another thread requesting ownership.
                    //
                    // It is hence able to break out of the loop
                    // and finish.
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
                // If the thread still has ownership of
                // the resource, release it.
                if (bAcquired)
                {
                    m_MyResource.Release();
                    bAcquired = false;
                }

                ThreadOutputText("Exiting Thread.", CONSOLE_COLOR_THREAD_01);
            }
        }

        // The entry-point method for Thread 02.
        //
        // It is identical to ThreadMethod01() save for
        // the color coding used.
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

        // This is a specialized method for outputting messages
        // to the console.
        //
        // It takes in a string parameter, representing the message
        // to output, and a ConsoleColor representing the color to
        // print the message in.
        //
        // This helps us distinguish between the output of the threads,
        // as each thread is color-coded differently.
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
