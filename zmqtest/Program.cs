using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;

namespace zmqtest
{
    class Program
    {
        static List<byte[]> worker_queue = new List<byte[]>();

        static void Main(string[] args)
        {
            var readySpan = System.Text.Encoding.UTF8.GetBytes("READY").AsSpan();

            using (var backend = new RouterSocket("@tcp://localhost:5556"))
            using (var frontend = new RouterSocket("@tcp://localhost:5557"))
            {
                Console.WriteLine("Server start.");

                while (true)
                {
                    if (backend.Poll(PollEvents.PollIn, TimeSpan.FromMilliseconds(64)) == PollEvents.PollIn)
                    {
                        // Receive the message from the backend socket
                        var message = new List<byte[]>();
                        if (!backend.TryReceiveMultipartBytes(ref message)) return;

                        var workerId = message[0];

                        worker_queue.Add(workerId);

                        if (!readySpan.SequenceEqual(message[2].AsSpan()))
                        {
                            frontend.SendMoreFrame(message[2]);
                            frontend.SendMoreFrameEmpty();
                            frontend.SendFrame(message[4]);
                        }
                        else
                            Console.WriteLine($"Worker {workerId} is ready!");
                    }

                    if (worker_queue.Count > 0)
                    {
                        // Poll frontend only if we have available workers

                        if (frontend.Poll(PollEvents.PollIn, TimeSpan.FromMilliseconds(64)) == PollEvents.PollIn)
                        {
                            var message = new List<byte[]>();
                            if (!frontend.TryReceiveMultipartBytes(ref message)) return;

                            backend.SendMoreFrame(worker_queue[0]);
                            backend.SendMoreFrameEmpty();
                            backend.SendMoreFrame(message[0]);
                            backend.SendMoreFrameEmpty();
                            backend.SendFrame(message[2]);

                            worker_queue.RemoveAt(0);
                        }
                    }
                }
            }
        }
    }
}
