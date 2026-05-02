entity.Property(e => e.DescriptionArabic)
    .HasMaxLength(255)
    .IsUnicode(false)
    .HasColumnName("DESCRIPTION_ARABIC");