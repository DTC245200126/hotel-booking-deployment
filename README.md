# HỆ THỐNG ĐẶT PHÒNG KHÁCH SẠN

Học phần: Triển khai và Quản trị Hệ thống Phần mềm

Đề tài số 6: Hệ thống đặt phòng khách sạn

## 1. Giới thiệu đề tài

Trong bài thực hành này, em xây dựng và triển khai hệ thống đặt phòng khách sạn bằng ASP.NET Core MVC trên .NET 9 và PostgreSQL 17.

Hệ thống hỗ trợ quản lý thông tin phòng, khách hàng và đặt phòng. Ngoài các chức năng của website, em sử dụng Docker Compose để triển khai các dịch vụ, Nginx làm reverse proxy, Prometheus và Grafana để giám sát hoạt động, Loki và Promtail để thu thập và tra cứu log.

Mục tiêu của bài là triển khai được một hệ thống web có cơ sở dữ liệu, có thể theo dõi tình trạng hoạt động và áp dụng một số biện pháp bảo mật cơ bản.

## 2. Công nghệ sử dụng

| Công nghệ | Mục đích |
|---|---|
| ASP.NET Core MVC (.NET 9) | Xây dựng website |
| PostgreSQL 17 | Lưu trữ dữ liệu |
| pgAdmin | Quản lý cơ sở dữ liệu |
| Docker Compose | Chạy và quản lý các dịch vụ |
| Nginx | Reverse proxy |
| Prometheus | Thu thập dữ liệu giám sát |
| Grafana | Hiển thị biểu đồ và dashboard |
| Loki | Lưu trữ log |
| Promtail | Thu thập log container |

## 3. Các chức năng của hệ thống

Website có các chức năng chính:

- Quản lý thông tin phòng khách sạn.
- Quản lý thông tin khách hàng.
- Tạo và xem danh sách đặt phòng.
- Kiểm tra thời gian đặt phòng để hạn chế đặt trùng lịch.

Các chức năng được kết nối với cơ sở dữ liệu PostgreSQL.

## 4. Hướng dẫn chạy chương trình

Bước 1: Cài đặt Docker Desktop và mở thư mục dự án bằng VS Code.

Bước 2: Tạo file `.env` từ `.env.example` nếu chưa có:

```powershell
if (-not (Test-Path .env)) { Copy-Item .env.example .env }
notepad .env
```

Thay các giá trị `CHANGE_ME_*` bằng mật khẩu riêng cho PostgreSQL, pgAdmin và Grafana; điền email dùng để đăng nhập pgAdmin. Không đưa file `.env` lên GitHub. File này đã được thêm vào `.gitignore`.

Bước 3: Kiểm tra và chạy hệ thống:

```powershell
docker compose config --quiet
docker compose up -d --build
docker compose ps
```

Bước 4: Truy cập các dịch vụ:

| Dịch vụ | Địa chỉ |
|---|---|
| Website | http://localhost:8085 |
| Website qua Nginx | http://localhost:8086 |
| pgAdmin | http://localhost:5050 |
| Prometheus | http://localhost:9095 |
| Grafana | http://localhost:3006 |

PostgreSQL và Loki được sử dụng trong mạng Docker, không cần mở cổng trực tiếp ra máy tính.

Để tạm dừng hệ thống:

```powershell
docker compose stop
```

Để chạy lại:

```powershell
docker compose up -d
```

**Lưu ý:** Không sử dụng `docker compose down -v` vì có thể làm mất dữ liệu lưu trong Docker volumes.

## 5. Giám sát hệ thống bằng Prometheus và Grafana

Em sử dụng Prometheus để thu thập thông tin hoạt động của website, container và PostgreSQL.

Các thành phần giám sát gồm:

- `prometheus`: Giám sát Prometheus.
- `cadvisor`: Theo dõi tài nguyên container.
- `postgres`: Theo dõi PostgreSQL.
- `hotel-website`: Kiểm tra website có hoạt động hay không.

Có thể kiểm tra tại:

http://localhost:9095/targets

Khi các dịch vụ hoạt động bình thường, trạng thái hiển thị là `UP`.

Grafana được sử dụng để hiển thị các thông tin giám sát thông qua dashboard **Hotel Booking Monitoring**, gồm trạng thái website, thông tin PostgreSQL và tài nguyên container.

## 6. Thu thập và tra cứu log

Em sử dụng Promtail để thu thập log từ các container và gửi về Loki.

Để xem log, truy cập Grafana, chọn Explore, sau đó chọn nguồn dữ liệu Loki.

Một số câu truy vấn LogQL:

**Truy vấn 1: Xem log website**

```logql
{service="web"}
```

**Truy vấn 2: Xem các yêu cầu GET của Nginx**

```logql
{service="nginx"} |= "GET"
```

**Truy vấn 3: Thống kê số dòng log trong 5 phút**

```logql
sum by (service) (count_over_time({service=~"nginx|web|postgres"}[5m]))
```

Các truy vấn giúp kiểm tra hoạt động của hệ thống và hỗ trợ tìm lỗi khi cần thiết.

## 7. Các biện pháp bảo mật

Trong quá trình triển khai, em áp dụng một số biện pháp bảo mật cơ bản:

- Sử dụng file `.env` để lưu mật khẩu, không đưa mật khẩu lên GitHub.
- Giới hạn các cổng truy cập từ máy tính bằng địa chỉ `127.0.0.1`.
- Không mở trực tiếp cổng PostgreSQL ra ngoài.
- Cấu hình Nginx với các HTTP security headers.
- Website chạy bằng user không phải root. Một số dịch vụ monitoring được bỏ Linux capabilities và bật `no-new-privileges`.
- Sử dụng Docker network để quản lý kết nối giữa các dịch vụ.

Hiện tại, Nginx sử dụng HTTP kết hợp với security headers cơ bản, chưa triển khai HTTPS.

Promtail và cAdvisor cần quyền đọc một số thông tin từ Docker và máy chủ để thu thập log, dữ liệu giám sát. Các quyền này được giới hạn phù hợp với môi trường thực hành.

## 8. Quản lý mã nguồn bằng GitHub

Mã nguồn và các file cấu hình được quản lý trên GitHub:

https://github.com/DTC245200126/hotel-booking-deployment

Quá trình thực hiện được chia thành các phần:

- Commit 1: Triển khai website, PostgreSQL, pgAdmin và Nginx.
- Commit 2: Tích hợp Prometheus và Grafana.
- Commit 3: Tích hợp Loki, Promtail và các truy vấn LogQL.
