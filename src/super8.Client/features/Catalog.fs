module super8.Client.Features.Catalog

open System
open System.Net.Http
open System.Net.Http.Json
open System.Text.Json.Serialization
open Bolero
open Bolero.Html
open Elmish

// TYPES

type BasicMovie = {
    id: int
    title: string
    description: string
    poster: string
    releaseDate: string
    voteAverage: float
}

type DetailedMovie = {
    adult: bool
    budget: int
    genres: string[]
    id: int
    description: string
    posterPath: string
    backdropPath: string
    releaseDate: string
    revenue: int
    title: string
    voteAverage: float
    recommendations: BasicMovie[]
}

type PaginatedMovies = {
    page: int
    results: BasicMovie[] 
    totalPages: int
    totalResults: int
}

type MoviesCategory
    = NowPlaying
    | Popular
    | TopRated
    | Upcoming

// EXTERNAL TYPES

type MovieItem [<JsonConstructor>] (adult, backdrop_path, genre_ids, id, original_language, original_title, overview, popularity, poster_path, release_date, title, video, vote_average, vote_count) =
    [<JsonInclude>] member val adult = adult
    [<JsonInclude>] member val backdrop_path = backdrop_path
    [<JsonInclude>] member val genre_ids = genre_ids
    [<JsonInclude>] member val id = id 
    [<JsonInclude>] member val original_language = original_language
    [<JsonInclude>] member val original_title = original_title
    [<JsonInclude>] member val overview = overview
    [<JsonInclude>] member val popularity = popularity
    [<JsonInclude>] member val poster_path = poster_path
    [<JsonInclude>] member val release_date = release_date
    [<JsonInclude>] member val title = title
    [<JsonInclude>] member val video = video
    [<JsonInclude>] member val vote_average = vote_average
    [<JsonInclude>] member val vote_count = vote_count 
        
type MoviesDateResponse [<JsonConstructor>] (maximum, minimum) =
    [<JsonInclude>] member val maximum = maximum
    [<JsonInclude>] member val minimum = minimum

type MoviesResponse [<JsonConstructor>] (
    dates,
    page,
    results,
    total_pages,
    total_results
) =
    [<JsonInclude>] member val dates = dates
    [<JsonInclude>] member val page = page
    [<JsonInclude>] member val results = results
    [<JsonInclude>] member val total_pages = total_pages
    [<JsonInclude>] member val total_results = total_results

type GenreResponse [<JsonConstructor>] (id, name) =
    [<JsonInclude>] member val id = id
    [<JsonInclude>] member val name = name

type DetailedMovieResponse [<JsonConstructor>] (adult, backdrop_path, belongs_to_collection, budget, genres, homepage, id, imdb_id, original_language, original_title, overview, popularity, poster_path, production_companies, production_countries, release_date, revenue, runtime, spoken_languages, status, tagline, title, video, vote_average, vote_count) =
    [<JsonInclude>] member val adult = adult
    [<JsonInclude>] member val backdrop_path = backdrop_path
    [<JsonInclude>] member val belongs_to_collection = belongs_to_collection
    [<JsonInclude>] member val budget = budget
    [<JsonInclude>] member val genres: GenreResponse[] = genres
    [<JsonInclude>] member val homepage = homepage
    [<JsonInclude>] member val id = id
    [<JsonInclude>] member val imdb_id = imdb_id
    [<JsonInclude>] member val original_language = original_language
    [<JsonInclude>] member val original_title = original_title
    [<JsonInclude>] member val overview = overview
    [<JsonInclude>] member val popularity = popularity
    [<JsonInclude>] member val poster_path = poster_path
    [<JsonInclude>] member val production_companies = production_companies
    [<JsonInclude>] member val production_countries = production_countries
    [<JsonInclude>] member val release_date = release_date
    [<JsonInclude>] member val revenue = revenue
    [<JsonInclude>] member val runtime = runtime
    [<JsonInclude>] member val spoken_languages = spoken_languages
    [<JsonInclude>] member val status = status
    [<JsonInclude>] member val tagline = tagline
    [<JsonInclude>] member val title = title
    [<JsonInclude>] member val video = video
    [<JsonInclude>] member val vote_average = vote_average
    [<JsonInclude>] member val vote_count = vote_count

type CompoundResponse = {
    detailedResponse: DetailedMovieResponse
    recommendationsResponse: MoviesResponse
}
  
let private categoryToString (category: MoviesCategory) =
    match category with
        | NowPlaying -> "now_playing"
        | Popular -> "popular"
        | TopRated -> "top_rated"
        | Upcoming -> "upcoming"

// COMMANDS

// let private releaseDate  =
//     match releaseDate.ToString().Split("-") with
//     | [|year; month; day|] -> day.ToString() + "/" + month.ToString() + "/" + year.ToString()
//     | _format -> "-"

let private itemToMovie (item: MovieItem) =
    let releaseDate =
        match item.release_date.ToString().Split("-") with
        | [|year; month; day|] -> day.ToString() + "/" + month.ToString() + "/" + year.ToString()
        | _format -> "-"
    {
        id = item.id
        title = item.title
        description = item.overview
        poster = item.poster_path
        releaseDate = releaseDate
        voteAverage = item.vote_average
    }
 
