// In certificateItemsFlat or wherever you prepare items
achievementPercentage: Number(
    String(item.achievementPercentage || 0)
        .replace("%", "")
        .trim()
) || 0,