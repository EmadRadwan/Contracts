import * as React from "react";
import { createStyledRow, RowStyleFn } from "./StyledRow";
import { useTranslationHelper } from "../../hooks/useTranslationHelper";

/**
 * Row colouring for reversed accounting transactions (auditor soft-delete requirement, Sep 2026).
 *
 * A posted transaction is never deleted; it is cancelled by a linked reversing transaction.
 * Every list that shows transactions marks both sides so the pair reads at a glance:
 *   - a REVERSAL row (carries `reversalOfAcctgTransId`)  -> amber
 *   - a REVERSED original (carries `reversedByAcctgTransId`) -> grey, muted text
 * Both fields come from ACCTG_TRANS_ATTRIBUTE and are present on every transaction DTO/record.
 */
export const REVERSAL_COLORS = {
    reversal: { backgroundColor: "#fff1c2", color: "#5c4300" },   // amber
    reversed: { backgroundColor: "#e6e8eb", color: "#6b7785" },   // grey, muted
} as const;

export const isReversalRow = (dataItem: any): boolean => !!dataItem?.reversalOfAcctgTransId;
export const isReversedRow = (dataItem: any): boolean => !!dataItem?.reversedByAcctgTransId;

/** Style for a reversal-state row, or undefined when the row is neither. */
export const reversalRowStyle = (dataItem: any): React.CSSProperties | undefined => {
    if (isReversalRow(dataItem)) return REVERSAL_COLORS.reversal;
    if (isReversedRow(dataItem)) return REVERSAL_COLORS.reversed;
    return undefined;
};

/**
 * Wraps an existing row-style function (e.g. the debit/credit tint) so that reversal colouring
 * wins when it applies and the base style is used otherwise.
 * Call at MODULE level like createStyledRow.
 */
export const createReversalAwareRow = (base?: RowStyleFn) =>
    createStyledRow((dataItem, rowProps) => reversalRowStyle(dataItem) ?? base?.(dataItem, rowProps));

/** Ready-made row component for grids that had no row styling before. */
export const ReversalRow = createReversalAwareRow();

/** Small legend to place above a transactions grid. Labels come from the translation files. */
export const ReversalLegend: React.FC<{ style?: React.CSSProperties }> = ({ style }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const swatch = (c: { backgroundColor: string; color: string }, text: string) => (
        <span style={{ display: "inline-flex", alignItems: "center", gap: 6, marginInlineEnd: 16 }}>
            <span style={{ width: 14, height: 14, borderRadius: 3, border: "1px solid #c9ced6", backgroundColor: c.backgroundColor, display: "inline-block" }} />
            <span style={{ fontSize: 12, color: "#46525f" }}>{text}</span>
        </span>
    );
    return (
        <div style={{ margin: "4px 0 6px", ...style }}>
            {swatch(REVERSAL_COLORS.reversal, getTranslatedLabel("accounting.transactions.reversal.legend.reversal", "Reversal entry"))}
            {swatch(REVERSAL_COLORS.reversed, getTranslatedLabel("accounting.transactions.reversal.legend.reversed", "Reversed (cancelled by a reversal)"))}
        </div>
    );
};

/**
 * ExcelJS counterpart of the grid colouring: fills the worksheet row with the same amber/grey and
 * mutes the font on a reversed original. `row` is an ExcelJS Row; typed loosely to avoid importing exceljs here.
 */
export const applyReversalFill = (row: any, dataItem: any): void => {
    const style = reversalRowStyle(dataItem);
    if (!style) return;
    const argb = "FF" + (style.backgroundColor as string).slice(1).toUpperCase();
    row.fill = { type: "pattern", pattern: "solid", fgColor: { argb } };
    if (isReversedRow(dataItem)) row.font = { ...(row.font ?? {}), color: { argb: "FF6B7785" } };
};
