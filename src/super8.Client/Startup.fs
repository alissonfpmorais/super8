namespace super8.Client

open System.Net.Http.Headers
open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Microsoft.Extensions.DependencyInjection
open System
open System.Net.Http

module Program =

    [<EntryPoint>]
    let Main args =
        let httpClient = new HttpClient()
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer <token>")
        
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<App.MyApp>("#main")
        builder.Services.AddScoped<HttpClient>(fun _ -> httpClient) |> ignore
        builder.Build().RunAsync() |> ignore
        0
