export interface ProductPromo {
    productPromoActionEnumId: string;
    productPromoId: string;
    promoText: string;
    amount: number
    condValue: number
    createdStamp: Date
    includeSubCategories: string
    inputParamEnumDescription: string
    inputParamEnumId: string
    lastUpdatedStamp?: Date
    operatorEnumDescription: string
    operatorEnumId: string
    productCategoryId: string
    productId: string
    productPromoActionEnumDescription: string
    productPromoActionSeqId: string
    productPromoCondSeqId: string
    productPromoRuleId: string
    promoName: string
    quantity: number
    isUsed?: boolean;
    fromDate?: Date;
    thruDate?: Date;
}

export const productPromoActions = [
    {
        ENUM_ID: "PROMO_GWP",
        ENUM_TYPE_ID: "PROD_PROMO_ACTION",
        ENUM_CODE: "PPA_GWP",
        SEQUENCE_ID: "01",
        DESCRIPTION: "Gift With Purchase"
    },
    {
        ENUM_ID: "PROMO_ORDER_AMOUNT",
        ENUM_TYPE_ID: "PROD_PROMO_ACTION",
        ENUM_CODE: "PPA_ORDER_AMOUNT",
        SEQUENCE_ID: "07",
        DESCRIPTION: "Order Amount Flat"
    },
    {
        ENUM_ID: "PROMO_ORDER_PERCENT",
        ENUM_TYPE_ID: "PROD_PROMO_ACTION",
        ENUM_CODE: "PPA_ORDER_PERCENT",
        SEQUENCE_ID: "06",
        DESCRIPTION: "Order Percent Discount"
    },
    {
        ENUM_ID: "PROMO_PROD_AMDISC",
        ENUM_TYPE_ID: "PROD_PROMO_ACTION",
        ENUM_CODE: "PPA_PROD_AMDISC",
        SEQUENCE_ID: "04",
        DESCRIPTION: "X Product for Y Discount"
    },
    {
        ENUM_ID: "PROMO_PROD_DISC",
        ENUM_TYPE_ID: "PROD_PROMO_ACTION",
        ENUM_CODE: "PPA_PROD_DISC",
        SEQUENCE_ID: "03",
        DESCRIPTION: "X Product for Y% Discount"
    },
    {
        ENUM_ID: "PROMO_PROD_PRICE",
        ENUM_TYPE_ID: "PROD_PROMO_ACTION",
        ENUM_CODE: "PPA_PROD_PRICE",
        SEQUENCE_ID: "05",
        DESCRIPTION: "X Product for Y Price"
    },
    {
        ENUM_ID: "PROMO_PROD_SPPRC",
        ENUM_TYPE_ID: "PROD_PROMO_ACTION",
        ENUM_CODE: "PPA_PROD_SPPRC",
        SEQUENCE_ID: "08",
        DESCRIPTION: "Product for [Special Promo] Price"
    },
    {
        ENUM_ID: "PROMO_SERVICE",
        ENUM_TYPE_ID: "PROD_PROMO_ACTION",
        ENUM_CODE: "SERVICE",
        SEQUENCE_ID: "10",
        DESCRIPTION: "Call Service"
    },
    {
        ENUM_ID: "PROMO_SHIP_CHARGE",
        ENUM_TYPE_ID: "PROD_PROMO_ACTION",
        ENUM_CODE: "PPA_SHIP_CHARGE",
        SEQUENCE_ID: "09",
        DESCRIPTION: "Shipping X% discount"
    },
    {
        ENUM_ID: "PROMO_TAX_PERCENT",
        ENUM_TYPE_ID: "PROD_PROMO_ACTION",
        ENUM_CODE: "PPA_TAX_PERCENT",
        SEQUENCE_ID: "11",
        DESCRIPTION: "Tax % Discount"
    }
]

export const productPromoConditions = [
    {
        ENUM_ID: "PPC_EQ",
        ENUM_TYPE_ID: "PROD_PROMO_COND",
        ENUM_CODE: "EQ",
        SEQUENCE_ID: "01",
        DESCRIPTION: "Is"
    },
    {
        ENUM_ID: "PPC_GT",
        ENUM_TYPE_ID: "PROD_PROMO_COND",
        ENUM_CODE: "GT",
        SEQUENCE_ID: "05",
        DESCRIPTION: "Is Greater Than"
    },
    {
        ENUM_ID: "PPC_GTE",
        ENUM_TYPE_ID: "PROD_PROMO_COND",
        ENUM_CODE: "GTE",
        SEQUENCE_ID: "06",
        DESCRIPTION: "Is Greater Than or Equal To"
    },
    {
        ENUM_ID: "PPC_LT",
        ENUM_TYPE_ID: "PROD_PROMO_COND",
        ENUM_CODE: "LT",
        SEQUENCE_ID: "03",
        DESCRIPTION: "Is Less Than"
    },
    {
        ENUM_ID: "PPC_LTE",
        ENUM_TYPE_ID: "PROD_PROMO_COND",
        ENUM_CODE: "LTE",
        SEQUENCE_ID: "04",
        DESCRIPTION: "Is Less Than or Equal To"
    },
    {
        ENUM_ID: "PPC_NEQ",
        ENUM_TYPE_ID: "PROD_PROMO_COND",
        ENUM_CODE: "NEQ",
        SEQUENCE_ID: "02",
        DESCRIPTION: "Is Not"
    }
]

