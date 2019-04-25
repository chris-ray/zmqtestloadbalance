using NetMQ.Sockets;
using NetMQ;
using System;
using System.Threading.Tasks;

namespace frontend
{
    class Program
    {
        static volatile int counter = 0;

        static void Main(string[] args)
        {
            //int num = 1;

            //if (args.Length == 1 && int.TryParse(args[0], out var num_i) && num_i > 1)
            //    num = num_i;

            //Console.WriteLine($"Starting {num} client(s)");

            //for (int i = 0; i < num; i++)
            //    CreateClient(i);

            using (var client = new RequestSocket(">tcp://localhost:5557"))
            {
                while (true)
                {
                    var input = "test";

                    client.SendFrame(input);
                    var str = client.ReceiveFrameString();

                    Console.WriteLine($"Client:  - {str}: {counter++}");
                }
            }

            Console.ReadLine();
        }

        //static void CreateClient(int clientId)
        //{
        //    Task.Run(() =>
        //    {
        //        using (var client = new RequestSocket(">tcp://localhost:5557"))
        //        {
        //            while (true)
        //            {
        //                var input = "test";

        //                client.SendFrame(input);
        //                var str = client.ReceiveFrameString();

        //                Console.WriteLine($"Client: {clientId} - {str}: {counter++}");
        //            }
        //        }
        //    });
        //}
    }
}
