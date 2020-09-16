using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class PermissionsApiTests
    {
        private readonly IGraphServiceClient graphClient;

        public PermissionsApiTests()
        {
            string token = Utils.GetTestToken().Result;
            graphClient = DefaultGraphApi.GetGraphServiceClient(token);
        }

        [TestMethod]
        public async Task TestCallCreateShareLinkAsync()
        {
            PermissionsApi permissionsApi = new PermissionsApi(graphClient);
            bool result = await permissionsApi.CallCreateShareLinkAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallAccessingShareLinkAsync()
        {
            PermissionsApi permissionsApi = new PermissionsApi(graphClient);
            bool result = await permissionsApi.CallAccessingShareLinkAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallGetSharingLinkAsync()
        {
            PermissionsApi permissionsApi = new PermissionsApi(graphClient);
            bool result = await permissionsApi.CallGetSharingLinkAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallUpdateSharingLinkAsync()
        {
            PermissionsApi permissionsApi = new PermissionsApi(graphClient);
            bool result = await permissionsApi.CallUpdateSharingLinkAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallDeleteSharingLinkAsync()
        {
            PermissionsApi permissionsApi = new PermissionsApi(graphClient);
            bool result = await permissionsApi.CallDeleteSharingLinkAsync();
            Assert.IsTrue(result);
        }
    }
}
