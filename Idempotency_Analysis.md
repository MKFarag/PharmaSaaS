# 🛡️ PharmaSaaS — Idempotency Analysis & Engineering Guide

> **دليل ومرجع تحليل الـ Idempotency لكل Endpoints نظام PharmaSaaS.**  
> تم إعداد هذا الملف كمرجع هندسي شامل لتحديد العمليات الحساسة التي تتطلب حماية ضد التكرار (Double Submit / Retries)، وتصنيفها حسب درجة الخطورة والتأثير المالي والمخزني.

---

## 1. ما هي الـ Idempotency ولماذا نحتاجها في PharmaSaaS؟

الـ **Idempotency** تعني أن تنفيذ نفس الطلب (HTTP Request) عدة مرات متتالية يؤدي إلى نفس النتيجة في النظام دون إحداث آثار جانبية إضافية (Side Effects) غير مقصودة.

### سيناريوهات حقيقية لحدوث التكرار:
1. **Double Click من الكاشير أو الصيدلي:** ضغط زر "حفظ الفاتورة" أو "تسجيل الدفعة" مرتين بسرعة بسبب بطء استجابة الشاشة.
2. **Network Drops & Auto-Retries:** انقطاع الشبكة بعد معالجة الطلب في السيرفر وقبل وصول الرد للـ Client، فيقوم المتصفح أو تطبيق الموبايل بإعادة إرسال نفس الطلب.
3. **Queue / Message Reprocessing:** إعادة تشغيل أوامر غير مقصودة.

---

## 2. القواعد المعمارية الأساسية للـ HTTP Methods

| HTTP Method | الطبيعة القياسية | الحاجة لـ Idempotency | السبب |
|---|:---:|:---:|---|
| **GET** | Idempotent & Safe | ❌ غير مطلوب | قراءة فقط، لا تُحدث أي تغيير في البيانات. |
| **DELETE** | Idempotent | ❌ غير مطلوب | حذف السجل مرة أو عشر مرات ينتج عنه نفس الحالة (السجل محذوف أو 404). |
| **PUT** | Idempotent غالباً | ❌ غير مطلوب* | استبدال كامل للبيانات بقيم ثابتة (`Set` وليس `Increment`). *استثناء: ما إذا كانت تطلق Side Effects كإضافة مخزون. |
| **PATCH** | يعتمد على العملية | ❌ غير مطلوب* | إذا كانت مجرد Toggle أو تغيير قيمة حقل محدد بـ Guard Logic. |
| **POST** | Non-Idempotent بطبيعته | ✅ **إلزامي التقييم** | العمليات التي تنشئ سجلات، تخصم مخزون، أو تنفذ معاملات مالية. |

---

## 3. جدول تحليل الـ Endpoints الكامل

