import * as React from 'react';

import {Calendar, DateInput, DatePicker} from '@progress/kendo-react-dateinputs';

const CustomCalendar = (props: any) => {
    return <Calendar {...props} bottomView={'decade'} topView="decade"/>;
};

const CustomDateInput = (props: any) => {
    return <DateInput {...props} format="yyyy"/>;
};

const FormDatePickerYears = () => {
    return (
        <div className='k-form-field-wrap'>
            <DatePicker calendar={CustomCalendar} dateInput={CustomDateInput} defaultValue={new Date}/>
        </div>
    );
};

export default FormDatePickerYears;