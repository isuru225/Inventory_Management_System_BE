﻿using TaskNest.Frontend.Models;
using TaskNest.Models;

namespace TaskNest.IServices
{
    public interface IRawDrugService
    {
        public Task<Object> AddNewRawDrug(RawDrugInfo rawDrugInfo);
        public Task<List<RawDrugInfo>> GetAllRawDrugs();
        public Task<Object> UpdateRawDrug(string Id, Dictionary<string, object> rawDrugUpdatedValues);
        public Task<RawDrug> GetRawDrugById(string Id);
        public Task<object> DeleteRawDrug(string rawDrugId);
    }
}
