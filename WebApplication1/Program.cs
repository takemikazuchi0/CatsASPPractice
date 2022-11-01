using Microsoft.AspNetCore.Mvc.Rendering;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
HttpClient Client = new HttpClient();
int statusCode = 404;

app.Run(async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";

    if (context.Request.Path == "/catsAPI")
    {
        var form = context.Request.Form;
        string urlString = form["urlString"];
        var result = Uri.TryCreate(urlString, UriKind.Absolute, out var uriResult)
                            &&(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (!result)
        {
            //TODO: implement bitch
        }
        
        try
        {
            HttpResponseMessage response = await Client.GetAsync(urlString);
           statusCode = Convert.ToInt32(response.StatusCode);
        }
        catch
        {
            //TODO: implement bitch
        }
        await context.Response.WriteAsync($"<h2>{urlString}</h2>"+
                                          $"<img src=\"https://http.cat/{statusCode}.jpg\"/>"+
                                          "<input type=\"button\" onclick=\"history.back();\" value=\"Назад\"/>");
    }
    else
    {
        await context.Response.SendFileAsync("index.html");
    }
});

app.Run();