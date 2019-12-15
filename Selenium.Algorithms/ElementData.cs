using OpenQA.Selenium;
using System.Collections.Generic;

namespace Selenium.Algorithms
{
    public class ElementData
    {
        public string Class { get; set; }
        public string Id { get; set; }
        public string DataAutomationId { get; set; }
        public IReadOnlyCollection<string> DataAutomationActions { get; set; }
        public string TagName { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }
        public bool IsTypingElement { get; set; }
        public string ExtraState { get; set; }
        public IWebElement WebElementReference { get; set; }
    }
}
