# Hotel Booking Deployment

Đề 6 - Hệ thống Đặt phòng Khách sạn, môn Triển khai và Quản trị Hệ thống Phần mềm.

## Kiến trúc

Ứng dụng ASP.NET Core MVC .NET 9 sử dụng PostgreSQL 17. Docker Compose triển khai website, pgAdmin, Nginx reverse proxy, Prometheus, Grafana, Loki và Promtail.

| Thành phần | Vai trò | Cổng truy cập trên máy host |
| --- | --- | --- |
| Nginx | Reverse proxy đến website; security headers | http://localhost:8086 |
| Website | Ứng dụng đặt phòng khách sạn | http://localhost:8085 |
| pgAdmin | Quản trị PostgreSQL | http://localhost:5050 |
| Prometheus | Thu thập metrics mỗi 15 giây | http://localhost:9095 |
| Grafana | Dashboard metrics và Explore/LogQL | http://localhost:3006 |
| PostgreSQL | Database `hotelbooking` | Không publish ra host |
| Loki | Lưu log tập trung, chỉ truy cập trong monitoring network | Không publish ra host |
| Promtail | Khám phá và gửi Docker logs của project đến Loki | Không publish ra host |

Prometheus scrape Prometheus, cAdvisor, postgres-exporter và Blackbox Exporter (kiểm tra website qua Nginx). Grafana provision sẵn datasource Prometheus và Loki cùng dashboard **Hotel Booking Monitoring**. Dashboard JSON được lưu tại `monitoring/grafana/dashboards/`.

Compose chia các container thành `database_network` và `monitoring_network`. Nginx nối vào cả hai để chuyển tiếp đến website và cho Blackbox Exporter kiểm tra qua Nginx. PostgreSQL không có cổng host; Loki cũng không publish cổng ra host.

## Chuẩn bị

Yêu cầu Docker Desktop với Docker Compose v2. Tại thư mục dự án, tạo `.env` nếu chưa có và điền mật khẩu mạnh, riêng biệt:

```powershell
if (-not (Test-Path .env)) { Copy-Item .env.example .env }
notepad .env
```

Đặt các giá trị `POSTGRES_PASSWORD`, `PGADMIN_PASSWORD` và `GRAFANA_PASSWORD`; `PGADMIN_EMAIL` là tài khoản đăng nhập pgAdmin. Dùng mật khẩu ngẫu nhiên dài do password manager tạo. Không dùng các giá trị `CHANGE_ME_*` để triển khai và không commit `.env`. File này nằm trong `.gitignore`; `.env.example` chỉ chứa placeholder.

## Khởi động và kiểm tra

```powershell
docker compose config --quiet
docker compose up -d --build
docker compose ps
```

`docker compose up` không xóa named volumes. PostgreSQL lưu dữ liệu trong `postgres_data`; Grafana, Prometheus, Loki và Promtail cũng có volumes riêng để giữ dữ liệu hoặc vị trí đọc log. Để dừng mà vẫn giữ dữ liệu, dùng `docker compose stop`; để chạy lại, dùng `docker compose up -d`. **Không chạy `docker compose down -v`**, vì tùy chọn `-v` xóa volumes.

Kiểm tra Prometheus targets tại **Status → Targets** hoặc chạy:

```powershell
Invoke-RestMethod http://localhost:9095/api/v1/targets
```

Các job mong đợi: `prometheus`, `cadvisor`, `postgres`, `hotel-website`.

## Grafana và dashboard

Đăng nhập Grafana tại http://localhost:3006 bằng user `admin` và mật khẩu `GRAFANA_PASSWORD` trong `.env`. Dashboard **Hotel Booking Monitoring** có các panel trạng thái website/PostgreSQL, kết nối và dung lượng database, CPU/RAM container, và transaction commit PostgreSQL.

Datasource được provision từ:

- `monitoring/grafana/provisioning/datasources/datasources.yml`
- `monitoring/grafana/provisioning/dashboards/provider.yml`

Dashboard JSON nằm trong `monitoring/grafana/dashboards/hotel-booking-monitoring.json`. Loki và Prometheus được truy cập nội bộ bằng tên service Compose.

## Loki, Promtail và LogQL

Promtail dùng Docker service discovery và chỉ giữ các container có label Compose project `hotel-booking-deployment`. Các log được gắn nhãn `service`, `container` và `stream`, sau đó gửi đến Loki. Mở Grafana **Explore**, chọn datasource **Loki** và chạy các truy vấn sau:

```logql
{service="web"}
```

Xem log truy cập Nginx:

```logql
{service="nginx"} |= "GET"
```

Đếm số log theo service trong 5 phút gần nhất:

```logql
sum by (service) (count_over_time({service=~"nginx|web|postgres"}[5m]))
```

Loki giữ log tối đa 7 ngày; Prometheus giữ metrics 3 ngày. Dữ liệu được lưu cục bộ trong named volumes, không gửi đến dịch vụ cloud.

## Hardening và giới hạn

- Website chạy non-root (UID 1654). Loki chạy non-root (UID 10001), filesystem chỉ đọc, bỏ Linux capabilities và bật `no-new-privileges`. Web, Prometheus, Grafana, postgres-exporter và Blackbox Exporter cũng bỏ capabilities và bật `no-new-privileges`.
- cAdvisor cần đọc một số đường dẫn host để thu thập chỉ số; các bind mount host được đặt read-only và cAdvisor chỉ tham gia monitoring network.
- Nginx thêm `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` và tắt `server_tokens`. Phần triển khai hiện dùng HTTP nội bộ/localhost, chưa cấu hình HTTPS.
- Cổng web, Nginx, pgAdmin, Prometheus và Grafana chỉ bind vào `127.0.0.1`. PostgreSQL và Loki không mở cổng host.
- Mật khẩu chỉ lấy từ `.env`; Compose yêu cầu các biến cần thiết và không cung cấp mật khẩu mặc định dùng được.
- Promtail hiện chạy UID 0 để đọc Docker logs và gọi Docker service discovery. Socket được mount `:ro`, nhưng **read-only mount không giới hạn các API mà client có thể gọi trên Docker daemon**; quyền truy cập socket vẫn rất nhạy cảm và có thể tương đương quyền quản trị host nếu Promtail bị xâm nhập. Cấu hình lọc log theo project, bỏ capabilities, filesystem chỉ đọc và không publish cổng Promtail, nhưng không loại bỏ rủi ro từ socket. Chỉ dùng trên máy tin cậy.
- Đề thi yêu cầu Promtail. Theo tài liệu Grafana, Promtail đã hết vòng đời hỗ trợ từ ngày 2-3-2026; giữ cấu hình này để đáp ứng yêu cầu học phần. Với triển khai mới cần được duy trì, hãy đánh giá chuyển sang Grafana Alloy.

Không thay đổi chức năng website, schema PostgreSQL hoặc dữ liệu đặt phòng khi bổ sung monitoring. Không xóa volumes để xử lý lỗi triển khai.

## Báo cáo học phần

README này hướng dẫn triển khai, nhưng không thay thế báo cáo tổng hợp tối thiểu 10 trang theo đề thi. Báo cáo cần thông tin sinh viên trên bìa và ảnh minh họa kết quả chạy website, pgAdmin, Grafana, LogQL và hardening.
