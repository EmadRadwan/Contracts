export default function ProjectForm({ project, cancelEdit, editMode }: Props) {
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";

    const { data: glAccounts = [], isLoading: isLoadingGlAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(
        companyId,
        { skip: !companyId }
    );

    // Simple initial values - just raw data from project
    const initialValues = useMemo(() => {
        if (editMode !== 2 || !project) {
            return {
                workEffortId: null,
                projectName: "",
                estimatedStartDate: null,
                estimatedCompletionDate: null,
                currentStatusId: "",
                glAccountId: null,
                operatingExpenseGlAccountId: null,
            };
        }

        return {
            workEffortId: project.workEffortId,
            projectName: project.projectName,
            estimatedStartDate: project.estimatedStartDate ? new Date(project.estimatedStartDate) : null,
            estimatedCompletionDate: project.estimatedCompletionDate ? new Date(project.estimatedCompletionDate) : null,
            currentStatusId: project.currentStatusId || "",
            glAccountId: project.glAccountId,                    // raw ID is enough
            operatingExpenseGlAccountId: project.operatingExpenseGlAccountId, // raw ID is enough
        };
    }, [editMode, project]);   // ← removed glAccounts dependency

    // Force re-mount of the Form when glAccounts finish loading or project changes
    const formKey = useMemo(() => {
        return `${project?.workEffortId || 'new'}-${glAccounts.length > 0 ? 'loaded' : 'loading'}`;
    }, [project?.workEffortId, glAccounts.length]);

    // ... rest of your code (handleSubmitData, formValidator, etc.)

    if (isLoadingGlAccounts && editMode === 2) {
        return <LoadingComponent message="Loading GL Accounts..." />;
    }

    return (
        <>
            <ProjectMenu selectedMenuItem={"projects"} />
            <Paper elevation={5} className={`div-container-withBorderCurved`} style={{ padding: '16px' }}>
                {/* Title ... */}

                <Form
                    key={formKey}                     {/* ← This is the most important line */}
                    initialValues={initialValues}
                    validator={formValidator}
                    onSubmit={handleSubmitData}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={"k-form-fieldset"}>
                                {/* ... other fields ... */}

                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Field
                                            id="glAccountId"
                                            name="glAccountId"
                                            validator={requiredValidator}
                                            label={getTranslatedLabel("project.projects.form.glAccount", "GL Account")}
                                            component={FormDropDownTreeGlAccount2}
                                            data={glAccounts || []}
                                            dataItemKey="glAccountId"
                                            textField="text"
                                            selectField="selected"
                                            expandField="expanded"
                                        />
                                    </Grid>
                                    <Grid item xs={6}>
                                        <Field
                                            id="operatingExpenseGlAccountId"
                                            name="operatingExpenseGlAccountId"
                                            validator={requiredValidator}
                                            label={getTranslatedLabel("project.projects.form.operatingExpenseGlAccount", "Operating Expense GL Account")}
                                            component={FormDropDownTreeGlAccount2}
                                            data={glAccounts || []}
                                            dataItemKey="glAccountId"
                                            textField="text"
                                            selectField="selected"
                                            expandField="expanded"
                                        />
                                    </Grid>
                                </Grid>

                                {/* buttons ... */}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}