| # | الموديول | الـ Endpoint | Method | طبيعة العملية | يحتاج Idempotency؟ | الأولوية | السبب والتأثير |
|---|---|---|:---:|---|:---:|:---:|---|
| **1** | **Auth** | `POST /auth/login` | POST | إصدار JWT | ❌ لا | — | كل استدعاء يعطي Token صالح؛ لا تغيير على بيانات النظام. |
| **2** | **Auth** | `POST /auth/refresh` | POST | تجديد Token | ❌ لا | — | تكرار الاستدعاء يعطي نفس النتيجة أو خطأ انتهاء صلاحية. |
| **3** | **Auth** | `POST /auth/revoke-refresh` | POST | إلغاء Token | ❌ لا | — | إلغاء التوكن عملية Idempotent بطبيعتها. |
| **4** | **Auth** | `POST /auth/register` | POST | إنشاء حساب موظف | ✅ نعم | 🟢 عادي | التكرار ينشئ حسابات مكررة (محمي بـ Unique Email). |
| **5** | **Auth** | `POST /auth/confirm` | POST | تفعيل البريد | ❌ لا | — | تفعيل الحساب لمرة ثانية يعطي نفس النتيجة. |
| **6** | **Auth** | `POST /auth/resend-confirm` | POST | إعادة إرسال كود | ❌ لا | — | لا آثار جانبية سوى إرسال إيميل إضافي. |
| **7** | **Auth** | `POST /auth/forget-password` | POST | طلب كود استرجاع | ❌ لا | — | توليد رمز استرجاع جديد دون تغيير في البيانات الحساسة. |
| **8** | **Auth** | `POST /auth/reset-password` | POST | إعادة تعيين كلمة السر | ❌ لا | — | تعيين كلمة السر عملية نهائية لا تضاعف بيانات. |
| **9** | **Subscription** | `POST /api/SubscriptionRequests` | POST | تقديم طلب اشتراك | ✅ نعم | 🟢 عادي | تجنب إنشاء طلبات onboarding مكررة لنفس الصيدلية. |
| **10** | **Subscription** | `PUT /api/SubscriptionRequests/{id}/status` | PUT | تغيير حالة الطلب | ❌ لا | — | PUT Idempotent وتحديث الحالة لنفس القيمة لا يكرر الإنشاء. |
| **11** | **Pharmacies** | `PUT /api/Pharmacies/{id}` | PUT | تعديل اسم الصيدلية | ❌ لا | — | تحديث بيانات ثابتة. |
| **12** | **Pharmacies** | `PUT /api/Pharmacies/toggle-status/{id}` | PUT | تفعيل/تعطيل الصيدلية | ❌ لا | — | تبديل حالة محمي بـ Guard Clause. |
| **13** | **Pharmacies** | `PUT /api/Pharmacies/{id}/subscription` | PUT | تمديد تاريخ الاشتراك | ❌ لا | — | تعيين تاريخ محدد (Idempotent). |
| **14** | **Users** | `POST /api/Users` | POST | إضافة موظف | ✅ نعم | 🟢 عادي | محمي بـ Unique Email، لكن الـ Key يمنع محاولات التكرار. |
| **15** | **Users** | `PUT /api/Users/{id}` | PUT | تعديل بيانات موظف | ❌ لا | — | تحديث بيانات ثابتة. |
| **16** | **Users** | `DELETE /api/Users/{id}` | DELETE | حذف موظف | ❌ لا | — | الحذف Idempotent. |
| **17** | **Roles** | `POST /api/Roles` | POST | إنشاء Role جديد | ✅ نعم | 🟢 عادي | تجنب إنشاء أدوار مكررة بنفس الصلاحيات. |
| **18** | **Roles** | `PUT /api/Roles/{id}` | PUT | تعديل Role وصلاحياته | ❌ لا | — | استبدال كامل للـ Permissions بالـ Claims الجديدة. |
| **19** | **Roles** | `PUT /api/Roles/{id}/toggle-status` | PUT | تفعيل/تعطيل Role | ❌ لا | — | تبديل حالة محمي. |
| **20** | **Categories** | `POST /api/Categories` | POST | إضافة تصنيف | ✅ نعم | 🟢 عادي | محمي بـ Unique Name، والـ Key يعطي Fast Return. |
| **21** | **Categories** | `PUT /api/Categories/{id}` | PUT | تعديل تصنيف | ❌ لا | — | تحديث اسم. |
| **22** | **Categories** | `DELETE /api/Categories/{id}` | DELETE | حذف تصنيف | ❌ لا | — | الحذف Idempotent. |
| **23** | **DosageForms** | `POST /api/DosageForms` | POST | إضافة شكل دوائي | ✅ نعم | 🟢 عادي | نفس منطق التصنيفات. |
| **24** | **DosageForms** | `PUT /api/DosageForms/{id}` | PUT | تعديل شكل دوائي | ❌ لا | — | تحديث اسم. |
| **25** | **DosageForms** | `DELETE /api/DosageForms/{id}` | DELETE | حذف شكل دوائي | ❌ لا | — | الحذف Idempotent. |
| **26** | **Products** | `POST /api/products` | POST | إضافة منتج | ✅ نعم | 🟢 عادي | محمي بـ Barcode Unique، والـ Key يمنع التكرار. |
| **27** | **Products** | `PUT /api/products/{id}` | PUT | تعديل منتج | ❌ لا | — | تحديث بيانات المنتج. |
| **28** | **Products** | `DELETE /api/products/{id}` | DELETE | حذف منتج | ❌ لا | — | الحذف Idempotent. |
| **29** | **Inventory** | `PUT /api/inventory/{productId}` | PUT | تعيين كمية وسعر المخزون | ❌ لا | — | العملية Set لكمية وسعر ثابت وليست إضافة تراكمية. |
| **30** | **Inventory** | `PATCH /api/inventory/{productId}/rare` | PATCH | تبديل علم النواقص | ❌ لا | — | Toggle Idempotent. |
| **31** | **Inventory** | `PATCH /api/inventory/{productId}/stock` | PATCH | تسوية المخزون | ❌ لا | — | تعيين القيمة المحددة للرصيد الفعلي. |
| **32** | **Expiry Reports** | `POST /api/ExpiryReport` | POST | تسجيل أدوية منتهية الصلاحية | ✅ **نعم** | 🔴 **حرج** | **تكرارها يخصم كمية إضافية من المخزون مرتين دون وجه حق.** |
| **33** | **Expiry Reports** | `DELETE /api/ExpiryReport/{id}` | DELETE | إلغاء التقرير واسترجاع المخزون | ❌ لا | — | الحذف Idempotent. |
| **34** | **Customers** | `POST /api/customers` | POST | إضافة عميل | ✅ نعم | 🟢 عادي | محمي بـ Unique Phone Number. |
| **35** | **Customers** | `PUT /api/customers/{id}` | PUT | تعديل بيانات عميل | ❌ لا | — | تحديث اسم ورقم. |
| **36** | **Loyalty** | `POST /api/loyalty/tiers` | POST | إضافة مستوى ولاء | ✅ نعم | 🟢 عادي | تجنب تكرار المستويات. |
| **37** | **Loyalty** | `PUT /api/loyalty/tiers/{id}` | PUT | تعديل مستوى ولاء | ❌ لا | — | تحديث العتبة المالية والاسم. |
| **38** | **Loyalty** | `DELETE /api/loyalty/tiers/{id}` | DELETE | حذف مستوى ولاء | ❌ لا | — | الحذف Idempotent. |
| **39** | **Loyalty** | `POST /api/loyalty/tiers/{tierId}/rules` | POST | إضافة قاعدة خصم للمستوى | ✅ نعم | 🟢 عادي | تجنب تكرار نفس نسبة الخصم مرتين لنفس المستوى. |
| **40** | **Loyalty** | `PUT /api/loyalty/rules/{ruleId}` | PUT | تعديل قاعدة خصم | ❌ لا | — | تحديث قاعدة الخصم. |
| **41** | **Loyalty** | `DELETE /api/loyalty/rules/{ruleId}` | DELETE | حذف قاعدة خصم | ❌ لا | — | الحذف Idempotent. |
| **42** | **Shifts** | `POST /api/shifts/open` | POST | فتح شيفت يومي | ✅ **نعم** | 🟡 **مهم** | **تجنب محاولة فتح شيفتات متزامنة في نفس اللحظة.** |
| **43** | **Shifts** | `POST /api/shifts/{id}/close` | POST | إغلاق الشيفت والتسوية | ✅ **نعم** | 🟡 **مهم** | **تجنب تكرار تسوية النقدية وحساب الفوارق المالية.** |
| **44** | **Invoices** | `POST /api/invoices` | POST | إنشاء فاتورة بيع | ✅ **نعم** | 🔴 **حرج جداً** | **أخطر عملية بالـ POS — تكرارها يخصم المخزون ويزيد المبيعات مرتين.** |
| **45** | **Invoices** | `POST /api/invoices/{id}/returns` | POST | تسجيل مرتجع فاتورة | ✅ **نعم** | 🔴 **حرج جداً** | **تكرارها يعيد المخزون مرتين ويرد أموالاً مكررة للعميل.** |
| **46** | **Expenses** | `POST /api/expenses` | POST | تسجيل مصروف نثري | ✅ **نعم** | 🟡 **مهم** | **تكرارها يخصم نفس المصروف مرتين من درج الكاشير.** |
| **47** | **Expenses** | `DELETE /api/expenses/{id}` | DELETE | حذف مصروف | ❌ لا | — | الحذف Idempotent. |
| **48** | **Suppliers** | `POST /api/suppliers` | POST | إضافة مورد | ✅ نعم | 🟢 عادي | محمي بـ Unique Name. |
| **49** | **Suppliers** | `PUT /api/suppliers/{id}` | PUT | تعديل بيانات مورد | ❌ لا | — | تحديث بيانات المورد. |
| **50** | **Suppliers** | `DELETE /api/suppliers/{id}` | DELETE | حذف مورد | ❌ لا | — | الحذف Idempotent. |
| **51** | **Suppliers** | `PATCH /api/suppliers/{id}/reset-balance` | PATCH | تصفير حساب المورد | ❌ لا | — | تصفير الرصيد لـ 0 عملية Idempotent بذاتها. |
| **52** | **Purchase Orders** | `POST /api/purchase-orders` | POST | إنشاء طلبية شراء | ✅ **نعم** | 🟡 **مهم** | **تكرارها ينشئ طلبيات مكررة ويزيد مديونية الصيدلية للمورد.** |
| **53** | **Purchase Orders** | `PUT /api/purchase-orders/{id}/receive` | PUT | استلام الطلبية وإيداعها بالمخزن | ✅ **نعم** | 🔴 **حرج** | **تكرارها يضيف كميات مكررة لمخزون الصيدلية.** |
| **54** | **Purchase Orders** | `DELETE /api/purchase-orders/{id}` | DELETE | إلغاء طلبية شراء | ❌ لا | — | الحذف Idempotent. |
| **55** | **Supplier Payments** | `POST /api/supplier-payments` | POST | تسجيل دفعة مالية للمورد | ✅ **نعم** | 🔴 **حرج جداً** | **تكرارها يسجل سداداً مالياً مكرراً ويؤثر على موازين الحسابات.** |
| **56** | **AI** | `POST /api/ai/parse-prescription` | POST | فحص وتحليل الروشتة | ❌ لا | — | تحليل الصورة لا يغير حالة قاعدة البيانات سوى تسجيل الاستهلاك. |

