entity.HasOne(d => d.SubProject)
    .WithMany()
    .HasForeignKey(d => d.SubProjectId)
    .IsRequired(false) // Explicitly mark as optional
    .HasConstraintName("WK_EFFRT_SUBPROJECT")
    .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of parent from affecting child