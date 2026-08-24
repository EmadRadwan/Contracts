# CRM Lead Assignment — Implementation Spec

**Date:** 2026-08-23
**Status:** Proposed, not started
**Context:** Blocks the CRM Admin / regular CRM user role split. See
`API/Sql/Production/drop_crm_roles_from_ahmad_2026_08_23.sql` and
`CRM-Module-Test-Plan.pdf` §15.3.

---

## 1. Why

Creating leads works — single and bulk Excel import. **Assigning them to a CRM user does
not exist.** Verified: no owner/assignee field on `Party`, `LeadDto` or `LeadRecord`; no
assignment logic anywhere in `Application/CRM` or the CRM front end.

The consequence: a 500-row import lands with every lead belonging to nobody, and there is
no screen or field to change that. The only ownership in the module is on the *opportunity*
(`SalesOpportunityRole` with `OWNER`), which is set once a deal already exists — far
downstream of where the work is actually distributed.

This is also why the admin/non-admin role split cannot currently be expressed: the job that
defines a CRM Admin has no feature, so there is no permission to grant or withhold.

---

## 2. Data model — use `PartyRelationship`, no schema migration

**`PartyRelationshipType` already contains a seeded `LEAD_OWNER` row.** It is the OFBiz-native
vehicle for exactly this, it is currently unused, and `PARTY_RELATIONSHIP` is empty (0 rows).
No new table and **no EF migration is required** for the core relationship.

One active assignment is one `PartyRelationship` row:

| Column | Value |
|---|---|
| `PartyIdFrom` | the sales rep (owner) |
| `RoleTypeIdFrom` | `SALES_REP` |
| `PartyIdTo` | the lead |
| `RoleTypeIdTo` | `LEAD` |
| `PartyRelationshipTypeId` | `LEAD_OWNER` |
| `FromDate` | assignment timestamp |
| `ThruDate` | `NULL` while active; set on reassignment |
| `Comments` | optional note ("bulk assign from 23-Aug import") |

EF key is composite: `(PartyIdFrom, PartyIdTo, RoleTypeIdFrom, RoleTypeIdTo, FromDate)`
(`DataContext.cs:31526`), so re-assigning the same rep to the same lead later is a distinct
row — history works naturally.

**Reassignment** = set `ThruDate = now` on the current open row, then insert a new one. Full
assignment history for free, which the module has nowhere else.

`PartyRelationship` has FK navigations to `PartyRole`, so both parties must already hold the
relevant role. Reuse the existing `EnsurePartyRoleExists` pattern from
`CreateSalesOpportunity.cs`.

### Alternatives rejected

- **`Party.OwnerPartyId` column.** `PARTY` is the shared table behind customers, suppliers and
  employees; a CRM-only column there is wrong, and it carries no history.
- **New `LeadAssignment` entity.** Requires a migration and duplicates what `LEAD_OWNER`
  already models.

### Assignee pool — already correct

`GetPartiesSalesRepsLov` filters on the `SALES_REP` party role. Verified that the CRM user
accounts map to parties already carrying `EMPLOYEE,SALES_REP` (Gamal→344, Ayman→346,
Mark→349, Ahlam→509). **The existing rep LOV works as the assignee picker unchanged.**

---

## 3. Backend

All files follow the repo's nested-class CQRS convention (`Verb + Entity`, `Command`/`Query`
+ `Handler` in one file, `Result<T>`, `DataContext` injected directly, no repository).

### New files — `Application/CRM/Leads/Assignment/`

| File | Purpose |
|---|---|
| `AssignLead.cs` | `Command { LeadPartyId, OwnerPartyId, Comments? }`. Closes any open `LEAD_OWNER` row, opens a new one. Transactional. Idempotent — reassigning to the current owner is a no-op. |
| `BulkAssignLeads.cs` | `Command { LeadPartyIds[], OwnerPartyId }`. Same logic in a loop, one transaction, returns per-lead success/failure like `CreateLeadsBatch` does. |
| `UnassignLead.cs` | `Command { LeadPartyId }`. Closes the open row, opens nothing. Returns the lead to the unassigned pool. |
| `ListLeadAssignmentHistory.cs` | `Query { LeadPartyId }` → owner, from, thru, assigned-by, comments. Newest first. |

### Modified

**`LeadRecord.cs` / `LeadDto.cs`** — add `OwnerPartyId`, `OwnerName`, `AssignedDate`.

**`ListLeads.cs`** — project the current owner via a correlated subselect on the open
relationship row. Keep it inside the `IQueryable` so OData filtering and sorting on owner
work server-side:

```csharp
OwnerPartyId = _context.PartyRelationships
    .Where(pr => pr.PartyIdTo == p.PartyId
              && pr.PartyRelationshipTypeId == "LEAD_OWNER"
              && pr.ThruDate == null)
    .Select(pr => pr.PartyIdFrom)
    .FirstOrDefault(),
```

`ListLeadsLov.cs` — same, so the picker can show ownership.

