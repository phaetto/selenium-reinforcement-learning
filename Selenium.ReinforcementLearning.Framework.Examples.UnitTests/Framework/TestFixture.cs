namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests.Framework
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Reflection;

    public sealed class TestFixture
    {
        public TestFixture()
        {
        }

        public WebDriver GetWebDriver()
        {
            var chromeOptions = new ChromeOptions();
            if (!Debugger.IsAttached)
            {
                chromeOptions.AddArgument("--headless");
                chromeOptions.AddArgument("--disable-gpu");
            }

            var binaryFolder = Path.Combine(AssemblyDirectory, "binaries", "ungoogled-chromium-96.0.4664.45-1_Win64");
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                binaryFolder = "/usr/local/bin/";
                chromeOptions.AddArgument("--disable-dev-shm-usage"); // overcome limited resource problems
                chromeOptions.AddArgument("disable-infobars"); // disabling infobars
                chromeOptions.AddArgument("--disable-extensions"); // disabling extensions
                chromeOptions.AddArgument("--no-sandbox"); // Bypass OS security model
            }

            var driver = new ChromeDriver(binaryFolder, chromeOptions);
            driver.Manage().Window.Size = new Size(1000, 768);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

            return driver;
        }

        public IPersistenceIO GetPersistenceProvider()
        {
            throw new NotImplementedException();
        }

        public static string AssemblyDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidProgramException();
            }
        }
    }

}
