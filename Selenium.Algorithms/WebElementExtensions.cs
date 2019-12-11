namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Remote;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class WebElementExtensions
    {
        /*
         * TODO:
         *  GetAttribute is super slow in selenium.
         *  We should be able to find another way, either using javascript and get all necessary info,
         *  or getting outerHTML and parsing it locally
         */
        private const string GetElementsInformationJavaScript = @"
var list = [];
for(var i = 0; i < arguments.length; ++i) {
    list.push({
        'class': arguments[i].className || null,
        'id': arguments[i].id || null,
        'data-automation-id': arguments[i].getAttribute('data-automation-id') || null,
        'tagName': arguments[i].tagName || null,
        'text': arguments[i].innerText || null,
    });
}
return list;
";

        public static IReadOnlyList<ElementData> GetElementsInformation(this IReadOnlyCollection<IWebElement> webElementCollection)
        {
            if (webElementCollection.Count == 0)
            {
                return new List<ElementData>().AsReadOnly();
            }

            var javaScriptExecutor = webElementCollection.ElementAt(0).GetJavascriptExecutor();

            var result = (IReadOnlyCollection<object>)javaScriptExecutor.ExecuteScript(GetElementsInformationJavaScript, webElementCollection.Cast<object>().ToArray());
            return result.Select(x =>
            {
                var dictionary = x as IDictionary<string, object>;
                return new ElementData
                {
                    Class = dictionary["class"] as string,
                    Id = dictionary["id"] as string,
                    DataAutomationId = dictionary["data-automation-id"] as string,
                    TagName = (dictionary["tagName"] as string).ToLowerInvariant(),
                    Text = dictionary["text"] as string,
                };
            })
            .ToList()
            .AsReadOnly();
        }

        public static bool ExtendedEquals(this IWebElement webElement, in IWebElement otherWebElement)
        {
            try
            {
                var elementsData = GetElementsInformation(new IWebElement[] { webElement, otherWebElement });
                var elementData = elementsData.ElementAt(0);
                var otherElementData = elementsData.ElementAt(1);

                if (!string.IsNullOrWhiteSpace(elementData.DataAutomationId) && elementData.DataAutomationId == otherElementData.DataAutomationId)
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(elementData.Id) && elementData.Id == otherElementData.Id)
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(elementData.Text))
                {
                    return elementData.TagName == otherElementData.TagName
                        && elementData.Text == otherElementData.Text
                        && elementData.Class == otherElementData.Class;
                }

                return elementData.TagName == otherElementData.TagName
                        && elementData.Class == otherElementData.Class;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        }

        public static int ExtendedGetHashCode(this IWebElement webElement)
        {
            var elementData = GetElementsInformation(new IWebElement[] { webElement }).First();
            return elementData.ExtendedGetHashCode();
        }

        public static int ExtendedGetHashCode(this ElementData elementData)
        {
            var hash = 13;

            if (!string.IsNullOrWhiteSpace(elementData.DataAutomationId))
            {
                hash = (hash * 7) + elementData.DataAutomationId.GetHashCode();
                return hash;
            }

            if (!string.IsNullOrWhiteSpace(elementData.Id))
            {
                hash = (hash * 7) + elementData.Id.GetHashCode();
                return hash;
            }

            hash = (hash * 7) + elementData.TagName.GetHashCode();
            hash = (hash * 7) + elementData.Class.GetHashCode();

            if (!string.IsNullOrWhiteSpace(elementData.Text))
            {
                hash = (hash * 7) + elementData.Text.GetHashCode();
            }

            return hash;
        }

        public static string ExtendedToString(this IWebElement webElement)
        {
            var elementData = GetElementsInformation(new IWebElement[] { webElement }).First();
            return elementData.ExtendedToString();
        }

        public static string ExtendedToString(this ElementData elementData)
        {
            var tagName = elementData.TagName;
            var dataAutomationId = elementData.DataAutomationId; // TODO: accept as argument
            if (!string.IsNullOrWhiteSpace(dataAutomationId))
            {
                return $"qs:{tagName}[data-automation-id='{dataAutomationId}']";
            }

            var id = elementData.Id;
            if (!string.IsNullOrWhiteSpace(id))
            {
                return $"qs:{tagName}#{id}";
            }

            var webElementText = elementData.Text;
            var webElementClass = elementData.Class;
            if (!string.IsNullOrWhiteSpace(webElementText))
            {
                return $"xpath://{tagName}[@class='{webElementClass}'][text()={EncodeXPathExpression(webElementText)}]";
            }

            return $"qs:{tagName}.{webElementClass.Trim().Replace(" ", ".")}";
        }

        public static bool CanBeInteracted(this IWebElement webElement)
        {
            try
            {
                /*
                 * Speed optimization: Do not check Displayed here - it can be done cheaper using the script execution.
                 */
                if (!(webElement?.Enabled ?? false))
                {
                    return false;
                }

                var javaScriptExecutor = webElement.GetJavascriptExecutor();
                var remoteWebElement = (RemoteWebElement)webElement;

                // TODO: For non headless runs:
                //var actions = new Actions(remoteWebDriver);
                //actions.MoveToElement(remoteWebElement);
                //actions.Perform();

                var script = $"return document.elementFromPoint({remoteWebElement.Coordinates.LocationInViewport.X + 1}, {remoteWebElement.Coordinates.LocationInViewport.Y + 1});";
                var remoteWebElementFromPoint = (RemoteWebElement)javaScriptExecutor.ExecuteScript(script);

                return remoteWebElementFromPoint != null
                    && ExtendedEquals(remoteWebElement, remoteWebElementFromPoint);
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        }

        public static int ExtendedGetHashCode(this IReadOnlyCollection<IWebElement> webElements)
        {
            var elementsData = GetElementsInformation(webElements);
            var hash = 13;
            foreach (var item in elementsData)
            {
                hash = (hash * 7) + item.ExtendedGetHashCode();
            }
            return hash;
        }

        public static string ExtendedToString(this IReadOnlyCollection<IWebElement> webElements)
        {
            var elementsData = GetElementsInformation(webElements);
            return string.Join(", ", elementsData.Select(x => x.ExtendedToString()));
        }

        public static IJavaScriptExecutor GetJavascriptExecutor(this IWebElement webElement)
        {
            return (IJavaScriptExecutor)((IWrapsDriver)webElement).WrappedDriver;
        }

        private static string EncodeXPathExpression(in string value)
        {
            if (!value.Contains("'"))
            {
                return '\'' + value + '\'';
            }
            else if (!value.Contains("\""))
            {
                return '"' + value + '"';
            }

            return "concat('" + value.Replace("'", "',\"'\",'") + "')";
        }
    }
}
