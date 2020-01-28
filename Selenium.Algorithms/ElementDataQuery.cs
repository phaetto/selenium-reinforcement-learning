namespace Selenium.Algorithms
{
    public readonly struct ElementDataQuery
    {
        public QueryType QueryType { get; }
        public string Query { get; }

        public ElementDataQuery(
            QueryType queryType,
            string query
        )
        {
            QueryType = queryType;
            Query = query;
        }
    }

    public enum QueryType
    {
        CssSelector,
        XPath
    }
}
