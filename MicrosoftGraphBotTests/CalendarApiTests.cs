using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class CalendarApiTests
    {
        private readonly IGraphServiceClient graphClient;

        public CalendarApiTests()
        {
            string token = Utils.GetTestToken().Result;
            graphClient = DefaultGraphApi.GetGraphServiceClient(token);
        }

        [TestMethod]
        public async Task TestCallCreateCalendarAsync()
        {
            CalendarApi calendarApi = new CalendarApi(graphClient);
            bool result = await calendarApi.CallCreateCalendarAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallUpdateCalendarAsync()
        {
            CalendarApi calendarApi = new CalendarApi(graphClient);
            bool result = await calendarApi.CallUpdateCalendarAsync();
            Assert.IsTrue(result);
        }
    }
}
