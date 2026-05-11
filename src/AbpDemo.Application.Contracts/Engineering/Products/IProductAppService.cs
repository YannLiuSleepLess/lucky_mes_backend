using System;
using System.Threading.Tasks;
using AbpDemo.Engineering.Products.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AbpDemo.Engineering.Products;

public interface IProductAppService : IApplicationService
{
    Task<PagedResultDto<ProductDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<ProductDto> GetAsync(Guid id);
    Task<ProductDto> CreateAsync(CreateProductRequest input);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest input);
    Task DeleteAsync(Guid id);

    // BOM 版本管理相关接口
    Task AddBomItemAsync(Guid productId, CreateBomItemRequest input);
    Task UpdateBomItemAsync(Guid productId, Guid versionId, Guid bomItemId, UpdateBomItemRequest input);
    Task DeleteBomItemAsync(Guid productId, Guid bomItemId);
    Task ReplaceBomItemsAsync(Guid productId, Guid versionId, ReplaceBomItemsRequest input); // 批量替换整个版本的 BOM

    Task<ProductVersionDto> GetVersionWithBomItemsAsync(Guid productId, Guid versionId);
    Task CreateNewBomVersionAsync(Guid productId, string reason);
    Task UpdateBomVersionAsync(Guid productId, Guid versionId, UpdateProductVersionRequest input);
    Task DeleteBomVersionAsync(Guid productId, Guid versionId);
}