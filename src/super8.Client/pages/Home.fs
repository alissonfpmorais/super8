module super8.Client.Pages.Home

open System
open System.Net.Http
open System.Security.Cryptography
open Bolero
open Bolero.Html
open Elmish
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open super8.Client.Features

// MODEL

type MovieSection = {
    loading: bool
    showcasing: Catalog.BasicMovie[]
    featuredMovie: Option<Catalog.BasicMovie>
    currentPage: int
    lastPage: Option<int>
}

type Model = {
    detailedMoviePath: string
    nowPlaying: MovieSection
    popular: MovieSection
    topRated: MovieSection
    upcoming: MovieSection
}

// INIT

type Message
    = GotNowPlaying of Catalog.PaginatedMovies
    | GotPopular of Catalog.PaginatedMovies
    | GotTopRated of Catalog.PaginatedMovies
    | GotUpcoming of Catalog.PaginatedMovies
    | FetchMovies of Catalog.MoviesCategory

let private defaultMovieSection = {
    loading = true
    showcasing = [||]
    featuredMovie = None
    currentPage = 0
    lastPage = None
}

let init (http: HttpClient) (detailedMoviePath: string) =
    {
        detailedMoviePath = detailedMoviePath
        nowPlaying = defaultMovieSection
        popular = defaultMovieSection
        topRated = defaultMovieSection
        upcoming = defaultMovieSection 
    },
    Cmd.batch [|
        Catalog.fetchNowPlaying http (defaultMovieSection.currentPage + 1) GotNowPlaying
        Catalog.fetchPopular http (defaultMovieSection.currentPage + 1) GotPopular
        Catalog.fetchTopRated http (defaultMovieSection.currentPage + 1) GotTopRated
        Catalog.fetchUpcoming http (defaultMovieSection.currentPage + 1) GotUpcoming
    |]

// UPDATE
   
let private updateMovieSection (movieSection: MovieSection) (paginatedMovies: Catalog.PaginatedMovies) =
    if movieSection.currentPage = paginatedMovies.page then
        movieSection
    else
        let showcasing = Array.concat [|
            movieSection.showcasing
            paginatedMovies.results
        |]
        {
            loading = false
            showcasing = showcasing 
            featuredMovie =
                match movieSection.featuredMovie with
                | None ->
                    showcasing.Length
                    |> RandomNumberGenerator.GetInt32
                    |> Array.get showcasing
                    |> Some
                | movie ->
                    movie
            currentPage = paginatedMovies.page
            lastPage = Some paginatedMovies.totalPages 
        }
    
let update (http: HttpClient) (message: Message) (model: Model) =
    match message with
    | GotNowPlaying paginatedMovies ->
        { model with
            nowPlaying = updateMovieSection model.nowPlaying paginatedMovies 
        },
        Cmd.none
        
    | GotPopular paginatedMovies ->
        { model with
            popular = updateMovieSection model.popular paginatedMovies 
        },
        Cmd.none
        
    | GotTopRated paginatedMovies ->
        { model with
            topRated = updateMovieSection model.topRated paginatedMovies 
        },
        Cmd.none
        
    | GotUpcoming paginatedMovies ->
        { model with
            upcoming = updateMovieSection model.upcoming paginatedMovies 
        },
        Cmd.none
    | FetchMovies Catalog.MoviesCategory.NowPlaying ->
        { model with nowPlaying = { model.nowPlaying with loading = true } },
        Catalog.fetchNowPlaying http (model.nowPlaying.currentPage + 1) GotNowPlaying
    | FetchMovies Catalog.MoviesCategory.Popular ->
        { model with popular = { model.popular with loading = true } },
        Catalog.fetchPopular http (model.popular.currentPage + 1) GotPopular
    | FetchMovies Catalog.MoviesCategory.TopRated ->
        { model with topRated = { model.topRated with loading = true } },
        Catalog.fetchTopRated http (model.topRated.currentPage + 1) GotTopRated
    | FetchMovies Catalog.MoviesCategory.Upcoming ->
        { model with upcoming = { model.upcoming with loading = true } },
        Catalog.fetchUpcoming http (model.upcoming.currentPage + 1) GotUpcoming

// VIEW

type private MoviesArea = {
    title: string
    movieSection: MovieSection
    movieCategory: Catalog.MoviesCategory
    path: string
    dispatch: Dispatch<Message>
}

let toCard (path: string) (movie: Catalog.BasicMovie) =
    let card = Catalog.card movie path
    li { card }

let private toMoviesArea (moviesArea: MoviesArea) =
    let hasMovies = moviesArea.movieSection.showcasing.Length > 0
    
    cond hasMovies <| function
    | false ->
        div {
            attr.``class`` "w-full h-screen"
        }
    | true ->
        section {
            attr.``class`` "grid grid-flow-row grid-cols-1 gap-4 first:mt-4 mt-8 mb-4"
            
            div {
                attr.``class`` "flex items-center justify-start px-4 uppercase font-bold"
                
                div {
                    attr.``class`` "border-b-2 border-rose-500 border-dashed"
                    
                    span {
                        attr.``class`` "text-rose-500 mr-2"   
                        "▷"
                    }
                    
                    span {
                        attr.``class`` "text-center"
                        moviesArea.title
                    }
                }
            }
            
            picture {
                cond moviesArea.movieSection.featuredMovie <| function
                | None ->
                    div {
                        attr.``class`` "w-full h-80"
                    }
                | Some movie ->
                    Catalog.featuredCard movie moviesArea.path
            }
            
            ul {
                attr.``class`` "grid grid-flow-col auto-cols-max gap-4 px-4 pb-4 overflow-x-scroll"
                forEach moviesArea.movieSection.showcasing (toCard moviesArea.path)
                
                li {
                    button {
                        attr.``class`` "flex flex-col justify-center items-center w-36 h-52"
                        attr.disabled moviesArea.movieSection.loading
                        
                        on.click (fun _event -> moviesArea.dispatch <| FetchMovies moviesArea.movieCategory)
                        
                        cond moviesArea.movieSection.loading <| function
                        | false ->
                            span {
                                attr.``class`` "p-4 bg-rose-500 rounded-full text-neutral-100 font-bold uppercase"
                                "Carregar"
                            }
                        | true ->
                            span {
                                attr.``class`` "p-4 bg-stone-500 rounded-full text-neutral-100 font-bold uppercase animate-pulse"
                                "Aguarde"
                            }
                    }
                }
            }
        }

let view (model: Model) (dispatch: Dispatch<Message>) =
    let moviesAreas = [|
        {
            title = "Em cartaz"
            movieSection = model.nowPlaying
            movieCategory = Catalog.MoviesCategory.NowPlaying
            path = model.detailedMoviePath
            dispatch = dispatch
        }
        {
            title = "Populares"
            movieSection = model.popular
            movieCategory = Catalog.MoviesCategory.Popular
            path = model.detailedMoviePath
            dispatch = dispatch
        }
        {
            title = "Mais bem avaliados"
            movieSection = model.topRated
            movieCategory = Catalog.MoviesCategory.TopRated
            path = model.detailedMoviePath
            dispatch = dispatch
        }
        {
            title = "Próximos lançamentos"
            movieSection = model.upcoming
            movieCategory = Catalog.MoviesCategory.Upcoming
            path = model.detailedMoviePath
            dispatch = dispatch
        }
    |]
    div {
        attr.``class`` "pt-2"
        
        forEach moviesAreas toMoviesArea
    }