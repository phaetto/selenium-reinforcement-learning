namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Remote;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static class WebElementExtensions
    {
        /*
         * GetAttribute is super slow in selenium.
         *  We are using JavaScript to get the necessary information for elements.
         */
        private const string GetElementsInformationJavaScript = @"
var list = [];
for(var i = 0; i < arguments.length; ++i) {
    var isTypingElement = (arguments[i].tagName.toLowerCase() === 'input' && arguments[i].getAttribute('type').toLowerCase() === 'text')
        || (arguments[i].tagName.toLowerCase() === 'textarea');
    
    list.push({
        'class': arguments[i].className || '',
        'id': arguments[i].id || '',
        'data-automation-id': arguments[i].getAttribute('data-automation-id') || '',
        'data-automation-actions': arguments[i].getAttribute('data-automation-actions') || [],
        'name': arguments[i].getAttribute('name') || '',
        'tagName': arguments[i].tagName.toLowerCase() || '',
        'text': arguments[i].innerText || '',
        'isTypingElement': isTypingElement,
        'extraState': isTypingElement ? arguments[i].value : '',
    });
}
return list;
";

        private const string GetElementsInteractionDataJavaScript = @"
var list = [];
for(var i = 0; i < arguments.length; ++i) {
    var rect = arguments[i].getBoundingClientRect();

    var isInViewPort = (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.top <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.left <= (window.innerWidth || document.documentElement.clientWidth)
    );
    
    list.push({
        'isInViewPort': isInViewPort,
        'top': rect.top,
        'left': rect.left,
        'width': rect.width,
        'height': rect.height,
        'isEnabled': !arguments[i].disabled,
    });
}
return list;
";

        private const string IsElementInCoordinatesChildOfElementJavaScript = @"
var coordX = arguments[0];
var coordY = arguments[1];
var element = arguments[2];

var elementAtCoordinates = document.elementFromPoint(coordX, coordY);

var checkForElement = elementAtCoordinates;
while(checkForElement && !checkForElement.isSameNode(element)) {
    checkForElement = checkForElement.parentNode;

    if (checkForElement === document.body) {
        return false;
    }
}

return !!checkForElement;
";

        public static IReadOnlyList<ElementData> GetElementsInformation(this IReadOnlyCollection<IWebElement> webElementCollection)
        {
            if (webElementCollection.Count == 0)
            {
                return new List<ElementData>().AsReadOnly();
            }

            var javaScriptExecutor = webElementCollection.ElementAt(0).GetJavascriptExecutor();

            var result = (IReadOnlyCollection<object>)javaScriptExecutor.ExecuteScript(GetElementsInformationJavaScript, webElementCollection.Cast<object>().ToArray());
            return result.Select((x, index) =>
            {
                var dictionary = (IDictionary<string, object>)x;

                return new ElementData(
                    Convert.ToString(dictionary["class"]),
                    Convert.ToString(dictionary["id"]),
                    Convert.ToString(dictionary["data-automation-id"]),
                    ParseAutomationActions(Convert.ToString(dictionary["data-automation-actions"])),
                    Convert.ToString(dictionary["tagName"]),
                    Convert.ToString(dictionary["text"]),
                    Convert.ToString(dictionary["name"]),
                    Convert.ToBoolean(dictionary["isTypingElement"]),
                    Convert.ToString(dictionary["extraState"]),
                    webElementCollection.ElementAt(index)
                );
            })
            .ToList()
            .AsReadOnly();
        }

        public static IReadOnlyList<ElementInteractionData> GetElementsInteractionData(this IReadOnlyCollection<IWebElement> webElementCollection)
        {
            if (webElementCollection.Count == 0)
            {
                return new List<ElementInteractionData>().AsReadOnly();
            }

            var javaScriptExecutor = webElementCollection.ElementAt(0).GetJavascriptExecutor();

            var result = (IReadOnlyCollection<object>)javaScriptExecutor.ExecuteScript(GetElementsInteractionDataJavaScript, webElementCollection.Cast<object>().ToArray());
            return result.Select((x, index) =>
            {
                var dictionary = (IDictionary<string, object>)x;

                return new ElementInteractionData(
                    Convert.ToBoolean(dictionary["isInViewPort"]),
                    Convert.ToInt32(dictionary["top"]),
                    Convert.ToInt32(dictionary["left"]),
                    Convert.ToInt32(dictionary["width"]),
                    Convert.ToInt32(dictionary["height"]),
                    Convert.ToBoolean(dictionary["isEnabled"]),
                    webElementCollection.ElementAt(index)
                );
            })
            .ToList()
            .AsReadOnly();
        }

        public static IReadOnlyCollection<IWebElement> ToInteractibleElements(this IReadOnlyCollection<IWebElement> webElementCollection)
        {
            return webElementCollection
                .Where(x =>
                {
                    try
                    {
                        return x.CanBeInteracted();
                    }
                    catch (StaleElementReferenceException)
                    {
                        return false;
                    }
                })
                .ToList()
                .AsReadOnly();
        }

        public static bool IsAnyInteractibleElement(this IReadOnlyCollection<IWebElement> webElementCollection)
        {
            return webElementCollection
                .Any(x =>
                {
                    try
                    {
                        return x.CanBeInteracted();
                    }
                    catch (StaleElementReferenceException)
                    {
                        return false;
                    }
                });
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

        public static string ExtendedToString(this IWebElement webElement)
        {
            var elementData = GetElementsInformation(new IWebElement[] { webElement }).First();
            return elementData.ExtendedToString();
        }

        public static bool CanBeInteracted(this IWebElement webElement)
        {
            try
            {
                // webElement.Enabled is slow and we use JS to find it out
                if (!(webElement?.Displayed ?? false))
                {
                    return false;
                }

                var elementPositionalData = GetElementsInteractionData(new IWebElement[] { webElement }).First();

                if (!elementPositionalData.IsEnabled)
                {
                    return false;
                }
                    
                var javaScriptExecutor = webElement.GetJavascriptExecutor();
                var webDriver = (IWebDriver)javaScriptExecutor;
                var windowSize = webDriver.Manage().Window.Size;

                if (!elementPositionalData.IsInViewPort)
                {
                    var actions = new Actions(javaScriptExecutor as IWebDriver);
                    actions.MoveToElement(webElement);
                    try
                    {
                        actions.Perform();
                        // Recalculate the position
                        elementPositionalData = GetElementsInteractionData(new IWebElement[] { webElement }).First();
                    }
                    catch (Exception) // Should that be a specific exception type?
                    {
                        return false;
                    }
                }

                if (elementPositionalData.Width == 0 || elementPositionalData.Height == 0)
                {
                    return false;
                }

                Debug.Assert(elementPositionalData.X >= 0, "Element outside of viewport: Left");
                Debug.Assert(elementPositionalData.Y >= 0, "Element outside of viewport: Top");
                Debug.Assert(elementPositionalData.X <= windowSize.Width, "Element outside of viewport: Right");
                Debug.Assert(elementPositionalData.Y <= windowSize.Height, "Element outside of viewport: Bottom");
                Debug.Assert(elementPositionalData.Width > 0, "Element does not have width");
                Debug.Assert(elementPositionalData.Height > 0, "Element does not have height");

                return (bool)javaScriptExecutor.ExecuteScript(
                    IsElementInCoordinatesChildOfElementJavaScript,
                    elementPositionalData.X + 1,
                    elementPositionalData.Y + 1,
                    webElement);
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        }

        public static IJavaScriptExecutor GetJavascriptExecutor(this IWebElement webElement)
        {
            return (IJavaScriptExecutor)((IWrapsDriver)webElement).WrappedDriver;
        }

        public static IReadOnlyCollection<string> ParseAutomationActions(string actions)
        {
            return actions?.Split(' ')
                ?.Select(x => x.Trim())
                ?.Where(x => !string.IsNullOrWhiteSpace(x))
                ?.ToList()
                ?.AsReadOnly()
                ?? new List<string>().AsReadOnly();
        }
    }
}
