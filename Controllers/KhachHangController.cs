using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq;
using System.IO; // Vẫn cần cho Path.GetExtension
using VAYTIEN.Models;
using VAYTIEN.Services;
using System;
using Microsoft.Extensions.Logging; // Thêm để log lỗi
namespace VAYTIEN.Controllers
{
    [Authorize]
    public class KhachHangController : Controller
    {
        private readonly QlvayTienContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public KhachHangController(QlvayTienContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // Action chính để khách hàng xem tất cả các hợp đồng và trạng thái của chúng
        [HttpGet]
        public async Task<IActionResult> ThongTinVay()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Challenge();

            var khachHang = await _context.KhachHangs
                .AsNoTracking()
                // Lấy kèm danh sách tài khoản ngân hàng của khách
                .Include(kh => kh.TaiKhoanNganHangs)
                .FirstOrDefaultAsync(kh => kh.Email == userEmail);

            if (khachHang == null) return RedirectToAction(nameof(CreateStep1));

            var hopDongs = await _context.HopDongVays
                .Where(h => h.MaKh == khachHang.MaKh)
                .Include(h => h.LichTraNos)
                .OrderByDescending(h => h.MaHopDong)
                .ToListAsync();

            // Tính toán tổng dư nợ
            decimal tongDuNoConLai = 0;
            foreach (var hd in hopDongs.Where(h => h.TinhTrang == "Đã duyệt"))
            {
                decimal tongGocDaTra = hd.LichTraNos.Where(l => l.TrangThai == "Đã trả").Sum(l => l.SoTienGoc ?? 0);
                tongDuNoConLai += hd.SoTienVay - tongGocDaTra;
            }

            var viewModel = new ThongTinVayViewModel
            {
                TongSoTienVay = tongDuNoConLai,
                // THÊM MỚI: Lấy danh sách tài khoản và số dư của khách hàng
                TaiKhoans = khachHang.TaiKhoanNganHangs.Select(tk => new TaiKhoanViewModel
                {
                    SoTaiKhoan = tk.SoTaiKhoan,
                    SoDu = tk.SoDu
                }).ToList(),
                HopDongs = hopDongs.Select(h => new ThongTinHopDongViewModel
                {
                    //... các thuộc tính khác giữ nguyên
                    MaHopDong = h.MaHopDong,
                    SoTienVay = h.SoTienVay,
                    NgayVay = h.NgayVay,
                    NgayHetHan = h.NgayHetHan,
                    TinhTrang = h.TinhTrang,
                    KyHanThang = h.KyHanThang,
                    LaiSuat = h.LaiSuat,
                    SoTienConLai = h.SoTienVay - h.LichTraNos.Where(l => l.TrangThai == "Đã trả").Sum(l => l.SoTienGoc ?? 0),
                    LichTra = h.LichTraNos.Select(l => new LichTraViewModel
                    {
                        KyHanThu = l.KyHanThu ?? 0,
                        NgayTra = l.NgayTra,
                        SoTienPhaiTra = l.SoTienPhaiTra,
                        TrangThai = l.TrangThai
                    }).OrderBy(x => x.KyHanThu).ToList()
                }).ToList()
            };

            ViewBag.LabelTongTien = "Tổng dư nợ gốc còn lại";
            return View(viewModel);
        }
        // GET: Bước 1 - Form thông tin cá nhân
        public IActionResult CreateStep1()
        {
            ViewBag.DoiTuongVayList = _context.DoiTuongVays.ToList();
            return View();
        }

        // POST: Bước 1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep1(KhachHang kh, IFormFile? anhFile)
        {
            if (kh.DoiTuongVayMaDoiTuongVay == null || kh.DoiTuongVayMaDoiTuongVay == 0)
                ModelState.AddModelError("DoiTuongVayMaDoiTuongVay", "Vui lòng chọn đối tượng vay");

            if (!ModelState.IsValid)
            {
                ViewBag.DoiTuongVayList = _context.DoiTuongVays.ToList();
                return View(kh);
            }

            // THAY THẾ LOGIC LƯU FILE CỤC BỘ BẰNG CLOUDINARY
            if (anhFile != null && anhFile.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(anhFile);
                if (imageUrl == null)
                {
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi tải ảnh lên. Vui lòng thử lại.");
                    ViewBag.DoiTuongVayList = _context.DoiTuongVays.ToList();
                    return View(kh);
                }

                // Tạo đối tượng KhachHangAnh mới và thêm vào tập hợp
                kh.AnhDinhKems.Add(new KhachHangAnh
                {
                    Url = imageUrl,
                    PublicId = ParsePublicIdFromCloudinaryUrl(imageUrl), // Bạn cần triển khai hàm này trong Controller hoặc Service
                    LoaiAnh = "AnhDaiDien" // Hoặc "CMNDTruoc", "CMNTSau" tùy loại ảnh
                });
            }

            TempData["KhachHang"] = System.Text.Json.JsonSerializer.Serialize(kh, new System.Text.Json.JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve // Quan trọng cho navigation properties nếu có
            });
            // TempData["KhachHang"] = System.Text.Json.JsonSerializer.Serialize(kh); // Dùng cái này nếu k có navigation properties
            return RedirectToAction("CreateStep2");
        }

