// Inside your form submit handler
const [createBillingAccount] = useCreateBillingAccountMutation(); // ← add this hook

const handleSubmit = async (values: BillingAccount) => {
    try {
        await createBillingAccount({
            partyId: values.partyId,
            projectId: values.projectId,
            accountLimit: values.accountLimit,
            fromDate: values.fromDate,
            thruDate: values.thruDate,
            description: values.description
        }).unwrap();

        onClose(); // close form and go back to list
    } catch (error) {
        console.error("Failed to create billing account", error);
    }
};