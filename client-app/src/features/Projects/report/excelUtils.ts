import ExcelJS from "exceljs";
import { saveAs } from "file-saver";

export interface Certificate {
  workEffortId: string;
  certificateNumber: string;
  certificateCategory: string;
  certificateCategoryDescription?: string;
  projectId: string;
  description?: string;
  partyNameSupplier?: string;
  partyNameContractor?: string;
  facilityName?: string;
}

export interface CertificateItem {
  id: string;
  workEffortId: string;
  workEffortParentId?: string;
  productId: string;
  productName: string;
  code?: string;
  description?: string;
  quantity: number;
  uomId?: string;
  uomName?: string;
  unitPrice?: number;
  totalAmount?: number;
  discount?: number;
  procurementDate?: string;
  transportationExpenses?: number;
  gratuities?: number;
  materialPrice?: number;
  laborPrice?: number;
  deductions?: number;
  deductionDescription?: string;
  deserved?: number;
  insurance?: number;
  additionalInsurance?: number;
  net?: number;
  achievementPercentage?: string | number;
  isLastInGroup?: boolean;
  productSubtotal?: number;
  mainItemDescription?: string;
  discountNote?: string;
}

export const sharedUtils = {
  safeString: (value: any): string => {
    if (value === null || value === undefined) return "N/A";
    if (typeof value === "object") {
      console.warn("safeString received object:", value);
      return "N/A";
    }
    if (typeof value === "number") return value.toString();
    return String(value);
  },
  rtlEmbed: (text: string): string => {
    return /\p{Script=Arabic}/u.test(text) ? `\u202B${text}` : text;
  },
  certificateTypeTranslations: {
    SUPPLY_PROCUREMENT_CERTIFICATE: "مستخلص توريدات",
    COMPANY_SUPPLY_SALE_CERTIFICATE: "مستخلص توريدات من مخازن الشركة",
    WORKMANSHIP_CONTRACTING_CERTIFICATE: "مستخلص مقاوله",
  },
  formatNumber: (value: number | undefined, decimals: number = 2): string => {
    if (value === undefined || value === null) return "N/A";
    return value.toLocaleString("en-US", {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals,
    });
  },
};

// REFACTOR: Add validation for items
// Purpose: Ensures items have required fields to prevent errors in Excel generation
// Improvement: Catches missing or invalid data early, logs errors for debugging
export const validateItems = (items: CertificateItem[], certificateType: string) => {
  const validationResults = items.map((item, index) => {
    const errors: string[] = [];
    if (!item.productName) errors.push("productName is missing");
    // REFACTOR: Make code optional
    // Purpose: Allows items without code to pass validation
    // Improvement: Matches API data where code may be undefined
    if (item.quantity === undefined || item.quantity < 0)
      errors.push("quantity is invalid");
    if (certificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
      if (item.materialPrice === undefined || item.laborPrice === undefined)
        errors.push("materialPrice/laborPrice missing");
    } else {
      if (item.unitPrice === undefined) errors.push("unitPrice is missing");
    }
    return { index, errors, item };
  });
  const invalidItems = validationResults.filter((result) => result.errors.length > 0);
  if (invalidItems.length > 0) {
    console.error("Invalid items detected:", invalidItems);
  }
  return { isValid: invalidItems.length === 0, invalidItems };
};

