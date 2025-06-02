using Approval_System.data;
using Approval_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize(Roles = "User")]
public class UserController : Controller
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Inbox()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var userSteps = _context.WorkflowSteps
            .Include(ws => ws.FileItem)
            .Where(ws => ws.UserId == userId)
            .OrderByDescending(ws => ws.FileItem.CreatedAt)
            .ToList();

        return View(userSteps);
    }


    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        var step = await _context.WorkflowSteps
            .Include(ws => ws.FileItem)
            .FirstOrDefaultAsync(ws => ws.Id == id);

        if (step == null || step.IsApproved || step.IsRejected)
            return NotFound();

        step.IsApproved = true;
        step.IsRejected = false;
        step.ApprovedAt = DateTime.Now;
        step.Action = "موافقة";

        // حفظ التعديلات الأولية
        await _context.SaveChangesAsync();

        // تحقق من وجود أي رفض
        bool anyRejected = _context.WorkflowSteps
            .Any(ws => ws.FileItemId == step.FileItemId && ws.IsRejected);

        if (anyRejected)
        {
            step.FileItem.Status = "مرفوض";
            step.FileItem.SentToUserId = null;
        }
        else
        {
            // تحقق هل كل الخطوات تم الموافقة عليها
            bool allApproved = _context.WorkflowSteps
                .Where(ws => ws.FileItemId == step.FileItemId)
                .All(ws => ws.IsApproved);

            if (allApproved)
            {
                step.FileItem.Status = "موافق";
                step.FileItem.SentToUserId = null;
            }
            else
            {
                // تحديد المستخدم التالي في الترتيب
                var nextStep = await _context.WorkflowSteps
                    .Where(ws => ws.FileItemId == step.FileItemId && !ws.IsApproved && !ws.IsRejected)
                    .OrderBy(ws => ws.Order)
                    .FirstOrDefaultAsync();

                if (nextStep != null)
                {
                    step.FileItem.Status = "قيد الانتظار";
                    step.FileItem.SentToUserId = nextStep.UserId;
                }
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Inbox");
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id)
    {
        var step = await _context.WorkflowSteps
            .Include(ws => ws.FileItem)
            .FirstOrDefaultAsync(ws => ws.Id == id);

        if (step == null || step.IsApproved || step.IsRejected)
            return NotFound();

        step.IsApproved = false;
        step.IsRejected = true;
        step.ApprovedAt = DateTime.Now;
        step.Action = "رفض";

        step.FileItem.Status = "مرفوض";
        step.FileItem.SentToUserId = null;

        await _context.SaveChangesAsync();
        return RedirectToAction("Inbox");
    }
}
