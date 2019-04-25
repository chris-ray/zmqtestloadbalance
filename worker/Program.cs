using NetMQ;
using NetMQ.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace worker
{
    class Program
    {
        static Random rand = new Random();

        static void Main(string[] args)
        {
            int num = 1;

            if (args.Length == 1 && int.TryParse(args[0], out var num_i) && num_i > 1)
                num = num_i;

            Console.WriteLine($"Starting {num} worker(s)");

            for (int i = 0; i < num; i++)
                CreateWorker(i);

            Console.ReadLine();
        }

        static void CreateWorker(int workerId)
        {
            Task.Run(() =>
            {
                using (var worker = new RequestSocket())  // connect
                {
                    worker.Options.Identity = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
                    worker.Connect("tcp://localhost:5556");

                    // Send a message from the client socket
                    worker.SendFrame("READY");

                    while (true)
                    {
                        var message = worker.ReceiveMultipartBytes();

                        var requestText = System.Text.Encoding.UTF8.GetString(message[2].AsSpan());

                        int delayMs = rand.Next(50, 150);

                        Console.WriteLine($"WORKER {workerId}: {requestText} ({delayMs})");

                        if (delayMs > 0)
                            Thread.Sleep(delayMs);

                        worker.SendMoreFrame(message[0]);
                        worker.SendMoreFrameEmpty();
                        worker.SendFrame("OK");
                    }
                }
            });
        }
    }
}
