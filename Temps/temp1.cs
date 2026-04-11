modelBuilder.Entity<SalesOpportunityAction>(entity =>
{
    entity.ToTable("SALES_OPPORTUNITY_ACTION");

    // Primary Key
    entity.HasKey(e => e.SalesOpportunityActionId);

    // Indexes
    entity.HasIndex(e => e.SalesOpportunityId, "SLSOPPACT_OPPID");
    entity.HasIndex(e => e.ActionTypeId, "SLSOPPACT_ACTION_TYP");
    entity.HasIndex(e => e.CancelReasonId, "SLSOPPACT_CANCEL_RSN");
    entity.HasIndex(e => e.NextActionTypeId, "SLSOPPACT_NEXT_ACTION");   // New
    entity.HasIndex(e => e.IsWon, "SLSOPPACT_IS_WON");                    // New
    entity.HasIndex(e => e.CreatedByUserLogin, "SLSOPPACT_USRLGN");
    entity.HasIndex(e => e.CreatedStamp, "SLSOPPACT_CRTS");
    entity.HasIndex(e => e.LastUpdatedStamp, "SLSOPPACT_UPDST");
    entity.HasIndex(e => new { e.SalesOpportunityId, e.ActionTypeId }, "SLSOPPACT_OPP_ACTION");

    // Column Mappings
    entity.Property(e => e.SalesOpportunityActionId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("SALES_OPPORTUNITY_ACTION_ID");

    entity.Property(e => e.SalesOpportunityId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("SALES_OPPORTUNITY_ID")
        .IsRequired();

    entity.Property(e => e.ActionTypeId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("ACTION_TYPE_ID")
        .IsRequired();

    entity.Property(e => e.IsAnswered)
        .HasColumnName("IS_ANSWERED")
        .HasDefaultValue(false);

    entity.Property(e => e.ActionDate)
        .HasColumnType("datetime")
        .HasColumnName("ACTION_DATE");

    entity.Property(e => e.NextActionTypeId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("NEXT_ACTION_TYPE_ID");

    entity.Property(e => e.CancelReasonId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("CANCEL_REASON_ID");

    entity.Property(e => e.Comment)
        .HasColumnType("text")
        .HasColumnName("COMMENT");

    entity.Property(e => e.IsWon)
        .HasColumnName("IS_WON")
        .HasDefaultValue(false);

    entity.Property(e => e.CreatedByUserLogin)
        .HasMaxLength(250)
        .IsUnicode(false)
        .HasColumnName("CREATED_BY_USER_LOGIN")
        .IsRequired();

    entity.Property(e => e.CreatedStamp)
        .HasColumnType("datetime")
        .HasColumnName("CREATED_STAMP")
        .IsRequired();

    entity.Property(e => e.LastUpdatedStamp)
        .HasColumnType("datetime")
        .HasColumnName("LAST_UPDATED_STAMP")
        .IsRequired();

    entity.Property(e => e.CreatedTxStamp)
        .HasColumnType("datetime")
        .HasColumnName("CREATED_TX_STAMP");

    entity.Property(e => e.LastUpdatedTxStamp)
        .HasColumnType("datetime")
        .HasColumnName("LAST_UPDATED_TX_STAMP");

    // Foreign Key Relationships
    entity.HasOne(d => d.SalesOpportunity)
        .WithMany(p => p.SalesOpportunityActions)
        .HasForeignKey(d => d.SalesOpportunityId)
        .HasConstraintName("SLSOPPACT_SLSOPP")
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(d => d.ActionType)
        .WithMany()
        .HasForeignKey(d => d.ActionTypeId)
        .HasConstraintName("SLSOPPACT_ACTION_TYP")
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(d => d.NextActionType)
        .WithMany()
        .HasForeignKey(d => d.NextActionTypeId)
        .HasConstraintName("SLSOPPACT_NEXT_ACTION_TYP")
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(d => d.CancelReason)
        .WithMany()
        .HasForeignKey(d => d.CancelReasonId)
        .HasConstraintName("SLSOPPACT_CANCEL_RSN")
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(d => d.CreatedByUserLoginNavigation)
        .WithMany()
        .HasForeignKey(d => d.CreatedByUserLogin)
        .HasConstraintName("SLSOPPACT_USRLGN")
        .OnDelete(DeleteBehavior.Restrict);
});