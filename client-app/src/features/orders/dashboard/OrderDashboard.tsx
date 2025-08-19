import { useEffect } from "react";
import { router } from "../../../app/router/Routes";
import OrdersList from "./order/OrdersList";


export default function OrderDashboard() {
    useEffect(() => {
        setTimeout(() => router.navigate("/orders"), 0)
    }, []);
    return (
        <>
            <OrdersList/>
        </>


    )
}


