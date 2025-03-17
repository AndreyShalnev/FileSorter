using ExternalSorting;

namespace FileSorter.FileProcessors
{
    internal class TempFileCleaner
    {
        private readonly Config _config;

        public TempFileCleaner(Config config)
        {
            _config = config;
        }

        public async Task<bool> DeleteChunkFilesAsync()
        {
            if (!Directory.Exists(_config.TempFolder))
                return true;

            var files = Directory.GetFiles(_config.TempFolder, $"{_config.ChunkFileNameBegining}*");
            var deleteTasks = files.Select(file => DeleteFileAsync(file)).ToList();

            await Task.WhenAll(deleteTasks);

            return deleteTasks.All(task => task.Result);
        }

        private async Task<bool> DeleteFileAsync(string file)
        {
            try
            {
                await Task.Run(() => File.Delete(file));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file {file}: {ex.Message}");
                return false;
            }
        }
    }
}
