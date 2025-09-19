// certificate.ts
import {CertificateItem} from "./certificateItem";

export interface Certificate {
  workEffortId?: string;
  workEffortTypeId?: string;
  projectId?: string;
  projectName?: string;
  certificateNumber?: string;
  partyIdSupplier?: { fromPartyId: string; partyName: string } | string;
  partyIdContractor?: { fromPartyId: string; partyName: string } | string;
  description?: string;
  estimatedStartDate?: string | Date;
  estimatedCompletionDate?: string | Date;
  certificateItems?: CertificateItem[];
  currentStatusId?: CertificateStatus;
  statusDescription?: string;
  relatedOrderId?: string;
  facilityId?: string;
  facilityName?: string;
}

export enum CertificateStatus {
  CREATED = "WEPR_CREATED",
  APPROVED = "WEPR_APPROVED",
  COMPLETE = "WEPR_COMPLETE",
}