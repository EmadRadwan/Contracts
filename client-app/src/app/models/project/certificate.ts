import {CertificateItem} from "./certificateItem";

export interface Certificate {
    workEffortId?: string;
    work_effort_type_id: string;
    projectNum?: string;
    projectName: string;
    partyId: string;
    description: string;
    estimated_start_date?: string | null;
    estimated_completion_date?: string | null;
    statusDescription?: string;
    certificateItems?: CertificateItem[];
    modificationType?: string;
}