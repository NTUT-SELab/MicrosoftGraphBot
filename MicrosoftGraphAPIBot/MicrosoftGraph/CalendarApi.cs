using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    /// <summary>
    /// Calendar Api 腳本
    /// </summary>
    public class CalendarApi : GraphApi
    {
        public const string Scope = "Calendars.Read Calendars.ReadWrite";

        public CalendarApi(IGraphServiceClient graphClient) : base(graphClient)
        {
        }

        public CalendarApi(ILogger<CalendarApi> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        /// <summary>
        /// 新增日曆流程
        /// 
        /// 包含: 列出日曆API、新增日曆API、刪除日曆API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallCreateCalendarAsync()
        {
            try
            {
                Calendar calendar = await CreateCalendarAsync(graphClient);

                IUserCalendarsCollectionPage calendars = await ListCalendarAsync(graphClient);
                bool isCreate = calendars.CurrentPage.Any(item => item.Id == calendar.Id);
                Trace.Assert(isCreate);

                await DeleteCalendarAsync(graphClient, calendar.Id);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 新增日曆流程
        /// 
        /// 包含: 列出日曆API、新增日曆API、刪除日曆API、更新日曆API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallUpdateCalendarAsync()
        {
            try
            {
                Calendar calendar = await CreateCalendarAsync(graphClient);

                IUserCalendarsCollectionPage calendars = await ListCalendarAsync(graphClient);
                bool isCreate = calendars.CurrentPage.Any(item => item.Id == calendar.Id);
                Trace.Assert(isCreate);

                Calendar newCalendar = await UpdateCalendarAsync(graphClient, calendar.Id);
                calendars = await ListCalendarAsync(graphClient);
                bool isUpdate = calendars.CurrentPage.Any(item => item.Id == calendar.Id && item.Name == newCalendar.Name);
                Trace.Assert(isUpdate);

                await DeleteCalendarAsync(graphClient, calendar.Id);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 列出日曆 Api
        /// </summary>
        /// <param name="graphClient"></param>
        /// <returns></returns>
        private static async Task<IUserCalendarsCollectionPage> ListCalendarAsync(IGraphServiceClient graphClient)
        {
            return await graphClient.Me.Calendars
                .Request()
                .GetAsync();
        }

        /// <summary>
        /// 新增日曆 Api
        /// </summary>
        /// <param name="graphClient"></param>
        /// <returns></returns>
        private static async Task<Calendar> CreateCalendarAsync(IGraphServiceClient graphClient)
        {
            var calendar = await graphClient.Me.Calendars
                                            .Request()
                                            .AddAsync(new Calendar { Name = Guid.NewGuid().ToString() });

            await Task.Delay(5000);
            return calendar;
        }

        /// <summary>
        /// 刪除日曆 Api
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        private static async Task DeleteCalendarAsync(IGraphServiceClient graphClient, string Id)
        {
            await graphClient.Me.Calendars[Id]
                            .Request()
                            .DeleteAsync();
        }

        /// <summary>
        /// 更新日曆 Api
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        private static async Task<Calendar> UpdateCalendarAsync(IGraphServiceClient graphClient, string Id)
        {
            var calendar = await graphClient.Me.Calendars[Id]
                                .Request()
                                .UpdateAsync(new Calendar { Name = Guid.NewGuid().ToString() });

            await Task.Delay(5000);
            return calendar;
        }
    }
}
