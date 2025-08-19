import * as React from 'react';

export const Item = (props: any) => {
    return (
        <React.Fragment>
            <span className={props.item.items ? 'k-state-disabled' : ''}>{props.item.text}</span>
        </React.Fragment>
    );
};