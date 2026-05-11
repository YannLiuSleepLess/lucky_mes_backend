using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AbpDemo.Engineering.Processes.Dtos;

public class CreateProcessRouteRequest
{
    [Required] public string RouteCode { get; set; }

    [Required] public string RouteName { get; set; }

    [Required] public Guid ProductId { get; set; }

    public List<CreateProcessStepRequest> Steps { get; set; } = new();
}

public class CreateProcessStepRequest
{
    [Required] public string StepNo { get; set; }

    [Required] public string StepName { get; set; }

    public decimal StandardTime { get; set; }
    public bool IsKeyProcess { get; set; }
    public string Description { get; set; }
}