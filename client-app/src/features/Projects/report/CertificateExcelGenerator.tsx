import React, { useEffect, useState } from "react";
import { Button } from "@mui/material";
import { toast } from "react-toastify";
import { useFetchCertificateItemsQuery } from "../../../app/store/apis/certificateItemsApi";
import { saveAs } from "file-saver";
import {Certificate} from "../../../app/models/project/certificate";

interface Props {
  certificate: Certificate;
  getTranslatedLabel: (key: string, defaultValue: string) => string;
}

const CertificateExcelGenerator: React.FC<Props> = ({ certificate, getTranslatedLabel }) => {
  const [fetchItems, setFetchItems] = useState(false);
  const [hasFetched, setHasFetched] = useState(false);
  const { data: items, isLoading, isError, error, isFetching } = useFetchCertificateItemsQuery(
    certificate.workEffortId || "",
    { skip: !fetchItems || !certificate.workEffortId }
  );

  // REFACTOR: Add useEffect to handle query completion
  // Purpose: Ensures items are processed only after the query completes
  // Improvement: Prevents premature access to undefined data, fixes TypeError
  useEffect(() => {
    console.log("Query state:", {
      workEffortId: certificate.workEffortId,
      fetchItems,
      skip: !fetchItems || !certificate.workEffortId,
      isFetching,
      isLoading,
      isError,
      error,
      items,
    });

    if (fetchItems && !isFetching && !isLoading && hasFetched) {
      handleItemsProcessing();
    }
  }, [fetchItems, isFetching, isLoading, isError, error, items, hasFetched]);

  // REFACTOR: Update handleItemsProcessing to use shared utility functions
  // Purpose: Processes items and generates Excel using shared utilities
  // Improvement: Reduces code duplication, ensures consistency with batch export
  const handleItemsProcessing = async () => {
    if (isError) {
      console.error("Failed to fetch certificate items:", error);
      toast.error(
        getTranslatedLabel(
          "certificate.items.fetchError",
          "Failed to fetch certificate items"
        )
      );
      setFetchItems(false);
      setHasFetched(false);
      return;
    }

    const itemsArray = Array.isArray(items) ? items : [];
    if (!itemsArray.length) {
      console.warn(`No items found for workEffortId: ${certificate.workEffortId}`);
      toast.warn(
        getTranslatedLabel(
          "certificate.items.empty",
          "No items available for this certificate"
        )
      );
      setFetchItems(false);
      setHasFetched(false);
      return;
    }

    try {
      // Calculate subtotal
      const subtotal = itemsArray.reduce((sum: number, item: any) => {
        const total =
          certificate.certificateCategory === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
            ? item.net || 0
            : item.totalAmount || 0;
        return sum + total;
      }, 0);

      // Format items to match SupplyCertificateExcel/WorkmanshipCertificateExcel props
      const formattedItems = itemsArray.map((item: any) => ({
        ...item,
        productName: item.productName || "N/A",
        code: item.code || "N/A",
        description: item.description || "",
        quantity: item.quantity || 0,
        uomName: item.uomName || "N/A",
        unitPrice: item.unitPrice || 0,
        displayTotal: item.totalAmount || 0,
        discount: item.discount || 0,
        formattedProcurementDate: item.procurementDate
          ? new Date(item.procurementDate).toLocaleDateString("en-US")
          : "N/A",
        transportationExpenses: item.transportationExpenses || 0,
        gratuities: item.gratuities || 0,
        materialPrice: item.materialPrice || 0,
        laborPrice: item.laborPrice || 0,
        deductions: item.deductions || 0,
        deductionDescription: item.deductionDescription || "",
        deserved: item.deserved || 0,
        insurance: item.insurance || 0,
        additionalInsurance: item.additionalInsurance || 0,
        net: item.net || 0,
        achievementPercentage: item.achievementPercentage || "0%",
        isLastInGroup: item.isLastInGroup || false,
        productSubtotal: item.productSubtotal || 0,
        mainItemDescription: item.mainItemDescription || "",
        discountNote: item.discountNote || "",
      }));

      let buffer: ArrayBuffer | null = null;
      if (certificate.certificateCategory === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
        buffer = await generateWorkmanshipExcel(certificate, formattedItems, subtotal, getTranslatedLabel);
      } else {
        buffer = await generateSupplyExcel(certificate, formattedItems, subtotal, getTranslatedLabel);
      }

      if (buffer) {
        const blob = new Blob([buffer], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });
        const fileName =
          certificate.certificateCategory === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
            ? `WorkmanshipCertificate_${certificate.certificateNumber}.xlsx`
            : `SupplyCertificate_${certificate.certificateNumber}.xlsx`;
        saveAs(blob, fileName);
        toast.success(
          getTranslatedLabel("certificate.excel.success", "Excel report generated successfully")
        );
      }
    } catch (err) {
      console.error("Error generating Excel report:", err);
      toast.error(
        getTranslatedLabel("certificate.excel.error", "Failed to generate Excel report")
      );
    } finally {
      setFetchItems(false);
      setHasFetched(false);
    }
  };

  // REFACTOR: Update handleGenerateExcel to trigger query
  // Purpose: Simplifies button click handler, delegates processing to useEffect
  // Improvement: Ensures query completion before processing, fixes timing issues
  const handleGenerateExcel = () => {
    if (!certificate.workEffortId || !certificate.certificateNumber) {
      console.warn("Invalid certificate data:", {
        workEffortId: certificate.workEffortId,
        certificateNumber: certificate.certificateNumber,
      });
      toast.error(
        getTranslatedLabel(
          "certificate.noWorkEffortId",
          "No certificate selected or invalid certificate number"
        )
      );
      return;
    }

    console.log("Triggering fetch for workEffortId:", certificate.workEffortId);
    setFetchItems(true);
    setHasFetched(true);
  };

  return (
    <Button
      color="primary"
      variant="outlined"
      disabled={isLoading || isFetching || !certificate.certificateNumber}
      onClick={handleGenerateExcel}
    >
      {isLoading || isFetching
        ? getTranslatedLabel("certificate.excel.loading", "Generating...")
        : getTranslatedLabel("certificate.excel", "Generate Excel")}
    </Button>
  );
};

export default CertificateExcelGenerator;