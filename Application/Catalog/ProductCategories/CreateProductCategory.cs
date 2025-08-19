using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductCategories;

public class CreateProductCategory
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

        public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper)
        {
            _userAccessor = userAccessor;
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ProductCategoryDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());


            var stamp = DateTime.Now;


            request.ProductCategoryMember.CreatedStamp = stamp;
            request.ProductCategoryMember.LastUpdatedStamp = stamp;

            _context.ProductCategoryMembers.Add(request.ProductCategoryMember);


            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return Result<ProductCategoryDto>.Failure("Failed to create Product Category");

            var productCategoryToReturn = _context.ProductCategoryMembers.Where(x => x.ProductId ==
                    request.ProductCategoryMember.ProductId
                    && x.ProductCategoryId ==
                    request.ProductCategoryMember.ProductCategoryId
                    && x.FromDate ==
                    request.ProductCategoryMember.FromDate)
                .ProjectTo<ProductCategoryDto>(_mapper.ConfigurationProvider).Single();

            return Result<ProductCategoryDto>.Success(productCategoryToReturn);
        }
    }
}