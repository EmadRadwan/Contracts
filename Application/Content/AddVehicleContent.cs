using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Content;

public class AddVehicleContent
{
    public class Command : IRequest<Result<VehicleContentDto>>
    {
        public IFormFile File { get; set; }
        public string VehicleId { get; set; } // Add this line
    }

    public class Handler : IRequestHandler<Command, Result<VehicleContentDto>>
    {
        private readonly IContentAccessor _contentAccessor;
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IContentAccessor contentAccessor, IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
            _contentAccessor = contentAccessor;
            _context = context;
        }

        public async Task<Result<VehicleContentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var contentActionResult = await _contentAccessor.AddContent(request.File);

            if (!contentActionResult.Success)
                return Result<VehicleContentDto>.Failure("Problem adding content to storage");

            var content = new VehicleContentDto
            {
                DataResourceId = contentActionResult.Result.PublicId,
                ObjectInfo = contentActionResult.Result.Url
            };
            var dateNow = DateTime.UtcNow;
            var nowDateTime = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, dateNow.Minute,
                dateNow.Second, 0, DateTimeKind.Utc);

            var dataResourceId = Guid.NewGuid().ToString();
            var contentId = Guid.NewGuid().ToString();

            try
            {
                // add file to database in tables Content and DataResource and VehicleContent
                var dataResource = new DataResource
                {
                    DataResourceId = dataResourceId,
                    DataResourceTypeId = "LOCAL_FILE",
                    DataResourceName = request.File.FileName,
                    ObjectInfo = contentActionResult.Result.Url,
                    MimeTypeId = request.File.ContentType,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                _context.DataResources.Add(dataResource);

                // add file to database in tables Content and DataResource and VehicleContent
                var contentDb = new Domain.Content
                {
                    ContentId = contentId,
                    DataResourceId = dataResourceId,
                    ContentTypeId = "DOCUMENT",
                    ContentName = request.File.FileName,
                    MimeTypeId = request.File.ContentType,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                _context.Contents.Add(contentDb);

                // add file to database in tables Content and DataResource and VehicleContent
                var vehicleContent = new VehicleContent
                {
                    VehicleId = request.VehicleId,
                    ContentId = contentId,
                    CreatedStamp = nowDateTime,
                    LastUpdatedStamp = nowDateTime
                };
                _context.VehicleContents.Add(vehicleContent);

                await _context.SaveChangesAsync(cancellationToken);
                return Result<VehicleContentDto>.Success(content);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                return Result<VehicleContentDto>.Failure(
                    $"An error occurred while adding content to the database: {ex.Message}");
            }
        }
    }
}