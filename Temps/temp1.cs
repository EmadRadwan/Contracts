public async Task<Result<PartyDto2>> Handle(Command request, CancellationToken cancellationToken)
{
    var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    var stamp = DateTime.UtcNow;

    try
    {
        // ────────────────────────────────────────────────────────────────
        // 1. Your existing Party / Person / Role / Status / Contacts creation
        //    (keep exactly as is — I'm only showing the additions below)
        // ────────────────────────────────────────────────────────────────

        var newPartyId = await _utilityService.GetNextSequence("Party");
        var party = new Party { /* ... your code ... */ };
        _context.Parties.Add(party);

        var person = new Person { /* ... */ };
        _context.Persons.Add(person);

        var partyRole = new PartyRole { /* EMPLOYEE */ };
        _context.PartyRoles.Add(partyRole);

        var partyStatus = new PartyStatus { /* ... */ };
        _context.PartyStatuses.Add(partyStatus);

        // contacts (mobile, email, address) — your existing code

        // ────────────────────────────────────────────────────────────────
        // 2. NEW: Employment
        // ────────────────────────────────────────────────────────────────
        var employment = new Employment
        {
            PartyIdFrom = "Company",
            PartyIdTo = newPartyId,
            FromDate = request.PartyDto.EmploymentStartDate ?? stamp,
            ThruDate = null,
            RoleTypeIdFrom = "INTERNAL_ORGANIZATIO",
            RoleTypeIdTo = "EMPLOYEE",
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.Employments.Add(employment);

        // ────────────────────────────────────────────────────────────────
        // 3. NEW: Position Fulfillment
        // ────────────────────────────────────────────────────────────────
        string positionId;

        if (!string.IsNullOrEmpty(request.PartyDto.SpecificEmplPositionId))
        {
            // Use existing position
            positionId = request.PartyDto.SpecificEmplPositionId;

            // Optional: validate it exists
            var posExists = await _context.EmplPositions
                .AnyAsync(p => p.EmplPositionId == positionId, cancellationToken);
            if (!posExists)
                return Result<PartyDto2>.Failure($"Position {positionId} not found.");
        }
        else
        {
            // Auto-create simple position from type
            if (string.IsNullOrEmpty(request.PartyDto.PositionTypeId))
                return Result<PartyDto2>.Failure("PositionTypeId is required.");

            // Validate type exists
            var typeExists = await _context.EmplPositionTypes
                .AnyAsync(t => t.EmplPositionTypeId == request.PartyDto.PositionTypeId, cancellationToken);
            if (!typeExists)
                return Result<PartyDto2>.Failure($"Position type {request.PartyDto.PositionTypeId} not found.");

            positionId = await _utilityService.GetNextSequence("EmplPosition"); // or custom naming

            var newPosition = new EmplPosition
            {
                EmplPositionId = positionId,
                StatusId = "EMPL_POS_ACTIVE",
                PartyId = "Company",
                EmplPositionTypeId = request.PartyDto.PositionTypeId,
                SalaryFlag = request.PartyDto.SalaryFlag ?? "Y",
                ExemptFlag = request.PartyDto.ExemptFlag ?? "Y",
                FulltimeFlag = request.PartyDto.FulltimeFlag ?? "Y",
                TemporaryFlag = request.PartyDto.TemporaryFlag ?? "N",
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.EmplPositions.Add(newPosition);
        }

        var fulfillment = new EmplPositionFulfillment
        {
            EmplPositionId = positionId,
            PartyId = newPartyId,
            FromDate = request.PartyDto.EmploymentStartDate ?? stamp,
            ThruDate = null,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.EmplPositionFulfillments.Add(fulfillment);

        // ────────────────────────────────────────────────────────────────
        // 4. NEW: Personal RateAmount (monthly override)
        // ────────────────────────────────────────────────────────────────
        if (request.PartyDto.MonthlyBaseSalary.HasValue && request.PartyDto.MonthlyBaseSalary > 0)
        {
            var rate = new RateAmount
            {
                RateTypeId = "AVERAGE_PAY_RATE",           // or your preferred type
                RateCurrencyUomId = request.PartyDto.CurrencyUomId ?? "EGP",
                PeriodTypeId = "RATE_MONTH",
                WorkEffortId = "_NA_",
                PartyId = newPartyId,                       // personal
                EmplPositionTypeId = "_NA_",
                FromDate = stamp,
                RateAmount = request.PartyDto.MonthlyBaseSalary.Value,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.RateAmounts.Add(rate);
        }

        // ────────────────────────────────────────────────────────────────
        // Save & Commit
        // ────────────────────────────────────────────────────────────────
        var success = await _context.SaveChangesAsync(cancellationToken) > 0;
        if (!success)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<PartyDto2>.Failure("Failed to save employee data");
        }

        await transaction.CommitAsync(cancellationToken);

        // Return success
        return Result<PartyDto2>.Success(new PartyDto2
        {
            PartyId = newPartyId,
            Description = $"{request.PartyDto.FirstName} (EMPLOYEE)",
            // ... fill other needed fields
        });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        return Result<PartyDto2>.Failure(ex.Message);
    }
}