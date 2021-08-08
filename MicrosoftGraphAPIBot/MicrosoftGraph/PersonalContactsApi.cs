using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    /// <summary>
    /// Personal contacts Api 腳本
    /// </summary>
    public class PersonalContactsApi : GraphApi
    {
        public const string Scope = "Contacts.Read Contacts.ReadWrite";

        public PersonalContactsApi(GraphServiceClient graphClient) : base(graphClient)
        {
        }

        public PersonalContactsApi(ILogger<PersonalContactsApi> logger, IConfiguration configuration) : base(logger, configuration)
        {
        }

        /// <summary>
        /// 新增聯絡人流程
        /// 
        /// 包含: 取得聯絡人資訊API、新增聯絡人資訊API、刪除聯絡人資訊API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallCreateContactAsync()
        {
            Contact contact = null;
            try
            {
                contact = await CreateContact(graphClient);
                Contact contact1 = await GetContact(graphClient, contact.Id);
                Utils.Assert(contact.DisplayName == contact1.DisplayName);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                if (contact != null)
                    try
                    {
                        await DeleteContact(graphClient, contact.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
            }
        }

        /// <summary>
        /// 更新聯絡人流程
        /// 
        /// 包含: 取得聯絡人資訊API、新增聯絡人資訊API、刪除聯絡人資訊API、更新聯絡人資訊API
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CallUpdateContactAsync()
        {
            Contact contact = null;
            try
            {
                contact = await CreateContact(graphClient);
                Contact contact1 = await GetContact(graphClient, contact.Id);
                Utils.Assert(contact.DisplayName == contact1.DisplayName);

                Contact updateContact = await UpdateContact(graphClient, contact.Id);
                contact1 = await GetContact(graphClient, contact.Id);
                Utils.Assert(updateContact.DisplayName == contact1.DisplayName);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                if (contact != null)
                    try
                    {
                        await DeleteContact(graphClient, contact.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
            }
        }

        /// <summary>
        /// 取得聯絡人資訊API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static async Task<Contact> GetContact(GraphServiceClient graphClient, string Id)
        {
            return await graphClient.Me.Contacts[Id]
                .Request()
                .GetAsync();
        }


        /// <summary>
        /// 新增聯絡人資訊API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <returns></returns>
        public static async Task<Contact> CreateContact(GraphServiceClient graphClient)
        {
            var contact = await graphClient.Me.Contacts
                .Request()
                .AddAsync(new Contact
                {
                    GivenName = Guid.NewGuid().ToString(),
                    DisplayName = Guid.NewGuid().ToString(),
                    Birthday = DateTime.Now
                });

            await Task.Delay(5000);
            return contact;
        }

        /// <summary>
        /// 刪除聯絡人資訊API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static async Task DeleteContact(GraphServiceClient graphClient, string Id)
        {
            await Task.Delay(Utils.DeleteDelayTime);
            await graphClient.Me.Contacts[Id]
                .Request()
                .DeleteAsync();
        }

        /// <summary>
        /// 更新聯絡人資訊API
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static async Task<Contact> UpdateContact(GraphServiceClient graphClient, string Id)
        {
            var contact = await graphClient.Me.Contacts[Id]
                .Request()
                .UpdateAsync(new Contact
                {
                    GivenName = Guid.NewGuid().ToString(),
                    DisplayName = Guid.NewGuid().ToString(),
                    Birthday = DateTime.Now
                });

            await Task.Delay(5000);
            return contact;
        }
    }
}
