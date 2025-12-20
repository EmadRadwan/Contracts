import React from 'react';
import { useSelector } from 'react-redux';

// REFACTOR: Created reusable <Can> component to centralize role-based visibility checks.
// This eliminates repeated user?.roles?.includes(...) checks across components,
// making code cleaner, easier to maintain, and less error-prone as the app grows.

interface CanProps {
    /** Role(s) required to show the children. Can be a single string or array. */
    perform: string | string[];

    /** Content to show if user has the required role(s) */
    children: React.ReactNode;

    /** Optional: What to show if user lacks permission (default: nothing) */
    fallback?: React.ReactNode;

    /** Optional: Extra condition (e.g., status-based) that must also be true */
    yes?: () => boolean;
}

export const Can: React.FC<CanProps> = ({
                                            perform,
                                            children,
                                            fallback = null,
                                            yes
                                        }) => {
    // Get roles from your auth state (adjust path if your selector is different)
    const roles = useSelector((state: any) => state.account.user?.roles || []);

    // Check if user has at least one of the required roles
    const hasRole = Array.isArray(perform)
        ? perform.some((role: string) => roles.includes(role))
        : roles.includes(perform);

    // Optional extra condition (useful for status checks, etc.)
    const extraCondition = yes ? yes() : true;

    if (!hasRole || !extraCondition) {
        return <>{fallback}</>;
    }

    return <>{children}</>;
};