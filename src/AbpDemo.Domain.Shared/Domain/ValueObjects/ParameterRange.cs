using System;

namespace AbpDemo.Domain.Shared.ValueObjects;

/// <summary>
/// 参数范围值对象
/// </summary>
public class ParameterRange
{
    public decimal MinValue { get; private set; }
    public decimal MaxValue { get; private set; }
    public decimal DefaultValue { get; private set; }
    public decimal Tolerance { get; private set; }

    protected ParameterRange()
    {
    }

    public ParameterRange(decimal minValue, decimal maxValue, decimal defaultValue, decimal tolerance = 0)
    {
        if (maxValue < minValue)
            throw new ArgumentException("最大值不能小于最小值");

        if (defaultValue < minValue || defaultValue > maxValue)
            throw new ArgumentException("默认值必须在最小值和最大值之间");

        if (tolerance < 0)
            throw new ArgumentException("公差不能为负数");

        MinValue = minValue;
        MaxValue = maxValue;
        DefaultValue = defaultValue;
        Tolerance = tolerance;
    }

    /// <summary>
    /// 检查值是否在范围内
    /// </summary>
    public bool IsInRange(decimal value)
    {
        return value >= MinValue && value <= MaxValue;
    }

    /// <summary>
    /// 检查值是否在公差范围内
    /// </summary>
    public bool IsWithinTolerance(decimal value)
    {
        var lowerLimit = DefaultValue - Tolerance;
        var upperLimit = DefaultValue + Tolerance;
        return value >= lowerLimit && value <= upperLimit;
    }
}