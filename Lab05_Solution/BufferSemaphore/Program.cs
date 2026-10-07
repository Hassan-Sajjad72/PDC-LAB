using System.Diagnostics;
using System.Threading;

int producers = args.Length > 0 ? int.Parse(args[0]) : 2;
int consumers = args.Length > 1 ? int.Parse(args[1]) : 2;
int capacity = args.Length > 2 ? int.Parse(args[2]) : 8;

const int ItemsPerProducer = 2500;
int total = producers * ItemsPerProducer;

int[] buffer = new int[capacity];
int inPos = 0, outPos = 0;
object gate = new();

using SemaphoreSlim emptySlots = new(capacity, capacity);
using SemaphoreSlim fullSlots = new(0, capacity);

int[] seen = new int[total];
int nextToConsume = 0;

var sw = Stopwatch.StartNew();

Thread[] producerThreads = new Thread[producers];
Thread[] consumerThreads = new Thread[consumers];

for (int p = 0; p < producers; p++)
{
    int producerId = p;
    producerThreads[p] = new Thread(() =>
    {
        int start = producerId * ItemsPerProducer;
        for (int i = 0; i < ItemsPerProducer; i++)
        {
            emptySlots.Wait();

            lock (gate)
            {
                buffer[inPos] = start + i;
                inPos = (inPos + 1) % capacity;
            }

            fullSlots.Release();
        }
    });
}

for (int c = 0; c < consumers; c++)
{
    consumerThreads[c] = new Thread(() =>
    {
        while (true)
        {
            lock (gate)
            {
                if (nextToConsume >= total) return;
                nextToConsume++;
            }

            fullSlots.Wait();

            int item;
            lock (gate)
            {
                item = buffer[outPos];
                outPos = (outPos + 1) % capacity;
            }

            emptySlots.Release();
            Interlocked.Increment(ref seen[item]);
        }
    });
}

foreach (var t in producerThreads) t.Start();
foreach (var t in consumerThreads) t.Start();
foreach (var t in producerThreads) t.Join();

for (int i = 0; i < consumers; i++)
    fullSlots.Release();

foreach (var t in consumerThreads) t.Join();
sw.Stop();

int lost = 0, duplicated = 0;
for (int i = 0; i < total; i++)
{
    if (seen[i] == 0) lost++;
    if (seen[i] > 1) duplicated += seen[i] - 1;
}

Console.WriteLine($"Configuration={producers}/{consumers}/{capacity}");
Console.WriteLine($"Lost={lost}");
Console.WriteLine($"Duplicated={duplicated}");
Console.WriteLine($"Time={sw.ElapsedMilliseconds} ms");
