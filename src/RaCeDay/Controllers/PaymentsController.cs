using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Models;
using RaCeDay.Services;

namespace RaCeDay.Controllers;

[Authorize, AutoValidateAntiforgeryToken]
public class PaymentsController(RaceWorkflow workflow) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int enrolmentId)
    {
        ViewData["EnrolmentId"] = enrolmentId;
        ViewData["CanAddPayment"] = User.IsInRole("Participant");
        return View(await workflow.Payments(User, enrolmentId));
    }

    [HttpGet, Authorize(Roles = "Participant")]
    public async Task<IActionResult> Create(int enrolmentId)
    {
        var amount = await workflow.ExpectedPaymentAmount(User, enrolmentId);
        ViewData["EnrolmentId"] = enrolmentId;
        return View(new PaymentInput { Amount = amount });
    }

    [HttpPost, Authorize(Roles = "Participant")]
    public async Task<IActionResult> Create(int enrolmentId, PaymentInput model)
    {
        ViewData["EnrolmentId"] = enrolmentId;
        if (!ModelState.IsValid) return View(model);
        await workflow.RecordPayment(User, enrolmentId, model);
        return RedirectToAction(nameof(Index), new { enrolmentId });
    }
}
