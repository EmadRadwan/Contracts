const DeleteCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    const { getTranslatedLabel } = useTranslationHelper();

    // Optional: Also disable if transaction is posted
    const isPosted = props.dataItem.isPosted === "Y";

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
            <Can perform="deleteAcctgTrans">
                <Button
                    size="small"
                    color="error"
                    variant="outlined"
                    onClick={() => handleDeleteClick(props.dataItem.acctgTransId)}
                    disabled={isDeleting || isPosted}
                >
                    {getTranslatedLabel(
                        "accounting.orgGL.accounting.summary.txns.deleteButton",
                        "Delete"
                    )}
                </Button>
            </Can>

            {/* Optional: Show visual indicator if posted and user has permission */}
            {isPosted && (
                <span style={{ marginLeft: 8, color: "gray", fontStyle: "italic" }}>
          {getTranslatedLabel(
              "accounting.orgGL.accounting.summary.txns.isPosted",
              "Posted"
          )}
        </span>
            )}
        </td>
    );
};