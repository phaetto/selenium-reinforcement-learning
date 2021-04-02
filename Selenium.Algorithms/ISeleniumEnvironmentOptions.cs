namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using System;
    using System.Collections.Generic;

    public interface ISeleniumEnvironmentOptions
    {
        IReadOnlyCollection<string> ActionableElementsCssSelectors { get; set; }
        IReadOnlyDictionary<string, string> InputTextData { get; set; }
        IReadOnlyCollection<string> LoadingElementsCssSelectors { get; set; }
        string Url { get; set; }
        Action<string> WriteLine { get; set; }
        Action<IWebDriver, ISeleniumEnvironmentOptions> SetupInitialState { get; set; }
    }
}