modelBuilder.Entity<AcctgTran>(entity =>
{
    entity.ToTable("ACCTG_TRANS");

    // ... your existing indexes and property configurations ...

    // REFACTOR: Added index on SalesRequestId for query performance
    entity.HasIndex(e => e.SalesRequestId, "IX_ACCTG_TRANS_SALES_REQUEST_ID");

    // REFACTOR: Map the new optional foreign key column
    // Length matches typical OFBiz primary key (36 chars), nullable.
    entity.Property(e => e.SalesRequestId)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("SALES_REQUEST_ID");

    // ... existing relationships ...

    // REFACTOR: Configure optional one-to-many relationship with SalesRequest
    // One SalesRequest → many AcctgTran
    // Optional: AcctgTran may or may not belong to a SalesRequest
    entity.HasOne(at => at.SalesRequest)
        .WithMany(sr => sr.AcctgTrans)
        .HasForeignKey(at => at.SalesRequestId)
        .OnDelete(DeleteBehavior.ClientSetNull)
        .HasConstraintName("FK_ACCTG_TRANS_SALES_REQUEST");
});