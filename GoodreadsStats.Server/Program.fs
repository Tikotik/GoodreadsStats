﻿open GoodreadsApi
open Utils
open Suave
open Suave.Filters
open Suave.Operators
open FSharp.Configuration
open BasicStatsCalculator
open GoodreadsStats.Model

type Settings = AppSettings< "app.config" >

let clientKey = Settings.ClientKey
let clientSecret = Settings.ClientSecret
let clientSideUrl = "http://localhost:1234"

let authorized token tokenSecret = 
    let (token, tokenSecret) = getAccessToken clientKey clientSecret token tokenSecret
    let accessData = getAccessData clientKey clientSecret token tokenSecret
    let user = getUser accessData
    json { AccessToken = token; AccessTokenSecret = tokenSecret; UserName = user.Name }

let reviews accessData = 
    let user = getUser accessData
    getAllReviews accessData user.Id "read" "date_read"

let createBook (r : Reviews.Review) = 
    { ReadAt = parseOptionDate r.ReadAt
      StartedAt = parseOptionDate r.StartedAt
      NumPages = r.Book.NumPages
      Book = 
          { Title = r.Book.Title
            Author = r.Book.Author.Name } }

let basicStats token tokenSecret = 
    let accessData = getAccessData clientKey clientSecret token tokenSecret
    let reviews = reviews accessData
    
    let readBooks = 
        reviews
        |> Seq.map createBook
        |> Seq.toArray
    json (basicStats readBooks)

let setCORSHeaders = setCORSHeaders clientSideUrl
let requestWithTokenParams f = request (processRequestWithTokenParams f)

let authorizationUrlRequest request = 
    let (authorizationUrl, token, tokenSecret) = getAuthorizationData clientKey clientSecret clientSideUrl
    json { Token = token; TokenSecret = tokenSecret; Url = authorizationUrl}

let webPart = 
    choose [ GET >=> choose [ path "/authorizationUrl" >=> setCORSHeaders >=> request authorizationUrlRequest
                              pathStarts "/authorized" >=> setCORSHeaders >=> requestWithTokenParams authorized
                              pathStarts "/basicStats" >=> setCORSHeaders >=> requestWithTokenParams basicStats ] ]

startWebServer defaultConfig webPart
