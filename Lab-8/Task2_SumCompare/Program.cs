using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MPI;

class Program
{
    const int N = 10000000;
    static void Main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            var c = Communicator.world;
            int rank = c.Rank, size = c.Size;
            if (N % size != 0) throw new Exception("Ranks must divide N");
            int chunk = N / size;
            long local = 0;
            // Each rank owns its own logical segment (all values are 1).
            for (int j = 0; j < chunk; j++) local++;
            c.Barrier();
            var sw = Stopwatch.StartNew();
            long collective = c.Reduce(local, Operation<long>.Add, 0);
            c.Barrier(); sw.Stop();
            if (rank == 0) Console.WriteLine($"Collective: {sw.Elapsed.TotalMilliseconds:F3} ms, correct={collective == N}");

            c.Barrier(); sw.Restart();
            if (rank == 0)
            {
                long total = local;
                for (int r = 1; r < size; r++) total += c.Receive<long>(r, 20);
                sw.Stop(); Console.WriteLine($"Point-to-point: {sw.Elapsed.TotalMilliseconds:F3} ms, correct={total == N}");
            }
            else c.Send(local, 0, 20);
            c.Barrier();

            if (rank == 0)
            {
                long[] values = Enumerable.Repeat(1L, N).ToArray();
                long sharedTotal = 0;
                var watch = Stopwatch.StartNew();
                Parallel.For<long>(0, size, new ParallelOptions { MaxDegreeOfParallelism = size },
                    () => 0L,
                    (worker, state, subtotal) =>
                    {
                        int start = (int)worker * chunk;
                        for (int j = start; j < start + chunk; j++) subtotal += values[j];
                        return subtotal;
                    },
                    subtotal => System.Threading.Interlocked.Add(ref sharedTotal, subtotal));
                watch.Stop();
                Console.WriteLine($"TPL: {watch.Elapsed.TotalMilliseconds:F3} ms, correct={sharedTotal == N}");
            }
        }
    }
}