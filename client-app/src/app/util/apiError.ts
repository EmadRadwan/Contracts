/**
 * Message to show the user for a failed RTK Query / fetch call.
 *
 * The API returns a failed Result as `{ title, status, errorCode? }` (BaseApiController →
 * BadRequest(ProblemDetails)). Several screens looked for `message` or `error` instead and fell
 * through to a generic "something went wrong", hiding refusals that name the blocking payment
 * or period. Read every shape the backend has ever used, in order, then the fallback.
 */
export const apiErrorMessage = (err: any, fallback: string): string => {
    const d = err?.data;
    if (typeof d === "string" && d.trim()) return d;
    return d?.title || d?.error || d?.message || d?.detail || err?.error || fallback;
};