export const inputParameters = [
    {
        "ENUM_ID": "PPIP_GEO_ID",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_GEO_ID",
        "SEQUENCE_ID": "18",
        "DESCRIPTION": "Shipping Destination"
    },
    {
        "ENUM_ID": "PPIP_LPMUP_AMT",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_LPMUP_AMT",
        "SEQUENCE_ID": "14",
        "DESCRIPTION": "List Price minus Unit Price (Amount)"
    },
    {
        "ENUM_ID": "PPIP_LPMUP_PER",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_LPMUP_PER",
        "SEQUENCE_ID": "15",
        "DESCRIPTION": "List Price minus Unit Price (Percent)"
    },
    {
        "ENUM_ID": "PPIP_NEW_ACCT",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_NEW_ACCT",
        "SEQUENCE_ID": "05",
        "DESCRIPTION": "Account Days Since Created"
    },
    {
        "ENUM_ID": "PPIP_ORDER_SHIPTOTAL",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_ORDER_SHIPTOTAL",
        "SEQUENCE_ID": "16",
        "DESCRIPTION": "Shipping Total"
    },
    {
        "ENUM_ID": "PPIP_ORDER_TOTAL",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_ORDER_TOTAL",
        "SEQUENCE_ID": "01",
        "DESCRIPTION": "Cart Sub-total"
    },
    {
        "ENUM_ID": "PPIP_ORST_HIST",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_ORST_HIST",
        "SEQUENCE_ID": "10",
        "DESCRIPTION": "Order sub-total X in last Y Months"
    },
    {
        "ENUM_ID": "PPIP_ORST_LAST_YEAR",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_ORST_LAST_YEAR",
        "SEQUENCE_ID": "13",
        "DESCRIPTION": "Order sub-total X last year"
    },
    {
        "ENUM_ID": "PPIP_ORST_YEAR",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_ORST_YEAR",
        "SEQUENCE_ID": "12",
        "DESCRIPTION": "Order sub-total X since beginning of current year"
    },
    {
        "ENUM_ID": "PPIP_PARTY_CLASS",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_PARTY_CLASS",
        "SEQUENCE_ID": "08",
        "DESCRIPTION": "Party Classification"
    },
    {
        "ENUM_ID": "PPIP_PARTY_GRP_MEM",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_PARTY_GRP_MEM",
        "SEQUENCE_ID": "07",
        "DESCRIPTION": "Party Group Member"
    },
    {
        "ENUM_ID": "PPIP_PARTY_ID",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_PARTY_ID",
        "SEQUENCE_ID": "06",
        "DESCRIPTION": "Party"
    },
    {
        "ENUM_ID": "PPIP_PRODUCT_AMOUNT",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_PRODUCT_AMOUNT",
        "SEQUENCE_ID": "03",
        "DESCRIPTION": "X Amount of Product"
    },
    {
        "ENUM_ID": "PPIP_PRODUCT_QUANT",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_PRODUCT_QUANT",
        "SEQUENCE_ID": "04",
        "DESCRIPTION": "X Quantity of Product"
    },
    {
        "ENUM_ID": "PPIP_PRODUCT_TOTAL",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_PRODUCT_TOTAL",
        "SEQUENCE_ID": "02",
        "DESCRIPTION": "Total Amount of Product"
    },
    {
        "ENUM_ID": "PPIP_RECURRENCE",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_RECURRENCE",
        "SEQUENCE_ID": "11",
        "DESCRIPTION": "Promotion Recurrence"
    },
    {
        "ENUM_ID": "PPIP_ROLE_TYPE",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "PPC_ROLE_TYPE",
        "SEQUENCE_ID": "09",
        "DESCRIPTION": "Role Type"
    },
    {
        "ENUM_ID": "PPIP_SERVICE",
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "ENUM_CODE": "SERVICE",
        "SEQUENCE_ID": "17",
        "DESCRIPTION": "Call Service"
    }
]

export const enumirationTypes = [
    {
        "ENUM_TYPE_ID": "PROD_PROMO_ACTION",
        "DESCRIPTION": "Product Promotion Action"
    },
    {
        "ENUM_TYPE_ID": "PROD_PROMO_COND",
        "DESCRIPTION": "Product Promotion Condition"
    },
    {
        "ENUM_TYPE_ID": "PROD_PROMO_IN_PARAM",
        "DESCRIPTION": "Product Promotion Input Parameter"
    }
]
