using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

namespace AdministrationPlatformTesting.Infrastructure;

internal sealed class TestSession : ISession
{
    private readonly ConcurrentDictionary<string, byte[]> _store = new();

    public IEnumerable<string> Keys => _store.Keys;

    public string Id { get; } = Guid.NewGuid().ToString("N");

    public bool IsAvailable { get; } = true;

    public void Clear() => _store.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _store.TryRemove(key, out _);

    public void Set(string key, byte[] value) => _store[key] = value;

    public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
}
