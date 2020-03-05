using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Selenium.Algorithms
{
    public sealed class SeleniumEnvironmentOptions : ISeleniumEnvironmentOptions
    {
        public IReadOnlyDictionary<string, string> InputTextData { get; set; } = new Dictionary<string, string>();
        public Action<string> WriteLine { get; set; } = x => Debug.WriteLine(x);
        public IReadOnlyCollection<string> ActionableElementsCssSelectors { get; set; } = new string[] { "body *[data-automation-id]" };
        public IReadOnlyCollection<string> LoadingElementsCssSelectors { get; set; } = new string[0];
        public string Url { get; set; } = string.Empty;
    }
}
