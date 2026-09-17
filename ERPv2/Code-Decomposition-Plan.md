# Code Decomposition Plan — Smaller Functions & Components

**Compiled:** 2026-09-16
**Purpose:** a practical, incremental plan for reducing the size of backend service methods/classes
and frontend components across the codebase, in service of two goals at once — the code being
easier to reason about, and the code being unit-testable. Companion to `ERPv2-Brief.md`.

> **How to use this:** this is a working-practice document, not a scheduled project. There is no
> phase in this plan that says "refactor GeneralLedgerService." Read §1 before anything else — it's
> the guardrail that keeps this plan from turning into the kind of open-ended rewrite the backend
> architecture review already rejected.

---

## 0. Why this and why now

Raised 2026-09-16, building on the backend architecture review (2026-08-06, see
`project_backend_architecture_review` in memory) and the test-coverage / unit-testing items already
added to the Smart Care capability catalog. The stated goal: smaller functions and components on
both ends of the stack, understood as current best practice and as the precondition for real unit
testing — not as an end in itself.

---

## 1. The trap to avoid

The architecture review already reached a specific verdict worth restating here: **don't schedule
"break up GeneralLedgerService" — or any god-service — as its own project.** A dedicated
decomposition effort against a multi-thousand-line file with zero test coverage is exactly the kind
of high-regression-risk work that loses to the next client bug ticket. Every mechanism in this plan
is designed to avoid that failure mode: decomposition happens *as a side effect of work already
happening*, never as its own scheduled initiative.

---

## 2. The core mechanism — tests before extraction, always

Smaller units and unit tests are not two separate goals. Decomposition is what *makes testing
possible*; a test is what *makes decomposition safe*. Every extraction, front or back, follows the
same three steps, no exceptions:

1. **Pin current behavior.** A characterization test around the existing method/component, calling
   it as a black box, asserting what it does *today* — bugs included, if any. Not fixing behavior
   yet, just pinning it.
2. **Extract.** Pull the piece actually being touched into its own small, named unit.
3. **Test the extracted piece directly.** Now that it's isolated, write a real unit test against it
   — no live database, no mocking a dozen unrelated dependencies, no full component render just to
   test one piece of logic.

That order matters. Extracting first without a safety net is how a "simple refactor" introduces a
silent GL-balance bug that surfaces three weeks later.

---

## 3. Backend

### 3.1 Prerequisite — blocks everything else in this section

Pick a test harness and commit to it: **EF Core InMemory or SQLite in-memory, fully mocked
collaborators.** This was priority #1 in the original architecture review and is still open. Nothing
in this plan produces a real unit test without it — today's `BusinessTest` project hits a live MySQL
database with hardcoded credentials and passes `null` for several constructor dependencies, which is
not a foundation to build on.

### 3.2 Sizing heuristic

So "smaller" isn't vague:
- A service **method** should be readable in one screen — rough target, under ~40–50 lines.
- A service **class** should have one cohesive responsibility and live well under 300–400 lines.

The five files furthest from that today (verified 2026-08-06):

| File | Lines |
|---|---|
| `Application/Accounting/Services/GeneralLedgerService.cs` | 6,481 |
| `Application/Manufacturing/ProductionRunService.cs` | 4,398 |
| `Application/Accounting/Services/AcctgReportsService.cs` | 3,337 |
| `Application/Accounting/Services/InvoiceHelperService.cs` | 2,282 |
| `Application/Order/Orders/OrderHelperService.cs` | 2,190 |

### 3.3 The rule, made concrete and enforceable

*Any ticket that requires editing a method inside one of these files must, as part of that ticket,
extract the specific method(s) touched into a small, named, independently-testable unit with a test
— before the ticket is called done.* Not the whole file. Not a scheduled sweep. Just the piece
already being touched.

The 8 files using `Lazy<T>` injection to route around circular dependencies get the identical
treatment: fix the one being touched when it's touched, don't schedule a dependency-graph cleanup
project.

### 3.4 Where to look first

**Pure calculation logic with no DB dependency** is the cheapest to extract and the highest-value to
test — debit/credit balancing, cost roll-ups, tax computation. A bug there is a wrong number on a
financial statement, not a cosmetic issue, which makes it the highest-leverage place to start.

---

## 4. Frontend

### 4.1 Sizing heuristic

A component is a decomposition candidate if it mixes data-fetching, business logic, and rendering in
one file, or carries several `useState`/`useEffect` pairs doing unrelated things. Target split:

- A **hook** — state + logic, testable with React Testing Library / a unit test runner, no rendering
  required.
- A **presentation component** — pure rendering, easy to snapshot-test.

### 4.2 Piggyback on migrations already in motion

This isn't a new workstream — it rides on two migrations CLAUDE.md and the UI/UX modernization plan
already commit to, feature-by-feature, as touched:

- **MobX → Redux.** The moment a screen's store is migrated is also the moment to split its
  component into hook + presentation. Same ticket, same touch point.
- **`agent.ts` → RTK Query.** RTK Query's data-fetching hooks naturally separate from rendering, so
  moving a screen off `agent.ts` is the same moment to decompose it.
- **Track D's shared components** (Menu-Exit, Actions-Menu, Status Ribbon) are themselves
  decomposition — promoting a pattern duplicated across screens into one tested component. Fold this
  explicitly into this plan rather than treating it as a separate track.

---

## 5. Sequencing

1. **Now** — commit to the backend test harness (EF InMemory/SQLite) and a frontend one (React
   Testing Library, paired with whatever runner fits the build tooling). This is the one piece of
   infrastructure that has to exist before anything below counts as "done."
2. **Prove the pattern on 2–3 concrete pilots**, chosen from work already planned — not a special
   refactor sprint. A GL posting method and one MobX store are good first picks: small enough to
   finish, real enough to build confidence in the pattern.
3. **Enforce the trigger rule going forward** — touching a god-service or an unmigrated
   MobX/`agent.ts` screen means extract-and-test as part of that ticket. A habit, not a project with
   its own budget.
4. **Later, once real coverage exists** — consider a lint/CI gate on new-code file and method size,
   so the god-services don't grow new siblings while the old ones shrink.

---

## 6. Explicitly rejected

- A scheduled "decomposition sprint" or dedicated refactor project against any specific god-service.
- Extracting without a characterization test first, even when the extraction looks trivial.
- Treating this as separate from the MobX→Redux / `agent.ts`→RTK Query migrations — it rides on them,
  it doesn't compete with them for time.
