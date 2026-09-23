using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    const int Iterations = 50;

    static void Main(string[] args)
    {
        // Child process immediately exits
        if (args.Length > 0 && args[0] == "--child")
        {
            return;
        }

        string executablePath = Environment.ProcessPath!;

        // Measure process creation
        Stopwatch processStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < Iterations; i++)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = false
            };

            startInfo.ArgumentList.Add("--child");

            using Process process = Process.Start(startInfo)!;
            process.WaitForExit();
        }

        processStopwatch.Stop();

        // Measure thread creation
        Stopwatch threadStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < Iterations; i++)
        {
            Thread thread = new Thread(() => { });

            thread.Start();
            thread.Join();
        }

        threadStopwatch.Stop();

        double avgProcessMs =
            processStopwatch.Elapsed.TotalMilliseconds / Iterations;

        double avgThreadMs =
            threadStopwatch.Elapsed.TotalMilliseconds / Iterations;

        double ratio = avgProcessMs / avgThreadMs;

        Console.WriteLine(
            $"Average process creation time: {avgProcessMs:F3} ms");

        Console.WriteLine(
            $"Average thread creation time: {avgThreadMs:F3} ms");

        Console.WriteLine(
            $"Process creation was {ratio:F1}x more expensive than thread creation.");
    }
}