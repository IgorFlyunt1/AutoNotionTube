#region Header
// -----------------------------------------------------------------------
//  <copyright file="DeleteVideoCommandHandler.cs" company="INVENTIO AG">
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

using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoNotionTube.Core.Application.Features.DeleteVideo
{
    public sealed class DeleteVideoCommandHandler : IRequestHandler<DeleteVideoCommand, Unit>
    {
        private readonly ILogger<DeleteVideoCommandHandler> _logger;

        public DeleteVideoCommandHandler(ILogger<DeleteVideoCommandHandler> logger)
        {
            _logger = logger;
        }

        public Task<Unit> Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
        {

            if (File.Exists(request.VideoFile))
            {
                try
                {
                    File.Delete(request.VideoFile);
                }
                catch (IOException ioExp)
                {
                    _logger.LogError(ioExp, "Could not delete file {Path}", request.VideoFile);
                }
            }
            
            _logger.LogInformation("Deleted file {Path}", request.VideoFile);

            return Unit.Task;
        }
    }
}
