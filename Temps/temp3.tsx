// Remove these imports if no longer needed
// import { FormComboBoxVirtualProject } from "...";
// import { useFetchSubProjectsQuery } from "...";
// import { MemoizedFormDropDownList2 } from "...";

// Keep or adjust these
import { FormDropDownTreeGlAccount2 } from "../../../app/common/form/FormDropDownTreeGlAccount2"; // ← assume this exists

// ────────────────────────────────────────────────

interface Props {
    // ... same
}

// Inside component:
export default function MultiPaymentItemForm({ ... }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "projects.multiPaymentCertificate.itemForm";

    // Remove:
    // const [selectedProjectId, setSelectedProjectId] = useState<string>("");
    // const { data: subProjects, isLoading: subProjectsLoading } = useFetchSubProjectsQuery(...);

    // Keep or adjust initialValues
    const initialValues = useMemo((): Partial<MultiPaymentItem> => ({
        // Remove projectId, subProjectId, subProjectName
        // workEffortId, workEffortIdParent, itemType, serviceId, productId, description, amounts...
        overrideGlAccountId: multiPaymentItem?.overrideGlAccountId || "", // ← new
        // ... rest
    }), [multiPaymentItem]);

    const handleSubmit = useCallback((values: Partial<MultiPaymentItem>) => {
        const serializedValues: MultiPaymentItem = {
            // Remove:
            // projectId: ..., projectName: ..., subProjectId: ..., subProjectName: ...

            // Add:
            overrideGlAccountId: values.overrideGlAccountId || "",

            // Keep the rest (itemType, serviceId, productId, amounts, parties, etc.)
            workEffortId: editMode === 2 ? multiPaymentItem?.workEffortId || ... : ...,
        // ...
    };

        if (editMode === 1) addItem(serializedValues);
        else updateItem(serializedValues);

        onClose();
    }, [...]);

    // ────────────────────────────────────────────────

    return (
        <Form
            initialValues={initialValues}
            onSubmit={handleSubmit}
            validator={partyValidator}
            render={(formRenderProps: FormRenderProps) => (
                <FormElement>
                    {/* Remove project & sub-project fields */}

                    {/* Replace with: */}
                    <Grid item xs={6}>  {/* or xs={12} depending on layout */}
                        <Field
                            id="overrideGlAccountId"
                            name="overrideGlAccountId"
                            label={getTranslatedLabel(`${localizationKey}.overrideGlAccountId`, "Override GL Account *")}
                            component={FormDropDownTreeGlAccount2}
                            data={glAccounts || []}           // ← you need to pass this prop – see note below
                            dataItemKey="glAccountId"
                            textField="text"
                            selectField="selected"
                            expandField="expanded"
                            validator={requiredValidator}
                            disabled={formEditMode > 3}
                        />
                    </Grid>

                    {/* Keep itemType, service/product, supplier/contractor, description, amounts... */}
                    {/* ... */}
                </FormElement>
            )}
        />
    );
}