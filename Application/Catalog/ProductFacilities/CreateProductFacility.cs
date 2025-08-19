using Application.Interfaces;
using Application.ProductFacilities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductFacilities;

public class CreateProductFacility
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

        public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper)
        {
            _userAccessor = userAccessor;
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ProductFacilityDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());


            var stamp = DateTime.UtcNow;


            var facilityProduct = new ProductFacility
            {
                ProductId = request.ProductFacility.ProductId,
                FacilityId = request.ProductFacility.FacilityId,
                MinimumStock = request.ProductFacility.MinimumStock,
                ReorderQuantity = request.ProductFacility.ReorderQuantity,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp,
            };

            _context.ProductFacilities.Add(facilityProduct);


            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<ProductFacilityDto>.Failure("Failed to create Product Facility");

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