// REFACTOR: Extract generateExcel logic for Supply certificates
// Purpose: Generate Excel worksheets for Supply certificates
// Improvement: Maintains consistency with ProjectCertificateForm reports
export const generateSupplyExcel = async (
  certificate: Certificate,
  items: CertificateItem[],
  subtotal: number,
  getTranslatedLabel: (key: string, defaultValue: string) => string
) => {
  const workbook = new ExcelJS.Workbook();
  workbook.created = new Date();
  workbook.creator = "System";

  const pageSize = 15;
  const pages = [];
  for (let i = 0; i < items.length; i += pageSize) {
    pages.push(items.slice(i, i + pageSize));
  }

  const { isValid } = validateItems(
    items,
    certificate.certificateCategory || "SUPPLY_PROCUREMENT_CERTIFICATE"
  );
  if (!isValid) {
    console.error("Cannot generate Supply Excel: Invalid items");
    return null;
  }

  let logoImageId: number | null = null;
  try {
    const response = await fetch("/goldenlandlogo.jpg");
    if (!response.ok) throw new Error("Failed to fetch logo");
    const blob = await response.blob();
    const arrayBuffer = await blob.arrayBuffer();
    logoImageId = workbook.addImage({
      buffer: arrayBuffer,
      extension: "jpeg",
    });
  } catch (error) {
    console.warn("Logo fetch failed:", error);
  }

  pages.forEach((pageItems, pageIndex) => {
    const worksheet = workbook.addWorksheet(`Page ${pageIndex + 1}`);
    worksheet.pageSetup = { paperSize: 9, orientation: "landscape" };
    worksheet.views = [{ rightToLeft: true }];
    worksheet.getColumn(1).font = { name: "Amiri", size: 10 };

    if (logoImageId !== null) {
      worksheet.addImage(logoImageId, {
        tl: { col: 0, row: 0 },
        ext: { width: 100, height: 100 },
        editAs: "absolute",
      });
      worksheet.getRow(1).height = 75;
      worksheet.getRow(2).height = 20;
      worksheet.getRow(3).height = 20;
      worksheet.addRow([]);
      worksheet.addRow([]);
      worksheet.addRow([]);
    } else {
      worksheet.addRow(["Logo Unavailable"]);
      worksheet.getRow(1).font = { name: "Amiri", size: 10, color: { argb: "FF0000" } };
      worksheet.getRow(1).alignment = { horizontal: "center", vertical: "middle" };
    }

    worksheet.addRow([
      getTranslatedLabel("projects.certificate.report.title", "Certificate Report") +
        ": " +
        sharedUtils.safeString(certificate.certificateNumber),
    ]);
    worksheet.mergeCells(`A${logoImageId !== null ? 4 : 2}:K${logoImageId !== null ? 4 : 2}`);
    worksheet.getRow(logoImageId !== null ? 4 : 2).font = {
      name: "Amiri",
      size: 14,
      bold: true,
    };
    worksheet.getRow(logoImageId !== null ? 4 : 2).alignment = {
      horizontal: "center",
      vertical: "middle",
      wrapText: true,
    };

    worksheet.addRow([
      getTranslatedLabel("projects.certificate.type", "Type") +
        ": " +
        (sharedUtils.certificateTypeTranslations[
          certificate.certificateCategory || "SUPPLY_PROCUREMENT_CERTIFICATE"
        ] || certificate.certificateCategory),
      getTranslatedLabel("projects.certificate.date", "Date") +
        ": " +
        new Date().toLocaleDateString("en-US", {
          year: "numeric",
          month: "numeric",
          day: "numeric",
        }),
    ]);
    worksheet.addRow([
      getTranslatedLabel("projects.certificate.description", "Description") +
        ": " +
        sharedUtils.safeString(certificate.description),
      getTranslatedLabel("projects.certificate.form.supplier", "Supplier") +
        ": " +
        sharedUtils.safeString(certificate.partyNameSupplier),
    ]);
    worksheet.addRow([
      getTranslatedLabel("projects.certificate.total", "Total") +
        ": " +
        sharedUtils.formatNumber(subtotal),
      getTranslatedLabel("projects.certificate.form.facility", "Facility") +
        ": " +
        sharedUtils.safeString(certificate.facilityName),
    ]);
    worksheet.getRow(logoImageId !== null ? 5 : 3).font = { name: "Amiri", size: 10 };
    worksheet.getRow(logoImageId !== null ? 6 : 4).font = { name: "Amiri", size: 10 };
    worksheet.getRow(logoImageId !== null ? 7 : 5).font = { name: "Amiri", size: 10 };
    worksheet.getRow(logoImageId !== null ? 5 : 3).alignment = {
      horizontal: "right",
      wrapText: true,
    };
    worksheet.getRow(logoImageId !== null ? 6 : 4).alignment = {
      horizontal: "right",
      wrapText: true,
    };
    worksheet.getRow(logoImageId !== null ? 7 : 5).alignment = {
      horizontal: "right",
      wrapText: true,
    };

    worksheet.addRow([]);

    const isSupplyWithDiscount =
      certificate.certificateCategory === "SUPPLY_PROCUREMENT_CERTIFICATE";
    const headers = [
      getTranslatedLabel("projects.certificate.items.list.item", "Item"),
      getTranslatedLabel("projects.certificate.items.list.code", "Code"),
      getTranslatedLabel("projects.certificate.items.list.description", "Description"),
      getTranslatedLabel("projects.certificate.items.list.quantity", "Quantity"),
      getTranslatedLabel("projects.certificate.items.list.unitOfMeasure", "Unit of Measure"),
      getTranslatedLabel("projects.certificate.items.list.unitPrice", "Unit Price"),
      getTranslatedLabel("projects.certificate.items.list.totalAmount", "Total Amount"),
      ...(isSupplyWithDiscount
        ? [getTranslatedLabel("projects.certificate.items.list.discount", "Discount")]
        : []),
      getTranslatedLabel("projects.certificate.items.list.procurementDate", "Procurement Date"),
      getTranslatedLabel("projects.certificate.items.list.transportationExpenses", "Transportation Expenses"),
      getTranslatedLabel("projects.certificate.items.list.gratuities", "Gratuities"),
    ];
    worksheet.addRow(headers);
    const headerRow = worksheet.getRow(worksheet.lastRow!.number);
    headerRow.font = { name: "Amiri", size: 10, bold: true };
    headerRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "F0F0F0" } };
    headerRow.alignment = { horizontal: "center", vertical: "middle", wrapText: true };
    headerRow.eachCell((cell) => {
      cell.border = {
        top: { style: "thin" },
        left: { style: "thin" },
        bottom: { style: "thin" },
        right: { style: "thin" },
      };
    });

    worksheet.columns = [
      { width: 40 }, // Item
      { width: 40 }, // Code
      { width: 100 }, // Description
      { width: 8 }, // Quantity
      { width: 12 }, // Unit of Measure
      { width: 10 }, // Unit Price
      { width: 10 }, // Total Amount
      ...(isSupplyWithDiscount ? [{ width: 10 }] : []), // Discount
      { width: 15 }, // Procurement Date
      { width: 10 }, // Transportation Expenses
      { width: 10 }, // Gratuities
    ];

    worksheet.getColumn(4).numFmt = "0";
    worksheet.getColumn(6).numFmt = "#,##0.00";
    worksheet.getColumn(7).numFmt = "#,##0.00";
    const discountCol = isSupplyWithDiscount ? 8 : 7;
    if (isSupplyWithDiscount) worksheet.getColumn(discountCol).numFmt = "#,##0.00";
    worksheet.getColumn(discountCol + 1).numFmt = "@";
    worksheet.getColumn(discountCol + 2).numFmt = "#,##0.00";
    worksheet.getColumn(discountCol + 3).numFmt = "#,##0.00";

    pageItems.forEach((item, index) => {
      const rowData = [
        item.isLastInGroup && item.productSubtotal !== undefined
          ? `${sharedUtils.rtlEmbed(sharedUtils.safeString(item.productName))} (${
              sharedUtils.formatNumber(item.productSubtotal, 2)
            })`
          : sharedUtils.rtlEmbed(sharedUtils.safeString(item.productName)),
        sharedUtils.safeString(item.code),
        sharedUtils.rtlEmbed(sharedUtils.safeString(item.description)),
        item.quantity !== undefined ? item.quantity : "N/A",
        sharedUtils.rtlEmbed(sharedUtils.safeString(item.uomName)),
        item.unitPrice !== undefined ? item.unitPrice : "N/A",
        item.displayTotal !== undefined ? item.displayTotal : "N/A",
        ...(isSupplyWithDiscount
          ? [item.discount !== undefined ? item.discount : "N/A"]
          : []),
        sharedUtils.safeString(item.formattedProcurementDate),
        item.transportationExpenses !== undefined ? item.transportationExpenses : "N/A",
        item.gratuities !== undefined ? item.gratuities : "N/A",
      ];
      const row = worksheet.addRow(rowData);
      row.font = { name: "Amiri", size: 9 };
      row.alignment = { horizontal: "right", vertical: "middle", wrapText: true };
      row.eachCell((cell) => {
        cell.border = {
          top: { style: "thin" },
          left: { style: "thin" },
          bottom: { style: "thin" },
          right: { style: "thin" },
        };
      });
    });

    if (pageItems.some((item) => item.mainItemDescription && item.mainItemDescription.trim())) {
      worksheet.addRow([]);
      worksheet.addRow([
        getTranslatedLabel(
          "projects.certificate.items.mainDescription",
          "Main Item Description"
        ),
      ]);
      worksheet.getRow(worksheet.lastRow!.number).font = {
        name: "Amiri",
        size: 10,
        bold: true,
      };
      pageItems.forEach((item) => {
        if (item.mainItemDescription && item.mainItemDescription.trim()) {
          worksheet.addRow([
            sharedUtils.rtlEmbed(sharedUtils.safeString(item.mainItemDescription)),
          ]);
          worksheet.getRow(worksheet.lastRow!.number).font = { name: "Amiri", size: 9 };
          worksheet.getRow(worksheet.lastRow!.number).alignment = {
            horizontal: "right",
            wrapText: true,
          };
        }
      });
    }

    if (pageItems.some((item) => item.discountNote && item.discountNote.trim())) {
      worksheet.addRow([]);
      worksheet.addRow([
        getTranslatedLabel(
          "projects.certificate.items.discountNote",
          "Discount Description Note"
        ),
      ]);
      worksheet.getRow(worksheet.lastRow!.number).font = {
        name: "Amiri",
        size: 10,
        bold: true,
      };
      pageItems.forEach((item) => {
        if (item.discountNote && item.discountNote.trim()) {
          worksheet.addRow([
            sharedUtils.rtlEmbed(sharedUtils.safeString(item.discountNote)),
          ]);
          worksheet.getRow(worksheet.lastRow!.number).font = { name: "Amiri", size: 9 };
          worksheet.getRow(worksheet.lastRow!.number).alignment = {
            horizontal: "right",
            wrapText: true,
          };
        }
      });
    }
  });

  const buffer = await workbook.xlsx.writeBuffer();
  return buffer;
};

