using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AbpDemo.Domain.Shared.ValueObjects;
using AbpDemo.Engineering.Products.Dtos;
using AbpDemo.Enums;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace AbpDemo.Engineering.Products;

public class ProductAppService : AbpDemoAppService, IProductAppService
{
    private readonly IRepository<Product, Guid> _productRepository;

    public ProductAppService(IRepository<Product, Guid> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResultDto<ProductDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _productRepository.GetQueryableAsync();
        var query = queryable
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "CreationTime DESC" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var products = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<ProductDto>(
            totalCount,
            products.Select(p => ObjectMapper.Map<Product, ProductDto>(p)).ToList()
        );
    }

    public async Task<ProductDto> GetAsync(Guid id)
    {
        var queryable = await _productRepository.GetQueryableAsync();
        // 使用 Include 加载产品版本和 BOM 子实体
        var product = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(p => p.Id == id)
                .Include(p => p.Versions)
                .ThenInclude(v => v.BomItems)
        );

        if (product == null) throw new EntityNotFoundException(typeof(Product), id);
        return ObjectMapper.Map<Product, ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest input)
    {
        // 检查编码唯一性
        if (await IsCodeUniqueAsync(input.ProductCode))
        {
            throw new UserFriendlyException("产品编码已存在");
        }

        var product = new Product(
            GuidGenerator.Create(),
            input.ProductCode,
            input.ProductName,
            input.Type
        );

        product.SetMaterial(input.Material);
        product.SetUnit(input.Unit);

        if (input.Specification != null)
        {
            var spec = new ProductSpecification(
                input.Specification.Length,
                input.Specification.Width,
                input.Specification.Height,
                input.Specification.Thickness,
                input.Specification.Weight,
                input.Specification.CustomSpecs
            );
            product.SetSpecification(spec);
        }

        await _productRepository.InsertAsync(product);
        return ObjectMapper.Map<Product, ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest input)
    {
        var product = await _productRepository.GetAsync(id);

        product.SetProductName(input.ProductName);
        product.SetType(input.Type);
        product.SetMaterial(input.Material);
        product.SetUnit(input.Unit);
        product.SetActive(input.IsActive);

        if (input.Specification != null)
        {
            var spec = new ProductSpecification(
                input.Specification.Length,
                input.Specification.Width,
                input.Specification.Height,
                input.Specification.Thickness,
                input.Specification.Weight,
                input.Specification.CustomSpecs
            );
            product.SetSpecification(spec);
        }

        await _productRepository.UpdateAsync(product);
        return ObjectMapper.Map<Product, ProductDto>(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _productRepository.DeleteAsync(id);
    }

    public async Task AddBomItemAsync(Guid productId, CreateBomItemRequest input)
    {
        var product = await _productRepository.GetAsync(productId);

        // 获取当前生效的 MBOM 版本，如果没有则创建一个
        var version = product.GetActiveVersion(BomType.MBOM);
        if (version == null)
        {
            version = product.CreateNewVersion(BomType.MBOM, "初始版本");
        }

        // 查询组件产品名称（冗余字段）
        var queryable = await _productRepository.GetQueryableAsync();
        var componentProduct = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(p => p.Id == input.ComponentProductId)
        );
        if (componentProduct == null)
        {
            throw new EntityNotFoundException(typeof(Product), input.ComponentProductId);
        }

        // 在版本上添加 BOM 项
        version.AddBomItem(
            input.ComponentProductId,
            componentProduct.ProductName, // 冗余字段
            input.Quantity,
            input.ScrapRate,
            input.Unit,
            input.Sequence,
            input.ParentItemId
        );

        await _productRepository.UpdateAsync(product);
    }

    public async Task DeleteBomItemAsync(Guid productId, Guid bomItemId)
    {
        var product = await _productRepository.GetAsync(productId);

        // 查找包含该 BOM 项的版本
        ProductVersion? targetVersion = null;
        foreach (var version in product.Versions)
        {
            if (version.BomItems.Any(i => i.Id == bomItemId))
            {
                targetVersion = version;
                break;
            }
        }

        if (targetVersion != null)
        {
            targetVersion.RemoveBomItem(bomItemId);
            await _productRepository.UpdateAsync(product);
        }
    }

    public async Task UpdateBomItemAsync(Guid productId, Guid versionId, Guid bomItemId, UpdateBomItemRequest input)
    {
        var product = await _productRepository.GetAsync(productId);

        // 查找指定版本
        var version = product.Versions.FirstOrDefault(v => v.Id == versionId)
                      ?? throw new EntityNotFoundException(typeof(ProductVersion), versionId);

        // 查找 BOM 项并更新
        var item = version.BomItems.FirstOrDefault(i => i.Id == bomItemId)
                   ?? throw new EntityNotFoundException(typeof(BomItem), bomItemId);

        // 使用反射或重新创建的方式更新（因为 BomItem 是子实体，没有公开 setter）
        // 这里采用先删除再添加的方式

        // 查询组件产品名称（冗余字段）
        var queryable = await _productRepository.GetQueryableAsync();
        var componentProduct = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(p => p.Id == input.ComponentProductId)
        );
        if (componentProduct == null)
        {
            throw new EntityNotFoundException(typeof(Product), input.ComponentProductId);
        }

        version.RemoveBomItem(bomItemId);
        version.AddBomItem(
            input.ComponentProductId,
            componentProduct.ProductName, // 冗余字段
            input.Quantity,
            input.ScrapRate,
            input.Unit,
            input.Sequence,
            input.ParentItemId
        );

        await _productRepository.UpdateAsync(product);
    }

    public async Task ReplaceBomItemsAsync(Guid productId, Guid versionId, ReplaceBomItemsRequest input)
    {
        var product = await _productRepository.GetAsync(productId);

        // 查找指定版本
        var version = product.Versions.FirstOrDefault(v => v.Id == versionId)
                      ?? throw new EntityNotFoundException(typeof(ProductVersion), versionId);

        // 清空旧 BOM 项（通过移除所有项）
        var itemsToRemove = version.BomItems.Select(i => i.Id).ToList();
        foreach (var itemId in itemsToRemove)
        {
            version.RemoveBomItem(itemId);
        }

        // 批量添加新 BOM 项
        foreach (var itemDto in input.Items)
        {
            // 查询组件产品名称（冗余字段）
            var queryable = await _productRepository.GetQueryableAsync();
            var componentProduct = await AsyncExecuter.FirstOrDefaultAsync(
                queryable.Where(p => p.Id == itemDto.ComponentProductId)
            );
            if (componentProduct == null)
            {
                throw new EntityNotFoundException(typeof(Product), itemDto.ComponentProductId);
            }

            version.AddBomItem(
                itemDto.ComponentProductId,
                componentProduct.ProductName, // 冗余字段
                itemDto.Quantity,
                itemDto.ScrapRate,
                itemDto.Unit,
                itemDto.Sequence,
                itemDto.ParentItemId
            );
        }

        await _productRepository.UpdateAsync(product);
    }

    public async Task<ProductVersionDto> GetVersionWithBomItemsAsync(Guid productId, Guid versionId)
    {
        var queryable = await _productRepository.GetQueryableAsync();
        var product = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(p => p.Id == productId)
                .Include(p => p.Versions)
                .ThenInclude(v => v.BomItems)
        );

        if (product == null) throw new EntityNotFoundException(typeof(Product), productId);

        var version = product.Versions.FirstOrDefault(v => v.Id == versionId)
                      ?? throw new EntityNotFoundException(typeof(ProductVersion), versionId);

        // 映射 DTO（ComponentProductName 已经从领域层冗余，无需二次查询）
        var dto = ObjectMapper.Map<ProductVersion, ProductVersionDto>(version);

        return dto;
    }

    public async Task UpdateBomVersionAsync(Guid productId, Guid versionId, UpdateProductVersionRequest input)
    {
        var product = await _productRepository.GetAsync(productId);

        var version = product.Versions.FirstOrDefault(v => v.Id == versionId)
                      ?? throw new EntityNotFoundException(typeof(ProductVersion), versionId);

        // 更新版本属性
        version.SetVersionNo(input.VersionNo);
        version.SetBomType(input.BomType);
        version.SetChangeReason(input.ChangeReason);

        if (input.IsActive)
        {
            version.Activate();
        }
        else
        {
            version.Deactivate();
        }

        await _productRepository.UpdateAsync(product);
    }

    public async Task DeleteBomVersionAsync(Guid productId, Guid versionId)
    {
        var product = await _productRepository.GetAsync(productId);

        var version = product.Versions.FirstOrDefault(v => v.Id == versionId)
                      ?? throw new EntityNotFoundException(typeof(ProductVersion), versionId);

        // 从产品中移除版本
        product.RemoveVersion(versionId);

        await _productRepository.UpdateAsync(product);
    }

    public async Task CreateNewBomVersionAsync(Guid productId, string reason)
    {
        var product = await _productRepository.GetAsync(productId);
        product.CreateNewVersion(BomType.MBOM, reason);
        await _productRepository.UpdateAsync(product);
    }

    private async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var queryable = await _productRepository.GetQueryableAsync();
        var query = queryable.Where(p => p.ProductCode == code);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await AsyncExecuter.CountAsync(query) > 0;
    }
}