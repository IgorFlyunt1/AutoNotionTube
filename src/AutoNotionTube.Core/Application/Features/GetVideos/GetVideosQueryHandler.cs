#region Header
// -----------------------------------------------------------------------
//  <copyright file="GetVideosQueryHandler.cs" company="INVENTIO AG">
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
using AutoNotionTube.Domain.Entities;
using MediatR;

namespace AutoNotionTube.Core.Application.Features.GetVideos
{
    public sealed class GetVideosQueryHandler : IRequestHandler<GetVideosQuery, IReadOnlyCollection<VideoFile>>
    {
        private readonly IVideoRepository _videoRepository;

        public GetVideosQueryHandler(IVideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
        }

        public async Task<IReadOnlyCollection<VideoFile>> Handle(GetVideosQuery request, CancellationToken cancellationToken)
        {
            return await _videoRepository.GetVideosAsync();
        }
    }
}
