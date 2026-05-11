using System;
using AbpDemo.Enums;
using Volo.Abp.Domain.Entities;

namespace AbpDemo.Engineering.Processes;

/// <summary>
/// 工序子实体
/// </summary>
public class ProcessStep : Entity<Guid>
{
    public Guid ProcessRouteId { get; private set; }
    public string StepNo { get; private set; }
    public string StepName { get; private set; }
    public int Sequence { get; private set; }
    public Guid? EquipmentTypeId { get; private set; }
    public Guid? WorkCenterId { get; private set; }
    public decimal StandardTime { get; private set; }
    public bool IsCritical { get; private set; }
    public bool IsInspectionRequired { get; private set; }
    public InspectionType? InspectionType { get; private set; }

    // 光伏特有字段
    public bool IsSortingStep { get; private set; } // 是否分选工序
    public SortType? SortingType { get; private set; } // 分选类型
    public decimal? ThresholdA { get; private set; } // A级阈值
    public decimal? ThresholdB { get; private set; } // B级阈值
    public decimal? ThresholdC { get; private set; } // C级阈值
    public Guid? NextStepForGradeA { get; private set; } // A级流向工序
    public Guid? NextStepForGradeB { get; private set; } // B级流向工序
    public Guid? NextStepForGradeC { get; private set; } // C级流向工序
    public Guid? InputProductId { get; private set; } // 输入物料（一对多转换）
    public Guid? OutputProductId { get; private set; } // 输出物料（一对多转换）
    public decimal? YieldRate { get; private set; } // 产出率

    protected ProcessStep()
    {
    }

    public ProcessStep(Guid id, Guid processRouteId, string stepNo, string stepName, decimal standardTime,
        bool isCritical, Guid? equipmentTypeId = null, Guid? workCenterId = null, int sequence = 0,
        bool isInspectionRequired = false, InspectionType? inspectionType = null)
    {
        Id = id;
        ProcessRouteId = processRouteId;
        SetStepNo(stepNo);
        SetStepName(stepName);
        SetStandardTime(standardTime);
        IsCritical = isCritical;
        EquipmentTypeId = equipmentTypeId;
        WorkCenterId = workCenterId;
        Sequence = sequence;
        IsInspectionRequired = isInspectionRequired;
        InspectionType = inspectionType;

        // 默认值
        IsSortingStep = false;
    }

    #region Public Methods

    public void SetStepNo(string stepNo)
    {
        if (string.IsNullOrWhiteSpace(stepNo))
            throw new ArgumentNullException(nameof(stepNo));

        // 验证格式：OP{SEQ}
        if (!stepNo.StartsWith("OP"))
            throw new ArgumentException("工序编号必须以OP开头");

        StepNo = stepNo;
    }

    public void SetStepName(string stepName)
    {
        StepName = stepName ?? throw new ArgumentNullException(nameof(stepName));
    }

    public void SetStandardTime(decimal standardTime)
    {
        if (standardTime < 0)
            throw new ArgumentException("标准工时必须大于等于0");

        StandardTime = standardTime;
    }

    public void SetSequence(int sequence)
    {
        if (sequence <= 0)
            throw new ArgumentException("序号必须大于0");

        Sequence = sequence;
    }

    /// <summary>
    /// 配置为分选工序（光伏特有）
    /// </summary>
    public void ConfigureAsSortingStep(SortType sortingType, decimal thresholdA, decimal thresholdB,
        decimal thresholdC,
        Guid? nextStepForGradeA = null, Guid? nextStepForGradeB = null, Guid? nextStepForGradeC = null)
    {
        IsSortingStep = true;
        SortingType = sortingType;
        ThresholdA = thresholdA;
        ThresholdB = thresholdB;
        ThresholdC = thresholdC;
        NextStepForGradeA = nextStepForGradeA;
        NextStepForGradeB = nextStepForGradeB;
        NextStepForGradeC = nextStepForGradeC;
    }

    /// <summary>
    /// 配置一对多转换工序（光伏特有）
    /// </summary>
    public void ConfigureMaterialConversion(Guid inputProductId, Guid outputProductId, decimal yieldRate)
    {
        if (yieldRate <= 0)
            throw new ArgumentException("产出率必须大于0");

        InputProductId = inputProductId;
        OutputProductId = outputProductId;
        YieldRate = yieldRate;
    }

    /// <summary>
    /// 评估分选等级（光伏特有）
    /// </summary>
    public GradeType? EvaluateGrade(decimal value)
    {
        if (!IsSortingStep)
            throw new InvalidOperationException("该工序不是分选工序");

        if (!ThresholdA.HasValue || !ThresholdB.HasValue || !ThresholdC.HasValue)
            throw new InvalidOperationException("分选阈值未配置");

        if (!SortingType.HasValue)
            throw new InvalidOperationException("分选类型未配置");

        return SortingType.Value switch
        {
            SortType.Efficiency => EvaluateByEfficiency(value),
            SortType.Power => EvaluateByPower(value),
            SortType.Thickness => EvaluateByThickness(value),
            SortType.Resistance => EvaluateByResistance(value),
            _ => throw new InvalidOperationException($"不支持的分选类型: {SortingType.Value}")
        };
    }

    /// <summary>
    /// 获取下一道工序（考虑分选等级）
    /// </summary>
    public Guid? GetNextStep(GradeType? grade = null)
    {
        if (IsSortingStep && grade.HasValue)
        {
            return grade switch
            {
                GradeType.A => NextStepForGradeA,
                GradeType.B => NextStepForGradeB,
                GradeType.C => NextStepForGradeC,
                GradeType.D => null, // D级报废
                _ => null
            };
        }

        // 非分选工序，返回null（由外部逻辑决定）
        return null;
    }

    #endregion

    #region Private Methods

    private GradeType EvaluateByEfficiency(decimal efficiency)
    {
        if (efficiency >= ThresholdA) return GradeType.A;
        if (efficiency >= ThresholdB) return GradeType.B;
        if (efficiency >= ThresholdC) return GradeType.C;
        return GradeType.D;
    }

    private GradeType EvaluateByPower(decimal power)
    {
        if (power >= ThresholdA) return GradeType.A;
        if (power >= ThresholdB) return GradeType.B;
        if (power >= ThresholdC) return GradeType.C;
        return GradeType.D;
    }

    private GradeType EvaluateByThickness(decimal thickness)
    {
        // 厚度越小越好，逻辑相反
        if (thickness <= ThresholdA) return GradeType.A;
        if (thickness <= ThresholdB) return GradeType.B;
        if (thickness <= ThresholdC) return GradeType.C;
        return GradeType.D;
    }

    private GradeType EvaluateByResistance(decimal resistance)
    {
        if (resistance >= ThresholdA) return GradeType.A;
        if (resistance >= ThresholdB) return GradeType.B;
        if (resistance >= ThresholdC) return GradeType.C;
        return GradeType.D;
    }

    #endregion
}