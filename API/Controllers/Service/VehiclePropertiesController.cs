using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Service;

public class VehiclePropertiesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new GetVehicleMakes.Query()));
    }

    [HttpGet("{makeId}/getVehicleModelsByMakeId")]
    public async Task<IActionResult> GetVehicleModelsByMakeId(string makeId)
    {
        return HandleResult(await Mediator.Send(new GetVehicleModelsByMakeId.Query { MakeId = makeId }));
    }

    [HttpGet("getVehicleModels")]
    public async Task<IActionResult> GetVehicleModels()
    {
        return HandleResult(await Mediator.Send(new GetVehicleModels.Query()));
    }

    [HttpGet("getVehicleTypes")]
    public async Task<IActionResult> GetVehicleTypes()
    {
        return HandleResult(await Mediator.Send(new GetVehicleTypes.Query()));
    }

    [HttpGet("getVehicleTransmissionTypes")]
    public async Task<IActionResult> GetVehicleTransmissionTypes()
    {
        return HandleResult(await Mediator.Send(new GetVehicleTransmissionTypes.Query()));
    }

    [HttpGet("getVehicleExteriorColors")]
    public async Task<IActionResult> GetVehicleExteriorColors()
    {
        return HandleResult(await Mediator.Send(new GetVehicleExteriorColors.Query()));
    }

    [HttpGet("getVehicleInteriorColors")]
    public async Task<IActionResult> GetVehicleInteriorColors()
    {
        return HandleResult(await Mediator.Send(new GetVehicleInteriorColors.Query()));
    }

    [HttpGet("getServiceRates")]
    public async Task<IActionResult> GetServiceRates()
    {
        return HandleResult(await Mediator.Send(new GetServiceRates.Query()));
    }

    [HttpGet("getServiceSpecifications")]
    public async Task<IActionResult> GetServiceSpecifications()
    {
        return HandleResult(await Mediator.Send(new GetServiceSpecifications.Query()));
    }


    [HttpPost("createVehicleMake", Name = "CreateVehicleMake")]
    public async Task<IActionResult> CreateVehicleMake(VehicleMakeDto vehicleMakeDto)
    {
        return HandleResult(await Mediator.Send(new CreateVehicleMake.Command { VehicleMakeDto = vehicleMakeDto }));
    }

    [HttpPost("createVehicleModel", Name = "CreateVehicleModel")]
    public async Task<IActionResult> CreateVehicleModel(VehicleModelDto vehicleModelDto)
    {
        return HandleResult(await Mediator.Send(new CreateVehicleModel.Command { VehicleModelDto = vehicleModelDto }));
    }

    [HttpPut("updateVehicleMake", Name = "UpdateVehicleMake")]
    public async Task<IActionResult> UpdateVehicleMake(VehicleMakeDto vehicleMakeDto)
    {
        return HandleResult(await Mediator.Send(new UpdateVehicleMake.Command { VehicleMakeDto = vehicleMakeDto }));
    }

    [HttpPut("updateVehicleModel", Name = "UpdateVehicleModel")]
    public async Task<IActionResult> UpdateVehicleModel(VehicleModelDto vehicleModelDto)
    {
        return HandleResult(await Mediator.Send(new UpdateVehicleModel.Command { VehicleModelDto = vehicleModelDto }));
    }

    [HttpPost("createServiceRate", Name = "CreateServiceRate")]
    public async Task<IActionResult> CreateServiceRate(ServiceRateDto serviceRateDto)
    {
        return HandleResult(await Mediator.Send(new CreateServiceRate.Command { ServiceRateDto = serviceRateDto }));
    }

    [HttpPost("createServiceSpecification", Name = "CreateServiceSpecification")]
    public async Task<IActionResult> CreateServiceSpecification(ServiceSpecificationDto serviceSpecificationDto)
    {
        return HandleResult(await Mediator.Send(new CreateServiceSpecification.Command
            { ServiceSpecificationDto = serviceSpecificationDto }));
    }

    [HttpPut("updateServiceRate", Name = "UpdateServiceRate")]
    public async Task<IActionResult> UpdateServiceRate(ServiceRateDto serviceRateDto)
    {
        return HandleResult(await Mediator.Send(new UpdateServiceRate.Command { ServiceRateDto = serviceRateDto }));
    }

    [HttpPut("updateServiceSpecification", Name = "UpdateServiceSpecification")]
    public async Task<IActionResult> UpdateServiceSpecification(ServiceSpecificationDto serviceSpecificationDto)
    {
        return HandleResult(await Mediator.Send(new UpdateServiceSpecification.Command
            { ServiceSpecificationDto = serviceSpecificationDto }));
    }
}