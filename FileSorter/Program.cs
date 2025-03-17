using FileSorter.FileProcessors;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Security.Principal;
using static System.Net.Mime.MediaTypeNames;

namespace ExternalSorting
{

    class Program
    {
        private static StreamWriter Logger;

        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build()
                .Get<Config>();

            Logger = new StreamWriter("Log.txt", append: true);
            
            if (!File.Exists(config.InputFile))
            {
                Console.WriteLine($"Input file doesn't exist!");
                return;
            }
            PrintFileInfo(config.InputFile, false);
            Console.WriteLine($"Chunk size: {config.ChunkSize}");

            if (File.Exists(config.OutputFile))
            {
                Console.WriteLine($"Output file already exist!");
                Console.WriteLine($"Can't save data into existing file");
                Console.WriteLine("Specify another output file");
                return;
            }

            var stopwatchSplitter = new Stopwatch();
            var stopwatchMerger = new Stopwatch();

            var cleaner = new TempFileCleaner(config);
            var splitter = new FileSplitter(config, new ProgressPrinter());
            var merger = new FileMerger(config, new ProgressPrinter());

            var cleanResult = await cleaner.DeleteChunkFilesAsync();
            if (!cleanResult)
            {
                Console.WriteLine("Temp chunks wasn't cleaned up. Can't start processing!");
                return;
            }

            stopwatchSplitter.Start();
            await splitter.SplitFileIntoChunksAsync(config.InputFile);
            stopwatchSplitter.Stop();

            stopwatchMerger.Start();
            await merger.MergeFiles();
            stopwatchMerger.Stop();


            LogMainInfo(config, stopwatchSplitter, stopwatchMerger);
            Logger.Close();
        }

        private static void LogMainInfo(Config? config, Stopwatch stopwatchSplitter, Stopwatch stopwatchMerger)
        {
            PrintFileInfo(config.InputFile);
            PrintExecutionData("Splitter", stopwatchSplitter);
            PrintExecutionData("Merger", stopwatchMerger);
            Console.WriteLine($"Chunk size: {config.ChunkSize}");
            Logger.WriteLine($"Chunk size: {config.ChunkSize}");
            Logger.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private static void PrintFileInfo(string fileName, bool writeIntoLogFile = true)
        {
            var fileInfo = new FileInfo(fileName);
            double fileSizeInBytes = fileInfo.Length;  
            double fileSizeInGB = fileSizeInBytes / (1024 * 1024 * 1024);  

            Console.WriteLine($"\n\nFile size: {fileSizeInGB:F2} Gb");

            if (writeIntoLogFile ) 
                Logger.WriteLine($"\n\nFile size: {fileSizeInGB:F2} Gb");

        }

        private static void PrintExecutionData(string processName, Stopwatch stopwatch)
        {
            TimeSpan timeElapsed = stopwatch.Elapsed;
            string formattedTime = string.Format("{0:D2}:{1:D2}.{2:D3}",
                timeElapsed.Minutes, timeElapsed.Seconds, timeElapsed.Milliseconds);

            Console.WriteLine($"{processName} Execution time: {formattedTime}");
            Logger.WriteLine($"{processName} Execution time: {formattedTime}");
        }

        private async Task MergeChunksAsync(List<string> chunkFiles, string outputFile)
        {
            var readers = chunkFiles.Select(f => new StreamReader(f)).ToList();
            var queue = new SortedDictionary<string, StreamReader>();

            foreach (var reader in readers)
            {
                if (!reader.EndOfStream)
                {
                    queue.Add(await reader.ReadLineAsync(), reader);
                }
            }

            using (var writer = new StreamWriter(outputFile))
            {
                while (queue.Count > 0)
                {
                    var (line, reader) = queue.First();
                    queue.Remove(line);
                    await writer.WriteLineAsync(line);

                    if (!reader.EndOfStream)
                    {
                        queue.Add(await reader.ReadLineAsync(), reader);
                    }
                }
            }

            foreach (var reader in readers)
            {
                reader.Dispose();
            }
        }
    }
}
