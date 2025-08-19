import React, {useCallback, useEffect, useMemo, useState} from "react";
import {useAppDispatch, useAppSelector, useChangeInvoiceStatusMutation,} from "../../../../app/store/configureStore";
import Grid from "@mui/material/Grid";
import {Paper, Typography} from "@mui/material";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import InvoiceItemsList from "../dashboard/InvoiceItemsList";
import useInvoice from "../hook/useInvoice";
import AccountingMenu from "../menu/AccountingMenu";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import InvoiceTransactionsList from "../../transaction/dashboard/InvoiceTransactionsList";
import {useNavigate, useOutletContext} from "react-router";
import Button from "@mui/material/Button";
import {setSelectedInvoice} from "../../slice/accountingSharedUiSlice";
import InvoicePaymentApplicationsList from "../dashboard/InvoicePaymentApplicationsList";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {toast} from "react-toastify";
import {Ribbon, RibbonContainer} from "react-ribbons";
import {useInvoiceTotal} from "../hook/useInvoiceTotal";

interface Props {
    invoiceId?: string;
    mode: "view" | "items";
}

export default function InvoiceDisplayForm({ invoiceId: propInvoiceId, mode}: Props) {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    // Purpose: Ensures invoiceId is always available, even if not passed as a prop
    // Improvement: Clarifies data source and prevents undefined invoiceId
    const { invoiceId: contextInvoiceId } = useOutletContext<{ invoiceId: string }>();
    // Use propInvoiceId if provided, otherwise fall back to contextInvoiceId
    const effectiveInvoiceId = propInvoiceId || contextInvoiceId;

    
    const selectedInvoice = useAppSelector((state) => state.accountingSharedUi.selectedInvoice);
    const {invoice, setInvoice} = useInvoice(effectiveInvoiceId);
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.invoices.display.form";
    const language = useAppSelector((state) => state.localization.language);
    const isArabic = language === "ar";

    const [showTransactionsList, setShowTransactionsList] = useState(false);
    const [showPaymentsList, setShowPaymentsList] = useState(false);

    const [changeStatus, {isLoading: isChangingStatus}] = useChangeInvoiceStatusMutation();
    const {iTotal, isLoading: isTotalLoading, error: totalError} = useInvoiceTotal(invoice?.invoiceId);


    const invoiceSource = useMemo(() => selectedInvoice || invoice, [selectedInvoice, invoice]);
    const invoiceType = useMemo(() => invoiceSource?.invoiceTypeId, [invoiceSource]);
    console.log('invoiceType:', invoiceType);
    useEffect(() => {
        if (selectedInvoice) {
            setInvoice(selectedInvoice);
        }
        return () => {
            dispatch(setSelectedInvoice(undefined));
        };
    }, [dispatch, selectedInvoice, setInvoice]);

    const permissions = useMemo(() => {
        const hasInvoiceId = !!invoiceSource?.invoiceId;
        const status = invoiceSource?.statusId;
        return {
            showInvoiceOverview: hasInvoiceId,
            canEditInvoice: hasInvoiceId && status === "INVOICE_IN_PROCESS",
            canEditInvoiceItems: hasInvoiceId && (status === "INVOICE_IN_PROCESS" || invoiceSource?.invoiceTypeId === "CUST_RTN_INVOICE"),
            canEditInvoiceApplications: hasInvoiceId && ["INVOICE_IN_PROCESS", "INVOICE_APPROVED", "INVOICE_READY"].includes(status),
            canSendPerEmail: hasInvoiceId,
        };
    }, [invoiceSource]);
    
    console.log('permissions:', permissions);

    const handleChangeInvoiceStatus = useCallback(
        async (statusId: string) => {
            if (!invoice?.invoiceId) {
                toast.error(getTranslatedLabel(`${localizationKey}.error`, "No invoice selected"));
                return;
            }

            if (totalError) {
                toast.error(getTranslatedLabel(`${localizationKey}.error`, "Failed to fetch invoice balance"));
                return;
            }

            if (statusId === "INVOICE_PAID" && typeof iTotal !== "number") {
                toast.error(
                    getTranslatedLabel(
                        `${localizationKey}.error`,
                        "Cannot set to Paid: Invalid invoice total"
                    )
                );
                return;
            }

            // REFACTOR: Removed confirmation prompt for INVOICE_PAID when iTotal > 0, as the menu option is now disabled in this case to prevent user attempts, improving UX by avoiding unnecessary prompts.
            if (
                statusId === "INVOICE_WRITEOFF" &&
                !window.confirm(
                    getTranslatedLabel(
                        `${localizationKey}.confirm-writeoff`,
                        `Do you want to write off this invoice number ${invoice.invoiceId}?`
                    )
                )
            ) {
                return;
            }
            if (
                statusId === "INVOICE_CANCELLED" &&
                !window.confirm(
                    getTranslatedLabel(
                        `${localizationKey}.confirm-cancel`,
                        `Do you want to cancel this invoice number ${invoice.invoiceId}?`
                    )
                )
            ) {
                return;
            }

            const statusChangeBody = {
                invoiceId: invoice.invoiceId,
                statusDate: new Date(),
                paidDate: statusId === "INVOICE_PAID" ? new Date() : invoice?.paidDate,
                statusId,
            };

            try {
                const result = await changeStatus(statusChangeBody).unwrap();
                setInvoice((prev) => ({
                    ...prev,
                    statusId: result.statusId,
                    statusDescription: result.statusDescription,
                }));
                toast.success(
                    getTranslatedLabel(
                        `${localizationKey}.status-changed`,
                        `Status successfully changed to ${result.statusDescription}`
                    )
                );
            } catch (e) {
                toast.error(getTranslatedLabel(`${localizationKey}.error`, "Something went wrong"));
                console.error(e);
            }
        },
        [invoice, iTotal, changeStatus, setInvoice, getTranslatedLabel, totalError]
    );


    const handleBackClick = () => {
        navigate("/invoices");
    };

    // Purpose: Aligns navigation with nested routes (/invoices/:invoiceId/edit, /invoices/new)
    // Improvement: Adds support for transactions/payments routes and improves UX
    const handleMenuSelect = async (e: MenuSelectEvent) => {
        const menuData = e.item.data || e.item.text;
        //console.log("handleMenuSelect:", { menuData, invoiceId, statusId: invoice?.statusId, invoice });

        switch (menuData) {
            case "new":
                navigate("/invoices/new");
                break;
            case "transactions":
                // REFACTOR: Support both modal and route-based transactions view
                // Purpose: Allows flexibility to use modal or full page based on context
                // Improvement: Future-proofs for route-based rendering
                if (location.pathname.includes("/transactions")) {
                    navigate(`/invoices/${invoice?.invoiceId}/transactions`);
                } else {
                    setShowTransactionsList(true);
                }
                break;
            case "payment-applications":
                if (location.pathname.includes("/payments")) {
                    navigate(`/invoices/${invoice?.invoiceId}/payments`);
                } else {
                    setShowPaymentsList(true);
                }
                break;
            case "update":
                console.log("Invoice:", invoice, "Status:", invoice?.statusId);
                if (invoice?.statusId === "INVOICE_IN_PROCESS") {
                    navigate(`/invoices/${invoice?.invoiceId}/edit`);
                } else {
                    toast.error(getTranslatedLabel(`${localizationKey}.error`, "Cannot edit: Invalid status"));
                }
                break;
            case "approve":
                handleChangeInvoiceStatus("INVOICE_APPROVED");
                break;
            case "ready":
                handleChangeInvoiceStatus("INVOICE_READY");
                break;
            case "paid":
                handleChangeInvoiceStatus("INVOICE_PAID");
                break;
            case "writeoff":
                handleChangeInvoiceStatus("INVOICE_WRITEOFF");
                break;
            case "cancel":
                handleChangeInvoiceStatus("INVOICE_CANCELLED");
                break;
            default:
                console.warn(`Unhandled menu item: ${menuData}`);
        }
    };

    // Purpose: Ensures consistent status rendering with fallback
    // Improvement: Reduces repetition and improves maintainability
    const renderSwitchStatus = () => {
        const statusId = invoice?.statusId || "UNKNOWN";
        const statusConfig: Record<string, { label: string; backgroundColor: string; foreColor: string }> = {
            INVOICE_IN_PROCESS: {
                label: getTranslatedLabel(`${localizationKey}.status.in-process`, "In Process"),
                backgroundColor: "green",
                foreColor: "#ffffff",
            },
            INVOICE_APPROVED: {
                label: getTranslatedLabel(`${localizationKey}.status.approved`, "Approved"),
                backgroundColor: "yellow",
                foreColor: "#000000",
            },
            INVOICE_READY: {
                label: getTranslatedLabel(`${localizationKey}.status.ready`, "Ready"),
                backgroundColor: "blue",
                foreColor: "#ffffff",
            },
            INVOICE_PAID: {
                label: getTranslatedLabel(`${localizationKey}.status.paid`, "Paid"),
                backgroundColor: "green",
                foreColor: "#ffffff",
            },
            INVOICE_WRITEOFF: {
                label: getTranslatedLabel(`${localizationKey}.status.writeoff`, "Write Off"),
                backgroundColor: "blue",
                foreColor: "#ffffff",
            },
            INVOICE_CANCELLED: {
                label: getTranslatedLabel(`${localizationKey}.status.cancelled`, "Cancelled"),
                backgroundColor: "red",
                foreColor: "#ffffff",
            },
            UNKNOWN: {
                label: getTranslatedLabel(`${localizationKey}.status.unknown`, "Unknown"),
                backgroundColor: "gray",
                foreColor: "#ffffff",
            },
        };
        return statusConfig[statusId] || statusConfig.UNKNOWN;
    };


    const status = renderSwitchStatus();


    const getAvailableStatusTransitions = () => {
        if (!invoiceSource) return {};
        return {
            toApproved: invoiceSource.statusId === "INVOICE_IN_PROCESS",
            toReady: invoiceSource.statusId === "INVOICE_APPROVED",
            toPaid: invoiceSource.statusId === "INVOICE_READY" && iTotal === 0,
            toWriteoff: invoiceSource.statusId === "INVOICE_READY",
            toCancelled: invoiceSource.statusId === "INVOICE_READY",
        };
    };

    return (
        <>
            {showTransactionsList && (
                <ModalContainer show={showTransactionsList} onClose={() => setShowTransactionsList(false)} width={950}>
                    <InvoiceTransactionsList
                        onClose={() => setShowTransactionsList(false)}
                        invoiceId={invoice?.invoiceId}
                        invoiceType={invoiceType} // Pass invoiceType prop
                    />
                </ModalContainer>
            )}
            {showPaymentsList && (
                <ModalContainer
                    show={showPaymentsList}
                    onClose={() => setShowPaymentsList(false)}
                    width={950}
                >
                    <InvoicePaymentApplicationsList
                        onClose={() => setShowPaymentsList(false)}
                    />
                </ModalContainer>
            )}
            <AccountingMenu selectedMenuItem="/invoices"/>
            <RibbonContainer>
                <Paper elevation={5} sx={{padding: 3, mt: 2}} className="div-container-withBorderCurved">
                    <Grid container spacing={2}>
                        <Grid item xs={10}>
                            <Typography
                                sx={{
                                    fontWeight: "bold",
                                    paddingLeft: 2,
                                    marginRight: 1,
                                    fontSize: "18px",
                                    color: invoice?.statusId === "INVOICE_IN_PROCESS" ? "green" : "black",
                                }}
                                variant="h6"
                            >
                                {getTranslatedLabel(`${localizationKey}.invoice-no`, "Invoice No:")}{" "}
                                <span>{invoice?.invoiceId}</span>
                            </Typography>
                        </Grid>
                        <Grid item xs={2}>
                            <Menu onSelect={handleMenuSelect}>
                                <MenuItem text={getTranslatedLabel("general.actions", "Actions")}>
                                    <MenuItem
                                        text={getTranslatedLabel(`${localizationKey}.actions.new`, "Create New Invoice")}
                                        data="new"
                                    />
                                    {invoice?.statusId === "INVOICE_IN_PROCESS" && (
                                        <MenuItem
                                            text={getTranslatedLabel(`${localizationKey}.actions.update`, "Edit")}
                                            data="update"
                                        />
                                    )}
                                    {getAvailableStatusTransitions().toApproved && (
                                        <MenuItem
                                            text={getTranslatedLabel(
                                                `${localizationKey}.actions.approve`,
                                                "Status to 'Approved'"
                                            )}
                                            data="approve"
                                        />
                                    )}
                                    {getAvailableStatusTransitions().toReady && (
                                        <MenuItem
                                            text={getTranslatedLabel(`${localizationKey}.actions.ready`, "Status to 'Ready'")}
                                            data="ready"
                                        />
                                    )}
                                    {getAvailableStatusTransitions().toPaid && (
                                        <MenuItem
                                            text={getTranslatedLabel(
                                                `${localizationKey}.actions.paid`,
                                                "Status to 'Paid'"
                                            )}
                                            data="paid"
                                            // REFACTOR: Removed redundant disabled prop, as the toPaid condition in getAvailableStatusTransitions now handles disabling when iTotal > 0, ensuring consistency and reducing conditional logic in the JSX.
                                            disabled={isTotalLoading || !!totalError}
                                        />
                                    )}
                                    {getAvailableStatusTransitions().toWriteoff && (
                                        <MenuItem
                                            text={getTranslatedLabel(
                                                `${localizationKey}.actions.writeoff`,
                                                "Status to 'Writeoff'"
                                            )}
                                            data="writeoff"
                                        />
                                    )}
                                    {getAvailableStatusTransitions().toCancelled && (
                                        <MenuItem
                                            text={getTranslatedLabel(
                                                `${localizationKey}.actions.cancel`,
                                                "Status to 'Cancelled'"
                                            )}
                                            data="cancel"
                                        />
                                    )}
                                </MenuItem>
                            </Menu>
                        </Grid>
                        <Grid item xs={1}>
                            {invoice?.statusId !== "INVOICE_IN_PROCESS" && (
                                <Ribbon
                                    side="right"
                                    type="corner"
                                    size="large"
                                    backgroundColor={status.backgroundColor}
                                    color={status.foreColor}
                                    fontFamily="sans-serif"
                                >
                                    {status.label}
                                </Ribbon>
                            )}
                        </Grid>
                    </Grid>
                    <Grid container spacing={2} mt={3}>
                        <Grid item xs={6}>
                            <Typography variant="h6" sx={{pl: 2}}>
                                {getTranslatedLabel(`${localizationKey}.invoice-type`, "Invoice Type:")}{" "}
                                <strong>{invoice?.invoiceTypeDescription || "N/A"}</strong>
                            </Typography>
                            <Typography variant="h6" sx={{pl: 2}}>
                                {getTranslatedLabel(`${localizationKey}.from-party`, "From Party:")}{" "}
                                <strong>{invoice?.fromPartyName || "N/A"}</strong>
                            </Typography>
                            <Typography variant="h6" sx={{pl: 2}}>
                                {getTranslatedLabel(`${localizationKey}.invoice-date`, "Invoice Date:")}{" "}
                                <strong>
                                    {invoice?.invoiceDate
                                        ? new Date(invoice.invoiceDate).toLocaleDateString(isArabic ? "ar-EG" : "en-GB")
                                        : "N/A"}
                                </strong>
                            </Typography>
                            <Typography variant="h6" sx={{pl: 2}}>
                                {getTranslatedLabel(`${localizationKey}.total`, "Total:")}{" "}
                                <span style={{fontWeight: "bold", color: "red", marginLeft: "10px"}}>
                  {iTotal || 0}
                </span>
                            </Typography>
                            {invoice?.billingAccountId && (
                                <Typography variant="h6" sx={{pl: 2}}>
                                    {getTranslatedLabel(`${localizationKey}.billing-account`, "Billing Account:")}{" "}
                                    <strong>
                    <span style={{color: "blue"}}>
                      {invoice?.billingAccountId} - {invoice?.billingAccountName}
                    </span>
                                    </strong>
                                </Typography>
                            )}
                        </Grid>
                        <Grid item xs={6}>
                            <Typography variant="h6">
                                {getTranslatedLabel(`${localizationKey}.invoice-status`, "Status:")}{" "}
                                <strong>{invoice?.statusDescription || "N/A"}</strong>
                            </Typography>
                            <Typography variant="h6">
                                {getTranslatedLabel(`${localizationKey}.to-party`, "Party To:")}{" "}
                                <strong>{invoice?.toPartyName || "N/A"}</strong>
                            </Typography>
                            <Typography variant="h6">
                                {getTranslatedLabel(`${localizationKey}.due-date`, "Due Date:")}{" "}
                                <strong>
                                    {invoice?.dueDate
                                        ? new Date(invoice.dueDate).toLocaleDateString(isArabic ? "ar-EG" : "en-GB")
                                        : "N/A"}
                                </strong>
                            </Typography>
                            <Typography variant="h6">
                                {getTranslatedLabel(`${localizationKey}.paid-date`, "Paid Date:")}{" "}
                                <strong>
                                    {invoice?.paidDate
                                        ? new Date(invoice.paidDate).toLocaleDateString(isArabic ? "ar-EG" : "en-GB")
                                        : "N/A"}
                                </strong>
                            </Typography>
                        </Grid>
                    </Grid>

                    <Grid container mt={2}>
                        <Grid item xs={10}>
                            <InvoiceItemsList
                                invoiceId={invoice?.invoiceId}
                                canEdit={permissions.canEditInvoiceItems}
                            />
                        </Grid>
                        <Grid item xs={2}>
                            <Menu onSelect={handleMenuSelect} vertical>
                                <MenuItem
                                    text={getTranslatedLabel(`${localizationKey}.actions.transactions`, "Transactions")}
                                    data="transactions"
                                />
                                <MenuItem
                                    text={getTranslatedLabel(
                                        `${localizationKey}.actions.payment-applications`,
                                        "Payment Applications"
                                    )}
                                    data="payment-applications"
                                />
                            </Menu>
                        </Grid>
                    </Grid>

                    <Grid container mt={3} spacing={1}>
                        <Grid item xs={2}>
                            {mode === "items" && permissions.canEditInvoice && (
                                <Button
                                    variant="contained"
                                    color="primary"
                                    onClick={() => navigate(`/invoices/${invoice?.invoiceId}/edit`)}
                                    //sx={{ mr: 2 }}
                                >
                                    {getTranslatedLabel(`${localizationKey}.edit-invoice`, "Edit Invoice")}
                                </Button>
                            )}
                        </Grid>
                        
                        <Grid item xs={2}>
                            <Button variant="contained" color="error" onClick={handleBackClick}>
                                {getTranslatedLabel(`${localizationKey}.back`, "Back")}
                            </Button>
                        </Grid>
                        
                    </Grid>

                    {isChangingStatus && (
                        <LoadingComponent
                            message={getTranslatedLabel(`${localizationKey}.processing`, "Processing Invoice...")}
                        />
                    )}
                </Paper>
            </RibbonContainer>
        </>
    );
}
