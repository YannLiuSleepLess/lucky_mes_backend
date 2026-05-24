using System;
using System.ComponentModel.DataAnnotations;

namespace AbpDemo.BasicData.WorkCenters.Dtos;

public class CreateWorkCenterRequest
{
    [Required] [StringLength(50)] public string WorkCenterCode { get; set; }

    [Required] [StringLength(200)] public string WorkCenterName { get; set; }

    [Required] public Guid WorkshopId { get; set; }

    [Range(1, int.MaxValue)] public int Capacity { get; set; } = 1;

    [Range(1, 3)] public int ShiftCount { get; set; } = 1;
}