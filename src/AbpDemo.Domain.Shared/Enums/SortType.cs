namespace AbpDemo.Enums;

/// <summary>
/// 分选类型（光伏特有）
/// </summary>
public enum SortType : byte
{
    None = 0,
    Efficiency = 1, // 效率分选（电池片）
    Power = 2, // 功率分选（组件）
    Thickness = 3, // 厚度分选（硅片）
    Resistance = 4 // 电阻率分选（硅片）
}