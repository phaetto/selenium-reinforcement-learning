namespace Selenium.Algorithms.IntegrationTests.Framework
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
                chromeOptions.AddArgument("headless");
            }

            var binaryFolder = @"ungoogled-chromium-96.0.4664.45-1_Win64";
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                binaryFolder = @"ungoogled-chromium_96.0.4664.45_1.vaapi_linux";
            }

            Console.WriteLine("Starting chromium...");

            var driver = new ChromeDriver(Path.Combine(AssemblyDirectory, "binaries", binaryFolder), chromeOptions);
            driver.Manage().Window.Size = new Size(1000, 768);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(5);

            return driver;
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
