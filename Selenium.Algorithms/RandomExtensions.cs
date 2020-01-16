namespace Selenium.Algorithms
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class RandomExtensions
    {
        public static RandomState Save(this Random random)
        {
            var binaryFormatter = new BinaryFormatter();
            using var temp = new MemoryStream();
            binaryFormatter.Serialize(temp, random);
            return new RandomState(temp.ToArray());
        }

        public static Random Restore(this RandomState state)
        {
            var binaryFormatter = new BinaryFormatter();
            using var temp = new MemoryStream(state.State);
            return (Random)binaryFormatter.Deserialize(temp);
        }
    }

    public struct RandomState
    {
        public readonly byte[] State;
        public RandomState(byte[] state)
        {
            State = state;
        }
    }
}
