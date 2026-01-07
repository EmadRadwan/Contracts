<Dialog open={roleDialogOpen} onClose={() => setRoleDialogOpen(false)} maxWidth="xs" fullWidth>
    <DialogTitle>
        {getTranslatedLabel("party.parties.list.changeMainRoleTitle", "Change Main Role")}
    </DialogTitle>
    <DialogContent>
        <FormControl fullWidth margin="normal">
            <InputLabel>
                {getTranslatedLabel("party.parties.list.newRole", "New Role")}
            </InputLabel>
            <Select
                value={newRole}
                label={getTranslatedLabel("party.parties.list.newRole", "New Role")}
                onChange={(e) => setNewRole(e.target.value as string)}
            >
                <MenuItem value="CUSTOMER">
                    {getTranslatedLabel("party.roles.customer", "Customer")}
                </MenuItem>
                <MenuItem value="SUPPLIER">
                    {getTranslatedLabel("party.roles.supplier", "Supplier")}
                </MenuItem>
                <MenuItem value="EMPLOYEE">
                    {getTranslatedLabel("party.roles.employee", "Employee")}
                </MenuItem>
                <MenuItem value="CONTRACTOR">
                    {getTranslatedLabel("party.roles.contractor", "Contractor")}
                </MenuItem>
            </Select>
        </FormControl>
    </DialogContent>
    <DialogActions>
        <Button onClick={() => setRoleDialogOpen(false)}>
            {getTranslatedLabel("common.cancel", "Cancel")}
        </Button>
        <Button
            onClick={async () => {
                if (!selectedPartyId || !newRole) return;

                try {
                    await updateMainRole({
                        partyId: selectedPartyId,
                        mainRole: newRole,
                    }).unwrap();

                    toast.success(
                        getTranslatedLabel("party.parties.list.roleUpdatedSuccess", "Role updated successfully")
                    );
                    setRoleDialogOpen(false);
                    setSelectedPartyId(null);
                    setNewRole("");
                } catch (err) {
                    toast.error(
                        getTranslatedLabel("party.parties.list.roleUpdateFailed", "Failed to update role")
                    );
                }
            }}
            disabled={isUpdatingRole || !newRole}
            variant="contained"
            color="primary"
        >
            {isUpdatingRole
                ? getTranslatedLabel("common.saving", "Saving...")
                : getTranslatedLabel("common.save", "Save")}
        </Button>
    </DialogActions>
</Dialog>