namespace Selenium.Algorithms
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ElementDataExtensions
    {

        public static int ExtendedGetHashCode(this ElementData elementData)
        {
            var hash = 13;

            if (!string.IsNullOrWhiteSpace(elementData.ExtraState))
            {
                hash = (hash * 7) + elementData.ExtraState.GetHashCode();
            }

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

        public static string ExtendedToString(this ElementData elementData)
        {
            var tagName = elementData.TagName;
            var dataAutomationId = elementData.DataAutomationId;
            if (!string.IsNullOrWhiteSpace(dataAutomationId))
            {
                return $"{tagName}[data-automation-id='{dataAutomationId}'], state: {elementData.ExtraState}";
            }

            var id = elementData.Id;
            if (!string.IsNullOrWhiteSpace(id))
            {
                return $"{tagName}#{id}, state: {elementData.ExtraState}";
            }

            var webElementText = elementData.Text;
            var webElementClass = elementData.Class;
            if (!string.IsNullOrWhiteSpace(webElementText))
            {
                return $"//{tagName}[@class='{webElementClass}'][text()={EncodeXPathExpression(webElementText)}], state: {elementData.ExtraState}";
            }

            return $"{tagName}.{webElementClass.Trim().Replace(" ", ".")}, state: {elementData.ExtraState}";
        }

        public static ElementDataQuery GetQuery(this ElementData elementData)
        {
            var tagName = elementData.TagName;
            var dataAutomationId = elementData.DataAutomationId;
            if (!string.IsNullOrWhiteSpace(dataAutomationId))
            {
                return new ElementDataQuery(QueryType.CssSelector, $"{tagName}[data-automation-id='{dataAutomationId}']");
            }

            var id = elementData.Id;
            if (!string.IsNullOrWhiteSpace(id))
            {
                return new ElementDataQuery(QueryType.CssSelector, $"{tagName}#{id}");
            }

            var webElementText = elementData.Text;
            var webElementClass = elementData.Class;
            if (!string.IsNullOrWhiteSpace(webElementText))
            {
                return new ElementDataQuery(QueryType.XPath, $"//{tagName}[@class='{webElementClass}'][text()={EncodeXPathExpression(webElementText)}]");
            }

            return new ElementDataQuery(QueryType.CssSelector, $"{tagName}.{webElementClass.Trim().Replace(" ", ".")}");
        }

        public static int ExtendedGetHashCode(this IReadOnlyCollection<ElementData> elementsData)
        {
            var hash = 13;
            foreach (var item in elementsData)
            {
                hash = (hash * 7) + item.ExtendedGetHashCode();
            }
            return hash;
        }

        public static string ExtendedToString(this IReadOnlyCollection<ElementData> elementsData)
        {
            var filteredElementDataAsString = elementsData
                .Select(x =>
                {
                    return x.ExtendedToString();
                })
                .Where(x => x != null);

            return string.Join(", ", filteredElementDataAsString);
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
