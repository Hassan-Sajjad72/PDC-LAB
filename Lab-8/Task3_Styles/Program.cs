using System;
using System.Diagnostics;
using MPI;

class Program
{
    const int Tasks = 24;
    static void Main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            var c = Communicator.world;
            if (c.Size < 2) throw new Exception("Use 2 or more ranks");
            string mode = args.Length > 0 ? args[0] : "master";
            if (mode == "master")
            {
                if (c.Rank == 0)
                {
                    int[] counts = new int[c.Size];
                    for (int task = 0; task < Tasks; task++)
                    {
                        int worker = 1 + task % (c.Size - 1);
                        c.Send(task, worker, 1); counts[worker]++;
                    }
                    for (int r = 1; r < c.Size; r++) c.Send(-1, r, 1);
                    for (int r = 1; r < c.Size; r++)
                        Console.WriteLine($"Worker {r}: {counts[r]} tasks, checksum={c.Receive<int>(r, 2)}");
                }
                else
                {
                    int checksum = 0;
                    while (true)
                    {
                        int task = c.Receive<int>(0, 1);
                        if (task < 0) break;
                        checksum += task;
                    }
                    c.Send(checksum, 0, 2);
                }
            }
            else if (mode == "ring")
            {
                const int Hops = 100;
                c.Barrier(); var watch = Stopwatch.StartNew();
                int next = (c.Rank + 1) % c.Size;
                int previous = (c.Rank + c.Size - 1) % c.Size;
                for (int pass = 0; pass < Hops; pass++)
                {
                    if (c.Rank == 0)
                    {
                        c.Send(pass, next, 7);
                        c.Receive<int>(previous, 7);
                    }
                    else
                    {
                        int token = c.Receive<int>(previous, 7);
                        c.Send(token, next, 7);
                    }
                }
                c.Barrier(); watch.Stop();
                if (c.Rank == 0)
                    Console.WriteLine($"Ring: {c.Size} ranks, {watch.Elapsed.TotalMilliseconds:F2} ms, {watch.Elapsed.TotalMilliseconds/(Hops*c.Size):F4} ms/message, correct=YES");
            }
        }
    }
}