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
using System.Collections.Generic; // Thêm cho ICollection nếu chưa có


namespace VAYTIEN.Controllers
{
    [Authorize]
    public class KhachHangController : Controller
    {
        private readonly QlvayTienContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<KhachHangController> _logger; // BỔ SUNG: Khai báo logger

        public KhachHangController(QlvayTienContext context, ICloudinaryService cloudinaryService, ILogger<KhachHangController> logger) // BỔ SUNG: Inject logger
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger; // Gán logger
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
                TaiKhoans = khachHang.TaiKhoanNganHangs.Select(tk => new TaiKhoanViewModel
                {
                    SoTaiKhoan = tk.SoTaiKhoan,
                    SoDu = tk.SoDu
                }).ToList(),
                HopDongs = hopDongs.Select(h => new ThongTinHopDongViewModel
                {
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

            // Xử lý upload ảnh mới
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
                    PublicId = _cloudinaryService.ParsePublicIdFromCloudinaryUrl(imageUrl), // Gọi từ service
                    LoaiAnh = "AnhDaiDien" // Hoặc "CMNDTruoc", "CMNTSau" tùy loại ảnh
                });
            }
            // else { kh.AnhDinhKems = new List<KhachHangAnh>(); } // Đảm bảo nếu không có ảnh thì collection rỗng

