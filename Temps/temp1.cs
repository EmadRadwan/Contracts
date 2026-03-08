if (existingReporting != null)
{
    _context.EmplPositionReportingStructs.Remove(existingReporting);
}

if (managerPosition != null)  // only if we actually have a new manager
{
    var newStruct = new EmplPositionReportingStruct
    {
        EmplPositionIdManagedBy  = currentPosition.EmplPositionId,
        EmplPositionIdReportingTo = managerPosition.EmplPositionId,
        FromDate                 = stamp,
        CreatedStamp             = stamp,
        LastUpdatedStamp         = stamp
    };
    _context.EmplPositionReportingStructs.Add(newStruct);
}