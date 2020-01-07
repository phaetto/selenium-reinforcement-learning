namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using System.Collections.Generic;
    using System.Linq;

    public static class StringListExtensions
    {
        private const string GetElementsInformationJavaScript = @"
var list = [];
for(var i = 0; i < arguments.length; ++i) {
    var elements = document.querySelectorAll(arguments[i]);
    
    Array.prototype.push.apply(list, elements);
}
return list;
";

        public static IReadOnlyList<IWebElement> GetElementsFromQuerySelectors(this IReadOnlyCollection<string> webElementQuerySelectorCollection, IJavaScriptExecutor javaScriptExecutor)
        {
            if (webElementQuerySelectorCollection.Count == 0)
            {
                return new List<IWebElement>().AsReadOnly();
            }

            var result = (IReadOnlyCollection<object>)javaScriptExecutor.ExecuteScript(GetElementsInformationJavaScript, webElementQuerySelectorCollection.Cast<object>().ToArray());
            return result.Cast<IWebElement>()
                .ToList()
                .AsReadOnly();
        }
    }
}
