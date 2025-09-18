using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class SignInController : Controller
    {
        private readonly IMiniGameAdminService _adminService;

        public SignInController(IMiniGameAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index(SignInQueryModel query)
        {
            var signInStats = await _adminService.GetSignInStatsAsync(query);
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminSignInIndexViewModel
            {
                SignInStats = signInStats,
                Users = users,
                SearchTerm = query.SearchTerm,
                Page = 1,
                PageSize = 10,
                TotalCount = signInStats.Count,
                TotalPages = 1
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Rules()
        {
            var rule = await _adminService.GetSignInRuleAsync();
            return View(rule);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRule(SignInRuleUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _adminService.UpdateSignInRuleAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "簽到規則更新成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "簽到規則更新失敗";
                }
            }
            return RedirectToAction("Rules");
        }

        [HttpPost]
        public async Task<IActionResult> AddSignIn(int userId, DateTime signInDate)
        {
            var result = await _adminService.AddUserSignInRecordAsync(userId, signInDate);
            if (result)
            {
                TempData["SuccessMessage"] = "簽到記錄添加成功";
            }
            else
            {
                TempData["ErrorMessage"] = "簽到記錄添加失敗";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSignIn(int userId, DateTime signInDate)
        {
            var result = await _adminService.RemoveUserSignInRecordAsync(userId, signInDate);
            if (result)
            {
                TempData["SuccessMessage"] = "簽到記錄刪除成功";
            }
            else
            {
                TempData["ErrorMessage"] = "簽到記錄刪除失敗";
            }
            return RedirectToAction("Index");
        }
    }
}
