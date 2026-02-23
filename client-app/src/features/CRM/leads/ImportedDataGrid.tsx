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
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import FormInput from '../../../app/common/form/FormInput';

interface ImportedDataGridProps {
  data: any[];
  fileName: string;
  onClose: () => void;
  // onSave?: (mappedData: SalesOpportunity[]) => void; // future: map & save
}

const ImportedDataGrid: React.FC<ImportedDataGridProps> = ({
  data,
  fileName,
  onClose,
}) => {
  const { getTranslatedLabel } = useTranslationHelper();
  const [gridData, setGridData] = useState(data);
  const [dataState, setDataState] = useState<State>({ skip: 0, take: 30 });

  const processed = process(gridData, dataState);

  // Dynamic columns from first row keys
  const columns = data.length > 0 ? Object.keys(data[0]) : [];

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

  // Basic inline editing for all columns
  const MyEditCell = (props: GridEditCellProps) => {
    const { dataItem, field, onChange } = props;
    return (
      <td className={"k-grid-edit-cell"} style={props.style}>
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
    <Column
      key={col}
      field={col}
      title={col}
      width={280}
      editor="text"
      cell={MyEditCell} // custom for better control; or remove for default
    />
  ));

  return (
    <Box sx={{ p: 2 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Typography variant="h6">
          Imported: {fileName} ({gridData.length} rows)
        </Typography>
        <Box>
          {/* <Button variant="contained" sx={{ mr: 1 }}>
            Save as Opportunities
          </Button> */}
          <Button variant="outlined" onClick={onClose}>
            Close
          </Button>
        </Box>
      </Box>

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
  );
};

export default ImportedDataGrid;