module super8.Client.Main

open System.Net.Http
open Microsoft.AspNetCore.Components
open Elmish
open Bolero
open Bolero.Html

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home

/// The Elmish application's model.
type Model =
    {
        page: Page
    }

let initModel =
    {
        page = Home
    }

/// The Elmish application's update messages.
type Message =
    | SetPage of Page

let update (_http: HttpClient) message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let homePage _model _dispatch =
    Main.Home().Elt()

let view model dispatch =
    Main()
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

    override this.Program =
        let update = update this.HttpClient
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
