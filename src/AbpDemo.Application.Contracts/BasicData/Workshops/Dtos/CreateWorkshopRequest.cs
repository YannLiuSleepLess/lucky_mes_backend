using System;
using System.ComponentModel.DataAnnotations;

namespace AbpDemo.BasicData.Workshops.Dtos;

public class CreateWorkshopRequest
{
    [Required] [StringLength(50)] public string WorkshopCode { get; set; }

    [Required] [StringLength(200)] public string WorkshopName { get; set; }

    [StringLength(200)] public string Location { get; set; }

    public Guid? ManagerId { get; set; }
}