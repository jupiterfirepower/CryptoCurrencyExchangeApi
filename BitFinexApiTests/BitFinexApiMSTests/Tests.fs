namespace BitFinexApiMSTests

open System
open System.Diagnostics
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
        let api = BitFinexApi(apiKey,apiSecret, 5000)

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
        member this.GetPairDetails () =
            let pairDetails = (api :> IBitFinexApi).GetPairDetails()
            Assert.IsTrue(pairDetails |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetTickers () =
            let ticker = (api :> IBitFinexApi).GetTickers(PairClass("btc","usd"))
            Assert.IsTrue(ticker <> BitFinexTicker.Default);

        [<TestMethod>]
        member this.GetTickersNotFound () =
            let ticker = (api :> IBitFinexApi).GetTickers(PairClass("btc","mmm"))
            Assert.IsTrue((ticker = BitFinexTicker.Default));

        [<TestMethod>]
        member this.GetStats () =
            let stats = (api :> IBitFinexApi).GetStats(PairClass("btc","usd"))
            Assert.IsTrue(stats |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetStatsNotFound () =
            let stats = (api :> IBitFinexApi).GetStats(PairClass("btc","mmm"))
            Assert.IsTrue(stats |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetOrderBook () =
            let orderBook = (api :> IBitFinexApi).GetOrderBook(PairClass("btc","usd"))
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
        member this.GetTradesNotFound () =
            let trades = (api :> IBitFinexApi).GetTrades(PairClass("btc","mmm"))
            Assert.IsTrue(trades |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetLends () =
            let lands = (api :> IBitFinexApi).GetLends("btc")
            Assert.IsTrue(lands |> (not << Seq.isEmpty));

        [<TestMethod>]
        member this.GetLendsNotFound () =
            let lands = (api :> IBitFinexApi).GetLends("mmm")
            Assert.IsTrue(lands |> Seq.isEmpty);

        [<TestMethod>]
        member this.GetLendBook () =
            let lendBook = (api :> IBitFinexApi).GetLendBook("btc")
            Assert.IsTrue(lendBook.Currency <> null);

        [<TestMethod>]
        member this.GetLendBookNotFound () =
            try
                let lendBook = (api :> IBitFinexApi).GetLendBook("mmm")
                Assert.IsFalse(lendBook.Currency <> null);
            with _ -> Assert.IsTrue(true);
