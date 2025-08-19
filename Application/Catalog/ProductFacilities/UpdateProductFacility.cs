using Application.Interfaces;
using Application.ProductFacilities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Catalog.ProductFacilities;

public class UpdateProductFacility
{
    public class Command : IRequest<Result<ProductFacilityDto>>
    {
        public ProductFacilityDto ProductFacility { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ProductFacility).SetValidator(new ProductFacilityValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<ProductFacilityDto>>
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

        public async Task<Result<ProductFacilityDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var productFacility = await _context.ProductFacilities.FindAsync(
                request.ProductFacility.ProductId,
                request.ProductFacility.FacilityId);

            if (productFacility == null) return null;


            var stamp = DateTime.Now;
            
            productFacility.MinimumStock = request.ProductFacility.MinimumStock;
            productFacility.ReorderQuantity = request.ProductFacility.ReorderQuantity;
            productFacility.LastUpdatedStamp = stamp;


            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return Result<ProductFacilityDto>.Failure("Failed to update Product Facility");

            var productFacilityToReturn = _context.ProductFacilities.Where(x => x.ProductId ==
                    request.ProductFacility.ProductId
                    && x.FacilityId ==
                    request.ProductFacility.FacilityId
                )
                .ProjectTo<ProductFacilityDto>(_mapper.ConfigurationProvider).Single();


            return Result<ProductFacilityDto>.Success(productFacilityToReturn);
        }
    }
}