# MEDQ – “Smart queue & medicine in stock near you will be!!”

**Nỗi đau xã hội:** Người bệnh mất thời gian chờ khám; dễ “hết thuốc/đội giá” khi đi mua. MEDQ giúp:

* Hiển thị **thời gian chờ ước tính** của phòng khám gần bạn (do nhân viên cập nhật + người dùng crowdsourced).
* Tìm **nhà thuốc còn hàng & giá tham khảo** theo toa.
* Đặt lịch khám đơn giản + nhắc lịch.
* “Hòm hồ sơ” (locker) bảo vệ kết quả khám/đơn thuốc để chia sẻ an toàn.

**Stack khuyến nghị (ổn định, ít chi phí):**

* **.NET 9 (LTS) + ASP.NET Core Minimal API** cho backend (an toàn, docs rõ ràng). ([Microsoft Learn][1])
* **EF Core + SQLite** (dev) ⇒ có thể đổi PostgreSQL/MySQL sau. ([Microsoft Learn][2])
* **Identity/JWT**, **Data Protection**, **Rate Limiting**, **In-Memory Cache**. ([Microsoft Learn][3])
* **OpenAPI** built-in .NET 9 templates *hoặc* Swashbuckle trên .NET 8 (đều OK). ([Microsoft Learn][4])
* Frontend: **Next.js** tối giản (chỉ các màn hình cần thiết).
* Triển khai local/Docker; chưa cần cloud để tiết kiệm.

---

## Roadmap 2 months (~45’/day)

Mỗi ngày làm 2 việc ngắn: **(A) đọc nhanh docs chính thức**, **(B) code bài tập nhỏ** trong repo MEDQ.

## Tuần 1 — Khởi động & Domain (C#/.NET + Minimal API)

**Mục tiêu:** chạy được API skeleton, model domain “Clinic/Pharmacy/Inventory/QueueReport/Appointment”.

* **D1:** A) Minimal API overview → tạo project. B) Health endpoint `/health`. ([Microsoft Learn][1])
* **D2:** A) DI trong .NET/ASP.NET Core. B) Tạo `IClock` + inject `ILogger<T>`. ([Microsoft Learn][5])
* **D3:** A) Options pattern. B) `AppOptions` (paging size, cache TTL). ([Microsoft Learn][6])
* **D4:** Thiết kế route chuẩn REST (resources/verbs/status codes). ([Microsoft Learn][7], [GitHub][8])
* **D5:** A) OpenAPI. B) Bật Swagger UI hoặc built-in OpenAPI (.NET 9). ([Microsoft Learn][9])
* **D6:** Viết endpoints v0: `GET /clinics`, `GET /pharmacies` (mock).
* **D7 (Ôn + check-in):** Review kiến trúc API & log format.

**Lưu ý & cảnh báo (tuần 1):**

* Chọn **.NET 8 (LTS)** nếu muốn “đi đường dài”; .NET 9 có OpenAPI built-in nhưng là STS.
* **Đừng** code “rỗng DI” (new trực tiếp trong handler); dùng container chuẩn của .NET. ([Microsoft Learn][10])
* Đặt **versioning** từ sớm (`/api/v1`). ([Microsoft Learn][7])

---

## Tuần 2 — EF Core & dữ liệu thật (seed)

**Mục tiêu:** map domain → DB, migrations, seed dữ liệu demo.

* **D8:** A) EF Core overview. B) Thêm DbContext + SQLite. ([Microsoft Learn][2])
* **D9:** A) Migrations. B) `dotnet ef migrations add Init` + `update`. ([Microsoft Learn][11])
* **D10:** Quan hệ 1-n (`Clinic`—`QueueReport`), n-n (`Pharmacy`—`Drug`). ([Microsoft Learn][12])
* **D11:** Truy vấn `Include`, paging `Skip/Take`, **AsNoTracking** cho GET. ([Microsoft Learn][13])
* **D12:** Seed dữ liệu demo (5 phòng khám, 10 nhà thuốc, 50 item).
* **D13:** Endpoints `GET /inventory?drug=` & `GET /queues?clinicId=`.
* **D14:** Viết 6–8 test xUnit cho repository/query.

**Cảnh báo:**

* **Migration** phải được review; tạo script khi cần deploy. ([Microsoft Learn][14])
* Tránh N+1 query; dùng `Include` có chủ đích.
* Chỉ bật tracking khi cần update.

