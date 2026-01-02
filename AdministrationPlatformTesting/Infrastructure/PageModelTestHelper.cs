using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;

namespace AdministrationPlatformTesting.Infrastructure;

internal static class PageModelTestHelper
{
    public static DefaultHttpContext CreateHttpContextWithSession(ISession session, int? userId = null, bool isAdmin = false)
    {
        var context = new DefaultHttpContext();
        context.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });
        if (userId.HasValue)
        {
            context.Session.SetInt32("UserId", userId.Value);
        }

        context.Session.SetInt32("IsAdmin", isAdmin ? 1 : 0);
        return context;
    }

    public static void AttachPageContext(PageModel pageModel, HttpContext context)
    {
        pageModel.PageContext = new PageContext { HttpContext = context };
        pageModel.TempData = new TempDataDictionary(context, new TestTempDataProvider());
    }

    private sealed class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = new TestSession();
    }
}
