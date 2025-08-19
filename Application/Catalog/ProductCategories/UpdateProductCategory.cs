using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Catalog.ProductCategories;

public class UpdateProductCategory
{
    public class Command : IRequest<Result<ProductCategoryDto>>
    {
        public ProductCategoryMember ProductCategoryMember { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ProductCategoryMember).SetValidator(new ProductCategoryValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<ProductCategoryDto>>
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

        public async Task<Result<ProductCategoryDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var productCategoryMember = await _context.ProductCategoryMembers.FindAsync(
                request.ProductCategoryMember.ProductCategoryId,
                request.ProductCategoryMember.ProductId,
                request.ProductCategoryMember.FromDate);

            if (productCategoryMember == null) return null;


            var stamp = DateTime.Now;

            request.ProductCategoryMember.LastUpdatedStamp = stamp;

            _mapper.Map(request.ProductCategoryMember, productCategoryMember);


            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return Result<ProductCategoryDto>.Failure("Failed to update Product Category");

            var productCategoryToReturn = _context.ProductCategoryMembers.Where(x => x.ProductCategoryId ==
                    request.ProductCategoryMember.ProductCategoryId
                    && x.ProductId ==
                    request.ProductCategoryMember.ProductId
                    && x.FromDate ==
                    request.ProductCategoryMember.FromDate)
                .ProjectTo<ProductCategoryDto>(_mapper.ConfigurationProvider).Single();


            return Result<ProductCategoryDto>.Success(productCategoryToReturn);
        }
    }
}