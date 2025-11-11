// REFACTOR: Store achievementPercentage as number (40), not "40%"
// Purpose: Match form + Excel logic – avoid NaN and validation errors
achievementPercentage: typeof item.achievementPercentage === 'string'
    ? parseFloat(item.achievementPercentage.replace('%', '')) || 0
    : item.achievementPercentage ?? 0,