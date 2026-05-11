using System;
using AbpDemo.Domain.Shared.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace AbpDemo.Engineering.Processes;

/// <summary>
/// 工艺参数子实体
/// </summary>
public class ProcessParameter : Entity<Guid>
{
    public Guid ProcessStepId { get; private set; }
    public string ParameterName { get; private set; }
    public string ParameterCode { get; private set; }
    public string Unit { get; private set; }
    public ParameterRange Range { get; private set; } // 值对象
    public bool IsCritical { get; private set; }
    public ParameterDataType DataType { get; private set; }

    // 光伏特有字段（SPC统计过程控制）
    public decimal? UCL { get; private set; } // 上控制限
    public decimal? LCL { get; private set; } // 下控制限
    public decimal? USL { get; private set; } // 上规格限
    public decimal? LSL { get; private set; } // 下规格限
    public int? SamplingIntervalSeconds { get; private set; } // 采样频率（秒）

    protected ProcessParameter()
    {
    }

    public ProcessParameter(Guid id, Guid processStepId, string parameterName, string parameterCode,
        string unit, ParameterRange range, bool isCritical = false,
        ParameterDataType dataType = ParameterDataType.Decimal)
    {
        Id = id;
        ProcessStepId = processStepId;
        SetParameterName(parameterName);
        SetParameterCode(parameterCode);
        Unit = unit;
        Range = range ?? throw new ArgumentNullException(nameof(range));
        IsCritical = isCritical;
        DataType = dataType;
    }

    /// <summary>
    /// 配置SPC控制限（光伏特有）
    /// </summary>
    public void ConfigureSpcLimits(decimal? ucl, decimal? lcl, decimal? usl, decimal? lsl,
        int? samplingIntervalSeconds = null)
    {
        UCL = ucl;
        LCL = lcl;
        USL = usl;
        LSL = lsl;
        SamplingIntervalSeconds = samplingIntervalSeconds;
    }

    /// <summary>
    /// 校验参数值是否在范围内
    /// </summary>
    public bool IsValueInRange(decimal value, out string errorMessage)
    {
        errorMessage = null;

        if (Range != null)
        {
            if (value < Range.MinValue || value > Range.MaxValue)
            {
                errorMessage = $"参数值 {value} 超出范围 [{Range.MinValue}, {Range.MaxValue}]";
                return false;
            }
        }

        // SPC控制限检查
        if (UCL.HasValue && value > UCL)
        {
            errorMessage = $"参数值 {value} 超过上控制限 {UCL}";
            return false;
        }

        if (LCL.HasValue && value < LCL)
        {
            errorMessage = $"参数值 {value} 低于下控制限 {LCL}";
            return false;
        }

        return true;
    }

    public void SetParameterName(string parameterName)
    {
        ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
    }

    public void SetParameterCode(string parameterCode)
    {
        ParameterCode = parameterCode ?? throw new ArgumentNullException(nameof(parameterCode));
    }
}

/// <summary>
/// 参数数据类型
/// </summary>
public enum ParameterDataType : byte
{
    Decimal = 1,
    Integer = 2,
    String = 3,
    Boolean = 4
}