---

## 4. تصنيف الأولويات الهندسية لتطبيق الـ Idempotency

### 🔴 الفئة الأولى: العمليات الحرجة مالياً ومخزنياً (Critical) — تطبيق إلزامي
هذه العمليات ينتج عن تكرارها كوارث محاسبية أو عدم تطابق في المخزون الفعلي:
1. `POST /api/invoices` — إنشاء فاتورة بيع (خصم كميات من المخزون + احتساب مبيعات).
2. `POST /api/invoices/{id}/returns` — معالجة المرتجع (رد أموال + إضافة كميات للمخزون).
3. `POST /api/supplier-payments` — دفعات الموردين (سداد ديون + حركة مالية).
4. `PUT /api/purchase-orders/{id}/receive` — استلام طلبيات الشراء (إضافة المخزون).
5. `POST /api/ExpiryReport` — تسجيل الهوالك والتوالف (خصم مباشر من المخزون).

### 🟡 الفئة الثانية: العمليات التشغيلية الهامة (Operational High)
تتعلق بانتظام الورديات والطلبيات والمصروفات النثرية:
1. `POST /api/shifts/open` — فتح شيفت جديد (منع فتح أكثر من شيفت لنفس اليوم أو الكاشير).
2. `POST /api/shifts/{id}/close` — إغلاق الشيفت والمطابقة المالية.
3. `POST /api/purchase-orders` — إنشاء أمر شراء تجنباً لتكرار الالتزامات.
4. `POST /api/expenses` — تسجيل المصروفات من صندوق الكاشير.

