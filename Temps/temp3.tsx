// Add these imports
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';

// Inside your component:
export default function AccountingTransactionsList() {
    // ... existing state & hooks ...

    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [transToDelete, setTransToDelete] = useState<string | null>(null);

    const [deleteAcctgTrans, { isLoading: isDeleting }] = useDeleteAcctgTransMutation();

    const handleDeleteClick = (acctgTransId: string) => {
        setTransToDelete(acctgTransId);
        setDeleteDialogOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!transToDelete) return;

        try {
            await deleteAcctgTrans(transToDelete).unwrap();
            // Optional: show success toast/notification
            setDeleteDialogOpen(false);
            setTransToDelete(null);
        } catch (err) {
            console.error('Delete failed:', err);
            // Optional: show error toast
        }
    };

    const handleCancelDelete = () => {
        setDeleteDialogOpen(false);
        setTransToDelete(null);
    };

    // NEW: Custom cell with Delete button
    const DeleteCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{ ...props.style }}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button
                    size="small"
                    color="error"
                    variant="outlined"
                    onClick={() => handleDeleteClick(props.dataItem.acctgTransId)}
                    disabled={isDeleting}
                >
                    Delete
                </Button>
            </td>
        );
    };

    // ... rest of your component ...

    return (
        <>
            {/* ... existing menu and paper ... */}

            <KendoGrid
                // ... existing props ...
            >
                <GridToolbar>
                    {/* ... existing toolbar ... */}
                </GridToolbar>

                {/* Add Delete column */}
                <Column
                    title="Actions"
                    width={120}
                    cell={DeleteCell}
                    locked={true}
                />

                {/* Existing columns */}
                <Column field="acctgTransId" cell={AcctTransDescriptionCell} ... />
                {/* ... other columns ... */}
            </KendoGrid>

            {/* Confirmation Dialog */}
            <Dialog
                open={deleteDialogOpen}
                onClose={handleCancelDelete}
                aria-labelledby="delete-dialog-title"
                aria-describedby="delete-dialog-description"
            >
                <DialogTitle id="delete-dialog-title">Confirm Deletion</DialogTitle>
                <DialogContent>
                    <DialogContentText id="delete-dialog-description">
                        Are you sure you want to delete accounting transaction{' '}
                        <strong>{transToDelete}</strong>? This action cannot be undone.
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCancelDelete} disabled={isDeleting}>
                        Cancel
                    </Button>
                    <Button
                        onClick={handleConfirmDelete}
                        color="error"
                        variant="contained"
                        disabled={isDeleting}
                        autoFocus
                    >
                        {isDeleting ? 'Deleting...' : 'Delete'}
                    </Button>
                </DialogActions>
            </Dialog>

            {isFetching && <LoadingComponent ... />}
                </>
                );
            }