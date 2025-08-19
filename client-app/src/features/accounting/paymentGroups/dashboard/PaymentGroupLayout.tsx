import React, { useEffect } from 'react'
import { setSelectedPaymentGroup } from '../../slice/accountingSharedUiSlice'
import { useAppDispatch } from '../../../../app/store/configureStore'
import { Outlet } from 'react-router'

const PaymentGroupLayout = () => {
  const dispatch = useAppDispatch()
      useEffect(() => {
          return () => {
              dispatch(setSelectedPaymentGroup(undefined))
          }
      }, [])
    return (
      <div>
        <Outlet />
      </div>
    )
}

export default PaymentGroupLayout