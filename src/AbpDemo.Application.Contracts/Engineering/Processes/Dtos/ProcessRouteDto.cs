using System;
using System.Collections.Generic;
using AbpDemo.Enums;

namespace AbpDemo.Engineering.Processes.Dtos;

public class ProcessRouteDto
{
    public Guid Id { get; set; }
    public string RouteCode { get; set; }
    public string RouteName { get; set; }
    public Guid ProductId { get; set; }
    public string Version { get; set; }
    public ProcessRouteStatus Status { get; set; }
    public int StepCount { get; set; } // 工艺步骤数量
    public List<ProcessStepDto> Steps { get; set; } = new();
}