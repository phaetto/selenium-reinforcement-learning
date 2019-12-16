namespace Selenium.Algorithms
{
    public sealed class ElementDataQuery
    {
        public QueryType QueryType { get; set; }
        public string Query { get; set; }
    }

    public enum QueryType
    {
        CssSelector,
        XPath
    }
}
