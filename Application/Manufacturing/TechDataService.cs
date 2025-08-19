using API.Middleware;
using Application.Common;
using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using InvalidOperationException = System.InvalidOperationException;

namespace Application.Manufacturing
{
    public interface ITechDataService
    {
        Task<DateTime> AddForward(TechDataCalendar techDataCalendar, DateTime dateFrom, long amount);
        Task<TechDataCalendar> GetTechDataCalendar(WorkEffort routingTask);
    }

    public class TechDataService : ITechDataService
    {
        private readonly DataContext _context;
        private readonly ILogger _logger;

        public TechDataService(DataContext context, ILogger<TechDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DateTime> AddForward(TechDataCalendar techDataCalendar, DateTime dateFrom, long amount)
        {
            DateTime dateTo = dateFrom;
            long nextCapacity = 0;

            try
            {
                nextCapacity = await CapacityRemaining(techDataCalendar, dateFrom);
            }
            catch (Exception ex)
            {
                // Log the error and handle the exception
                Console.Error.WriteLine($"Error retrieving capacity remaining for the start date: {ex.Message}");
                throw new ServiceException("Error retrieving capacity remaining for the start date.", ex);
            }

            if (amount <= nextCapacity)
            {
                dateTo = dateTo.AddMilliseconds(amount);
                amount = 0;
            }
            else
            {
                amount -= nextCapacity;
            }

            while (amount > 0)
            {
                try
                {
                    var result = await StartNextDay(techDataCalendar, dateTo);
                    dateTo = result.DateTo;
                    nextCapacity = (long)result.NextCapacity; // Ensure casting to long

                    if (amount <= nextCapacity)
                    {
                        dateTo = dateTo.AddMilliseconds(amount);
                        amount = 0;
                    }
                    else
                    {
                        amount -= nextCapacity;
                    }
                }
                catch (Exception ex)
                {
                    // Log the error and handle the exception
                    Console.Error.WriteLine($"Error starting next day in the calendar: {ex.Message}");
                    throw new ServiceException("Error starting next day in the calendar.", ex);
                }
            }

            return dateTo;
        }

        public async Task<TechDataCalendar> GetTechDataCalendar(WorkEffort routingTask)
        {
            FixedAsset machineGroup = null;
            TechDataCalendar techDataCalendar = null;

            try
            {
                machineGroup = await _context.FixedAssets
                    .FindAsync(routingTask.FixedAssetId);
            }
            catch (Exception ex)
            {
                // Log the error or handle it accordingly
                Console.Error.WriteLine($"Error retrieving FixedAsset for routingTask: {ex.Message}");
                throw new ServiceException("Error retrieving FixedAsset for routingTask.", ex);
            }

            if (machineGroup != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(machineGroup.CalendarId))
                    {
                        try
                        {
                            techDataCalendar = await _context.TechDataCalendars
                                .FindAsync(machineGroup.CalendarId);
                        }
                        catch (Exception ex)
                        {
                            // Log the error for TechDataCalendar retrieval
                            Console.Error.WriteLine(
                                $"Error retrieving TechDataCalendar for machineGroup: {ex.Message}");
                            throw new ServiceException("Error retrieving TechDataCalendar for machineGroup.", ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            var machines = await _context.FixedAssets
                                .Where(cf => cf.ParentFixedAssetId == machineGroup.FixedAssetId)
                                .ToListAsync();

                            if (machines.Any())
                            {
                                var machine = machines.First();
                                try
                                {
                                    techDataCalendar = await _context.TechDataCalendars
                                        .FindAsync(machine.CalendarId);
                                }
                                catch (Exception ex)
                                {
                                    // Log the error for child machine's TechDataCalendar retrieval
                                    Console.Error.WriteLine(
                                        $"Error retrieving TechDataCalendar for child machine: {ex.Message}");
                                    throw new ServiceException("Error retrieving TechDataCalendar for child machine.",
                                        ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log the error for child machine retrieval
                            Console.Error.WriteLine($"Error retrieving child machines for machineGroup: {ex.Message}");
                            throw new ServiceException("Error retrieving child machines for machineGroup.", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error for machineGroup handling
                    Console.Error.WriteLine($"Error handling machineGroup logic: {ex.Message}");
                    throw new ServiceException("Error handling machineGroup logic.", ex);
                }
            }

            if (techDataCalendar == null)
            {
                try
                {
                    techDataCalendar = await _context.TechDataCalendars
                        .FindAsync("DEFAULT");
                }
                catch (Exception ex)
                {
                    // Log the error for default TechDataCalendar retrieval
                    Console.Error.WriteLine($"Error retrieving default TechDataCalendar: {ex.Message}");
                    throw new ServiceException("Error retrieving default TechDataCalendar.", ex);
                }
            }

            return techDataCalendar;
        }

        public async Task<long> CapacityRemaining(TechDataCalendar techDataCalendar, DateTime dateFrom)
        {
            TechDataCalendarWeek techDataCalendarWeek = null;

            try
            {
                // Use FindAsync to retrieve the TechDataCalendarWeek
                techDataCalendarWeek = await _context.TechDataCalendarWeeks
                    .FindAsync(techDataCalendar.CalendarId);
            }
            catch (Exception ex)
            {
                // Log the error and handle the exception
                Console.Error.WriteLine($"Error reading Calendar Week associated with calendar: {ex.Message}");
                throw new ServiceException("Error reading Calendar Week associated with calendar.", ex);
            }

            if (techDataCalendarWeek == null)
            {
                Console.Error.WriteLine("Error reading Calendar Week associated with calendar");
                return 0;
            }

            // Convert dateFrom to a DateTime instance
            var cDateTrav = dateFrom;

            // Get the day of the week as an integer (Sunday = 0, Monday = 1, ..., Saturday = 6)
            int dayOfWeek = (int)cDateTrav.DayOfWeek;

            DayStartCapacityResult position;

            try
            {
                position = await DayStartCapacityAvailable(techDataCalendarWeek, dayOfWeek);
            }
            catch (Exception ex)
            {
                // Log the error and handle the exception
                Console.Error.WriteLine($"Error retrieving start capacity for day {dayOfWeek}: {ex.Message}");
                throw new ServiceException($"Error retrieving start capacity for day {dayOfWeek}.", ex);
            }

            int moveDay = position.MoveDay;
            if (moveDay != 0) return 0;

            var startTime = position.StartTime;
            var capacity = position.Capacity;

            var startAvailablePeriod = cDateTrav.Date.Add(startTime);
            if (dateFrom < startAvailablePeriod) return 0;

            var endAvailablePeriod = startAvailablePeriod.AddMilliseconds(capacity);
            if (dateFrom > endAvailablePeriod) return 0;

            return (long)(endAvailablePeriod - dateFrom).TotalMilliseconds;
        }

        public async Task<DayStartCapacityResult> DayStartCapacityAvailable(TechDataCalendarWeek techDataCalendarWeek,
            int dayStart)
        {
            int moveDay = 0;
            double? capacity = null;
            TimeSpan? startTime = null;

            while (capacity == null || capacity == 0)
            {
                switch (dayStart)
                {
                    case 1: // Monday
                        capacity = techDataCalendarWeek.MondayCapacity;
                        startTime = techDataCalendarWeek.MondayStartTime?.TimeOfDay;
                        break;
                    case 2: // Tuesday
                        capacity = techDataCalendarWeek.TuesdayCapacity;
                        startTime = techDataCalendarWeek.TuesdayStartTime?.TimeOfDay;
                        break;
                    case 3: // Wednesday
                        capacity = techDataCalendarWeek.WednesdayCapacity;
                        startTime = techDataCalendarWeek.WednesdayStartTime?.TimeOfDay;
                        break;
                    case 4: // Thursday
                        capacity = techDataCalendarWeek.ThursdayCapacity;
                        startTime = techDataCalendarWeek.ThursdayStartTime?.TimeOfDay;
                        break;
                    case 5: // Friday
                        capacity = techDataCalendarWeek.FridayCapacity;
                        startTime = techDataCalendarWeek.FridayStartTime?.TimeOfDay;
                        break;
                    case 6: // Saturday
                        capacity = techDataCalendarWeek.SaturdayCapacity;
                        startTime = techDataCalendarWeek.SaturdayStartTime?.TimeOfDay;
                        break;
                    case 0: // Sunday
                        capacity = techDataCalendarWeek.SundayCapacity;
                        startTime = techDataCalendarWeek.SundayStartTime?.TimeOfDay;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dayStart), "Invalid day of the week.");
                }

                // If there's no capacity or start time for this day, move to the next day
                if (capacity == null || capacity == 0)
                {
                    moveDay += 1;
                    dayStart = (dayStart == 6) ? 0 : dayStart + 1; // Move to next day, wrap to Sunday after Saturday

                    // Safety check: If we've looped over all 7 days without finding capacity, throw an error
                    if (moveDay > 6)
                    {
                        throw new InvalidOperationException("No available capacity for the entire week.");
                    }
                }
            }

            // Check that we successfully found a day with capacity and a valid start time
            if (capacity.HasValue && startTime.HasValue)
            {
                return new DayStartCapacityResult
                {
                    Capacity = capacity.Value,
                    StartTime = startTime.Value,
                    MoveDay = moveDay
                };
            }
            else
            {
                throw new InvalidOperationException("Failed to find valid capacity and start time.");
            }
        }


        public async Task<StartNextDayResult> StartNextDay(TechDataCalendar techDataCalendar, DateTime dateFrom)
        {
            var result = new StartNextDayResult();
            DateTime dateTo;
            TechDataCalendarWeek techDataCalendarWeek;

            try
            {
                // Read TechDataCalendarWeek
                techDataCalendarWeek = await _context.TechDataCalendarWeeks
                    .FindAsync(techDataCalendar.CalendarWeekId);

                if (techDataCalendarWeek == null)
                {
                    throw new InvalidOperationException("Problem reading Calendar Week associated with calendar.");
                }
            }
            catch (Exception ex)
            {
                // Log and throw a more specific exception
                Console.Error.WriteLine($"Error reading Calendar Week: {ex.Message}");
                throw new ServiceException("Problem reading Calendar Week.", ex);
            }

            // Initialize calendar instance
            var cDateTrav = dateFrom;
            var position = await DayStartCapacityAvailable(techDataCalendarWeek, (int)cDateTrav.DayOfWeek);
            TimeSpan startTime = position.StartTime;
            int moveDay = position.MoveDay;

            dateTo = (moveDay == 0) ? dateFrom : dateFrom.Date.AddDays(moveDay);
            DateTime startAvailablePeriod = dateTo.Date.Add(startTime);

            if (dateTo < startAvailablePeriod)
            {
                dateTo = startAvailablePeriod;
            }
            else
            {
                dateTo = dateTo.Date.AddDays(1);
                cDateTrav = dateTo;
                position = await DayStartCapacityAvailable(techDataCalendarWeek, (int)cDateTrav.DayOfWeek);
                startTime = position.StartTime;
                moveDay = position.MoveDay;

                if (moveDay != 0)
                {
                    dateTo = dateTo.Date.AddDays(moveDay);
                }

                dateTo = dateTo.Add(startTime);
            }

            result.DateTo = dateTo;
            result.NextCapacity = position.Capacity;

            return result;
        }
    }
}