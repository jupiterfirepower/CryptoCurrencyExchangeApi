// Learn more about F# at http://fsharp.org

open System
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open CryptCurrency.Common.DataContracts
open CryptCurrency.BitFinex.WebApi
open CryptCurrency.BitFinex.Model
open CryptCurrency.Common.Retry

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"

    let api = BitFinexApi("11","222", 1)
    try
        let lends = api.GetLends("btc")
        printfn "%A" lends
    with
     | _ as e -> printfn "%A" e
    try
        let lands = (api :> IBitFinexApi).AsyncGetLends("btc") |> Async.RunSynchronously
        printfn "%A" lands
    with
     | _ as e -> printfn "%A" e
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code
