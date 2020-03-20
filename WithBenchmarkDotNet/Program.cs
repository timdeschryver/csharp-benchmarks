using BenchmarkDotNet.Running;

namespace WithBenchmarkDotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<JoinBenchmark>();
        }
    }
}
