# PharmaSaaS — Task Tracker

> Auto-generated from `PharmaSaaS_SystemAnalysis.xlsx` + codebase review on 2026-07-14.
> Keep this file up to date as features are completed or deviated from the spec.

---

## ✅ Completed

### Auth
- [x] `POST /auth/login` — Login (GetToken)
- [x] `POST /auth/refresh` — Refresh token
- [x] `POST /auth/revoke-refresh` — Revoke refresh token
- [x] `POST /auth/register` — Register new employee ⚠️ *extra vs xlsx (xlsx only has Login)*
- [x] `POST /auth/confirm` — Confirm email ⚠️ *extra vs xlsx*
- [x] `POST /auth/resend-confirm` — Resend email confirmation ⚠️ *extra vs xlsx*
- [x] `POST /auth/forget-password` — Send password reset ⚠️ *extra vs xlsx*
- [x] `POST /auth/reset-password` — Reset password ⚠️ *extra vs xlsx*

### Subscription Requests
- [x] `POST /api/SubscriptionRequests` — Submit Subscription Request
- [x] `GET /api/SubscriptionRequests/{id}` — Get Subscription Request
- [x] `GET /api/SubscriptionRequests` — List Subscription Requests ⚠️ *no status filter in current impl*
- [x] `PUT /api/SubscriptionRequests/{id}/status` — Update Request Status

### Pharmacies (SystemOwner)
- [x] `GET /api/Pharmacies` — List Pharmacies
- [x] `GET /api/Pharmacies/{id}` — Get Pharmacy
- [x] `PUT /api/Pharmacies/{id}` — Update Pharmacy
- [x] `PUT /api/Pharmacies/toggle-status/{id}` — Toggle Pharmacy Status
- [x] `PUT /api/Pharmacies/{id}/subscription` — Set Pharmacy Subscription

### Employees (= Users in code)
- [x] `GET /api/Users` — List Users ✅ *xlsx updated*
- [x] `POST /api/Users` — Add User ✅ *xlsx updated*
- [x] `PUT /api/Users/{id}` — Update User ✅ *xlsx updated*
- [x] `DELETE /api/Users/{id}` → not implemented yet — see below

### Roles (extra vs xlsx — not in xlsx at all)
- [x] `GET /api/Roles` — List Roles ⚠️ *not in xlsx*
- [x] `POST /api/Roles` — Add Role ⚠️ *not in xlsx*
- [x] `PUT /api/Roles/{id}` — Update Role ⚠️ *not in xlsx*
- [x] `PUT /api/Roles/{id}/toggle-status` — Toggle Role Status ⚠️ *not in xlsx*
- [x] `PUT /api/Roles/{id}/permissions` — Set Role Permissions ⚠️ *not in xlsx*

### Profile (extra vs xlsx — not in xlsx at all)
- [x] `GET /api/Profile` — Get own profile ⚠️ *not in xlsx*
- [x] `PUT /api/Profile` — Update own profile ⚠️ *not in xlsx*
- [x] `PUT /api/Profile/change-password` — Change password ⚠️ *not in xlsx*
- [x] `PUT /api/Profile/change-email` — Change email ⚠️ *not in xlsx*

### Lookups (Categories & DosageForms)
- [x] `GET /api/Categories` — List Categories ✅ *xlsx updated*
- [x] `POST /api/Categories` — Add Category ✅ *xlsx updated*
- [x] `PUT /api/Categories/{id}` — Update Category ⚠️ *extra vs xlsx (unconfirmed)*
- [x] `DELETE /api/Categories/{id}` — Delete Category ⚠️ *extra vs xlsx (unconfirmed)*
- [x] `GET /api/DosageForms` — List DosageForms ✅ *xlsx updated*
- [x] `POST /api/DosageForms` — Add DosageForm ✅ *xlsx updated*
- [x] `PUT /api/DosageForms/{id}` — Update DosageForm ⚠️ *extra vs xlsx (unconfirmed)*
- [x] `DELETE /api/DosageForms/{id}` — Delete DosageForm ⚠️ *extra vs xlsx (unconfirmed)*

