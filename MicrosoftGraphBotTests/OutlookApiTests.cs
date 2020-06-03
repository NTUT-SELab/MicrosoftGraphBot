using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class OutlookApiTests
    {
        private IGraphServiceClient graphClient;

        public OutlookApiTests()
        {
            string token = Utils.GetTestToken().Result;
            graphClient = DefaultGraphApi.GetGraphServiceClient(token);
        }

        [TestMethod]
        public async Task TestCallCreateMessageAsync()
        {
            bool result = await OutlookApi.CallCreateMessageAsync(graphClient);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallUpdateMessage()
        {
            bool result = await OutlookApi.CallUpdateMessageAsync(graphClient);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallSendMessage()
        {
            bool result = await OutlookApi.CallSendMessageAsync(graphClient);
            Assert.IsTrue(result);
        }
    }
}
