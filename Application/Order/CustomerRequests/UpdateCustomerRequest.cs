using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.CustomerRequests;

public class UpdateCustomerRequest
{
    public class Command : IRequest<Result<CustRequestDto>>
    {
        public CustRequestDto CustRequestDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.CustRequestDto).SetValidator(new CustRequestValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<CustRequestDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
            _context = context;
        }

        public async Task<Result<CustRequestDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());

            var requestStatusSubmitted = await _context.StatusItems.SingleOrDefaultAsync(x =>
                x.StatusId == "CRQ_SUBMITTED");

            var customerRequest = await _context.CustRequests.Include(x => x.CustRequestItems)
                .SingleOrDefaultAsync(x => x.CustRequestId == request.CustRequestDto.CustRequestId);

            if (customerRequest == null) return null;

            var stamp = DateTime.Now;

            customerRequest.LastUpdatedStamp = stamp;
            customerRequest.LastModifiedDate = stamp;
            customerRequest.FromPartyId = request.CustRequestDto.FromPartyId;
            customerRequest.LastModifiedByUserLogin = user.Id;


            foreach (var updatedItem in request.CustRequestDto.CustRequestItems)
            {
                var savedItem = customerRequest.CustRequestItems.ToList()
                    .FirstOrDefault(item => item.CustRequestId == updatedItem.CustRequestId &&
                                            item.CustRequestItemSeqId == updatedItem.CustRequestItemSeqId);

                if (savedItem != null)
                {
                    if (updatedItem.IsProductDeleted)
                    {
                        _context.CustRequestItems.Remove(savedItem);
                    }
                    else
                    {
                        savedItem.ProductId = updatedItem.ProductId;
                        savedItem.Quantity = updatedItem.Quantity;
                    }
                }
                else
                {
                    var newItem = new CustRequestItem
                    {
                        CustRequestId = updatedItem.CustRequestId,
                        CustRequestItemSeqId = updatedItem.CustRequestItemSeqId,
                        ProductId = updatedItem.ProductId,
                        Quantity = updatedItem.Quantity,
                        StatusId = updatedItem.StatusId
                    };
                    _context.CustRequestItems.Add(newItem);
                }
            }


            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                transaction.Rollback();
                return Result<CustRequestDto>.Failure("Failed to create Customer Request");
            }

            transaction.Commit();

            var custRequestToReturn = (from rqst in _context.CustRequests
                join pty in _context.Parties on rqst.FromPartyId equals pty.PartyId
                join crt in _context.CustRequestTypes on rqst.CustRequestTypeId equals crt.CustRequestTypeId
                join sts in _context.StatusItems on rqst.StatusId equals sts.StatusId
                //join enm in _context.Enumerations on rqst.SalesChannelEnumId equals enm.EnumId
                join pcm in _context.PartyContactMeches on pty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                      && pty.PartyId == request.CustRequestDto.FromPartyId &&
                      rqst.CustRequestId == request.CustRequestDto.CustRequestId
                select new CustRequestDto
                {
                    CustRequestId = rqst.CustRequestId,
                    FromPartyName = pty.Description + " ( " + tn.AreaCode + "-" + tn.ContactNumber + " )",
                    CustRequestDate = DateTime.SpecifyKind(rqst.CustRequestDate.Truncate(TimeSpan.FromSeconds(1)),
                        DateTimeKind.Utc)
                }).ToList();


            return Result<CustRequestDto>.Success(custRequestToReturn[0]);
        }
    }
}