namespace Selenium.Algorithms.IntegrationTests.Framework
{
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Remote;
    using System;
    using System.Diagnostics;
    using System.Drawing;

    public sealed class TestFixture
    {
        public TestFixture()
        {
        }

        public RemoteWebDriver GetWebDriver()
        {
            var chromeOptions = new ChromeOptions();
            if (!Debugger.IsAttached)
            {
                chromeOptions.AddArgument("headless");
            }

            var driver = new ChromeDriver(@".\", chromeOptions);
            driver.Manage().Window.Size = new Size(1000, 768);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(5);

            return driver;
        }
    }

}
