#region Header
// -----------------------------------------------------------------------
//  <copyright file="GetCaptionsHandler.cs" company="INVENTIO AG">
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

using AutoNotionTube.Core.Exceptions;
using AutoNotionTube.Core.Extensions;
using AutoNotionTube.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoNotionTube.Core.Application.Features.GetCaptions
{
    public class GetCaptionsHandler : IRequestHandler<GetCaptionsQuery, string>
    {
        private readonly IYoutubeService _youtubeService;
        private readonly ILogger<GetCaptionsHandler> _logger;

        public GetCaptionsHandler(IYoutubeService youtubeService, ILogger<GetCaptionsHandler> logger)
        {
            _youtubeService = youtubeService;
            _logger = logger;
        }

        public async Task<string> Handle(GetCaptionsQuery request, CancellationToken cancellationToken)
        {
            var waitTime = request.Seconds.GetApproximateCaptionWaitTime(request.SizeMb);
            var maxAttempts = 10;
            var attempt = 0;

            while (attempt < maxAttempts)
            {
                _logger.LogInformation(
                    "Attempt {Attempt} to get captions for video with ID {RequestVideoId}, waiting : {WaitTime} sec",
                    attempt + 1, request.VideoId, waitTime);
                
                await Task.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken);

                bool hasCaptions = await _youtubeService.VideoHasCaptions(request.VideoId, cancellationToken);
                string captions = string.Empty;

                if (hasCaptions)
                {
                    captions = await _youtubeService.GetCaptions(request.VideoId, cancellationToken);
                }

                if (!string.IsNullOrEmpty(captions))
                {
                    string captionWithoutTimestamps = captions.RemoveTimestamps();
                    string captionWithoutExtraNewlines = captionWithoutTimestamps.RemoveExtraNewlines();
                    _logger.LogInformation("Captions for video with ID {RequestVideoId} successfully retrieve", request.VideoId);
                    return captionWithoutExtraNewlines;
                }

                attempt++;
            }

            throw new CaptionNotAvailableException(
                $"Captions for video with ID {request.VideoId} are still not available after {maxAttempts} attempts.");
        }

    }
}
