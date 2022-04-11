namespace Selenium.ReinforcementLearning.Framework
{
    using System;
    using Xunit;
    using Xunit.Sdk;

    [XunitTestCaseDiscoverer("Selenium.ReinforcementLearning.Framework.TestCaseTrainFactDiscoverer", "Selenium.ReinforcementLearning.Framework")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class TrainAttribute : FactAttribute
    {
    }
}
