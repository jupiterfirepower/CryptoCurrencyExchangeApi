namespace CryptCurrency.BitFinex

open System
open System.Threading.Tasks
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
    open System.Net.Http
    open System.Threading
    open System.IO


    [<Interface>]
    type IBitFinexApi =
        abstract GetSupportedPairs: unit -> seq<PairClass>
        abstract AsyncGetSupportedPairs: unit -> Async<seq<PairClass>>
        abstract GetSupportedPairsAsync: unit -> Task<seq<PairClass>>
        abstract GetSupportedPairsAsync: CancellationToken -> Task<seq<PairClass>>
        abstract GetSupportedPairsAsync: CancellationToken option -> Task<seq<PairClass>>
        
        abstract GetPairDetails: unit -> seq<BitFinexPairDetails>
        abstract AsyncGetPairDetails: unit -> Async<seq<BitFinexPairDetails>>
        abstract GetPairDetailsAsync: unit -> Task<seq<BitFinexPairDetails>>
        abstract GetPairDetailsAsync: CancellationToken -> Task<seq<BitFinexPairDetails>>
        abstract GetPairDetailsAsync: CancellationToken option-> Task<seq<BitFinexPairDetails>>

        abstract GetTickers: PairClass -> BitFinexTicker
        abstract AsyncGetTickers: PairClass -> Async<BitFinexTicker>
        abstract GetTickersAsync: PairClass -> Task<BitFinexTicker>
        abstract GetTickersAsync: PairClass * CancellationToken -> Task<BitFinexTicker>
        abstract GetTickersAsync: PairClass * CancellationToken option -> Task<BitFinexTicker>

        abstract GetStats: PairClass -> seq<BitFinexStats>
        abstract AsyncGetStats: PairClass -> Async<seq<BitFinexStats>>
        abstract GetStatsAsync: PairClass -> Task<seq<BitFinexStats>>
        abstract GetStatsAsync: PairClass * CancellationToken -> Task<seq<BitFinexStats>>
        abstract GetStatsAsync: PairClass * CancellationToken option -> Task<seq<BitFinexStats>>

        abstract GetOrderBook: PairClass -> OrderBook
        abstract AsyncGetOrderBook: PairClass -> Async<OrderBook>
        abstract GetOrderBookAsync: PairClass -> Task<OrderBook>
        abstract GetOrderBookAsync: PairClass * CancellationToken -> Task<OrderBook>
        abstract GetOrderBookAsync: PairClass * CancellationToken option -> Task<OrderBook>

        abstract GetTrades: PairClass -> seq<BitFinexTrade>
        abstract AsyncGetTrades: PairClass -> Async<seq<BitFinexTrade>>
        abstract GetTradesAsync: PairClass -> Task<seq<BitFinexTrade>>
        abstract GetTradesAsync: PairClass * CancellationToken -> Task<seq<BitFinexTrade>>
        abstract GetTradesAsync: PairClass * CancellationToken option -> Task<seq<BitFinexTrade>>

        abstract GetLends: string -> seq<BitFinexLend>
        abstract AsyncGetLends: string -> Async<seq<BitFinexLend>>
        abstract GetLendsAsync: string -> Task<seq<BitFinexLend>>
        abstract GetLendsAsync: string * CancellationToken -> Task<seq<BitFinexLend>>
        abstract GetLendsAsync: string * CancellationToken option -> Task<seq<BitFinexLend>>

        abstract GetLendBook: string -> BitFinexLendBook
        abstract AsyncGetLendBook: string -> Async<BitFinexLendBook>
        abstract GetLendBookAsync: string -> Task<BitFinexLendBook>
        abstract GetLendBookAsync: string * CancellationToken -> Task<BitFinexLendBook>
        abstract GetLendBookAsync: string * CancellationToken option -> Task<BitFinexLendBook>

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

    let AsyncWebRequestGetString (url:string, token:CancellationToken, webtimeout: int) = async {
                                        let handler = new HttpClientHandler()
                                        handler.AutomaticDecompression <- DecompressionMethods.Deflate ||| DecompressionMethods.GZip
                                        use httpClient = new System.Net.Http.HttpClient(handler)
                                        httpClient.Timeout <- new TimeSpan(0,0,0,0, webtimeout)
                                        use! response = httpClient.GetAsync(url, token) |> Async.AwaitTask
                                        response.EnsureSuccessStatusCode () |> ignore
                                        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                                        return content
                                      }

    let GetSupportedPairs webtimeout = let pairs = BitFinexPair.Parse(Http.RequestString(baseUrl +^ symbols, httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout))
                            //let result = pairs |> Seq.map (fun x -> let data = (x.Batch( 3, fun ch -> new string [|for c in ch -> c|]))
                            //                                        PairClass(Seq.item 0 data, Seq.item 1 data))
                                       let result = pairs |> Seq.map (fun x -> let data = x.toArrayByChunkSize(3)
                                                                               PairClass(Seq.item 0 data, Seq.item 1 data))
                                       result

    let AsyncGetSupportedPairs webtimeout = async{  let! pairs =  Http.AsyncRequestString(baseUrl +^ symbols, httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                    let pairs = BitFinexPair.Parse(pairs)
                                                    let result = pairs |> Seq.map (fun x -> let data = x.toArrayByChunkSize(3)
                                                                                            PairClass(data.[0], data.[1]))
                                                    return result  }

    let GetSupportedPairsAsync(token:CancellationToken, webtimeout: int) = async{  
                                                    try
                                                        let! pairs =  AsyncWebRequestGetString(baseUrl +^ symbols, token, webtimeout)
                                                        let pairs = BitFinexPair.Parse(pairs)
                                                        let result = pairs |> Seq.map (fun x -> let data = x.toArrayByChunkSize(3)
                                                                                                PairClass(data.[0], data.[1]))
                                                        return result
                                                    with 
                                                        | :? OperationCanceledException -> return Seq.empty 
                                                    }

    let GetPairDetails webtimeout = let pairDetails = BitFinexPairDetails.Parse(Http.RequestString(baseUrl +^ "symbols_details", httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout))
                                    let result = pairDetails |> Seq.map (fun x -> { Pair = x.Pair; PricePrecision = x.PricePrecision; InitialMargin = x.InitialMargin; MinimumMargin = x.MinimumMargin; MaximumOrderSize = x.MaximumOrderSize; Expiration = x.Expiration; Margin = x.Margin }); 
                                    result


    let AsyncGetPairDetails webtimeout = async{  let! pairDetails =  Http.AsyncRequestString(baseUrl +^ "symbols_details", httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                 let pairDetails = BitFinexPairDetails.Parse(pairDetails)
                                                 let result = pairDetails |> Seq.map (fun x -> { Pair = x.Pair; PricePrecision = x.PricePrecision; InitialMargin = x.InitialMargin; MinimumMargin = x.MinimumMargin; MaximumOrderSize = x.MaximumOrderSize; Expiration = x.Expiration; Margin = x.Margin }); 
                                                 return result  }

    let GetPairDetailsAsync(token:CancellationToken, webtimeout: int) = async{  
                                                    try
                                                        let! pairDetailsJson =  AsyncWebRequestGetString(baseUrl +^ "symbols_details", token, webtimeout)
                                                        let pairDetails = BitFinexPairDetails.Parse(pairDetailsJson)
                                                        let result = pairDetails |> Seq.map (fun x -> { Pair = x.Pair; PricePrecision = x.PricePrecision; InitialMargin = x.InitialMargin; MinimumMargin = x.MinimumMargin; MaximumOrderSize = x.MaximumOrderSize; Expiration = x.Expiration; Margin = x.Margin }); 
                                                        return result
                                                    with 
                                                        | :? OperationCanceledException -> return Seq.empty 
                                                    }

    //let GetActiveOrders = let methods = apiVersion ^ "orders"
    //                      let data = BitFinexOrderStatus.Parse(PrivateQuery("","", baseUrl ^ methods, [ ("request", ("/" ^ methods) :> obj); ("nonce", (Convert.ToString(getNonce)) :> obj) ]))
    //                      data
    let getOrderBook (pair: PairClass) (webtimeout:int) = let orderBook = BitFinexOrderBook.Parse(Http.RequestString(baseUrl +^ "book/" +^ pair.BaseCurrency +^ pair.CounterCurrency, customizeHttpRequest = addDecompression, timeout = webtimeout))
                                                          let bids = orderBook.Bids.Where(fun x -> x.Price > 0m && x.Amount > 0m).
                                                                        Select(fun b -> new Order(pair, b.Price, b.Amount, BitFinex, MarketSide.Bid, DateTime.UtcNow, OrderType.Limit, ExternalExchange, TimeInForce.GoodTillCancel))
                                                          let asks = orderBook.Asks.Where(fun x -> x.Price > 0m && x.Amount > 0m).
                                                                        Select(fun b -> new Order(pair, b.Price, b.Amount, BitFinex, MarketSide.Ask, DateTime.UtcNow, OrderType.Limit, ExternalExchange, TimeInForce.GoodTillCancel))
                                                          new OrderBook( bids, asks, BitFinex, pair, DateTime.UtcNow )

    let AsyncGetOrderBook (pair: PairClass) (webtimeout:int) = async{  let! orderBook =  Http.AsyncRequestString(baseUrl +^ "book/" +^ pair.BaseCurrency +^ pair.CounterCurrency, httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                       let orderBook = BitFinexOrderBook.Parse(orderBook)
                                                                       let bids = orderBook.Bids.Where(fun x -> x.Price > 0m && x.Amount > 0m).
                                                                                    Select(fun b -> new Order(pair, b.Price, b.Amount, BitFinex, MarketSide.Bid, DateTime.UtcNow, OrderType.Limit, ExternalExchange, TimeInForce.GoodTillCancel))
                                                                       let asks = orderBook.Asks.Where(fun x -> x.Price > 0m && x.Amount > 0m).
                                                                                    Select(fun b -> new Order(pair, b.Price, b.Amount, BitFinex, MarketSide.Ask, DateTime.UtcNow, OrderType.Limit, ExternalExchange, TimeInForce.GoodTillCancel))
                                                                       return new OrderBook( bids, asks, BitFinex, pair, DateTime.UtcNow )  }

    let GetOrderBookAsync(pair: PairClass, token:CancellationToken, webtimeout: int) = async{  
                                                    
                                                        let! orderBook =  AsyncWebRequestGetString(baseUrl +^ "book/" +^ pair.BaseCurrency +^ pair.CounterCurrency, token, webtimeout)
                                                        let orderBook = BitFinexOrderBook.Parse(orderBook)
                                                        let bids = orderBook.Bids.Where(fun x -> x.Price > 0m && x.Amount > 0m).
                                                                             Select(fun b -> new Order(pair, b.Price, b.Amount, BitFinex, MarketSide.Bid, DateTime.UtcNow, OrderType.Limit, ExternalExchange, TimeInForce.GoodTillCancel))
                                                        let asks = orderBook.Asks.Where(fun x -> x.Price > 0m && x.Amount > 0m).
                                                                             Select(fun b -> new Order(pair, b.Price, b.Amount, BitFinex, MarketSide.Ask, DateTime.UtcNow, OrderType.Limit, ExternalExchange, TimeInForce.GoodTillCancel))
                                                        return new OrderBook( bids, asks, BitFinex, pair, DateTime.UtcNow ) 
                                                        }
                                      
    let getLendBook(currency,webtimeout) = let landBook = BitFinexLendBook.Parse(Http.RequestString(baseUrl +^ "lendbook/" +^ currency, customizeHttpRequest = addDecompression, timeout = webtimeout))
                                           { Currency = currency;
                                             Asks = landBook.Asks |> Seq.map (fun x -> { Rate = x.Rate; Amount = x.Amount; Period = x.Period; Timestamp = x.Timestamp; Frr = x.Frr }); 
                                             Bids = landBook.Bids |> Seq.map (fun x -> { Rate = x.Rate; Amount = x.Amount; Period = x.Period; Timestamp = x.Timestamp; Frr = x.Frr }) }

    let AsyncGetLendBook(currency,webtimeout) = async{ 
                                                        let! landBook =  Http.AsyncRequestString(baseUrl +^ "lendbook/" +^ currency, httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                        let landBook = BitFinexLendBook.Parse(landBook)
                                                        return   { Currency = currency;
                                                             Asks = landBook.Asks |> Seq.map (fun x -> { Rate = x.Rate; Amount = x.Amount; Period = x.Period; Timestamp = x.Timestamp; Frr = x.Frr }); 
                                                             Bids = landBook.Bids |> Seq.map (fun x -> { Rate = x.Rate; Amount = x.Amount; Period = x.Period; Timestamp = x.Timestamp; Frr = x.Frr }) }
                                                }

    let GetLendBookAsync(currency: string, token:CancellationToken, webtimeout: int) = async{  
                                                    
                                                        let! lendBook =  AsyncWebRequestGetString(baseUrl +^ "lendbook/" +^ currency, token, webtimeout)
                                                        let lendBook = BitFinexLendBook.Parse(lendBook)
                                                        return   { Currency = currency;
                                                             Asks = lendBook.Asks |> Seq.map (fun x -> { Rate = x.Rate; Amount = x.Amount; Period = x.Period; Timestamp = x.Timestamp; Frr = x.Frr }); 
                                                             Bids = lendBook.Bids |> Seq.map (fun x -> { Rate = x.Rate; Amount = x.Amount; Period = x.Period; Timestamp = x.Timestamp; Frr = x.Frr }) }
                                                        }

    type WebApiError(code : int, msg : string) =
        inherit Exception(msg)
        member e.Code = code
        new (msg : string) = WebApiError(0, msg)
    
    exception Error of WebApiError

    let GetTicker (pair: PairClass) (webtimeout:int) = try
                                                            let tiker = BitFinexPubTicker.Parse(Http.RequestString(baseUrl +^ "pubticker/" +^ pair.BaseCurrency +^ pair.CounterCurrency, customizeHttpRequest = addDecompression, timeout = webtimeout))
                                                            { Mid = tiker.Mid; Bid = tiker.Bid; Ask = tiker.Ask; LastPrice = tiker.LastPrice; Low = tiker.Low; High = tiker.High; Volume = tiker.Volume; Timestamp = tiker.Timestamp; }
                                                        with 
                                                        | ex -> raise(WebApiError(404, "GetTicker not found"))
                                                   
    
    let AsyncGetTicker (pair: PairClass) (webtimeout:int) = async{  let! tiker =  Http.AsyncRequestString(baseUrl +^ "pubticker/" +^ pair.BaseCurrency +^ pair.CounterCurrency, httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                    let tiker = BitFinexPubTicker.Parse(tiker)
                                                                    let result = { Mid = tiker.Mid; Bid = tiker.Bid; Ask = tiker.Ask; LastPrice = tiker.LastPrice; Low = tiker.Low; High = tiker.High; Volume = tiker.Volume; Timestamp = tiker.Timestamp; } 
                                                                    return result  }  
                                                                    

                                                                    
    let GetTickerAsync(pair: PairClass, token:CancellationToken, webtimeout: int) = async{  
                                                    try
                                                        let! tikerJson =  AsyncWebRequestGetString(baseUrl +^ "pubticker/" +^ pair.BaseCurrency +^ pair.CounterCurrency, token, webtimeout)
                                                        let tiker = BitFinexPubTicker.Parse(tikerJson)
                                                        let result = { Mid = tiker.Mid; Bid = tiker.Bid; Ask = tiker.Ask; LastPrice = tiker.LastPrice; Low = tiker.Low; High = tiker.High; Volume = tiker.Volume; Timestamp = tiker.Timestamp; } 
                                                        return result 
                                                    with 
                                                        | :? OperationCanceledException as ex -> return raise(WebApiError(300, "GetTickerAsync OperationCanceledException"))
                                                    }

    let GetStats (pair:PairClass) (webtimeout:int) = BitFinexPubStats.Parse(Http.RequestString(baseUrl +^ "stats/" +^ pair.BaseCurrency +^ pair.CounterCurrency, customizeHttpRequest = addDecompression, timeout = webtimeout)) 
                                                     |> Seq.map (fun x -> { Period = x.Period; Volume = x.Volume })

    let AsyncGetStats (pair: PairClass) (webtimeout:int) = async{  let! stats = Http.AsyncRequestString(baseUrl +^ "stats/" +^ pair.BaseCurrency +^ pair.CounterCurrency, httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                   let stats = BitFinexPubStats.Parse(stats)
                                                                   let result = stats |> Seq.map (fun x -> { Period = x.Period; Volume = x.Volume })
                                                                   return result  }

    let GetStatsAsync(pair: PairClass, token:CancellationToken, webtimeout: int) = async{  
                                                    try
                                                        let! statsJson =  AsyncWebRequestGetString(baseUrl +^ "stats/" +^ pair.BaseCurrency +^ pair.CounterCurrency, token, webtimeout)
                                                        let stats = BitFinexPubStats.Parse(statsJson)
                                                        let result = stats |> Seq.map (fun x -> { Period = x.Period; Volume = x.Volume })
                                                        return result
                                                    with 
                                                        | :? OperationCanceledException -> return Seq.empty 
                                                    }

    let private GetTrades (pair:PairClass) (webtimeout:int) = BitFinexPubTrades.Parse(Http.RequestString(baseUrl +^ "trades/" +^ pair.BaseCurrency +^ pair.CounterCurrency, customizeHttpRequest = addDecompression, timeout = webtimeout)) 
                                                              |> Seq.map (fun x -> { Timestamp = x.Timestamp; Tid = x.Tid; Price = x.Price; Amount = x.Amount; Exchange = x.Exchange; Type = x.Type })

    let AsyncGetTrades (pair: PairClass) (webtimeout:int) = async{  let! trades = Http.AsyncRequestString(baseUrl +^ "trades/" +^ pair.BaseCurrency +^ pair.CounterCurrency, httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                    let trades = BitFinexPubTrades.Parse(trades)
                                                                    let result = trades |> Seq.map (fun x -> { Timestamp = x.Timestamp; Tid = x.Tid; Price = x.Price; Amount = x.Amount; Exchange = x.Exchange; Type = x.Type })
                                                                    return result  }

    let GetTradesAsync(pair: PairClass, token:CancellationToken, webtimeout: int) = async{  
                                                    try
                                                        let! trades =  AsyncWebRequestGetString(baseUrl +^ "trades/" +^ pair.BaseCurrency +^ pair.CounterCurrency, token, webtimeout)
                                                        let trades = BitFinexPubTrades.Parse(trades)
                                                        let result = trades |> Seq.map (fun x -> { Timestamp = x.Timestamp; Tid = x.Tid; Price = x.Price; Amount = x.Amount; Exchange = x.Exchange; Type = x.Type })
                                                        return result
                                                    with 
                                                        | :? OperationCanceledException -> return Seq.empty 
                                                    }

    let getLends(currency,webtimeout) = BitFinexLends.Parse(Http.RequestString(baseUrl +^ "lends/" +^ currency, customizeHttpRequest = addDecompression, timeout = webtimeout))
                                        |> Seq.map (fun x -> { Rate = x.Rate; AmountLent = x.AmountLent; AmountUsed = x.AmountUsed; Timestamp = x.Timestamp })


    let AsyncGetLends (currency: string) (webtimeout:int) = async{  try
                                                                        let! lends = Http.AsyncRequestString(baseUrl +^ "lends/" +^ currency, httpMethod = Get, headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                        let lends = BitFinexLends.Parse(lends)
                                                                        let result = lends |> Seq.map (fun x -> { Rate = x.Rate; AmountLent = x.AmountLent; AmountUsed = x.AmountUsed; Timestamp = x.Timestamp })
                                                                        return result 
                                                                     with 
                                                                        //| :? System.Net.WebException as wex when wex.Status=WebExceptionStatus.ProtocolError && (wex.Response :> System.Net.WebResponse :?> System.Net.HttpWebResponse).StatusCode = System.Net.HttpStatusCode.BadRequest -> return Seq.empty  
                                                                        | :? WebException as e when e.Status=WebExceptionStatus.ProtocolError -> 
                                                                                use stream = e.Response.GetResponseStream() 
                                                                                let reader = new StreamReader(stream) 
                                                                                let body = reader.ReadToEnd()
                                                                                if body.Contains("{\"message\":\"Unknown currency\"}") then return Seq.empty else return raise(WebApiError(300, body))
                                                                         
                                                                 }

    let GetLendsAsync(currency: string, token:CancellationToken, webtimeout: int) = async{  
                                                    try
                                                        let! lends =  AsyncWebRequestGetString(baseUrl +^ "lends/" +^ currency, token, webtimeout)
                                                        let lends = BitFinexLends.Parse(lends)
                                                        let result = lends |> Seq.map (fun x -> { Rate = x.Rate; AmountLent = x.AmountLent; AmountUsed = x.AmountUsed; Timestamp = x.Timestamp })
                                                        return result
                                                    with 
                                                        | :? OperationCanceledException -> return Seq.empty 
                                                        | :? System.Net.WebException -> return Seq.empty
                                                    }

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

         do 
            if timeout.IsSome && timeout.Value <= 0 then invalidArg "timeout" "timeout must be greater than zero."
            if key.IsNullOrEmpty() then nullArg "key can't be null or empty."
            if secret.IsNullOrEmpty() then nullArg "secret can't be null or empty."

         interface IBitFinexApi with
            member x.GetSupportedPairs() = let pairs = retryHelper (GetSupportedPairs webtimeout) defaultRetryParams
                                           pairs

                                              
            member x.AsyncGetSupportedPairs() = AsyncGetSupportedPairs webtimeout
            /// Expose C#-friendly asynchronous method that returns Task
            member x.GetSupportedPairsAsync() = Async.StartAsTask((AsyncGetSupportedPairs webtimeout)) 
            /// Expose C#-friendly asynchronous method that returns Task
            /// and takes cancellation token to support cancellation...
            member x.GetSupportedPairsAsync(token:CancellationToken) = Async.StartAsTask(GetSupportedPairsAsync(token, webtimeout))
            member x.GetSupportedPairsAsync(?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                        Async.StartAsTask(GetSupportedPairsAsync(deftoken, webtimeout))
                                                                        

            member x.GetPairDetails() = let pairDetails = retryHelper (GetPairDetails webtimeout) defaultRetryParams
                                        pairDetails
            
            member x.AsyncGetPairDetails() = AsyncGetPairDetails webtimeout
            member x.GetPairDetailsAsync() = Async.StartAsTask(AsyncGetPairDetails(webtimeout))
            member x.GetPairDetailsAsync(token:CancellationToken) =  Async.StartAsTask(GetPairDetailsAsync(token, webtimeout))
            member x.GetPairDetailsAsync(?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                     Async.StartAsTask(GetPairDetailsAsync(deftoken, webtimeout))

            
            member x.GetTickers(pair:PairClass) = let ticker = retryHelper (GetTicker pair webtimeout) defaultRetryParams
                                                  ticker

            member x.AsyncGetTickers(pair:PairClass) = AsyncGetTicker pair webtimeout
            member x.GetTickersAsync(pair:PairClass) = Async.StartAsTask(AsyncGetTicker pair webtimeout)
            member x.GetTickersAsync(pair:PairClass, token:CancellationToken) = Async.StartAsTask(GetTickerAsync(pair, token, webtimeout))
            member x.GetTickersAsync(pair:PairClass, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                                 Async.StartAsTask(GetTickerAsync(pair, deftoken, webtimeout))

            member x.GetStats(pair:PairClass) = try
                                                     let stats = retryHelper (GetStats pair webtimeout) defaultRetryParams
                                                     stats
                                                with
                                                   _ -> Seq.empty

            member x.AsyncGetStats(pair:PairClass) = AsyncGetStats pair webtimeout
            member x.GetStatsAsync(pair:PairClass) = Async.StartAsTask(AsyncGetStats pair webtimeout)
            member x.GetStatsAsync(pair:PairClass, token:CancellationToken) = Async.StartAsTask(GetStatsAsync(pair, token, webtimeout))
            member x.GetStatsAsync(pair:PairClass, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                               Async.StartAsTask(GetStatsAsync(pair, deftoken, webtimeout))

            member x.GetOrderBook(pair:PairClass) = let orderBook = retryHelper (getOrderBook pair webtimeout) defaultRetryParams
                                                    orderBook

            member x.AsyncGetOrderBook(pair:PairClass) = AsyncGetOrderBook pair webtimeout
            member x.GetOrderBookAsync(pair:PairClass) = Async.StartAsTask(AsyncGetOrderBook pair webtimeout)
            member x.GetOrderBookAsync(pair:PairClass, token:CancellationToken) = Async.StartAsTask(GetOrderBookAsync(pair, token, webtimeout))
            member x.GetOrderBookAsync(pair:PairClass, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                                   Async.StartAsTask(GetOrderBookAsync(pair, deftoken, webtimeout))

            member x.GetTrades(pair:PairClass) = try
                                                    let trades = retryHelper (GetTrades pair webtimeout) defaultRetryParams
                                                    trades
                                                 with
                                                    _ -> Seq.empty

            member x.AsyncGetTrades(pair:PairClass) = AsyncGetTrades pair webtimeout
            member x.GetTradesAsync(pair:PairClass) = Async.StartAsTask(AsyncGetTrades pair webtimeout)
            member x.GetTradesAsync(pair:PairClass, token:CancellationToken) = Async.StartAsTask(GetTradesAsync(pair, token, webtimeout))
            member x.GetTradesAsync(pair:PairClass, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                                Async.StartAsTask(GetTradesAsync(pair, deftoken, webtimeout))

            member x.GetLends(currency:string) = try
                                                    let lands = retryHelper (getLends(currency,webtimeout)) defaultRetryParams
                                                    lands
                                                 with
                                                    _ -> Seq.empty

            member x.AsyncGetLends(currency:string) = AsyncGetLends currency webtimeout
            member x.GetLendsAsync(currency:string) = Async.StartAsTask(AsyncGetLends currency webtimeout)
            member x.GetLendsAsync(currency:string, token:CancellationToken) = Async.StartAsTask(GetLendsAsync(currency, token, webtimeout))
            member x.GetLendsAsync(currency:string, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                                Async.StartAsTask(GetLendsAsync(currency, deftoken, webtimeout))

            member x.GetLendBook(currency:string) = let landBook = retryHelper (getLendBook(currency,webtimeout)) defaultRetryParams
                                                    landBook

            member x.AsyncGetLendBook(currency:string) = AsyncGetLendBook(currency, webtimeout)
            member x.GetLendBookAsync(currency:string) = Async.StartAsTask(AsyncGetLendBook(currency, webtimeout))
            member x.GetLendBookAsync(currency:string, token:CancellationToken) = Async.StartAsTask(GetLendBookAsync(currency, token, webtimeout))
            member x.GetLendBookAsync(currency:string, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                                   Async.StartAsTask(GetLendBookAsync(currency, deftoken, webtimeout))

         member x.GetSupportedPairs() = (x :> IBitFinexApi).GetSupportedPairs()
         member x.AsyncGetSupportedPairs() = (x :> IBitFinexApi).AsyncGetSupportedPairs()
         /// Expose C#-friendly asynchronous method that returns Task
         member x.GetSupportedPairsAsync() = (x :> IBitFinexApi).GetSupportedPairsAsync()
         /// Expose C#-friendly asynchronous method that returns Task
         member x.GetSupportedPairsAsync(token:CancellationToken) = (x :> IBitFinexApi).GetSupportedPairsAsync(token)
         member x.GetSupportedPairsAsync(?token:CancellationToken) = (x :> IBitFinexApi).GetSupportedPairsAsync(token)

         member x.GetPairDetails() = (x :> IBitFinexApi).GetPairDetails()
         member x.AsyncGetPairDetails() = (x :> IBitFinexApi).AsyncGetPairDetails()
         /// Expose C#-friendly asynchronous method that returns Task
         member x.GetPairDetailsAsync() = (x :> IBitFinexApi).GetPairDetailsAsync()
         /// Expose C#-friendly asynchronous method that returns Task
         member x.GetPairDetailsAsync(token:CancellationToken) = (x :> IBitFinexApi).GetPairDetailsAsync(token)
         member x.GetPairDetailsAsync(?token:CancellationToken) = (x :> IBitFinexApi).GetPairDetailsAsync(token)

         member x.GetTickers(pair:PairClass) = (x :> IBitFinexApi).GetTickers(pair)
         member x.AsyncGetTickers(pair:PairClass) = (x :> IBitFinexApi).AsyncGetTickers(pair)
         member x.GetTickersAsync(pair:PairClass) = (x :> IBitFinexApi).GetTickersAsync(pair)
         member x.GetTickersAsync(pair:PairClass, token:CancellationToken) = (x :> IBitFinexApi).GetTickersAsync(pair, token)
         member x.GetTickersAsync(pair:PairClass, ?token:CancellationToken) = (x :> IBitFinexApi).GetTickersAsync(pair, token)

         member x.GetStats(pair:PairClass) = (x :> IBitFinexApi).GetStats(pair)
         member x.AsyncGetStats(pair:PairClass) = (x :> IBitFinexApi).AsyncGetStats(pair)
         member x.GetStatsAsync(pair:PairClass) = (x :> IBitFinexApi).GetStatsAsync(pair)
         member x.GetStatsAsync(pair:PairClass, token:CancellationToken) = (x :> IBitFinexApi).GetStatsAsync(pair, token)
         member x.GetStatsAsync(pair:PairClass, ?token:CancellationToken) = (x :> IBitFinexApi).GetStatsAsync(pair, token)

         member x.GetOrderBook(pair:PairClass) = (x :> IBitFinexApi).GetOrderBook(pair)
         member x.AsyncGetOrderBook(pair:PairClass) = (x :> IBitFinexApi).AsyncGetOrderBook(pair)
         member x.GetOrderBookAsync(pair:PairClass) = (x :> IBitFinexApi).GetOrderBookAsync(pair)
         member x.GetOrderBookAsync(pair:PairClass, token:CancellationToken) = (x :> IBitFinexApi).GetOrderBookAsync(pair, token)
         member x.GetOrderBookAsync(pair:PairClass, ?token:CancellationToken) = (x :> IBitFinexApi).GetOrderBookAsync(pair, token)

         member x.GetTrades(pair:PairClass) = (x :> IBitFinexApi).GetTrades(pair)
         member x.AsyncGetTrades(pair:PairClass) = (x :> IBitFinexApi).AsyncGetTrades(pair)
         member x.GetTradesAsync(pair:PairClass) = (x :> IBitFinexApi).GetTradesAsync(pair)
         member x.GetTradesAsync(pair:PairClass, token:CancellationToken) = (x :> IBitFinexApi).GetTradesAsync(pair, token)
         member x.GetTradesAsync(pair:PairClass, ?token:CancellationToken) = (x :> IBitFinexApi).GetTradesAsync(pair, token)

         member x.GetLends(currency:string) = (x :> IBitFinexApi).GetLends(currency)
         member x.AsyncGetLends(currency:string) = (x :> IBitFinexApi).AsyncGetLends(currency)
         member x.GetLendsAsync(currency:string) = (x :> IBitFinexApi).GetLendsAsync(currency)
         member x.GetLendsAsync(currency:string, token:CancellationToken) = (x :> IBitFinexApi).GetLendsAsync(currency, token)
         member x.GetLendsAsync(currency:string, ?token:CancellationToken) = (x :> IBitFinexApi).GetLendsAsync(currency, token)

         member x.GetLendBook(currency:string) = (x :> IBitFinexApi).GetLendBook(currency)
         member x.AsyncGetLendBook(currency:string) = (x :> IBitFinexApi).AsyncGetLendBook(currency)
         member x.GetLendBookAsync(currency:string) = (x :> IBitFinexApi).GetLendBookAsync(currency)
         member x.GetLendBookAsync(currency:string, token:CancellationToken) = (x :> IBitFinexApi).GetLendBookAsync(currency, token)
         member x.GetLendBookAsync(currency:string, ?token:CancellationToken) = (x :> IBitFinexApi).GetLendBookAsync(currency, token)
                                                

    
                                       

            

