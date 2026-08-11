export interface AccountClass {
    glAccountClassId: string
    glAccountClassDescription: string
}

export interface AccountType {
    glAccountTypeId: string
    glAccountTypeDescription: string
}

export interface ResourceType {
    glResourceTypeId: string
    glResourceTypeDescription: string
}

export interface ParentAccount {
    parentGlAccountId: string
    parentAccountName: string
}

export interface GlAccount {
    accountCode: string
    accountName: string
    description?: string
    externalId?: string
    glAccountClassId: string
    glAccountId: string
    glAccountTypeDescription?: string
    glAccountTypeId?: string
    glResourceTypeDescription?: string
    glResourceTypeId: string
    glXbrlClassId?: string
    glReportId?: string
    glReportDescription?: string
    glClassCourseId?: string
    glClassCourseDescription?: string
    glSubClassId?: string
    glSubClassDescription?: string
    glSubClass2Id?: string
    glSubClass2Description?: string
    glAccountCourseLabelId?: string
    glAccountCourseLabelDescription?: string
    glSubAccountCourseLabelId?: string
    glSubAccountCourseLabelDescription?: string
    parentAccountName: string
    parentGlAccountId?: string
    productId?: string
    expanded?: boolean
    children?: GlAccount[]
}