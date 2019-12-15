using OpenQA.Selenium;

namespace Selenium.Algorithms
{
    public class ElementData
    {
        public string Class { get; set; }
        public string Id { get; set; }
        public string DataAutomationId { get; set; }
        public string TagName { get; set; }
        public string Text { get; set; }
        public IWebElement WebElementReference { get; set; }
    }
}
