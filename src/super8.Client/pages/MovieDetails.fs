module super8.Client.Pages.MovieDetails

open System
open System.Net.Http
open System.Security.Cryptography
open Bolero
open Bolero.Html
open Elmish
open Microsoft.AspNetCore.Components
open super8.Client.Features

// MODEL

type Model = {
    detailedMoviePath: string
    movie: Option<Catalog.DetailedMovie>
}

// INIT

type Message
    = GotDetailedMovie of Catalog.DetailedMovie

let init (http: HttpClient) (movieId: int) (detailedMoviePath: string) =
    {
        detailedMoviePath = detailedMoviePath
        movie = None
    },
    Catalog.fetchDetailedMovie http movieId GotDetailedMovie 

// UPDATE

let update (_http: HttpClient) (message: Message) (model: Model) =
    match message with
    | GotDetailedMovie movie -> { model with movie = Some movie }, Cmd.none

// VIEW

let toCard (path: string) (movie: Catalog.BasicMovie) =
    let card = Catalog.card movie path
    li { card }
    
let view (model: Model) _dispatch =
    match model.movie with
    | None ->
        empty()
    | Some movie ->
        div {
            div {
                attr.``class`` "flex justify-start items-center w-full h-40"
                attr.style ("background-image: url(https://image.tmdb.org/t/p/original" + movie.backdropPath + "); background-size: cover;")
                
                div {
                    attr.``class`` "px-2 py-2 w-32 h-36"
                    
                    img {
                        attr.``class`` "w-full h-full object-contain rounded-lg"
                        attr.src ("https://image.tmdb.org/t/p/original" + movie.posterPath)
                        attr.alt ("Poster do filme " + movie.title)
                    }
                }
            }
            
            div {
                attr.``class`` "grid grid-flow-row gap-4 px-4 mt-4 "
                
                div {
                    span {
                        attr.``class`` "font-bold text-2xl"
                        movie.title
                    }
                }
                
                div {
                    attr.``class`` "flex justify-between items-center"
                    
                    span {
                        attr.``class`` "px-2 border-2 border-rose-500 bg-neutral-700 rounded-full text-neutral-100 text-xl font-bold font-sans"
                        movie.voteAverage.ToString("0.0").Replace(".", ",")
                    }
                    
                    span {
                        attr.``class`` "font-bold"
                        movie.releaseDate
                    }
                }
                
                match movie.description with
                | "" ->
                    empty()
                | description ->
                    p {
                        span {
                            attr.``class`` "font-bold"
                            
                            "Sinopse: "
                        }
                        
                        description
                    }
            }
            
            div {
                attr.``class`` "mt-4"
                
                span {
                    attr.``class`` "px-4 font-bold"
                    "Recomendações"
                }
                
                ul {
                    attr.``class`` "grid grid-flow-col auto-cols-max gap-4 mt-2 px-4 pb-4 overflow-x-scroll"
                    forEach movie.recommendations (toCard model.detailedMoviePath)
                }
            }
        }

