import React, { useEffect } from 'react'
import { setSelectedFinancialAccount } from '../../slice/accountingSharedUiSlice'
import { useAppDispatch } from '../../../../app/store/configureStore'
import { Outlet } from 'react-router'

const FinancialAccountLayout = () => {
  const dispatch = useAppDispatch()
      useEffect(() => {
          return () => {
              dispatch(setSelectedFinancialAccount(undefined))
          }
      }, [dispatch])
    return (
      <div>
        <Outlet />
      </div>
    )
}

export default FinancialAccountLayout