### 🟢 الفئة الثالثة: عمليات الإنشاء العادية (Natural Guard)
عمليات إضافة Entities جديدة، والتي تحتوي بطبيعتها على شروط تحقق وفرادة (Unique Constraints مثل Email, Phone, Barcode, Name):
- `POST /api/products`
- `POST /api/customers`
- `POST /api/suppliers`
- `POST /api/Users`
- `POST /api/Roles`
- `POST /api/Categories`
- `POST /api/DosageForms`
- `POST /api/loyalty/tiers`

---

## 5. خطة التنفيذ المقترحة لاحقاً (Implementation Blueprint)

عندما تقرر البدء في التطبيق العملي، هذا هو الأسلوب الهندسي الأفضل المتوافق مع معمارية المشروع (Clean Architecture & Multi-Tenancy):

### أ) طريقة العمل (Mechanism):
1. العميل يرسل في الـ Request Header مفتاحاً فريداً: `Idempotency-Key: <UUID>`.
2. يتم فحص المفتاح مع الـ `TenantId`:
   - **إذا لم يُنفذ من قبل:** يتم حجز المفتاح مع حالة `Processing` وتنفيذ الـ Handler.
   - **عند النجاح:** يتم تخزين الـ Status Code والـ Response Body وتحديث الحالة إلى `Completed`.
   - **إذا أُعيد إرسال نفس المفتاح لاحقاً:** يتم إرجاع الـ Cached Response مباشرة دون الدخول للـ Handler أو تكرار العمليات.
   - **إذا وصل طلب بنفس المفتاح بينما الأول ما زال قيد المعالجة (In-Flight):** يتم إرجاع `409 Conflict` تجنباً للـ Race Conditions.

### ب) مكان التخزين (Storage Strategy):
- استخدام **Distributed Cache (Redis / MemoryCache)** مع فترة صلاحية مناسبة (TTL: 24 to 48 Hours).
- أو جدول قاعدة بيانات مخصص `IdempotencyRecords` لكل صيدلية يضمن الـ Persistence الكامل.
