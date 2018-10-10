namespace CryptCurrency.BitFinex

open System
open System.Net
open System.Collections.Generic
open CryptCurrency.Common.DataContracts
open FSharp.Data;
//open MoreLinq;
open FSharp.Data.HttpRequestHeaders
open FSharp.Data.HttpMethod
open System.Text
open Newtonsoft.Json
open System.Security.Cryptography
open System.Linq

module Model =
    type BitFinexResponse = { 
                              [<JsonProperty(PropertyName = "result")>]
                              Result : string;
                              [<JsonProperty(PropertyName = "message")>]
                              Message : string;
                              [<JsonProperty(PropertyName = "order_id")>]
                              OrderId : string; } 
                              member x.IsSuccess = String.IsNullOrEmpty(x.Message)
    type BitFinexBidAsk = { Rate : decimal
                            Amount : decimal
                            Period : int
                            Timestamp : decimal
                            Frr : bool }

    type BitFinexLendBook = { Currency : string; Bids : seq<BitFinexBidAsk>; Asks : seq<BitFinexBidAsk> }
    type BitFinexTicker = { Mid : decimal
                            Bid : decimal
                            Ask : decimal
                            LastPrice : decimal 
                            Low : decimal
                            High : decimal
                            Volume : decimal
                            Timestamp : decimal }
                            with
                                static member Default = { Mid = 0.0m; Bid = 0.0m; Ask = 0.0m; LastPrice = 0.0m; Low = 0.0m; High = 0.0m; Volume = 0.0m; Timestamp = 0.0m; }

    type BitFinexStats =  {  Period : int; Volume : decimal }

    type BitFinexTrade =  { Timestamp : int; Tid : int; Price : decimal; Amount : decimal; Exchange : string; Type : string; }

    type BitFinexLend = {  Rate : decimal; AmountLent : decimal; AmountUsed : decimal; Timestamp : int }

    type BitFinexPairDetails = {  Pair : string; PricePrecision : int; InitialMargin : decimal; MinimumMargin : decimal;  MaximumOrderSize : decimal; Expiration : string; Margin : bool }


