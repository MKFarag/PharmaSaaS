# 💊 PharmaSaaS — System Analysis

> **Single source of truth** for all modules, API endpoints, actors, inputs/outputs, and role requirements.
> All modules below are **fully implemented**. This document reflects the actual running codebase.
>
> | Column | Meaning |
> |---|---|
> | **Name** | Endpoint / operation name |
> | **Actor** | Who initiates the action |
> | **Path** | HTTP Method + Route |
> | **Input** | Request body / query params |
> | **Output** | Response body / status code |
> | **Roles / Auth** | Authorization requirement |

---

## Module Index

| Module | Endpoints | Auth |
|---|---|---|
| [Auth](#auth) | 8 | Public (IP rate-limited) |
| [Profile](#profile) | 5 | `[Authorize]` + permission |
| [Subscription Requests](#subscription-requests) | 4 | Public submit · `[SystemOwner]` manage |
| [Subscription Payments](#subscription-payments) | 3 | Public upload · `[SystemOwner]` manage |
| [Pharmacies](#pharmacies) | 5 | `[SystemOwner]` |
| [Users / Employees](#users--employees) | 5 | `[Authorize]` + permission |
| [Roles](#roles) | 6 | `[Authorize]` + permission |
| [Categories](#categories) | 5 | `[Authorize]` + permission |
| [Dosage Forms](#dosage-forms) | 5 | `[Authorize]` + permission |
| [Products](#products) | 6 | `[Authorize]` + permission |
| [Inventory](#inventory) | 5 | `[Authorize]` + permission |
| [Expiry Reports](#expiry-reports) | 3 | `[Authorize]` + permission |
| [Customers](#customers) | 4 | `[Authorize]` + permission |
| [Loyalty](#loyalty) | 7 | `[Authorize]` + permission |
| [Shifts](#shifts) | 4 | `[Authorize]` + permission |
| [Invoices](#invoices) | 5 | `[Authorize]` + permission |
| [Expenses](#expenses) | 3 | `[Authorize]` + permission |
| [Suppliers](#suppliers) | 6 | `[Authorize]` + permission |
| [Purchase Orders](#purchase-orders) | 8 | `[Authorize]` + permission |
| [Supplier Payments](#supplier-payments) | 2 | `[Authorize]` + permission |
| [Reports](#reports) | 5 | `[Authorize]` + permission |
| [AI](#ai) | 3 | `[Authorize]` + permission |

---

## Auth

> Route prefix: `/Auth` (no `/api/` prefix). All endpoints are public. Rate-limited by IP.

| Name | Path | Input | Output |
|---|---|---|---|
| Login | `POST /Auth` | `{ email, password }` | `{ token, refreshToken, role }` |
| Refresh Token | `POST /Auth/refresh` | `{ token, refreshToken }` | `{ token, refreshToken }` |
| Revoke Refresh | `POST /Auth/revoke-refresh` | `{ token, refreshToken }` | `200 OK` |
| Register | `POST /Auth/register` | `{ fullName, email, password }` | `{ id }` · email confirmation sent |
| Confirm Email | `POST /Auth/confirm` | `{ userId, token }` | `200 OK` |
| Resend Confirm | `POST /Auth/resend-confirm` | `{ email }` | `200 OK` |
| Forget Password | `POST /Auth/forget-password` | `{ email }` | `200 OK` · reset code sent by email |
| Reset Password | `POST /Auth/reset-password` | `{ email, token, newPassword }` | `200 OK` |

**Login use cases:**
1. Validate credentials against tenant `ApplicationDbContext`
2. Check `IsActive = true` on the user account
3. Check pharmacy subscription is not expired
4. Generate JWT with `PharmacyId`, `UserId`, `RoleId`, and permission claims
5. Issue refresh token, return both

**Register notes:** Creates account with `IsActive = false`. Sends confirmation email. User cannot log in until email is confirmed.

---

## Profile

> Route prefix: `/api/profile`. Requires `[Authorize]`. PharmacyId resolved from JWT.

| Name | Path | Input | Output |
|---|---|---|---|
| Get My Profile | `GET /api/profile` | — | `{ id, fullName, email, role }` |
| Update Profile | `PUT /api/profile` | `{ fullName }` | `204 No Content` |
| Change Password | `PUT /api/profile/change-password` | `{ currentPassword, newPassword }` | `204 No Content` |
| Change Email | `PUT /api/profile/change-email` | `{ newEmail, password }` | `204 No Content` · confirmation email sent to new address |
| Confirm Email Change | `POST /api/profile/confirm-change-email` | `{ userId, newEmail, token }` | `204 No Content` |

---

## Subscription Requests

> Submit is public (IP rate-limited, idempotent via `Idempotency-Key` header). Management requires `[SystemOwner]`.

| Name | Path | Input | Output |
|---|---|---|---|
| Submit Request | `POST /api/SubscriptionRequests` | `{ pharmacyName, ownerName, phoneNumber, email }` | `{ id }` |
| List Requests | `GET /api/SubscriptionRequests` | `?status` (optional filter) | `[{ id, pharmacyName, ownerName, phoneNumber, email, createdAt, status }]` |
| Get Request | `GET /api/SubscriptionRequests/{id}` | — | `{ id, pharmacyName, ownerName, phoneNumber, email, createdAt, status }` |
| Update Status | `PUT /api/SubscriptionRequests/{id}/status` | `{ status }` | `204 No Content` |

**Status values:** `Pending` → `Contacted` → `Approved` / `Rejected`

**On Approved:** A new `Pharmacy` entity is created in the shared DB with a generated `TenantId`. Status notification email sent to requester.

**Submit use cases:**
1. Validate all fields present
2. Check email not already submitted
3. Create `SubscriptionRequest` with `Status = Pending`
4. Send email notification to platform team
5. Return request id

---

## Subscription Payments

> Upload is public (`[NoTenantRequired]`, IP rate-limited). Management requires `[SystemOwner]`.

| Name | Path | Input | Output |
|---|---|---|---|
| Upload Payment Proof | `POST /api/subscription-payments/{pharmacyId}` | `multipart/form-data` image (JPG/PNG) | `201 Created` |
| List Pending Proofs | `GET /api/subscription-payments` | — | `[{ id, pharmacyId, uploadedAt, imageUrl }]` |
| Verify Payment | `PUT /api/subscription-payments/{id}/verify` | `{ extendByMonths, amount }` | `204 No Content` |

**Verify use cases:**
1. Validate payment exists and belongs to a known pharmacy
2. Check payment is not already verified (`409` if so)
3. Record verified amount
4. Extend `Pharmacy.SubscriptionEnd` by `extendByMonths`
5. Return `204`

---

## Pharmacies

> All endpoints protected by `[SystemOwner]`.

| Name | Path | Input | Output |
|---|---|---|---|
| List Pharmacies | `GET /api/Pharmacies` | — | `[{ id, tenantId, name, subscriptionStart, subscriptionEnd, isActive }]` |
| Get Pharmacy | `GET /api/Pharmacies/{id}` | — | `{ id, tenantId, name, subscriptionStart, subscriptionEnd, isActive }` |
| Update Pharmacy | `PUT /api/Pharmacies/{id}` | `{ name }` | `204 No Content` |
| Toggle Status | `PUT /api/Pharmacies/toggle-status/{id}` | — | `204 No Content` |
| Set Subscription | `PUT /api/Pharmacies/{id}/subscription` | `{ end (date) }` | `204 No Content` |

**Set Subscription:** `end` must be a future date. Sets `SubscriptionEnd` directly (for admin overrides). Normal renewal goes through Subscription Payments.

---

## Users / Employees

> Route prefix: `/api/Users`. Tenant-scoped. PharmacyId from JWT.
> **No delete endpoint** — users are disabled via `toggle-status` instead (soft disable).

| Name | Path | Input | Output |
|---|---|---|---|
| List Users | `GET /api/Users` | — | `[{ id, fullName, email, role, isActive }]` |
| Get User | `GET /api/Users/{id}` | — | `{ id, fullName, email, role, isActive }` |
| Add User | `POST /api/Users` | `{ fullName, email, password, roleId }` | `{ id, fullName, email, role }` |
| Update User | `PUT /api/Users/{id}` | `{ fullName, roleId }` | `204 No Content` |
| Toggle Status | `PUT /api/Users/{id}/toggle-status` | — | `204 No Content` |

**Add User:** Idempotent (requires `Idempotency-Key` header). Email must be unique within the pharmacy. Account is active by default (no email confirmation required when created by admin).

---

## Roles

> Route prefix: `/api/Roles`. Tenant-scoped. Permissions are stored as ASP.NET Identity role claims.

| Name | Path | Input | Output |
|---|---|---|---|
| List Roles | `GET /api/Roles` | — | `[{ id, name, isActive, permissionsCount }]` |
| List All Permissions | `GET /api/Roles/permissions` | — | `[{ value, displayName }]` |
| Get Role | `GET /api/Roles/{id}` | — | `{ id, name, isActive, permissions: [] }` |
| Add Role | `POST /api/Roles` | `{ name, permissions: [] }` | `{ id, name, isActive, permissions }` |
| Update Role | `PUT /api/Roles/{id}` | `{ name, permissions: [] }` | `204 No Content` |
| Toggle Status | `PUT /api/Roles/toggle-status/{id}` | — | `204 No Content` |

**Add Role:** Idempotent. Name must be unique within the pharmacy. Returns `409` on duplicate name.

---

## Categories

> Route prefix: `/api/Categories`. Tenant-scoped. Results cached via HybridCache.

| Name | Path | Input | Output |
|---|---|---|---|
| List Categories | `GET /api/Categories` | — | `[{ id, name }]` |
| Get Category | `GET /api/Categories/{id}` | — | `{ id, name }` |
| Add Category | `POST /api/Categories` | `{ name }` | `{ id }` |
| Update Category | `PUT /api/Categories/{id}` | `{ name }` | `204 No Content` |
| Delete Category | `DELETE /api/Categories/{id}` | — | `204 No Content` |

**Delete:** Returns `409` if any products reference this category.

---

## Dosage Forms

> Route prefix: `/api/DosageForms`. Tenant-scoped. Results cached via HybridCache.

| Name | Path | Input | Output |
|---|---|---|---|
| List Dosage Forms | `GET /api/DosageForms` | — | `[{ id, name }]` |
| Get Dosage Form | `GET /api/DosageForms/{id}` | — | `{ id, name }` |
| Add Dosage Form | `POST /api/DosageForms` | `{ name }` | `{ id }` |
| Update Dosage Form | `PUT /api/DosageForms/{id}` | `{ name }` | `204 No Content` |
| Delete Dosage Form | `DELETE /api/DosageForms/{id}` | — | `204 No Content` |

**Delete:** Returns `409` if any products reference this dosage form.

---

## Products

> Route prefix: `/api/products`. Tenant-scoped. Paginated list.

| Name | Path | Input | Output |
|---|---|---|---|
| List Products | `GET /api/products` | `?search&categoryId&dosageFormId` (paginated) | `IPaginatedList<{ id, name, barcode, concentration, category, dosageForm }>` |
| Get Product | `GET /api/products/{id}` | — | `{ id, name, barcode, concentration, maxStorageTemperature, category, dosageForm }` |
| Barcode Lookup | `GET /api/products/barcode/{barcode}` | — | `{ id, name, barcode, concentration, maxStorageTemperature, category, dosageForm }` |
| Create Product | `POST /api/products` | `{ name, barcode, concentration?, maxStorageTemperature?, categoryId, dosageFormId }` | `{ id }` |
| Update Product | `PUT /api/products/{id}` | `{ name, barcode, concentration?, maxStorageTemperature?, categoryId, dosageFormId }` | `204 No Content` |
| Delete Product | `DELETE /api/products/{id}` | — | `204 No Content` |

**Create uniqueness checks:**
- Barcode unique within the pharmacy
- `(name + concentration + dosageFormId)` composite unique within the pharmacy

**Delete:** Returns `409` if any inventory records or invoice items reference this product.

---

## Inventory

> Route prefix: `/api/Inventory`. Tenant-scoped. Paginated list with query filters.
> The old separate `/shortages`, `/stock-level`, and `/out-of-stock` sub-routes **no longer exist**.
> All filtering is done via query params on `GET /api/Inventory`.

| Name | Path | Input | Output |
|---|---|---|---|
| List Inventory | `GET /api/Inventory` | `?isRare=bool&belowTarget=bool&outOfStock=bool&searchValue&pageNumber&pageSize` | `IPaginatedList<{ product, quantity, sellingPrice, targetStock, isRare }>` |
| Get Inventory | `GET /api/Inventory/{productId}` | — | `{ product, quantity, sellingPrice, targetStock, isRare, lastUpdated }` |
| Update Stock | `PUT /api/Inventory/{productId}` | `{ quantity, sellingPrice, targetStock }` | `204 No Content` |
| Toggle Rare | `PATCH /api/Inventory/{productId}/rare` | — (no body, toggles automatically) | `204 No Content` |
| Adjust Stock | `PATCH /api/Inventory/{productId}/stock` | `{ adjustment }` (positive or negative integer) | `204 No Content` |

**Inventory filter params (all optional, combinable):**
- `isRare=true` — products manually flagged as rare/scarce
- `belowTarget=true` — products below their `targetStock`
- `outOfStock=true` — products with `quantity = 0`
- `searchValue` — search by product name

**Shortage thresholds:**
- Auto shortage: `quantity ≤ 25%` of `targetStock`
- Manual shortage: `IsRare = true` (toggled regardless of quantity)
- Both flags can be active simultaneously

---

## Expiry Reports

> Route prefix: `/api/expiry-report`. Tenant-scoped.

| Name | Path | Input | Output |
|---|---|---|---|
| List Expiry Reports | `GET /api/expiry-report` | `?year` (optional filter) | `IPaginatedList<{ id, product, expiredQuantity, notes, reportDate }>` |
| Log Expiry Report | `POST /api/expiry-report` | `{ productId, expiredQuantity, notes? }` | `204 No Content` |
| Revert Expiry Report | `DELETE /api/expiry-report/{id}` | — | `204 No Content` |

**Log use cases:**
1. Validate product belongs to pharmacy
2. Check `expiredQuantity ≤ current inventory quantity`
3. Create `ExpiryReport` record
4. Deduct `expiredQuantity` from `Inventory.Quantity`

**Revert:** Re-adds the expired quantity back to inventory and deletes the report.

---

## Customers

> Route prefix: `/api/customers`. Tenant-scoped. Paginated list.

| Name | Path | Input | Output |
|---|---|---|---|
| List Customers | `GET /api/customers` | `?search` (name or phone, paginated) | `IPaginatedList<{ id, fullName, phoneNumber, totalSpend, currentTier }>` |
| Get Customer | `GET /api/customers/{id}` | — | `{ id, fullName, phoneNumber, totalSpend, currentTier, invoiceHistory }` |
| Add Customer | `POST /api/customers` | `{ fullName, phoneNumber }` | `{ id }` |
| Update Customer | `PUT /api/customers/{id}` | `{ fullName, phoneNumber }` | `204 No Content` |

**Add use cases:**
1. Check `phoneNumber` not duplicate within pharmacy
2. Create customer with `TotalSpend = 0`, `CurrentTierId = null`

---

## Loyalty

> Route prefix: `/api/loyalty`. Tenant-scoped. Tier-based discount system.

| Name | Path | Input | Output |
|---|---|---|---|
| List Tiers | `GET /api/loyalty/tiers` | — | `[{ id, name, spendThreshold, discountRules: [] }]` |
| Create Tier | `POST /api/loyalty/tiers` | `{ name, spendThreshold }` | `{ id }` |
| Update Tier | `PUT /api/loyalty/tiers/{id}` | `{ name, spendThreshold }` | `204 No Content` |
| Delete Tier | `DELETE /api/loyalty/tiers/{id}` | — | `204 No Content` |
| Add Rule | `POST /api/loyalty/tiers/{tierId}/rules` | `{ minInvoiceAmount, discountPercent }` | `{ id }` |
| Update Rule | `PUT /api/loyalty/rules/{ruleId}` | `{ minInvoiceAmount, discountPercent }` | `204 No Content` |
| Delete Rule | `DELETE /api/loyalty/rules/{ruleId}` | — | `204 No Content` |

**Update Tier:** Re-evaluates all customers against the new `spendThreshold` and updates `CurrentTierId` for any that qualify.

**Delete Tier:** Returns `409` if any customers currently have `CurrentTierId = this tier`.

**How discounts are applied at invoice creation:**
- If `customer.CurrentTier` has rules, find the rule where `invoice.Total ≥ rule.MinInvoiceAmount`
- Apply `DiscountPercent` → `FinalAmount = Total - DiscountAmount`

---

## Shifts

> Route prefix: `/api/shifts`. One open shift allowed per pharmacy per day.

| Name | Path | Input | Output |
|---|---|---|---|
| List Shifts | `GET /api/shifts` | `?date` (optional, paginated) | `IPaginatedList<{ id, date, openedBy, status, openingBalance }>` |
| Get Shift | `GET /api/shifts/{id}` | — | `{ id, date, openedBy, status, openingBalance, totalSales, totalReturns, totalExpenses, expectedBalance, actualBalance, discrepancy }` |
| Open Shift | `POST /api/shifts/open` | `{ openingBalance }` | `{ id }` |
| Close Shift | `POST /api/shifts/{id}/close` | `{ actualBalance }` | `{ expectedBalance, discrepancy }` |

**Open use cases:**
1. Check no open shift already exists for this pharmacy today
2. Create shift with `Status = Open`

**Close use cases:**
1. Validate shift belongs to pharmacy and is `Open`
2. Calculate `ExpectedBalance = OpeningBalance + TotalSales − TotalReturns − TotalExpenses`
3. Calculate `Discrepancy = ActualBalance − ExpectedBalance`
4. Set `Status = Closed`
5. Return reconciliation result

---

## Invoices

> Route prefix: `/api/invoices`. Requires an open shift.

| Name | Path | Input | Output |
|---|---|---|---|
| List Invoices | `GET /api/invoices` | `?shiftId&customerId&date` (paginated) | `IPaginatedList<{ id, finalAmount, customer, shift, itemCount }>` |
| Get Invoice | `GET /api/invoices/{id}` | — | `{ id, items, customer, total, discountAmount, finalAmount, returns }` |
| Create Invoice | `POST /api/invoices` | `{ shiftId, customerId?, items: [{ productId, quantity, unitPrice }] }` | `{ id, total, discountAmount, finalAmount }` |
| Process Return | `POST /api/invoices/{id}/return` | `{ productId, quantity }` | `{ refundAmount }` |
| Preview Return Impact | `GET /api/invoices/{id}/return/preview` | `?productId&quantity` | `{ refundAmount, newInventoryQuantity }` |

**Create Invoice use cases:**
1. Validate shift is `Open` and belongs to pharmacy
2. Validate all `productId`s belong to pharmacy
3. Check sufficient inventory quantity for each item
4. Calculate `Total = Σ (quantity × unitPrice)`
5. If `customerId` provided → check customer's tier and discount rules
6. If `Total ≥ MinInvoiceAmount` for the tier rule → apply `DiscountPercent`
7. `FinalAmount = Total − DiscountAmount`
8. Create invoice and line items
9. Deduct quantities from inventory
10. Update `Customer.TotalSpend`
11. Re-evaluate customer tier (may upgrade)
12. Return invoice summary

**Process Return use cases:**
1. Validate invoice belongs to pharmacy
2. Validate `productId` was part of that invoice
3. Check `returnQuantity ≤ originally sold quantity`
4. Create `Return` record
5. Re-add quantity to inventory
6. Calculate `refundAmount = quantity × original unitPrice`

---

## Expenses

> Route prefix: `/api/expenses`. Tenant-scoped. Tied to shifts.

| Name | Path | Input | Output |
|---|---|---|---|
| Log Expense | `POST /api/expenses` | `{ shiftId, amount, description }` | `{ id }` |
| List by Shift | `GET /api/expenses/shift/{shiftId}` | — | `[{ id, amount, description, date }]` |
| Delete Expense | `DELETE /api/expenses/{id}` | — | `204 No Content` |

**Log use cases:**
1. Validate `shiftId` belongs to pharmacy
2. Check shift is currently `Open`
3. Validate `amount > 0`
4. Create expense record

**Delete:** Returns `409` if the linked shift is already closed.

---

## Suppliers

> Route prefix: `/api/suppliers`. Tenant-scoped. List is cached.
> **Balance semantics:** Negative balance = pharmacy owes money to supplier. Balance decrements automatically when a purchase order is created. Reset sets it to zero.

| Name | Path | Input | Output |
|---|---|---|---|
| List Suppliers | `GET /api/suppliers` | — (cached) | `[{ id, name, contactInfo, balance, lastOrderDate }]` |
| Get Supplier | `GET /api/suppliers/{id}` | — | `{ id, name, contactInfo, balance, lastOrderDate }` |
| Add Supplier | `POST /api/suppliers` | `{ name, contactInfo? }` | `{ id, name, contactInfo, balance, lastOrderDate }` |
| Update Supplier | `PUT /api/suppliers/{id}` | `{ name, contactInfo? }` | `204 No Content` |
| Delete Supplier | `DELETE /api/suppliers/{id}` | — | `204 No Content` |
| Reset Balance | `PATCH /api/suppliers/{id}/reset-balance` | — | `204 No Content` |

**Delete:** Returns `409` if any purchase orders are linked to this supplier.

---

## Purchase Orders

> Route prefix: `/api/purchase-orders`. Tenant-scoped. Paginated list.
> **Two-phase flow:** Create (pending) → Approve (receive into inventory).
> Approve supports partial receipt — each item can have a `receivedQuantity` less than ordered, and items not delivered can be flagged as `isMissing`.

| Name | Path | Input | Output |
|---|---|---|---|
| List POs | `GET /api/purchase-orders` | `?supplierId` (paginated) | `IPaginatedList<{ id, supplier, orderDate, totalAmount, status, itemCount }>` |
| Get PO | `GET /api/purchase-orders/{id}` | — | `{ id, supplier, orderDate, totalAmount, status, items }` |
| Get PO Items by Status | `GET /api/purchase-orders/{id}/items` | `?status` (`Received` / `Missing`) | `[{ product, orderedQuantity, receivedQuantity, unitCost, status }]` |
| Create PO | `POST /api/purchase-orders` | `{ supplierId, items: [{ productId, quantity, unitCost }] }` | `{ id, totalAmount }` |
| Update PO | `PUT /api/purchase-orders/{id}` | `{ supplierId, items: [] }` | `{ id, totalAmount }` |
| Approve / Receive PO | `PUT /api/purchase-orders/{id}/approve` | `{ items: [{ productId, receivedQuantity, unitCost, isMissing }] }` | `204 No Content` |
| Return PO Items | `PUT /api/purchase-orders/{id}/return` | `{ items: [{ productId, quantityToReturn, returnReason }] }` | `204 No Content` |
| Delete PO | `DELETE /api/purchase-orders/{id}` | — | `204 No Content` |

**Create PO:** Does **not** update inventory. Sets `Status = Pending`. Decrements supplier balance by `TotalAmount`.

**Update PO:** Only allowed while `Status = Pending`. Returns `409` if already received.

**Approve PO:** Idempotent (requires `Idempotency-Key`). Adds `receivedQuantity` to inventory for each item. Items marked `isMissing = true` are recorded but do not affect inventory. Sets `Status = Received`.

**Return PO Items:** Deducts returned quantities from inventory. Only allowed on received POs.

**Delete PO:** Returns `409` if `Status = Received`.

---

## Supplier Payments

> Route prefix: `/api/supplier-payments`. Tenant-scoped.
> A single payment can cover one or multiple purchase orders.

| Name | Path | Input | Output |
|---|---|---|---|
| List Payments | `GET /api/supplier-payments` | `?supplierId` (optional) | `[{ id, supplier, amount, paidAt, orderIds }]` |
| Record Payment | `POST /api/supplier-payments` | `{ supplierId, amount, orderIds[] }` | `{ id }` |

**Record Payment use cases:**
1. Validate `supplierId` belongs to pharmacy
2. Validate all `orderIds` belong to pharmacy and to that supplier
3. Validate `amount > 0`
4. Create payment record linking to all referenced orders

---

## Reports

> Route prefix: `/api/reports`. Tenant-scoped. Rate-limited (sliding window).

| Name | Path | Input | Output |
|---|---|---|---|
| Daily Report | `GET /api/reports/daily` | `?date` (optional, defaults to today) | `{ date, totalSales, totalReturns, totalExpenses, discrepancy, netRevenue, topProducts[5] }` |
| Net Profit Report | `GET /api/reports/profit` | `?from&to` | `{ totalSales, totalExpenses, totalDiscrepancies, netProfit }` |
| Shortage Report | `GET /api/reports/shortages` | — | `[{ product, quantity, targetStock, shortageType }]` |
| Expense Report | `GET /api/reports/expenses` | `?from&to` | `[{ id, amount, description, date, shiftDate }]` |
| Top Products Report | `GET /api/reports/top-products` | `?from&to&limit` (default 10) | `[{ product, totalQuantitySold, totalRevenue }]` |

**Net Profit formula:**
```
Net Profit = Total Sales − Total Expenses − |Negative Shift Discrepancies|
```
> Purchase order costs are funded by owner capital and are excluded from this calculation.

**Shortage Report — shortageType values:** `Manual` · `Auto` · `Both`
- `Auto`: `quantity ≤ 25%` of `targetStock`
- `Manual`: `IsRare = true`

**Top Products:** Ranked by `totalQuantitySold` descending. Revenue is `Σ (quantity × unitPrice at time of sale)`.

**Date validation:** If `from > to`, returns `400 Bad Request`.

---

## AI

> Route prefix: `/api/AI`. Tenant-scoped.
> **Free quota:** 30 calls/month per pharmacy shared across both AI operations (parse-prescription + suggest-medications).
> When quota is exhausted → `429` with `{ retryAfter: "<first-day-of-next-month UTC>" }`.
> Pharmacies can bring their own OpenRouter API key to bypass the quota entirely.

| Name | Path | Input | Output |
|---|---|---|---|
| Parse Prescription | `POST /api/AI/parse-prescription` | `multipart/form-data` image (JPG/JPEG/PNG, max 5 MB) | `{ detectedDrugs: ["Amoxicillin", ...] }` OR `429` |
| Suggest Medications | `POST /api/AI/suggest-medications` | `{ description }` (Arabic or English, max 500 chars) | `{ suggestions: ["Paracetamol", ...] }` OR `429` |
| Set API Key | `PUT /api/AI/api-key` | `{ apiKey }` (or `null` to clear) | `204 No Content` |

**Parse Prescription use cases:**
1. Validate image: size ≤ 5 MB, extension in `[jpg, jpeg, png]`, blocked binary signatures rejected
2. Check `AIUsageLog` for current pharmacy this month
3. If limit reached and no custom key configured → return `429`
4. If custom key configured → use it (bypass quota)
5. Send image to OpenRouter (`google/gemini-2.0-flash-001`)
6. Parse LLM response → extract drug names
7. Increment usage in `AIUsageLog`
8. Return detected names

**Suggest Medications:** Same quota logic. Accepts free-text symptom/need description in Arabic or English.

**Set API Key:** Stored encrypted on the pharmacy's `AIUsageLog` record. Send `null` to revert to platform-only mode.
