
using System;
using System.Threading;

class Program
{
    static void Worker()
    {
        Thread.Sleep(200);
    }

    static void Main()
    {
        Thread t = new Thread(Worker);

        Console.WriteLine(
            $"After creation: {t.ThreadState}");
        // Conceptual state: New

        t.Start();

        Console.WriteLine(
            $"Immediately after Start(): {t.ThreadState}");
        // Conceptual state: Runnable / Ready or Running

        Thread.Sleep(50);

        Console.WriteLine(
            $"While worker is sleeping: {t.ThreadState}");
        // Conceptual state: Blocked / Waiting

        t.Join();

        Console.WriteLine(
            $"After Join() completes: {t.ThreadState}");
        // Conceptual state: Terminated
    }
}