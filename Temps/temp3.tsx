<Form
    initialValues={{ ...initialValues, _itemsVersion: 0 }}
    onSubmit={handleSubmit}
    render={(formRenderProps) => (
        <>
            {/* Hidden field that Kendo watches */}
            <Field name="_itemsVersion" type="hidden" component="input" />

            {/* rest of form ... */}

            <Button
                type="submit"
                disabled={
                    !formRenderProps.valid ||
                    (!formRenderProps.modified && editMode !== 1) ||
                    apiLoading ||
                    items.length === 0
                }
            >
                ...
            </Button>
        </>
    )}
/>