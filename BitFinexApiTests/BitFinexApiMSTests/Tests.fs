namespace BitFinexApiMSTests

open System
open System.Linq
open System.Threading
open System.Threading.Tasks
open Microsoft.VisualStudio.TestTools.UnitTesting
open CryptCurrency.Common.DataContracts
open CryptCurrency.BitFinex.WebApi
open CryptCurrency.BitFinex.Model

module WebApiKeys =
    [<Literal>]
    let apiKey = "key"
    [<Literal>]
    let apiSecret = "secret"

module MSTests =
    open WebApiKeys

    [<TestClass>]
    type TestClass () =
        let api = BitFinexApi(apiKey,apiSecret, 10000)

        [<TestMethod>]
        member this.WebApiKeyArgumentNullException () =
            try
                let apit = BitFinexApi(null,apiSecret, 8000)
                Assert.IsTrue(false)
            with | :? ArgumentNullException -> Assert.IsTrue(true)
                 | _ -> Assert.IsTrue(false)

        [<TestMethod>]
        member this.WebApiSecretArgumentNullException () =
            try
                let apit = BitFinexApi(apiKey,null, 8000)
                Assert.IsTrue(false)
            with | :? ArgumentNullException -> Assert.IsTrue(true)
                 | _ -> Assert.IsTrue(false)

        [<TestMethod>]
        member this.WebApiTimeoutArgumentException () =
            try
                let apit = BitFinexApi(apiKey,apiSecret, -1)
                Assert.IsTrue(false)
            with | :? ArgumentException -> Assert.IsTrue(true)
                 | _ -> Assert.IsTrue(false)

        [<TestMethod>]
        member this.WebApiTimeoutAllArgumentException () =
            try
                let apit = BitFinexApi(null,null, -1)
                Assert.IsTrue(false)
            with 
                 | :? ArgumentNullException -> Assert.IsTrue(true)
                 | :? ArgumentException -> Assert.IsTrue(true)
                 | _ -> Assert.IsTrue(false)

        [<TestMethod>]
        member this.GetSupportedPairs () =
            let pairs = (api :> IBitFinexApi).GetSupportedPairs()
            Assert.IsTrue(Seq.forall (fun (x : PairClass) -> x.BaseCurrency.Length = 3 && x.CounterCurrency.Length = 3) pairs)
            Assert.IsTrue(pairs |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.AsyncGetSupportedPairs () =
            let pairs = (api :> IBitFinexApi).AsyncGetSupportedPairs() |> Async.RunSynchronously
            Assert.IsTrue(Seq.forall (fun (x : PairClass) -> x.BaseCurrency.Length = 3 && x.CounterCurrency.Length = 3) pairs)
            Assert.IsTrue(pairs |> (not << Seq.isEmpty));
        
        [<TestMethod>]
        member this.GetSupportedPairsAsync () =
            let pairs = (api :> IBitFinexApi).GetSupportedPairsAsync() |> Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(Seq.forall (fun (x : PairClass) -> x.BaseCurrency.Length = 3 && x.CounterCurrency.Length = 3) pairs)
            Assert.IsTrue(pairs |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetSupportedPairsAsyncCancellationToken () =
            let cts = new CancellationTokenSource()
            let pairs = (api :> IBitFinexApi).GetSupportedPairsAsync(cts.Token) |> Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(Seq.forall (fun (x : PairClass) -> x.BaseCurrency.Length = 3 && x.CounterCurrency.Length = 3) pairs)
            Assert.IsTrue(pairs |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetSupportedPairsAsyncCancellationTokenCancel () =
            let cts = new CancellationTokenSource()
            let task = (api :> IBitFinexApi).GetSupportedPairsAsync(cts.Token) |> Async.AwaitTask
            cts.Cancel() 
            let pairs = task |> Async.RunSynchronously
            Assert.IsTrue(Seq.forall (fun (x : PairClass) -> x.BaseCurrency.Length = 3 && x.CounterCurrency.Length = 3) pairs)
            Assert.IsTrue(pairs |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetSupportedPairsAsyncCancellationTokenNone () =
            let pairs = (api :> IBitFinexApi).GetSupportedPairsAsync(CancellationToken.None) |> Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(Seq.forall (fun (x : PairClass) -> x.BaseCurrency.Length = 3 && x.CounterCurrency.Length = 3) pairs)
            Assert.IsTrue(pairs |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetSupportedPairsAsyncCancellationTokenOption () =
            let cts = new CancellationTokenSource()
            let pairs = (api :> IBitFinexApi).GetSupportedPairsAsync(Some cts.Token) |> Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(Seq.forall (fun (x : PairClass) -> x.BaseCurrency.Length = 3 && x.CounterCurrency.Length = 3) pairs)
            Assert.IsTrue(pairs |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetSupportedPairsAsyncCancellationTokenOptionCancel () =
            let cts = new CancellationTokenSource()
            let task = (api :> IBitFinexApi).GetSupportedPairsAsync(Some cts.Token) |> Async.AwaitTask 
            cts.Cancel() 
            let pairs = task |> Async.RunSynchronously
            Assert.IsTrue(Seq.forall (fun (x : PairClass) -> x.BaseCurrency.Length = 3 && x.CounterCurrency.Length = 3) pairs)
            Assert.IsTrue(pairs |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetSupportedPairsAsyncCancellationTokenOptionNone () =
            let pairs = (api :> IBitFinexApi).GetSupportedPairsAsync(None) |> Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(Seq.forall (fun (x : PairClass) -> x.BaseCurrency.Length = 3 && x.CounterCurrency.Length = 3) pairs)
            Assert.IsTrue(pairs |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetPairDetails () =
            let pairDetails = (api :> IBitFinexApi).GetPairDetails()
            Assert.IsTrue(pairDetails |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.AsyncGetPairDetails () =
            let pairDetails = (api :> IBitFinexApi).AsyncGetPairDetails() |>  Async.RunSynchronously
            Assert.IsTrue(pairDetails |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetPairDetailsAsync () =
            let pairDetails = (api :> IBitFinexApi).GetPairDetailsAsync() |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(pairDetails |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetPairDetailsAsyncCancellationToken () =
            let cts = new CancellationTokenSource()
            let pairDetails = (api :> IBitFinexApi).GetPairDetailsAsync(cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(pairDetails |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetPairDetailsAsyncCancellationTokenNone () =
            let api = BitFinexApi(apiKey,apiSecret, 10000)
            let pairDetails = (api :> IBitFinexApi).GetPairDetailsAsync(CancellationToken.None) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(pairDetails |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetPairDetailsAsyncCancellationTokenCancel () =
            let cts = new CancellationTokenSource()
            let task = (api :> IBitFinexApi).GetPairDetailsAsync(cts.Token) |> Async.AwaitTask
            cts.Cancel() 
            let pairDetails = task |> Async.RunSynchronously
            Assert.IsTrue(pairDetails |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetPairDetailsAsyncCancellationTokenOption () =
            let cts = new CancellationTokenSource()
            let pairDetails = (api :> IBitFinexApi).GetPairDetailsAsync(Some cts.Token) |> Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(pairDetails |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetPairDetailsAsyncCancellationTokenOptionCancel () =
            let cts = new CancellationTokenSource()
            let task = (api :> IBitFinexApi).GetPairDetailsAsync(Some cts.Token) |> Async.AwaitTask 
            cts.Cancel() 
            let pairDetails = task |> Async.RunSynchronously
            Assert.IsTrue(pairDetails |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetPairDetailsAsyncCancellationTokenOptionNone () =
            let pairDetails = (api :> IBitFinexApi).GetPairDetailsAsync(None) |> Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(pairDetails |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetTickers () =
            let ticker = (api :> IBitFinexApi).GetTickers(PairClass("btc","usd"))
            Assert.IsTrue(ticker <> BitFinexTicker.Default);
        
        [<TestMethod>]
        member this.AsyncGetTickers () =
            let ticker = (api :> IBitFinexApi).AsyncGetTickers(PairClass("btc","usd")) |>  Async.RunSynchronously
            Assert.IsTrue(ticker <> BitFinexTicker.Default);

        [<TestMethod>]
        member this.GetTickersAsync () =
            let ticker = (api :> IBitFinexApi).GetTickersAsync(PairClass("btc","usd")) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(ticker <> BitFinexTicker.Default);

        [<TestMethod>]
        member this.GetTickersAsyncCancellationToken () =
            let cts = new CancellationTokenSource()
            let ticker = (api :> IBitFinexApi).GetTickersAsync(PairClass("btc","usd"), cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(ticker <> BitFinexTicker.Default)

        [<TestMethod>]
        member this.GetTickersAsyncCancellationTokenCancel () =
            try
                let cts = new CancellationTokenSource()
                let task = (api :> IBitFinexApi).GetTickersAsync(PairClass("btc","usd"), cts.Token) |>  Async.AwaitTask
                cts.Cancel() 
                task |> Async.RunSynchronously |> ignore
                Assert.IsTrue(false)
            with
            | :? AggregateException as agx when (agx.InnerException :? TaskCanceledException) && agx.InnerException.Message.Contains("A task was canceled")  -> Assert.IsTrue(true)
            | _ -> Assert.IsTrue(false)

        [<TestMethod>]
        member this.GetTickersAsyncCancellationTokenOption () =
            let cts = new CancellationTokenSource()
            let ticker = (api :> IBitFinexApi).GetTickersAsync(PairClass("btc","usd"), Some cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(ticker <> BitFinexTicker.Default);

        [<TestMethod>]
        member this.GetTickersAsyncCancellationTokenOptionCancel () =
            try
                let cts = new CancellationTokenSource()
                let task = (api :> IBitFinexApi).GetTickersAsync(PairClass("btc","usd"), Some cts.Token) |>  Async.AwaitTask
                cts.Cancel() 
                task |> Async.RunSynchronously |> ignore
                Assert.IsTrue(false)
            with
                | :? AggregateException as agx when (agx.InnerException :? TaskCanceledException) && agx.InnerException.Message.Contains("A task was canceled") -> Assert.IsTrue(true)
                | _ -> Assert.IsTrue(false)

        [<TestMethod>]
        member this.GetTickersAsyncCancellationTokenOptionNone () =
            let ticker = (api :> IBitFinexApi).GetTickersAsync(PairClass("btc","usd"), None) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(ticker <> BitFinexTicker.Default);

        [<TestMethod>]
        member this.GetTickersNotFound () =
            try
                (api :> IBitFinexApi).GetTickers(PairClass("btc","mmm")) |> ignore
            with
                | :? WebApiError as ex when ex.Code = 400 -> Assert.IsTrue(true);
                | _ -> Assert.IsTrue(false);

        [<TestMethod>]
        member this.AsyncGetTickersNotFound () =
            try
                (api :> IBitFinexApi).AsyncGetTickers(PairClass("btc","mmm")) |> Async.RunSynchronously |> ignore
            with
                | :? WebApiError as ex when ex.Code = 400 -> Assert.IsTrue(true);
                | _ -> Assert.IsTrue(false);

        [<TestMethod>]
        member this.GetTickersAsyncNotFound () =
            try
                let cts = new CancellationTokenSource()
                (api :> IBitFinexApi).GetTickersAsync(PairClass("btc","mmm"), cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously |> ignore
            with
                | :? AggregateException as ae when (ae.InnerException :? WebApiError) && ae.Message.Contains("Unknown Symbol from pair parameter") -> Assert.IsTrue(true)
                | _ -> Assert.IsTrue(false);

        [<TestMethod>]
        member this.GetStats () =
            let stats = (api :> IBitFinexApi).GetStats(PairClass("btc","usd"))
            Assert.IsTrue(stats |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.AsyncGetStats () =
            let stats = (api :> IBitFinexApi).AsyncGetStats(PairClass("btc","usd")) |>  Async.RunSynchronously
            Assert.IsTrue(stats |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetStatsAsync () =
            let stats = (api :> IBitFinexApi).GetStatsAsync(PairClass("btc","usd")) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(stats |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetStatsAsyncCancellationToken () =
            let api = BitFinexApi(apiKey,apiSecret, 10000)
            let cts = new CancellationTokenSource()
            let stats = (api :> IBitFinexApi).GetStatsAsync(PairClass("btc","usd"), cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(stats |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetStatsAsyncCancellationTokenCancel () =
                let cts = new CancellationTokenSource()
                let task = (api :> IBitFinexApi).GetStatsAsync(PairClass("btc","usd"), cts.Token) |>  Async.AwaitTask
                cts.Cancel() 
                let stats = task |> Async.RunSynchronously
                Assert.IsTrue(stats |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetStatsAsyncCancellationTokenOption () =
            let cts = new CancellationTokenSource()
            let stats = (api :> IBitFinexApi).GetStatsAsync(PairClass("btc","usd"), Some cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(stats |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetStatsAsyncCancellationTokenOptionCancel () =
                let cts = new CancellationTokenSource()
                let task = (api :> IBitFinexApi).GetStatsAsync(PairClass("btc","usd"), Some cts.Token) |>  Async.AwaitTask 
                cts.Cancel() 
                let stats = task |> Async.RunSynchronously
                Assert.IsTrue(stats |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetStatsAsyncCancellationTokenOptionNone () =
            let stats = (api :> IBitFinexApi).GetStatsAsync(PairClass("btc","usd"), None) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(stats |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetStatsNotFound () =
            let stats = (api :> IBitFinexApi).GetStats(PairClass("btc","mmm"))
            Assert.IsTrue(stats |> Seq.isEmpty);

        [<TestMethod>]
        member this.AsyncGetStatsNotFound () =
            let stats = (api :> IBitFinexApi).AsyncGetStats(PairClass("btc","mmm")) |>  Async.RunSynchronously
            Assert.IsTrue(stats |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetStatsAsyncNotFound () =
            let cts = new CancellationTokenSource()
            let stats = (api :> IBitFinexApi).GetStatsAsync(PairClass("btc","mmm"), Some cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(stats |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetOrderBook () =
            let orderBook = (api :> IBitFinexApi).GetOrderBook(PairClass("btc","usd"))
            Assert.IsTrue(orderBook.Exchange <> null);

        [<TestMethod>]
        member this.AsyncGetOrderBook () =
            let orderBook = (api :> IBitFinexApi).AsyncGetOrderBook(PairClass("btc","usd")) |>  Async.RunSynchronously
            Assert.IsTrue(orderBook.Exchange <> null);
        
        [<TestMethod>]
        member this.GetOrderBookAsync () =
            let orderBook = (api :> IBitFinexApi).GetOrderBookAsync(PairClass("btc","usd")) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(orderBook.Exchange <> null);

        [<TestMethod>]
        member this.GetOrderBookAsyncCancellationToken () =
            let cts = new CancellationTokenSource()
            let orderBook = (api :> IBitFinexApi).GetOrderBookAsync(PairClass("btc","usd"), cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(orderBook.Exchange <> null);

        [<TestMethod>]
        member this.GetOrderBookAsyncCancellationTokenCancel () =
            try
                let cts = new CancellationTokenSource()
                let task = (api :> IBitFinexApi).GetOrderBookAsync(PairClass("btc","usd"), cts.Token) |>  Async.AwaitTask
                cts.Cancel() 
                task |> Async.RunSynchronously |> ignore
            with
                | :? AggregateException as agx when (agx.InnerException :? TaskCanceledException) && agx.InnerException.Message.Contains("A task was canceled")  -> Assert.IsTrue(true);
                | _ -> Assert.IsTrue(false);

        [<TestMethod>]
        member this.GetOrderBookAsyncCancellationTokenOption () =
            let cts = new CancellationTokenSource()
            let orderBook = (api :> IBitFinexApi).GetOrderBookAsync(PairClass("btc","usd"), Some cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(orderBook.Exchange <> null);

        [<TestMethod>]
        member this.GetOrderBookAsyncCancellationTokenOptionCancel () =
            try
                let cts = new CancellationTokenSource()
                let task = (api :> IBitFinexApi).GetOrderBookAsync(PairClass("btc","usd"), cts.Token) |>  Async.AwaitTask
                cts.Cancel() 
                task |> Async.RunSynchronously |> ignore
            with
                | :? AggregateException as agx when (agx.InnerException :? TaskCanceledException) && agx.InnerException.Message.Contains("A task was canceled") -> Assert.IsTrue(true);
                | _ -> Assert.IsTrue(false);

        [<TestMethod>]
        member this.GetOrderBookAsyncCancellationTokenOptionNone () =
            let orderBook = (api :> IBitFinexApi).GetOrderBookAsync(PairClass("btc","usd"), None) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(orderBook.Exchange <> null);

        [<TestMethod>]
        member this.GetOrderBookNotFound () =
            try
                let orderBook = (api :> IBitFinexApi).GetOrderBook(PairClass("btc","mmm"))
                Assert.IsFalse(orderBook.Exchange <> null);
            with _ -> Assert.IsTrue(true);

        [<TestMethod>]
        member this.GetTrades () =
            let trades = (api :> IBitFinexApi).GetTrades(PairClass("btc","usd"))
            Assert.IsTrue(trades |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.AsyncGetTrades () =
            let trades = (api :> IBitFinexApi).AsyncGetTrades(PairClass("btc","usd")) |>  Async.RunSynchronously
            Assert.IsTrue(trades |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetTradesAsync () =
            let trades = (api :> IBitFinexApi).GetTradesAsync(PairClass("btc","usd")) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(trades |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetTradesAsyncCancellationToken () =
            let cts = new CancellationTokenSource()
            let trades = (api :> IBitFinexApi).GetTradesAsync(PairClass("btc","usd"), cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(trades |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetTradesAsyncCancellationTokenCancel () =
            let cts = new CancellationTokenSource()
            let task = (api :> IBitFinexApi).GetTradesAsync(PairClass("btc","usd"), cts.Token) |>  Async.AwaitTask
            cts.Cancel() 
            let trades = task |> Async.RunSynchronously
            Assert.IsTrue(trades |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetTradesAsyncCancellationTokenOption () =
            let cts = new CancellationTokenSource()
            let trades = (api :> IBitFinexApi).GetTradesAsync(PairClass("btc","usd"), Some cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(trades |> (not << Seq.isEmpty));


        [<TestMethod>]
        member this.GetTradesAsyncCancellationTokenOptionCancel () =
            let cts = new CancellationTokenSource()
            let task = (api :> IBitFinexApi).GetTradesAsync(PairClass("btc","usd"), Some cts.Token) |>  Async.AwaitTask
            cts.Cancel() 
            let trades = task |> Async.RunSynchronously
            Assert.IsTrue(trades |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetTradesAsyncCancellationTokenOptionNone () =
            let trades = (api :> IBitFinexApi).GetTradesAsync(PairClass("btc","usd"), None) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(trades |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetTradesNotFound () =
            let trades = (api :> IBitFinexApi).GetTrades(PairClass("btc","mmm"))
            Assert.IsTrue(trades |> Seq.isEmpty);

        [<TestMethod>]
        member this.AsyncGetTradesNotFound () =
            let trades = (api :> IBitFinexApi).AsyncGetTrades(PairClass("btc","mmm")) |>  Async.RunSynchronously
            Assert.IsTrue(trades |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetTradesAsyncNotFound () =
            let cts = new CancellationTokenSource()
            let trades = (api :> IBitFinexApi).GetTradesAsync(PairClass("btc","mmm"), cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(trades |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetLends () =
            let lands = (api :> IBitFinexApi).GetLends("btc")
            Assert.IsTrue(lands |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.AsyncGetLends () =
            let lands = (api :> IBitFinexApi).AsyncGetLends("btc") |>  Async.RunSynchronously
            Assert.IsTrue(lands |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetLendsAsync () =
            let lands = (api :> IBitFinexApi).GetLendsAsync("btc") |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(lands |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetLendsAsyncCancellationToken () =
            let cts = new CancellationTokenSource()
            let lands = (api :> IBitFinexApi).GetLendsAsync("btc", cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(lands |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetLendsAsyncCancellationTokenCancel () =
            let cts = new CancellationTokenSource()
            let task = (api :> IBitFinexApi).GetLendsAsync("btc", cts.Token) |>  Async.AwaitTask
            cts.Cancel() 
            let lends = task |> Async.RunSynchronously
            Assert.IsTrue(lends |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetLendsAsyncCancellationTokenOption () =
            let cts = new CancellationTokenSource()
            let lands = (api :> IBitFinexApi).GetLendsAsync("btc", Some cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(lands |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetLendsAsyncCancellationTokenOptionCancel () =
            let cts = new CancellationTokenSource()
            let task = (api :> IBitFinexApi).GetLendsAsync("btc", Some cts.Token) |>  Async.AwaitTask
            cts.Cancel() 
            let lends = task |> Async.RunSynchronously
            Assert.IsTrue(lends |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetLendsAsyncCancellationTokenOptionNone () =
            let lands = (api :> IBitFinexApi).GetLendsAsync("btc", None) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(lands |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetLendsNotFound () =
            let lands = (api :> IBitFinexApi).GetLends("mmm")
            Assert.IsTrue(lands |> Seq.isEmpty);

        [<TestMethod>]
        member this.AsyncGetLendsTimeout () =
            try
                let apil = BitFinexApi(apiKey,apiSecret, 1)
                (apil :> IBitFinexApi).AsyncGetLends("btc") |> Async.RunSynchronously |> ignore
                Assert.IsTrue(false);
            with
            | :? System.Net.WebException as we when (we.InnerException :? TimeoutException) -> Assert.IsTrue(true);
            | _ -> Assert.IsFalse(true);

        [<TestMethod>]
        member this.GetLendsTimeout () =
            try
                let apil = BitFinexApi(apiKey,apiSecret, 1)
                (apil :> IBitFinexApi).GetLends("btc") |> ignore
                Assert.IsTrue(false);
            with
            | :? System.Net.WebException as we when (we.InnerException :? TimeoutException) -> Assert.IsTrue(true);
            | _ -> Assert.IsFalse(true);

        [<TestMethod>]
        member this.AsyncGetLendsNotFound () =
            let lands = (api :> IBitFinexApi).AsyncGetLends("mmm") |> Async.RunSynchronously
            Assert.IsTrue(lands |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetLendsAsyncNotFound () =
            let cts = new CancellationTokenSource()
            let lands = (api :> IBitFinexApi).GetLendsAsync("mmm", cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(lands |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetLendBook () =
            let lendBook = (api :> IBitFinexApi).GetLendBook("btc")
            Assert.IsTrue(lendBook.Currency <> null);

        [<TestMethod>]
        member this.AsyncGetLendBook () =
            let lendBook = (api :> IBitFinexApi).AsyncGetLendBook("btc") |> Async.RunSynchronously
            Assert.IsTrue(lendBook.Currency <> null);
        
        [<TestMethod>]
        member this.GetLendBookAsync() =
            let lendBook = (api :> IBitFinexApi).GetLendBookAsync("btc") |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(lendBook.Currency <> null);

        [<TestMethod>]
        member this.GetLendBookAsyncCancellationToken() =
            let cts = new CancellationTokenSource()
            let lendBook = (api :> IBitFinexApi).GetLendBookAsync("btc", cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(lendBook.Currency <> null);

        [<TestMethod>]
        member this.GetLendBookAsyncCancellationTokenCancel() =
            try
                let cts = new CancellationTokenSource()
                let task = (api :> IBitFinexApi).GetLendBookAsync("btc", cts.Token) |>  Async.AwaitTask
                cts.Cancel() 
                task |> Async.RunSynchronously |> ignore
            with
                | :? AggregateException as agx when (agx.InnerException :? TaskCanceledException) && agx.InnerException.Message.Contains("A task was canceled") -> Assert.IsTrue(true);
                | _ -> Assert.IsTrue(false);

        [<TestMethod>]
        member this.GetLendBookAsyncCancellationTokenOption() =
            let cts = new CancellationTokenSource()
            let lendBook = (api :> IBitFinexApi).GetLendBookAsync("btc", Some cts.Token) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(lendBook.Currency <> null);
            Assert.IsTrue(lendBook.Asks |> (not << Seq.isEmpty));
            Assert.IsTrue(lendBook.Bids |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetLendBookAsyncCancellationTokenOptionCancel() =
            try
                let cts = new CancellationTokenSource()
                let task = (api :> IBitFinexApi).GetLendBookAsync("btc", Some cts.Token) |>  Async.AwaitTask
                cts.Cancel() 
                task |> Async.RunSynchronously |> ignore
            with
                | :? AggregateException as agx when (agx.InnerException :? TaskCanceledException) && agx.InnerException.Message.Contains("A task was canceled") -> Assert.IsTrue(true);
                | _ -> Assert.IsTrue(false);

        [<TestMethod>]
        member this.GetLendBookAsyncCancellationTokenOptionNone() =
            let lendBook = (api :> IBitFinexApi).GetLendBookAsync("btc", None) |>  Async.AwaitTask |> Async.RunSynchronously
            Assert.IsTrue(lendBook.Currency <> null);

        [<TestMethod>]
        member this.GetLendBookNotFound () =
            try
                let lendBook = (api :> IBitFinexApi).GetLendBook("mmm")
                Assert.IsFalse(lendBook.Currency <> null);
            with 
                | :? WebApiError as ex when ex.Code = 400 -> Assert.IsTrue(true);
                | _ -> Assert.IsTrue(false);

        [<TestMethod>]
        member this.AsyncGetLendBookNotFound () =
            try
                let lendBook = (api :> IBitFinexApi).AsyncGetLendBook("mmm") |> Async.RunSynchronously
                Assert.IsFalse(lendBook.Currency <> null);
            with 
                | :? WebApiError as ex when ex.Code = 400 -> Assert.IsTrue(true);
                | _ -> Assert.IsTrue(false);

        [<TestMethod>]
        member this.GetLendBookAsyncNotFound () =
            try
                let lendBook = (api :> IBitFinexApi).GetLendBookAsync("mmm") |>  Async.AwaitTask |> Async.RunSynchronously
                Assert.IsFalse(lendBook.Currency <> null);
            with 
               | :? AggregateException as agx when (agx.InnerException :? WebApiError) && (agx.InnerException :?> WebApiError).Code = 400 
                                                   && (agx.InnerException :?> WebApiError).InnerException.
                                                   Message.Contains("(400) Bad Request") -> Assert.IsTrue(true);
               | _ -> Assert.IsTrue(false);
