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
        'class': arguments[i].className || null,
        'id': arguments[i].id || null,
        'data-automation-id': arguments[i].getAttribute('data-automation-id') || null,
        'data-automation-actions': arguments[i].getAttribute('data-automation-actions') || null,
        'name': arguments[i].getAttribute('name') || null,
        'tagName': arguments[i].tagName.toLowerCase() || null,
        'text': arguments[i].innerText || null,
        'isTypingElement': isTypingElement,
        'extraState': isTypingElement ? arguments[i].value : null,
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
while(!checkForElement.isSameNode(element)) {
    checkForElement = checkForElement.parentNode;

    if (checkForElement === document.body) {
        return false;
    }
}

return true;
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
                var dictionary = x as IDictionary<string, object>;
                return new ElementData
                {
                    Class = dictionary["class"] as string,
                    Id = dictionary["id"] as string,
                    DataAutomationId = dictionary["data-automation-id"] as string,
                    DataAutomationActions = ParseAutomationActions(dictionary["data-automation-actions"] as string),
                    TagName = dictionary["tagName"] as string,
                    Text = dictionary["text"] as string,
                    Name = dictionary["name"] as string,
                    IsTypingElement = Convert.ToBoolean(dictionary["isTypingElement"]),
                    ExtraState = dictionary["extraState"] as string,
                    WebElementReference = webElementCollection.ElementAt(index),
                };
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
                var dictionary = x as IDictionary<string, object>;
                return new ElementInteractionData
                {
                    IsInViewPort = Convert.ToBoolean(dictionary["isInViewPort"]),
                    Y = Convert.ToInt32(dictionary["top"]),
                    X = Convert.ToInt32(dictionary["left"]),
                    Width = Convert.ToInt32(dictionary["width"]),
                    Height = Convert.ToInt32(dictionary["height"]),
                    IsEnabled = Convert.ToBoolean(dictionary["isEnabled"]),
                    WebElementReference = webElementCollection.ElementAt(index),
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
                var remoteWebElement = (RemoteWebElement)webElement;
                var webDriver = remoteWebElement.WrappedDriver;
                var windowSize = webDriver.Manage().Window.Size;

                if (!elementPositionalData.IsInViewPort)
                {
                    var actions = new Actions(javaScriptExecutor as IWebDriver);
                    actions.MoveToElement(remoteWebElement);
                    try
                    {
                        actions.Perform();
                        // Recalculate the position
                        elementPositionalData = GetElementsInteractionData(new IWebElement[] { webElement }).First();
                    }
                    catch (Exception) // Should that be a specific excpetion type?
                    {
                        return false;
                    }
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
                    remoteWebElement);
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
