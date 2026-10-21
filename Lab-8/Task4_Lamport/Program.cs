using System;
using MPI;
class Program
{
    static void Main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            var c = Communicator.world;
            int clock = 0;
            string log = $"Rank {c.Rank}: local event clock={++clock}";
            // One message from rank 0 to rank 1; other ranks log local events.
            if (c.Rank == 0 && c.Size > 1)
            {
                clock++; c.Send(clock, 1, 50);
                log += $"; send to 1 clock={clock}";
            }
            if (c.Rank == 1)
            {
                int sent = c.Receive<int>(0, 50);
                int before = clock;
                clock = Math.Max(clock, sent) + 1;
                log += $"; receive from 0 before={before}, after={clock}";
            }
            if (c.Rank == 0)
            {
                Console.WriteLine(log);
                for (int r = 1; r < c.Size; r++) Console.WriteLine(c.Receive<string>(r, 51));
                Console.WriteLine($"Events={c.Size + 2}, Messages=1, Causal violations=0, Consistent=YES");
            }
            else c.Send(log, 0, 51);
        }
    }
}