namespace Selenium.Algorithms.ReinforcementLearning.Repetitions
{
    public class RepetitionContext
    {
        public int MaximumRepetitions { get; }
        public int CurrentCounter { get; private set; } = 0;

        public RepetitionContext(int maximumRepetitions)
        {
            MaximumRepetitions = maximumRepetitions;
        }

        public bool Step()
        {
            ++CurrentCounter;
            return CurrentCounter < MaximumRepetitions;
        }

        public bool CanContinue()
        {
            return CurrentCounter < MaximumRepetitions;
        }
    }
}
