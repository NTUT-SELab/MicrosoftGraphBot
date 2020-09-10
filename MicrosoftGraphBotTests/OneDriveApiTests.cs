using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class OneDriveApiTests
    {
        private readonly IGraphServiceClient graphClient;

        public OneDriveApiTests()
        {
            string token = Utils.GetTestToken().Result;
            graphClient = DefaultGraphApi.GetGraphServiceClient(token);
        }

        [TestMethod]
        public async Task TestCallCreateFolderAsync()
        {
            OneDriveApi oneDriveApi = new OneDriveApi(graphClient);
            bool result = await oneDriveApi.CallCreateFolderAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallUploadFileAsync()
        {
            OneDriveApi oneDriveApi = new OneDriveApi(graphClient);
            bool result = await oneDriveApi.CallUpdateDriveItemAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallMoveDriveItemAsync()
        {
            OneDriveApi oneDriveApi = new OneDriveApi(graphClient);
            bool result = await oneDriveApi.CallMoveDriveItemAsync();
            Assert.IsTrue(result);
        }
    }
}
