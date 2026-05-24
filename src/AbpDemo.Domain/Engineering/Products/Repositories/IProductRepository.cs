using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbpDemo.Engineering.Products.Aggregates;
using AbpDemo.Enums;
using Volo.Abp.Domain.Repositories;

namespace AbpDemo.Engineering.Products.Repositories;

public interface IProductRepository : IRepository<Product, Guid>
{
    Task<Product> GetWithVersionsAsync(Guid id); // 包含产品版本集合
    Task<Product> GetVersionWithBomItemsAsync(Guid versionId); // 获取指定版本的完整BOM
    Task<Product> FindByCodeAsync(string productCode);
    Task<List<Product>> GetByTypeAsync(ProductType type);
    Task<bool> IsCodeUniqueAsync(string productCode, Guid? excludeId = null);
}