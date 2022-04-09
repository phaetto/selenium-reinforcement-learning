namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests.Framework
{
    using System.IO;
    using System.Threading.Tasks;

    public sealed class FileIO : IPersistenceIO
    {
        public Task<bool> Exists(string testIdentifier)
        {
            return Task.FromResult(File.Exists($"{testIdentifier}.trained.json"));
        }

        public async Task<string> Read(string testIdentifier)
        {
            return await File.ReadAllTextAsync($"{testIdentifier}.trained.json");
        }

        public async Task Write(string testIdentifier, string serializedState)
        {
            await File.WriteAllTextAsync($"{testIdentifier}.trained.json", serializedState);
        }
    }
}
