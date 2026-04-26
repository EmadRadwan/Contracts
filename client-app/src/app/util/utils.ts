const isoDateFormat = /^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?)?$/;

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

export const parseDate = (dateStr: string | Date | null | undefined): Date | null => {
    if (!dateStr) return null;
    if (dateStr instanceof Date) return dateStr;
    const parsed = new Date(dateStr);
    return isNaN(parsed.getTime()) ? null : parsed;
};

// REFACTOR: Extracted from main component - used everywhere for consistency
export const normalizeNumeric = (value: any): number | null => {
    if (value === null || value === undefined || value === "") return null;
    const num = Number(value);
    return isNaN(num) ? null : num;
};

export const normalizeToDateString = (date: Date | string | null | undefined): string | null => {
    if (!date) return null;

    const d = new Date(date);

    // Check if date is valid
    if (isNaN(d.getTime())) return null;

    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
};

export {}