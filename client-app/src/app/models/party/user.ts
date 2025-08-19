interface Role {
    Name: string;
    PercentageAllowed: number;
}

export interface User {
    username: string;
    displayName: string;
    token: string;
    image?: string;
    dualLanguage: string
    roles?: Role[];
}

export interface UserFormValues {
    email: string;
    password: string;
    displayName?: string;
    username?: string;
}