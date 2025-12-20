import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import {useAppSelector} from "../store/configureStore";

interface RequireRoleProps {
    allowedRoles: string | string[];  // Role(s) required to access this route
    fallbackPath?: string;            // Where to redirect if not allowed (default: home or not-found)
}

export default function RequireRole({
                                        allowedRoles,
                                        fallbackPath = "/not-found"
                                    }: RequireRoleProps) {
    const { user } = useAppSelector(state => state.account);

    if (!user) {
        // Not logged in → send to login
        return <Navigate to="/login" replace />;
    }

    const roles = user.roles || [];

    const hasAccess = Array.isArray(allowedRoles)
        ? allowedRoles.some(role => roles.includes(role))
        : roles.includes(allowedRoles);

    // REFACTOR: Added role-based route protection using RequireRole component.
    // This ensures users can only access routes/modules they have permission for,
    // complementing the <Can> visibility in the header. Prevents direct URL access.
    if (!hasAccess) {
        return <Navigate to={fallbackPath} replace />;
    }

    return <Outlet />;  // Render child routes
}