using System;
using Volo.Abp.Application.Dtos;

namespace AbpDemo.BasicData.WorkCenters.Dtos;

/// <summary>
/// 工作中心分页查询请求（支持按车间筛选）
/// </summary>
public class GetWorkCenterListRequest : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 按车间ID筛选
    /// </summary>
    public Guid? WorkshopId { get; set; }
}