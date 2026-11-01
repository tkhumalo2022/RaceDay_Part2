using System.ComponentModel.DataAnnotations;
using RaCeDay.Portal.Models;

namespace RaCeDay.Tests;

public class PortalResultFormTests
{
    private static List<ValidationResult> Errors(ResultForm form)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), errors, true);
        return errors;
    }

    [Fact]
    public void FinishedResultRequiresTimesAndPosition()
    {
        var errors = Errors(new ResultForm { ResultStatus = "Finished" });
        Assert.Contains(errors, e => e.ErrorMessage!.Contains("finisher", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AFinishEarlierThanStartIsRejected()
    {
        var errors = Errors(new ResultForm
        {
            ResultStatus = "Finished",
            StartTime = new TimeOnly(11, 0),
            FinishTime = new TimeOnly(10, 30),
            OverallPosition = 1
        });
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ResultForm.FinishTime)));
    }

    [Fact]
    public void NonFinisherMayHaveNoTimesOrPosition()
    {
        Assert.Empty(Errors(new ResultForm { ResultStatus = "DNS" }));
    }

    [Fact]
    public void ValidFinishedResultPassesValidation()
    {
        Assert.Empty(Errors(new ResultForm
        {
            ResultStatus = "Finished",
            StartTime = new TimeOnly(8, 0),
            FinishTime = new TimeOnly(9, 15),
            OverallPosition = 4
        }));
    }
}
