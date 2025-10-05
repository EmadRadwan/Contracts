import React, { useCallback, useState } from "react";
import { Grid as KendoGrid, GridColumn as Column, GridToolbar, GridCustomFooterCellProps } from "@progress/kendo-react-grid";
import { orderBy, SortDescriptor } from "@progress/kendo-data-query";
import { Box, Button, Typography } from "@mui/material";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useGetCertificatesByPartyQuery } from "../../../app/store/apis/projectsApi";

interface CertificatesListModalProps {
    show: boolean;
    onClose: () => void;
    contractorId?: string;
    supplierId?: string;
    certificateType: string;
}

export default function CertificatesListModal({
                                                  show,
                                                  onClose,
                                                  contractorId,
                                                  supplierId,
                                                  certificateType,
                                              }: CertificatesListModalProps) {
    const { getTranslatedLabel } = useTranslationHelper();
    const initialSort: Array<SortDescriptor> = [{ field: "certificateNumber", dir: "asc" }];
    const [sort, setSort] = useState(initialSort);

    // REFACTOR: Use RTK Query hook within the modal component
    // Purpose: Fetches certificates based on passed contractorId or supplierId
    // Improvement: Centralizes data fetching within the modal, aligning with component responsibility
    const {
        data: certificates = [],
        isLoading: isCertificatesLoading,
        error: certificatesError,
    } = useGetCertificatesByPartyQuery(
        { contractorId, supplierId, certificateType },
        { skip: !show || (!contractorId && !supplierId) }
    );

    // REFACTOR: Added calculation for total sum
    // Purpose: Computes the sum of the 'total' field for display in the footer
    // Improvement: Provides a clear summary of the total values, enhancing user understanding
    const totalSum = certificates.reduce((sum, cert) => sum + (cert.total || 0), 0);

    // REFACTOR: Added TotalFooterCell for the 'total' column
    // Purpose: Displays the sum of the 'total' field in the footer of the 'total' column
    // Improvement: Mimics the approach in AcctgTransEntryList for consistency and clarity
    const TotalFooterCell = (props: GridCustomFooterCellProps) => (
        <td style={{ textAlign: "right", fontWeight: "bold", color: "#1565C0" }}>
            {getTranslatedLabel('projects.certificate.totalSum', 'Total Sum')}: {totalSum.toFixed(2)}
        </td>
    );

    const showSupplier = certificateType === "SUPPLY_PROCUREMENT_CERTIFICATE";
    const showContractor = ["WORKMANSHIP_CONTRACTING_CERTIFICATE", "COMPANY_SUPPLY_SALE_CERTIFICATE"].includes(certificateType);

    const columns = [
        {
            field: "certificateNumber",
            title: getTranslatedLabel('projects.certificate.certificateNumber', 'Certificate Number'),
            width: 150,
        },
        {
            field: "projectName",
            title: getTranslatedLabel('projects.certificate.projectName', 'Project Name'),
            width: 200,
        },
        {
            field: "description",
            title: getTranslatedLabel('projects.certificate.description', 'Description'),
            width: 250,
        },
        {
            field: "statusDescription",
            title: getTranslatedLabel('projects.certificate.status', 'Status'),
            width: 150,
        },
        {
            field: "total",
            title: getTranslatedLabel('projects.certificate.total', 'Total'),
            format: "{0:n2}",
            width: 220,
            // REFACTOR: Added footerCell to the 'total' column
            // Purpose: Links the TotalFooterCell to display the sum of totals
            // Improvement: Ensures the total sum is displayed only in the relevant column
            footerCell: TotalFooterCell,
        },
        ...(showSupplier
            ? [{
                field: "partyNameSupplier",
                title: getTranslatedLabel('projects.certificate.supplier', 'Supplier'),
                width: 200,
            }]
            : []),
        ...(showContractor
            ? [{
                field: "partyNameContractor",
                title: getTranslatedLabel('projects.certificate.contractor', 'Contractor'),
                width: 200,
            }]
            : []),
        {
            field: "estimatedStartDate",
            title: getTranslatedLabel('projects.certificate.startDate', 'Start Date'),
            width: 150,
            cell: (props: any) => (
                <td>
                    {props.dataItem.estimatedStartDate
                        ? new Date(props.dataItem.estimatedStartDate).toLocaleDateString()
                        : 'N/A'}
                </td>
            ),
        },
        {
            field: "estimatedCompletionDate",
            title: getTranslatedLabel('projects.certificate.completionDate', 'Completion Date'),
            width: 150,
            cell: (props: any) => (
                <td>
                    {props.dataItem.estimatedCompletionDate
                        ? new Date(props.dataItem.estimatedCompletionDate).toLocaleDateString()
                        : 'N/A'}
                </td>
            ),
        },
    ];

    const sortedData = orderBy(certificates, sort);

    return (
        <ModalContainer show={show} onClose={onClose} width={1200}>
            <Typography variant="h6" component="h2" gutterBottom>
                {getTranslatedLabel('projects.certificate.list', 'Certificates List')}
            </Typography>
            {isCertificatesLoading ? (
                <LoadingComponent message={getTranslatedLabel('projects.certificate.loading', 'Loading Certificates...')} />
            ) : certificatesError ? (
                <Typography color="error">
                    {getTranslatedLabel('projects.certificate.error', 'Failed to load certificates')}
                </Typography>
            ) : certificates.length === 0 ? (
                <Typography>
                    {getTranslatedLabel('projects.certificate.noData', 'No certificates found')}
                </Typography>
            ) : (
                <KendoGrid
                    style={{ height: "60vh" }}
                    data={sortedData}
                    sortable
                    scrollable="scrollable"
                    resizable
                    sort={sort}
                    onSortChange={(e) => setSort(e.sort)}
                >
                    <GridToolbar>
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                            <Button variant="contained" color="primary" onClick={onClose}>
                                {getTranslatedLabel('projects.certificate.close', 'Close')}
                            </Button>
                        </Box>
                    </GridToolbar>
                    {columns.map((column, index) => (
                        <Column key={index} {...column} />
                    ))}
                </KendoGrid>
            )}
        </ModalContainer>
    );
}