# 1) Bản đồ kỹ năng ↔ JD (để bạn biết học gì trúng đích)

* **CS nền tảng** → JD1 “CS fundamentals, network, OS, DB”
* **Backend hiệu năng & scale** (HTTP, gRPC, WebSocket, concurrency, caching, SQL+NoSQL) → JD1/JD3
* **Linux, Git, Docker/K8s, Cloud** → JD1 “Linux, Docker/K8s”
* **TypeScript/NestJS** (hoặc **Go**; .NET giữ vai trò “nền tảng khái niệm”) → JD1/JD2/JD3
* **Solidity + EVM** (ERC20/721/1155, proxy/upgradeable, security) → JD4
* **Data pipeline/analytics** (ETL → Postgres/BigQuery, Metabase) → JD1 “data lake/analytics”
* **FE tối thiểu có nghề** (Next.js + wagmi/viem) → JD2/JD4 (không “todo app”)

---

# 2) Lộ trình 16 tuần (Web2 vững → Web3 chuyên sâu)

> Nhịp học gợi ý: 2h/ngày, 5–6 ngày/tuần. Mỗi tuần đều có **Core → Lab → Deliverable**.
> Ngôn ngữ BE chính bạn chọn 1: **A) TypeScript/NestJS** (phổ biến trong crypto) **hoặc** **B) Go** (hiệu năng, infra).
> .NET dùng để “đào lõi” OOP, async, memory—giúp chuyển ngôn ngữ khác mượt.

### Pha 1 — NỀN TẢNG CỐT LÕI (Tuần 1–4)

**Tuần 1: C#/.NET để hiểu bản chất**

* CLR/GC, value vs reference, async/await state machine, LINQ deferred, struct/class/record, IDisposable.
* **Lab**: 10 truy vấn LINQ khó + benchmark; viết lại bằng Go/TS để so sánh biểu đạt.
* **Deliverable**: “Core Patterns Notes” (bảng so sánh C# ↔ TS ↔ Go).

**Tuần 2: HTTP, REST đúng chuẩn, gRPC, WebSocket**

* Idempotency, status code (204/409/429…), ETag, pagination chuẩn; gRPC vs REST; WebSocket vs SSE; backpressure.
* **Lab**: API REST + gRPC cho cùng một use case; SignalR (hoặc ws) push realtime.
* **Deliverable**: Postman collection + k6 script đo RPS/latency.

**Tuần 3: CSDL sâu (SQL + NoSQL)**

* Mô hình hoá, index, plan, isolation levels, transaction; Redis (cache/patterns), Mongo (chỉ để so sánh).
* **Lab**: tối ưu 3 query (trước/sau cùng execution plan); cache aside + stampede control.
* **Deliverable**: “DB Tuning Report” có số liệu.

**Tuần 4: Linux, Git, Docker, CI**

* Quy trình request→logs→metrics→traces; Git flow, PR review; Dockerfile tối ưu layer; GitHub Actions build/test.
* **Lab**: docker-compose (api+db+redis), healthcheck; CI chạy test & lint.
* **Deliverable**: repo template sản xuất tối thiểu.

### Pha 2 — BACKEND CHUYÊN NGHIỆP (Tuần 5–8)

> Chọn **A) NestJS/TypeScript** hoặc **B) Go** (mục tiêu JD1/JD3).

**A) NestJS (khuyên nếu hướng dApp + FE Next.js)**

* Module/DI/Guard/Pipe, Validation, Swagger; Prisma + Postgres; BullMQ; OpenAPI.
* **Lab**: viết middleware/guard, policy RBAC; retry/circuit breaker (axios + Polly-like).
* **Deliverable**: “Service Template” (REST + gRPC via @grpc/grpc-js) + test (Jest).

**B) Go (khuyên nếu thích hiệu năng/hạ tầng)**

* net/http, context, goroutine/channel, worker pool, sync.Map, pprof; sqlc/gorm + Redis; grpc-go.
* **Lab**: implement rate limiter token bucket, graceful shutdown, pprof profile CPU/heap.
* **Deliverable**: microservice Go đạt 3k–10k RPS (k6 + báo cáo).

> Cả hai track tuần 8 bổ sung:
> **Observability & Resilience**

* Serilog/OpenTelemetry (TS dùng otel SDK, Go dùng otel-go), metrics (Prometheus), tracing; retry, timeout, circuit breaker.
* **Lab**: inject lỗi giả, show dashboard Grafana.
* **Deliverable**: “Resilience Playbook”.

### Pha 3 — BLOCKCHAIN (Tuần 9–12)

**Tuần 9: EVM & Solidity nền tảng**

* EOA/Contract, calldata/storage/memory, gas, events/logs; ERC20/721/1155; lỗi thường gặp (reentrancy, access control).
* **Lab**: ERC20 + vesting; test bằng **Foundry** (unit + fuzz + invariant).
* **Deliverable**: Gas report + coverage.

**Tuần 10: Proxy/Upgradeable & Security**

* UUPS/Transparent proxy, initializer; access control; pausable; timelock; role-based auth.
* **Lab**: nâng ERC20 thành upgradeable, viết script deploy & verify.
* **Deliverable**: Security checklist + POC tấn công reentrancy (test).

**Tuần 11: Tích hợp dApp**

* Next.js + wagmi/viem; WalletConnect; safe signing; subgraph cơ bản (The Graph) hoặc indexer tự viết.
* **Lab**: dashboard token transfers + WebSocket mempool (alchemy/ws hoặc self-node nếu có).
* **Deliverable**: dApp demo + README có kiến trúc.

