namespace Selenium.Algorithms.IntegrationTests.Framework
{
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Remote;
    using System.Drawing;

    public sealed class TestFixture
    {
        public TestFixture()
        {
        }

        public RemoteWebDriver GetWebDriver()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");

            var driver = new ChromeDriver(@".\", chromeOptions);
            driver.Manage().Window.Size = new Size(1000, 768);

            return driver;
        }
    }

}
