using Application.Catalog.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class GetServiceSpecifications
{
    public class Query : IRequest<Result<List<ServiceSpecificationDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<ServiceSpecificationDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ServiceSpecificationDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var serviceSpecifications = await _context.ServiceSpecifications
                .Include(x => x.Make)
                .Include(x => x.Model)
                .Include(x => x.Product)
                .Select(x => new ServiceSpecificationDto
                {
                    ServiceSpecificationId = x.ServiceSpecificationId,
                    MakeId = x.MakeId,
                    MakeDescription = x.Make.Description,
                    ModelId = x.ModelId,
                    ModelDescription = x.Model.Description,
                    ProductId = new ProductServiceLovDto
                    {
                        ProductId = x.ProductId,
                        ProductName = x.Product.ProductName
                    },
                    ProductName = x.Product.ProductName,
                    FromDate = x.FromDate,
                    ThruDate = x.ThruDate,
                    StandardTimeInMinutes = x.StandardTimeInMinutes
                }).ToListAsync(cancellationToken);

            return Result<List<ServiceSpecificationDto>>.Success(serviceSpecifications);
        }
    }
}