using System.Collections.Generic;

namespace AbpDemo.Engineering.Products.Dtos;

public class ReplaceBomItemsRequest
{
    public List<CreateBomItemRequest> Items { get; set; } = new List<CreateBomItemRequest>();
}