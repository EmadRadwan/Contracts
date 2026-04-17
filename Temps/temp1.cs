// ... existing indexes ...

entity.HasIndex(e => e.OperatingExpenseGlAccountId, "WK_EFFRT_OP_EXP_GL");

// ... existing properties ...

entity.Property(e => e.OperatingExpenseGlAccountId)
    .HasMaxLength(36)
    .IsUnicode(false)
    .HasColumnName("OPERATING_EXPENSE_GL_ACCOUNT_ID");

// ... existing relationships ...

// Project GL Account (existing)
entity.HasOne(we => we.ProjectGlAccount)
    .WithMany(gl => gl.WorkEfforts)
    .HasForeignKey(we => we.GlAccountId);

// NEW: Operating Expense GL Account
entity.HasOne(d => d.OperatingExpenseGlAccount)
    .WithMany() 
    .HasForeignKey(d => d.OperatingExpenseGlAccountId)
    .IsRequired(false)
    .HasConstraintName("WK_EFFRT_OP_EXP_GL")
    .OnDelete(DeleteBehavior.Restrict);