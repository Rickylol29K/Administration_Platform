using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AdministrationPlatformTesting.Infrastructure;

internal sealed class TestTempDataProvider : ITempDataProvider
{
    private static readonly object StorageKey = new();

    public IDictionary<string, object> LoadTempData(HttpContext context)
    {
        if (context.Items.TryGetValue(StorageKey, out var stored) && stored is IDictionary<string, object> data)
        {
            return new Dictionary<string, object>(data);
        }

        return new Dictionary<string, object>();
    }

    public void SaveTempData(HttpContext context, IDictionary<string, object> values)
    {
        context.Items[StorageKey] = new Dictionary<string, object>(values);
    }
}
