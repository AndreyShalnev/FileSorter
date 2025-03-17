using ExternalSorting;
using FileSorter.Extentions;
using System.Text;

namespace FileSorter.FileProcessors
{
    public class FileSplitter
    {
        private readonly Config _config;
        private readonly ProgressPrinter ProgressPrinter;

        public FileSplitter(Config config, ProgressPrinter progressPrinter)
        {
            _config = config;
            ProgressPrinter = progressPrinter;
        }


        /// <summary>
        /// Method read the data from big file and split it into chunks basing on string start 
        /// and group numbers for the same string. It shold save some memory because the key string
        /// would be less repeatable in the chanked files.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        public async Task SplitFileIntoChunksAsync(string inputFile)
        {
            if (!Directory.Exists(_config.TempFolder))
            {
                Directory.CreateDirectory(_config.TempFolder);
            }

            var fileInfo = new FileInfo(inputFile);
            ProgressPrinter.Start(nameof(FileSplitter), (long)(fileInfo.Length * 1.9));

            using (var reader = new StreamReader(inputFile))
            {

                while (!reader.EndOfStream)
                {
                    var chunkBuffers = new Dictionary<string, Dictionary<string, List<int>>>();
                    var chunksOfStringsWithNumbers = await ReadChunk(reader);

                    foreach (var chunk in chunksOfStringsWithNumbers)
                    {
                        string chunkFile = Path.Combine(_config.TempFolder, $"{_config.ChunkFileNameBegining}{chunk.Key}.txt");
                        StringBuilder stringBuilder = new StringBuilder();

                        // Ordering here isn't required, but it would make the data in chunk file partially pre_sorted
                        // and makes final sorting faster.
                        foreach (var kvp in chunk.Value.OrderBy(x => x.Key))
                        {
                            string numbers = string.Join(',', kvp.Value.OrderBy(x => x));
                            stringBuilder.AppendLine($"{numbers}. {kvp.Key}");
                            //ProgressPrinter.PrintData();
                        }

                        using (var writer = new StreamWriter(chunkFile, append: true))
                        {
                            await writer.WriteAsync(stringBuilder.ToString());
                        }
                    }
                }
                ProgressPrinter.Finish(nameof(FileSplitter));
            }
        }

        /// <summary>
        /// Read chunk and group it by chunk name and then by string key. 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, Dictionary<string, List<int>>>> ReadChunk(StreamReader reader)
        {
            var chunkedGroupedLines = new Dictionary<string, Dictionary<string, List<int>>>();

            for (int i = 0; i < _config.ChunkSize && !reader.EndOfStream; i++)
            {
                string line = await reader.ReadLineAsync();
                var text = StringExtentions.ParseOriginalLine(line, out int number);
                var chunkName = GetStringChunkFileName(text);

                if (!chunkedGroupedLines.ContainsKey(chunkName))
                    chunkedGroupedLines[chunkName] = new Dictionary<string, List<int>>();

                var chunk = chunkedGroupedLines[chunkName];

                if (!chunk.ContainsKey(text))
                    chunk[text] = new List<int>();

                chunk[text].Add(number);
                
                ProgressPrinter.MakeStep(line.Length * sizeof(char));
            }

            return chunkedGroupedLines;
        }

        private string GetStringChunkFileName(string str)
        {
            if (str.Length >= 2)
                return str.Substring(0, 2).ToLower();

            if (str.Length == 1)
                return str.ToLower() + "_";

            return "__";

        }
    }
}
