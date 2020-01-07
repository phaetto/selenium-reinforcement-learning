namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using OpenQA.Selenium;
    using Xunit;

    public class ExampleUnitTest1
    {
        // http://automationpractice.com/index.php

        // [Train]
        public void FromAToB(IWebDriver webDriver) // : ITrainable
        {
            // Page URL
            // Actionable element query selectors
            // Input data
            // Prerequisites (other tests)
            // Initial state navigation
            // Goal validation

            // Trains from A -> B
            // return ITrainable
        }

        [Fact]
        public void Test1()
        {
            // Use ChromeDriver to initialize

            // var trainedTest = new TrainedTest(chromeDriver, inputData)
            // trainedTest.Do(FromAToB)
            // trainedTest.Do(FromBToC)

            // Check driver for page verification
        }
    }
}