**Tuần 12: Integrations & Data**

* Viết **indexer**: consume logs → normalize → Postgres/BigQuery; Metabase dashboards.
* **Lab**: ETL chuẩn hoá ERC20 Transfer → bảng fact.
* **Deliverable**: 3 chart KPI (holder growth, top senders, gas usage).

### Pha 4 — CHUYÊN HOÁ & HỒ SƠ (Tuần 13–16)

* **K8s căn bản** (deployment, service, HPA), secret, configmap; autoscaling.
* **Bài toán hiệu năng**: batching, async queue, idempotency key, outbox pattern.
* **Solana (Rust+Anchor)** *hoặc* **Move (Aptos/Sui)** ở mức module/lập trình tài nguyên (tuỳ thời gian).
* **Portfolio & viết case study**: mỗi project 1–2 trang giải thích trade-off, logs/metrics, test.

---

# 3) 3 dự án “đi thi việc” (chọn 2, build đến production)

1. **High-Perf Order Service** (JD1/JD3)

   * REST + gRPC; WebSocket streaming; RBAC; Redis cache; rate-limit; OpenTelemetry.
   * Benchmark k6; pprof/clinic/0x (tuỳ ngôn ngữ).
2. **DeFi Payroll/Vesting** (JD4)

   * Solidity ERC20 + Vesting + **Upgradeable**, Timelock; Foundry tests (fuzz/invariant).
   * dApp Next.js + wagmi/viem; subgraph/indexer + Metabase.
3. **Telegram Mini-app (ưu tiên JD2)**

   * Next.js mini-app + backend Nest/Go; auth + ký off-chain; hiển thị dữ liệu on-chain.

> **Tiêu chuẩn bắt buộc**: test >= 40% (BE) / >= 80% (Solidity), CI lint+test+build, Docker, README có hình kiến trúc, scripts chạy 1 lệnh.

---

# 4) Cách học “đúng” (chống bệnh todo app)

* **Thiết kế trước khi code**: sequence diagram, data flow, schema, API spec (OpenAPI), SLA/SLO.
* **Không trộn rối**: tách **controller/service/repo**, tách **domain logic** khỏi IO.
* **Error & retry có chủ đích**: timeouts, exponential backoff, idempotency key.
* **Log/Metrics/Trace**: mọi request quan trọng có correlation id; có dashboard.
* **Test kim tự tháp**: unit → integration (testcontainers) → e2e (k6/Playwright).
* **Git như người lớn**: commit message chuẩn (Conventional), PR nhỏ, code review checklist.
* **Tự debug đến cùng**: đặt giả thuyết → thu thập dữ liệu → tái hiện tối thiểu → fix → postmortem ngắn.

---

# 5) Checklists tự đánh giá (trước khi apply)

**Web/Network**

* [ ] Giải thích REST chuẩn, PUT vs PATCH, 204/304/409/429.
* [ ] Vẽ pipeline middleware & giải thích gRPC vs REST.
* [ ] WebSocket backpressure & reconnect strategy.

**DB**

* [ ] Đọc và diễn giải execution plan.
* [ ] Khi nào dùng index composite, partial; xoá N+1.

**Concurrency**

* [ ] TS: event loop, micro/macro task; Go: goroutine/channel; C#: async state machine.

**Security**

* [ ] JWT rotation, token storage an toàn, CSRF/XSS/SQLi; secure file upload.
* [ ] Smart contract: reentrancy, access control, checks-effects-interactions.

**Blockchain**

* [ ] Triển khai ERC20/721, upgradeable (UUPS/Transparent).
* [ ] Foundry test: fuzz/invariant; Gas optimization cơ bản.
* [ ] Hiểu fee/gas, storage layout, event logs.

**Ops**

* [ ] Dockerfile slim, healthcheck, readiness/liveness.
* [ ] Metrics/traces hiển thị được QPS, p95 lat, error rate.

---

# 6) Tuyến học ngôn ngữ (để “switch” dễ)

* **.NET → TS/Nest**: DI/attribute → decorator/provider; async/await giống nhau; EF Core ↔ Prisma.
* **.NET → Go**: OOP → composition; async/await → goroutine/channel; LINQ → for/map/reduce thủ công; Serilog ↔ zap/logrus.
* **TS/Nest → Go**: middleware/guard ↔ middleware/interceptor; class-validator ↔ struct tag + validator.

---

# 7) Kế hoạch 7 ngày bắt đầu ngay

* **Ngày 1–2**: Tuần 1 Core (LINQ nâng cao, async state machine) + notes so sánh C#/TS/Go.
* **Ngày 3**: Thiết kế REST spec (OpenAPI) + gRPC schema cho “Order Service”.
* **Ngày 4–5**: Dựng repo template (Nest **hoặc** Go) + Docker + CI; 3 endpoint + test.
* **Ngày 6**: Thêm Redis cache + rate limit + k6 bench đầu tiên.
* **Ngày 7**: README kiến trúc + kết quả bench + TODO tuần tới.

---

Muốn, mình có thể **tạo sẵn 2 skeleton** để bạn bắt tay vào ngay (chọn 1 hoặc lấy cả hai):

* **NestJS Skeleton**: REST + gRPC + Prisma + Redis + OpenTelemetry + k6 scripts.
* **Go Skeleton**: net/http + grpc-go + sqlc + Redis + pprof + k6 scripts.

Và **Foundry Starter** cho Solidity (ERC20 upgradeable + test fuzz/invariant).
Bạn muốn mình xuất các skeleton đó cho track nào trước: **NestJS** hay **Go**?
