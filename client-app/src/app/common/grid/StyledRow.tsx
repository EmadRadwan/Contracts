import * as React from "react";
import { GridCustomRowProps } from "@progress/kendo-react-grid";

export type RowStyleFn = (dataItem: any, rowProps: GridCustomRowProps) => React.CSSProperties | undefined;

/**
 * Builds a Grid `rows={{ data: ... }}` component that renders the default <tr>
 * with an extra style computed from the row's dataItem.
 *
 * Replaces the KendoReact v6 `rowRender={(tr, props) => cloneElement(tr, { style })}` pattern.
 *
 * IMPORTANT: call this at MODULE level, not inside a component. KendoReact v16 mounts
 * `rows.data` as a React component; a new identity on every render would remount every
 * row (and lose focus in editable grids).
 */
export const createStyledRow = (getStyle: RowStyleFn): React.ComponentType<GridCustomRowProps> => {
  const StyledRow = (props: GridCustomRowProps) => (
    <tr {...props.trProps} style={{ ...props.trProps?.style, ...getStyle(props.dataItem, props) }}>
      {props.children}
    </tr>
  );
  StyledRow.displayName = "StyledRow";
  return StyledRow;
};
