// certificate.ts
import {CertificateItem} from "./certificateItem";

export interface Certificate {
  workEffortId?: string; // Maps to WorkEffort.WorkEffortId
  workEffortTypeId: string; // Maps to WorkEffort.WorkEffortTypeId (e.g., "PROCUREMENT_CERTIFICATE" or "CONTRACTING_CERTIFICATE")
  projectId?: string; // Maps to WorkEffort.WorkEffortName
  projectName?: string; // Maps to WorkEffort.ProjectName
  partyId?: string; // Maps to WorkEffort.PartyId
  description?: string; // Maps to WorkEffort.Description
  estimatedStartDate?: string | null; // Maps to WorkEffort.EstimatedStartDate (ISO string)
  estimatedCompletionDate?: string | null; // Maps to WorkEffort.EstimatedCompletionDate (ISO string)
  statusDescription?: string; // Maps to WorkEffort.StatusDescription
  certificateItems?: CertificateItem[]; // Array of associated items
}