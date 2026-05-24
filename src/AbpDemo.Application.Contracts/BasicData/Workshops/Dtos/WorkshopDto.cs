using System;
using Volo.Abp.Application.Dtos;

namespace AbpDemo.BasicData.Workshops.Dtos;

public class WorkshopDto : AuditedEntityDto<Guid>
{
    public string WorkshopCode { get; set; }
    public string WorkshopName { get; set; }
    public string Location { get; set; }
    public Guid? ManagerId { get; set; }
    public bool IsActive { get; set; }
}