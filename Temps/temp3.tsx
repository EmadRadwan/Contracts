const findItemByKey = (items: any[], key: string | number): any | null => {
    for (const item of items) {
        if (item[dataItemKey] === key) {
            return item;
        }
        if (item.items?.length) {
            const found = findItemByKey(item.items, key);
            if (found) return found;
        }
    }
    return null;
};

// Then:
const selectedValue = value ? findItemByKey(data || [], value) : null;
value={selectedValue || null}