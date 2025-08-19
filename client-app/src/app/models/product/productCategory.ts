export interface ProductCategory {
    productCategoryId: string;
    productCategoryTypeId?: string;
    primaryParentCategoryId: string | null;
    categoryName?: string | null;
    description: string;
    longDescription?: string | null;
    categoryImageUrl?: string | null;
    linkOneImageUrl?: string | null;
    linkTwoImageUrl?: string | null;
    detailScreen?: string | null;
    showInSelect?: string | null;
    lastUpdatedStamp?: string | null;
    createdStamp?: string | null;
    items?: any[]
    text?: string
}

