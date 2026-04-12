useEffect(() => {
    if (projectId !== row.projectId) {
        handleRowChange(index, "projectId" as any, projectId);
        if (!projectId) {
            handleRowChange(index, "subProjectId" as any, undefined);
            handleRowChange(index, "subProjectName" as any, undefined);
        }
    }
}, [projectId, row.projectId]);   // removed index & handleRowChange from deps (they are stable)

useEffect(() => {
    if (subProjects && row.subProjectId) {
        const sp = subProjects.find((p: any) => p.workEffortId === row.subProjectId);
        if (sp && sp.subProjectName !== row.subProjectName) {
            handleRowChange(index, "subProjectName" as any, sp.subProjectName);
        }
    }
}, [subProjects, row.subProjectId, row.subProjectName]);   // removed index & handleRowChange