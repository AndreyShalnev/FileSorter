using System.Diagnostics;

namespace ExternalSorting
{
    public class ProgressPrinter
    {
        private long CurrentStep = 0;
        private long Target = 0;
        private int Progress = -1;
        private Stopwatch Stopwatch = new Stopwatch();

        public void Start(string processName, long target)
        {
            Stopwatch.Start();
            CurrentStep = 0;
            Target = target;
            Console.WriteLine($"\nStart: {processName} ");
        }

        public void MakeStep()
        {
            try
            {
                CurrentStep++;

                int progress = (int)((CurrentStep + 1) / (double)Target * 100);
                if (Progress < progress)
                {
                    Progress = progress;
                    PrintData();
                }
            }
            catch { }
        }

        public void PrintData()
        {
            TimeSpan timeElapsed = Stopwatch.Elapsed;
            string formattedTime = string.Format("{0:D2}:{1:D2}", timeElapsed.Minutes, timeElapsed.Seconds);

            Console.CursorLeft = 0;
            Console.Write($"Progress: {Progress}%      {formattedTime}");
        }



        public void MakeStep(int step)
        {
            CurrentStep += step - 1;
            MakeStep();
        }

        public void Finish(string processName)
        {
            Stopwatch.Stop();
            Progress = 100;
            PrintData(); Console.WriteLine($"\nFinish: {processName}");
        }
    }
}
