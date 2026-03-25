using Microsoft.Extensions.Caching.Memory;

namespace PMSS.API.Services;

public class TemporaryDownloadStore(IMemoryCache cache)
{
    private const int DefaultTtlMinutes = 60;

    public string Save(byte[] data, string contentType, string fileName, TimeSpan? ttl = null)
    {
        var token = Guid.NewGuid().ToString("N");
        var expiresIn = ttl ?? TimeSpan.FromMinutes(DefaultTtlMinutes);

        cache.Set(token, new StoredGeneratedFile(data, contentType, fileName), expiresIn);
        return token;
    }

    public bool TryGet(string token, out StoredGeneratedFile? file)
    {
        if (cache.TryGetValue(token, out StoredGeneratedFile? stored))
        {
            file = stored;
            return true;
        }

        file = null;
        return false;
    }
}

public sealed record StoredGeneratedFile(byte[] Data, string ContentType, string FileName);
