using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Facilities.Facilities;

public class UpdateFacility
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

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<FacilityDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var facility = await _context.Facilities.FindAsync(request.Facility.FacilityId);

            if (facility == null) return null;

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());


            var stamp = DateTime.UtcNow;

            request.Facility.LastUpdatedStamp = stamp;

            _mapper.Map(request.Facility, facility);


            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return Result<FacilityDto>.Failure("Failed to update Facility");

            var facilityToReturn = _context.Facilities.Where(x => x.FacilityId ==
                                                                  request.Facility.FacilityId)
                .ProjectTo<FacilityDto>(_mapper.ConfigurationProvider).Single();


            return Result<FacilityDto>.Success(facilityToReturn);
        }
    }
}