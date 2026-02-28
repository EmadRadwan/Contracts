import React, { useEffect, useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Grid,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Paper,
  Box,
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {
  useCreateProductCategoryMemberMutation,
  useDeleteProductCategoryMemberMutation,
  useFetchProductCategoryMembersQuery,
  useFetchProductCategoriesFlatQuery,
} from "../../../../app/store/apis";
import { MemoizedFormMuiAutoCompleteProductCategory } from "../../../../app/common/form/FormMuiAutoCompleteProductCategory";

interface Props {
  productId: string;
  productName: string;
  open: boolean;
  onClose: () => void;
}

export default function ManageProductCategoryMembersModal({
  productId,
  productName,
  open,
  onClose,
}: Props) {
  const { getTranslatedLabel } = useTranslationHelper();
  const [newMemberCategoryId, setNewMemberCategoryId] = useState<string | null>(null);

  const { data: categoryMembers, isLoading: isLoadingMembers, refetch: refetchMembers } = useFetchProductCategoryMembersQuery(
    productId,
    { skip: !productId }
  );

  useEffect(() => {
    if (open && productId) {
      refetchMembers();
    }
  }, [open, productId, refetchMembers]);

  const { data: productCategoriesFlat, isLoading: isLoadingCategories } = useFetchProductCategoriesFlatQuery(undefined);

  const [createMember, { isLoading: isAdding }] = useCreateProductCategoryMemberMutation();
  const [deleteMember, { isLoading: isDeleting }] = useDeleteProductCategoryMemberMutation();

  const handleAddMember = async () => {
    if (!productId || !newMemberCategoryId) return;
    try {
      await createMember({
        productId: productId,
        productCategoryId: newMemberCategoryId,
        fromDate: new Date().toISOString(),
      }).unwrap();
      toast.success(getTranslatedLabel("product.category.member.addedSuccess", "Category member added successfully"));
      setNewMemberCategoryId(null);
    } catch (error) {
      toast.error(getTranslatedLabel("product.category.member.addedError", "Failed to add category member"));
    }
  };

  const handleDeleteMember = async (member: any) => {
    try {
      await deleteMember({
        productId: member.productId,
        productCategoryId: member.productCategoryId,
        fromDate: member.fromDate,
      }).unwrap();
      toast.success(getTranslatedLabel("product.category.member.deletedSuccess", "Category member deleted successfully"));
    } catch (error) {
      toast.error(getTranslatedLabel("product.category.member.deletedError", "Failed to delete category member"));
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        {getTranslatedLabel("product.category.members.manageTitle", "Manage Category Memberships for")} {productName}
      </DialogTitle>
      <DialogContent dividers>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <Box sx={{ p: 2, border: '1px solid #ddd', borderRadius: 1, mb: 2 }}>
              <Typography variant="h6" gutterBottom>
                {getTranslatedLabel("product.category.members.addTitle", "Add to Category")}
              </Typography>
              <Grid container spacing={2} alignItems="center">
                <Grid item xs={10}>
                  <MemoizedFormMuiAutoCompleteProductCategory
                    id="newMemberCategoryId"
                    name="newMemberCategoryId"
                    label={getTranslatedLabel("product.category.members.selectCategory", "Select Category")}
                    data={productCategoriesFlat || []}
                    dataItemKey="productCategoryId"
                    textField="description"
                    value={newMemberCategoryId}
                    onChange={(e: any) => setNewMemberCategoryId(e.value)}
                  />
                </Grid>
                <Grid item xs={2}>
                  <IconButton
                    color="primary"
                    onClick={handleAddMember}
                    disabled={!newMemberCategoryId || isAdding}
                    size="large"
                  >
                    <AddIcon fontSize="large" />
                  </IconButton>
                </Grid>
              </Grid>
            </Box>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="h6" gutterBottom>
              {getTranslatedLabel("product.category.members.currentMemberships", "Current Memberships")}
            </Typography>
            {isLoadingMembers || isLoadingCategories ? (
              <LoadingComponent message="Loading data..." />
            ) : (
              <TableContainer component={Paper}>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>{getTranslatedLabel("product.category.members.categoryId", "Category ID")}</TableCell>
                      <TableCell>{getTranslatedLabel("product.category.members.categoryName", "Category Name")}</TableCell>
                      <TableCell>{getTranslatedLabel("product.category.members.fromDate", "From Date")}</TableCell>
                      <TableCell align="right">{getTranslatedLabel("common.actions", "Actions")}</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {categoryMembers?.map((member: any) => (
                      <TableRow key={`${member.productCategoryId}-${member.fromDate}`}>
                        <TableCell>{member.productCategoryId}</TableCell>
                        <TableCell>{member.description}</TableCell>
                        <TableCell>{new Date(member.fromDate).toLocaleDateString()}</TableCell>
                        <TableCell align="right">
                          <IconButton
                            color="error"
                            onClick={() => handleDeleteMember(member)}
                            disabled={isDeleting}
                          >
                            <DeleteIcon />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                    {(!categoryMembers || categoryMembers.length === 0) && (
                      <TableRow>
                        <TableCell colSpan={4} align="center">
                          {getTranslatedLabel("product.category.members.noMemberships", "No additional memberships found")}
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} variant="outlined">
          {getTranslatedLabel("common.close", "Close")}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
