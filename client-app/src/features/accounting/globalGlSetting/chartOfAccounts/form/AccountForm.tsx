import {Box, Button, Grid, Paper, Typography} from '@mui/material'
import {Field, Form, FormElement} from '@progress/kendo-react-form'
import React, {useRef, useState} from 'react'
import FormInput from '../../../../../app/common/form/FormInput'
import {MemoizedFormDropDownList} from '../../../../../app/common/form/MemoizedFormDropDownList'
import {useSelector} from 'react-redux'
import {accountClasses, accountTypes, parentAccounts, resourceTypes} from '../../../slice/accountingUiSlice'
import {GlAccount} from '../../../../../app/models/accounting/globalGlSettings'
import AccountingMenu from '../../../invoice/menu/AccountingMenu'

interface Props {
    selectedAccount?: GlAccount
    editMode: number
    cancelEdit: () => void
    onSubmit: (data: any) => void
}

const AccountForm = ({selectedAccount, editMode, cancelEdit, onSubmit}: Props) => {
    const [formEditMode, setFormEditMode] = useState(editMode)
    const formRef = useRef(null)
    console.log(selectedAccount)
    const parentAccountsList = useSelector(parentAccounts)
    const accountClassesList = useSelector(accountClasses)
    const accountTypesList = useSelector(accountTypes)
    const resourceTypesList = useSelector(resourceTypes)

    return (
        <>
            <AccountingMenu selectedMenuItem='globalGL'/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container>
                    <Grid item xs={12}>
                        <Typography color={"green"} sx={{p: 2}} variant='h4'>
                            Global GL Account
                        </Typography>
                    </Grid>
                    <Grid item xs={3}>
                        <Typography color={"black"} sx={{p: 2}} variant='h5'>
                            Account Number:
                        </Typography>
                    </Grid>
                    <Grid item xs={3}>
                        <Box display='flex' justifyContent="left">
                            <Typography sx={{p: 2}}
                                        variant='h5'> {selectedAccount && selectedAccount?.glAccountId} </Typography>
                        </Box>
                    </Grid>
                </Grid>

                <Form initialValues={selectedAccount ? selectedAccount : {}}
                      ref={formRef}
                      key={JSON.stringify(selectedAccount)}
                      onSubmit={values => onSubmit(values)}
                      render={(formRenderPropd) => (
                          <FormElement>
                              <fieldset className={'k-form-fieldset'}>
                                  <Grid container spacing={2} alignItems={"center"}>
                                      <Grid item xs={4}>
                                          <Field component={FormInput}
                                                 disabled={true}
                                                 id={"glAccountId"}
                                                 name={"glAccountId"}
                                                 label={"GL Account Id"}
                                          />
                                      </Grid>
                                      <Grid item xs={4}>
                                          <Field component={MemoizedFormDropDownList}
                                                 data={parentAccountsList ? parentAccountsList : []}
                                                 dataItemKey={"parentGlAccountId"}
                                                 textField={"parentAccountName"}
                                                 id={"parentGlAccountId"}
                                                 name={"parentGlAccountId"}
                                                 label={"Parent GL Account"}
                                          />
                                      </Grid>
                                      <Grid item xs={4}>
                                          <Field component={MemoizedFormDropDownList}

                                                 data={accountClassesList ? accountClassesList : []}
                                                 dataItemKey={"glAccountClassId"}
                                                 textField={"glAccountClassDescription"}
                                                 id={"glAccountClassId"}
                                                 name={"glAccountClassId"}
                                                 label={"GL Account Class"}
                                          />
                                      </Grid>
                                      <Grid item xs={4}>
                                          <Field component={MemoizedFormDropDownList}

                                                 data={accountTypesList ? accountTypesList : []}
                                                 dataItemKey={"glAccountTypeId"}
                                                 textField={"glAccountTypeDescription"}
                                                 id={"glAccountTypeId"}
                                                 name={"glAccountTypeId"}
                                                 label={"GL Account Type Id"}
                                          />
                                      </Grid>
                                      <Grid item xs={4}>
                                          <Field component={MemoizedFormDropDownList}
                                                 data={resourceTypesList ? resourceTypesList : []}
                                                 dataItemKey={"glResourceTypeId"}
                                                 textField={"glResourceTypeDescription"}
                                                 id={"glResourceTypeId"}
                                                 name={"glResourceTypeId"}
                                                 label={"GL Resource Type Id"}
                                          />
                                      </Grid>
                                      <Grid item xs={4}>
                                          <Field component={FormInput}
                                                 id={"accountCode"}
                                                 name={"accountCode"}
                                                 label={"Account Code"}
                                          />
                                      </Grid>
                                      <Grid item xs={4}>
                                          <Field component={FormInput}
                                                 id={"accountName"}
                                                 name={"accountName"}
                                                 label={"Account Name"}
                                          />
                                      </Grid>
                                      <Grid item xs={4}>
                                          <Field component={FormInput}
                                                 id={"description"}
                                                 name={"description"}
                                                 label={"Description"}
                                          />
                                      </Grid>
                                      <Grid item xs={4}>
                                          <Field component={FormInput}
                                                 id={"productId"}
                                                 name={"productId"}
                                                 label={"Product ID"}
                                          />
                                      </Grid>
                                      <Grid item xs={4}>
                                          <Field component={FormInput}
                                                 id={"externalId"}
                                                 name={"externalId"}
                                                 label={"External ID"}
                                          />
                                      </Grid>
                                  </Grid>
                              </fieldset>
                              <div className="k-form-buttons">
                                  <Grid item container spacing={2}>
                                      <Grid item xs={1}>
                                          <Button
                                              variant="contained"
                                              type={'submit'}
                                              color="success"
                                          >
                                              Update
                                          </Button>
                                      </Grid>
                                      <Grid item xs={3}>
                                          <Button onClick={() => cancelEdit()} color="error" variant="contained">
                                              Cancel
                                          </Button>
                                      </Grid>

                                  </Grid>
                              </div>
                          </FormElement>
                      )}
                />
            </Paper>
        </>
    )
}

export default AccountForm