### Controller — `LeadsController.cs`

```
POST   /api/leads/{id}/assign        AssignLead
POST   /api/leads/bulk-assign        BulkAssignLeads
DELETE /api/leads/{id}/assign        UnassignLead
GET    /api/leads/{id}/assignment-history
```

### Rule: assignment is not a side effect of creation

`CreateLead` and `CreateLeadsBatch` stay unchanged. Assignment is always explicit, so an
unassigned lead is a real, queryable state rather than an accident. The import flow gets an
assign step in the UI (§4.2) that calls `bulk-assign` after the batch save succeeds.

---

## 4. Frontend

### 4.1 Leads grid — `LeadsList.tsx`

- **Owner column**, sortable and filterable, showing rep name or an "Unassigned" chip.
- **Row action: Assign / Reassign**, opening the assign modal.
- **Checkbox selection + a bulk "Assign selected" toolbar button.** This is the piece that
  makes a 500-row import workable.
- **Quick filters:** All / Unassigned / Mine.

### 4.2 Import flow — `ImportedDataGrid.tsx`

After the batch save result dialog, offer *"Assign these N leads to…"* with the rep picker.
Calls `bulk-assign` with the newly created party ids. Skippable — leads can stay unassigned.

### 4.3 New — `AssignLeadModal.tsx`

Rep picker (`FormComboBoxVirtualPartySalesRep`, already used by the opportunity form),
optional comment, current owner shown when reassigning, and the assignment history list.

### 4.4 RTK Query — `leadsApi.ts`

`assignLead`, `bulkAssignLeads`, `unassignLead`, `fetchLeadAssignmentHistory`. All invalidate
`{ type: "Lead", id: "LIST" }` and the affected lead id.

---

## 5. RBAC

### New roles

| Role | Gates |
|---|---|
| `CRM_Leads_Assign` | Assign, reassign, unassign, bulk assign — **the CRM Admin's defining permission** |
| `CRM_Leads_ViewAll` | See all leads. Without it, a user sees only leads assigned to them |

Add both to `requiredRoles` in `SeedContracts.cs` (roleManager creates them at startup — not
a migration). Then grant to Emad and Ayman only; that is the moment the admin/non-admin split
becomes real.

### Resulting split

| | CRM Admin (Emad, Ayman) | Regular CRM user (the other 10) |
|---|---|---|
| `CRM_View` | ✅ | ✅ |
| `CRM_Leads_View/Create/Edit` | ✅ | ✅ |
| `CRM_Leads_Assign` | ✅ | ❌ |
| `CRM_Leads_ViewAll` | ✅ | ❌ — sees own leads only |
| `CRM_Leads_Delete` | ✅ | ❌ |

### Prerequisite: enforcement does not exist yet

**No granular CRM role is read anywhere in the code today** — only `CRM_View`, at the route
level (`Routes.tsx:233`). Adding roles without enforcement repeats the existing problem. So
this work must include:

1. Server-side role checks in the assignment handlers (a lead must not be reassignable by
   POSTing directly to the API).
2. `ListLeads` scoping — a user without `CRM_Leads_ViewAll` gets their own leads only. This
   must be a server-side filter, not a hidden UI column.
3. UI gating of the assign controls.

Item 2 is the security-relevant one and should not be deferred.

---

## 6. Delivery order

| # | Step | Notes |
|---|---|---|
| 1 | `AssignLead` + `UnassignLead` + endpoints | Core write path |
| 2 | Owner column in `ListLeads`/`LeadRecord` | Makes assignment visible |
| 3 | Assign modal + row action | Smallest useful slice — ship here |
| 4 | `BulkAssignLeads` + grid multi-select | Makes import usable |
| 5 | Assign step in the import flow | Closes the main workflow |
| 6 | Seed the two roles, enforce server-side, scope `ListLeads` | Makes the split real |
| 7 | Assignment history in the modal | Nice to have |
| 8 | Apply the role split to the 10 users | The original request |

Steps 1–3 are the minimum that lets a CRM Admin do the job. Step 6 is what the role decision
is waiting on.

---

## 7. Test additions

Fold into `CRM-Module-Test-Plan.pdf` as a new Suite K:

- Assign an unassigned lead; owner appears in the grid.
- Reassign; old row gets `ThruDate`, new row opens, history shows both.
- Unassign; lead returns to the Unassigned filter.
- Bulk-assign 50 leads from a selection.
- Import 100 leads, then bulk-assign from the result dialog.
- Regular CRM user sees only their own leads — **verify against the API directly**, not just
  the UI.
- Regular CRM user POSTing to `/leads/{id}/assign` is rejected.
- Reassigning to the current owner is a no-op, not a duplicate row.
- Assign to a party lacking `SALES_REP` → rejected.

---

## 8. Out of scope

Round-robin / automatic distribution; territory rules; assignment notifications; first-response
SLA; lead scoring; reassigning the opportunity `OWNER` when its lead is reassigned (deliberately
independent — a deal keeps its closer).
