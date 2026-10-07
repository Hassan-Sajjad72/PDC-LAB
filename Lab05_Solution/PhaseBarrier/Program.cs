using System.Diagnostics;
using System.Threading;

int workers = args.Length > 0 && int.TryParse(args[0], out int w) ? w : 4;
bool noBarrier = args.Contains("--no-barrier");

const int Rows = 100;
const int Cols = 100;
const int Rounds = 20;

int[,] values = new int[Rows, Cols];

object gate = new();
int waiting = 0;
int generation = 0;

void SignalAndWait()
{
    lock (gate)
    {
        int myGeneration = generation;
        waiting++;

        if (waiting == workers)
        {
            waiting = 0;
            generation++;
            Monitor.PulseAll(gate);
            return;
        }

        while (myGeneration == generation)
            Monitor.Wait(gate);
    }
}

void Work(int id)
{
    int start = id * Rows / workers;
    int end = (id + 1) * Rows / workers;

    for (int round = 0; round < Rounds; round++)
    {
        for (int r = start; r < end; r++)
            for (int c = 0; c < Cols; c++)
                values[r, c]++;

        if (!noBarrier) SignalAndWait();
        if (!noBarrier) SignalAndWait();
    }
}

var sw = Stopwatch.StartNew();

Thread[] threads = Enumerable.Range(0, workers)
    .Select(id => new Thread(() => Work(id)))
    .ToArray();

foreach (var t in threads) t.Start();
foreach (var t in threads) t.Join();

sw.Stop();

int mismatches = 0;
for (int r = 0; r < Rows; r++)
    for (int c = 0; c < Cols; c++)
        if (values[r, c] != Rounds) mismatches++;

Console.WriteLine($"Workers={workers}");
Console.WriteLine($"Mode={(noBarrier ? "No barrier" : "Barrier")}");
Console.WriteLine($"Correct={(mismatches == 0 ? "Y" : "N")}");
Console.WriteLine($"Mismatching cells={mismatches}");
Console.WriteLine($"Time={sw.ElapsedMilliseconds} ms");
Console.WriteLine($"Environment.ProcessorCount={Environment.ProcessorCount}");
