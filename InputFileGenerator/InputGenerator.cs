using System.Diagnostics;
using System.Text;

namespace ExternalSorting
{
    public class InputGenerator
    {
        private readonly Random _random = new Random();
        private readonly Config _config;
        private readonly HashSet<string> _generatedLines = new HashSet<string>();
        private bool _isGeneratedLinesFull = false;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public InputGenerator(Config config)
        {
            _config = config;
        }

        public async Task GenerateDataToFileAsync(string fileName)
        {
            _stopwatch.Start();
            using (var writer = new StreamWriter(fileName, append: true))
            {
                for (int i = 0; i < _config.NumberOfLines; i++)
                {
                    string line = GenerateLine();

                    await writer.WriteLineAsync(line);

                    PrintProgress(i, _config.NumberOfLines);
                }
            }
            _stopwatch.Stop();
            Console.WriteLine();
        }

        public string GenerateLine()
        {
            int number = _random.Next(_config.MinInt, _config.MaxInt);
            int wordCount = _random.Next(1, _config.MaxWordsPerLine);

            if (_random.NextDouble() < _config.RepeatProbability && _generatedLines.Count > 0)
            {
                var repeatedString = _generatedLines.ElementAt(_random.Next(_generatedLines.Count-1));
                return $"{number}. {repeatedString}";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < wordCount; i++)
            {
                int wordLength = _random.Next(_config.MinStringLength, _config.MaxStringLength) - sb.Length;

                if (wordLength <= 0)
                    break;

                sb.Append(GenerateRandomWord(wordLength));

                if (i < wordCount - 1)
                    sb.Append(" ");
            }
            
            if (!_isGeneratedLinesFull
                && _random.NextDouble() < _config.RepeatProbability)
            {
                if (_generatedLines.Count > 2000) 
                    _isGeneratedLinesFull = true;

                _generatedLines.Add(sb.ToString());
            }

            string randomString = sb.ToString();
            return $"{number}. {randomString}";
        }

        public string GenerateRandomWord(int length)
        {
            StringBuilder word = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                word.Append((char)_random.Next(97, 123)); 
            }
            return word.ToString();
        }

        private void PrintProgress(int i, int target)
        {
            if (i % (target / 100) == 0 || i == target - 1)
            {
                TimeSpan timeElapsed = _stopwatch.Elapsed;
                string formattedTime = string.Format("{0:D2}:{1:D2}", timeElapsed.Minutes, timeElapsed.Seconds);

                int progress = (int)((i + 1) / (double)target * 100);
                Console.CursorLeft = 0;
                Console.Write($"Progress: {progress}%      {formattedTime}");
            }
        }
    }
}
