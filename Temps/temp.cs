using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SeedData
{
    public static async Task Initialize(MyDbContext context)
    {
        if (!context.Facilities.Any() && !context.WorkEfforts.Any())
        {
            var projectNames = new List<string>
            {
                "مول الصحراوى 2 فدان",
                "الصحراوى 10.5 فدان",
                "الصحراوى 3 فدان",
                "الصحراوى 4 فدان",
                "الثروة الخضراء - علاء العمدة",
                "الثروة الخضراء - سعودى",
                "الثروة الخضراء - نسيم",
                "الثالث زايد",
                "السابع زايد",
                "التاسع زايد",
                "بيت الوطن لادريس أكتوبر",
                "بيت الوطن لادريس التجمع"
            };

            var subProjects = new List<string>
            {
                "مبنى الادارة",
                "اسطبل الخيول",
                "قرية الماكولات"
            };

            var glAccountMapping = new Dictionary<string, string>
            {
                { "مول الصحراوى 2 فدان", "124424" },
                { "الصحراوى 10.5 فدان", "124425" },
                { "الصحراوى 3 فدان", "124426" },
                { "الصحراوى 4 فدان", "124427" },
                { "الثروة الخضراء - علاء العمدة", "124428" },
                { "الثروة الخضراء - سعودى", "124429" },
                { "الثروة الخضراء - نسيم", "124430" },
                { "الثالث زايد", "124431" },
                { "السابع زايد", "124432" },
                { "التاسع زايد", "124433" },
                { "بيت الوطن لادريس أكتوبر", "124422" },
                { "بيت الوطن لادريس التجمع", "124421" }
            };

            var stamp = DateTime.UtcNow;
            var counter = 100;

            foreach (var projectName in projectNames)
            {
                var newProjectSerial = counter.ToString();
                counter++; // Increment counter for the next project

                var facility = CreateFacility(projectName, stamp);
                context.Facilities.Add(facility);

                var project = new WorkEffort
                {
                    WorkEffortId = newProjectSerial,
                    ProjectName = projectName,
                    WorkEffortTypeId = "PROJECT",
                    CurrentStatusId = "WEPR_IN_PROGRESS", // Default status as placeholder
                    EstimatedStartDate = stamp, // Default start date
                    EstimatedCompletionDate = stamp.AddDays(30), // Default completion date
                    CreatedDate = stamp,
                    LastUpdatedStamp = stamp,
                    FacilityId = facility.FacilityId,
                    GlAccountId = glAccountMapping.ContainsKey(projectName) ? glAccountMapping[projectName] : null
                };
                context.WorkEfforts.Add(project);

                if (projectName == "الصحراوى 10.5 فدان")
                {
                    foreach (var subProjectName in subProjects)
                    {
                        var subProjectSerial = counter.ToString();
                        counter++; // Increment counter for the next sub-project

                        var subProject = new WorkEffort
                        {
                            WorkEffortId = subProjectSerial,
                            SubProjectName = subProjectName,
                            WorkEffortTypeId = "SUB_PROJECT",
                            CurrentStatusId = "WEPR_IN_PROGRESS",
                            EstimatedStartDate = stamp,
                            EstimatedCompletionDate = stamp.AddDays(30),
                            CreatedDate = stamp,
                            LastUpdatedStamp = stamp,
                            FacilityId = facility.FacilityId,
                            ProjectId = newProjectSerial, // Link to parent project
                            GlAccountId = glAccountMapping.ContainsKey(projectName) ? glAccountMapping[projectName] : null
                        };
                        context.WorkEfforts.Add(subProject);
                    }
                }
            }

            var mainStore = new Facility
            {
                FacilityId = "b6705327-bb0b-421f-9a1e-e94bbf7a68d2",
                FacilityTypeId = "MAIN_STORE",
                FacilityName = "المخزن الرءيسى",
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            context.Facilities.Add(mainStore);

            await context.SaveChangesAsync();
        }
    }

    private static Facility CreateFacility(string projectName, DateTime stamp)
    {
        return new Facility
        {
            FacilityId = Guid.NewGuid().ToString(),
            FacilityTypeId = "PROJECT_FACILITY",
            FacilityName = projectName,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
    }
}