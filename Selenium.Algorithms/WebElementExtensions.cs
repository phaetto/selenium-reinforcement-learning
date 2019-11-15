using OpenQA.Selenium;
using System;

namespace Selenium.Algorithms
{
    public static class WebElementExtensions
    {
        public static bool ExtendedEquals(this IWebElement webElement, IWebElement otherWebElement)
        {
            return webElement.Displayed == otherWebElement.Displayed
                && webElement.Enabled == otherWebElement.Enabled
                && webElement.Selected == otherWebElement.Selected
                && webElement.TagName == otherWebElement.TagName
                && webElement.Text == otherWebElement.Text
                && webElement.GetAttribute("innerHTML") == otherWebElement.GetAttribute("innerHTML");
        }

        public static int ExtendedGetHashCode(this IWebElement webElement)
        {
            var hash = 13;
            hash = (hash * 7) + webElement.Displayed.GetHashCode();
            hash = (hash * 7) + webElement.Enabled.GetHashCode();
            hash = (hash * 7) + webElement.Selected.GetHashCode();
            hash = (hash * 7) + webElement.TagName.GetHashCode();
            hash = (hash * 7) + webElement.Text.GetHashCode();
            hash = (hash * 7) + webElement.GetAttribute("innerHTML").GetHashCode();
            return hash;
        }

        public static string ExtendedToString(this IWebElement webElement)
        {
            var testIdString = webElement.GetAttribute("data-test-id");
            if (!string.IsNullOrWhiteSpace(testIdString))
            {
                return $"{webElement.TagName}[data-test-id='{testIdString}']";
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
                return $"{webElement.TagName}.{classString}{nameStringValue}{typeStringValue}";
            }

            throw new NotSupportedException("Element that is actionable does not have a unique id. Use 'data-test-id' attribute (best practice), id or unique class name.");
        }
    }
}