---

## Tuần 3 — Xếp hàng & dự đoán đơn giản + Caching

**Mục tiêu:** mô hình “ước tính chờ” + cache đọc nhiều.

* **D15:** Thiết kế `QueueReport` (source: staff/user).
* **D16:** Thuật toán ước tính đơn giản: rolling average 1–3h.
* **D17:** **In-memory cache** `IMemoryCache` cho `GET /queues`. ([Microsoft Learn][15])
* **D18:** **Response caching** cho public GET. ([Microsoft Learn][16])
* **D19:** Viết invalidation đơn giản khi có report mới.
* **D20:** Thêm `/metrics` (minh hoạ), log timing.
* **D21 (Ôn):** A/B test thủ công: cache on/off, ghi chênh lệch.

**Cảnh báo:**

* In-memory cache **không** phù hợp nhiều instance (khi scale-out). ([Microsoft Learn][17])
* Không phụ thuộc 100% dữ liệu cache; luôn có đường đi DB. ([Microsoft Learn][15])

---

## Tuần 4 — AuthN/AuthZ, Validation, Data Protection, Rate-limit

**Mục tiêu:** bảo vệ API & dữ liệu người dùng.

* **D22:** Model validation (DataAnnotations) + trả lỗi 400 chuẩn. ([Microsoft Learn][18])
* **D23:** Authentication/Authorization (policy “Staff”, “User”). ([Microsoft Learn][19])
* **D24:** ASP.NET Core **Identity** (đăng ký/đăng nhập cơ bản). ([Microsoft Learn][3])
* **D25:** **Data Protection** cho khoá bảo mật/locker. ([Microsoft Learn][20])
* **D26:** **Rate Limiter middleware** chống spam API. ([Microsoft Learn][21])
* **D27:** Secret Manager (dev) + note Key Vault (prod). ([Microsoft Learn][22])
* **D28 (Ôn):** Viết checklist security ngắn cho repo.

**Cảnh báo:**

* **Không** log PII/kết quả khám trong logs.
* Luân phiên key Data Protection theo mặc định (90 ngày) nếu deploy dài hạn. ([Microsoft Learn][23])
* Đặt **password policy** hợp lý, bật lockout. ([Microsoft Learn][24])

---

## Tuần 5 — Tìm thuốc còn hàng & giá, UX tối thiểu

**Mục tiêu:** hoàn thiện luồng “upload toa → tìm nhà thuốc”.

* **D29:** Model `Prescription` + upload file (chỉ metadata).
* **D30:** Endpoint `POST /prescriptions` (validate mime/size).
* **D31:** `GET /pharmacies/search?drug=...&radius=...&openNow=true`.
* **D32:** Thêm **OpenAPI tags/examples** để dev frontend dễ dùng. ([Microsoft Learn][9])
* **D33:** Next.js UI v0: search thuốc, xem chi tiết nhà thuốc.
* **D34:** Caching “cache-aside” cho danh mục thuốc. ([Microsoft Learn][25])
* **D35 (Ôn):** Chạy mini usability test với người quen.

**Cảnh báo:**

* Nêu rõ **disclaimer**: giá/chứa hàng **tham khảo** (cập nhật bởi người dùng/nhà thuốc).
* Chống **over-posting** bằng DTO riêng cho input.

---

## Tuần 6 — Hẹn khám đơn giản & nhắc lịch

**Mục tiêu:** flow đặt lịch tối thiểu + email nhắc.

* **D36:** Model `Appointment` (time slot, status).
* **D37:** `POST /appointments` (check trùng giờ).
* **D38:** Hosted Service: gửi email nhắc trước 2h (dev: log). ([Microsoft Learn][26])
* **D39:** Thêm **rate limit** riêng cho `POST` chống spam. ([Microsoft Learn][21])
* **D40:** Logging phân cấp theo `CorrelationId`. ([Microsoft Learn][27])
* **D41:** Test integration (WebApplicationFactory).
* **D42 (Ôn):** Soát lại errors → chuẩn hoá response (problem+json). ([Microsoft Learn][7])

**Cảnh báo:**

* Hosted Service chỉ **best-effort**; đừng chặn request để gửi email. ([Microsoft Learn][26])
* Gửi thật thì dùng queue/dịch vụ mail, nhưng giai đoạn này mock/log.

---

