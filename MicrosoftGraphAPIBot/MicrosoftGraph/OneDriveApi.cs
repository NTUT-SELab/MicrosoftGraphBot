using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    public class OneDriveApi : GraphApi
    {
        public const string Scope = "Files.Read Files.ReadWrite Files.Read.All Files.ReadWrite.All";

        public OneDriveApi(IGraphServiceClient graphClient) : base(graphClient)
        {
        }

        public OneDriveApi(ILogger<OneDriveApi> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        /// <summary>
        /// 新增資料夾流程
        /// 
        /// 包含: 新增資料夾API、列出OneDrive所有內容API、刪除資料夾API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallCreateFolderAsync()
        {
            try
            {
                DriveItem item = await CreateFolderAsync(graphClient);
                IDriveItemChildrenCollectionPage items = await ListDriveItemAsync(graphClient);

                bool isCreate = items.CurrentPage.Any(driveItem => driveItem.Id == item.Id);
                if (!isCreate)
                    return false;

                await DeleteDriveItemAsync(graphClient, item.Id);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 更新指定項目流程
        /// 
        /// 包含: 新增資料夾API、列出OneDrive所有內容API、更新指定項目內容API、刪除資料夾API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallUpdateDriveItemAsync()
        {
            try
            {
                DriveItem folderItem = await CreateFolderAsync(graphClient);
                IDriveItemChildrenCollectionPage items = await ListDriveItemAsync(graphClient);

                bool isCreate = items.CurrentPage.Any(driveItem => driveItem.Name == folderItem.Name);
                if (!isCreate)
                    return false;

                DriveItem newFolderItem = await UpdateDriveItemAsync(graphClient, folderItem.Id);
                items = await ListDriveItemAsync(graphClient);

                bool isUpdate = items.CurrentPage.Any(driveItem => driveItem.Name == newFolderItem.Name);
                if (!isUpdate)
                    return false;

                await DeleteDriveItemAsync(graphClient, folderItem.Id);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 移動指定項目流程
        /// 
        /// 包含: 新增資料夾API、列出OneDrive所有內容API、移動指定項目API、列出指定目錄內容API、刪除資料夾API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallMoveDriveItemAsync()
        {
            try
            {
                DriveItem[] folderItem = new DriveItem[2];
                folderItem[0] = await CreateFolderAsync(graphClient);
                folderItem[1] = await CreateFolderAsync(graphClient);
                IDriveItemChildrenCollectionPage items = await ListDriveItemAsync(graphClient);
                bool isUpdate = items.CurrentPage.Count(driveItem => folderItem.Select(c => c.Id).Contains(driveItem.Id)) == 2;
                if (!isUpdate)
                    return false;

                await MoveDriveItemAsync(graphClient, folderItem[0].Id, folderItem[1].Id);

                IDriveItemChildrenCollectionPage folder1Items = await GetDriveItemAsync(graphClient, folderItem[0].Id);
                bool isMove = folder1Items.CurrentPage.Any(driveItem => driveItem.Id == folderItem[1].Id);
                if (!isMove)
                    return false;

                await DeleteDriveItemAsync(graphClient, folderItem[0].Id);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 列出OneDrive所有內容 API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <returns></returns>
        private static async Task<IDriveItemChildrenCollectionPage> ListDriveItemAsync(IGraphServiceClient graphClient)
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
        private static async Task<IDriveItemChildrenCollectionPage> GetDriveItemAsync(IGraphServiceClient graphClient, string itemId)
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
        private static async Task<DriveItem> CreateFolderAsync(IGraphServiceClient graphClient)
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

            DriveItem item =  await graphClient.Me.Drive.Root.Children
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
        private static async Task DeleteDriveItemAsync(IGraphServiceClient graphClient, string itemId)
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
        private static async Task<DriveItem> UpdateDriveItemAsync(IGraphServiceClient graphClient, string itemId)
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
        private static async Task MoveDriveItemAsync(IGraphServiceClient graphClient, string parentItemId, string itemId)
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
