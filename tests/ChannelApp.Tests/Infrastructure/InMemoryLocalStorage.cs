using Blazored.LocalStorage;

namespace ChannelApp.Tests.Infrastructure;

public class InMemoryLocalStorage : ILocalStorageService
{
    private readonly Dictionary<string, object?> _store = [];

    #pragma warning disable CS0067 // Events are required by interface but unused in tests
    public event EventHandler<ChangingEventArgs>? Changing;
    public event EventHandler<ChangedEventArgs>? Changed;
    #pragma warning restore CS0067

    public ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(key, out var value) && value is T typed)
        {
            return new ValueTask<T?>(typed);
        }
        return new ValueTask<T?>(default(T));
    }

    public ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = default)
    {
        _store[key] = data;
        return ValueTask.CompletedTask;
    }

    public ValueTask<string?> GetItemAsStringAsync(string key, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public ValueTask SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public ValueTask RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public ValueTask ClearAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public ValueTask<int> LengthAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public ValueTask<string?> KeyAsync(int index, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public ValueTask<IEnumerable<string>> KeysAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
