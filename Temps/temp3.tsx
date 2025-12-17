{user?.roles?.includes('ReviewCertificate') &&
[CertificateStatus.CREATED, CertificateStatus.REQUIRES_EDIT].includes(currentStatusId) && (
    <>
        <MenuItem onClick={() => handleStatusUpdate('MarkReadyForApproval')}>
            Mark as Ready for Approval
        </MenuItem>
        {/* Optionally keep or remove "Requires Editing" when already in REQUIRES_EDIT */}
    </>
)}