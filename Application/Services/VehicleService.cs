using Application.Catalog.ProductStores;
using Application.Core;
using Application.order.Quotes;
using Domain;
using Persistence;

namespace Application.Services;

public interface IVehicleService
{

    /*Task<Quote> CreateJobQuote(JobQuoteDto jobQuoteDto);
    Task<Quote> UpdateJobQuote(JobQuoteDto jobQuoteDto);*/
    Task<ProductCategory> CreateVehicleMake(VehicleMakeDto vehicleMakeDto);
    Task<ProductCategory> CreateVehicleModel(VehicleModelDto vehicleModelDto);
    Task<ProductCategory> UpdateVehicleMake(VehicleMakeDto vehicleMakeDto);
    Task<ProductCategory> UpdateVehicleModel(VehicleModelDto vehicleModelDto);
    Task<ServiceRate> CreateServiceRate(ServiceRateDto serviceRateDto);
    Task<ServiceSpecification> CreateServiceSpecification(ServiceSpecificationDto serviceSpecificationDto);
    Task<ServiceRate> UpdateServiceRate(ServiceRateDto serviceRateDto);
    Task<ServiceSpecification> UpdateServiceSpecification(ServiceSpecificationDto serviceSpecificationDto);
}

public class VehicleService : IVehicleService
{
    private readonly DataContext _context;
    private readonly IProductStoreService _productStoreService;
    private readonly IQuoteService _quoteService;
    private readonly IUtilityService _utilityService;


    public VehicleService(DataContext context, IUtilityService utilityService, IQuoteService quoteService,
        IProductStoreService productStoreService)
    {
        _context = context;
        _utilityService = utilityService;
        _quoteService = quoteService;
        _productStoreService = productStoreService;
    }


    /*public async Task<Vehicle> CreateVehicle(VehicleDto vehicleDto)
    {
        var stamp = DateTime.UtcNow;

        //todo add created by, requires user id to be added to user_login table
        var vehicle = new Vehicle
        {
            VehicleId = Guid.NewGuid().ToString(),
            ChassisNumber = vehicleDto.ChassisNumber,
            Vin = vehicleDto.Vin,
            Year = vehicleDto.Year,
            PlateNumber = vehicleDto.PlateNumber,
            FromPartyId = vehicleDto.FromPartyId.FromPartyId,
            MakeId = vehicleDto.MakeId,
            ModelId = vehicleDto.ModelId,
            VehicleTypeId = vehicleDto.VehicleTypeId,
            TransmissionTypeId = vehicleDto.TransmissionTypeId,
            ExteriorColorId = vehicleDto.ExteriorColorId,
            InteriorColorId = vehicleDto.InteriorColorId,
            Mileage = vehicleDto.Mileage,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.Vehicles.Add(vehicle);
        return await Task.FromResult(vehicle);
    }

    public async Task<Vehicle> UpdateVehicle(VehicleDto vehicleDto)
    {
        var stamp = DateTime.UtcNow;

        var vehicle = await _context.Vehicles.FindAsync(vehicleDto.VehicleId);


        if (vehicle != null)
        {
            vehicle.Vin = vehicleDto.Vin;
            vehicle.Year = vehicleDto.Year;
            vehicle.PlateNumber = vehicleDto.PlateNumber;
            vehicle.FromPartyId = vehicleDto.FromPartyId.FromPartyId;
            vehicle.MakeId = vehicleDto.MakeId;
            vehicle.ModelId = vehicleDto.ModelId;
            vehicle.VehicleTypeId = vehicleDto.VehicleTypeId;
            vehicle.TransmissionTypeId = vehicleDto.TransmissionTypeId;
            vehicle.ExteriorColorId = vehicleDto.ExteriorColorId;
            vehicle.InteriorColorId = vehicleDto.InteriorColorId;
            vehicle.Mileage = vehicleDto.Mileage;
            vehicle.LastUpdatedStamp = stamp;
        }


        return vehicle;
    }
    */

