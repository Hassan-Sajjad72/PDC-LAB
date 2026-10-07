using System.Diagnostics;
using System.Threading;

const int BufferSize = 8;
const int Items = 10000;

int[] buffer = new int[BufferSize];
int inPos = 0, outPos = 0, count = 0;
int producerRetries = 0, consumerRetries = 0, orderErrors = 0;
long checksum = 0, expectedChecksum = (long)Items * (Items + 1) / 2;
object gate = new();

var sw = Stopwatch.StartNew();

Thread producer = new(() =>
{
    for (int i = 1; i <= Items; i++)
    {
        while (true)
        {
            lock (gate)
            {
                if (count < BufferSize)
                {
                    buffer[inPos] = i;
                    inPos = (inPos + 1) % BufferSize;
                    count++;
                    break;
                }
            }
            producerRetries++;
            Thread.Yield();
        }
    }
});

Thread consumer = new(() =>
{
    int expected = 1;
    for (int i = 0; i < Items; i++)
    {
        int item;
        while (true)
        {
            lock (gate)
            {
                if (count > 0)
                {
                    item = buffer[outPos];
                    outPos = (outPos + 1) % BufferSize;
                    count--;
                    break;
                }
            }
            consumerRetries++;
            Thread.Yield();
        }

        checksum += item;
        if (item != expected) orderErrors++;
        expected++;
    }
});

producer.Start();
consumer.Start();
producer.Join();
consumer.Join();
sw.Stop();

Console.WriteLine($"Items={Items}");
Console.WriteLine($"Checksum={checksum}, Expected={expectedChecksum}, Checksum OK={(checksum == expectedChecksum ? "Y" : "N")}");
Console.WriteLine($"Order errors={orderErrors}");
Console.WriteLine($"Producer retries={producerRetries}");
Console.WriteLine($"Consumer retries={consumerRetries}");
Console.WriteLine($"Time={sw.ElapsedMilliseconds} ms");
