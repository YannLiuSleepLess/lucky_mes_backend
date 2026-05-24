using System;
using Volo.Abp.Application.Dtos;

namespace AbpDemo.BasicData.WorkCenters.Dtos;

public class WorkCenterDto : AuditedEntityDto<Guid>
{
    public string WorkCenterCode { get; set; }
    public string WorkCenterName { get; set; }
    public Guid WorkshopId { get; set; }
    public string WorkshopName { get; set; }
    public int Capacity { get; set; }
    public int ShiftCount { get; set; }
    public bool IsActive { get; set; }
}