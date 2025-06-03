using Approval_System.data;
using Approval_System.Models;
using Approval_System.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Approval_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = model.Password,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserList");
        }

        [HttpGet]
        public IActionResult UserList()
        {
            var users = _context.Users
                .Where(u => u.Role == "User")
                .ToList();

            return View(users);
        }

        [HttpGet]
        public IActionResult CreateFile()
        {
            var users = _context.Users.Where(u => u.Role == "User").ToList();

            var model = new CreateFileViewModel
            {
                AllUsers = users
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFile(CreateFileViewModel model)
        {
            model.AllUsers = _context.Users.Where(u => u.Role == "User").ToList();

            var selectedOrders = model.UserOrders
                .Where(x => model.SelectedUserIds.Contains(x.Key))
                .Select(x => x.Value)
                .ToList();

            if (!ModelState.IsValid || selectedOrders.Count != model.SelectedUserIds.Count || selectedOrders.Distinct().Count() != selectedOrders.Count)
            {
                if (selectedOrders.Count != model.SelectedUserIds.Count)
                    ModelState.AddModelError("", "يجب تحديد ترتيب لكل مستخدم تم اختياره.");

                if (selectedOrders.Distinct().Count() != selectedOrders.Count)
                    ModelState.AddModelError("", "يجب أن يكون الترتيب فريدًا لكل مستخدم.");

                return View(model);
            }

            // رفع الملف
            string uniqueFileName = null;
            if (model.UploadedFile != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);

                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.UploadedFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.UploadedFile.CopyToAsync(fileStream);
                }
            }

            var file = new FileItem
            {
                Title = model.Title,
                Description = model.Description,
                CreatedAt = DateTime.Now,
                CreatedById = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                SentToUserId = model.UserOrders.OrderBy(x => x.Value).First().Key,
                Status = "قيد الانتظار",
                FilePath = uniqueFileName
            };

            _context.FileItems.Add(file);
            await _context.SaveChangesAsync();

            foreach (var kvp in model.UserOrders.Where(kvp => model.SelectedUserIds.Contains(kvp.Key)))
            {
                var step = new WorkflowStep
                {
                    FileItemId = file.Id,
                    UserId = kvp.Key,
                    Order = kvp.Value,
                    IsApproved = false
                };

                _context.WorkflowSteps.Add(step);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("FileList");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult FileList()
        {
            var files = _context.FileItems
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            return View(files);
        }


        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && u.Role == "User");
            if (user == null)
                return NotFound();

            var model = new CreateUserViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Password = user.PasswordHash // يمكنك تركه فارغًا لو ما حبيت تظهر الباسورد
            };

            ViewBag.UserId = user.Id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(int id, CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Id == id && u.Role == "User");
            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Email = model.Email;

            // فقط حدث كلمة المرور إذا تم إدخالها
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = model.Password;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("UserList");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreUser(int id)
        {
            var user = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.Id == id && u.IsDeleted);
            if (user == null)
                return NotFound();

            user.IsDeleted = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("DeletedUsers");
        }



        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.Id == id && u.Role == "User");
            if (user == null)
                return NotFound();

            user.IsDeleted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("UserList");
        }

        [HttpGet]
        public IActionResult FileDetails(int id)
        {
            var file = _context.FileItems
                .Include(f => f.CreatedBy)
                .Include(f => f.WorkflowSteps)
                    .ThenInclude(ws => ws.User)
                .FirstOrDefault(f => f.Id == id);

            if (file == null)
                return NotFound();

            return View(file);
        }
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ResumeWorkflow(int fileId)
{
    var file = _context.FileItems
        .Include(f => f.WorkflowSteps)
        .FirstOrDefault(f => f.Id == fileId);

    if (file == null || file.Status != "مرفوض")
        return NotFound();

    // العثور على أول خطوة تم رفضها
    var rejectedStep = file.WorkflowSteps
        .Where(ws => !ws.IsApproved && ws.IsRejected)
        .OrderBy(ws => ws.Order)
        .FirstOrDefault();

    if (rejectedStep == null)
        return BadRequest("لا يوجد خطوات مرفوضة يمكن تجاوزها.");

    // تحديث الإجراء لهذه الخطوة
    rejectedStep.Action = "تم تجاوزه من قبل الأدمن";

    // استئناف العملية من أول شخص بعده
    var nextStep = file.WorkflowSteps
        .Where(ws => ws.Order > rejectedStep.Order && !ws.IsApproved && !ws.IsRejected)
        .OrderBy(ws => ws.Order)
        .FirstOrDefault();

    if (nextStep == null)
    {
        // لا يوجد مستخدمين آخرين بعد الرافض
        file.Status = "موافق";
        file.SentToUserId = null;
    }
    else
    {
        file.Status = "قيد الانتظار";
        file.SentToUserId = nextStep.UserId;
    }

    await _context.SaveChangesAsync();

    return RedirectToAction("FileDetails", new { id = fileId });
}


        [HttpGet]
        public IActionResult DeletedUsers()
        {
            var deletedUsers = _context.Users
                .IgnoreQueryFilters()
                .Where(u => u.Role == "User" && u.IsDeleted)
                .ToList();

            return View("DeletedUsers", deletedUsers);
        }


    }

}

