modelBuilder.Entity<GlAccount>(entity =>
{
    entity.ToTable("GL_ACCOUNT");
    entity.HasKey(e => e.GlAccountId);

    // Existing properties...
    entity.Property(e => e.GlAccountId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_ACCOUNT_ID");

    entity.Property(e => e.GlAccountTypeId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_ACCOUNT_TYPE_ID");

    entity.Property(e => e.GlAccountClassId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_ACCOUNT_CLASS_ID");

    // ... other existing properties ...

    entity.Property(e => e.GlReportId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_REPORT_ID");

    entity.Property(e => e.GlClassCourseId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_CLASS_COURSE_ID");

    entity.Property(e => e.GlSubClassId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_SUB_CLASS_ID");

    entity.Property(e => e.GlSubClass2Id)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_SUB_CLASS2_ID");

    entity.Property(e => e.GlAccountCourseLabelId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_ACCOUNT_COURSE_LABEL_ID");

    // ==================== NEW PROPERTY ====================
    entity.Property(e => e.GlSubAccountCourseLabelId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_SUB_ACCOUNT_COURSE_LABEL_ID");
    // ======================================================

    // Timestamps
    entity.Property(e => e.LastUpdatedStamp).HasColumnName("LAST_UPDATED_STAMP");
    entity.Property(e => e.LastUpdatedTxStamp).HasColumnName("LAST_UPDATED_TX_STAMP");
    entity.Property(e => e.CreatedStamp).HasColumnName("CREATED_STAMP");
    entity.Property(e => e.CreatedTxStamp).HasColumnName("CREATED_TX_STAMP");

    // ==================== NEW RELATIONSHIP ====================
    entity.HasOne(e => e.GlSubAccountCourseLabel)
        .WithMany(e => e.GlAccounts)
        .HasForeignKey(e => e.GlSubAccountCourseLabelId)
        .OnDelete(DeleteBehavior.SetNull);   // Recommended for lookup tables
    // =========================================================
});