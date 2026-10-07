using System.Diagnostics;
using System.Threading;

string mode = args.Length > 0 ? args[0] : "cond";
int workers = args.Length > 1 && int.TryParse(args[1], out int n) ? n : 1;
bool signalFirst = args.Length > 2 && args[2] == "signal-first";

object gate = new();
bool dataReady = false;
int value = 0;
int blocked = 0;
int received = 0;

void Publish()
{
    lock (gate)
    {
        value = 42;
        dataReady = true;
        Monitor.PulseAll(gate);
    }
}

void Worker()
{
    lock (gate)
    {
        while (!dataReady)
        {
            Interlocked.Increment(ref blocked);
            Monitor.Wait(gate);
        }

        received = value;
    }
}

var sw = Stopwatch.StartNew();
var process = Process.GetCurrentProcess();
var cpuStart = process.TotalProcessorTime;

if (mode == "spin")
{
    Thread worker = new(() =>
    {
        while (!Volatile.Read(ref dataReady))
            Thread.SpinWait(1000);

        received = Volatile.Read(ref value);
    });

    worker.Start();
    Thread.Sleep(100);
    Volatile.Write(ref value, 42);
    Volatile.Write(ref dataReady, true);
    worker.Join();
}
else
{
    Thread[] threads = Enumerable.Range(0, workers)
        .Select(_ => new Thread(Worker))
        .ToArray();

    if (signalFirst)
        Publish();

    foreach (var t in threads) t.Start();

    if (!signalFirst)
    {
        Thread.Sleep(100);
        Publish();
    }

    foreach (var t in threads) t.Join();
}

sw.Stop();
var cpu = process.TotalProcessorTime - cpuStart;

Console.WriteLine($"Scenario={mode}{(signalFirst ? " signal-first" : "")}");
Console.WriteLine($"Workers={workers}");
Console.WriteLine($"Times blocked={blocked}");
Console.WriteLine($"Value received={received}");
Console.WriteLine($"Wall time={sw.ElapsedMilliseconds} ms");
Console.WriteLine($"CPU time={cpu.TotalMilliseconds:F2} ms");
