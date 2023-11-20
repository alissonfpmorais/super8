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
            img {
                attr.``class`` ""
                attr.src ""
                attr.alt ""
            }
            
            span {
                movie.title
            }
            
            div {
                span {
                    movie.voteAverage.ToString("0.0").Replace(".", ",")
                }
                
                span {
                    movie.releaseDate
                }
            }
            
            span {
                "Sinopse"
            }
            
            p {
                movie.description
            }
            
            ul {
                forEach movie.recommendations (toCard model.detailedMoviePath)
            }
        }

