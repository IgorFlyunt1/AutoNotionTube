using AutoNotionTube.Domain.Entities;
using MediatR;

namespace AutoNotionTube.Core.Application.Features.GetVideos;

public sealed record GetVideosQuery : IRequest<IReadOnlyCollection<VideoFile>>;