/**
 * Grid Styling & Filter Components - Usage Example
 *
 * This example shows how to apply the common grid styling and filter components
 * to any Kendo Grid-based list component.
 */

import React, { useState } from 'react';
import { Grid as KendoGrid, GridColumn as Column, GridDataStateChangeEvent, GridToolbar } from '@progress/kendo-react-grid';
import { DataResult, State } from '@progress/kendo-data-query';
import Button from '@mui/material/Button';

// Import the common grid components and styling
import { TextFilterCell, DateFilterCell, NumericFilterCell } from './index';
import './grid.styles.css';

interface ExampleData {
    id: string;
    name: string;
    date: Date;
    amount: number;
    status: string;
}

export function GridStylingExample() {
    const [data, setData] = useState<DataResult>({ data: [], total: 0 });
    const [dataState, setDataState] = useState<State>({ take: 10, skip: 0 });

    const handleDataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    // Sample data
    const sampleData: ExampleData[] = [
        { id: 'ORD001', name: 'Product A', date: new Date('2024-01-15'), amount: 1500, status: 'APPROVED' },
        { id: 'ORD002', name: 'Product B', date: new Date('2024-02-20'), amount: 2500, status: 'PENDING' },
        { id: 'ORD003', name: 'Product C', date: new Date('2024-03-10'), amount: 3000, status: 'REJECTED' },
    ];

    React.useEffect(() => {
        setData({ data: sampleData, total: sampleData.length });
    }, []);

    return (
        <div style={{ padding: '20px' }}>
            <h2>Grid Styling Example</h2>
            <p>This example demonstrates the common grid styling and filter components.</p>

            <KendoGrid
                className="kendo-grid-styled"  {/* ← Apply the styling class */}
                style={{ height: '400px' }}
                resizable={true}
                filterable={true}  {/* ← Enable filtering */}
                sortable={true}
                pageable={true}
                {...dataState}
                data={data}
                onDataStateChange={handleDataStateChange}
            >
                <GridToolbar>
                    <Button variant="contained" color="primary">
                        Create New Item
                    </Button>
                </GridToolbar>

                {/* Text Filter Example */}
                <Column
                    field="id"
                    title="ID"
                    width={150}
                    filterable={true}
                    filter="text"
                    filterCell={TextFilterCell}  {/* ← Custom text filter */}
                />

                {/* Text Filter Example */}
                <Column
                    field="name"
                    title="Name"
                    filterable={true}
                    filter="text"
                    filterCell={TextFilterCell}  {/* ← Custom text filter */}
                />

                {/* Date Filter Example */}
                <Column
                    field="date"
                    title="Date"
                    format="{0: dd/MM/yyyy}"
                    filterable={true}
                    filter="date"
                    filterCell={DateFilterCell}  {/* ← Custom date filter with operators */}
                />

                {/* Numeric Filter Example */}
                <Column
                    field="amount"
                    title="Amount"
                    filterable={true}
                    filter="numeric"
                    filterCell={NumericFilterCell}  {/* ← Custom numeric filter with operators */}
                />

                {/* Text Filter with Status */}
                <Column
                    field="status"
                    title="Status"
                    filterable={true}
                    filter="text"
                    filterCell={TextFilterCell}  {/* ← Custom text filter */}
                />
            </KendoGrid>

            <div style={{ marginTop: '20px', padding: '10px', backgroundColor: '#f5f5f5', borderRadius: '4px' }}>
                <h3>Implementation Notes</h3>
                <ul>
                    <li><code>className="kendo-grid-styled"</code> applies the CSS styling</li>
                    <li><code>filterCell={TextFilterCell}</code> uses text-only filters (no dropdown)</li>
                    <li><code>filterCell={DateFilterCell}</code> uses date picker with operator dropdown</li>
                    <li><code>filterCell={NumericFilterCell}</code> uses number input with operator dropdown</li>
                    <li>All filter cells have a clear button to reset the filter</li>
                </ul>
            </div>

            <div style={{ marginTop: '20px', padding: '10px', backgroundColor: '#e3f2fd', borderRadius: '4px' }}>
                <h3>Quick Start for Your Component</h3>
                <pre>{`// 1. Import the components and styling
import { TextFilterCell, DateFilterCell, NumericFilterCell } from '../app/common/grid';
import '../app/common/grid/grid.styles.css';

// 2. Add className to KendoGrid
<KendoGrid className="kendo-grid-styled" ...>

// 3. Add filterCell to columns
<Column field="name" filterable={true} filter="text" filterCell={TextFilterCell} />
<Column field="date" filterable={true} filter="date" filterCell={DateFilterCell} />
<Column field="amount" filterable={true} filter="numeric" filterCell={NumericFilterCell} />
`}</pre>
            </div>
        </div>
    );
}

/**
 * CUSTOM FILTER EXAMPLE
 *
 * If you need a custom filter for a specific column type:
 */

import { GridFilterCellProps } from '@progress/kendo-react-grid';

export const CustomPhoneFilterCell = (props: GridFilterCellProps) => {
    return (
        <div className="k-filtercell">
            <div className="k-filtercell-wrapper">
                <input
                    type="tel"
                    className="k-textbox"
                    value={props.value ?? ""}
                    onChange={(e) => {
                        props.onChange({
                            value: e.target.value,
                            operator: "startswith",  // Always use startswith for phone numbers
                            syntheticEvent: e
                        });
                    }}
                    placeholder="Search by phone..."
                    style={{ flex: 1 }}
                />
            </div>
        </div>
    );
};

/**
 * USAGE IN COMPONENT:
 * <Column field="phone" filterable={true} filter="text" filterCell={CustomPhoneFilterCell} />
 */
