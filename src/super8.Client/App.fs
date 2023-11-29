module super8.Client.App

open System
open System.Net.Http
open Microsoft.AspNetCore.Components
open Elmish
open Bolero
open Bolero.Html
open super8.Client

type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/movie/{id}">] MovieDetails of id: int

type Model =
    {
        page: Page
        homeModel: Pages.Home.Model
        movieDetailsModel: Option<Pages.MovieDetails.Model>
    }

type Message =
    | SetPage of Page
    | HomeMessages of Pages.Home.Message
    | MovieDetailsMessages of Pages.MovieDetails.Message

let router = Router.infer SetPage (fun model -> model.page)

let init (http: HttpClient) _ =
    let homeModel, homeCmds = Pages.Home.init http "movie"
    
    {
        page = Home
        homeModel = homeModel
        movieDetailsModel = None 
    },
    Cmd.map HomeMessages homeCmds

let changePage (http: HttpClient) (page: Page) (model: Model) =
    match page with
    | Home ->
        let homeModel, homeCmds = Pages.Home.init http "movie"
        {
            model with
                page = page
                homeModel = homeModel
        },
        Cmd.map HomeMessages homeCmds
    | MovieDetails movieId ->
        let movieDetailsModel, movieDetailsCmds = Pages.MovieDetails.init http movieId "movie"
        {
            model with
                page = page
                movieDetailsModel = Some movieDetailsModel 
        },
        Cmd.map MovieDetailsMessages movieDetailsCmds

let update (http: HttpClient) (message: Message) (model: Model) =
    match message with
    | SetPage page ->
        changePage http page model
    | HomeMessages message ->
        let nextHomeModel, homeCmds = Pages.Home.update http message model.homeModel
        { model with homeModel = nextHomeModel }, Cmd.map HomeMessages homeCmds
    | MovieDetailsMessages message ->
        match model.movieDetailsModel with
        | None ->
            model, Cmd.none
        | Some movieDetailsModel ->
            let nextMovieDetailsModel, movieDetailsCmds = Pages.MovieDetails.update http message movieDetailsModel
            { model with movieDetailsModel = Some nextMovieDetailsModel }, Cmd.map MovieDetailsMessages movieDetailsCmds

let view (model: Model) dispatch =
    concat {
        header {
            attr.id "page-header"
            attr.``class`` "sticky top-0 flex justify-start items-center w-full h-16 md:h-28 bg-stone-800 drop-shadow-lg z-10"
            
            a {
                attr.``class`` "ml-4 font-medium text-neutral-100 text-2xl md:text-4xl"
                attr.href "./"
                
                "Super"
                
                span {
                    attr.``class`` "font-bold text-rose-500 text-2xl md:text-4xl"
                    "8"   
                }
            }
        }
        
        main {
            attr.id "page-content"
            
            cond model.page <| function
            | Home -> Pages.Home.view model.homeModel (fun homeMsg -> dispatch <| HomeMessages homeMsg)
            | MovieDetails _movieId->
                match model.movieDetailsModel with
                | None ->
                    empty()
                | Some movieDetailsModel ->
                    Pages.MovieDetails.view movieDetailsModel (fun movieDetailsMsg -> dispatch <| MovieDetailsMessages movieDetailsMsg)
        }
        
        footer {
            attr.id "page-footer"
        }
    }

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

    override this.Program =
        let init = init this.HttpClient
        let update = update this.HttpClient
        Program.mkProgram init update view
        |> Program.withRouter router
