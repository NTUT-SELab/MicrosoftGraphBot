﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    /// <summary>
    /// Outlook Api 腳本
    /// </summary>
    public class OutlookApi : GraphApi
    {
        public const string Scope = "Mail.Read Mail.ReadWrite Mail.Send";

        public OutlookApi(GraphServiceClient graphClient) : base(graphClient)
        {
        }

        public OutlookApi(ILogger<OutlookApi> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        /// <summary>
        /// 新增草稿流程
        /// 
        /// 包含: 新增草稿API、取得訊息API、刪除訊息API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallCreateMessageAsync()
        {
            Message message = null;
            try
            {
                message = await CreateMessageAsync(graphClient);
                Message message1 = await GetMessageAsync(graphClient, message.Id);

                Trace.Assert(message.Id == message1.Id);

                return true;
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                if (message != null)
                    try
                    {
                        await DeleteMessageAsync(graphClient, message.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
            }
        }

        /// <summary>
        /// 更新草稿流程
        /// 
        /// 包含: 新增草稿API、取得訊息API、刪除訊息API、更新訊息API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallUpdateMessageAsync()
        {
            Message message = null;
            try
            {
                message = await CreateMessageAsync(graphClient);
                Message message1 = await GetMessageAsync(graphClient, message.Id);

                Trace.Assert(message.Id == message1.Id);

                Guid id = await UpdateMessageAsync(graphClient, message.Id);
                message1 = await GetMessageAsync(graphClient, message.Id);

                Trace.Assert(message1.Subject == id.ToString());

                return true;
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                if (message != null)
                    try
                    {
                        await DeleteMessageAsync(graphClient, message.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
            }
        }

        /// <summary>
        /// 發送訊息流程
        /// 
        /// 包含: 新增草稿API、取得訊息API、刪除訊息API、發送訊息API、列出所有訊息API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallSendMessageAsync()
        {
            IEnumerable<Message> messages1 = null;
            try
            {
                Message message = await CreateMessageAsync(graphClient);
                Message message1 = await GetMessageAsync(graphClient, message.Id);

                Trace.Assert(message.Id == message1.Id);

                await SendMessageAsync(graphClient, message.Id);

                IList<Message> messages = await ListMessageAsync(graphClient);
                messages1 = messages.Where(item => item.Subject.Contains(message.Subject));
                Trace.Assert(messages1.Any());

                return true;
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                if (messages1 != null)
                    try
                    {
                        foreach (Message message2 in messages1)
                            await DeleteMessageAsync(graphClient, message2.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
            }
        }

        /// <summary>
        /// 列出所有訊息API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <returns></returns>
        private static async Task<IUserMessagesCollectionPage> ListMessageAsync(GraphServiceClient graphClient)
        {
            IUserMessagesCollectionPage messages = await graphClient.Me.Messages
                .Request()
                .Select(e => new {e.Sender, e.Subject})
                .GetAsync();

            return messages;
        }

        /// <summary>
        /// 新增草稿API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <returns></returns>
        private static async Task<Message> CreateMessageAsync(GraphServiceClient graphClient)
        {
            Guid Id = Guid.NewGuid();
            var message = new Message
            {
                Subject = $"Bot {Id}",
                Importance = Importance.Low,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = Id.ToString()
                },
                ToRecipients = new List<Recipient>()
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = (await DefaultGraphApi.GetUserInfoAsync(graphClient)).Mail
                        }
                    }
                }
            };

            message = await graphClient.Me.Messages
                .Request()
                .AddAsync(message);

            return message;
        }

        /// <summary>
        /// 更新訊息API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="mailId"></param>
        /// <returns></returns>
        private static async Task<Guid> UpdateMessageAsync(GraphServiceClient graphClient, string mailId)
        {
            Guid Id = Guid.NewGuid();
            var message = new Message
            {
                Subject = Id.ToString(),
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = "content-value"
                },
                InferenceClassification = InferenceClassificationType.Other
            };

            await graphClient.Me.Messages[mailId].Request().UpdateAsync(message);

            return Id;
        }

        /// <summary>
        /// 發送訊息API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="mailId"></param>
        /// <returns></returns>
        private static async Task SendMessageAsync(GraphServiceClient graphClient, string mailId)
        {
            await graphClient.Me.Messages[mailId].Send().Request().PostAsync();
            await Task.Delay(5000);
        }

        /// <summary>
        /// 刪除訊息API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="mailId"></param>
        /// <returns></returns>
        private static async Task DeleteMessageAsync(GraphServiceClient graphClient, string mailId)
        {
            await graphClient.Me.Messages[mailId].Request().DeleteAsync();
        }

        /// <summary>
        /// 取得訊息API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="mailId"></param>
        /// <returns></returns>
        private static async Task<Message> GetMessageAsync(GraphServiceClient graphClient, string mailId)
        {
            return await graphClient.Me.Messages[mailId].Request().GetAsync();
        }
    }
}
