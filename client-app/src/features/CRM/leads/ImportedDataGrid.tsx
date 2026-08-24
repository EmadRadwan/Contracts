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
import { useAppSelector } from '../../../app/store/configureStore';
import BulkAssignLeadsModal from './BulkAssignLeadsModal';

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
  createdPartyIds: string[];
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

const localizationKey = 'crm.leads.imported';

const ImportedDataGrid: React.FC<ImportedDataGridProps> = ({ data, fileName, onClose }) => {
  const { getTranslatedLabel } = useTranslationHelper();
  const [gridData, setGridData] = useState(data);
  const [dataState, setDataState] = useState<State>({ skip: 0, take: 30 });
  const [createBatchLeads, { isLoading: isSaving }] = useCreateLeadsBatchMutation();
  const [batchResult, setBatchResult] = useState<BatchCreateLeadsResult | null>(null);
  const [resultModalOpen, setResultModalOpen] = useState(false);
  const [failedIndices, setFailedIndices] = useState<Set<number>>(new Set());
  const [assignOpen, setAssignOpen] = useState(false);
  const [assignedDone, setAssignedDone] = useState(false);
  const language = useAppSelector((state) => state.localization.language);
  const { user } = useAppSelector((state) => state.account);
  const canAssign = (user?.roles || []).includes('CRM_Leads_Assign');

  const processed = process(gridData, dataState);
  const columns = data.length > 0 ? Object.keys(data[0]) : [];

  const handleSave = async () => {
    try {
      const mapped = gridData.map(mapRowToDto);
      const result: BatchCreateLeadsResult = await createBatchLeads(mapped).unwrap();
      setBatchResult(result);

      // Show the summary whenever there is something to report or to assign.
      if (result.failed > 0 || result.successful > 0) {
        setResultModalOpen(true);
      } else {
        onClose();
      }
    } catch (error) {
      console.error('Error saving leads:', error);
    }
  };

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
      <Tooltip title={getTranslatedLabel(`${localizationKey}.deleteRow`, 'Delete row')}>
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
    <Column key={col} field={col} title={col} editor="text" cell={MyEditCell} />
  ));

  return (
    <>
      <Box sx={{ p: 2 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
          <Typography variant="h6">
            {getTranslatedLabel(`${localizationKey}.importedTitle`, 'Imported:')} {fileName} ({gridData.length} {getTranslatedLabel(`${localizationKey}.rows`, 'rows')})
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="contained"
              color="primary"
              // startIcon={<AddIcon />}
              onClick={handleSave}
              disabled={isSaving}
            >
              <AddIcon sx={{ ml: language === "ar" ? 0.5 : 0, mr: language === "ar" ? 0 : 0.5 }} />
              {isSaving
                ? getTranslatedLabel(`${localizationKey}.saving`, 'Saving...')
                : getTranslatedLabel(`${localizationKey}.save`, 'Save')}
            </Button>
            <Button variant="outlined" onClick={onClose}>
              {getTranslatedLabel(`${localizationKey}.close`, 'Close')}
            </Button>
          </Box>
        </Box>

        {failedIndices.size > 0 && (
          <Alert severity="warning" sx={{ mb: 2 }}>
            {getTranslatedLabel(`${localizationKey}.failedRowsBannerPrefix`, 'Showing')}{' '}
            <strong>{gridData.length}</strong>{' '}
            {getTranslatedLabel(
              `${localizationKey}.failedRowsBannerSuffix`,
              'failed row(s) — fix the errors and click Save again.'
            )}
          </Alert>
        )}

        <KendoGrid
          data={processed}
          {...dataState}
          onDataStateChange={(e) => setDataState(e.dataState)}
          // pageable={{ pageSizes: [30, 60] }}
          sortable
          filterable
          editable
          style={{ height: '65vh' }}
        >
          <GridToolbar>
            <Typography variant="subtitle1">
              {getTranslatedLabel(`${localizationKey}.gridToolbarTitle`, 'Preview & Edit Imported Data')}
            </Typography>
          </GridToolbar>
          {editableColumns}
          <Column
            title={getTranslatedLabel(`${localizationKey}.actionsColumn`, 'Actions')}
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
          {batchResult && batchResult.failed > 0
            ? <WarningAmberIcon color="warning" />
            : <CheckCircleOutlineIcon color="success" />}
          {getTranslatedLabel(`${localizationKey}.importSummaryTitle`, 'Import Summary')}
        </DialogTitle>

        <DialogContent dividers>
          <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
            <Chip
              label={`${getTranslatedLabel(`${localizationKey}.total`, 'Total')}: ${batchResult?.totalReceived ?? 0}`}
              variant="outlined"
            />
            <Chip
              icon={<CheckCircleOutlineIcon />}
              label={`${getTranslatedLabel(`${localizationKey}.saved`, 'Saved')}: ${batchResult?.successful ?? 0}`}
              color="success"
              variant="outlined"
            />
            <Chip
              icon={<WarningAmberIcon />}
              label={`${getTranslatedLabel(`${localizationKey}.failed`, 'Failed')}: ${batchResult?.failed ?? 0}`}
              color="error"
              variant="outlined"
            />
          </Box>

          {/* Assignment step - imported leads are created unassigned */}
          {canAssign && batchResult && batchResult.createdPartyIds?.length > 0 && (
            <Alert
              severity={assignedDone ? 'success' : 'info'}
              sx={{ mb: 3 }}
              action={
                !assignedDone && (
                  <Button color="inherit" size="small" onClick={() => setAssignOpen(true)}>
                    {getTranslatedLabel(`${localizationKey}.assignNow`, 'Assign')}
                  </Button>
                )
              }
            >
              {assignedDone
                ? getTranslatedLabel(`${localizationKey}.assignDone`, 'The imported leads have been assigned.')
                : getTranslatedLabel(
                    `${localizationKey}.assignPrompt`,
                    '{0} leads were created without an owner. Assign them to a sales rep now?'
                  ).replace('{0}', String(batchResult.createdPartyIds.length))}
            </Alert>
          )}

          {batchResult && batchResult.errors.length > 0 && (
            <>
              <Typography variant="subtitle2" sx={{ mb: 1, color: 'error.main' }}>
                {getTranslatedLabel(
                  `${localizationKey}.failedRowsDescription`,
                  'The following rows could not be saved:'
                )}
              </Typography>
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableHead>
                    <TableRow sx={{ backgroundColor: 'grey.100' }}>
                      <TableCell>
                        <strong>{getTranslatedLabel(`${localizationKey}.rowNumber`, 'Row #')}</strong>
                      </TableCell>
                      <TableCell>
                        <strong>{getTranslatedLabel(`${localizationKey}.firstName`, 'First Name')}</strong>
                      </TableCell>
                      <TableCell>
                        <strong>{getTranslatedLabel(`${localizationKey}.email`, 'Email')}</strong>
                      </TableCell>
                      <TableCell>
                        <strong>{getTranslatedLabel(`${localizationKey}.reason`, 'Reason')}</strong>
                      </TableCell>
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
          {batchResult && batchResult.failed > 0 && (
            <Button
              onClick={handleKeepFailedRows}
              variant="contained"
              color="warning"
              startIcon={<WarningAmberIcon />}
            >
              {getTranslatedLabel(`${localizationKey}.fixFailedRows`, 'Fix Failed Rows')}
            </Button>
          )}

          <Button onClick={handleResultModalClose} variant="outlined">
            {getTranslatedLabel(`${localizationKey}.done`, 'Done')}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Bulk-assign the leads this import just created */}
      <BulkAssignLeadsModal
        open={assignOpen}
        onClose={() => setAssignOpen(false)}
        leadPartyIds={batchResult?.createdPartyIds ?? []}
        onAssigned={() => setAssignedDone(true)}
      />
    </>
  );
};

export default ImportedDataGrid;