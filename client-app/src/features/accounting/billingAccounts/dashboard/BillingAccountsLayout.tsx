import React, { useEffect } from 'react'
import BillingAccountsMenu from '../menu/BillingAccountsMenu'
import { Outlet } from 'react-router'
import { useAppDispatch } from '../../../../app/store/configureStore'
import { setSelectedBillingAccount } from '../../slice/accountingSharedUiSlice'

const BillingAccountsLayout = () => {
    const dispatch = useAppDispatch()
    useEffect(() => {
        return () => {
            dispatch(setSelectedBillingAccount(undefined))
        }
    }, [dispatch])
  return (
    <div>
      <Outlet />
    </div>
  )
}

export default BillingAccountsLayout