```csharp
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Threading.Tasks;

namespace Application.Catalog.Products.Services.Cost
{
    public class CostService : ICostService
    {
        private readonly DataContext _context;

        public CostService(DataContext context)
        {
            _context = context;
        }

        public async Task<string> CreateWorkEffortCostCalc(WorkEffortCostCalc workEffortCostCalc)
        {
            // REFACTOR: Validate input to prevent null entity and required fields
            if (workEffortCostCalc == null)
            {
                throw new ArgumentNullException(nameof(workEffortCostCalc), "WorkEffortCostCalc entity cannot be null");
            }
            if (string.IsNullOrEmpty(workEffortCostCalc.WorkEffortId))
            {
                throw new ArgumentException("WorkEffortId is required", nameof(workEffortCostCalc.WorkEffortId));
            }
            if (string.IsNullOrEmpty(workEffortCostCalc.CostComponentCalcId))
            {
                throw new ArgumentException("CostComponentCalcId is required", nameof(workEffortCostCalc.CostComponentCalcId));
            }
            try
            {
                await _context.WorkEffortCostCalcs.AddAsync(workEffortCostCalc);
                return workEffortCostCalc.WorkEffortId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> UpdateWorkEffortCostCalc(WorkEffortCostCalc workEffortCostCalc)
        {
            // REFACTOR: Validate input to prevent null entity and required fields
            // Purpose: Ensures the record can be identified for update
            if (workEffortCostCalc == null)
            {
                throw new ArgumentNullException(nameof(workEffortCostCalc), "WorkEffortCostCalc entity cannot be null");
            }
            if (string.IsNullOrEmpty(workEffortCostCalc.WorkEffortId))
            {
                throw new ArgumentException("WorkEffortId is required", nameof(workEffortCostCalc.WorkEffortId));
            }
            if (string.IsNullOrEmpty(workEffortCostCalc.CostComponentCalcId))
            {
                throw new ArgumentException("CostComponentCalcId is required", nameof(workEffortCostCalc.CostComponentCalcId));
            }
            if (string.IsNullOrEmpty(workEffortCostCalc.CostComponentTypeId))
            {
                throw new ArgumentException("CostComponentTypeId is required", nameof(workEffortCostCalc.CostComponentTypeId));
            }
            if (workEffortCostCalc.FromDate == default)
            {
                throw new ArgumentException("FromDate is required", nameof(workEffortCostCalc.FromDate));
            }

            try
            {
                // REFACTOR: Truncate FromDate to date part for comparison
                // Purpose: Matches records ignoring the time component using MySQL DATE()
                var truncatedFromDate = workEffortCostCalc.FromDate.Date;

                // REFACTOR: Find the existing record using MySQL DATE() function
                // Purpose: Ensures the correct record is targeted for update
                var existingRecord = await _context.WorkEffortCostCalcs
                    .FirstOrDefaultAsync(x =>
                        x.WorkEffortId == workEffortCostCalc.WorkEffortId &&
                        x.CostComponentCalcId == workEffortCostCalc.CostComponentCalcId &&
                        x.CostComponentTypeId == workEffortCostCalc.CostComponentTypeId &&
                        EF.Functions.MySqlDate(x.FromDate) == truncatedFromDate);

                if (existingRecord == null)
                {
                    throw new InvalidOperationException("WorkEffortCostCalc record not found for the provided key.");
                }

                // REFACTOR: Update the existing record with new values
                // Purpose: Ensures only the provided fields are updated, maintaining data integrity
                existingRecord.CostComponentTypeId = workEffortCostCalc.CostComponentTypeId;
                existingRecord.CostComponentCalcId = workEffortCostCalc.CostComponentCalcId;
                existingRecord.FromDate = workEffortCostCalc.FromDate;
                existingRecord.ThruDate = workEffortCostCalc.ThruDate;

                _context.WorkEffortCostCalcs.Update(existingRecord);
                return workEffortCostCalc.WorkEffortId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> DeleteWorkEffortCostCalc(WorkEffortCostCalc workEffortCostCalc)
        {
            // REFACTOR: Validate input to prevent null entity and required fields
            // Purpose: Ensures the record can be identified for deletion
            if (workEffortCostCalc == null)
            {
                throw new ArgumentNullException(nameof(workEffortCostCalc), "WorkEffortCostCalc entity cannot be null");
            }
            if (string.IsNullOrEmpty(workEffortCostCalc.WorkEffortId))
            {
                throw new ArgumentException("WorkEffortId is required", nameof(workEffortCostCalc.WorkEffortId));
            }
            if (string.IsNullOrEmpty(workEffortCostCalc.CostComponentCalcId))
            {
                throw new ArgumentException("CostComponentCalcId is required", nameof(workEffortCostCalc.CostComponentCalcId));
            }
            if (string.IsNullOrEmpty(workEffortCostCalc.CostComponentTypeId))
            {
                throw new ArgumentException("CostComponentTypeId is required", nameof(workEffortCostCalc.CostComponentTypeId));
            }
            if (workEffortCostCalc.FromDate == default)
            {
                throw new ArgumentException("FromDate is required", nameof(workEffortCostCalc.FromDate));
            }

            try
            {
                // REFACTOR: Truncate FromDate to date part for comparison
                // Purpose: Matches records ignoring the time component using MySQL DATE()
                var truncatedFromDate = workEffortCostCalc.FromDate.Date;

                // REFACTOR: Find the existing record using MySQL DATE() function
                // Purpose: Ensures the correct record is targeted for deletion
                var existingRecord = await _context.WorkEffortCostCalcs
                    .FirstOrDefaultAsync(x =>
                        x.WorkEffortId == workEffortCostCalc.WorkEffortId &&
                        x.CostComponentCalcId == workEffortCostCalc.CostComponentCalcId &&
                        x.CostComponentTypeId == workEffortCostCalc.CostComponentTypeId &&
                        EF.Functions.MySqlDate(x.FromDate) == truncatedFromDate);

                if (existingRecord == null)
                {
                    throw new InvalidOperationException("WorkEffortCostCalc record not found for the provided key.");
                }

                // REFACTOR: Remove the record from DbContext
                // Purpose: Marks the entity for deletion in the next SaveChanges call
                _context.WorkEffortCostCalcs.Remove(existingRecord);

                return workEffortCostCalc.WorkEffortId; // Return WorkEffortId as primary identifier
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
```