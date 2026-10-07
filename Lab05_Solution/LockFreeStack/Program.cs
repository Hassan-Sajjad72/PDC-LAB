using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

sealed class Node
{
    public int Value;
    public Node? Next;

    public Node(int value)
    {
        Value = value;
    }
}

sealed class LockFreeStack
{
    private Node? head;

    public void Push(int value)
    {
        Node node = new(value);

        while (true)
        {
            Node? oldHead = Volatile.Read(ref head);
            node.Next = oldHead;

            if (Interlocked.CompareExchange(ref head, node, oldHead) == oldHead)
                return;
        }
    }

    public bool Pop(out int value)
    {
        while (true)
        {
            Node? oldHead = Volatile.Read(ref head);

            if (oldHead == null)
            {
                value = 0;
                return false;
            }

            Node? next = oldHead.Next;

            if (Interlocked.CompareExchange(ref head, next, oldHead) == oldHead)
            {
                value = oldHead.Value;
                return true;
            }
        }
    }

    public bool IsEmpty()
    {
        return Volatile.Read(ref head) == null;
    }
}

class Program
{
    static void Main(string[] args)
    {
        int threadsCount = 4;

        if (args.Length > 0 && int.TryParse(args[0], out int t))
            threadsCount = t;

        const int ItemsPerThread = 5000;
        int total = threadsCount * ItemsPerThread;

        var stack = new LockFreeStack();
        int[] seen = new int[total];

        var sw = Stopwatch.StartNew();

        Thread[] threads = Enumerable.Range(0, threadsCount)
            .Select(id => new Thread(() =>
            {
                int start = id * ItemsPerThread;

                for (int i = 0; i < ItemsPerThread; i++)
                    stack.Push(start + i);

                for (int i = 0; i < ItemsPerThread; i++)
                {
                    int value;

                    while (!stack.Pop(out value))
                        Thread.Yield();

                    Interlocked.Increment(ref seen[value]);
                }
            }))
            .ToArray();

        foreach (Thread thread in threads)
            thread.Start();

        foreach (Thread thread in threads)
            thread.Join();

        sw.Stop();

        int lost = 0;
        int duplicated = 0;

        for (int i = 0; i < total; i++)
        {
            if (seen[i] == 0)
                lost++;

            if (seen[i] > 1)
                duplicated += seen[i] - 1;
        }

        Console.WriteLine($"Threads={threadsCount}");
        Console.WriteLine($"Nodes (total)={total}");
        Console.WriteLine($"Lost={lost}");
        Console.WriteLine($"Duplicated={duplicated}");
        Console.WriteLine($"Stack empty={(stack.IsEmpty() ? "Y" : "N")}");
        Console.WriteLine($"Time={sw.ElapsedMilliseconds} ms");
    }
}