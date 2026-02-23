import React, { useState, useCallback } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  CircularProgress,
  Alert,
} from '@mui/material';
import { useDropzone } from 'react-dropzone';
import * as XLSX from 'xlsx';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';

interface ExcelUploadDialogProps {
  open: boolean;
  onClose: () => void;
  onDataParsed: (data: any[], fileName: string) => void;
}

const ExcelUploadDialog: React.FC<ExcelUploadDialogProps> = ({
  open,
  onClose,
  onDataParsed,
}) => {
  const { getTranslatedLabel } = useTranslationHelper();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onDrop = useCallback(
    (acceptedFiles: File[]) => {
      const file = acceptedFiles[0];
      if (!file) return;

      setError(null);
      setLoading(true);

      const reader = new FileReader();
      reader.onload = (e) => {
  try {
    const data = e.target?.result;
    if (!data) throw new Error('No data');

    const workbook = XLSX.read(data, { type: 'array' });
    const sheetName = workbook.SheetNames[0];
    const worksheet = workbook.Sheets[sheetName];

    // Parse to array of arrays first (header: 1) to easily inspect rows
    // Use blankrows: false to skip obviously blank ones early
    const rawRows = XLSX.utils.sheet_to_json(worksheet, {
      header: 1,           // get array of arrays
      blankrows: false,    // skip truly empty rows early
      defval: '',          // empty cells → ""
    }) as any[][];

    // Extract headers from first row
    const headers = (rawRows[0] || []).map((h: any) => 
      (h || '').toString().trim() || `Column${rawRows[0].indexOf(h) + 1}`
    );

    // Convert data rows (skip header) and filter empty ones
    const rows = rawRows.slice(1)
      .filter((row: any[]) => {
        // A row is "empty" if EVERY cell is falsy, empty string, whitespace-only, or null/undefined
        return row.some((cell: any) => {
          const val = cell?.toString?.()?.trim?.() ?? '';
          return val !== '' && val !== null && val !== undefined;
        });
      })
      .map((row: any[]) =>
        headers.reduce((obj: Record<string, any>, header, i) => {
          obj[header] = row[i] ?? '';
          return obj;
        }, {})
      );

    if (rows.length === 0) {
      throw new Error('No data rows found after filtering empty rows.');
    }

    onDataParsed(rows, file.name);
    onClose();
  } catch (err) {
    setError('Failed to parse file or no valid data found after removing empty rows.');
    console.error(err);
  } finally {
    setLoading(false);
  }
};
      reader.readAsArrayBuffer(file);
    },
    [onDataParsed, onClose]
  );

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx', '.xls'],
      'text/csv': ['.csv'],
    },
    maxFiles: 1,
  });

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {getTranslatedLabel('crm.leads.uploadExcel', 'Upload Excel/CSV')}
      </DialogTitle>
      <DialogContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Box
          {...getRootProps()}
          sx={{
            border: '2px dashed',
            borderColor: isDragActive ? 'primary.main' : 'grey.400',
            borderRadius: 2,
            p: 6,
            textAlign: 'center',
            bgcolor: isDragActive ? 'action.hover' : 'background.paper',
            cursor: 'pointer',
            transition: 'all 0.2s',
          }}
        >
          <input {...getInputProps()} />
          {loading ? (
            <CircularProgress />
          ) : (
            <>
              <Typography variant="h6" gutterBottom>
                {isDragActive
                  ? 'Drop the file here...'
                  : 'Drag & drop file here, or click to browse'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Supported: .xlsx, .xls, .csv (max 1 file)
              </Typography>
            </>
          )}
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
      </DialogActions>
    </Dialog>
  );
};

export default ExcelUploadDialog;