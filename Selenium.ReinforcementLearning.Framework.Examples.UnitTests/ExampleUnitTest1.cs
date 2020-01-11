namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using OpenQA.Selenium;
    using Xunit;

    public class ExampleUnitTest1
    {
        // http://automationpractice.com/index.php

        // [Train]
        public void FromAToB() // : ITrainable
        {
            // This should be more like a lower level spec
            // It should be fed to the tooling that is using algorithms and that tooling should have the references to IDriver
            // This frees us from having to lock the version of our IDriver to every test module
            // Is this possible???

            // ChromeOptions
            // Page Size()
            // SeleniumEnvironment (without inheritance)
            //      Page URL
            //      Actionable element query selectors
            //      Input data
            //      Goal validation
            // Prerequisites (other tests)
            // Initial state navigation
            // Initial epoch navigation
            // Optional: Epochs, MaximumActions

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