## Tuần 7 — System Design & Hiệu năng

**Mục tiêu:** bền vững hơn + guideline thiết kế.

* **D43:** Azure **Well-Architected**: 5 trụ cột → tự đánh giá MEDQ. ([Microsoft Learn][28])
* **D44:** **Cloud Design Patterns** (Retry, Circuit-Breaker). ([Microsoft Learn][29])
* **D45:** Rà soát **REST design** (naming, versioning, status). ([Microsoft Learn][7], [GitHub][8])
* **D46:** Thử **Response Caching** và **In-memory cache** tối ưu. ([Microsoft Learn][16])
* **D47:** Giới thiệu **CQRS (light)** cho truy vấn nặng. ([Microsoft Learn][30])
* **D48:** Profiling N+1, no-tracking; đo lại thời gian endpoints. ([Microsoft Learn][13])
* **D49:** Mock interview #1 (API design + DB modeling).

**Cảnh báo:**

* Đừng “microservices hoá” sớm; giữ **monolith modul hoá**.
* Cache có **TTL rõ ràng**; đừng cache dữ liệu nhạy cảm.

---

## Tuần 8 — Hoàn thiện MVP & Demo

**Mục tiêu:** đóng gói bản demo có thể cho người thật dùng thử.

* **D50:** Cleanup code, bật OpenAPI đẹp, example cho mỗi endpoint. ([Microsoft Learn][9])
* **D51:** Script seed + README “How to run” (1 lệnh).
* **D52:** Chính sách quyền riêng tư & điều khoản cơ bản.
* **D53:** Test: đăng ký → up toa → tìm thuốc → đặt lịch → nhận nhắc.
* **D54:** Health Checks & readiness endpoint. ([Microsoft Learn][31])
* **D55:** Demo checklist, quay video màn hình.
* **D56:** Mock interview #2 (thuật toán collections/LINQ + system design “scale MEDQ”).

**Cảnh báo:**

* **Không** thu thập dữ liệu sức khoẻ thật khi demo; dùng **data giả**.
* Ẩn mọi **secrets** bằng User-Secrets (dev) / Key Vault (prod). ([Microsoft Learn][22])

---

## “Bridge” sang Node/Nest, React/Next & Go (mỗi Chủ nhật cuối tuần)

* Tuần 2: viết lại 1 endpoint “GET /clinics” bằng **NestJS** để so sánh DI/Decorators.
* Tuần 3: viết `GET /queues` bằng **Go + chi/gin** (so sánh handler & context).
* Tuần 5: trang Next.js “search thuốc” gọi API thật (SSR vs CSR).
* Tuần 7: thử **CQRS** đọc nặng bằng Go (chỉ query model).
  (Ý đồ: **vững web2 trước**, sau này thêm **audit trail** hoặc **supply chain** đơn thuốc bằng blockchain khi đã có người dùng.)

---

## Checklist chất lượng (rút gọn cho từng mảng)

* **API:** versioning, ProblemDetails, rate-limit, OpenAPI examples. ([Microsoft Learn][21])
* **DB/EF:** migrations có script/review; query read-only dùng **AsNoTracking**; index cơ bản. ([Microsoft Learn][14])
* **Security:** validation input, roles, Data Protection, **không log PII**; secrets tách khỏi repo. ([Microsoft Learn][18])
* **Perf:** in-memory caching (single instance) + response caching; log timing; health checks. ([Microsoft Learn][15])
* **Design:** tuân theo **Azure Well-Architected** + **REST API Best Practices**. ([Microsoft Learn][28])

---

## Những “bẫy” thường gặp & lời khuyên như người đi trước

1. **Over-engineering sớm:** Monolith sạch + module hoá trước; scale khi có tín hiệu. (Tuần 7 mới bàn micro-patterns.) ([Microsoft Learn][29])
2. **Bỏ qua log & cấu hình:** Chuẩn hoá `ILogger`, `IOptions<T>`, đặt `CorrelationId` ngay từ Tuần 1. ([Microsoft Learn][27])
3. **EF Core chậm vì tracking & N+1:** Dùng `AsNoTracking` cho GET; `Include` có mục đích; đo trước tối ưu sau. ([Microsoft Learn][13])
4. **Cache sai chỗ:** Cache dữ liệu đọc nhiều, ít đổi; có TTL; invalidate khi ghi. ([Microsoft Learn][32])
5. **Auth “cho có”:** Chính sách vai trò rõ, rate-limit theo user/IP, không log token. ([Microsoft Learn][33])
6. **Secrets lọt repo:** Dev dùng **User-Secrets**; production hãy nghĩ **Key Vault**. ([Microsoft Learn][22])
7. **Doc nghèo nàn:** OpenAPI có **examples**, README có lệnh chạy một phát. ([Microsoft Learn][9])