    public async Task<ServiceRate> CreateServiceRate(ServiceRateDto serviceRateDto)
    {
        var stamp = DateTime.UtcNow;
        // get product store for logged in user
        var productStore = await _productStoreService.GetProductStoreForLoggedInUser();

        //todo add created by, requires user id to be added to user_login table
        var serviceRate = new ServiceRate
        {
            ServiceRateId = Guid.NewGuid().ToString(),
            ProductStoreId = productStore.ProductStoreId,
            MakeId = serviceRateDto.MakeId,
            ModelId = serviceRateDto.ModelId,
            FromDate = serviceRateDto.FromDate,
            ThruDate = serviceRateDto.ThruDate,
            Rate = serviceRateDto.Rate,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.ServiceRates.Add(serviceRate);
        return await Task.FromResult(serviceRate);
    }

    public async Task<ServiceSpecification> CreateServiceSpecification(ServiceSpecificationDto serviceSpecificationDto)
    {
        var stamp = DateTime.UtcNow;

        //todo add created by, requires user id to be added to user_login table
        var serviceSpecification = new ServiceSpecification
        {
            ServiceSpecificationId = Guid.NewGuid().ToString(),
            ProductId = serviceSpecificationDto.ProductId.ProductId,
            MakeId = serviceSpecificationDto.MakeId,
            ModelId = serviceSpecificationDto.ModelId,
            FromDate = serviceSpecificationDto.FromDate,
            ThruDate = serviceSpecificationDto.ThruDate,
            StandardTimeInMinutes = serviceSpecificationDto.StandardTimeInMinutes,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.ServiceSpecifications.Add(serviceSpecification);
        return await Task.FromResult(serviceSpecification);
    }

    public async Task<ServiceRate> UpdateServiceRate(ServiceRateDto serviceRateDto)
    {
        var stamp = DateTime.UtcNow;

        var serviceRate = await _context.ServiceRates.FindAsync(serviceRateDto.ServiceRateId);
        if (serviceRate == null) return null;

        serviceRate.ProductStoreId = serviceRateDto.ProductStoreId;
        serviceRate.MakeId = serviceRateDto.MakeId;
        serviceRate.ModelId = serviceRateDto.ModelId;
        serviceRate.FromDate = serviceRateDto.FromDate;
        serviceRate.ThruDate = serviceRateDto.ThruDate;
        serviceRate.Rate = serviceRateDto.Rate;
        serviceRate.LastUpdatedStamp = stamp;

        return serviceRate;
    }

    public async Task<ServiceSpecification> UpdateServiceSpecification(ServiceSpecificationDto serviceSpecificationDto)
    {
        var stamp = DateTime.UtcNow;

        var serviceSpecification =
            await _context.ServiceSpecifications.FindAsync(serviceSpecificationDto.ServiceSpecificationId);
        if (serviceSpecification == null) return null;

        serviceSpecification.ProductId = serviceSpecificationDto.ProductId.ProductId;
        serviceSpecification.MakeId = serviceSpecificationDto.MakeId;
        serviceSpecification.ModelId = serviceSpecificationDto.ModelId;
        serviceSpecification.FromDate = serviceSpecificationDto.FromDate;
        serviceSpecification.ThruDate = serviceSpecificationDto.ThruDate;
        serviceSpecification.StandardTimeInMinutes = serviceSpecificationDto.StandardTimeInMinutes;
        serviceSpecification.LastUpdatedStamp = stamp;

        return serviceSpecification;
    }

    public async Task<ProductCategory> CreateVehicleMake(VehicleMakeDto vehicleMakeDto)
    {
        var stamp = DateTime.UtcNow;

        //todo add created by, requires user id to be added to user_login table
        var productCategory = new ProductCategory
        {
            ProductCategoryId = vehicleMakeDto.MakeId,
            PrimaryParentCategoryId = "VEHICLE_MAKE",
            Description = vehicleMakeDto.MakeDescription,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.ProductCategories.Add(productCategory);
        return await Task.FromResult(productCategory);
    }

    public async Task<ProductCategory> UpdateVehicleMake(VehicleMakeDto vehicleMakeDto)
    {
        var stamp = DateTime.UtcNow;

        var productCategory = await _context.ProductCategories.FindAsync(vehicleMakeDto.MakeId);
        if (productCategory == null) return null;

        productCategory.Description = vehicleMakeDto.MakeDescription;
        productCategory.LastUpdatedStamp = stamp;

        return productCategory;
    }

    public async Task<ProductCategory> CreateVehicleModel(VehicleModelDto vehicleModelDto)
    {
        var stamp = DateTime.UtcNow;

        //todo add created by, requires user id to be added to user_login table
        var productCategory = new ProductCategory
        {
            ProductCategoryId = vehicleModelDto.ModelId,
            PrimaryParentCategoryId = vehicleModelDto.MakeId,
            Description = vehicleModelDto.ModelDescription,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.ProductCategories.Add(productCategory);
        return await Task.FromResult(productCategory);
    }

    public async Task<ProductCategory> UpdateVehicleModel(VehicleModelDto vehicleModelDto)
    {
        var stamp = DateTime.UtcNow;

        var productCategory = await _context.ProductCategories.FindAsync(vehicleModelDto.ModelId);
        if (productCategory == null) return null;

        productCategory.Description = vehicleModelDto.ModelDescription;
        productCategory.LastUpdatedStamp = stamp;

        return productCategory;
    }

    /*public async Task<Quote> CreateJobQuote(JobQuoteDto vehicleJobQuoteDto)
    {
        var stamp = DateTime.UtcNow;
        var newQuoteSerial = await _utilityService.GetNextSequence("Quote");


        //todo add created by, requires user id to be added to user_login table
        var vehicleJobQuote = new Quote
        {
            VehicleId = vehicleJobQuoteDto.VehicleId,
            PartyId = vehicleJobQuoteDto.FromPartyId,
            QuoteId = newQuoteSerial,
            StatusId = "QUO_CREATED",
            QuoteTypeId = "JOB_QUOTE",
            CustomerRemarks = vehicleJobQuoteDto.CustomerRemarks,
            InternalRemarks = vehicleJobQuoteDto.InternalRemarks,
            IssueDate = stamp,
            GrandTotal = vehicleJobQuoteDto.GrandTotal,
            ValidFromDate = stamp,
            ValidThruDate = stamp.AddDays(30),
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.Quotes.Add(vehicleJobQuote);

        // create quote items
        var quoteItems = _quoteService.CreateQuoteItems(vehicleJobQuoteDto.QuoteItems, newQuoteSerial);

        // create quote adjustments
        var quoteAdjustments =
            _quoteService.CreateQuoteAdjustments(vehicleJobQuoteDto.QuoteAdjustments, newQuoteSerial);
        return await Task.FromResult(vehicleJobQuote);
    }*/

    /*public async Task<Quote> UpdateJobQuote(JobQuoteDto vehicleJobQuoteDto)
    {
        var stamp = DateTime.UtcNow;

        var vehicleJobQuote = await _context.Quotes.FindAsync(vehicleJobQuoteDto.QuoteId);
        if (vehicleJobQuote == null) return null;

        vehicleJobQuote.VehicleId = vehicleJobQuoteDto.VehicleId;
        vehicleJobQuote.PartyId = vehicleJobQuoteDto.FromPartyId;
        vehicleJobQuote.CustomerRemarks = vehicleJobQuoteDto.CustomerRemarks;
        vehicleJobQuote.InternalRemarks = vehicleJobQuoteDto.InternalRemarks;
        vehicleJobQuote.LastUpdatedStamp = stamp;
        _context.Quotes.Update(vehicleJobQuote);

       // var quoteItems = _quoteService.UpdateQuoteItems(vehicleJobQuoteDto.QuoteItems, vehicleJobQuoteDto.QuoteId);

        return vehicleJobQuote;
    }*/
}