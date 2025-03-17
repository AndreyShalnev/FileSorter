namespace FileSorter.Extentions
{
    internal static class StringExtentions
    {
        public static string ParseOriginalLine(string line, out int number)
        {
            int dotIndex = line.IndexOf('.');
            number = int.Parse(line.AsSpan(0,dotIndex));
            return line.Substring(dotIndex + 1).Trim();
        }

        public static string ParseChunkFileLine(string line, out List<int> numbers)
        {
            int dotIndex = line.IndexOf('.');
            numbers = line[..dotIndex].Split(',').Select(int.Parse).ToList();
            return line.Substring(dotIndex + 1).Trim();
        }
    }
}
