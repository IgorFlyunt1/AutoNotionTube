#region Header
// -----------------------------------------------------------------------
//  <copyright file="GetCaptionsQuery.cs" company="INVENTIO AG">
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

namespace AutoNotionTube.Core.Application.Features.GetCaptions
{
    public class GetCaptionsQuery : IRequest<string>
    {
        public string VideoId { get; set; } = null!;
        public int Seconds { get; set; }
        public double SizeMb { get; set; }
    }
}
