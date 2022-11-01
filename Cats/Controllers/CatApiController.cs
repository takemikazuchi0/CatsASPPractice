using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Cats.Controllers;

[ApiController]
[Route("[controller]")]
public class CatApiController : Controller
{
    private IMemoryCache _memoryCache;
    private static int _statusCode = 100;
    static readonly HttpClient Client = new HttpClient();

    public CatApiController(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    private async Task<byte[]> CacheGet(string url)
    {
        if (_memoryCache.TryGetValue(url, out byte[] value))
        {
            return value;
        }

        CacheSet(url, await DownloadImage(url));
        return await CacheGet(url);
    }

    private async Task CacheSet(string url, byte[] img)
    {
        await Task.Run(() => _memoryCache.Set(url, img, TimeSpan.FromSeconds(5)));
    }

    private async Task<byte[]> GetDefault(string path)
    {
        return await System.IO.File.ReadAllBytesAsync(path);
    }

    private async Task<byte[]> DownloadImage(string url)
    {
        var data = await new HttpClient().GetAsync(url);

        byte[] image = await data.Content.ReadAsByteArrayAsync();

        return image;
    }

    [HttpGet]
    [Route("ProcessUrl")]
    public async Task<FileContentResult> ProcessUrl(string url)
    {
        var result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        var contentType = "image/jpeg";

        if (!result)
        {
            return File(await GetDefault("./images/cat-error.jpg"), contentType);
        }

        try
        {
            HttpResponseMessage response = await Client.GetAsync(url);
            _statusCode = Convert.ToInt32(response.StatusCode);
        }
        catch
        {
            return File(await GetDefault("./images/cat-404.jpg"), contentType);
        }

        return File(await CacheGet($"https://http.cat/{_statusCode}.jpg"), contentType);
    }
}