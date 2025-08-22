Bạn là mentor kỹ sư backend senior kiêm tech interviewer. Nhiệm vụ: thiết kế lộ trình 8 tuần (mỗi ngày 45 phút) giúp tôi nắm CHẮC CÁC LÕI C#/.NET và backend web2, đủ nền để chuyển sang web3/blockchain sau này. Hãy làm theo yêu cầu chi tiết dưới đây.

# Bối cảnh & mục tiêu

- Ưu tiên nền tảng C#/.NET (Core + Framework) và ASP.NET Core để hiểu bản chất, không “học vẹt”.
- Frontend chỉ ở mức tối thiểu, tập trung vào backend (logic, kiến trúc, hiệu năng, bảo mật).
- Sử dụng nguồn chính thống, ưu tiên tài liệu mới nhất từ Microsoft (.NET docs, ASP.NET Core, EF Core).
- Tôi muốn "vừa học vừa làm": mỗi tuần phải có mini-project/feature thật, không phải todo list đơn giản.
- Định hướng dài hạn: trở thành backend dev vững web2 và có thể mở rộng sang blockchain dev (Solidity/NestJS/Go/Rust).

# Chuẩn đầu ra bắt buộc

1) BẢNG LỘ TRÌNH CHI TIẾT: 8 tuần × 7 ngày × 45 phút/ngày.
   - Mỗi ngày gồm: (a) Mục tiêu học, (b) Tài liệu chính thống (link), (c) Bài tập code, (d) Câu hỏi tự kiểm, (e) Checklist chất lượng.
2) DỰ ÁN XUYÊN SUỐT (capstone): backend ASP.NET Core chuẩn production mini (RESTful), có:
   - Phân lớp sạch (Domain/Application/Infrastructure), DI, DTO/Mapping, Validation (FluentValidation), EF Core + Migrations, Repository (nếu hợp lý), Logging (Serilog), Caching (Redis), Health checks, OpenAPI/Swagger, Dockerfile, CI bước build/test cơ bản, Git flow chuẩn.
   - Test: xUnit (unit), Integration test cơ bản, hướng dẫn debug.
   - Bảo mật: authn/authz đúng, JWT lưu an toàn (không localStorage phía FE), rate limit, input sanitization, error handling/global exception.
   - REST đúng chuẩn: tài nguyên, idempotency, status codes (phân biệt PUT/PATCH, 204 No Content…), pagination, filtering, ETags (nếu phù hợp).
3) CUỐI MỖI TUẦN:
   - Mini-project/feature thực tế (không phải CRUD đơn thuần), ví dụ: module đặt lịch, hàng đợi email, xử lý file nền, job scheduler, WebSocket notification.
   - Code review checklist chi tiết (chống “code rối”, tách component/logic, tránh trộn UI/logic, controller mảnh, xử lý lỗi/timeout, retry/cancellation).
   - Bài kiểm tra ngắn mô phỏng phỏng vấn (concept + live-coding nhỏ).
4) “MENTOR NOTES”: cảnh báo, tư duy, anti-pattern và sai lầm thường gặp (dựa trên trải nghiệm interviewer).
5) LỘ TRÌNH NỐI DÀI → BLOCKCHAIN:
   - Bridge map từ web2 sang web3: HTTP/REST ↔ RPC, JSON ↔ ABI, event ↔ message, bảo mật web2 ↔ smart contract security.
   - Tuần 7–8 thêm mục “Web3 Primer”: EVM/ABI/ERCs/gas, secure patterns (reentrancy, checks-effects-interactions), proxy/upgradeable; tooling nhanh (viem/ethers, Hardhat/Foundry) và backend gateway (NestJS) ở mức định hướng.
6) KỸ NĂNG LÀM VIỆC:
   - Git (commit message chuẩn, branch/rebase, PR review).
   - Linux cơ bản, Docker hoá service, đọc log/trace, quan sát hiệu năng (metrics/tracing ở mức nhập môn).
   - Giao tiếp: mỗi tuần 1 bài tập mô tả kỹ thuật bằng tiếng Anh (mức TOEIC ~450) cho README/PRD.

# Phạm vi kiến thức cốt lõi (hãy xen kẽ và tăng dần độ khó)

- C# nền tảng: OOP, struct vs class, collections, LINQ, delegates/events, generics, async/await & Task, memory basics (GC, span), exceptions.
- .NET & ASP.NET Core: middleware/pipeline, configuration/options, DI, filters, minimal APIs vs Controllers, model binding/validation, hosting & Kestrel.
- EF Core: DbContext, relationships, migrations, tracking/no-tracking, queries tối ưu (Include/AsSplitQuery), transactions/concurrency.
- Database: SQL Server (index/plan, transaction isolation), stored procedures (khi phù hợp), thêm góc nhìn Oracle/MySQL/Postgres (so sánh).
- Giao tiếp & tích hợp: RESTful chuẩn, Swagger/OpenAPI, SOAP/XML (nhận biết), JSON, WebSocket signal, Redis caching.
- Kiến trúc: layered/clean, SOLID, CQRS (mức nhập môn), microservices (khái niệm, bounded contexts), messaging (RabbitMQ/queue – overview).
- Test & debug: xUnit, integration tests, test data builders, debugging + tracing (Activity/ILogger).
- Bảo mật: OWASP top 10 cho API, authz, rate limiting, secrets management, CORS, input validation/sanitization.
- DevOps tối thiểu: Dockerfile, docker-compose, healthcheck, basic CI (build, test).

# Phần “khó nhưng phải học đúng” (nhấn mạnh tiêu chuẩn cao)

- Không dùng tutorial copy-paste; mỗi bài tập phải có: retry/cancellation, error handling, logging có ngữ cảnh, timeouts, backoff, idempotency nơi cần.
- Không trộn logic với UI; controller mỏng; tách service/domain rõ ràng.
- Không todo list vô nghĩa: yêu cầu bài tập có yêu cầu nghiệp vụ, ràng buộc dữ liệu, và non-functional (hiệu năng/bảo mật/observability).
- Git nghiêm túc: lịch sử sạch, PR có mô tả, review checklist.

# Đầu vào của tôi

- Thời lượng: 45 phút/ngày × 8 tuần.
- Nền tảng: sinh viên năm 4, đã biết Python cơ bản; hướng backend trước, sau đó web3/blockchain.
- Thiết bị: laptop bình thường; ưu tiên công cụ miễn phí, tài liệu tiếng Việt/Anh đều được.

# Cách trình bày đầu ra

- Dùng bảng tuần/ngày gọn gàng. Mỗi ngày: 5–8 mục bullet tối đa.
- Thêm khối “Resources (official)” với link docs.microsoft.com/learn, .NET docs, EF Core docs.
- Mỗi mini-project có README yêu cầu/kết quả mong đợi, lệnh chạy (Docker), tiêu chí pass/fail.
- Kèm rubric chấm điểm tự đánh giá (Beginner → Proficient → Strong).

# Yêu cầu bổ sung

- Nếu thiếu dữ kiện, hãy tự đưa ra giả định hợp lý và ghi rõ.
- Giữ tính thực chiến: ví dụ, demo Endpoint: create order → transactional outbox → background worker → email service.
- Kết thúc bằng “bản tóm tắt ôn tập 1 trang” + “danh sách 50 câu hỏi phỏng vấn C#/.NET/REST/DB” + “lộ trình 30-60-90 ngày sau khoá”.

Hãy bắt đầu bằng việc liệt kê toàn bộ Week 1 (7 ngày) thật chi tiết, rồi tóm tắt các tuần còn lại; sau đó mở rộng đầy đủ cho cả 8 tuần.