let private fetchMovies (http: HttpClient) (category: MoviesCategory) (nextPage: int) (toMsg: PaginatedMovies -> 'msg) : Cmd<'msg> =
    let http () =
        let option = categoryToString category
        let url = "https://api.themoviedb.org/3/movie/" + option + "?language=pt-BR&page=" + nextPage.ToString()
        async {
            let! response = http.GetFromJsonAsync<MoviesResponse>(url) |> Async.AwaitTask
            return response
        }
    let mapper (response: MoviesResponse) =
        toMsg {
            page = response.page
            results = Array.map itemToMovie response.results
            totalPages = response.total_pages
            totalResults = response.total_results 
        }
    Cmd.OfAsync.perform http () mapper

let fetchNowPlaying (http: HttpClient) (nextPage: int) (toMsg: PaginatedMovies -> 'msg) : Cmd<'msg> =
    fetchMovies http NowPlaying nextPage toMsg
    
let fetchPopular (http: HttpClient) (nextPage: int) (toMsg: PaginatedMovies -> 'msg) : Cmd<'msg> =
    fetchMovies http Popular nextPage toMsg
    
let fetchTopRated (http: HttpClient) (nextPage: int) (toMsg: PaginatedMovies -> 'msg) : Cmd<'msg> =
    fetchMovies http TopRated nextPage toMsg
    
let fetchUpcoming (http: HttpClient) (nextPage: int) (toMsg: PaginatedMovies -> 'msg) : Cmd<'msg> =
    fetchMovies http Upcoming nextPage toMsg

let responseToGenre (response: GenreResponse) : string =
    response.name.ToString()

let responseToMovie (response: CompoundResponse) : DetailedMovie =
    let detailedResponse = response.detailedResponse
    let releaseDate =
        match detailedResponse.release_date.ToString().Split("-") with
        | [|year; month; day|] -> day.ToString() + "/" + month.ToString() + "/" + year.ToString()
        | _format -> "-"
    {
        adult = detailedResponse.adult
        budget = detailedResponse.budget
        genres = Array.map responseToGenre detailedResponse.genres 
        id = detailedResponse.id
        description =  detailedResponse.overview
        posterPath = detailedResponse.poster_path
        backdropPath = detailedResponse.backdrop_path
        releaseDate = releaseDate
        revenue = detailedResponse.revenue
        title = detailedResponse.title
        voteAverage = detailedResponse.vote_average
        recommendations = Array.map itemToMovie response.recommendationsResponse.results
    }
    
let fetchDetailedMovie (http: HttpClient) (movieId: int) (toMsg: DetailedMovie -> 'msg) : Cmd<'msg> =
    let http () =
        let url = "https://api.themoviedb.org/3/movie/" + movieId.ToString() + "?language=pt-BR"
        let recommendationsUrl = "https://api.themoviedb.org/3/movie/" + movieId.ToString() + "/recommendations?language=pt-BR"
        async {
            let! response = http.GetFromJsonAsync<DetailedMovieResponse>(url) |> Async.AwaitTask
            let! recommendationsResponse = http.GetFromJsonAsync<MoviesResponse>(recommendationsUrl) |> Async.AwaitTask
            return { detailedResponse = response; recommendationsResponse = recommendationsResponse }
        }
    let mapper (response: CompoundResponse) =
        response
        |> responseToMovie
        |> toMsg
    Cmd.OfAsync.perform http () mapper
    
// VIEW
    
let featuredCard (movie: BasicMovie) (path: string) =
    a {
        attr.``class`` "w-full h-full lg:rounded-xl"
        attr.href (path + "/" + movie.id.ToString())
        
        div {
            attr.``class`` "relative"
            
            span {
                attr.``class`` "absolute top-0 left-0 m-4 px-2 md:px-4 md:py-2 border-2 border-rose-500 bg-neutral-700 rounded-full text-neutral-100 text-xl md:text-3xl font-bold font-sans"
                movie.voteAverage.ToString("0.0").Replace(".", ",")
            }
            
            img {
                attr.``class`` "w-full lg:rounded-xl"
                attr.src ("https://image.tmdb.org/t/p/original" + movie.poster)
                attr.alt ("Pôster do filme" <> movie.title)
            }
        }
    }
    
let featuredDetails (movie: BasicMovie) (path: string) =
    div {
        attr.``class`` "flex flex-col justify-start items-start ml-4"
        
        a {
            attr.``class`` "hover:text-stone-500"
            attr.href (path + "/" + movie.id.ToString())
            
            span {
                attr.``class`` "font-bold text-4xl"
                movie.title
            }
        }
        
        match movie.description with
        | "" ->
            empty()
        | description ->
            p {
                attr.``class`` "mt-4 text-2xl"
            
                span {
                    attr.``class`` "font-bold"
                    "Sinopse: "
                }
                
                description
            }
    }

let card (movie: BasicMovie) (path: string) =
    a {
        attr.``class`` "flex justify-start bg-gray-100 shadow rounded-xl w-36 md:w-48 h-52 md:h-72"
        attr.href (path + "/" + movie.id.ToString())
        
        div {
            attr.``class`` "relative"
            
            span {
                attr.``class`` "absolute top-0 left-0 ml-2 mt-2 px-2 md:px-3 md:py-1 border-2 border-rose-500 bg-neutral-700 rounded-full text-neutral-100 text-xl md:text-2xl font-bold font-sans"
                movie.voteAverage.ToString("0.0").Replace(".", ",")
            }
            
            img {
                attr.``class`` "w-36 md:w-48 h-52 md:h-72 rounded-xl"
                attr.src ("https://image.tmdb.org/t/p/original" + movie.poster)
                attr.alt ("Pôster do filme" <> movie.title)
            }
        }
    }