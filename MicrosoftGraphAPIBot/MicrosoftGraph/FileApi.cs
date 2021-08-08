using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    public static class FileApi
    {
        /// <summary>
        /// 列出OneDrive所有內容 API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <returns></returns>
        public static async Task<IDriveItemChildrenCollectionPage> ListDriveItemAsync(GraphServiceClient graphClient)
        {
            IDriveItemChildrenCollectionPage children = await graphClient.Me.Drive.Root.Children
                .Request()
                .GetAsync();

            return children;
        }

        /// <summary>
        /// 列出指定目錄內容 API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static async Task<IDriveItemChildrenCollectionPage> GetDriveItemAsync(GraphServiceClient graphClient, string itemId)
        {
            return await graphClient.Me.Drive.Items[itemId].Children
                        .Request()
                        .GetAsync();
        }

        /// <summary>
        /// 新增資料夾 API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <returns></returns>
        public static async Task<DriveItem> CreateFolderAsync(GraphServiceClient graphClient)
        {
            var driveItem = new DriveItem
            {
                Name = Guid.NewGuid().ToString(),
                Folder = new Folder
                {
                },
                AdditionalData = new Dictionary<string, object>()
                {
                    {"@microsoft.graph.conflictBehavior", "rename"}
                }
            };

            DriveItem item = await graphClient.Me.Drive.Root.Children
                                .Request()
                                .AddAsync(driveItem);

            await Task.Delay(5000);
            return item;
        }

        /// <summary>
        /// 刪除資料夾 API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static async Task DeleteDriveItemAsync(GraphServiceClient graphClient, string itemId)
        {
            await graphClient.Me.Drive.Items[itemId]
                .Request()
                .DeleteAsync();
        }

        /// <summary>
        /// 更新指定項目內容 API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static async Task<DriveItem> UpdateDriveItemAsync(GraphServiceClient graphClient, string itemId)
        {
            var driveItem = new DriveItem
            {
                Name = Guid.NewGuid().ToString()
            };

            DriveItem item = await graphClient.Me.Drive.Items[itemId]
                                .Request()
                                .UpdateAsync(driveItem);

            await Task.Delay(5000);
            return item;
        }

        /// <summary>
        /// 移動指定項目 API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="parentItemId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static async Task MoveDriveItemAsync(GraphServiceClient graphClient, string parentItemId, string itemId)
        {
            var driveItem = new DriveItem
            {
                ParentReference = new ItemReference
                {
                    Id = parentItemId
                },
            };

            await graphClient.Me.Drive.Items[itemId]
                .Request()
                .UpdateAsync(driveItem);

            await Task.Delay(5000);
        }
    }
}
