namespace Selenium.ReinforcementLearning.Framework
{
    using System.Threading.Tasks;

    public interface IPersistenceIO
    {
        public Task<bool> Exists(string testIdentifier);
        public Task<string> Read(string testIdentifier);
        public Task Write(string testIdentifier, string serializedState);
    }
}
