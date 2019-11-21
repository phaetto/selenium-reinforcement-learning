namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Remote;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class WebElementExtensions
    {
        public static bool ExtendedEquals(this IWebElement webElement, IWebElement otherWebElement)
        {
            try
            {
                return webElement.Displayed == otherWebElement.Displayed
                    && webElement.Enabled == otherWebElement.Enabled
                    && webElement.Selected == otherWebElement.Selected
                    && webElement.TagName == otherWebElement.TagName
                    && webElement.Text == otherWebElement.Text
                    //&& webElement.GetAttribute("innerHTML") == otherWebElement.GetAttribute("innerHTML")
                    ;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        }

        public static int ExtendedGetHashCode(this IWebElement webElement)
        {
            try
            {
                var hash = 13;
                hash = (hash * 7) + webElement.Displayed.GetHashCode();
                hash = (hash * 7) + webElement.Enabled.GetHashCode();
                hash = (hash * 7) + webElement.Selected.GetHashCode();
                hash = (hash * 7) + webElement.TagName.GetHashCode();
                hash = (hash * 7) + webElement.Text.GetHashCode();
                //hash = (hash * 7) + webElement.GetAttribute("innerHTML").GetHashCode();
                return hash;
            }
            catch (StaleElementReferenceException)
            {
                return 0;
            }
        }

        public static string ExtendedToString(this IWebElement webElement)
        {
            var testIdString = webElement.GetAttribute("data-automation-id"); // TODO: accept as argument
            if (!string.IsNullOrWhiteSpace(testIdString))
            {
                return $"{webElement.TagName}[data-automation-id='{testIdString}']";
            }

            var idString = webElement.GetAttribute("id");
            if (!string.IsNullOrWhiteSpace(idString))
            {
                return $"{webElement.TagName}#{idString}";
            }

            var nameString = webElement.GetAttribute("name");
            var nameStringValue = string.Empty;
            if (!string.IsNullOrWhiteSpace(nameString))
            {
                nameStringValue = $"[name='{nameString}']";
            }

            var typeString = webElement.GetAttribute("name");
            var typeStringValue = string.Empty;
            if (!string.IsNullOrWhiteSpace(typeString))
            {
                typeStringValue = $"[name='{typeString}']";
            }

            var classString = webElement.GetAttribute("class");
            if (!string.IsNullOrWhiteSpace(classString))
            {
                classString = classString.Trim().Replace(" ", ".");
                return $"{webElement.TagName}.{classString}{nameStringValue}{typeStringValue}";
            }

            throw new NotSupportedException("Element that is actionable does not have a unique id. Use 'data-test-id' attribute (best practice), id or unique class name.");
        }

        public static bool CanBeInteracted(this IWebElement webElement, in IWebDriver webDriver)
        {
            if (!((webElement?.Displayed ?? false)
                && (webElement?.Enabled ?? false)))
            {
                return false;
            }

            var remoteWebDriver = webDriver as RemoteWebDriver;
            if (remoteWebDriver == null)
            {
                return true;
            }

            var remoteWebElement = (RemoteWebElement)webElement;
            var remoteWebElementFromPoint = (RemoteWebElement)remoteWebDriver.ExecuteScript($"return document.elementFromPoint({remoteWebElement.Coordinates.LocationInViewport.X + 1}, {remoteWebElement.Coordinates.LocationInViewport.Y + 1})");

            return remoteWebElementFromPoint != null
                && ExtendedEquals(remoteWebElement, remoteWebElementFromPoint);
        }

        public static int ExtendedGetHashCode(this IReadOnlyCollection<IWebElement> webElements)
        {
            var hash = 13;
            foreach (var item in webElements)
            {
                hash = (hash * 7) + item.ExtendedGetHashCode();
            }
            return hash;
        }

        public static string ExtendedToString(this IReadOnlyCollection<IWebElement> webElements)
        {
            return string.Join(", ", webElements.Select(x => x.ExtendedToString()));
        }
    }
}
