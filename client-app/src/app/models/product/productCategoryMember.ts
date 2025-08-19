export interface ProductCategoryMember {
    productCategoryId: string;
    description: string | null;
    productId: string;
    fromDate: Date;
    thruDate?: any | null;
    comments?: any | null;
}

