import React, { useEffect } from "react";
import VehiclesList from "./VehiclesList";
import { router } from "../../../app/router/Routes";


export default function ServiceDashboard() {
    useEffect(() => {
        setTimeout(() => router.navigate("/vehicles"), 0)
    }, []);
    return (
        <>
            <VehiclesList/>
        </>


    )
}


