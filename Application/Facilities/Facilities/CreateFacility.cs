using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Facilities.Facilities;

public class CreateFacility
{
    public class Command : IRequest<Result<FacilityDto>>
    {
        public Facility Facility { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Facility).SetValidator(new FacilityValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<FacilityDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;


        public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper)
        {
            _userAccessor = userAccessor;
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<FacilityDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());


            var stamp = DateTime.UtcNow;

            //request.Facility.FacilityId = Guid.NewGuid().ToString();
            request.Facility.CreatedStamp = stamp;
            request.Facility.LastUpdatedStamp = stamp;

            var facility = _context.Facilities.Add(request.Facility);


            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return Result<FacilityDto>.Failure("Failed to create Facility");

            var facilityToReturn = _context.Facilities.Where(x => x.FacilityId ==
                                                                  request.Facility.FacilityId)
                .ProjectTo<FacilityDto>(_mapper.ConfigurationProvider).Single();


            return Result<FacilityDto>.Success(facilityToReturn);
        }
    }
}