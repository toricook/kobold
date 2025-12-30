using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;
using System;
using System.Linq;
using System.Reflection;

namespace Tests.Performance
{
    /// <summary>
    /// Performance profiler runner that generates markdown documentation.
    /// Run with: dotnet run -c Release --project Tests --framework net8.0 --no-build
    /// </summary>
    public class PerformanceProfiler
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Kobold Engine Performance Profiler");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            var config = ManualConfig.Create(DefaultConfig.Instance)
                .AddExporter(MarkdownExporter.GitHub)
                .AddExporter(CsvMeasurementsExporter.Default)
                .AddExporter(HtmlExporter.Default);

            // Run all benchmarks in this assembly
            var summary = BenchmarkRunner.Run(Assembly.GetExecutingAssembly(), config, args);

            Console.WriteLine();
            Console.WriteLine("===========================================");
            Console.WriteLine("Performance profiling complete!");
            Console.WriteLine($"Results saved to: {summary.FirstOrDefault()?.ResultsDirectoryPath}");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("Look for:");
            Console.WriteLine("  - *-report.md files for markdown documentation");
            Console.WriteLine("  - *-measurements.csv for raw data");
            Console.WriteLine("  - *-report.html for interactive results");
        }
    }
}
