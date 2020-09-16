using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    public class PermissionsApi : GraphApi
    {
        public PermissionsApi(IGraphServiceClient graphClient) : base(graphClient)
        {
        }

        public PermissionsApi(ILogger<PermissionsApi> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        /// <summary>
        /// 新增分享連結流程
        /// 
        /// 包含: 產生分享連結API、列出分享連結API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallCreateShareLinkAsync()
        {
            try
            {
                DriveItem item = await FileApi.CreateFolderAsync(graphClient);
                IDriveItemChildrenCollectionPage items = await FileApi.ListDriveItemAsync(graphClient);

                bool isCreate = items.CurrentPage.Any(driveItem => driveItem.Id == item.Id);
                Trace.Assert(isCreate);

                Permission link = await CreateShareLinkAsync(graphClient, item.Id);
                var links = await ListShareLinkAsync(graphClient, item.Id);

                isCreate = links.CurrentPage.Any(linkItem => linkItem.Id == link.Id);
                Trace.Assert(isCreate);

                await FileApi.DeleteDriveItemAsync(graphClient, item.Id);
                return true;
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 存取分享連結流程
        /// 
        /// 包含: 產生分享連結API、列出分享連結API、透過分享連結存取指定DriveItem API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallAccessingShareLinkAsync()
        {
            try
            {
                DriveItem item = await FileApi.CreateFolderAsync(graphClient);
                IDriveItemChildrenCollectionPage items = await FileApi.ListDriveItemAsync(graphClient);

                bool isCreate = items.CurrentPage.Any(driveItem => driveItem.Id == item.Id);
                Trace.Assert(isCreate);

                Permission link = await CreateShareLinkAsync(graphClient, item.Id);
                var links = await ListShareLinkAsync(graphClient, item.Id);

                isCreate = links.CurrentPage.Any(linkItem => linkItem.Id == link.Id);
                Trace.Assert(isCreate);

                SharedDriveItem sharedItem = await AccessingSharedLinkAsync(graphClient, link.Link.WebUrl);
                Trace.Assert(sharedItem.Name == item.Name);

                await FileApi.DeleteDriveItemAsync(graphClient, item.Id);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 取得分享連結資訊流程
        /// 
        /// 包含: 產生分享連結API、取得分享連結資訊API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallGetSharingLinkAsync()
        {
            DriveItem item = await FileApi.CreateFolderAsync(graphClient);
            IDriveItemChildrenCollectionPage items = await FileApi.ListDriveItemAsync(graphClient);

            bool isCreate = items.CurrentPage.Any(driveItem => driveItem.Id == item.Id);
            Trace.Assert(isCreate);

            Permission link = await CreateShareLinkAsync(graphClient, item.Id);
            Permission linkInfo = await GetShareLinkAsync(graphClient, item.Id, link.Id);
            Trace.Assert(string.Join(',', link.Roles) == string.Join(',', linkInfo.Roles));

            await FileApi.DeleteDriveItemAsync(graphClient, item.Id);
            return true;
        }

        /// <summary>
        /// 更新分享連結權限流程
        /// 
        /// 包含: 產生分享連結API、列出分享連結API、更新分享連結權限API、取得分享連結資訊API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallUpdateSharingLinkAsync()
        {
            try
            {
                DriveItem item = await FileApi.CreateFolderAsync(graphClient);
                IDriveItemChildrenCollectionPage items = await FileApi.ListDriveItemAsync(graphClient);

                bool isCreate = items.CurrentPage.Any(driveItem => driveItem.Id == item.Id);
                Trace.Assert(isCreate);

                Permission link = await CreateShareLinkAsync(graphClient, item.Id);
                var links = await ListShareLinkAsync(graphClient, item.Id);

                isCreate = links.CurrentPage.Any(linkItem => linkItem.Id == link.Id);
                Trace.Assert(isCreate);

                link = await UpdateShareLinkAsync(graphClient, item.Id, link.Id);
                Permission linkInfo = await GetShareLinkAsync(graphClient, item.Id, link.Id);
                Trace.Assert(string.Join(',', link.Roles) == string.Join(',', linkInfo.Roles));

                await FileApi.DeleteDriveItemAsync(graphClient, item.Id);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 刪除分享連結流程
        /// 
        /// 包含: 產生分享連結API、列出分享連結API、刪除分享連結API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallDeleteSharingLinkAsync()
        {
            try
            {
                DriveItem item = await FileApi.CreateFolderAsync(graphClient);
                IDriveItemChildrenCollectionPage items = await FileApi.ListDriveItemAsync(graphClient);

                bool isCreate = items.CurrentPage.Any(driveItem => driveItem.Id == item.Id);
                Trace.Assert(isCreate);

                Permission link = await CreateShareLinkAsync(graphClient, item.Id);
                var links = await ListShareLinkAsync(graphClient, item.Id);

                isCreate = links.CurrentPage.Any(linkItem => linkItem.Id == link.Id);
                Trace.Assert(isCreate);

                await DeleteShareLinkAsync(graphClient, item.Id, link.Id);
                links = await ListShareLinkAsync(graphClient, item.Id);

                isCreate = links.CurrentPage.Any(linkItem => linkItem.Id == link.Id);
                Trace.Assert(!isCreate);

                await FileApi.DeleteDriveItemAsync(graphClient, item.Id);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 列出指定 Drive item 的所有分享連結資訊
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static async Task<IDriveItemPermissionsCollectionPage> ListShareLinkAsync(IGraphServiceClient graphClient, string itemId)
        {
            return await graphClient.Me.Drive.Items[itemId].Permissions
                                    .Request()
                                    .GetAsync();
        }

        /// <summary>
        /// 產生指定 Drive item 的分享連結
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static async Task<Permission> CreateShareLinkAsync(IGraphServiceClient graphClient, string itemId)
        {
            string type = "view";
            var scope = "anonymous";

            Permission item = await graphClient.Me.Drive.Items[itemId]
                .CreateLink(type, scope, null, null, null)
                .Request()
                .PostAsync();

            await Task.Delay(5000);
            return item;
        }

        /// <summary>
        /// 透過分享連結存取指定 Drive item
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="sharingUrl"></param>
        /// <returns></returns>
        private static async Task<SharedDriveItem> AccessingSharedLinkAsync(IGraphServiceClient graphClient, string sharingUrl)
        {
            string base64Value = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sharingUrl));
            string encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/', '_').Replace('+', '-');

            return await graphClient.Shares[encodedUrl]
                                    .Request()
                                    .GetAsync();
        }

        /// <summary>
        /// 取得分享連結資訊
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="itemId"></param>
        /// <param name="linkId"></param>
        /// <returns></returns>
        private static async Task<Permission> GetShareLinkAsync(IGraphServiceClient graphClient, string itemId, string linkId)
        {
            return await graphClient.Me.Drive.Items[itemId].Permissions[linkId]
                                    .Request()
                                    .GetAsync();
        }

        /// <summary>
        /// 更新分享連結權限
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="itemId"></param>
        /// <param name="linkId"></param>
        /// <returns></returns>
        private static async Task<Permission> UpdateShareLinkAsync(IGraphServiceClient graphClient, string itemId, string linkId)
        {
            var permission = new Permission { Roles = new List<String>{ "write" }};

            Permission item = await graphClient.Me.Drive.Items[itemId].Permissions[linkId]
                                                .Request()
                                                .UpdateAsync(permission);

            await Task.Delay(5000);
            return item;
        }

        /// <summary>
        /// 刪除分享連結
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="itemId"></param>
        /// <param name="linkId"></param>
        /// <returns></returns>
        private static async Task DeleteShareLinkAsync(IGraphServiceClient graphClient, string itemId, string linkId)
        {
            await graphClient.Me.Drive.Items[itemId].Permissions[linkId]
                            .Request()
                            .DeleteAsync();

            await Task.Delay(5000);
        }
    }
}
