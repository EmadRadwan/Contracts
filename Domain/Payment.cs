namespace Domain;

public class Payment
{
    public Payment()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        Deductions = new HashSet<Deduction>();
        FinAccountTrans = new HashSet<FinAccountTran>();
        PaymentApplicationPayments = new HashSet<PaymentApplication>();
        PaymentApplicationToPayments = new HashSet<PaymentApplication>();
        PaymentAttributes = new HashSet<PaymentAttribute>();
        PaymentBudgetAllocations = new HashSet<PaymentBudgetAllocation>();
        PaymentContents = new HashSet<PaymentContent>();
        PaymentGroupMembers = new HashSet<PaymentGroupMember>();
        PerfReviews = new HashSet<PerfReview>();
        ReturnItemResponses = new HashSet<ReturnItemResponse>();
    }

    public string PaymentId { get; set; } = null!;
    public string? PaymentTypeId { get; set; }
    public string? PaymentMethodTypeId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? PaymentGatewayResponseId { get; set; }
    public string? PaymentPreferenceId { get; set; }
    public string? PartyIdFrom { get; set; }
    public string? PartyIdTo { get; set; }
    public string? RoleTypeIdTo { get; set; }
    public string? StatusId { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? PaymentRefNum { get; set; }
    public decimal Amount { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? Comments { get; set; }
    public string? FinAccountTransId { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public decimal? ActualCurrencyAmount { get; set; }
    public string? ActualCurrencyUomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? ActualCurrencyUom { get; set; }
    public Uom? CurrencyUom { get; set; }
    public FinAccountTran? FinAccountTransNavigation { get; set; }
    public GlAccount? OverrideGlAccount { get; set; }
    public Party? PartyIdFromNavigation { get; set; }
    public Party? PartyIdToNavigation { get; set; }
    public PaymentGatewayResponse? PaymentGatewayResponse { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public PaymentMethodType? PaymentMethodType { get; set; }
    public OrderPaymentPreference? PaymentPreference { get; set; }
    public PaymentType? PaymentType { get; set; }
    public RoleType? RoleTypeIdToNavigation { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<Deduction> Deductions { get; set; }
    public ICollection<FinAccountTran> FinAccountTrans { get; set; }
    public ICollection<PaymentApplication> PaymentApplicationPayments { get; set; }
    public ICollection<PaymentApplication> PaymentApplicationToPayments { get; set; }
    public ICollection<PaymentAttribute> PaymentAttributes { get; set; }
    public ICollection<PaymentBudgetAllocation> PaymentBudgetAllocations { get; set; }
    public ICollection<PaymentContent> PaymentContents { get; set; }
    public ICollection<PaymentGroupMember> PaymentGroupMembers { get; set; }
    public ICollection<PerfReview> PerfReviews { get; set; }
    public ICollection<ReturnItemResponse> ReturnItemResponses { get; set; }
}