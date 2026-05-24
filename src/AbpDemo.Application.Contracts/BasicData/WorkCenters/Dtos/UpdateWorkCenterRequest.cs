using System;
using System.ComponentModel.DataAnnotations;

namespace AbpDemo.BasicData.WorkCenters.Dtos;

public class UpdateWorkCenterRequest
{
    [Required] [StringLength(200)] public string WorkCenterName { get; set; }

    [Required] public Guid WorkshopId { get; set; }

    [Range(1, int.MaxValue)] public int Capacity { get; set; }

    [Range(1, 3)] public int ShiftCount { get; set; }

    public bool IsActive { get; set; }
}