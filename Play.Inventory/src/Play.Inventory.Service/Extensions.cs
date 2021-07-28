using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service
{
    public static class Extensions
    {
        public static InventoryItemDto AsDto(this InventoryItem inventoryItem)
        {
            return new InventoryItemDto(inventoryItem.CatalogItemId,
                                        inventoryItem.Quantity,
                                        inventoryItem.AcquiredDate);
        }
    }
}