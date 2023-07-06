using Grpc.Net.Client;
using GrpcServiceDemo;
using System;
using System.Threading.Tasks;

namespace GrpcClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(new HelloRequest { Name = "hh" });

            Console.WriteLine(reply);
            Console.WriteLine("Press any key to exit ... ");
            Console.ReadLine();
        }
    }
}
