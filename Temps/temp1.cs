modelBuilder.Entity<WorkEffort>(entity =>
{
    entity.ToTable("WORK_EFFORT");

    // ... your existing configuration ...

    // ADD THESE LINES:

    // Property configuration
    entity.Property(e => e.CostCenterId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("COST_CENTER_ID");

    // Index (recommended for performance)
    entity.HasIndex(e => e.CostCenterId, "WK_EFFRT_COST_CENTER");

    // Relationship configuration
    entity.HasOne(d => d.CostCenter)
        .WithMany(p => p.Payments)           // Note: Currently CostCenter has Payments
        .HasForeignKey(d => d.CostCenterId)
        .IsRequired(false)                   // Optional relationship
        .HasConstraintName("WK_EFFRT_COST_CENTER")
        .OnDelete(DeleteBehavior.Restrict);  // Prevent accidental deletion of CostCenter

    // ... rest of your existing configuration
});