#r "node_modules/fable-powerpack/Fable.PowerPack.dll"

#load "Model.fsx"
#load "Actions.fsx"
#load "Utils.fsx"
#load "Components.fsx"
#load "Reducer.fsx"
#load "Fable.Import.Global.fsx"

open Fable.Import.Global
open Utils
open Model
open Actions
open Fable.PowerPack

let booksPerPage = 10.0

let downloadReadBooksOnPage accessToken accessTokenSecret (callback: ReadBook[] -> unit) page =
    let url = completeUrl "readBooks" (sprintf "?token=%s&tokenSecret=%s&perPage=%i&page=%i" accessToken accessTokenSecret (int booksPerPage) page)
    fetchAsJson url (Fable.Core.JsInterop.ofJson >> callback)

let downloadBooksDetailsOnPage accessToken accessTokenSecret (callback: BookDetail[] -> unit) page =
    let url = completeUrl "readBooksDetails" (sprintf "?token=%s&tokenSecret=%s&perPage=%i&page=%i" accessToken accessTokenSecret (int booksPerPage) page)
    fetchAsJson url (Fable.Core.JsInterop.ofJson >> callback)
 
let pagesCount booksCount = booksCount / booksPerPage |> System.Math.Ceiling |> int

let startDownloadReadBooks accessToken accessTokenSecret callback detailsCallback=
    let getReadBooks booksCount = 
        [1..(pagesCount booksCount)] |> Seq.iter (downloadReadBooksOnPage accessToken accessTokenSecret callback)
        [1..(pagesCount booksCount)] |> Seq.iter (downloadBooksDetailsOnPage accessToken accessTokenSecret detailsCallback)
    let readBooksCountUrl = completeUrlWithToken "readBooksCount" accessToken accessTokenSecret
    fetchAsJson readBooksCountUrl (float >> getReadBooks)