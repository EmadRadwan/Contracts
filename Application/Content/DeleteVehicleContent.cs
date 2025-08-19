using Application.Interfaces;
using MediatR;
using Persistence;

namespace Application.Content;

public class DeleteVehicleContent
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Id { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
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

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var result = await _contentAccessor.DeleteContent(request.Id);

            if (result == null) return Result<Unit>.Failure("Problem deleting content from Digital Ocean Spaces");


            return Result<Unit>.Success(Unit.Value);
        }
    }
}