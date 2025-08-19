export interface CustomTimePeriod {
    customTimePeriodId: string
    parentPeriodId?: string | null
    periodTypeId: string
    periodTypeDescription: string
    periodNum: number
    periodName: string
    fromDate: Date
    thruDate: Date
    isClosed: string
}