/** Shape of the KendoReact v16 Grid `edit` prop: `{ [dataItemKeyValue]: true | ["field", ...] }`. */
export type EditDescriptor = Record<string, boolean | string[]>;

/**
 * Builds the Grid `edit` descriptor from an in-edit flag carried on the data items.
 *
 * Replaces the KendoReact v6 `editField="inEdit"` prop: v16 no longer reads a flag off each
 * data item — it wants a descriptor keyed by `dataItemKey`. Deriving it from the same `data`
 * the Grid renders keeps the existing `inEdit` state model and command cells unchanged.
 *
 * Pair with `editable={{ enabled: true, mode: "inline" }}` so the Grid never initiates edit
 * transitions on its own (only `incell` mode does that on cell click).
 */
export const editDescriptorFrom = (
  data: ReadonlyArray<any> | undefined,
  dataItemKey: string,
  flag = "inEdit"
): EditDescriptor => {
  const edit: EditDescriptor = {};
  for (const item of data ?? []) {
    if (item?.[flag]) edit[item[dataItemKey]] = true;
  }
  return edit;
};