// REFACTOR: Extract generateExcel logic for Workmanship certificates
// Purpose: Generate Excel worksheets for Workmanship certificates
// Improvement: Maintains consistency with ProjectCertificateForm reports
export const generateWorkmanshipExcel = async (
  certificate: Certificate,
  items: CertificateItem[],
  subtotal: number,
  getTranslatedLabel: (key: string, defaultValue: string) => string
) => {
  const workbook = new ExcelJS.Workbook();
  workbook.created = new Date();
  workbook.creator = "System";

  const pageSize = 15;
  const pages = [];
  for (let i = 0; i < items.length; i += pageSize) {
    pages.push(items.slice(i, i + pageSize));
  }

  const { isValid } = validateItems(items, "WORKMANSHIP_CONTRACTING_CERTIFICATE");
  if (!isValid) {
    console.error("Cannot generate Workmanship Excel: Invalid items");
    return null;
  }

  let logoImageId: number | null = null;
  try {
    const response = await fetch("/goldenlandlogo.jpg");
    if (!response.ok) throw new Error("Failed to fetch logo");
    const blob = await response.blob();
    const arrayBuffer = await blob.arrayBuffer();
    logoImageId = workbook.addImage({
      buffer: arrayBuffer,
      extension: "jpeg",
    });
  } catch (error) {
    console.warn("Logo fetch failed:", error);
  }

  pages.forEach((pageItems, pageIndex) => {
    const worksheet = workbook.addWorksheet(`Page ${pageIndex + 1}`);
    worksheet.pageSetup = { paperSize: 9, orientation: "landscape" };
    worksheet.views = [{ rightToLeft: true }];
    worksheet.getColumn(1).font = { name: "Amiri", size: 10 };

    if (logoImageId !== null) {
      worksheet.addImage(logoImageId, {
        tl: { col: 0, row: 0 },
        ext: { width: 100, height: 100 },
        editAs: "absolute",
      });
      worksheet.getRow(1).height = 75;
      worksheet.getRow(2).height = 20;
      worksheet.getRow(3).height = 20;
      worksheet.addRow([]);
      worksheet.addRow([]);
      worksheet.addRow([]);
    } else {
      worksheet.addRow(["Logo Unavailable"]);
      worksheet.getRow(1).font = { name: "Amiri", size: 10, color: { argb: "FF0000" } };
      worksheet.getRow(1).alignment = { horizontal: "center", vertical: "middle" };
    }

    worksheet.addRow([
      getTranslatedLabel("projects.certificate.report.title", "Certificate Report") +
        ": " +
        sharedUtils.safeString(certificate.certificateNumber),
    ]);
    worksheet.mergeCells(`A${logoImageId !== null ? 4 : 2}:O${logoImageId !== null ? 4 : 2}`);
    worksheet.getRow(logoImageId !== null ? 4 : 2).font = {
      name: "Amiri",
      size: 14,
      bold: true,
    };
    worksheet.getRow(logoImageId !== null ? 4 : 2).alignment = {
      horizontal: "center",
      vertical: "middle",
      wrapText: true,
    };

    worksheet.addRow([
      getTranslatedLabel("projects.certificate.type", "Type") +
        ": " +
        (sharedUtils.certificateTypeTranslations["WORKMANSHIP_CONTRACTING_CERTIFICATE"] ||
          "WORKMANSHIP_CONTRACTING_CERTIFICATE"),
      getTranslatedLabel("projects.certificate.date", "Date") +
        ": " +
        new Date().toLocaleDateString("en-UK"),
    ]);
    worksheet.addRow([
      getTranslatedLabel("projects.certificate.description", "Description") +
        ": " +
        sharedUtils.safeString(certificate.description),
      getTranslatedLabel("projects.certificate.form.contractor", "Contractor") +
        ": " +
        sharedUtils.safeString(certificate.partyNameContractor),
    ]);
    worksheet.addRow([
      getTranslatedLabel("projects.certificate.total", "Total") +
        ": " +
        sharedUtils.formatNumber(subtotal),
    ]);
    worksheet.getRow(logoImageId !== null ? 5 : 3).font = { name: "Amiri", size: 10 };
    worksheet.getRow(logoImageId !== null ? 6 : 4).font = { name: "Amiri", size: 10 };
    worksheet.getRow(logoImageId !== null ? 7 : 5).font = { name: "Amiri", size: 10 };
    worksheet.getRow(logoImageId !== null ? 5 : 3).alignment = {
      horizontal: "right",
      wrapText: true,
    };
    worksheet.getRow(logoImageId !== null ? 6 : 4).alignment = {
      horizontal: "right",
      wrapText: true,
    };
    worksheet.getRow(logoImageId !== null ? 7 : 5).alignment = {
      horizontal: "right",
      wrapText: true,
    };

    worksheet.addRow([]);

    const headers = [
      getTranslatedLabel("projects.certificate.items.list.item", "Item"),
      getTranslatedLabel("projects.certificate.items.list.code", "Code"),
      getTranslatedLabel("projects.certificate.items.list.description", "Description"),
      getTranslatedLabel("projects.certificate.items.list.quantity", "Quantity"),
      getTranslatedLabel("projects.certificate.items.list.unitOfMeasure", "Unit of Measure"),
      getTranslatedLabel("projects.certificate.items.list.materialPrice", "Material Price"),
      getTranslatedLabel("projects.certificate.items.list.laborPrice", "Labor Price"),
      getTranslatedLabel("projects.certificate.items.list.totalAmount", "Total Amount"),
      getTranslatedLabel("projects.certificate.items.list.deductions", "Deductions"),
      getTranslatedLabel("projects.certificate.items.list.deductionDescription", "Deduction Description"),
      getTranslatedLabel("projects.certificate.items.list.deserved", "Deserved"),
      getTranslatedLabel("projects.certificate.items.list.insurance", "Insurance"),
      getTranslatedLabel("projects.certificate.items.list.additionalInsurance", "Additional Insurance"),
      getTranslatedLabel("projects.certificate.items.list.net", "Net"),
      getTranslatedLabel("projects.certificate.items.list.achievementPercentage", "Achievement Percentage"),
    ];
    worksheet.addRow(headers);
    const headerRow = worksheet.getRow(worksheet.lastRow!.number);
    headerRow.font = { name: "Amiri", size: 10, bold: true };
    headerRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "F0F0F0" } };
    headerRow.alignment = { horizontal: "center", vertical: "middle", wrapText: true };
    headerRow.eachCell((cell) => {
      cell.border = {
        top: { style: "thin" },
        left: { style: "thin" },
        bottom: { style: "thin" },
        right: { style: "thin" },
      };
    });

    worksheet.columns = [
      { width: 40 }, // Item
      { width: 40 }, // Code
      { width: 25 }, // Description
      { width: 8 }, // Quantity
      { width: 12 }, // Unit of Measure
      { width: 10 }, // Material Price
      { width: 10 }, // Labor Price
      { width: 10 }, // Total Amount
      { width: 10 }, // Deductions
      { width: 15 }, // Deduction Description
      { width: 11 }, // Deserved
      { width: 8 }, // Insurance
      { width: 10 }, // Additional Insurance
      { width: 8 }, // Net
      { width: 12 }, // Achievement Percentage
    ];

    worksheet.getColumn(4).numFmt = "0";
    worksheet.getColumn(6).numFmt = "#,##0.00";
    worksheet.getColumn(7).numFmt = "#,##0.00";
    worksheet.getColumn(8).numFmt = "#,##0.00";
    worksheet.getColumn(9).numFmt = "#,##0.00";
    worksheet.getColumn(11).numFmt = "#,##0.00";
    worksheet.getColumn(12).numFmt = "#,##0.00";
    worksheet.getColumn(13).numFmt = "#,##0.00";
    worksheet.getColumn(14).numFmt = "#,##0.00";
    worksheet.getColumn(15).numFmt = "0%";

    pageItems.forEach((item, index) => {
      let achievementValue: number | string = item.achievementPercentage || "0%";
      const achievementStr = sharedUtils.safeString(item.achievementPercentage);
      if (achievementStr.endsWith("%")) {
        const parsed = parseFloat(achievementStr.slice(0, -1));
        if (!isNaN(parsed)) {
          achievementValue = parsed / 100;
        }
      } else if (typeof item.achievementPercentage === "number") {
        achievementValue = item.achievementPercentage / 100;
      }

      const rowData = [
        item.isLastInGroup && item.productSubtotal !== undefined
          ? `${sharedUtils.rtlEmbed(sharedUtils.safeString(item.productName))} (${
              sharedUtils.formatNumber(item.productSubtotal, 2)
            })`
          : sharedUtils.rtlEmbed(sharedUtils.safeString(item.productName)),
        sharedUtils.safeString(item.code),
        sharedUtils.rtlEmbed(sharedUtils.safeString(item.description)),
        item.quantity !== undefined ? item.quantity : "N/A",
        sharedUtils.rtlEmbed(sharedUtils.safeString(item.uomName)),
        item.materialPrice !== undefined ? item.materialPrice : "N/A",
        item.laborPrice !== undefined ? item.laborPrice : "N/A",
        item.displayTotal !== undefined ? item.displayTotal : "N/A",
        item.deductions !== undefined ? item.deductions : "N/A",
        sharedUtils.rtlEmbed(sharedUtils.safeString(item.deductionDescription)),
        item.deserved !== undefined ? item.deserved : "N/A",
        item.insurance !== undefined ? item.insurance : "N/A",
        item.additionalInsurance !== undefined ? item.additionalInsurance : "N/A",
        item.net !== undefined ? item.net : "N/A",
        achievementValue,
      ];
      const row = worksheet.addRow(rowData);
      row.font = { name: "Amiri", size: 9 };
      row.alignment = { horizontal: "right", vertical: "middle", wrapText: true };
      row.eachCell((cell) => {
        cell.border = {
          top: { style: "thin" },
          left: { style: "thin" },
          bottom: { style: "thin" },
          right: { style: "thin" },
        };
      });
    });

    if (pageItems.some((item) => item.mainItemDescription && item.mainItemDescription.trim())) {
      worksheet.addRow([]);
      worksheet.addRow([
        getTranslatedLabel(
          "projects.certificate.items.mainDescription",
          "Main Item Description"
        ),
      ]);
      worksheet.getRow(worksheet.lastRow!.number).font = {
        name: "Amiri",
        size: 10,
        bold: true,
      };
      pageItems.forEach((item) => {
        if (item.mainItemDescription && item.mainItemDescription.trim()) {
          worksheet.addRow([
            sharedUtils.rtlEmbed(sharedUtils.safeString(item.mainItemDescription)),
          ]);
          worksheet.getRow(worksheet.lastRow!.number).font = { name: "Amiri", size: 9 };
          worksheet.getRow(worksheet.lastRow!.number).alignment = {
            horizontal: "right",
            wrapText: true,
          };
        }
      });
    }

    if (pageItems.some((item) => item.discountNote && item.discountNote.trim())) {
      worksheet.addRow([]);
      worksheet.addRow([
        getTranslatedLabel(
          "projects.certificate.items.discountNote",
          "Discount Description Note"
        ),
      ]);
      worksheet.getRow(worksheet.lastRow!.number).font = {
        name: "Amiri",
        size: 10,
        bold: true,
      };
      pageItems.forEach((item) => {
        if (item.discountNote && item.discountNote.trim()) {
          worksheet.addRow([
            sharedUtils.rtlEmbed(sharedUtils.safeString(item.discountNote)),
          ]);
          worksheet.getRow(worksheet.lastRow!.number).font = { name: "Amiri", size: 9 };
          worksheet.getRow(worksheet.lastRow!.number).alignment = {
            horizontal: "right",
            wrapText: true,
          };
        }
      });
    }
  });

  const buffer = await workbook.xlsx.writeBuffer();
  return buffer;
};