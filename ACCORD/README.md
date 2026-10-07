# ACCORD

Project C# .NET 10 độc lập: backend ASP.NET Core Minimal API và frontend HTML/CSS/JavaScript được phục vụ cùng origin. Không phụ thuộc solution CQRS, Node.js hoặc thư viện Supabase bên thứ ba. Backend kết nối Supabase Auth và Data REST API bằng HttpClient.

## Chạy ứng dụng

1. Cài .NET SDK 10.
2. Tạo Supabase project. Bảng được tạo tự động bằng migration khi ứng dụng khởi động.
3. Trong thư mục ACCORD, cấu hình thông tin project bằng user secrets:

```powershell
dotnet user-secrets set "Supabase:Url" "https://YOUR_PROJECT.supabase.co"
dotnet user-secrets set "Supabase:PublishableKey" "YOUR_PUBLISHABLE_KEY"
dotnet user-secrets set "ConnectionStrings:SupabaseDatabase" "Host=YOUR_POOLER_HOST;Port=5432;Database=postgres;Username=postgres.YOUR_PROJECT;Password=YOUR_DATABASE_PASSWORD;SSL Mode=VerifyFull;Timeout=15"
dotnet run --urls http://localhost:5080
```

Mở http://localhost:5080 để vào thẳng trang chủ, không có màn hình đăng nhập/đăng ký. Trong Supabase Dashboard → Authentication → Sign In / Providers, bật **Allow anonymous sign-ins** để lưu công việc bằng phiên khách. Có thể thay user secrets bằng biến môi trường `Supabase__Url` và `Supabase__PublishableKey` khi triển khai.

API chỉ dùng publishable key (hoặc legacy anon key), không dùng service_role/secret key: RLS bảo đảm mỗi tài khoản chỉ truy cập công việc của chính mình. Migration dùng connection string PostgreSQL riêng có quyền DDL; không đưa connection string vào frontend hoặc Git. Production cần reverse proxy HTTPS và cấu hình forwarded headers/rate limiting phù hợp với proxy.

## Migration tự động

Trong Supabase Dashboard, chọn **Connect → Session pooler** và lấy host, port, username, database. Mật khẩu là mật khẩu database đã đặt khi tạo project; có thể đặt lại trong Database Settings nếu quên. Dùng session pooler port 5432 hoặc direct connection; session pooler hỗ trợ IPv4. Connection string Npgsql dùng dạng `Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=VerifyFull`, không phải URI `postgresql://...`. Nếu mật khẩu chứa dấu chấm phẩy, đặt giá trị Password trong dấu nháy kép theo cú pháp connection string Npgsql.

`Database:AutoMigrate` mặc định là `true`. Trước khi mở HTTP server, ACCORD kết nối PostgreSQL và chạy các file SQL nhúng trong `Migrations/` theo tên tăng dần. Lịch sử và SHA-256 được lưu ở `accord_internal.schema_migrations`; migration đã chạy được bỏ qua. Không sửa migration đã áp dụng: thêm file mới như `002_add_description.sql` rồi build lại.

Migration chạy trong transaction và khóa advisory để tránh hai instance chạy cùng lúc. Khi có lỗi, transaction rollback và ứng dụng dừng khởi động. Nếu trước đây đã chạy schema thủ công, migration đầu dùng `IF NOT EXISTS` để giữ dữ liệu trong bảng hiện có; bảng phải có cấu trúc tương thích. RLS, grants và policy vẫn được áp dụng. Sau commit, PostgREST được thông báo tải lại schema.

Thiếu connection string sẽ dừng khởi động với hướng dẫn cấu hình. Muốn chủ động chạy ứng dụng không migration, đặt `Database:AutoMigrate=false`; khi đó bạn tự chịu trách nhiệm cập nhật schema. Biến môi trường tương ứng khi triển khai: `ConnectionStrings__SupabaseDatabase`, `Database__AutoMigrate`. Nếu database dùng CA riêng, cấu hình `Root Certificate` thay vì tắt xác thực chứng chỉ.

Tài liệu kết nối: https://supabase.com/docs/guides/database/connecting-to-postgres.

