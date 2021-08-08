using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class PersonalContactsApiTests
    {
        private readonly GraphServiceClient graphClient;

        public PersonalContactsApiTests()
        {
            string token = Utils.GetTestToken().Result;
            graphClient = DefaultGraphApi.GetGraphServiceClient(token);
        }

        [TestMethod]
        public async Task TestCallCreateContactAsync()
        {
            PersonalContactsApi personalContactsApi = new PersonalContactsApi(graphClient);
            bool result = await personalContactsApi.CallCreateContactAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallUpdateContactAsync()
        {
            PersonalContactsApi personalContactsApi = new PersonalContactsApi(graphClient);
            bool result = await personalContactsApi.CallUpdateContactAsync();
            Assert.IsTrue(result);
        }
    }
}
