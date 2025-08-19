import * as React from 'react';

export const Item = (props) => {
    const { item, itemHierarchicalIndex } = props;
    const isLeaf = !item.items || item.items.length === 0;

    // *** REFACTORED: Removed invalid onItemClick call since DropDownTree does not provide this prop; rely on built-in onChange and onExpandChange for selection and expansion handling ***
    // The click handling is managed by DropDownTree's built-in onChange and onExpandChange.
    // No explicit onClick handler needed here; DropDownTree handles clicks internally.
    return (
        <div
            className={`k-treeview-item${isLeaf ? '' : ' k-parent-node'}`}
        >
            {item.text}
        </div>
    );
};