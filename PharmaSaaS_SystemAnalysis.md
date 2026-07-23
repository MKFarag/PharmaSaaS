# 💊 PharmaSaaS — System Analysis

> **Single source of truth** for all modules, API endpoints, actors, inputs/outputs, use cases, and role requirements.  
> Each module section contains a table of endpoints. Column meaning:
>
> | Column | Meaning |
> |---|---|
> | **Name** | Endpoint / operation name |
> | **Actor** | Who initiates the action |
> | **Path** | HTTP Method + Route |
> | **Input** | Request body / query params |
> | **Output** | Response body / status code |
> | **Description** | Plain-language summary |
> | **Use Cases** | Ordered step-by-step logic |
> | **Roles** | Which roles are authorized |
> | **Receiver** | System or user that consumes the response |

---

## Module Index

| Module | Description |
|---|---|
| [Auth](#auth) | Employee login, JWT issuance |
| [Subscription Requests](#subscription-requests) | Public onboarding request submission and platform review |
| [Pharmacies](#pharmacies) | Pharmacy tenant management (CRUD, status toggle, subscription dates) |
| [Employees](#employees) | Employee accounts and role assignment |
| [Products](#products) | Product catalog, barcode lookup, storage info |
| [Inventory](#inventory) | Stock levels, shortage flags, expiry reports |
| [Customers](#customers) | Customer profiles and invoice history |
| [Loyalty](#loyalty) | Tier definitions and automatic discount rules |
| [Shifts](#shifts) | Daily shift lifecycle and cash reconciliation |
| [Invoices](#invoices) | POS sales invoices, discount application, returns |
| [Expenses](#expenses) | Petty cash expenses linked to shifts |
| [Suppliers](#suppliers) | Supplier management |
| [Purchase Orders](#purchase-orders) | Procurement orders and inventory auto-update |
| [Supplier Payments](#supplier-payments) | Payment recording covering single or multiple orders |
| [Reports](#reports) | Daily sales, profit, expense, shortage, and top-selling reports |
| [AI](#ai) | Prescription image parsing via OpenRouter |
| [Lookups](#lookups) | Categories and dosage form reference data |

---

## Auth

> **Single endpoint.** The xlsx spec defines only `Login`. Register, email confirmation, password reset, and refresh tokens are implemented but deviate from this spec (see `tasks/tasks.md`).

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| Login | Any Employee | `POST /api/auth/login` | `{ email, password }` | `{ token, role }` | Authenticates an employee and returns a JWT containing PharmacyId, BranchId, and RoleId claims. |

**Use Cases — Login:**
1. Validate credentials
2. Check account is active
3. Check pharmacy subscription is not expired
4. Generate JWT with PharmacyId and RoleId claims
5. Return token

**Roles:** All Roles | **Receiver:** Employee

---

## Subscription Requests

> Accessible by the **public** (no auth) for submission. Management endpoints protected by `[SystemOwner]`.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| Submit Subscription Request | Public | `POST /api/SubscriptionRequests` | `{ pharmacyName, ownerName, phoneNumber, email }` | `{ id }` | Submits a new pharmacy onboarding request. Saved with Status=Pending. Triggers an email notification to the platform team. |
| Get Subscription Request | SystemOwner | `GET /api/SubscriptionRequests/{id}` | — | `{ id, pharmacyName, ownerName, phoneNumber, email, createdAt, status }` | Retrieves details of a specific subscription request by ID. |
| List Subscription Requests | SystemOwner | `GET /api/subscription-requests` | `?status` (optional) | `[{ id, pharmacyName, ownerName, phoneNumber, email, createdAt, status }]` | Returns all subscription requests. Can be filtered by status (Pending, Contacted, Approved, Rejected). |
| Update Request Status | SystemOwner | `PUT /api/SubscriptionRequests/{id}/status` | `{ status }` | `204 No Content` | Updates the status. On Approved → Pharmacy entity auto-created in shared DB with generated TenantId. |

**Use Cases — Submit Subscription Request:**
1. Validate all fields present
2. Check email not already submitted
3. Create SubscriptionRequest with Status=Pending
4. Send email notification
5. Return request id

**Use Cases — List Subscription Requests:**
1. Validate [SystemOwner]
2. Apply optional status filter
3. Return results ordered by CreatedAt desc

**Use Cases — Update Request Status:**
1. Validate [SystemOwner]
2. Validate id exists
3. Validate status is one of: Contacted, Approved, Rejected
4. Update status
5. If Approved → create Pharmacy entity with TenantId, send status notification email

**Status values:** `Pending` | `Contacted` | `Approved` | `Rejected`

---

## Pharmacies

> All endpoints protected by `[SystemOwner]`.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Pharmacies | SystemOwner | `GET /api/Pharmacies` | — | `[{ id, tenantId, name, subscriptionStart, subscriptionEnd, isActive }]` | Returns all registered pharmacy tenants. |
| Get Pharmacy | SystemOwner | `GET /api/Pharmacies/{id}` | — | `{ id, tenantId, name, subscriptionStart, subscriptionEnd, isActive }` | Retrieves details of a specific pharmacy. Returns 404 if not found. |
| Update Pharmacy | SystemOwner | `PUT /api/Pharmacies/{id}` | `{ name }` | `204 No Content` | Updates the name. Checks for duplicate name under same tenant. Returns 409 if name exists. |
| Toggle Pharmacy Status | SystemOwner | `PUT /api/Pharmacies/toggle-status/{id}` | — | `204 No Content` | Toggles the IsActive flag. Disabled pharmacies block write operations. |
| Set Pharmacy Subscription | SystemOwner | `PUT /api/Pharmacies/{id}/subscription` | `{ end (date) }` | `204 No Content` | Sets the subscription end date. The end date must be in the future. |

**Use Cases — Toggle Pharmacy Status:**
1. Validate [SystemOwner]
2. Validate id exists
3. Toggle IsActive
4. Return 204

**Use Cases — Set Pharmacy Subscription:**
1. Validate [SystemOwner]
2. Validate id exists
3. Validate end date is in future
4. Set SubscriptionEnd
5. Return 204

---

## Employees

> Tenant-scoped. PharmacyId extracted from JWT.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Employees | Manager / Owner | `GET /api/Users` | — (PharmacyId from JWT) | `[{ id, fullName, role }]` | Returns all employees in the pharmacy. |
| Add Employee | Owner / Manager | `POST /api/Users` | `{ fullName, email, password, roleId }` | `{ id }` | Creates a new employee account under the pharmacy. |
| Update Employee | Owner / Manager | `PUT /api/Users/{id}` | `{ fullName, roleId }` | `204 No Content` | Updates employee details or reassigns their role/branch. |
| Delete Employee | Owner | `DELETE /api/Users/{id}` | — | `204 No Content` | Removes an employee account. Soft delete. |

**Use Cases — Add Employee:**
1. Validate all fields
2. Check email not already used
3. Create ApplicationUser
4. Assign role
5. Return id

**Use Cases — Delete Employee:**
1. Validate employee belongs to pharmacy
2. Check employee has no open shifts
3. Soft delete account

**Roles:** Owner, Manager | **Receiver:** Employee / Management

---

## Products

> Tenant-scoped. PharmacyId extracted from JWT.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Products | Any Employee | `GET /api/products` | `?search&categoryId&dosageFormId` (optional) | `[{ id, name, barcode, concentration, category, dosageForm }]` | Returns all products with optional filters. |
| Get Product by Barcode | Any Employee | `GET /api/products/barcode/{barcode}` | — | `{ id, name, barcode, concentration, maxStorageTemperature, category, dosageForm }` | Looks up a product by barcode. Used by POS scanner. Returns 404 if not found. |
| Get Product | Any Employee | `GET /api/products/{id}` | — | `{ id, name, barcode, concentration, maxStorageTemperature, category, dosageForm }` | Retrieves full product details by ID. |
| Create Product | Manager / Pharmacist | `POST /api/products` | `{ name, barcode, concentration?, maxStorageTemperature?, categoryId, dosageFormId }` | `{ id }` | Creates a new product. Checks barcode uniqueness and composite (name+concentration+dosageForm) uniqueness. |
| Update Product | Manager / Pharmacist | `PUT /api/products/{id}` | `{ name, barcode, concentration?, maxStorageTemperature?, categoryId, dosageFormId }` | `204 No Content` | Updates product details. |
| Delete Product | Manager | `DELETE /api/products/{id}` | — | `204 No Content` | Deletes a product. Only if it has no inventory or invoice history. |

**Use Cases — Create Product:**
1. Validate product belongs to pharmacy
2. Check barcode not duplicate in pharmacy
3. Check (name + concentration + dosageFormId) composite not duplicate
4. Validate categoryId and dosageFormId exist
5. Create product
6. Return id

**Use Cases — Delete Product:**
1. Validate product belongs to pharmacy
2. Check no inventory records exist
3. Check no invoice items reference product
4. Delete product

**Roles:** Owner, Manager, Pharmacist (read: all) | **Receiver:** Employee / POS Screen

---

## Inventory

> Tenant-scoped. PharmacyId extracted from JWT.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Inventory | Any Employee | `GET /api/inventory` | — (PharmacyId from JWT) | `[{ product, quantity, purchasePrice, targetStock, isRare }]` | Returns current stock levels per product. `isRare` flags products rare in the market. |
| Update Stock | Pharmacist / Manager | `PUT /api/inventory/{productId}` | `{ quantity, purchasePrice, targetStock }` | `204 No Content` | Updates quantity, purchase price, and target stock for a product. |
| Toggle Rare Flag | Pharmacist / Manager | `PATCH /api/inventory/{productId}/shortage` | — (no body, toggles automatically) | `204 No Content` | Toggles the `isRare` flag. Marks product as rare/scarce regardless of stock quantity. |
| Rare Products Report | Manager / Owner | `GET /api/inventory/shortages` | — | `[{ product, quantity, targetStock, isRare }]` | Returns all products currently flagged as rare (`isRare = true`). |
| Stock Level Report | Manager / Owner | `GET /api/inventory/stock-level?belowTarget={bool}` | `belowTarget: bool` | `[{ product, quantity, targetStock, isRare }]` | Returns products filtered by stock level relative to their target. Only products with a configured `targetStock` are included. |
| Out-of-Stock Report | Manager / Owner | `GET /api/inventory/out-of-stock` | — | `[{ product, quantity, targetStock, isRare }]` | Returns all products with zero quantity (completely out of stock). |
| Log Expiry Report | Pharmacist | `POST /api/inventory/expiry-report` | `{ productId, expiredQuantity, notes? }` | `204 No Content` | Records expired stock and automatically deducts the expired quantity from inventory. |

**Use Cases — Log Expiry Report:**
1. Validate product belongs to pharmacy
2. Check expiredQuantity <= current quantity
3. Create ExpiryReport record
4. Deduct expiredQuantity from Inventory.Quantity
5. Return 204

**Use Cases — Stock Level Report:**
1. Extract PharmacyId
2. Filter: targetStock set AND (quantity < targetStock if belowTarget=true, else quantity >= targetStock)
3. Return list

**Roles:** Owner, Manager, Pharmacist (write); All Employees (read) | **Receiver:** Employee / Management / Procurement Team

---

## Customers

> Tenant-scoped. PharmacyId extracted from JWT.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Customers | Any Employee | `GET /api/customers` | `?search` (optional name or phone) | `[{ id, fullName, phone, totalSpend, currentTier }]` | Returns all customers with optional search. |
| Customer Profile | Manager / Owner | `GET /api/customers/{id}` | — | `{ id, fullName, phone, totalSpend, currentTier, invoiceHistory }` | Returns full customer profile including invoice history and loyalty tier. |
| Add Customer | Any Employee | `POST /api/customers` | `{ fullName, phoneNumber }` | `{ id }` | Registers a new customer. Creates with TotalSpend=0. |
| Update Customer | Any Employee | `PUT /api/customers/{id}` | `{ fullName, phoneNumber }` | `204 No Content` | Updates customer contact details. |

**Use Cases — Add Customer:**
1. Validate fullName and phoneNumber present
2. Check phoneNumber not duplicate in pharmacy
3. Create customer with TotalSpend=0
4. Return id

**Roles:** All Employees | **Receiver:** POS Screen / Customer

---

## Loyalty

> Tier-based discount system. Tenant-scoped.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Tiers | Manager / Owner | `GET /api/loyalty/tiers` | — | `[{ id, name, spendThreshold, discountRules }]` | Returns all loyalty tiers and their discount rules. |
| Create Tier | Manager | `POST /api/loyalty/tiers` | `{ name, spendThreshold }` | `{ id }` | Defines a new loyalty tier (e.g. Silver at 2000 EGP cumulative spend). |
| Update Tier | Manager | `PUT /api/loyalty/tiers/{id}` | `{ name, spendThreshold }` | `204 No Content` | Updates tier. Re-evaluates all customers against new threshold. |
| Delete Tier | Manager | `DELETE /api/loyalty/tiers/{id}` | — | `204 No Content` | Deletes a tier. Only allowed if no customers are currently assigned to it. |
| Add Discount Rule | Manager | `POST /api/loyalty/tiers/{tierId}/rules` | `{ minInvoiceAmount, discountPercent }` | `{ id }` | Adds a discount rule to a tier (e.g. 2% off invoices above 500 EGP for Silver members). |
| Update Discount Rule | Manager | `PUT /api/loyalty/rules/{ruleId}` | `{ minInvoiceAmount, discountPercent }` | `204 No Content` | Updates an existing discount rule. |
| Delete Discount Rule | Manager | `DELETE /api/loyalty/rules/{ruleId}` | — | `204 No Content` | Removes a discount rule from a tier. |

**Use Cases — Delete Tier:**
1. Validate tier belongs to pharmacy
2. Check no customers have CurrentTierId = this tier
3. Delete tier and its rules

**Use Cases — Add Discount Rule:**
1. Validate tier belongs to pharmacy
2. Validate minInvoiceAmount > 0 and discountPercent > 0
3. Create rule
4. Return id

**Roles:** Owner, Manager | **Receiver:** POS Screen (discount application during invoicing)

---

## Shifts

> Daily shift lifecycle. One open shift per pharmacy per day.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Shifts | Manager / Owner | `GET /api/shifts` | `?date` (optional) | `[{ id, date, employee, status, openingBalance }]` | Returns shifts with optional filtering by date. |
| Shift Detail | Manager / Owner | `GET /api/shifts/{id}` | — | `{ id, date, employee, openingBalance, expectedBalance, actualBalance, discrepancy, totalSales, totalReturns, totalExpenses }` | Returns full details and reconciliation summary of a specific shift. |
| Open Shift | Cashier / Pharmacist | `POST /api/shifts/open` | `{ openingBalance }` | `{ id }` | Opens the daily shift. Only one open shift allowed per pharmacy per day. |
| Close Shift | Manager / Owner | `POST /api/shifts/{id}/close` | `{ actualBalance }` | `{ expectedBalance, discrepancy }` | Closes the shift, calculates expected balance, and surfaces any cash discrepancy. |

**Use Cases — Open Shift:**
1. Validate PharmacyId from JWT
2. Check no open shift already exists for pharmacy today
3. Create shift with Status=Open
4. Return shift id

**Use Cases — Close Shift:**
1. Validate shift belongs to pharmacy
2. Check shift is currently Open
3. Calculate: `ExpectedBalance = OpeningBalance + TotalSales - TotalReturns - TotalExpenses`
4. Calculate: `Discrepancy = ActualBalance - ExpectedBalance`
5. Set Status=Closed
6. Return reconciliation result

**Roles:** Owner, Manager (list/close); Owner, Manager, Pharmacist, Cashier (open) | **Receiver:** Management / Cashier

---

## Invoices

> POS sales flow. Requires an open shift.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Invoices | Any Employee | `GET /api/invoices` | `?shiftId&customerId&date` (optional) | `[{ id, finalAmount, customer, shift, itemCount }]` | Returns invoices with optional filters. |
| Invoice Detail | Any Employee | `GET /api/invoices/{id}` | — | `{ id, items, customer, total, discountAmount, finalAmount, returns }` | Returns full invoice details including all items and any returns. |
| Create Invoice | Cashier / Pharmacist | `POST /api/invoices` | `{ shiftId, customerId?, items: [{ productId, quantity, unitPrice }] }` | `{ id, total, discountAmount, finalAmount }` | Creates a sales invoice, applies loyalty discount if eligible, deducts inventory, and updates customer total spend. |
| Process Return | Cashier / Pharmacist | `POST /api/invoices/{id}/returns` | `{ productId, quantity }` | `{ refundAmount }` | Processes a product return linked to an existing invoice. Re-adds quantity to inventory. |

**Use Cases — Create Invoice:**
1. Validate shift is Open and belongs to pharmacy
2. Validate all productIds belong to pharmacy
3. Check sufficient quantity in inventory for each item
4. Calculate `Total = sum of (quantity * unitPrice)`
5. If customerId provided → check customer tier and discount rules
6. If `Total >= MinInvoiceAmount` for tier → apply DiscountPercent
7. `FinalAmount = Total - DiscountAmount`
8. Create invoice and items
9. Deduct quantities from inventory
10. Update Customer.TotalSpend
11. Re-evaluate customer tier
12. Return invoice summary

**Use Cases — Process Return:**
1. Validate invoice belongs to pharmacy
2. Validate productId was part of that invoice
3. Check return quantity <= originally sold quantity
4. Create Return record
5. Re-add quantity to inventory
6. Calculate `refundAmount = quantity * original unitPrice`
7. Return refund amount

**Roles:** Owner, Manager, Pharmacist, Cashier | **Receiver:** Customer / Employee

---

## Expenses

> Petty cash expenses tied to shifts. Tenant-scoped.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Expenses | Manager / Owner | `GET /api/expenses` | `?shiftId` (optional) | `[{ id, amount, description, date }]` | Returns expenses logged against a shift. |
| Log Expense | Cashier / Pharmacist | `POST /api/expenses` | `{ shiftId, amount, description }` | `{ id }` | Records a petty cash expense (rent, electricity, etc.) against the active shift. |
| Delete Expense | Manager | `DELETE /api/expenses/{id}` | — | `204 No Content` | Removes an expense. Only allowed if the linked shift is still open. |

**Use Cases — Log Expense:**
1. Validate shiftId belongs to pharmacy
2. Check shift is currently Open
3. Validate amount > 0
4. Create expense record
5. Return id

**Use Cases — Delete Expense:**
1. Validate expense belongs to pharmacy
2. Check linked shift is still Open
3. Delete expense

**Roles:** Owner, Manager (list/delete); Owner, Manager, Pharmacist, Cashier (log) | **Receiver:** Pharmacy System

---

## Suppliers

> Supplier registry. Tenant-scoped.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Suppliers | Manager / Owner | `GET /api/suppliers` | — | `[{ id, name, contactInfo }]` | Returns all suppliers registered under the pharmacy. |
| Add Supplier | Manager | `POST /api/suppliers` | `{ name, contactInfo }` | `{ id }` | Registers a new supplier. Checks for name duplicates. |
| Update Supplier | Manager | `PUT /api/suppliers/{id}` | `{ name, contactInfo }` | `204 No Content` | Updates supplier info. |
| Delete Supplier | Manager | `DELETE /api/suppliers/{id}` | — | `204 No Content` | Deletes a supplier. Only if no purchase orders are linked. |

**Use Cases — Delete Supplier:**
1. Validate supplier belongs to pharmacy
2. Check no purchase orders linked
3. Delete

**Roles:** Owner, Manager | **Receiver:** Management / Procurement

---

## Purchase Orders

> Procurement flow. Links to suppliers and updates inventory on receipt.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Purchase Orders | Manager / Owner | `GET /api/purchase-orders` | `?supplierId&date` (optional) | `[{ id, supplier, orderDate, totalAmount, itemCount }]` | Returns purchase orders with optional filters. |
| Purchase Order Detail | Manager / Owner | `GET /api/purchase-orders/{id}` | — | `{ id, supplier, orderDate, totalAmount, items: [{ product, quantity, unitCost }] }` | Returns full details of a purchase order including all items. |
| Create Purchase Order | Manager | `POST /api/purchase-orders` | `{ supplierId, items: [{ productId, quantity, unitCost }] }` | `{ id, totalAmount }` | Creates a new purchase order. Validates all productIds. Does NOT immediately update inventory. |
| Receive Purchase Order | Manager / Pharmacist | `PUT /api/purchase-orders/{id}/receive` | — | `204 No Content` | Marks the PO as received and automatically adds quantities to inventory. |
| Delete Purchase Order | Manager | `DELETE /api/purchase-orders/{id}` | — | `204 No Content` | Deletes a PO. Only if not yet received. |

**Use Cases — Create Purchase Order:**
1. Validate supplierId belongs to pharmacy
2. Validate all productIds belong to pharmacy
3. Calculate `TotalAmount = sum(quantity * unitCost)`
4. Create PO with Status=Pending
5. Return id and totalAmount

**Use Cases — Receive Purchase Order:**
1. Validate PO belongs to pharmacy
2. Check PO status is Pending
3. For each item: add quantity to Inventory and update purchasePrice
4. Set PO Status=Received
5. Return 204

**Roles:** Owner, Manager | **Receiver:** Inventory System / Management

---

## Supplier Payments

> Payment recording. Can cover a single or multiple purchase orders.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Payments | Manager / Owner | `GET /api/supplier-payments` | `?supplierId` (optional) | `[{ id, supplier, amount, paidAt, orderIds }]` | Returns payments with optional supplier filter. |
| Record Payment | Manager | `POST /api/supplier-payments` | `{ supplierId, amount, orderIds[] }` | `{ id }` | Records a payment to a supplier. Can reference one or more purchase orders. |

**Use Cases — Record Payment:**
1. Validate supplierId belongs to pharmacy
2. Validate all orderIds belong to pharmacy and to that supplier
3. Validate amount > 0
4. Create payment record linking to orders
5. Return id

**Roles:** Owner, Manager | **Receiver:** Accounting / Supplier

---

## Reports

> Analytics and reporting. All tenant-scoped via JWT PharmacyId.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| Daily Report | Manager / Owner | `GET /api/reports/daily` | `?date` (optional) | `{ date, totalSales, totalReturns, totalExpenses, discrepancy, netRevenue, topProducts }` | Full daily summary for a branch: sales, returns, expenses, shift discrepancy, and top selling products. |
| Net Profit Report | Owner | `GET /api/reports/profit` | `?from&to` | `{ totalSales, totalCOGS, totalExpenses, totalDiscrepancies, netProfit }` | Net profit for a date range. Formula: `Net Profit = Total Sales - COGS - Total Expenses - Total Shift Discrepancies`. |
| Shortage Report | Manager / Pharmacist | `GET /api/reports/shortages` | — | `[{ product, quantity, targetStock, type }]` | All products in shortage — auto (<=25% target stock) or manually flagged. |
| Expense Report | Owner / Manager | `GET /api/reports/expenses` | `?from&to` | `[{ id, amount, description, date, shift }]` | All expenses logged within the given date range. |
| Top Selling Products | Owner / Manager | `GET /api/reports/top-products` | `?from&to&limit` (default 10) | `[{ product, totalQuantitySold, totalRevenue }]` | Top-selling products by quantity sold within the date range. |

**Use Cases — Net Profit Report:**
1. Extract PharmacyId from JWT
2. Aggregate sales in date range
3. Calculate COGS from inventory purchase prices
4. Sum expenses in date range
5. Sum negative discrepancies
6. Calculate net profit
7. Return breakdown

**Use Cases — Shortage Report:**
1. Extract PharmacyId
2. Filter inventory where `isAutoShortage=true` OR `isManualShortage=true`
3. Return with shortage type indicator

**Roles:** Owner, Manager (most); Owner only (profit report); Owner, Manager, Pharmacist (shortage)

---

## AI

> Prescription parsing via OpenRouter LLM. Rate-limited per pharmacy per month.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| Parse Prescription | Pharmacist / Cashier | `POST /api/ai/parse-prescription` | `{ imageBase64 }` | `{ detectedDrugs: ["Amoxicillin", "Paracetamol"] }` OR `{ retryAfter: "2024-01-15T08:00:00" }` | Accepts a base64 image of a handwritten prescription, sends it to an LLM via OpenRouter, and returns detected drug names ready for POS search. Returns `retryAfter` if monthly limit is reached. |

**Use Cases — Parse Prescription:**
1. Validate imageBase64 present
2. Check AIUsageLogs for current pharmacy this month
3. If limit reached → return 429 with `retryAfter` timestamp
4. Send image to OpenRouter API
5. Parse LLM response → extract drug names
6. Log usage in AIUsageLogs
7. Return detected drug names

**Roles:** Owner, Manager, Pharmacist, Cashier | **Receiver:** POS Screen / Pharmacist

---

## Lookups

> Reference data: Categories and Dosage Forms. Tenant-scoped.

| Name | Actor | Path | Input | Output | Description |
|---|---|---|---|---|---|
| List Categories | Any Employee | `GET /api/Categories` | — | `[{ id, name }]` | Returns all product categories (Medicine, Cosmetic, Medical Accessory). |
| Add Category | Manager | `POST /api/Categories` | `{ name }` | `{ id }` | Adds a new product category. Checks for name duplicates. |
| List Dosage Forms | Any Employee | `GET /api/DosageForms` | — | `[{ id, name }]` | Returns all dosage forms (Tablet, Syrup, Capsule, Injection). |
| Add Dosage Form | Manager | `POST /api/DosageForms` | `{ name }` | `{ id }` | Adds a new dosage form. Checks for name duplicates. |

**Roles:** Owner, Manager (write); All Roles (read) | **Receiver:** Employee / POS Screen

---

## Implementation Status

| Module | Status |
|---|---|
| Auth | ✅ Implemented (extended beyond spec: register, confirm email, reset password, refresh tokens) |
| Subscription Requests | ✅ Implemented |
| Pharmacies | ✅ Implemented |
| Employees (Users + Roles) | ✅ Implemented |
| Products | ✅ Implemented |
| Inventory | ✅ Implemented |
| Lookups (Categories + DosageForms) | ✅ Implemented |
| Customers | ✅ Implemented |
| Loyalty | ✅ Implemented |
| Shifts | ❌ Not yet implemented |
| Invoices | ❌ Not yet implemented |
| Expenses | ❌ Not yet implemented |
| Suppliers | ❌ Not yet implemented |
| Purchase Orders | ❌ Not yet implemented |
| Supplier Payments | ❌ Not yet implemented |
| Reports | ❌ Not yet implemented |
| AI | ❌ Not yet implemented |
