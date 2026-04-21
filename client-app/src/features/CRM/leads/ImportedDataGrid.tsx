import React, { useState } from 'react';
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
  GridCellProps,
  GridEditCellProps,
} from '@progress/kendo-react-grid';
import { process, State } from '@progress/kendo-data-query';
import {
  Box,
  Button,
  Typography,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Alert,
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import FormInput from '../../../app/common/form/FormInput';
import { useCreateLeadsBatchMutation } from '../../../app/store/configureStore';
import AddIcon from '@mui/icons-material/Add';

interface ImportedDataGridProps {
  data: any[];
  fileName: string;
  onClose: () => void;
}

interface BatchLeadError {
  index: number;
  firstName: string;
  email: string;
  reason: string;
}

interface BatchCreateLeadsResult {
  totalReceived: number;
  successful: number;
  failed: number;
  errors: BatchLeadError[];
}

const DATA_SOURCE_MAP: Record<string, string> = {
  facebook: 'FACEBOOK',
  instagram: 'INSTAGRAM',
  linkedin: 'LINKEDIN',
  indirect: 'INDIRECT',
  other: 'OTHER',
};

const mapRowToDto = (row: any) => ({
  firstName: row['First Name'] ?? '',
  middleName: row['Last Name'] ?? '',
  personalTitle: row['Title'] ?? '',
  infoString: row['Email'] ?? '',
  mobileContactNumber: row['Mobile Number'] ? String(row['Mobile Number']) : '',
  dataSourceId:
    DATA_SOURCE_MAP[(row['Lead source'] ?? '').toString().toLowerCase()] ?? 'OTHER',
  address1: row['Address1'] ?? '',
  address2: row['Address2'] ?? '',
  city: row['City'] ?? '',
  geoId: row['Country'] ?? '',
});

const ImportedDataGrid: React.FC<ImportedDataGridProps> = ({ data, fileName, onClose }) => {
  const { getTranslatedLabel } = useTranslationHelper();
  const [gridData, setGridData] = useState(data);
  const [dataState, setDataState] = useState<State>({ skip: 0, take: 30 });
  const [createBatchLeads, { isLoading: isSaving }] = useCreateLeadsBatchMutation();
  const [batchResult, setBatchResult] = useState<BatchCreateLeadsResult | null>(null);
  const [resultModalOpen, setResultModalOpen] = useState(false);
  // Tracks which rows in the current gridData are failed (by their original index)
  const [failedIndices, setFailedIndices] = useState<Set<number>>(new Set());

  const processed = process(gridData, dataState);
  const columns = data.length > 0 ? Object.keys(data[0]) : [];

  const handleSave = async () => {
    try {
      const mapped = gridData.map(mapRowToDto);
      const result: BatchCreateLeadsResult = await createBatchLeads(mapped).unwrap();
      setBatchResult(result);

      if (result.failed > 0) {
        setResultModalOpen(true);
      } else {
        onClose();
      }
    } catch (error) {
      console.error('Error saving leads:', error);
    }
  };

  // Keep only the failed rows in the grid so the user can fix & retry
  const handleKeepFailedRows = () => {
    if (!batchResult) return;

    const failedIndexSet = new Set(batchResult.errors.map((e) => e.index));
    const failedRows = gridData.filter((_, i) => failedIndexSet.has(i));

    setGridData(failedRows);
    setFailedIndices(new Set(Array.from({ length: failedRows.length }, (_, i) => i)));
    setBatchResult(null);
    setResultModalOpen(false);
  };

  const handleResultModalClose = () => {
    setResultModalOpen(false);
    onClose();
  };

  const DeleteCell = (props: GridCellProps) => (
    <td>
      <Tooltip title="Delete row">
        <IconButton
          size="small"
          color="error"
          onClick={() => {
            const newData = gridData.filter((_, i) => i !== props.dataIndex);
            setGridData(newData);
          }}
        >
          <DeleteIcon fontSize="medium" />
        </IconButton>
      </Tooltip>
    </td>
  );

  const MyEditCell = (props: GridEditCellProps) => {
    const { dataItem, field, onChange } = props;
    return (
      <td className="k-grid-edit-cell" style={props.style}>
        <FormInput
          type="text"
          defaultValue={dataItem[field]}
          onBlur={(e) => {
            onChange({
              dataIndex: props.dataIndex,
              dataItem: { ...dataItem, [field]: e.target.value },
              field,
              syntheticEvent: e,
            });
          }}
          style={{ width: '100%' }}
        />
      </td>
    );
  };

  const editableColumns = columns.map((col) => (
    <Column key={col} field={col} title={col} width={280} editor="text" cell={MyEditCell} />
  ));

  return (
    <>
      <Box sx={{ p: 2 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
          <Typography variant="h6">
            Imported: {fileName} ({gridData.length} rows)
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="contained"
              color="primary"
              startIcon={<AddIcon />}
              onClick={handleSave}
              disabled={isSaving}
            >
              {isSaving ? 'Saving...' : 'Save'}
            </Button>
            <Button variant="outlined" onClick={onClose}>
              Close
            </Button>
          </Box>
        </Box>

        {/* Banner shown when grid is displaying failed rows only */}
        {failedIndices.size > 0 && (
          <Alert severity="warning" sx={{ mb: 2 }}>
            Showing <strong>{gridData.length}</strong> failed row(s) — fix the errors and click Save again.
          </Alert>
        )}

        <KendoGrid
          data={processed}
          {...dataState}
          onDataStateChange={(e) => setDataState(e.dataState)}
          pageable={{ pageSizes: [30, 60] }}
          sortable
          filterable
          editable
          style={{ height: '65vh' }}
        >
          <GridToolbar>
            <Typography variant="subtitle1">Preview & Edit Imported Data</Typography>
          </GridToolbar>
          {editableColumns}
          <Column
            title="Actions"
            width={80}
            cell={DeleteCell}
            filterable={false}
            sortable={false}
          />
        </KendoGrid>
      </Box>

      {/* ==================== BATCH RESULT MODAL ==================== */}
      <Dialog
        open={resultModalOpen}
        onClose={handleResultModalClose}
        maxWidth="sm"
        fullWidth
        PaperProps={{ sx: { borderRadius: 2 } }}
      >
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WarningAmberIcon color="warning" />
          Import Summary
        </DialogTitle>

        <DialogContent dividers>
          {/* Summary chips */}
          <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
            <Chip label={`Total: ${batchResult?.totalReceived ?? 0}`} variant="outlined" />
            <Chip
              icon={<CheckCircleOutlineIcon />}
              label={`Saved: ${batchResult?.successful ?? 0}`}
              color="success"
              variant="outlined"
            />
            <Chip
              icon={<WarningAmberIcon />}
              label={`Failed: ${batchResult?.failed ?? 0}`}
              color="error"
              variant="outlined"
            />
          </Box>

          {/* Failed rows table */}
          {batchResult && batchResult.errors.length > 0 && (
            <>
              <Typography variant="subtitle2" sx={{ mb: 1, color: 'error.main' }}>
                The following rows could not be saved:
              </Typography>
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableHead>
                    <TableRow sx={{ backgroundColor: 'grey.100' }}>
                      <TableCell><strong>Row #</strong></TableCell>
                      <TableCell><strong>First Name</strong></TableCell>
                      <TableCell><strong>Email</strong></TableCell>
                      <TableCell><strong>Reason</strong></TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {batchResult.errors.map((err) => (
                      <TableRow key={err.index}>
                        <TableCell>{err.index + 1}</TableCell>
                        <TableCell>{err.firstName}</TableCell>
                        <TableCell>{err.email}</TableCell>
                        <TableCell>
                          <Chip label={err.reason} color="error" size="small" />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </>
          )}
        </DialogContent>

        <DialogActions sx={{ gap: 1 }}>
          {/* Fix failed rows — loads them back into the grid */}
          <Button
            onClick={handleKeepFailedRows}
            variant="contained"
            color="warning"
            startIcon={<WarningAmberIcon />}
          >
            Fix Failed Rows
          </Button>

          {/* Done — close everything */}
          <Button onClick={handleResultModalClose} variant="outlined">
            Done
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default ImportedDataGrid;