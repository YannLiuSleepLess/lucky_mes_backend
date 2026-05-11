using System;
using System.Threading.Tasks;

namespace AbpDemo.Engineering.Processes;

/// <summary>
/// 工艺路线领域服务接口
/// </summary>
public interface IProcessRouteDomainService
{
    /// <summary>
    /// 从模板创建工艺路线
    /// </summary>
    Task<ProcessRoute> CreateFromTemplateAsync(Guid templateId, Guid productId);

    /// <summary>
    /// 校验工艺路线完整性
    /// </summary>
    Task<bool> ValidateProcessRouteAsync(ProcessRoute processRoute, out string errorMessage);

    /// <summary>
    /// 克隆为新版本
    /// </summary>
    Task<ProcessRoute> CloneAsNewVersionAsync(Guid processRouteId, string newVersionNo);
}