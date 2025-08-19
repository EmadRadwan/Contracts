namespace Domain;

public class CommContentAssocType
{
    public CommContentAssocType()
    {
        CommEventContentAssocs = new HashSet<CommEventContentAssoc>();
    }

    public string CommContentAssocTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<CommEventContentAssoc> CommEventContentAssocs { get; set; }
}