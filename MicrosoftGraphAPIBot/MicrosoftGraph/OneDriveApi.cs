﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    /// <summary>
    /// OneDrive Api 腳本
    /// </summary>
    public class OneDriveApi : GraphApi
    {
        public const string Scope = "Files.Read Files.ReadWrite Files.Read.All Files.ReadWrite.All";

        public OneDriveApi(GraphServiceClient graphClient) : base(graphClient)
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
            DriveItem item = null;
            try
            {
                item = await FileApi.CreateFolderAsync(graphClient);
                IDriveItemChildrenCollectionPage items = await FileApi.ListDriveItemAsync(graphClient);

                bool isCreate = items.CurrentPage.Any(driveItem => driveItem.Id == item.Id);
                Trace.Assert(isCreate);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                if (item != null)
                    try
                    {
                        await FileApi.DeleteDriveItemAsync(graphClient, item.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
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
            DriveItem folderItem = null;
            try
            {
                folderItem = await FileApi.CreateFolderAsync(graphClient);
                IDriveItemChildrenCollectionPage items = await FileApi.ListDriveItemAsync(graphClient);

                bool isCreate = items.CurrentPage.Any(driveItem => driveItem.Name == folderItem.Name);
                Trace.Assert(isCreate);

                DriveItem newFolderItem = await FileApi.UpdateDriveItemAsync(graphClient, folderItem.Id);
                items = await FileApi.ListDriveItemAsync(graphClient);

                bool isUpdate = items.CurrentPage.Any(driveItem => driveItem.Name == newFolderItem.Name);
                Trace.Assert(isUpdate);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                if (folderItem != null)
                    try
                    {
                        await FileApi.DeleteDriveItemAsync(graphClient, folderItem.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
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
            DriveItem[] folderItem = null;
            try
            {
                folderItem = new DriveItem[2];
                folderItem[0] = await FileApi.CreateFolderAsync(graphClient);
                folderItem[1] = await FileApi.CreateFolderAsync(graphClient);
                IDriveItemChildrenCollectionPage items = await FileApi.ListDriveItemAsync(graphClient);
                bool isUpdate = items.CurrentPage.Count(driveItem => folderItem.Select(c => c.Id).Contains(driveItem.Id)) == 2;
                Trace.Assert(isUpdate);

                await FileApi.MoveDriveItemAsync(graphClient, folderItem[0].Id, folderItem[1].Id);

                IDriveItemChildrenCollectionPage folder1Items = await FileApi.GetDriveItemAsync(graphClient, folderItem[0].Id);
                bool isMove = folder1Items.CurrentPage.Any(driveItem => driveItem.Id == folderItem[1].Id);
                Trace.Assert(isMove);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                if (folderItem != null)
                    try
                    {
                        await FileApi.DeleteDriveItemAsync(graphClient, folderItem[0].Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
            }
        }
    }
}
