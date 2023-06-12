#region Header
// -----------------------------------------------------------------------
//  <copyright file="CreateNotionNoteHandler.cs" company="INVENTIO AG">
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

using AutoNotionTube.Core.Interfaces;
using MediatR;

namespace AutoNotionTube.Core.Application.Features.CreateNotionNote
{
    public class CreateNotionNoteHandler : IRequestHandler<CreateNotionNoteCommand, bool>
    {
        private readonly INotionService _notionService;

        public CreateNotionNoteHandler(INotionService notionService)
        {
            _notionService = notionService;
        }

        public async Task<bool> Handle(CreateNotionNoteCommand request, CancellationToken cancellationToken)
        {
            return await _notionService.CreateNote(request.NotionNoteRequest); 
        }
    }
}