        // GET: Bước 2 - Form thông tin vay
        public IActionResult CreateStep2()
        {
            // Đọc dữ liệu khách hàng từ TempData
            if (TempData["KhachHang"] is not string khachHangJson)
            {
                // Nếu không có, quay về bước 1
                return RedirectToAction("CreateStep1");
            }

            // Quan trọng: Đặt dữ liệu vào ViewBag để View có thể truy cập
            ViewBag.KhachHangJson = khachHangJson;

            // Chuẩn bị dữ liệu cho các dropdown
            ViewBag.LoaiVayList = _context.LoaiVays.ToList();
            ViewBag.LoaiTienTeList = _context.LoaiTienTes.ToList();

            // Tạo ViewModel rỗng để truyền cho View
            var viewModel = new CreateStep2ViewModel();

            return View(viewModel);
        }

        // POST: Bước 2 - Xử lý Form (ĐÃ SỬA)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep2(CreateStep2ViewModel viewModel, string khachHangJson)
        {
            // Kiểm tra xem dữ liệu khách hàng từ bước 1 có được gửi lên không
            if (string.IsNullOrEmpty(khachHangJson))
            {
                TempData["Error"] = "Phiên đăng ký đã hết hạn. Vui lòng thử lại.";
                return RedirectToAction("CreateStep1");
            }

            // Deserialize lại đối tượng KhachHang
            var khachHang = JsonSerializer.Deserialize<KhachHang>(khachHangJson)!;
            khachHang.Email = User.Identity?.Name; // Gán email của người dùng đang đăng nhập

            // Gán các đối tượng từ ViewModel
            var hopDong = viewModel.HopDong;
            var taiSan = viewModel.TaiSan;

            // Kiểm tra và lưu thông tin khách hàng (logic cũ giữ nguyên)
            var existingKH = await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(k => k.Email == khachHang.Email);
            if (existingKH != null)
            {
                khachHang.MaKh = existingKH.MaKh;
            }
            else
            {
                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();
            }

            // Gán các thông tin còn lại và lưu Hợp đồng, Tài sản...
            hopDong.MaKh = khachHang.MaKh;
            hopDong.TinhTrang = "Chờ phê duyệt";
            hopDong.SoTienConLai = hopDong.SoTienVay;

            // Tự động gán lãi suất từ loại vay đã chọn
            var loaiVay = await _context.LoaiVays.FindAsync(hopDong.MaLoaiVay);
            if (loaiVay != null && loaiVay.LaiSuat.HasValue)
            {
                hopDong.LaiSuat = (decimal)loaiVay.LaiSuat.Value;
            }

            _context.HopDongVays.Add(hopDong);
            await _context.SaveChangesAsync();

            taiSan.MaHopDong = hopDong.MaHopDong;
            _context.TaiSanTheChaps.Add(taiSan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Gửi yêu cầu vay vốn thành công!";
            return RedirectToAction("XacNhan");
        }
        // GET: Hiển thị trang xác nhận
        public IActionResult XacNhan()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }

