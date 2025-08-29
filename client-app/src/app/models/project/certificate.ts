
// REFACTOR: Define CertificateItem
// Purpose: Model for certificate item data
// Context: Updated to include fields for both PROCUREMENT_CERTIFICATE and CONTRACTING_CERTIFICATE
export interface CertificateItem {
  productId: any | null;
  uomId: string | null;
  quantity: number;
  unitPrice: number;
  procurementDate?: Date | null;
  facilityId?: string | null;
  discount?: number;
  total: number;
  deductions?: number;
  deserved?: number;
  insurance?: number;
  net?: number;
}