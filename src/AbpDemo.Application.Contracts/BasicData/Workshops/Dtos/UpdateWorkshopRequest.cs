using System;
using System.ComponentModel.DataAnnotations;

namespace AbpDemo.BasicData.Workshops.Dtos;

public class UpdateWorkshopRequest
{
    [Required] [StringLength(200)] public string WorkshopName { get; set; }

    [StringLength(200)] public string Location { get; set; }

    public Guid? ManagerId { get; set; }

    public bool IsActive { get; set; }
}