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
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJjMzdjNWI4OWMzNTdhMDA5NzY0MDZiZTYxZmQzNmFlZSIsInN1YiI6IjVhNjFjZGZjOTI1MTQxMGUyMzAxMmVkNCIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.oE5seX9_utdew0Ne6kWuGtuA7ihEcEMRqMoHnuBtcbk")
        
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<App.MyApp>("#main")
        builder.Services.AddScoped<HttpClient>(fun _ -> httpClient) |> ignore
        builder.Build().RunAsync() |> ignore
        0
