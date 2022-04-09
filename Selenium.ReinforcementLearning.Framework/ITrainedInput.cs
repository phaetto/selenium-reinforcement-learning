namespace Selenium.ReinforcementLearning.Framework
{
    using OpenQA.Selenium;
    using Selenium.Algorithms;
    using Selenium.Algorithms.ReinforcementLearning;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITrainedInput
    {
        public Task<Experiment<IReadOnlyCollection<ElementData>>> GetExperiment(IWebDriver driver, IPersistenceIO persistenceIO);
    }
}
