namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using System.Collections.Generic;
    using System.Linq;

    public static class StringListExtensions
    {
        private const string GetElementsFromJavaScript = @"
var list = [];
for(var i = 0; i < arguments.length; ++i) {
    var elements = document.querySelectorAll(arguments[i]);
    
    Array.prototype.push.apply(list, elements);
}
return list;
";

        private const string GetListsOfElementsFromJavaScript = @"
var list = [];
for(var i = 0; i < arguments.length; ++i) {
    var argList = arguments[i];
    var result = [];
    for(var j = 0; j < argList.length; ++j) {
        var elements = document.querySelectorAll(argList[j]);
    
        Array.prototype.push.apply(result, elements);
    }
    list.push(result);
}
return list;
";

        public static IReadOnlyList<IWebElement> GetElementsFromQuerySelectors(this IReadOnlyCollection<string> webElementQuerySelectorCollection, IJavaScriptExecutor javaScriptExecutor)
        {
            if (webElementQuerySelectorCollection.Count == 0)
            {
                return new List<IWebElement>().AsReadOnly();
            }

            var result = (IReadOnlyCollection<object>)javaScriptExecutor.ExecuteScript(GetElementsFromJavaScript, webElementQuerySelectorCollection.Cast<object>().ToArray());
            return result.Cast<IWebElement>()
                .ToList()
                .AsReadOnly();
        }

        public static IReadOnlyList<IReadOnlyList<IWebElement>> GetMultiListElementsFromQuerySelectors(
            this IReadOnlyCollection<IReadOnlyCollection<string>> webElementQuerySelectorCollection,
            IJavaScriptExecutor javaScriptExecutor)
        {
            if (webElementQuerySelectorCollection.Count == 0)
            {
                return new List<List<IWebElement>>().AsReadOnly();
            }

            var result = (IReadOnlyCollection<object>)javaScriptExecutor.ExecuteScript(GetListsOfElementsFromJavaScript, webElementQuerySelectorCollection.Cast<object>().ToArray());
            return result
                .Select(x => x as IReadOnlyList<IWebElement> ?? Enumerable.Empty<IWebElement>().ToList().AsReadOnly())
                .Cast<IReadOnlyList<IWebElement>>()
                .ToList()
                .AsReadOnly();
        }
    }
}
