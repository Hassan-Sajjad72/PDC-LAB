using System;
using MPI;

class Program
{
    static void Main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            Intracommunicator comm = Communicator.world;
            int rank = comm.Rank, size = comm.Size;
            string host = System.Environment.MachineName;
            string line = $"Rank {rank}/{size} on {host}";
            Console.WriteLine("Unordered: " + line);

            if (rank == 0)
            {
                Console.WriteLine("Ordered report:");
                Console.WriteLine(line);
                for (int r = 1; r < size; r++)
                    Console.WriteLine(comm.Receive<string>(r, 10));
            }
            else comm.Send(line, 0, 10);
        }
    }
}