modelBuilder.Entity<WorkEffort>(entity =>
{
    entity.ToTable("WORK_EFFORT");

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

    entity.Property(e => e.WorkEffortId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("WORK_EFFORT_ID");

    entity.Property(e => e.AccommodationMapId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("ACCOMMODATION_MAP_ID");

    entity.Property(e => e.AccommodationSpotId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("ACCOMMODATION_SPOT_ID");

    entity.Property(e => e.ActualCompletionDate)
        .HasColumnType("datetime")
        .HasColumnName("ACTUAL_COMPLETION_DATE");

    entity.Property(e => e.ActualMilliSeconds).HasColumnName("ACTUAL_MILLI_SECONDS");

    entity.Property(e => e.ActualSetupMillis).HasColumnName("ACTUAL_SETUP_MILLIS");

    entity.Property(e => e.ActualStartDate)
        .HasColumnType("datetime")
        .HasColumnName("ACTUAL_START_DATE");

    entity.Property(e => e.CreatedByUserLogin)
        .HasMaxLength(250)
        .IsUnicode(false)
        .HasColumnName("CREATED_BY_USER_LOGIN");

    entity.Property(e => e.CreatedDate)
        .HasColumnType("datetime")
        .HasColumnName("CREATED_DATE");

    entity.Property(e => e.CreatedStamp)
        .HasColumnType("datetime")
        .HasColumnName("CREATED_STAMP");

    entity.Property(e => e.CreatedTxStamp)
        .HasColumnType("datetime")
        .HasColumnName("CREATED_TX_STAMP");

    entity.Property(e => e.CurrentStatusId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("CURRENT_STATUS_ID");

    entity.Property(e => e.Description)
        .HasMaxLength(255)
        .IsUnicode(false)
        .HasColumnName("DESCRIPTION");

    entity.Property(e => e.EstimateCalcMethod)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("ESTIMATE_CALC_METHOD");

    entity.Property(e => e.EstimatedCompletionDate)
        .HasColumnType("datetime")
        .HasColumnName("ESTIMATED_COMPLETION_DATE");

    entity.Property(e => e.EstimatedMilliSeconds).HasColumnName("ESTIMATED_MILLI_SECONDS");

    entity.Property(e => e.EstimatedSetupMillis).HasColumnName("ESTIMATED_SETUP_MILLIS");

    entity.Property(e => e.EstimatedStartDate)
        .HasColumnType("datetime")
        .HasColumnName("ESTIMATED_START_DATE");

    entity.Property(e => e.FacilityId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("FACILITY_ID");

    entity.Property(e => e.FixedAssetId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("FIXED_ASSET_ID");

    entity.Property(e => e.InfoUrl)
        .HasMaxLength(255)
        .IsUnicode(false)
        .HasColumnName("INFO_URL");

    entity.Property(e => e.LastModifiedByUserLogin)
        .HasMaxLength(250)
        .IsUnicode(false)
        .HasColumnName("LAST_MODIFIED_BY_USER_LOGIN");

    entity.Property(e => e.LastModifiedDate)
        .HasColumnType("datetime")
        .HasColumnName("LAST_MODIFIED_DATE");

    entity.Property(e => e.LastStatusUpdate)
        .HasColumnType("datetime")
        .HasColumnName("LAST_STATUS_UPDATE");

    entity.Property(e => e.LastUpdatedStamp)
        .HasColumnType("datetime")
        .HasColumnName("LAST_UPDATED_STAMP");

    entity.Property(e => e.LastUpdatedTxStamp)
        .HasColumnType("datetime")
        .HasColumnName("LAST_UPDATED_TX_STAMP");

    entity.Property(e => e.LocationDesc)
        .HasMaxLength(255)
        .IsUnicode(false)
        .HasColumnName("LOCATION_DESC");

    entity.Property(e => e.MoneyUomId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("MONEY_UOM_ID");

    entity.Property(e => e.NoteId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("NOTE_ID");

    entity.Property(e => e.PercentComplete).HasColumnName("PERCENT_COMPLETE");

    entity.Property(e => e.Priority).HasColumnName("PRIORITY");

    entity.Property(e => e.QuantityProduced)
        .HasColumnType("decimal(18, 6)")
        .HasColumnName("QUANTITY_PRODUCED");

    entity.Property(e => e.QuantityRejected)
        .HasColumnType("decimal(18, 6)")
        .HasColumnName("QUANTITY_REJECTED");

    entity.Property(e => e.QuantityToProduce)
        .HasColumnType("decimal(18, 6)")
        .HasColumnName("QUANTITY_TO_PRODUCE");

    entity.Property(e => e.RecurrenceInfoId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("RECURRENCE_INFO_ID");

    entity.Property(e => e.Reserv2ndPPPerc)
        .HasColumnType("decimal(18, 6)")
        .HasColumnName("RESERV2ND_P_P_PERC");

    entity.Property(e => e.ReservNthPPPerc)
        .HasColumnType("decimal(18, 6)")
        .HasColumnName("RESERV_NTH_P_P_PERC");

    entity.Property(e => e.ReservPersons)
        .HasColumnType("decimal(18, 6)")
        .HasColumnName("RESERV_PERSONS");

    entity.Property(e => e.RevisionNumber).HasColumnName("REVISION_NUMBER");

    entity.Property(e => e.RuntimeDataId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("RUNTIME_DATA_ID");

    entity.Property(e => e.ScopeEnumId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("SCOPE_ENUM_ID");

    entity.Property(e => e.SendNotificationEmail)
        .HasMaxLength(1)
        .IsUnicode(false)
        .HasColumnName("SEND_NOTIFICATION_EMAIL")
        .IsFixedLength();

    entity.Property(e => e.ServiceLoaderName)
        .HasMaxLength(100)
        .IsUnicode(false)
        .HasColumnName("SERVICE_LOADER_NAME");

    entity.Property(e => e.ShowAsEnumId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("SHOW_AS_ENUM_ID");

    entity.Property(e => e.SourceReferenceId)
        .HasMaxLength(60)
        .IsUnicode(false)
        .HasColumnName("SOURCE_REFERENCE_ID");

    entity.Property(e => e.SpecialTerms)
        .HasMaxLength(255)
        .IsUnicode(false)
        .HasColumnName("SPECIAL_TERMS");

    entity.Property(e => e.TempExprId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("TEMP_EXPR_ID");

    entity.Property(e => e.TimeTransparency).HasColumnName("TIME_TRANSPARENCY");

    entity.Property(e => e.TotalMilliSecondsAllowed).HasColumnName("TOTAL_MILLI_SECONDS_ALLOWED");

    entity.Property(e => e.TotalMoneyAllowed)
        .HasColumnType("decimal(18, 2)")
        .HasColumnName("TOTAL_MONEY_ALLOWED");

    entity.Property(e => e.UniversalId)
        .HasMaxLength(60)
        .IsUnicode(false)
        .HasColumnName("UNIVERSAL_ID");

    entity.Property(e => e.WorkEffortName)
        .HasMaxLength(255)
        .IsUnicode(false)
        .HasColumnName("WORK_EFFORT_NAME");

    entity.Property(e => e.WorkEffortParentId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("WORK_EFFORT_PARENT_ID");

    entity.Property(e => e.WorkEffortPurposeTypeId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("WORK_EFFORT_PURPOSE_TYPE_ID");

    entity.Property(e => e.WorkEffortTypeId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("WORK_EFFORT_TYPE_ID");

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

    entity.Property(e => e.ProjectNum)
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

    entity.Property(e => e.ProjectId)
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

    entity.HasIndex(e => e.ProjectId, "WK_EFFRT_PROJECT");
    entity.HasIndex(e => e.PartyId, "WK_EFFRT_PARTY");
    entity.HasIndex(e => e.RelatedOrderId, "WK_EFFRT_RELATED_ORDER");
    entity.HasIndex(e => e.ProductId, "WK_EFFRT_PRODUCT");

    entity.HasOne(d => d.Party)
        .WithMany(p => p.WorkEfforts) // Updated to use the new WorkEfforts collection
        .HasForeignKey(d => d.PartyId)
        .HasConstraintName("WK_EFFRT_PARTY");

    entity.HasOne(d => d.Project)
        .WithMany() // No inverse collection yet; define if needed
        .HasForeignKey(d => d.ProjectId)
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

    // New properties for Contracts system
    entity.Property(e => e.DiscountAmount)
        .HasColumnType("decimal(18, 2)")
        .HasColumnName("DISCOUNT_AMOUNT"); // REFACTOR: Added to track discounts, improving financial detail.

    entity.Property(e => e.Deductions)
        .HasColumnType("decimal(18, 2)")
        .HasColumnName("DEDUCTIONS"); // REFACTOR: Added to handle deductions, enhancing financial accuracy.

    entity.Property(e => e.InsuranceAmount)
        .HasColumnType("decimal(18, 2)")
        .HasColumnName("INSURANCE_AMOUNT"); // REFACTOR: Added to manage insurance costs, supporting risk-related tracking.

    entity.Property(e => e.AchievementPercent)
        .HasColumnType("decimal(5, 2)")
        .HasColumnName("ACHIEVEMENT_PERCENT"); // REFACTOR: Added to measure achievement separately from completion, improving performance metrics.

    entity.Property(e => e.IsProductCompanyPurchased)
        .HasMaxLength(1)
        .IsUnicode(false)
        .HasColumnName("IS_PRODUCT_COMPANY_PURCHASED")
        .IsFixedLength(); // REFACTOR: Added to indicate company-purchased products, aiding procurement tracking.
});