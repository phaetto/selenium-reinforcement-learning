namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public sealed class SeleniumEnvironmentOptions : ISeleniumEnvironmentOptions
    {
        public IReadOnlyDictionary<string, string> InputTextData { get; set; } = new Dictionary<string, string>();
        public Action<string> WriteLine { get; set; } = x => Debug.WriteLine(x);
        public IReadOnlyCollection<string> ActionableElementsCssSelectors { get; set; } = new string[] { "body *[data-automation-id]" };
        public IReadOnlyCollection<string> LoadingElementsCssSelectors { get; set; } = new string[0];
        public string Url { get; set; } = string.Empty;
        public Action<IWebDriver, ISeleniumEnvironmentOptions> SetupInitialState { get; set; } = (webDriver, options) => webDriver.Navigate().GoToUrl(options.Url);

    }
}