### Products
- [x] `GET /api/products` — List Products (with category/dosageForm filters)
- [x] `GET /api/products/{id}` — Get Product ⚠️ *extra vs xlsx (xlsx doesn't have Get by ID)*
- [x] `GET /api/products/barcode/{barcode}` — Lookup by Barcode
- [x] `POST /api/products` — Add Product
- [x] `PUT /api/products/{id}` — Update Product
- [x] `DELETE /api/products/{id}` — Delete Product

### Inventory
- [x] `GET /api/inventory` — List All Inventory
- [x] `GET /api/inventory/shortages` — Rare-in-market products (`IsRare` flag)
- [x] `GET /api/inventory/stock-level?belowTarget=true/false` — Products below/above target stock ⚠️ *extra vs xlsx*
- [x] `GET /api/inventory/out-of-stock` — Out-of-stock products (Quantity = 0) ⚠️ *extra vs xlsx*
- [x] `PUT /api/inventory/{productId}` — Update Stock
- [x] `PATCH /api/inventory/{productId}/rare` — Toggle IsRare flag ✅ *xlsx updated*

### Expiry Reports
- [x] `GET /api/ExpiryReport` — List Expiry Reports (paginated, filtered by year) ⚠️ *extra vs xlsx*
- [x] `POST /api/ExpiryReport` — Log Expiry Report ⚠️ *moved from Inventory*
- [x] `DELETE /api/ExpiryReport/{id}` — Revert Expiry Report and add stock back ⚠️ *extra vs xlsx*

---

## 🔲 Not Yet Started

### Customers
- [x] `GET /api/customers` — List Customers (with search)
- [x] `GET /api/customers/{id}` — Customer Profile (with invoice history)
- [x] `POST /api/customers` — Add Customer
- [x] `PUT /api/customers/{id}` — Update Customer

### Loyalty
- [x] `GET /api/loyalty/tiers` — List Tiers
- [x] `POST /api/loyalty/tiers` — Create Tier
- [x] `PUT /api/loyalty/tiers/{id}` — Update Tier
- [x] `DELETE /api/loyalty/tiers/{id}` — Delete Tier
- [x] `POST /api/loyalty/tiers/{tierId}/rules` — Add Discount Rule
- [x] `PUT /api/loyalty/rules/{ruleId}` — Update Discount Rule
- [x] `DELETE /api/loyalty/rules/{ruleId}` — Delete Discount Rule

### Shifts (POS prerequisite)
- [ ] `GET /api/shifts` — List Shifts (with date filter)
- [ ] `GET /api/shifts/{id}` — Shift Detail
- [ ] `POST /api/shifts/open` — Open Shift
- [ ] `POST /api/shifts/{id}/close` — Close Shift

### Invoices (POS)
- [ ] `GET /api/invoices` — List Invoices
- [ ] `GET /api/invoices/{id}` — Invoice Detail
- [ ] `POST /api/invoices` — Create Invoice (with loyalty discount + inventory deduction)
- [ ] `POST /api/invoices/{id}/returns` — Process Return

### Expenses
- [ ] `GET /api/expenses` — List Expenses (with shift filter)
- [ ] `POST /api/expenses` — Log Expense
- [ ] `DELETE /api/expenses/{id}` — Delete Expense

### Suppliers (Procurement)
- [ ] `GET /api/suppliers` — List Suppliers
- [ ] `POST /api/suppliers` — Add Supplier
- [ ] `PUT /api/suppliers/{id}` — Update Supplier
- [ ] `DELETE /api/suppliers/{id}` — Delete Supplier

### Purchase Orders (Procurement)
- [ ] `GET /api/purchase-orders` — List Purchase Orders
- [ ] `GET /api/purchase-orders/{id}` — Purchase Order Detail
- [ ] `POST /api/purchase-orders` — Create Purchase Order (+ auto update inventory)

### Supplier Payments
- [ ] `GET /api/supplier-payments` — List Supplier Payments
- [ ] `POST /api/supplier-payments` — Record Payment

### Reports
- [ ] `GET /api/reports/daily` — Daily Report
- [ ] `GET /api/reports/profit` — Net Profit Report
- [ ] `GET /api/reports/shortages` — Shortage Report (reports version)
- [ ] `GET /api/reports/expenses` — Expense Report
- [ ] `GET /api/reports/top-products` — Top Selling Products Report

### AI
- [ ] `POST /api/ai/parse-prescription` — Parse Prescription (OpenAI vision, usage-limited)

---

## ⚠️ Deviations from xlsx

These are features implemented but **not in the xlsx** or with **different routes/behavior**:

| Feature | xlsx Spec | Actual Implementation | Confirmed by User? |
|---|---|---|---|
| Auth Register | ❌ not in xlsx | `POST /auth/register` | ❓ |
| Confirm Email | ❌ not in xlsx | `POST /auth/confirm` | ❓ |
| Resend Confirm | ❌ not in xlsx | `POST /auth/resend-confirm` | ❓ |
| Forget Password | ❌ not in xlsx | `POST /auth/forget-password` | ❓ |
| Reset Password | ❌ not in xlsx | `POST /auth/reset-password` | ❓ |
| Roles CRUD | ❌ not in xlsx | Full CRUD + permissions | ❓ |
| Profile | ❌ not in xlsx | Get/Update/ChangePassword/ChangeEmail | ❓ |
| Categories route | `/api/lookups/categories` | `/api/Categories` | ✅ xlsx updated |
| DosageForms route | `/api/lookups/dosage-forms` | `/api/DosageForms` | ✅ xlsx updated |
| Get Product by ID | ❌ not in xlsx | `GET /api/products/{id}` | ❓ |
| Users route | `/api/employees` | `/api/Users` | ✅ xlsx updated |
| List Sub. Requests | has `?status` filter | no filter in current impl | ❓ |
| Toggle Shortage | body `{ isManualShortage: bool }` | no body, toggle via PATCH | ✅ xlsx updated |
| Adjust Stock | ❌ not in xlsx | `PATCH /api/Inventory/{productId}/stock` | ❓ |

---

## 📋 Suggested Implementation Order

Based on dependencies:

1. **Customers** — prerequisite for Invoices & Loyalty
2. **Shifts** — prerequisite for Invoices & Expenses
3. **Invoices (POS)** — core revenue feature
4. **Expenses** — depends on Shifts
5. **Loyalty** — depends on Customers + Invoices
6. **Suppliers** — standalone
7. **Purchase Orders** — depends on Suppliers
8. **Supplier Payments** — depends on Purchase Orders
9. **Reports** — depends on all above
10. **AI** — independent, can be done anytime
