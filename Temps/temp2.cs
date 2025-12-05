protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // === CostCenter entity ===
    modelBuilder.Entity<CostCenter>(entity =>
    {
        entity.HasKey(e => e.CostCenterId)
              .HasName("PK_COST_CENTER");

        entity.ToTable("CostCenter");

        entity.Property(e => e.CostCenterId)
              .HasMaxLength(20)
              .IsUnicode(false)
              .HasColumnName("COST_CENTER_ID");

        entity.Property(e => e.Description)
              .HasMaxLength(255)
              .IsUnicode(false)
              .HasColumnName("DESCRIPTION");

        entity.Property(e => e.IsOutPayment)
              .HasMaxLength(1)
              .IsUnicode(false)
              .HasColumnName("IS_OUT_PAYMENT")
              .HasDefaultValue("N")
              .IsFixedLength();

        // Standard timestamp columns
        entity.Property(e => e.LastUpdatedStamp).HasColumnName("LAST_UPDATED_STAMP");
        entity.Property(e => e.LastUpdatedTxStamp).HasColumnName("LAST_UPDATED_TX_STAMP");
        entity.Property(e => e.CreatedStamp).HasColumnName("CREATED_STAMP");
        entity.Property(e => e.CreatedTxStamp).HasColumnName("CREATED_TX_STAMP");
    });

    // === Payment entity – add CostCenter relationship ===
    modelBuilder.Entity<Payment>(entity =>
    {
        // ... your existing Payment config ...

        // REFACTOR: CostCenterId foreign key (same style as WorkEffortId)
        entity.Property(e => e.CostCenterId)
              .HasMaxLength(20)
              .IsUnicode(false)
              .HasColumnName("COST_CENTER_ID");

        // REFACTOR: One CostCenter → Many Payments
        entity.HasOne(p => p.CostCenter)
              .WithMany(cc => cc.Payments)
              .HasForeignKey(p => p.CostCenterId)
              .HasConstraintName("FK_PAYMENT_COST_CENTER");

        // REFACTOR: Index for performance (very common filter in reports)
        entity.HasIndex(p => p.CostCenterId)
              .HasDatabaseName("IX_PAYMENT_COST_CENTER_ID");
    });
}