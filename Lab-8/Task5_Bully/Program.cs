using System;
using MPI;
class Program
{
    static void Main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            var c = Communicator.world;
            int crashed = args.Length > 0 ? int.Parse(args[0]) : c.Size - 1;
            int initiator = args.Length > 1 ? int.Parse(args[1]) : 0;
            if (crashed < 0 || crashed >= c.Size || initiator == crashed)
                throw new Exception("Invalid crashed rank or initiator");
            // Simulated crash: all ranks remain alive in MPI to avoid aborting the job.
            int candidate = c.Rank == crashed ? -1 : c.Rank;
            int leader = c.Allreduce(candidate, Operation<int>.Max);
            if (c.Rank == initiator)
            {
                for (int higher = initiator + 1; higher < c.Size; higher++)
                    if (higher != crashed) Console.WriteLine($"ELECTION {initiator} -> {higher}; OK {higher} -> {initiator}");
                Console.WriteLine($"COORDINATOR: rank {leader}; crashed={crashed}");
            }
            c.Barrier();
            if (c.Rank != crashed) Console.WriteLine($"Rank {c.Rank} agrees leader={leader}");
        }
    }
}