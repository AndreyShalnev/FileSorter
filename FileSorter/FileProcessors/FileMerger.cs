using ExternalSorting;
using FileSorter.Extentions;
using System.Text;

public class FileMerger
{
    private readonly Config _config;
    private readonly ProgressPrinter ProgressPrinter;

    public FileMerger(Config config, ProgressPrinter progressPrinter) 
    {
        _config = config;
        ProgressPrinter = progressPrinter;
    }

    public async Task MergeFiles()
    {
        var files = Directory.GetFiles(_config.TempFolder, $"{_config.ChunkFileNameBegining}*.txt")
                     .OrderBy(file => Path.GetFileName(file))
                     .ToList();
        
        ProgressPrinter.Start(nameof(FileMerger), files.Count);

        using (var writer = new StreamWriter(_config.OutputFile))
        {
            foreach (var file in files)
            {
                var chunkData = await ReadFileAndGroupResults(file);
                await SaveToFileAsync(chunkData, writer);
                ProgressPrinter.MakeStep();
            }
        }
    }

    private async Task<Dictionary<string, List<int>>> ReadFileAndGroupResults(string file)
    {
        var result = new Dictionary<string, List<int>>();

        using (var reader = new StreamReader(file))
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var key = StringExtentions.ParseChunkFileLine(line, out var numbers);

                if (result.ContainsKey(key))
                {
                    result[key].AddRange(numbers);
                }
                else
                {
                    result[key] = numbers;
                }
            }
        }

        return result;
    }

    private async Task SaveToFileAsync(Dictionary<string, List<int>> sortedData, StreamWriter writer)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var entry in sortedData.OrderBy(i => i.Key))
        {
            foreach (var number in entry.Value.Order())
            {
                stringBuilder.AppendLine($"{number}. {entry.Key}");
            }
        }
        
        await writer.WriteAsync(stringBuilder.ToString());
    }
}
