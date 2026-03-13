modelBuilder.Entity<SalesOpportunityProduct>(entity =>
{
    entity.ToTable("SALES_OPPORTUNITY_PRODUCT");

    entity.Property(e => e.Id)
          .HasMaxLength(36)
          .IsUnicode(false)
          .HasColumnName("SALES_OPPORTUNITY_PRODUCT_ID");
    entity.HasKey(e => e.SalesOpportunityProductId);

    // Indexes for fast lookups (similar to your SLSOPP_... pattern)
    entity.HasIndex(e => e.SalesOpportunityId, "SLSOPPRD_OPPID");
    entity.HasIndex(e => e.ProductId, "SLSOPPRD_PRODID");
    entity.HasIndex(e => e.WorkEffortId, "SLSOPPRD_WKEFFID");

    // Column mappings
    entity.Property(e => e.SalesOpportunityId)
          .HasMaxLength(36)
          .IsUnicode(false)
          .HasColumnName("SALES_OPPORTUNITY_ID");

    entity.Property(e => e.ProductId)
          .HasMaxLength(36)
          .IsUnicode(false)
          .HasColumnName("PRODUCT_ID")
          .IsRequired(false); // nullable

    entity.Property(e => e.WorkEffortId)
          .HasMaxLength(36)
          .IsUnicode(false)
          .HasColumnName("WORK_EFFORT_ID")
          .IsRequired(false); // nullable
    

    entity.Property(e => e.Quantity)
          .HasColumnType("decimal(18,3)")
          .HasDefaultValue(1m)
          .HasColumnName("QUANTITY");
    

    entity.Property(e => e.Notes)
          .HasColumnType("text")
          .HasColumnName("NOTES");

    entity.Property(e => e.FromDate)
          .HasColumnType("datetime")
          .HasColumnName("FROM_DATE")
          .IsRequired(); // part of PK

    entity.Property(e => e.ThruDate)
          .HasColumnType("datetime")
          .HasColumnName("THRU_DATE");

    // Audit fields (matching your pattern)
    entity.Property(e => e.CreatedStamp)
          .HasColumnType("datetime")
          .HasColumnName("CREATED_STAMP");

    entity.Property(e => e.LastUpdatedStamp)
          .HasColumnType("datetime")
          .HasColumnName("LAST_UPDATED_STAMP");

    entity.Property(e => e.CreatedByUserLogin)
          .HasMaxLength(250)
          .IsUnicode(false)
          .HasColumnName("CREATED_BY_USER_LOGIN");

    entity.Property(e => e.LastModifiedByUserLogin)
          .HasMaxLength(250)
          .IsUnicode(false)
          .HasColumnName("LAST_MODIFIED_BY_USER_LOGIN");

    // Foreign keys with OFBiz-style constraint names
    entity.HasOne(d => d.SalesOpportunity)
          .WithMany(p => p.SalesOpportunityProducts)
          .HasForeignKey(d => d.SalesOpportunityId)
          .OnDelete(DeleteBehavior.Cascade)
          .HasConstraintName("SLSOPPRD_SLSOPP");

    entity.HasOne(d => d.Product)
          .WithMany() // no reverse collection needed unless you add it to Product
          .HasForeignKey(d => d.ProductId)
          .OnDelete(DeleteBehavior.Restrict) // don't allow deleting apartment if referenced
          .HasConstraintName("SLSOPPRD_PROD");

    entity.HasOne(d => d.WorkEffort)
          .WithMany() // no reverse collection unless added to WorkEffort
          .HasForeignKey(d => d.WorkEffortId)
          .OnDelete(DeleteBehavior.Restrict)
          .HasConstraintName("SLSOPPRD_WKEFF");
});