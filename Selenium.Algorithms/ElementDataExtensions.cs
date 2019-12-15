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
                return $"qs:{tagName}[data-automation-id='{dataAutomationId}'], State: {elementData.ExtraState}";
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
                return $"xpath://{tagName}[@class='{webElementClass}'][text()={EncodeXPathExpression(webElementText)}], State: {elementData.ExtraState}";
            }

            return $"qs:{tagName}.{webElementClass.Trim().Replace(" ", ".")}, State: {elementData.ExtraState}";
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
