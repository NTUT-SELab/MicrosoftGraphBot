using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    /// <summary>
    /// 控制每次腳本執行服務的數量
    /// </summary>
    public class ApiController
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly GraphApi[] graphApis;
        private IGraphServiceClient graphClient;
        private string authName;

        public ApiController(ILogger<ApiController> logger, IConfiguration configuration, IEnumerable<GraphApi> graphApis) => 
            (this.logger, this.configuration, this.graphApis) = (logger, configuration, graphApis.ToArray());

        /// <summary>
        /// 執行 api 服務
        /// 
        /// 如: Outlook api ... 等
        /// </summary>
        /// <param name="graphClient"> The object of GraphServiceClient. </param>
        /// <param name="authName"> token 別名 </param>
        /// <returns> (api service class name, method name, result) 的集合 </returns>
        public async Task<IReadOnlyList<(string, string, bool)>> RunAsync(IGraphServiceClient graphClient, string authName)
        {
            this.graphClient = graphClient;
            this.authName = authName;
            logger.LogInformation($"{authName} start.");

            int numberOfServiceCall = 0;
            if (configuration != null && configuration["API:NumberOfServiceCall"] != null)
                numberOfServiceCall = int.Parse(configuration["API:NumberOfServiceCall"]);

            List<(string, string, bool)> results = new List<(string, string, bool)>();
            if (numberOfServiceCall == 0 || graphApis.Length < numberOfServiceCall)
                foreach (GraphApi api in graphApis)
                    results.AddRange(await CallApi(api));
            else
            {
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                foreach (GraphApi api in graphApis.OrderBy(item => rnd.Next()).Take(numberOfServiceCall))
                    results.AddRange(await CallApi(api));
            }

            return results;
        }

        /// <summary>
        /// 開始執行 Microsoft Graph 服務
        /// </summary>
        /// <param name="api"> Microsoft Graph 服務 </param>
        /// <returns> call api result message </returns>
        private async Task<IEnumerable<(string, string, bool)>> CallApi(GraphApi api)
        {
            IReadOnlyList<(string, bool)> results = await api.RunAsync(graphClient).ToListAsync();
            foreach (var result in results)
            {
                string message = $"{authName}~{api.GetType().Name}~{result.Item1}: {result.Item2}";
                if (result.Item2)
                    logger.LogInformation(message);
                else
                    logger.LogError(message);
            }
            return results.Select(result => (api.GetType().Name, result.Item1, result.Item2));
        }
    }

    /// <summary>
    /// GraphApi 腳本抽象類別
    /// </summary>
    public abstract class GraphApi
    {
        protected IGraphServiceClient graphClient;
        protected readonly ILogger logger;
        private readonly IConfiguration configuration;

        protected GraphApi(IGraphServiceClient graphClient) => this.graphClient = graphClient;

        protected GraphApi(ILogger logger, IConfiguration configuration) => 
            (this.logger, this.configuration) = (logger, configuration);

        /// <summary>
        /// 執行 Api 腳本
        /// </summary>
        /// <param name="graphClient"> The object of GraphServiceClient. </param>
        /// <returns> (method name, result) 的集合 </returns>
        public async IAsyncEnumerable<(string, bool)> RunAsync(IGraphServiceClient graphClient)
        {
            this.graphClient = graphClient;

            int numberOfMethodCall = 0;

            if (configuration != null && configuration["API:NumberOfMethodCall"] != null)
                numberOfMethodCall = int.Parse(configuration["API:NumberOfMethodCall"]);

            IEnumerable<MethodInfo> methodInfos = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(method => method.Name.Contains("Call"));

            if (numberOfMethodCall == 0 || methodInfos.Count() < numberOfMethodCall)
                foreach (MethodInfo method in methodInfos)
                    yield return await CallApi(method);
            else
            {
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                foreach (MethodInfo method in methodInfos.OrderBy(item => rnd.Next()).Take(numberOfMethodCall))
                    yield return await CallApi(method);
            }
        }

        private async Task<(string, bool)> CallApi(MethodInfo method)
        {
            Task<bool> result = (Task<bool>)method.Invoke(this, null);
            return (method.Name, await result);
        }
    }
}
