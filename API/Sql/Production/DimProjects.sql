-- =============================================================
-- DIM PROJECTS – Clean dimension table (Power BI DimProjects)
-- =============================================================
DROP VIEW IF EXISTS DimProjects;
CREATE OR REPLACE VIEW DimProjects AS
SELECT
    -- Surrogate key – unique identifier for relationships
    we.WORK_EFFORT_ID                                   AS ProjectId,              -- KEY

    -- Main display name – this is what users see
    we.PROJECT_NAME                                     AS ProjectName,            -- e.g. "الصحراوى 3 فدان"

    -- Status
    CASE we.CURRENT_STATUS_ID
        WHEN 'WEPR_CREATED'     THEN 'Created'
        WHEN 'WEPR_IN_PROGRESS' THEN 'In Progress'
        WHEN 'WEPR_COMPLETE'    THEN 'Completed'
        WHEN 'WEPR_CANCELLED'   THEN 'Cancelled'
        WHEN 'WEPR_ON_HOLD'     THEN 'On Hold'
        ELSE 'Unknown'
        END                                                 AS StatusName,

    -- Dates
    we.ESTIMATED_START_DATE                             AS PlannedStartDate,
    we.ESTIMATED_COMPLETION_DATE                        AS PlannedEndDate,

    -- Facility / Site
    we.FACILITY_ID                                      AS FacilityId,
    fac.FACILITY_NAME                                   AS FacilityName



FROM WORK_EFFORT we
         LEFT JOIN FACILITY fac
                   ON we.FACILITY_ID = fac.FACILITY_ID
         LEFT JOIN WORK_EFFORT parent_we
                   ON we.WORK_EFFORT_PARENT_ID = parent_we.WORK_EFFORT_ID
                       AND parent_we.WORK_EFFORT_TYPE_ID = 'PROJECT'

WHERE we.WORK_EFFORT_TYPE_ID = 'PROJECT';