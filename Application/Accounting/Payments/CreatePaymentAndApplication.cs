using Application.Accounting.Services;
using MediatR;

namespace Application.Accounting.Payments;
public class CreatePaymentAndApplicationCommand : IRequest<Result<CreatePaymentAndApplicationResponse>>
{
    public string PaymentTypeId { get; set; }
    public string PartyIdFrom { get; set; }
    public string PartyIdTo { get; set; }
    public string? StatusId { get; set; }
    public decimal Amount { get; set; }
    public string? InvoiceId { get; set; }
    public string? InvoiceItemSeqId { get; set; }
    public string BillingAccountId { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string? TaxAuthGeoId { get; set; }
}

public class CreatePaymentAndApplicationHandler : IRequestHandler<CreatePaymentAndApplicationCommand, Result<CreatePaymentAndApplicationResponse>>
{
    private readonly IPaymentHelperService _paymentHelperService;

    public CreatePaymentAndApplicationHandler(IPaymentHelperService paymentHelperService)
    {
        _paymentHelperService = paymentHelperService;
    }
   

    public async Task<Result<CreatePaymentAndApplicationResponse>> Handle(CreatePaymentAndApplicationCommand request, CancellationToken cancellationToken)
    {
        return await _paymentHelperService.CreatePaymentAndApplication(new CreatePaymentAndApplicationRequest
        {
            PaymentTypeId = request.PaymentTypeId,
            PartyIdFrom = request.PartyIdFrom,
            PartyIdTo = request.PartyIdTo,
            StatusId = request.StatusId,
            Amount = request.Amount,
            InvoiceId = request.InvoiceId,
            InvoiceItemSeqId = request.InvoiceItemSeqId,
            BillingAccountId = request.BillingAccountId,
            OverrideGlAccountId = request.OverrideGlAccountId,
            TaxAuthGeoId = request.TaxAuthGeoId
        });
    }
}