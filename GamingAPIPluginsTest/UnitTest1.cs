using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Oxide.Plugins;
using System.Reflection;
using System.Collections.Generic;
using ConVar;
using Oxide.Ext.GamingApi.MessageQueue;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            GamingAPI plugin = new GamingAPI();
            MethodInfo methodInfo = typeof(GamingAPI).GetMethod("OnPlayerChat", BindingFlags.NonPublic | BindingFlags.Instance);
            var p = new BasePlayer();
            p.UserIDString = "test";
            p.displayName = "test"; 
            object[] parameters = { p, "test", Chat.ChatChannel.Global };
            methodInfo.Invoke(plugin, parameters);
            Assert.AreEqual(GamingApiMessageQueue.Instance.GetCurrentMessagesInQueue(), 1);
        }
    }
}
