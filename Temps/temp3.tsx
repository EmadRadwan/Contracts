transformResponse: (response: any, meta, arg) => {
    let totalCount = response.length; // Fallback to actual returned data
    try {
        const headerCount = JSON.parse(meta!.response!.headers.get("count")!);
        totalCount = headerCount.totalCount ?? response.length;
    } catch (e) {
        console.warn("Failed to parse count header", e);
    }
    return { data: response, totalCount };
},