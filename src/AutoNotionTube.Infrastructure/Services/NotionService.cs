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

using AutoNotionTube.Core.DTOs;
using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Notion.Client;

namespace AutoNotionTube.Infrastructure.Services
{
    public class NotionService : INotionService
    {
        private readonly NotionSettings _notionSettings;
        private readonly INotionClient _notionClient;

        public NotionService(IOptions<NotionSettings> notionSettings, INotionClient notionClient)
        {
            _notionClient = notionClient;
            _notionSettings = notionSettings.Value;
        }
            
        public async Task<bool> CreateNote(NotionNoteRequest note)
        {
          
            var pagesCreateParameters = PagesCreateParametersBuilder
                .Create(new DatabaseParentInput { DatabaseId = _notionSettings.DatabaseId })
                .AddProperty("Name",
                    new TitlePropertyValue
                    {
                        Title = new List<RichTextBase>
                        {
                            new RichTextText { Text = new Text { Content = "Test Page Title" } }
                        }
                    })
                .Build();

            // Create a new page in Notion.
            var page = await _notionClient.Pages.CreateAsync(pagesCreateParameters);

            // Return true if the page was created successfully.
            return page != null;
        }
    }
}