            // Sử dụng JsonSerializerOptions để serialize ReferenceHandler.Preserve cho navigation properties
            // Nếu bạn không có navigation properties trong KhachHang được serialize,
            // có thể dùng JsonSerializer.Serialize(kh) đơn giản hơn.
            TempData["KhachHang"] = System.Text.Json.JsonSerializer.Serialize(kh, new System.Text.Json.JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            });
            return RedirectToAction("CreateStep2");
        }

        // GET: Bước 2 - Form thông tin vay
        public IActionResult CreateStep2()
        {
            if (TempData["KhachHang"] is not string khachHangJson)
            {
                return RedirectToAction("CreateStep1");
            }
            ViewBag.KhachHangJson = khachHangJson;
            ViewBag.LoaiVayList = _context.LoaiVays.ToList();
            ViewBag.LoaiTienTeList = _context.LoaiTienTes.ToList();
            var viewModel = new CreateStep2ViewModel();
            return View(viewModel);
        }

        // POST: Bước 2 - Xử lý Form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep2(CreateStep2ViewModel viewModel, string khachHangJson)
        {
            if (string.IsNullOrEmpty(khachHangJson))
            {
                TempData["Error"] = "Phiên đăng ký đã hết hạn. Vui lòng thử lại.";
                return RedirectToAction("CreateStep1");
            }

            // Quan trọng: Deserialize với ReferenceHandler.Preserve nếu bạn đã serialize như vậy
            var khachHang = System.Text.Json.JsonSerializer.Deserialize<KhachHang>(khachHangJson, new System.Text.Json.JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            })!;
            khachHang.Email = User.Identity?.Name;

            // Kiểm tra và lưu thông tin khách hàng
            var existingKH = await _context.KhachHangs
                                            .Include(k => k.AnhDinhKems) // Cần include AnhDinhKems để xử lý nếu khách hàng đã tồn tại
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(k => k.Email == khachHang.Email);

            if (existingKH != null)
            {
                // Nếu khách hàng đã tồn tại, copy các thông tin cần cập nhật
                existingKH.HoTen = khachHang.HoTen;
                existingKH.CmndCccd = khachHang.CmndCccd;
                existingKH.NgaySinh = khachHang.NgaySinh;
                existingKH.DiaChi = khachHang.DiaChi;
                existingKH.Sdt = khachHang.Sdt;
                existingKH.NgheNghiep = khachHang.NgheNghiep;
                existingKH.TinhTrangHonNhan = khachHang.TinhTrangHonNhan;
                existingKH.DoiTuongVayMaDoiTuongVay = khachHang.DoiTuongVayMaDoiTuongVay;

                // Cập nhật AnhDinhKems: Xóa ảnh cũ và thêm ảnh mới từ bước 1
                if (khachHang.AnhDinhKems != null && khachHang.AnhDinhKems.Any())
                {
                    // Xóa tất cả ảnh cũ của khách hàng này trong DB và Cloudinary
                    foreach (var oldAnh in existingKH.AnhDinhKems.ToList()) // Dùng ToList() để tránh lỗi khi sửa đổi collection
                    {
                        await _cloudinaryService.DeleteImageAsync(oldAnh.PublicId!);
                        _context.KhachHangAnhs.Remove(oldAnh);
                    }
                    // Thêm ảnh mới từ khachHang (được truyền từ bước 1)
                    foreach (var newAnh in khachHang.AnhDinhKems)
                    {
                        existingKH.AnhDinhKems.Add(newAnh); // Thêm ảnh mới từ bước 1
                    }
                }

                _context.Update(existingKH); // Đánh dấu là cập nhật
                khachHang.MaKh = existingKH.MaKh; // Gán lại MaKh để dùng cho HopDong
            }
            else // Khách hàng mới
            {
                _context.KhachHangs.Add(khachHang);
            }

            await _context.SaveChangesAsync(); // Lưu khách hàng (mới hoặc cập nhật) để có MaKh

            // Gán các thông tin còn lại và lưu Hợp đồng, Tài sản...
            var hopDong = viewModel.HopDong;
            var taiSan = viewModel.TaiSan;

            hopDong.MaKh = khachHang.MaKh; // Đảm bảo dùng MaKh đã được lưu/cập nhật
            hopDong.TinhTrang = "Chờ phê duyệt";
            hopDong.SoTienConLai = hopDong.SoTienVay;

            var loaiVay = await _context.LoaiVays.FindAsync(hopDong.MaLoaiVay);
            if (loaiVay != null && loaiVay.LaiSuat.HasValue)
            {
                hopDong.LaiSuat = loaiVay.LaiSuat.Value;
            }
            else
            {
                hopDong.LaiSuat = 0; // Đặt mặc định 0 nếu không tìm thấy hoặc null
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

            var khachHang = await _context.KhachHangs
                                            .Include(kh => kh.DoiTuongVay) // Để hiển thị Đối tượng vay
                                            .Include(kh => kh.AnhDinhKems) // PHẢI INCLUDE để tải danh sách ảnh
                                            .FirstOrDefaultAsync(kh => kh.Email == userEmail);

            if (khachHang == null)
            {
                return RedirectToAction(nameof(CreateStep1));
            }
            return View(khachHang);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var khachHang = await _context.KhachHangs
                                            .Include(kh => kh.DoiTuongVay) // Để tải tên đối tượng vay nếu cần hiển thị
                                            .Include(kh => kh.AnhDinhKems) // PHẢI INCLUDE để tải danh sách ảnh hiện có
                                            .FirstOrDefaultAsync(kh => kh.MaKh == id);

            if (khachHang == null) return NotFound();
            if (khachHang.Email != User.Identity?.Name && !User.IsInRole("Admin")) return Forbid();

            // Truyền danh sách DoiTuongVay cho dropdown
            ViewBag.DoiTuongVayList = await _context.DoiTuongVays.ToListAsync();

            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKh,HoTen,CmndCccd,NgaySinh,DiaChi,Sdt,Email,NgheNghiep,TinhTrangHonNhan,DoiTuongVayMaDoiTuongVay")] KhachHang khachHang, IFormFile? anhFile)
        {
            if (id != khachHang.MaKh)
            {
                return NotFound();
            }

            var userEmail = User.Identity?.Name;
            // Tải lại khách hàng hiện tại từ DB để Entity Framework theo dõi và để lấy các collection (AnhDinhKems)
            var khachHangToUpdate = await _context.KhachHangs
                                                    .Include(kh => kh.AnhDinhKems) // RẤT QUAN TRỌNG: Include để tải ảnh cũ
                                                    .FirstOrDefaultAsync(kh => kh.MaKh == id);

            if (khachHangToUpdate == null) return NotFound();

            // Kiểm tra bảo mật: người dùng chỉ có thể sửa thông tin của chính họ
            if (khachHangToUpdate.Email != userEmail && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Loại bỏ lỗi ModelState cho các trường không được bind từ form hoặc readonly
            ModelState.Remove("Email"); // Email là readonly trên form
            ModelState.Remove("DiemTinDung"); // Không bind trực tiếp từ form
            ModelState.Remove("HanMucVay"); // Không bind trực tiếp từ form
            ModelState.Remove("AnhDinhKems"); // Nếu AnhDinhKems là ICollection, cũng có thể loại bỏ

            // Cập nhật các thuộc tính từ model binded (khachHang)
            // vào đối tượng đang được theo dõi (khachHangToUpdate)
            khachHangToUpdate.HoTen = khachHang.HoTen;
            khachHangToUpdate.CmndCccd = khachHang.CmndCccd;
            khachHangToUpdate.NgaySinh = khachHang.NgaySinh;
            khachHangToUpdate.DiaChi = khachHang.DiaChi;
            khachHangToUpdate.Sdt = khachHang.Sdt;
            khachHangToUpdate.NgheNghiep = khachHang.NgheNghiep;
            khachHangToUpdate.TinhTrangHonNhan = khachHang.TinhTrangHonNhan;
            khachHangToUpdate.DoiTuongVayMaDoiTuongVay = khachHang.DoiTuongVayMaDoiTuongVay;


            if (ModelState.IsValid)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Xử lý upload ảnh mới nếu người dùng có chọn file
                        if (anhFile != null && anhFile.Length > 0)
                        {
                            // Lấy ảnh đại diện cũ (nếu có, giả sử chỉ có 1 ảnh loại "AnhDaiDien")
                            var oldAvatar = khachHangToUpdate.AnhDinhKems.FirstOrDefault(a => a.LoaiAnh == "AnhDaiDien");

                            // Upload ảnh mới
                            var newImageUrl = await _cloudinaryService.UploadImageAsync(anhFile);
                            if (newImageUrl == null)
                            {
                                // Ghi log và báo lỗi thân thiện nếu upload Cloudinary thất bại
                                _logger.LogError("Cloudinary upload failed for user {UserId}", id);
                                TempData["Error"] = "Có lỗi xảy ra khi tải ảnh lên Cloudinary. Vui lòng thử lại.";
                                await transaction.RollbackAsync();
                                // Cần tải lại ViewBag.DoiTuongVayList nếu quay về View
                                ViewBag.DoiTuongVayList = await _context.DoiTuongVays.ToListAsync();
                                return View(khachHang); // Quay lại form với lỗi, giữ dữ liệu người dùng đã nhập
                            }

                            // Xóa ảnh cũ khỏi Cloudinary và DB
                            if (oldAvatar != null)
                            {
                                bool deletedFromCloudinary = await _cloudinaryService.DeleteImageAsync(oldAvatar.PublicId!);
                                if (!deletedFromCloudinary)
                                {
                                    _logger.LogWarning("Failed to delete old image {PublicId} from Cloudinary for user {UserId}", oldAvatar.PublicId, id);
                                    // Không throw lỗi ở đây để không chặn transaction chính nếu việc xóa ảnh cũ không quá quan trọng
                                }
                                _context.KhachHangAnhs.Remove(oldAvatar); // Xóa khỏi DB
                            }

                            // Thêm ảnh mới vào DB
                            khachHangToUpdate.AnhDinhKems.Add(new KhachHangAnh
                            {
                                Url = newImageUrl,
                                PublicId = ParsePublicIdFromCloudinaryUrl(newImageUrl),
                                LoaiAnh = "AnhDaiDien", // Gán loại ảnh nếu cần
                                NgayTaiLen = DateTime.UtcNow
                            });
                        }
                        // ELSE: Nếu người dùng KHÔNG chọn file mới, và không có logic xóa ảnh cũ nếu người dùng bỏ trống
                        // thì ảnh cũ vẫn giữ nguyên (nếu không có hành động nào để remove nó).
                        // Nếu user xoá ảnh cũ bằng cách nào đó (chưa có chức năng), thì logic này cần cập nhật thêm.

                        await _context.SaveChangesAsync(); // Lưu các thay đổi của khachHangToUpdate (bao gồm AnhDinhKems)
                        await transaction.CommitAsync();

                        TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Concurrency error during update for user {UserId}", id);
                        // Kiểm tra lại sự tồn tại của bản ghi
                        if (!_context.KhachHangs.Any(e => e.MaKh == khachHang.MaKh))
                        {
                            return NotFound();
                        }
                        else
                        {
                            TempData["Error"] = "Thông tin đã bị người khác thay đổi. Vui lòng thử lại.";
                            // Cần tải lại ViewBag.DoiTuongVayList nếu quay về View
                            ViewBag.DoiTuongVayList = await _context.DoiTuongVays.ToListAsync();
                            return View(khachHang); // Quay lại form với lỗi
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error updating user {UserId} and image", id);
                        TempData["Error"] = "Đã xảy ra lỗi khi cập nhật thông tin cá nhân: " + ex.Message;
                        // Cần tải lại ViewBag.DoiTuongVayList nếu quay về View
                        ViewBag.DoiTuongVayList = await _context.DoiTuongVays.ToListAsync();
                        return View(khachHang); // Quay lại form với lỗi, truyền khachHang (model binded) để giữ dữ liệu
                    }
                }
                return RedirectToAction(nameof(ThongTinCaNhan)); // Không cần id, nó sẽ tự tìm khách hàng theo email
            }

            // Nếu ModelState không hợp lệ, quay lại form Edit để hiển thị lỗi
            // Cần tải lại ViewBag.DoiTuongVayList nếu quay về View
            ViewBag.DoiTuongVayList = await _context.DoiTuongVays.ToListAsync();
            return View(khachHang);
        }

        // BỔ SUNG: Hàm ParsePublicIdFromCloudinaryUrl (PRIVATE)
        // Hàm này sẽ được gọi từ CreateStep1 và Edit POST
        private string ?ParsePublicIdFromCloudinaryUrl(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return null;

            try
            {
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath; // Kết quả: /your_cloud_name/image/upload/v12345/folder/image_name.png

                var uploadIndex = path.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
                if (uploadIndex == -1) return null;

                var publicIdWithVersion = path.Substring(uploadIndex + "/upload/".Length);

                // Loại bỏ phần version (nếu có, dạng v12345/)
                var versionIndex = publicIdWithVersion.IndexOf("/v", StringComparison.OrdinalIgnoreCase);
                if (versionIndex != -1 && (publicIdWithVersion.Length > versionIndex + 1 && Char.IsDigit(publicIdWithVersion[versionIndex + 1])))
                {
                    var nextSlashIndex = publicIdWithVersion.IndexOf('/', versionIndex + 1);
                    if (nextSlashIndex != -1)
                    {
                        publicIdWithVersion = publicIdWithVersion.Substring(nextSlashIndex + 1);
                    }
                }

                // Loại bỏ phần mở rộng file (.png, .jpg, ...)
                var lastDotIndex = publicIdWithVersion.LastIndexOf('.');
                if (lastDotIndex != -1)
                {
                    publicIdWithVersion = publicIdWithVersion.Substring(0, lastDotIndex);
                }
                _logger.LogDebug($"Parsed publicId from {imageUrl}: {publicIdWithVersion}");
                return publicIdWithVersion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to parse publicId from URL: {imageUrl}");
                return null;
            }
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
