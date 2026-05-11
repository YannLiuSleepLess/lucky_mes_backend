using System;

namespace AbpDemo.Engineering.Processes.Dtos;

public class ProcessStepDto
{
    public Guid Id { get; set; }
    public string StepNo { get; set; }
    public string StepName { get; set; }
    public int Sequence { get; set; }
    public decimal StandardTime { get; set; }
    public bool IsKeyProcess { get; set; }
    public string Description { get; set; }
}