#region Header
// -----------------------------------------------------------------------
//  <copyright file="NotionService.cs" company="INVENTIO AG">
//      Copyright © 2023 INVENTIO AG
//      All rights reserved.
//      INVENTIO AG, Seestrasse 55, CH-6052 Hergiswil, owns and retains all copyrights and other intellectual property rights in this
//      document. Any reproduction, translation, copying or storing in data processing units in any form or by any means without prior
//      permission of INVENTIO AG is regarded as infringement and will be prosecuted.
// 
//      'CONFIDENTIAL'
//      This document contains confidential information that is proprietary to the Schindler Group. Neither this document nor the
//      information contained herein shall be disclosed to third parties nor used for manufacturing or any other application without
//      written consent of INVENTIO AG.
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System.Text;
using System.Text.Json;
using AutoNotionTube.Core.DTOs;
using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoNotionTube.Infrastructure.Services
{
    public class NotionService : INotionService
    {
        private readonly NotionSettings _notionSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotionService> _logger;

        public NotionService(IOptions<NotionSettings> notionSettings, IHttpClientFactory clientFactory,
            ILogger<NotionService> logger)
        {
            _logger = logger;
            _httpClient = clientFactory.CreateClient();
            _notionSettings = notionSettings.Value;
        }

        public async Task<bool> CreateNote(NotionNoteRequest note)
        {
            _logger.LogInformation("Calling the Azure Function to create a new Notion Note");
            
            var data = new StringContent(JsonSerializer.Serialize(note), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_notionSettings.AzureFunctionUrl, data);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Something went wrong when calling the Azure Function. Status Code: {ResponseStatusCode}",
                    response.StatusCode);
                throw new Exception("Something went wrong when calling the API.");
            }

            string result = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Successfully called the Azure Function. Response: {Response}", result);
            
            return true;
        }
    }
}
