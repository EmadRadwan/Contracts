const isoDateFormat = /(\d{4}-[01]\d-[0-3]\dT[0-2]\d:[0-5]\d:[0-5]\d\.\d+)|(\d{4}-[01]\d-[0-3]\dT[0-2]\d:[0-5]\d:[0-5]\d)|(\d{4}-[01]\d-[0-3]\dT[0-2]\d:[0-5]\d)/

function isIsoDateString(value: any): boolean {
    return value && typeof value === "string" && isoDateFormat.test(value);
}

export function handleDatesArray(entities: any) {
    const ret: any[] = []
    entities.forEach((entity: any) => {
        const copy = Object.assign({}, entity);
        for (const key of Object.keys(copy)) {
            const value = copy[key];
            if (isIsoDateString(value)) {
                copy[key] = new Date(value);
            }
        }
        ret.push(copy)
    })

    return ret
}

export function handleDatesObject(entity: any) {
    const copy = Object.assign({}, entity);
    for (const key of Object.keys(copy)) {
        const value = copy[key];
        if (isIsoDateString(value)) {
            copy[key] = new Date(value);
        }
    }

    return copy
}

export const formatCurrency = (value: any, curencyCode: string = "USD") => {
    if (value == null) return "-"; // Handle null or undefined values gracefully
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: curencyCode,
    }).format(value);
  };

export const formatCurrency2 = (value: number, currencyUomId: string = 'EGP', locale: string = 'en'): string => {
    const currencyMap: Record<string, { en: string; ar: string }> = {
        EGP: { en: 'EGP', ar: 'ج.م' },
        USD: { en: '$', ar: '$' },
        EUR: { en: '€', ar: '€' },
        // Add other currencies as needed
    };

    const symbol = currencyMap[currencyUomId]?.[locale] || currencyUomId || 'EGP';
    const formattedValue = value.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',');

    return locale === 'ar' ? `${formattedValue} ${symbol}` : `${symbol} ${formattedValue}`;
};

export {}