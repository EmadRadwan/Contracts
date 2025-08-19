import React, { useEffect } from "react";
import FacilityInventoryList from "./FacilityInventoryList";
import { router } from "../../../app/router/Routes";


export default function FacilityDashboard() {
    useEffect(() => {
        setTimeout(() => router.navigate("/facilityInventories"), 0)
    }, []);
    return (
        <>
            <FacilityInventoryList/>
        </>


    )
}