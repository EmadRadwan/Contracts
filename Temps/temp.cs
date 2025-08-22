modelBuilder.Entity<WorkEffort>(entity =>
{
    entity.ToTable("WORK_EFFORT");

    // Existing indexes
    entity.HasIndex(e => e.AccommodationMapId, "WK_EFFRT_ACC_MAP");
    entity.HasIndex(e => e.AccommodationSpotId, "WK_EFFRT_ACC_SPOT");
    entity.HasIndex(e => e.CurrentStatusId, "WK_EFFRT_CURSTTS");
    entity.HasIndex(e => e.EstimateCalcMethod, "WK_EFFRT_CUS_MET");
    entity.HasIndex(e => e.FacilityId, "WK_EFFRT_FACILITY");
    entity.HasIndex(e => e.FixedAssetId, "WK_EFFRT_FXDASST");
    entity.HasIndex(e => e.MoneyUomId, "WK_EFFRT_MON_UOM");
    entity.HasIndex(e => e.NoteId, "WK_EFFRT_NOTE");
    entity.HasIndex(e => e.WorkEffortParentId, "WK_EFFRT_PARENT");
    entity.HasIndex(e => e.WorkEffortPurposeTypeId, "WK_EFFRT_PRPTYP");
    entity.HasIndex(e => e.RecurrenceInfoId, "WK_EFFRT_RECINFO");
    entity.HasIndex(e => e.RuntimeDataId, "WK_EFFRT_RNTMDTA");
    entity.HasIndex(e => e.ScopeEnumId, "WK_EFFRT_SC_ENUM");
    entity.HasIndex(e => e.TempExprId, "WK_EFFRT_TEMPEXPR");
    entity.HasIndex(e => e.WorkEffortTypeId, "WK_EFFRT_TYPE");
    entity.HasIndex(e => e.CreatedTxStamp, "WORK_EFFORT_TXCRTS");
    entity.HasIndex(e => e.LastUpdatedTxStamp, "WORK_EFFORT_TXSTMP");
    entity.HasIndex(e => e.projectId, "WK_EFFRT_PROJECT");
    entity.HasIndex(e => e.PartyId, "WK_EFFRT_PARTY");
    entity.HasIndex(e => e.RelatedOrderId, "WK_EFFRT_RELATED_ORDER");
    entity.HasIndex(e => e.ProductId, "WK_EFFRT_PRODUCT");

    // Existing properties (abbreviated for brevity)
    entity.Property(e => e.WorkEffortId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("WORK_EFFORT_ID");

    // ... (other existing properties remain unchanged)

    // New properties
    entity.Property(e => e.projectNum)
        .HasMaxLength(60)
        .IsUnicode(false)
        .HasColumnName("PROJECT_NUM");

    entity.Property(e => e.CertificateNumber)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("CERTIFICATE_NUMBER");

    entity.Property(e => e.ProjectName)
        .HasMaxLength(255)
        .IsUnicode(false)
        .HasColumnName("PROJECT_NAME");

    entity.Property(e => e.TotalAmount)
        .HasColumnType("decimal(18, 2)")
        .HasColumnName("TOTAL_AMOUNT");

    entity.Property(e => e.projectId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("PROJECT_ID");

    entity.Property(e => e.PartyId)
        .HasColumnName("PARTY_ID");

    entity.Property(e => e.RelatedOrderId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("RELATED_ORDER_ID");

    entity.Property(e => e.CertificateCategory)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("CERTIFICATE_CATEGORY");

    entity.Property(e => e.SupplierOrContractorType)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("SUPPLIER_OR_CONTRACTOR_TYPE");

    entity.Property(e => e.LineNumber)
        .HasColumnName("LINE_NUMBER");

    entity.Property(e => e.Quantity)
        .HasColumnType("decimal(18, 6)")
        .HasColumnName("QUANTITY");

    entity.Property(e => e.Rate)
        .HasColumnType("decimal(18, 2)")
        .HasColumnName("RATE");

    entity.Property(e => e.CompletionPercentage)
        .HasColumnType("decimal(5, 2)")
        .HasColumnName("COMPLETION_PERCENTAGE");

    entity.Property(e => e.DueAmount)
        .HasColumnType("decimal(18, 2)")
        .HasColumnName("DUE_AMOUNT");

    entity.Property(e => e.PaidAmount)
        .HasColumnType("decimal(18, 2)")
        .HasColumnName("PAID_AMOUNT");

    entity.Property(e => e.RemainingAmount)
        .HasColumnType("decimal(18, 2)")
        .HasColumnName("REMAINING_AMOUNT");

    entity.Property(e => e.Notes)
        .HasMaxLength(255)
        .IsUnicode(false)
        .HasColumnName("NOTES");

    entity.Property(e => e.ProductId)
        .HasColumnName("PRODUCT_ID");

    // Existing relationships
    entity.HasOne(d => d.AccommodationMap)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.AccommodationMapId)
        .HasConstraintName("WK_EFFRT_ACC_MAP");

    entity.HasOne(d => d.AccommodationSpot)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.AccommodationSpotId)
        .HasConstraintName("WK_EFFRT_ACC_SPOT");

    entity.HasOne(d => d.CurrentStatus)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.CurrentStatusId)
        .HasConstraintName("WK_EFFRT_CURSTTS");

    entity.HasOne(d => d.EstimateCalcMethodNavigation)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.EstimateCalcMethod)
        .HasConstraintName("WK_EFFRT_CUS_MET");

    entity.HasOne(d => d.Facility)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.FacilityId)
        .HasConstraintName("WK_EFFRT_FACILITY");

    entity.HasOne(d => d.FixedAsset)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.FixedAssetId)
        .HasConstraintName("WK_EFFRT_FXDASST");

    entity.HasOne(d => d.MoneyUom)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.MoneyUomId)
        .HasConstraintName("WK_EFFRT_MON_UOM");

    entity.HasOne(d => d.Note)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.NoteId)
        .HasConstraintName("WK_EFFRT_NOTE");

    entity.HasOne(d => d.RecurrenceInfo)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.RecurrenceInfoId)
        .HasConstraintName("WK_EFFRT_RECINFO");

    entity.HasOne(d => d.RuntimeData)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.RuntimeDataId)
        .HasConstraintName("WK_EFFRT_RNTMDTA");

    entity.HasOne(d => d.ScopeEnum)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.ScopeEnumId)
        .HasConstraintName("WK_EFFRT_SC_ENUM");

    entity.HasOne(d => d.TempExpr)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.TempExprId)
        .HasConstraintName("WK_EFFRT_TEMPEXPR");

    entity.HasOne(d => d.WorkEffortParent)
        .WithMany(p => p.InverseWorkEffortParent)
        .HasForeignKey(d => d.WorkEffortParentId)
        .HasConstraintName("WK_EFFRT_PARENT");

    entity.HasOne(d => d.WorkEffortPurposeType)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.WorkEffortPurposeTypeId)
        .HasConstraintName("WK_EFFRT_PRPTYP");

    entity.HasOne(d => d.WorkEffortType)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.WorkEffortTypeId)
        .HasConstraintName("WK_EFFRT_TYPE");

    // Existing relationships for new properties
    entity.HasOne(d => d.Party)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.PartyId)
        .HasConstraintName("WK_EFFRT_PARTY");

    entity.HasOne(d => d.Project)
        .WithMany() // No inverse collection yet; define if needed
        .HasForeignKey(d => d.projectId)
        .HasConstraintName("WK_EFFRT_PROJECT")
        .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete if appropriate

    entity.HasOne(d => d.Product)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.ProductId)
        .HasConstraintName("WK_EFFRT_PRODUCT");

    entity.HasOne(d => d.RelatedPurchaseOrder)
        .WithMany(p => p.WorkEfforts)
        .HasForeignKey(d => d.RelatedOrderId)
        .HasConstraintName("WK_EFFRT_RELATED_ORDER");

    // Ignore the conflicting navigation if not needed
    // entity.Ignore(e => e.Project); // Uncomment if Project should not use InverseWorkEffortParent
});