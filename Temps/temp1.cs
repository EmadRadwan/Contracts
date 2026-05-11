// 1. Report Table Mapping
modelBuilder.Entity<GlReport>(entity =>
{
    entity.ToTable("GL_REPORT");
    entity.HasKey(e => e.GlReportId);

    entity.Property(e => e.GlReportId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_REPORT_ID");

    entity.Property(e => e.Description)
        .HasMaxLength(255)
        .IsUnicode()
        .HasColumnName("DESCRIPTION");
    
    entity.Property(e => e.DescriptionArabic)
        .HasMaxLength(255)
        .HasColumnName("DESCRIPTION_ARABIC");
    
});

// 2. Class Course Table Mapping
modelBuilder.Entity<GlClassCourse>(entity =>
{
    entity.ToTable("GL_CLASS_COURSE");
    entity.HasKey(e => e.GlClassCourseId);

    entity.Property(e => e.GlClassCourseId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_CLASS_COURSE_ID");

    entity.Property(e => e.Description)
        .HasMaxLength(255)
        .IsUnicode()
        .HasColumnName("DESCRIPTION");
    
    entity.Property(e => e.DescriptionArabic)
        .HasMaxLength(255)
        .HasColumnName("DESCRIPTION_ARABIC");
});

// 3. Sub Class Table Mapping
modelBuilder.Entity<GlSubClass>(entity =>
{
    entity.ToTable("GL_SUB_CLASS");
    entity.HasKey(e => e.GlSubClassId);

    entity.Property(e => e.GlSubClassId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_SUB_CLASS_ID");

    entity.Property(e => e.Description)
        .HasMaxLength(255)
        .IsUnicode()
        .HasColumnName("DESCRIPTION");
    
    entity.Property(e => e.DescriptionArabic)
        .HasMaxLength(255)
        .HasColumnName("DESCRIPTION_ARABIC");
});

// 4. Sub Class 2 Table Mapping (The Functional Area table)
modelBuilder.Entity<GlSubClass2>(entity =>
{
    entity.ToTable("GL_SUB_CLASS_2");
    entity.HasKey(e => e.GlSubClass2Id);

    entity.Property(e => e.GlSubClass2Id)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_SUB_CLASS_2_ID");

    entity.Property(e => e.Description)
        .HasMaxLength(255)
        .IsUnicode()
        .HasColumnName("DESCRIPTION");
    
    entity.Property(e => e.DescriptionArabic)
        .HasMaxLength(255)
        .HasColumnName("DESCRIPTION_ARABIC");
});

// 5. Account Course Label Table Mapping
modelBuilder.Entity<GlAccountCourseLabel>(entity =>
{
    entity.ToTable("GL_ACCOUNT_COURSE_LABEL");
    entity.HasKey(e => e.GlAccountCourseLabelId);

    entity.Property(e => e.GlAccountCourseLabelId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("GL_ACCOUNT_COURSE_LABEL_ID");

    entity.Property(e => e.Description)
        .HasMaxLength(255)
        .IsUnicode()
        .HasColumnName("DESCRIPTION");
    
    entity.Property(e => e.DescriptionArabic)
        .HasMaxLength(255)
        .HasColumnName("DESCRIPTION_ARABIC");

    entity.Property(e => e.SignMultiplier)
        .HasDefaultValue(1)
        .HasColumnName("SIGN_MULTIPLIER");
});