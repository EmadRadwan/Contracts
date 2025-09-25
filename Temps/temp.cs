 modelBuilder.Entity<WorkEffort>(entity =>
            {
                entity.ToTable("WORK_EFFORT");


                entity.HasIndex(e => e.CurrentStatusId, "WK_EFFRT_CURSTTS");

                entity.HasIndex(e => e.EstimateCalcMethod, "WK_EFFRT_CUS_MET");

                entity.HasIndex(e => e.FacilityId, "WK_EFFRT_FACILITY");

                entity.HasIndex(e => e.FixedAssetId, "WK_EFFRT_FXDASST");

                entity.HasIndex(e => e.NoteId, "WK_EFFRT_NOTE");

                entity.HasIndex(e => e.WorkEffortParentId, "WK_EFFRT_PARENT");

                entity.HasIndex(e => e.WorkEffortPurposeTypeId, "WK_EFFRT_PRPTYP");

                entity.HasIndex(e => e.WorkEffortTypeId, "WK_EFFRT_TYPE");

                entity.HasIndex(e => e.CreatedTxStamp, "WORK_EFFORT_TXCRTS");

                entity.HasIndex(e => e.LastUpdatedTxStamp, "WORK_EFFORT_TXSTMP");

                entity.Property(e => e.WorkEffortId)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("WORK_EFFORT_ID");

                entity.Property(e => e.ActualCompletionDate)
                    .HasColumnType("datetime")
                    .HasColumnName("ACTUAL_COMPLETION_DATE");

                entity.Property(e => e.ActualMilliSeconds).HasColumnName("ACTUAL_MILLI_SECONDS");

                entity.Property(e => e.ActualSetupMillis).HasColumnName("ACTUAL_SETUP_MILLIS");

                entity.Property(e => e.ActualStartDate)
                    .HasColumnType("datetime")
                    .HasColumnName("ACTUAL_START_DATE");

                entity.Property(e => e.CreatedByUserLogin)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("CREATED_BY_USER_LOGIN");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("CREATED_DATE");

                entity.Property(e => e.CreatedStamp)
                    .HasColumnType("datetime")
                    .HasColumnName("CREATED_STAMP");

                entity.Property(e => e.CreatedTxStamp)
                    .HasColumnType("datetime")
                    .HasColumnName("CREATED_TX_STAMP");

                entity.Property(e => e.CurrentStatusId)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("CURRENT_STATUS_ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(4000)
                    .IsUnicode(false)
                    .HasColumnName("DESCRIPTION");

				entity.Property(e => e.DeductionDescription)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .HasColumnName("DEDUCTION_DESCRIPTION");

                entity.Property(e => e.EstimateCalcMethod)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("ESTIMATE_CALC_METHOD");

                entity.Property(e => e.EstimatedCompletionDate)
                    .HasColumnType("datetime")
                    .HasColumnName("ESTIMATED_COMPLETION_DATE");

                entity.Property(e => e.EstimatedMilliSeconds).HasColumnName("ESTIMATED_MILLI_SECONDS");

                entity.Property(e => e.EstimatedSetupMillis).HasColumnName("ESTIMATED_SETUP_MILLIS");

                entity.Property(e => e.EstimatedStartDate)
                    .HasColumnType("datetime")
                    .HasColumnName("ESTIMATED_START_DATE");

                entity.Property(e => e.FacilityId)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("FACILITY_ID");

                entity.Property(e => e.FixedAssetId)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("FIXED_ASSET_ID");


                entity.Property(e => e.LastModifiedByUserLogin)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("LAST_MODIFIED_BY_USER_LOGIN");

                entity.Property(e => e.LastModifiedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("LAST_MODIFIED_DATE");

                entity.Property(e => e.LastStatusUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("LAST_STATUS_UPDATE");

                entity.Property(e => e.LastUpdatedStamp)
                    .HasColumnType("datetime")
                    .HasColumnName("LAST_UPDATED_STAMP");

                entity.Property(e => e.LastUpdatedTxStamp)
                    .HasColumnType("datetime")
                    .HasColumnName("LAST_UPDATED_TX_STAMP");

                entity.Property(e => e.NoteId)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("NOTE_ID");


                entity.Property(e => e.Priority).HasColumnName("PRIORITY");

                entity.Property(e => e.QuantityProduced)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("QUANTITY_PRODUCED");

                entity.Property(e => e.QuantityRejected)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("QUANTITY_REJECTED");

                entity.Property(e => e.QuantityToProduce)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("QUANTITY_TO_PRODUCE");

                entity.Property(e => e.Reserv2ndPPPerc)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("RESERV2ND_P_P_PERC");

                entity.Property(e => e.ReservNthPPPerc)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("RESERV_NTH_P_P_PERC");

                entity.Property(e => e.ReservPersons)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("RESERV_PERSONS");

                entity.Property(e => e.RevisionNumber).HasColumnName("REVISION_NUMBER");

               
                entity.Property(e => e.WorkEffortName)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("WORK_EFFORT_NAME");

                entity.Property(e => e.WorkEffortParentId)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("WORK_EFFORT_PARENT_ID");

                entity.Property(e => e.WorkEffortPurposeTypeId)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("WORK_EFFORT_PURPOSE_TYPE_ID");

                entity.Property(e => e.WorkEffortTypeId)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("WORK_EFFORT_TYPE_ID");

             

                entity.HasOne(d => d.CurrentStatus)
                    .WithMany(p => p.WorkEfforts)
                    .HasForeignKey(d => d.CurrentStatusId)
                    .HasConstraintName("WK_EFFRT_CURSTTS");

                entity.HasOne(d => d.EstimateCalcMethodNavigation)
                    .WithMany(p => p.WorkEfforts)
                    .HasForeignKey(d => d.EstimateCalcMethod)
                    .HasConstraintName("WK_EFFRT_CUS_MET");

                   entity.HasOne(d => d.Facility)
                   .WithMany(p => p.WorkEfforts)
                   .HasForeignKey(d => d.FacilityId)
                   .IsRequired(false) 
                   .HasConstraintName("WK_EFFRT_FACILITY");

                entity.HasOne(d => d.FixedAsset)
                    .WithMany(p => p.WorkEfforts)
                    .HasForeignKey(d => d.FixedAssetId)
                    .HasConstraintName("WK_EFFRT_FXDASST");

           

                entity.HasOne(d => d.Note)
                    .WithMany(p => p.WorkEfforts)
                    .HasForeignKey(d => d.NoteId)
                    .HasConstraintName("WK_EFFRT_NOTE");

          

                entity.HasOne(d => d.WorkEffortParent)
                    .WithMany(p => p.InverseWorkEffortParent)
                    .HasForeignKey(d => d.WorkEffortParentId)
                    .HasConstraintName("WK_EFFRT_PARENT");

                entity.HasOne(d => d.WorkEffortPurposeType)
                    .WithMany(p => p.WorkEfforts)
                    .HasForeignKey(d => d.WorkEffortPurposeTypeId)
                    .HasConstraintName("WK_EFFRT_PRPTYP");

                entity.HasOne(d => d.WorkEffortType)
                    .WithMany(p => p.WorkEfforts)
                    .HasForeignKey(d => d.WorkEffortTypeId)
                    .HasConstraintName("WK_EFFRT_TYPE");
                    
                 
                    
                 entity.Property(e => e.ProjectNum)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("PROJECT_NUM");
            
                entity.Property(e => e.CertificateNumber)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("CERTIFICATE_NUMBER");
            
                entity.Property(e => e.ProjectName)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("PROJECT_NAME");
            
                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("TOTAL_AMOUNT");
            
                entity.Property(e => e.ProjectId)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("PROJECT_ID");
            
               entity.Property(e => e.PartyIdSupplier)
        		.HasMaxLength(36)
        		.IsUnicode(false)
       			 .HasColumnName("PARTY_ID_SUPPLIER");
       			 
    entity.Property(e => e.PartyIdContractor)
        .HasMaxLength(36)
        .IsUnicode(false)
        .HasColumnName("PARTY_ID_CONTRACTOR");
            
                entity.Property(e => e.RelatedOrderId)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("RELATED_ORDER_ID");
            
                entity.Property(e => e.CertificateCategory)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("CERTIFICATE_CATEGORY");
            
                entity.Property(e => e.SupplierOrContractorType)
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .HasColumnName("SUPPLIER_OR_CONTRACTOR_TYPE");
            
                entity.Property(e => e.LineNumber)
                    .HasColumnName("LINE_NUMBER");
            
                entity.Property(e => e.Quantity)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("QUANTITY");
            
                entity.Property(e => e.Rate)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("RATE");
                    
                    entity.Property(e => e.MaterialPrice)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("MATERIAL_PRICE");
                    
            entity.Property(e => e.LaborPrice)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("LABOR_PRICE");
            
                entity.Property(e => e.CompletionPercentage)
                    .HasColumnType("decimal(5, 2)")
                    .HasColumnName("COMPLETION_PERCENTAGE");
            
                entity.Property(e => e.DueAmount)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("DUE_AMOUNT");
            
                entity.Property(e => e.PaidAmount)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("PAID_AMOUNT");
            
                entity.Property(e => e.RemainingAmount)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("REMAINING_AMOUNT");
            
                entity.Property(e => e.Notes)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("NOTES");
            
                entity.Property(e => e.ProductId)
                    .HasColumnName("PRODUCT_ID");
            
                entity.HasIndex(e => e.ProjectId, "WK_EFFRT_PROJECT");
                 entity.HasIndex(e => e.PartyIdSupplier, "WK_EFFRT_SUPPLIER");
    entity.HasIndex(e => e.PartyIdContractor, "WK_EFFRT_CONTRACTOR");
    entity.HasIndex(e => new { e.PartyIdSupplier, e.PartyIdContractor, e.CertificateCategory }, "WK_EFFRT_SUPPLIER_CONTRACTOR_CERTCAT").IsUnique(false);

                entity.HasIndex(e => e.RelatedOrderId, "WK_EFFRT_RELATED_ORDER");
                entity.HasIndex(e => e.ProductId, "WK_EFFRT_PRODUCT");
                entity.HasIndex(e => e.CertificateNumber, "WK_EFFRT_CERT_NUM");
                
            
        entity.HasOne(d => d.SupplierParty)
        .WithMany(p => p.WorkEffortsAsSupplier)
        .HasForeignKey(d => d.PartyIdSupplier)
        .HasConstraintName("WK_EFFRT_SUPPLIER");

    entity.HasOne(d => d.ContractorParty)
        .WithMany(p => p.WorkEffortsAsContractor)
        .HasForeignKey(d => d.PartyIdContractor)
        .HasConstraintName("WK_EFFRT_CONTRACTOR");
            
                entity.HasOne(d => d.Project)
                    .WithMany() 
                    .HasForeignKey(d => d.ProjectId)
                    .HasConstraintName("WK_EFFRT_PROJECT")
                    .OnDelete(DeleteBehavior.Restrict); 
            
            
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.WorkEfforts)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("WK_EFFRT_PRODUCT");
            
                entity.HasOne(d => d.RelatedOrder)
                    .WithMany(p => p.WorkEfforts)
                    .HasForeignKey(d => d.RelatedOrderId)
                    .HasConstraintName("WK_EFFRT_RELATED_ORDER");
                       
                       entity.Property(e => e.Discount)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("DISCOUNT"); 
            
                entity.Property(e => e.Deductions)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("DEDUCTIONS"); 
            
                entity.Property(e => e.Insurance)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("INSURANCE");
            
                entity.Property(e => e.AdditionalInsurance)
                    .HasColumnType("decimal(18,3)")
                    .HasColumnName("ADDITIONAL_INSURANCE");
            
 

                        });
                  