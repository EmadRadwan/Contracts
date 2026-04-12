const BulkAddRowItem: React.FC<BulkAddRowItemProps> = memo(({
                                                                row,
                                                                index,
                                                                glAccounts,
                                                                handleRowChange,
                                                                handleRemoveRow,
                                                                rowsCount,
                                                                costCenters,
                                                                itemTypes,
                                                                localAmount,
                                                            }) => {

    // === Queries ===
    const { data: projects } = useFetchWorkEffortsByGlAccountIdQuery(
        { glAccountId: row.glAccountId, workEffortTypeId: "PROJECT" },
        { skip: !row.glAccountId }
    );

    const projectId = projects && projects.length > 0 ? projects[0].workEffortId : undefined;

    const { data: subProjects } = useFetchWorkEffortsByGlAccountIdQuery(
        {
            glAccountId: row.glAccountId,
            workEffortTypeId: "SUB_PROJECT",
            workEffortParentId: projectId
        },
        { skip: !projectId }
    );

    // === Effects ===
    useEffect(() => {
        if (projectId && projectId !== row.projectId) {
            handleRowChange(index, "projectId" as any, projectId);
        } else if (!projectId && row.projectId) {
            handleRowChange(index, "projectId" as any, undefined);
            handleRowChange(index, "subProjectId" as any, undefined);
            handleRowChange(index, "subProjectName" as any, undefined);
        }
    }, [projectId, row.projectId, index, handleRowChange]);

    useEffect(() => {
        if (subProjects && row.subProjectId) {
            const sp = subProjects.find((p: any) => p.workEffortId === row.subProjectId);
            if (sp && sp.subProjectName !== row.subProjectName) {
                handleRowChange(index, "subProjectName" as any, sp.subProjectName);
            }
        }
    }, [subProjects, row.subProjectId, row.subProjectName, index, handleRowChange]);

    return (
        <TableRow key={row.tempId}>
            {/* GL Account */}
            <TableCell sx={{ minWidth: 350 }}>
                <FormDropDownTreeGlAccount3
                    data={glAccounts || []}
                    value={row.glAccountId}
                    onChange={(e: any) => handleRowChange(index, "glAccountId", e.value)}
                    dataItemKey="glAccountId"
                    textField="text"
                    selectField="selected"
                    expandField="expanded"
                    name="glAccountId"
                    touched={false}
                    visited={false}
                    modified={false}
                />
            </TableCell>

            {/* Description */}
            <TableCell sx={{ minWidth: 250 }}>
                <TextField
                    fullWidth
                    size="small"
                    value={row.description || ""}
                    onChange={(e) => handleRowChange(index, "description", e.target.value)}
                />
            </TableCell>

            {/* Amount */}
            <TableCell sx={{ minWidth: 120 }}>
                <TextField
                    fullWidth
                    size="small"
                    type="number"
                    value={localAmount ?? ""}
                    onChange={(e) => handleRowChange(index, "amount", e.target.value)}
                    inputProps={{ min: 0, step: "0.01" }}
                />
            </TableCell>

            {/* Estimated Start Date - Fixed for calendar */}
            <TableCell sx={{ minWidth: 150 }}>
                <TextField
                    fullWidth
                    size="small"
                    type="date"
                    value={row.estimatedStartDate || ""}
                    onChange={(e) => handleRowChange(index, "estimatedStartDate", e.target.value)}
                    InputLabelProps={{ shrink: true }}
                    onClick={(e) => e.stopPropagation()}
                />
            </TableCell>

            {/* Item Type */}
            <TableCell sx={{ minWidth: 200 }}>
                <MemoizedFormComboBox2
                    data={itemTypes}
                    textField="description"
                    dataItemKey="itemType"
                    value={row.itemType || null}
                    onChange={(e: any) => handleRowChange(index, "itemType", e.value)}
                />
            </TableCell>

            {/* Service */}
            <TableCell sx={{ minWidth: 300 }}>
                <FormSimpleComboBoxServiceVirtual
                    value={row.serviceId}
                    onChange={(e: any) => handleRowChange(index, "serviceId", e.value)}
                    textField="productName"
                    dataItemKey="productId"
                    name="serviceId"
                    touched={false}
                    visited={false}
                    modified={false}
                />
            </TableCell>

            {/* Product */}
            <TableCell sx={{ minWidth: 300 }}>
                <FormSimpleComboBoxRawMaterialVirtual
                    value={row.productId}
                    onChange={(e: any) => handleRowChange(index, "productId", e.value)}
                    textField="productName"
                    dataItemKey="productId"
                    name="productId"
                    touched={false}
                    visited={false}
                    modified={false}
                    disabled={row.itemType !== "MATERIALS"}
                />
            </TableCell>

            {/* Supplier */}
            <TableCell sx={{ minWidth: 300 }}>
                <FormComboBoxVirtualAllParties
                    value={row.party}
                    onChange={(e: any) => handleRowChange(index, "party", e.value)}
                    name="party"
                    touched={false}
                    visited={false}
                    modified={false}
                />
            </TableCell>

            {/* Sub Project */}
            <TableCell sx={{ minWidth: 250 }}>
                {subProjects && subProjects.length > 0 ? (
                    <MemoizedFormComboBox2
                        data={subProjects.filter((sp: any) => !!sp)}
                        textField="subProjectName"
                        dataItemKey="workEffortId"
                        value={row.subProjectId || null}
                        onChange={(e: any) => {
                            const selectedId = e?.value;
                            const sp = subProjects.find((p: any) => p.workEffortId === selectedId);
                            handleRowChange(index, "subProjectId", selectedId, {
                                subProjectName: sp?.subProjectName || ""
                            });
                        }}
                    />
                ) : (
                    "-"
                )}
            </TableCell>

            {/* Cost Center */}
            <TableCell sx={{ minWidth: 250 }}>
                <MemoizedFormComboBox2
                    id={`costCenterId-${row.tempId}`}
                    name="costCenterId"
                    data={(costCenters || []).filter((cc: any) => !!cc)}
                    textField="description"
                    dataItemKey="costCenterId"
                    value={row.costCenterId || null}
                    onChange={(e: any) => {
                        const selectedId = e?.value;
                        const selectedCc = costCenters.find((c: any) => c.costCenterId === selectedId);
                        handleRowChange(index, "costCenterId", selectedId, {
                            costCenterName: selectedCc?.description || ""
                        });
                    }}
                />
            </TableCell>

            {/* Delete */}
            <TableCell sx={{ width: 50 }}>
                <IconButton
                    color="error"
                    onClick={() => handleRemoveRow(index)}
                    disabled={rowsCount === 1}
                >
                    <DeleteIcon />
                </IconButton>
            </TableCell>
        </TableRow>
    );
});