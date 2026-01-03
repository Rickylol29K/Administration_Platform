using AdministrationPlat.Pages;
using AdministrationPlatformTesting.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Pages;

[TestClass]
public class MiscPageTests
{
    [TestMethod]
    public void ErrorModel_OnGet_SetsRequestId()
    {
        var logger = new NullLogger<ErrorModel>();
        var page = new ErrorModel(logger);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session);
        context.TraceIdentifier = "trace-123";
        PageModelTestHelper.AttachPageContext(page, context);

        page.OnGet();

        Assert.AreEqual("trace-123", page.RequestId);
        Assert.IsTrue(page.ShowRequestId);
    }

    [TestMethod]
    public void PrivacyModel_OnGet_DoesNotThrow()
    {
        var logger = new NullLogger<PrivacyModel>();
        var page = new PrivacyModel(logger);

        page.OnGet();
    }
}
