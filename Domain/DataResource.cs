namespace Domain;

public class DataResource
{
    public DataResource()
    {
        ContentDataResources = new HashSet<Content>();
        ContentRevisionItemNewDataResources = new HashSet<ContentRevisionItem>();
        ContentRevisionItemOldDataResources = new HashSet<ContentRevisionItem>();
        ContentTemplateDataResources = new HashSet<Content>();
        DataResourceAttributes = new HashSet<DataResourceAttribute>();
        DataResourceMetaData = new HashSet<DataResourceMetaDatum>();
        DataResourcePurposes = new HashSet<DataResourcePurpose>();
        DataResourceRoles = new HashSet<DataResourceRole>();
        ProductFeatureDataResources = new HashSet<ProductFeatureDataResource>();
    }

    public string DataResourceId { get; set; } = null!;
    public string? DataResourceTypeId { get; set; }
    public string? DataTemplateTypeId { get; set; }
    public string? DataCategoryId { get; set; }
    public string? DataSourceId { get; set; }
    public string? StatusId { get; set; }
    public string? DataResourceName { get; set; }
    public string? LocaleString { get; set; }
    public string? MimeTypeId { get; set; }
    public string? CharacterSetId { get; set; }
    public string? ObjectInfo { get; set; }
    public string? SurveyId { get; set; }
    public string? SurveyResponseId { get; set; }
    public string? RelatedDetailId { get; set; }
    public string? IsPublic { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CharacterSet? CharacterSet { get; set; }
    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public DataCategory? DataCategory { get; set; }
    public DataResourceType? DataResourceType { get; set; }
    public DataSource? DataSource { get; set; }
    public DataTemplateType? DataTemplateType { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public StatusItem? Status { get; set; }
    public Survey? Survey { get; set; }
    public SurveyResponse? SurveyResponse { get; set; }
    public AudioDataResource AudioDataResource { get; set; } = null!;
    public ElectronicText ElectronicText { get; set; } = null!;
    public ImageDataResource ImageDataResource { get; set; } = null!;
    public OtherDataResource OtherDataResource { get; set; } = null!;
    public VideoDataResource VideoDataResource { get; set; } = null!;
    public ICollection<Content> ContentDataResources { get; set; }
    public ICollection<ContentRevisionItem> ContentRevisionItemNewDataResources { get; set; }
    public ICollection<ContentRevisionItem> ContentRevisionItemOldDataResources { get; set; }
    public ICollection<Content> ContentTemplateDataResources { get; set; }
    public ICollection<DataResourceAttribute> DataResourceAttributes { get; set; }
    public ICollection<DataResourceMetaDatum> DataResourceMetaData { get; set; }
    public ICollection<DataResourcePurpose> DataResourcePurposes { get; set; }
    public ICollection<DataResourceRole> DataResourceRoles { get; set; }
    public ICollection<ProductFeatureDataResource> ProductFeatureDataResources { get; set; }
}