module Utils =
    let getNonce = let currentNonce = DateTime.UtcNow.AddDays(1.0).Ticks
                   currentNonce

    let convert x = 
        let d = Dictionary<string, obj>()
        x |> Seq.iter d.Add
        d

    let paramsToJsonToBase64 parameters = let json = JsonConvert.SerializeObject(convert parameters, Formatting.None)
                                          let bytes = Encoding.UTF8.GetBytes(json)
                                          Convert.ToBase64String(bytes)

    let SignHMacSha384 (key:string, data:byte[]) : byte[] = let hashMaker = new HMACSHA384(Encoding.ASCII.GetBytes(key))
                                                            hashMaker.ComputeHash(data)

    let Sign(payload: string, secretKey:string) = let data = BitConverter.ToString(SignHMacSha384(secretKey, Encoding.UTF8.GetBytes(payload)))
                                                  data.Replace("-", "").ToLower()

    let UnixTimeStampToDateTime(unixTimeStamp:double) : DateTime = let dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                                                                   let dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime()
                                                                   dtDateTime

    let addDecompression (req: HttpWebRequest) =
        //req.Timeout <- 5000
        req.AutomaticDecompression <- DecompressionMethods.GZip ||| DecompressionMethods.Deflate
        req

    [<Literal>]
    let private XBFXAPIKEY = "X-BFX-APIKEY"
    [<Literal>]
    let private XBFXPAYLOAD = "X-BFX-PAYLOAD"
    [<Literal>]
    let private XBFXSIGNATURE = "X-BFX-SIGNATURE"

    let privateQuery(key: string, secret : string, url: string, parameters) : string = 
        let result = Http.RequestString(url , headers = [ (XBFXAPIKEY, key); (XBFXPAYLOAD, paramsToJsonToBase64 parameters); (XBFXSIGNATURE, Sign((paramsToJsonToBase64 parameters), secret)) ], customizeHttpRequest = addDecompression) 
        result

    let PrivateQuery(key: string, secret : string, url: string, parameters: seq<string * obj>) : string = 
        try
            let nonce = Convert.ToString(getNonce)
            let parameters' = Seq.append parameters [("nonce", Convert.ToString(getNonce) :> obj)]
            let resultData = privateQuery(key, secret, url, parameters')
            resultData
        with
        | _ as ex -> raise(ex)

    let Query<'T>(url) = let data = Http.RequestString(url)
                         JsonConvert.DeserializeObject<'T>(data)

module WebApi =
    open Utils
    open Model
    open CryptCurrency.Common.Retry
    open CryptCurrency.Common.StringEx

    [<Interface>]
    type IBitFinexApi =
        abstract GetSupportedPairs: unit -> seq<PairClass>
        abstract GetPairDetails: unit -> seq<BitFinexPairDetails>
        abstract GetTickers: PairClass -> BitFinexTicker
        abstract GetStats: PairClass -> seq<BitFinexStats>
        abstract GetOrderBook: PairClass -> OrderBook
        abstract GetTrades: PairClass -> seq<BitFinexTrade>
        abstract GetLends: string -> seq<BitFinexLend>
        abstract GetLendBook: string -> BitFinexLendBook

    type BitFinexPair = JsonProvider<"./data/BitFinexPair.json">
    type BitFinexPairDetails = JsonProvider<"./data/BitFinexPairDetails.json">
    type BitFinexOrderBook = JsonProvider<"./data/BitFinexOrderBook.json">
    type BitFinexLendBook = JsonProvider<"./data/BitFinexLandBook.json">
    type BitFinexPubTicker = JsonProvider<"./data/BitFinexPubTicker.json">
    type BitFinexPubStats = JsonProvider<"./data/BitFinexStats.json">
    type BitFinexPubTrades = JsonProvider<"./data/BitFinexTrades.json">
    type BitFinexLends = JsonProvider<"./data/BitFinexLends.json">

    let (+^) l r = sprintf "%s%s" l r
    let private baseUrl = "https://api.bitfinex.com/v1/"
    let private symbols = "symbols"

    let GetSupportedPairs webtimeout = let pairs = BitFinexPair.Parse(Http.RequestString(baseUrl +^ symbols, httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout))
                            //let result = pairs |> Seq.map (fun x -> let data = (x.Batch( 3, fun ch -> new string [|for c in ch -> c|]))
                            //                                        PairClass(Seq.item 0 data, Seq.item 1 data))
                                       let result = pairs |> Seq.map (fun x -> let data = x.toArrayByChunkSize(3)
                                                                               PairClass(Seq.item 0 data, Seq.item 1 data))
                                       result

    let GetPairDetails webtimeout = let pairDetails = BitFinexPairDetails.Parse(Http.RequestString(baseUrl +^ "symbols_details", httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout))
                                    let result = pairDetails |> Seq.map (fun x -> { Pair = x.Pair; PricePrecision = x.PricePrecision; InitialMargin = x.InitialMargin; MinimumMargin = x.MinimumMargin; MaximumOrderSize = x.MaximumOrderSize; Expiration = x.Expiration; Margin = x.Margin }); 
                                    result

    let GetSupportedPairsAsync = async{  let! pairs =  Http.AsyncRequestString(baseUrl +^ symbols)
                                         let pairs = BitFinexPair.Parse(pairs)
                                         let result = pairs |> Seq.map (fun x -> let data = x.toArrayByChunkSize(3)
                                                                                 PairClass(data.[0], data.[1]))
                                         return result  }

    //let GetActiveOrders = let methods = apiVersion ^ "orders"
    //                      let data = BitFinexOrderStatus.Parse(PrivateQuery("","", baseUrl ^ methods, [ ("request", ("/" ^ methods) :> obj); ("nonce", (Convert.ToString(getNonce)) :> obj) ]))
    //                      data
    let getOrderBook (pair: PairClass) (webtimeout:int) = let orderBook = BitFinexOrderBook.Parse(Http.RequestString(baseUrl +^ "book/" +^ pair.BaseCurrency +^ pair.CounterCurrency, customizeHttpRequest = addDecompression, timeout = webtimeout))
                                                          let bids = orderBook.Bids.Where(fun x -> x.Price > 0m && x.Amount > 0m).
                                                                        Select(fun b -> new Order(pair, b.Price, b.Amount, BitFinex, MarketSide.Bid, DateTime.UtcNow, OrderType.Limit, ExternalExchange, TimeInForce.GoodTillCancel))
                                                          let asks = orderBook.Asks.Where(fun x -> x.Price > 0m && x.Amount > 0m).
                                                                        Select(fun b -> new Order(pair, b.Price, b.Amount, BitFinex, MarketSide.Ask, DateTime.UtcNow, OrderType.Limit, ExternalExchange, TimeInForce.GoodTillCancel))
                                                          new OrderBook( bids, asks, BitFinex, pair, DateTime.UtcNow )
                                      
    let getLendBook(currency,webtimeout) = let landBook = BitFinexLendBook.Parse(Http.RequestString(baseUrl +^ "lendbook/" +^ currency, customizeHttpRequest = addDecompression, timeout = webtimeout))
                                           { Currency = currency;
                                             Asks = landBook.Asks |> Seq.map (fun x -> { Rate = x.Rate; Amount = x.Amount; Period = x.Period; Timestamp = x.Timestamp; Frr = x.Frr }); 
                                             Bids = landBook.Bids |> Seq.map (fun x -> { Rate = x.Rate; Amount = x.Amount; Period = x.Period; Timestamp = x.Timestamp; Frr = x.Frr }) }

    let GetTicker (pair: PairClass) (webtimeout:int) = let tiker = BitFinexPubTicker.Parse(Http.RequestString(baseUrl +^ "pubticker/" +^ pair.BaseCurrency +^ pair.CounterCurrency, customizeHttpRequest = addDecompression, timeout = webtimeout))
                                                       { Mid = tiker.Mid; Bid = tiker.Bid; Ask = tiker.Ask; LastPrice = tiker.LastPrice; Low = tiker.Low; High = tiker.High; Volume = tiker.Volume; Timestamp = tiker.Timestamp; }

    let GetStats (pair:PairClass) (webtimeout:int) = BitFinexPubStats.Parse(Http.RequestString(baseUrl +^ "stats/" +^ pair.BaseCurrency +^ pair.CounterCurrency, customizeHttpRequest = addDecompression, timeout = webtimeout)) 
                                                     |> Seq.map (fun x -> { Period = x.Period; Volume = x.Volume })

    let private GetTrades (pair:PairClass) (webtimeout:int) = BitFinexPubTrades.Parse(Http.RequestString(baseUrl +^ "trades/" +^ pair.BaseCurrency +^ pair.CounterCurrency, customizeHttpRequest = addDecompression, timeout = webtimeout)) 
                                                              |> Seq.map (fun x -> { Timestamp = x.Timestamp; Tid = x.Tid; Price = x.Price; Amount = x.Amount; Exchange = x.Exchange; Type = x.Type })

    let getLends(currency,webtimeout) = BitFinexLends.Parse(Http.RequestString(baseUrl +^ "lends/" +^ currency, customizeHttpRequest = addDecompression, timeout = webtimeout))
                                        |> Seq.map (fun x -> { Rate = x.Rate; AmountLent = x.AmountLent; AmountUsed = x.AmountUsed; Timestamp = x.Timestamp })

    let private retryHelper f defaultRetryParams = 
                                let result = 
                                          (retry {
                                                   return f
                                          }) defaultRetryParams 
                                result

    type BitFinexApi(key:string, secret:string, ?timeout:int)=
         let apiKey = key
         let apiSecret = secret
         let webtimeout = defaultArg timeout 5000 
         let defaultRetryParams = { maxRetries = 3; waitBetweenRetries = 0 }
         interface IBitFinexApi with
            member x.GetSupportedPairs() = try
                                              let pairs = retryHelper (GetSupportedPairs webtimeout) defaultRetryParams
                                              pairs
                                           with
                                              _ -> reraise()

            member x.GetPairDetails() = try
                                              let pairDetails = retryHelper (GetPairDetails webtimeout) defaultRetryParams
                                              pairDetails
                                        with
                                             _ -> reraise()

            
            member x.GetTickers(pair:PairClass) = try
                                                     let ticker = retryHelper (GetTicker pair webtimeout) defaultRetryParams
                                                     ticker
                                                  with
                                                     _ -> BitFinexTicker.Default

            member x.GetStats(pair:PairClass) = try
                                                     let stats = retryHelper (GetStats pair webtimeout) defaultRetryParams
                                                     stats
                                                with
                                                   _ -> Seq.empty

            member x.GetOrderBook(pair:PairClass) = try
                                                        let orderBook = retryHelper (getOrderBook pair webtimeout) defaultRetryParams
                                                        orderBook
                                                    with _ -> reraise()

            member x.GetTrades(pair:PairClass) = try
                                                    let trades = retryHelper (GetTrades pair webtimeout) defaultRetryParams
                                                    trades
                                                 with
                                                    _ -> Seq.empty

            member x.GetLends(currency:string) = try
                                                    let lands = retryHelper (getLends(currency,webtimeout)) defaultRetryParams
                                                    lands
                                                 with
                                                    _ -> Seq.empty
            member x.GetLendBook(currency:string) = try
                                                        let landBook = retryHelper (getLendBook(currency,webtimeout)) defaultRetryParams
                                                        landBook
                                                    with
                                                        _ -> reraise()
         member x.GetSupportedPairs() = (x :> IBitFinexApi).GetSupportedPairs
         member x.GetPairDetails() = (x :> IBitFinexApi).GetPairDetails
         member x.GetTickers(pair:PairClass) = (x :> IBitFinexApi).GetTickers(pair)
         member x.GetStats(pair:PairClass) = (x :> IBitFinexApi).GetStats(pair)
         member x.GetOrderBook(pair:PairClass) = (x :> IBitFinexApi).GetOrderBook(pair)
         member x.GetTrades(pair:PairClass) = (x :> IBitFinexApi).GetTrades(pair)
         member x.GetLands(currency:string) = (x :> IBitFinexApi).GetLends(currency)
         member x.GetLendBook(currency:string) = (x :> IBitFinexApi).GetLendBook(currency)
                                                

    
                                       

            

