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
    fac.FACILITY_NAME                                   AS FacilityName,

    -- -----------------------------------------------------------------
    -- Ownership: is this the company's own project, or work the company
    -- is doing for someone else? Source: WORK_EFFORT.IS_COMPANY_PROJECT
    -- (tinyint(1), nullable — added 2026-08-17, backfilled by
    --  API/Sql/Localhost/set_is_company_project_flags.sql).
    --
    -- NULL is treated as 0 ("work for others"): that matches the default
    -- of the ProjectForm checkbox and keeps legacy rows that predate the
    -- column out of the company-project figures rather than silently
    -- inflating them.
    --
    -- NOTE for Power BI: COALESCE strips the tinyint(1) display width, so
    -- the MySQL connector surfaces this as a Whole Number (0/1), NOT as
    -- True/False. Write DAX as [IsCompanyProject] = 1, not = TRUE().
    -- -----------------------------------------------------------------
    COALESCE(we.IS_COMPANY_PROJECT, 0)                  AS IsCompanyProject,       -- 1 = company's own, 0 = for others

    -- Ready-made slicer / legend labels so the report doesn't have to
    -- build them in DAX (same pattern as DimApartments status labels)
    CASE WHEN COALESCE(we.IS_COMPANY_PROJECT, 0) = 1
             THEN 'مشاريع الشركة'
         ELSE 'أعمال للغير'
        END                                             AS ProjectOwnership,

    CASE WHEN COALESCE(we.IS_COMPANY_PROJECT, 0) = 1
             THEN 'Company Project'
         ELSE 'Work for Others'
        END                                             AS ProjectOwnershipEnglish

FROM WORK_EFFORT we
         LEFT JOIN FACILITY fac
                   ON we.FACILITY_ID = fac.FACILITY_ID
         LEFT JOIN WORK_EFFORT parent_we
                   ON we.WORK_EFFORT_PARENT_ID = parent_we.WORK_EFFORT_ID
                       AND parent_we.WORK_EFFORT_TYPE_ID = 'PROJECT'

WHERE we.WORK_EFFORT_TYPE_ID = 'PROJECT';