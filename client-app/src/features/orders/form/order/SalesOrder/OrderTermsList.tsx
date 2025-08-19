import React, { useCallback, useState } from 'react'
import { useAppDispatch, useAppSelector, useFetchOrderTermsQuery } from '../../../../../app/store/configureStore'
import { orderBy, SortDescriptor, State } from '@progress/kendo-data-query';
import ModalContainer from '../../../../../app/common/modals/ModalContainer';
import { Button, Grid } from '@mui/material';
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import { useSelector } from 'react-redux';
import { orderTermsEntities } from '../../../slice/orderTermsUiSlice';
import OrderTermForm from './OrderTermForm';
import { OrderTerm } from '../../../../../app/models/order/orderTerm';
import { useTranslationHelper } from '../../../../../app/hooks/useTranslationHelper';
import { selectAdjustedOrderTerms } from '../../../slice/orderSelectors';

interface OrderTermsListProps {
    onClose: () => void
    orderId: string
}

const OrderTermsList = ({onClose, orderId}: OrderTermsListProps) => {
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const initialSort: Array<SortDescriptor> = [
        {field: "termId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const dispatch = useAppDispatch();
    const [editMode, setEditMode] = useState(0);
    const [show, setShow] = useState(false);
    const [orderTerm, setOrderTerm] = useState<OrderTerm | undefined>()
    const orderFormEditMode: any = useAppSelector(state => state.ordersUi.orderFormEditMode);
    const orderTermsUi = useSelector(orderTermsEntities)
    const selectedOrderTerms = useSelector(selectAdjustedOrderTerms)
    const {getTranslatedLabel} = useTranslationHelper()
    const localizationKey = 'order.so.terms'
    const {data: orderTermsData} = useFetchOrderTermsQuery(orderId)

    const memoizedOnClose = useCallback(
        () => {
            setShow(false)
            setOrderTerm(undefined)
        },
        [],
    );
    
    console.log('orderTermsUi', orderTermsUi)
    console.log('selectedOrderTerms', selectedOrderTerms)
    const handleSelectOrderTerm = (orderTermId: string) => {
        const selectedTerm = orderTermsUi?.find((ot: OrderTerm) => ot.orderId.concat(ot.termTypeId) === orderTermId)
        if (selectedTerm) {
            setOrderTerm(selectedTerm)
            setEditMode(2)
            setShow(true)
        }
    }
    const TermTypeCell = (props: any) => {
        return (
            <td>
                <Button
                    onClick={() => handleSelectOrderTerm(props.dataItem.orderId.concat(props.dataItem.termTypeId))}
                >
                    {props.dataItem.termTypeName}
                </Button>
            </td>
        )
    }
    
    console.log('selectedOrderTerms', selectedOrderTerms)
    
  return (
    <>
        {show && 
            <ModalContainer show={show} onClose={memoizedOnClose} width={800}>
                <OrderTermForm onClose={memoizedOnClose} selectedTerm={orderTerm} />
            </ModalContainer>
        }
        <Grid container padding={2} columnSpacing={1}>
            <Grid container>
                <div className="div-container">
                <KendoGrid className="main-grid"
                style={{height: "60vh", width: 770}}
                    data={orderBy(selectedOrderTerms ?? [], sort).slice(page.skip, page.take + page.skip)}
                    sortable={true}
                    sort={sort}
                    onSortChange={(e: GridSortChangeEvent) => {
                        setSort(e.sort);
                    }}
                    skip={page.skip}
                    take={page.take}
                    total={orderTermsUi ? orderTermsUi.length : 0}
                    pageable={true}
                    onPageChange={pageChange}
                >
                    <GridToolbar>
                        <Grid>
                            <Button variant={"contained"} disabled={orderFormEditMode > 2} onClick={() => {
                                setShow(true)
                                setEditMode(1)
                            }}>
                                {getTranslatedLabel(`${localizationKey}.add`, "Create Term")}
                            </Button>
                        </Grid>
                    </GridToolbar>
                    <Column field="termTypeId" title={getTranslatedLabel(`${localizationKey}.type`, "Term Type")} width={220} cell={TermTypeCell} />
                    <Column field="termDays" title={getTranslatedLabel(`${localizationKey}.days`, "Term Days")} />
                    <Column field="termValue" title={getTranslatedLabel(`${localizationKey}.termValue`, "Term Value")} />
                    <Column field="textValue" title={getTranslatedLabel(`${localizationKey}.textValue`, "Text Value")} />
                    <Column field="description" title={getTranslatedLabel(`${localizationKey}.description`, "Description")} />
                </KendoGrid>
                </div>
                <Grid container spacing={3} mt={1} pl={1}>
                    <Grid item>
                        <Button variant='contained' color="error"onClick={onClose}>
                            {getTranslatedLabel("general.close", "Close")}
                        </Button>
                    </Grid>
                </Grid>
            </Grid>
        </Grid>
    </>
  )
}

export default OrderTermsList