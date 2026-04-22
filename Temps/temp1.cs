entity.Property(e => e.Description)
    .HasMaxLength(4000)
    .IsUnicode(false)
    .HasColumnName("DESCRIPTION");