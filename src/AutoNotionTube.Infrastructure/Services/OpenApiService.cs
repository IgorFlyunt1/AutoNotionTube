#region Header
// -----------------------------------------------------------------------
//  <copyright file="OpenApiService.cs" company="INVENTIO AG">
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

using AutoNotionTube.Core.Constants;
using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenApiResponse = AutoNotionTube.Core.DTOs.OpenApiResponse;

namespace AutoNotionTube.Infrastructure.Services
{
    public class OpenApiService : IOpenApiService
    {
        private readonly OpenAIAPI _api;
        private readonly ILogger<OpenApiService> _logger;

        public OpenApiService(IOptions<OpenApiSettings> openApiSettings, ILogger<OpenApiService> logger)
        {
            _logger = logger;
            OpenApiSettings openApiSettings1 = openApiSettings.Value;
            _api = new OpenAIAPI(openApiSettings1.ApiKey);
        }

        public async Task<OpenApiResponse> GetSummarize(string captions)
        {
            _logger.LogInformation("OpenAPI service called");
            
            OpenApiResponse response = new();

            // Create a new conversation
            var chat = _api.Chat.CreateConversation();

            // Start the conversation
            chat.AppendSystemMessage(OpenApiConstants.Start);

            // Summarize the video
            chat.AppendUserInput($"{OpenApiConstants.Summarize}\n{captions}");
            string summary = await chat.GetResponseFromChatbotAsync();
            response.Summary = summary;
            
            Task.Delay(10000).Wait();

            // Generate steps
            chat.AppendUserInput($"{OpenApiConstants.Steps}");
            string steps = await chat.GetResponseFromChatbotAsync();
            response.Steps = steps;
            
            Task.Delay(10000).Wait();

            // Generate short summary
            chat.AppendUserInput($"{OpenApiConstants.ShortSummarize}");
            string shortSummary = await chat.GetResponseFromChatbotAsync();
            response.ShortSummary = shortSummary;

            Task.Delay(10000).Wait();

            // Generate tags
            chat.AppendUserInput($"{OpenApiConstants.Tags}");
            string tags = await chat.GetResponseFromChatbotAsync();
            response.Tags = tags.Split(',').ToList();
            
            _logger.LogInformation("OpenAPI service finished");

            return response;
        }
    }
}
