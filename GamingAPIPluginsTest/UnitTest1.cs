using Asyncapi.Nats.Client.Models;
using NUnit.Framework;
using System.Text.Json;

namespace GamingAPIPluginTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            ServerStarted temp = new ServerStarted();
            temp.Timestamp = "Test";
            string json = JsonSerializer.Serialize(temp);
            ServerStarted output = JsonSerializer.Deserialize<ServerStarted>(json);
            string json2 = JsonSerializer.Serialize(output);
            Assert.AreEqual(json, json2);
        }
    }
}