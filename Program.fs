module GiraffeSample.App

open System
open System.IO
open System.Collections.Generic
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe.HttpContextExtensions
open Giraffe.HttpHandlers
open Giraffe.Middleware
open Giraffe.Razor.HttpHandlers
open Giraffe.Razor.Middleware
open Giraffe.Tasks
open GiraffeSample.Models

open LunchTypes
open DataAccess

let handleLunchFilter (next: HttpFunc) (ctx: HttpContext) =
    let filter = ctx.BindQueryString<LunchFilter>()
    let logger = ctx.GetLogger()
    if filter.VegetarianOptions.IsSome then
        logger.LogInformation <| filter.VegetarianOptions.ToString()
    else
        logger.LogInformation "No filter"

    let lunchSpots = LunchAccess.getLunches logger.LogInformation filter
    json lunchSpots next ctx

let handleAddLunch (next: HttpFunc) (ctx: HttpContext) =
    task {
        let! lunch = ctx.BindJson<LunchSpot>()
        LunchAccess.addLunch lunch
        return! text (sprintf "Added %s to the lunch spots." lunch.Name) next ctx
    }

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> razorHtmlView "Index" { Text = "Hello world, from Giraffe!" }
                route "/hello-json" >=> json [| "Hello,"; "use"; "JSON!" |] // An F# array of strings.
                route "/goodbye-xml" >=> xml "Goodbye, XML!"
                route "/lunch" >=> handleLunchFilter
            ]
        POST >=>
            route "/lunch/add" >=> handleAddLunch
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler errorHandler
    app.UseStaticFiles() |> ignore
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    let sp  = services.BuildServiceProvider()
    let env = sp.GetService<IHostingEnvironment>()
    let viewsFolderPath = Path.Combine(env.ContentRootPath, "Views")
    services.AddRazorEngine viewsFolderPath |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l >= LogLevel.Information
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main argv =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0