        // Các action quản lý thông tin cá nhân
        [HttpGet]
        public async Task<IActionResult> ThongTinCaNhan()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Challenge();

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.Email == userEmail);
            if (khachHang == null)
            {
                return RedirectToAction(nameof(CreateStep1));
            }
            return View(khachHang);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null) return NotFound();
            if (khachHang.Email != User.Identity?.Name && !User.IsInRole("Admin")) return Forbid();
            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKh,HoTen,CmndCccd,NgaySinh,DiaChi,Sdt,Email,NgheNghiep,TinhTrangHonNhan")] KhachHang khachHang, IFormFile? anhFile)
        {
            if (id != khachHang.MaKh)
            {
                return NotFound();
            }

            var userEmail = User.Identity?.Name;
            // Tải lại khách hàng hiện tại từ DB để lấy các thuộc tính không được bind từ form (ví dụ: AnhDinhKems cũ)
            var existingKhachHang = await _context.KhachHangs
                                                    .Include(kh => kh.AnhDinhKems) // Đảm bảo tải ảnh cũ
                                                    .FirstOrDefaultAsync(kh => kh.MaKh == id);

            if (existingKhachHang == null) return NotFound();
            if (existingKhachHang.Email != userEmail && !User.IsInRole("Admin")) return Forbid();

            // Cập nhật các thuộc tính từ model binded vào existing entity
            existingKhachHang.HoTen = khachHang.HoTen;
            existingKhachHang.CmndCccd = khachHang.CmndCccd;
            existingKhachHang.NgaySinh = khachHang.NgaySinh;
            existingKhachHang.DiaChi = khachHang.DiaChi;
            existingKhachHang.Sdt = khachHang.Sdt;
            // existingKhachHang.Email = khachHang.Email; // Email không nên thay đổi qua form này
            existingKhachHang.NgheNghiep = khachHang.NgheNghiep;
            existingKhachHang.TinhTrangHonNhan = khachHang.TinhTrangHonNhan;
            // DoiTuongVayMaDoiTuongVay cũng có thể cần được cập nhật nếu có trên form Edit

            ModelState.Remove("DoiTuongVayMaDoiTuongVay"); // Xóa lỗi nếu không có trong form Edit

            if (ModelState.IsValid)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Xử lý upload ảnh mới nếu người dùng có chọn file
                        if (anhFile != null && anhFile.Length > 0)
                        {
                            // Lấy ảnh đại diện cũ (nếu có, giả sử chỉ có 1 ảnh đại diện)
                            var oldAvatar = existingKhachHang.AnhDinhKems.FirstOrDefault(a => a.LoaiAnh == "AnhDaiDien"); // Hoặc loại ảnh tương ứng

                            // Upload ảnh mới
                            var newImageUrl = await _cloudinaryService.UploadImageAsync(anhFile);
                            if (newImageUrl == null)
                            {
                                throw new Exception("Lỗi khi tải ảnh mới lên Cloudinary.");
                            }

                            // Xóa ảnh cũ khỏi Cloudinary và DB
                            if (oldAvatar != null)
                            {
                                await _cloudinaryService.DeleteImageAsync(oldAvatar.PublicId!); // publicId phải không null
                                _context.KhachHangAnhs.Remove(oldAvatar);
                            }

                            // Thêm ảnh mới vào DB
                            existingKhachHang.AnhDinhKems.Add(new KhachHangAnh
                            {
                                Url = newImageUrl,
                                PublicId = ParsePublicIdFromCloudinaryUrl(newImageUrl),
                                LoaiAnh = "AnhDaiDien", // Gán loại ảnh nếu cần
                                NgayTaiLen = DateTime.UtcNow
                            });
                        }
                        // Nếu không có file mới, và không có logic xóa ảnh cũ nếu người dùng bỏ trống
                        // thì ảnh cũ vẫn giữ nguyên (nếu không có hành động nào để remove nó).

                        _context.Update(existingKhachHang); // Hoặc Entry(existingKhachHang).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.KhachHangs.Any(e => e.MaKh == khachHang.MaKh))
                        {
                            await transaction.RollbackAsync();
                            return NotFound();
                        }
                        else
                        {
                            throw; // Ném lại để xử lý lỗi đồng thời
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "Đã xảy ra lỗi khi cập nhật thông tin và ảnh: " + ex.Message;
                        // Logger.LogError(ex, "Lỗi cập nhật khách hàng và ảnh.");
                        return View(khachHang); // Quay lại form với lỗi
                    }
                }
                return RedirectToAction(nameof(ThongTinCaNhan)); // Không cần id, nó sẽ tự tìm khách hàng theo email
            }

            // Nếu dữ liệu nhập không hợp lệ, quay lại form Edit để hiển thị lỗi
            return View(khachHang);
        }
        private string? ParsePublicIdFromCloudinaryUrl(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return null;

            // Ví dụ URL: https://res.cloudinary.com/your_cloud_name/image/upload/v1678901234/vaytien_app_uploads/unique_id.png
            // PublicId muốn lấy: vaytien_app_uploads/unique_id
            var uri = new Uri(imageUrl);
            var path = uri.AbsolutePath; // Kết quả: /your_cloud_name/image/upload/v1678901234/vaytien_app_uploads/unique_id.png

            // Tìm vị trí của "/upload/"
            var uploadIndex = path.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
            if (uploadIndex == -1) return null;

            var publicIdWithVersion = path.Substring(uploadIndex + "/upload/".Length); // v1678901234/vaytien_app_uploads/unique_id.png

            // Loại bỏ phần version (nếu có, dạng v12345/)
            var versionIndex = publicIdWithVersion.IndexOf("/v", StringComparison.OrdinalIgnoreCase);
            if (versionIndex != -1 && (publicIdWithVersion.Length > versionIndex + 1 && Char.IsDigit(publicIdWithVersion[versionIndex + 1])))
            {
                var nextSlashIndex = publicIdWithVersion.IndexOf('/', versionIndex + 1);
                if (nextSlashIndex != -1)
                {
                    publicIdWithVersion = publicIdWithVersion.Substring(nextSlashIndex + 1); // vaytien_app_uploads/unique_id.png
                }
            }

            // Loại bỏ phần mở rộng file (.png, .jpg, ...)
            var lastDotIndex = publicIdWithVersion.LastIndexOf('.');
            if (lastDotIndex != -1)
            {
                publicIdWithVersion = publicIdWithVersion.Substring(0, lastDotIndex); // vaytien_app_uploads/unique_id
            }

            return publicIdWithVersion;
        }
        public async Task<IActionResult> ChiTiet(int maHopDong, int kyHanThu)
        {
            var lichTra = await _context.LichTraNos
                .Include(l => l.MaHopDongNavigation.MaKhNavigation)
                .FirstOrDefaultAsync(x => x.MaHopDong == maHopDong && x.KyHanThu == kyHanThu);

            if (lichTra?.MaHopDongNavigation?.MaKhNavigation == null) return NotFound();
            if (lichTra.MaHopDongNavigation.MaKhNavigation.Email != User.Identity.Name) return Forbid();

            // =======================================================
            // SỬA LỖI: KIỂM TRA XEM ĐÂY CÓ PHẢI KỲ HẠN ĐÚNG ĐỂ THANH TOÁN KHÔNG
            // =======================================================
            var nextPayableInstallment = await _context.LichTraNos
                .Where(l => l.MaHopDong == maHopDong && l.TrangThai != "Đã trả")
                .OrderBy(l => l.KyHanThu)
                .FirstOrDefaultAsync();

            if (nextPayableInstallment != null && kyHanThu > nextPayableInstallment.KyHanThu)
            {
                TempData["Error"] = $"Vui lòng thanh toán cho kỳ hạn #{nextPayableInstallment.KyHanThu} trước.";
                return RedirectToAction("ThongTinVay", "KhachHang");
            }
            // =======================================================

            var viewModel = new ThanhToanViewModel
            {
                MaHopDong = maHopDong,
                KyHan = kyHanThu,
                TenKhachHang = lichTra.MaHopDongNavigation.MaKhNavigation.HoTen,
                NgayTra = lichTra.NgayTra?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today,
                SoTienPhaiTra = lichTra.SoTienPhaiTra ?? 0,
                TrangThai = lichTra.TrangThai ?? "Chưa trả"
            };
            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> DiemTinDung()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Challenge();

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.Email == userEmail);
            if (khachHang == null)
            {
                // Nếu chưa có hồ sơ, không thể xem điểm
                return RedirectToAction(nameof(CreateStep1));
            }

            // Nếu khách hàng chưa được chấm điểm, thực hiện chấm điểm lần đầu
            if (khachHang.DiemTinDung == null)
            {
                // Giả sử bạn đã inject IDiemTinDungService vào controller này
                // await _diemTinDungService.CapNhatDiemVaHanMucAsync(khachHang.MaKh);
                // Tải lại dữ liệu sau khi chấm điểm
                // khachHang = await _context.KhachHangs.FindAsync(khachHang.MaKh);

                // Tạm thời gán điểm và hạn mức để hiển thị, logic chấm điểm thật sẽ nằm ở service
                khachHang.DiemTinDung = 500;
                khachHang.HanMucVay = 75000000;
            }

            return View(khachHang); // Trả về View DiemTinDung.cshtml
        }
        // BỔ SUNG: Action xem chi tiết hợp đồng
       

    }

}
