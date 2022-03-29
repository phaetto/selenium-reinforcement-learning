namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISeleniumEnvironmentOptions
    {
        IReadOnlyCollection<string> ActionableElementsCssSelectors { get; set; }
        IReadOnlyDictionary<string, string> InputTextData { get; set; }
        IReadOnlyCollection<string> LoadingElementsCssSelectors { get; set; }
        string Url { get; set; }
        Action<string> WriteLine { get; set; }
        Func<IWebDriver, ISeleniumEnvironmentOptions, Task> SetupInitialState { get; set; }
    }
}