---

Nếu bạn muốn, mình có thể đóng gói thành **file Markdown checklist theo từng ngày** (tick từng mục là xong) để bạn in/ghi chú. Muốn làm theo template này chứ?

[1]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview?view=aspnetcore-9.0&utm_source=chatgpt.com "Minimal APIs overview"
[2]: https://learn.microsoft.com/en-us/ef/core/?utm_source=chatgpt.com "Overview of Entity Framework Core - EF Core"
[3]: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&utm_source=chatgpt.com "Introduction to Identity on ASP.NET Core"
[4]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-9.0&utm_source=chatgpt.com "Generate OpenAPI documents"
[5]: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection?utm_source=chatgpt.com "Dependency injection - .NET | Microsoft Learn"
[6]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-9.0&utm_source=chatgpt.com "Options pattern in ASP.NET Core"
[7]: https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design?utm_source=chatgpt.com "Web API Design Best Practices - Azure Architecture Center"
[8]: https://github.com/microsoft/api-guidelines?utm_source=chatgpt.com "Microsoft REST API Guidelines"
[9]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-9.0&utm_source=chatgpt.com "Overview of OpenAPI support in ASP.NET Core API apps"
[10]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-9.0&utm_source=chatgpt.com "Dependency injection in ASP.NET Core | Microsoft Learn"
[11]: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?utm_source=chatgpt.com "Migrations Overview - EF Core"
[12]: https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many?utm_source=chatgpt.com "One-to-many relationships - EF Core"
[13]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices?view=aspnetcore-9.0&utm_source=chatgpt.com "ASP.NET Core Best Practices"
[14]: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?utm_source=chatgpt.com "Applying Migrations - EF Core"
[15]: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-9.0&utm_source=chatgpt.com "Cache in-memory in ASP.NET Core"
[16]: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-9.0&utm_source=chatgpt.com "Response caching in ASP.NET Core"
[17]: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/overview?view=aspnetcore-9.0&utm_source=chatgpt.com "Overview of caching in ASP.NET Core"
[18]: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-9.0&utm_source=chatgpt.com "Model validation in ASP.NET Core MVC and Razor Pages"
[19]: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-9.0&utm_source=chatgpt.com "Overview of ASP.NET Core Authentication"
[20]: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-9.0&utm_source=chatgpt.com "ASP.NET Core Data Protection Overview"
[21]: https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-9.0&utm_source=chatgpt.com "Rate limiting middleware in ASP.NET Core"
[22]: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-9.0&utm_source=chatgpt.com "Safe storage of app secrets in development in ASP.NET Core"
[23]: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/default-settings?view=aspnetcore-9.0&utm_source=chatgpt.com "Data Protection key management and lifetime in ASP.NET ..."
[24]: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-9.0&utm_source=chatgpt.com "Configure ASP.NET Core Identity"
[25]: https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside?utm_source=chatgpt.com "Cache-Aside pattern - Azure Architecture Center"
[26]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-9.0&utm_source=chatgpt.com "Background tasks with hosted services in ASP.NET Core"
[27]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-9.0&utm_source=chatgpt.com "Logging in .NET and ASP.NET Core"
[28]: https://learn.microsoft.com/en-us/azure/well-architected/?utm_source=chatgpt.com "Azure Well-Architected Framework"
[29]: https://learn.microsoft.com/en-us/azure/architecture/patterns/?utm_source=chatgpt.com "Cloud Design Patterns - Azure Architecture Center"
[30]: https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs?utm_source=chatgpt.com "CQRS Pattern - Azure Architecture Center"
[31]: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-9.0&utm_source=chatgpt.com "Health checks in ASP.NET Core"
[32]: https://learn.microsoft.com/en-us/azure/architecture/best-practices/caching?utm_source=chatgpt.com "Caching guidance - Azure Architecture Center"
[33]: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-9.0&utm_source=chatgpt.com "Introduction to authorization in ASP.NET Core"
