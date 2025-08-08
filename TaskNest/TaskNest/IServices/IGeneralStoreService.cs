using TaskNest.Frontend.Models;
using TaskNest.Models;

namespace TaskNest.IServices
{
    public interface IGeneralStoreService
    {
        public Task<Object> AddNewGeneralStoreItem(GeneralStoreItemInfo generalStoreItemInfo);
        public Task<List<GeneralStoreItemInfo>> GetAllGeneralStoreItems();
        public Task<GeneralStoreItem> GetGeneralStoreItemById(string Id);
        public Task<Object> UpdateGeneralStoreItem(string Id, GeneralStoreItemInfo generalStoreItemUpdatedValues);
        public Task<Object> UpdateGeneralStoreInventory(string Id, InventoryUpdate generalStoreItemUpdatedValues);
        public Task<object> DeleteGeneralStoreItemDrug(string generalStoreItemId);


    }
}
