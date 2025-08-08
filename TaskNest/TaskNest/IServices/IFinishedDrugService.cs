using TaskNest.Frontend.Models;
using TaskNest.Models;

namespace TaskNest.IServices
{
    public interface IFinishedDrugService
    {
        public Task<Object> AddNewFinishedDrug(FinishedDrugInfo finishedDrugInfo);
        public Task<List<FinishedDrugInfo>> GetAllFinishedDrugs();
        public Task<FinishedDrug> GetFinishedDrugById(string Id);
        public Task<Object> UpdateFinishedDrug(string Id, FinishedDrugInfo finishedDrugUpdatedValues);
        public Task<Object> UpdateFinishedDrugInventory(string Id, InventoryUpdate finishedDrugUpdatedValues);
        public Task<object> DeleteFinishedDrug(string finishedDrugId);
    }
}
