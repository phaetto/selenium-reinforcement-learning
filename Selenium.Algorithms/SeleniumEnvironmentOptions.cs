using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Selenium.Algorithms
{
    public sealed class SeleniumEnvironmentOptions
    {
        public IReadOnlyDictionary<string, string> InputTextData { get; set; } = new Dictionary<string, string>();
        public Action<string> WriteLineMethod { get; set; } = x => Debug.WriteLine(x);
        public IReadOnlyCollection<string> ActionableELementsCssSelectors { get; set; } = new string[] { "body *[data-automation-id]" };
        public string Url { get; set; } = string.Empty;
    }
}
