namespace Domain;

public class Content
{
    public Content()
    {
        AgreementContents = new HashSet<AgreementContent>();
        CommEventContentAssocs = new HashSet<CommEventContentAssoc>();
        ContentApprovals = new HashSet<ContentApproval>();
        ContentAssocContentIdToNavigations = new HashSet<ContentAssoc>();
        ContentAssocContents = new HashSet<ContentAssoc>();
        ContentAttributes = new HashSet<ContentAttribute>();
        ContentKeywords = new HashSet<ContentKeyword>();
        ContentMetaData = new HashSet<ContentMetaDatum>();
        ContentPurposes = new HashSet<ContentPurpose>();
        ContentRevisions = new HashSet<ContentRevision>();
        ContentRoles = new HashSet<ContentRole>();
        CustRequestContents = new HashSet<CustRequestContent>();
        FacilityContents = new HashSet<FacilityContent>();
        InverseDecoratorContent = new HashSet<Content>();
        InverseInstanceOfContent = new HashSet<Content>();
        InverseOwnerContent = new HashSet<Content>();
        InvoiceContents = new HashSet<InvoiceContent>();
        OrderContents = new HashSet<OrderContent>();
        PartyContents = new HashSet<PartyContent>();
        PaymentContents = new HashSet<PaymentContent>();
        PortalPages = new HashSet<PortalPage>();
        ProdConfItemContents = new HashSet<ProdConfItemContent>();
        ProductCategoryContents = new HashSet<ProductCategoryContent>();
        ProductContents = new HashSet<ProductContent>();
        ProductPromoContents = new HashSet<ProductPromoContent>();
        SubscriptionResources = new HashSet<SubscriptionResource>();
        SurveyResponseAnswers = new HashSet<SurveyResponseAnswer>();
        WebPages = new HashSet<WebPage>();
        WebSiteContents = new HashSet<WebSiteContent>();
        WebSitePathAliases = new HashSet<WebSitePathAlias>();
        WorkEffortContents = new HashSet<WorkEffortContent>();
    }

    public string ContentId { get; set; } = null!;
    public string? ContentTypeId { get; set; }
    public string? OwnerContentId { get; set; }
    public string? DecoratorContentId { get; set; }
    public string? InstanceOfContentId { get; set; }
    public string? DataResourceId { get; set; }
    public string? TemplateDataResourceId { get; set; }
    public string? DataSourceId { get; set; }
    public string? StatusId { get; set; }
    public string? PrivilegeEnumId { get; set; }
    public string? ServiceName { get; set; }
    public string? CustomMethodId { get; set; }
    public string? ContentName { get; set; }
    public string? Description { get; set; }
    public string? LocaleString { get; set; }
    public string? MimeTypeId { get; set; }
    public string? CharacterSetId { get; set; }
    public int? ChildLeafCount { get; set; }
    public int? ChildBranchCount { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CharacterSet? CharacterSet { get; set; }
    public ContentType? ContentType { get; set; }
    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public CustomMethod? CustomMethod { get; set; }
    public DataResource? DataResource { get; set; }
    public DataSource? DataSource { get; set; }
    public Content? DecoratorContent { get; set; }
    public Content? InstanceOfContent { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public Content? OwnerContent { get; set; }
    public Enumeration? PrivilegeEnum { get; set; }
    public StatusItem? Status { get; set; }
    public DataResource? TemplateDataResource { get; set; }
    public WebSitePublishPoint WebSitePublishPoint { get; set; } = null!;
    public ICollection<AgreementContent> AgreementContents { get; set; }
    public ICollection<CommEventContentAssoc> CommEventContentAssocs { get; set; }
    public ICollection<ContentApproval> ContentApprovals { get; set; }
    public ICollection<ContentAssoc> ContentAssocContentIdToNavigations { get; set; }
    public ICollection<ContentAssoc> ContentAssocContents { get; set; }
    public ICollection<ContentAttribute> ContentAttributes { get; set; }
    public ICollection<ContentKeyword> ContentKeywords { get; set; }
    public ICollection<ContentMetaDatum> ContentMetaData { get; set; }
    public ICollection<ContentPurpose> ContentPurposes { get; set; }
    public ICollection<ContentRevision> ContentRevisions { get; set; }
    public ICollection<ContentRole> ContentRoles { get; set; }
    public ICollection<CustRequestContent> CustRequestContents { get; set; }
    public ICollection<FacilityContent> FacilityContents { get; set; }
    public ICollection<Content> InverseDecoratorContent { get; set; }
    public ICollection<Content> InverseInstanceOfContent { get; set; }
    public ICollection<Content> InverseOwnerContent { get; set; }
    public ICollection<InvoiceContent> InvoiceContents { get; set; }
    public ICollection<OrderContent> OrderContents { get; set; }
    public ICollection<PartyContent> PartyContents { get; set; }
    public ICollection<PaymentContent> PaymentContents { get; set; }
    public ICollection<PortalPage> PortalPages { get; set; }
    public ICollection<ProdConfItemContent> ProdConfItemContents { get; set; }
    public ICollection<ProductCategoryContent> ProductCategoryContents { get; set; }
    public ICollection<ProductContent> ProductContents { get; set; }
    public ICollection<ProductPromoContent> ProductPromoContents { get; set; }
    public ICollection<SubscriptionResource> SubscriptionResources { get; set; }
    public ICollection<SurveyResponseAnswer> SurveyResponseAnswers { get; set; }
    public ICollection<WebPage> WebPages { get; set; }
    public ICollection<WebSiteContent> WebSiteContents { get; set; }
    public ICollection<WebSitePathAlias> WebSitePathAliases { get; set; }
    public ICollection<WorkEffortContent> WorkEffortContents { get; set; }
    public ICollection<VehicleContent> VehicleContents { get; set; }
}