Với project hiện tại, chứng chỉ CA nằm tại `certificates/prod-ca-2021.crt`. Connection string trong user secrets đã thêm `Root Certificate` trỏ tới file này, giữ `SSL Mode=VerifyFull`. Khi chuyển máy hoặc triển khai, cập nhật đường dẫn chứng chỉ cho môi trường mới.

## Chức năng và API

Menu ngang theo thứ tự **Home → Task**. Home (`#home`, mặc định) hiển thị tối đa 5 công việc mới nhất, theo `created_at` giảm dần. Task (`#task`) hiển thị toàn bộ danh sách và cho phép thêm, hoàn thành, xóa công việc. Hai trang dùng chung phiên khách trong trình duyệt.

- `GET /api/health`: trạng thái cấu hình cục bộ; không kiểm tra kết nối Supabase thực tế.
- `POST /api/session/guest`: tạo phiên khách tự động, không yêu cầu email/mật khẩu.
- `POST /api/session/refresh`: gia hạn phiên khách, JSON `{ "refreshToken": "..." }`.
- `GET /api/tasks`, `POST /api/tasks`: đọc/thêm công việc (`{ "title": "..." }`).
- `PATCH /api/tasks/{id}`: cập nhật hoàn thành (`{ "isComplete": true }`).
- `DELETE /api/tasks/{id}`: xóa công việc.

API công việc yêu cầu `Authorization: Bearer <Supabase access_token>`, frontend tự quản lý phiên khách và refresh token. Supabase xác thực JWT và thực thi RLS như trước, không mở bảng cho role anon. Phiên được lưu trong localStorage để truy cập lại công việc sau khi tải lại trang. Xóa dữ liệu trình duyệt, dùng trình duyệt khác hoặc mất refresh token sẽ mất quyền truy cập công việc của phiên khách trước; không tự tạo phiên mới khi refresh thất bại. Bản khởi đầu chưa có liên kết tài khoản, realtime hoặc upload file.

## Backend CQRS

Backend tách luồng đọc và ghi bằng handler được đăng ký trực tiếp với DI, không cần MediatR. Hai luồng dùng cùng database Supabase; hiện chưa cần read database riêng hoặc event sourcing.

```text
Api/Endpoints.cs
  GET /tasks → ListTasksQuery → ListTasksQueryHandler → ITaskReader
  POST /tasks → CreateTaskCommand → CreateTaskCommandHandler → ITaskWriter
  PATCH /tasks/{id} → SetTaskCompletionCommand → handler → ITaskWriter
  DELETE /tasks/{id} → DeleteTaskCommand → handler → ITaskWriter
  POST /session/* → session commands → handlers → ISessionGateway
                                              ↓
Infrastructure/Supabase → SupabaseClient → Supabase REST/Auth
```

`Application/` chứa command, query, handler, validation và interface truy cập dữ liệu; không phụ thuộc HttpContext, IResult hay Npgsql. `Api/` ánh xạ request sang command/query và kết quả sang HTTP. `Infrastructure/` xử lý URL, HTTP headers, JWT chuyển tiếp và lỗi Supabase. `DependencyInjection.cs` nối các thành phần. `Program.cs` chỉ cấu hình hosting, middleware, migration và route.

Route và JSON của frontend được giữ nguyên. Command thêm task kiểm tra/chuẩn hóa title trước khi ghi. Query chỉ dùng `ITaskReader`; command chỉ dùng `ITaskWriter`. JWT được truyền theo từng request đến Supabase để áp dụng RLS.

## Kiểm tra

```powershell
dotnet build ACCORD.slnx
dotnet test ACCORD.slnx -c Release
```

Sau khi bật anonymous sign-ins: mở hai trình duyệt/phiên riêng, thêm công việc ở mỗi phiên, xác nhận không đọc/sửa/xóa được công việc của phiên khác; kiểm tra tải lại trang, thêm, đánh dấu hoàn thành và xóa trên giao diện.

Test API trong `tests/ACCORD.Tests` dùng Supabase HTTP giả lập và tắt migration, không truy cập hoặc thay đổi database thật. Test kiểm tra command validation, query ordering, truyền JWT, body ghi, phiên khách/refresh và ánh xạ lỗi.

Tài liệu chính thức: https://supabase.com/docs/guides/api và https://supabase.com/docs/guides/getting-started/api-keys.
