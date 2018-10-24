namespace CryptCurrency.BitFinex

open System
open System.Threading.Tasks
open System.Net
open System.Collections.Generic
open CryptCurrency.Common.DataContracts
open FSharp.Data;
open FSharp.Data.HttpRequestHeaders
open FSharp.Data.HttpMethod
open System.Text
open Newtonsoft.Json
open System.Security.Cryptography
open System.Linq

module Model =
    [<AllowNullLiteral>]
    type BitFinexResponse() = 
            let mutable result = ""
            let mutable message = ""
            let mutable orderId = ""
            [<JsonProperty(PropertyName = "result")>]
            member x.Result with get() = result
                            and set(v) = result <- v
            [<JsonProperty(PropertyName = "message")>]
            member x.Message with get() = message
                             and set(v) = message <- v
            [<JsonProperty(PropertyName = "order_id")>]
            member x.OrderId with get() = orderId
                             and set(v) = orderId <- v
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
                                static member Default = { Mid = 0.0m; Bid = 0.0m; Ask = 0.0m; LastPrice = 0.0m; 
                                                          Low = 0.0m; High = 0.0m; Volume = 0.0m; Timestamp = 0.0m; }

    type BitFinexStats =  {  Period : int; Volume : decimal }

    type BitFinexTrade =  { Timestamp : int; Tid : int; Price : decimal; 
                            Amount : decimal; Exchange : string; Type : string; }

    type BitFinexLend = {  Rate : decimal; AmountLent : decimal; AmountUsed : decimal; Timestamp : int }

    type BitFinexPairDetails = {  Pair : string; PricePrecision : int; InitialMargin : decimal; 
                                  MinimumMargin : decimal;  MaximumOrderSize : decimal; 
                                  Expiration : string; Margin : bool }

    [<AllowNullLiteral>]
    type BitFinexOrderStatus() = 
            let mutable id:int = 0
            let mutable symbol = ""
            let mutable exchange = ""
            let mutable price = 0m
            let mutable avgExecutionPrice = 0m
            let mutable type' = ""
            let mutable timestamp = ""
            let mutable isLive = false
            let mutable isCancelled = false
            let mutable wasForced = false
            let mutable executedAmount = 0m
            let mutable remainingAmount = 0m
            let mutable originalAmount = 0m
            let mutable orderid:int = 0
            [<JsonProperty(PropertyName = "id")>]
            member x.Id with get() = id
                             and set(v) = id <- v
            [<JsonProperty(PropertyName = "symbol")>]
            member x.Symbol with get() = symbol
                             and set(v) = symbol <- v
            [<JsonProperty(PropertyName = "exchange")>]
            member x.Exchange with get() = exchange
                              and set(v) = exchange <- v
            [<JsonProperty(PropertyName = "price")>]
            member x.Price with get() = price
                              and set(v) = price <- v
            [<JsonProperty(PropertyName = "avg_execution_price")>]
            member x.AvgExecutionPrice with get() = avgExecutionPrice
                                        and set(v) = avgExecutionPrice <- v
            [<JsonProperty(PropertyName = "type")>]
            member x.Type with get() = type'
                              and set(v) = type' <- v
            [<JsonProperty(PropertyName = "timestamp")>]
            member x.Timestamp with get() = timestamp
                                 and set(v) = timestamp <- v
            [<JsonProperty(PropertyName = "is_live")>]
            member x.IsLive with get() = isLive
                                 and set(v) = isLive <- v
            [<JsonProperty(PropertyName = "is_cancelled")>]
            member x.IsCancelled with get() = isCancelled
                                 and set(v) = isCancelled <- v
            [<JsonProperty(PropertyName = "was_forced")>]
            member x.WasForced with get() = wasForced
                                 and set(v) = wasForced <- v
            [<JsonProperty(PropertyName = "executed_amount")>]
            member x.ExecutedAmount with get() = executedAmount
                                    and set(v) = executedAmount <- v
            [<JsonProperty(PropertyName = "remaining_amount")>]
            member x.RemainingAmount with get() = remainingAmount
                                     and set(v) = remainingAmount <- v
            [<JsonProperty(PropertyName = "original_amount")>]
            member x.OriginalAmount with get() = originalAmount
                                     and set(v) = originalAmount <- v
            [<JsonProperty(PropertyName = "order_id")>]
            member x.OrderId with get() = orderid
                                     and set(v) = orderid <- v

    [<AllowNullLiteral>]
    type BitFinexWalletBalance() = 
            let mutable type' = ""
            let mutable currency = ""
            let mutable amount = 0m
            let mutable available = 0m
            [<JsonProperty(PropertyName = "type")>]
            member x.Type with get() = type'
                              and set(v) = type' <- v
            [<JsonProperty(PropertyName = "currency")>]
            member x.Currency with get() = currency
                                 and set(v) = currency <- v
            [<JsonProperty(PropertyName = "amount")>]
            member x.Amount with get() = amount
                                 and set(v) = amount <- v
            [<JsonProperty(PropertyName = "available")>]
            member x.Available with get() = available
                                 and set(v) = available <- v

    [<AllowNullLiteral>]
    type BitFinexFee() = 
            let mutable pairs = ""
            let mutable makerFees = 0m
            let mutable takerFees = 0m
            [<JsonProperty(PropertyName = "pair")>]
            member x.Pairs with get() = pairs
                              and set(v) = pairs <- v
            [<JsonProperty(PropertyName = "maker_fees")>]
            member x.MakerFees with get() = makerFees
                                 and set(v) = makerFees <- v
            [<JsonProperty(PropertyName = "taker_fees")>]
            member x.TakerFees with get() = takerFees
                                 and set(v) = takerFees <- v

    [<AllowNullLiteral>]
    type BitFinexAccountInfo() = 
            let mutable fees:List<BitFinexFee> = null
            [<JsonProperty(PropertyName = "fees")>]
            member x.Fees with get() = fees
                              and set(v) = fees <- v

    [<AllowNullLiteral>]
    type BitFinexMarginLimit() = 
            let mutable onpair = ""
            let mutable initialMargin = 0m
            let mutable marginRequirement = 0m
            let mutable tradableBalance = 0m
            [<JsonProperty(PropertyName = "on_pair")>]
            member x.Pairs with get() = onpair
                           and set(v) = onpair <- v
            [<JsonProperty(PropertyName = "initial_margin")>]
            member x.InitialMargin with get() = initialMargin
                                   and set(v) = initialMargin <- v
            [<JsonProperty(PropertyName = "margin_requirement")>]
            member x.MarginRequirement with get() = marginRequirement
                                       and set(v) = marginRequirement <- v
            [<JsonProperty(PropertyName = "tradable_balance")>]
            member x.TradableBalance with get() = tradableBalance
                                     and set(v) = tradableBalance <- v
    
    [<AllowNullLiteral>]
    type BitFinexMargin() = 
            let mutable marginBalance = ""
            let mutable tradableBalance = ""
            let mutable unrealizedPl = 0
            let mutable unrealizedSwap = 0
            let mutable netValue = ""
            let mutable requiredMargin = 0
            let mutable leverage = ""
            let mutable marginRequirement = ""
            let mutable marginLimits:List<BitFinexMarginLimit> = null
            let mutable message = ""
            [<JsonProperty(PropertyName = "margin_balance")>]
            member x.MarginBalance with get() = marginBalance
                                    and set(v) = marginBalance <- v
            [<JsonProperty(PropertyName = "tradable_balance")>]
            member x.TradableBalance with get() = tradableBalance
                                     and set(v) = tradableBalance <- v
            [<JsonProperty(PropertyName = "unrealized_pl")>]
            member x.UnrealizedPl with get() = unrealizedPl
                                  and set(v) = unrealizedPl <- v
            [<JsonProperty(PropertyName = "unrealized_swap")>]
            member x.UnrealizedSwap with get() = unrealizedSwap
                                    and set(v) = unrealizedSwap <- v
            [<JsonProperty(PropertyName = "net_value")>]
            member x.NetValue with get() = netValue
                              and set(v) = netValue <- v
            [<JsonProperty(PropertyName = "required_margin")>]
            member x.RequiredMargin with get() = requiredMargin
                                    and set(v) = requiredMargin <- v
            [<JsonProperty(PropertyName = "leverage")>]
            member x.Leverage with get() = leverage
                              and set(v) = leverage <- v
            [<JsonProperty(PropertyName = "margin_requirement")>]
            member x.MarginRequirement with get() = marginRequirement
                                       and set(v) = marginRequirement <- v
            [<JsonProperty(PropertyName = "margin_limits")>]
            member x.MarginLimits with get() = marginLimits
                                  and set(v) = marginLimits <- v
            [<JsonProperty(PropertyName = "message")>]
            member x.Message with get() = message
                             and set(v) = message <- v

    [<AllowNullLiteral>]
    type BitFinexMyTrade() = 
            [<JsonProperty(PropertyName = "price")>]
            member val Price = 0m  with get, set
            [<JsonProperty(PropertyName = "amount")>]
            member val Amount = 0m with get, set
            [<JsonProperty(PropertyName = "timestamp")>]
            member val TimeStamp=double 0.0 with get, set
            [<JsonProperty(PropertyName = "until")>]
            member val Until=0L with get, set
            [<JsonProperty(PropertyName = "exchange")>]
            member val Exchange="" with get, set
            [<JsonProperty(PropertyName = "type")>]
            member val Type="" with get, set
            [<JsonProperty(PropertyName = "fee_currency")>]
            member val FeeCurrency="" with get, set
            [<JsonProperty(PropertyName = "fee_amount")>]
            member val FeeAmount=0m with get, set
            [<JsonProperty(PropertyName = "tid")>]
            member val Tid=0L with get, set
            [<JsonProperty(PropertyName = "order_id")>]
            member val OrderId=0L with get, set

         
module Utils =
    open System.Globalization;

    let getNonce = let currentNonce = DateTime.UtcNow.AddDays(1.0).Ticks
                   currentNonce

    let convert x = 
        let d = Dictionary<string, obj>()
        x |> Seq.iter d.Add
        d

    let paramsToJsonToBase64 parameters = let json = JsonConvert.SerializeObject(convert parameters, Formatting.None)
                                          let bytes = Encoding.UTF8.GetBytes(json)
                                          Convert.ToBase64String(bytes)

    let paramsDictToJsonToBase64(parameters:Dictionary<string, obj>) = let json = JsonConvert.SerializeObject(parameters, Formatting.None)
                                                                       let bytes = Encoding.UTF8.GetBytes(json)
                                                                       Convert.ToBase64String(bytes)

    let SignHMacSha384 (key:string, data:byte[]) : byte[] = let hashMaker = new HMACSHA384(Encoding.ASCII.GetBytes(key))
                                                            hashMaker.ComputeHash(data)

    let Sign(payload: string, secretKey:string) = let data = BitConverter.ToString(SignHMacSha384(secretKey, Encoding.UTF8.GetBytes(payload)))
                                                  data.Replace("-", String.Empty).ToLower()

    let UnixTimeStampToDateTime(unixTimeStamp:double) : DateTime = let dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                                                                   let dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime()
                                                                   dtDateTime

    let addDecompression (req: HttpWebRequest) =
        req.AutomaticDecompression <- DecompressionMethods.GZip ||| DecompressionMethods.Deflate
        req

    [<Literal>]
    let internal XBFXAPIKEY = "X-BFX-APIKEY"
    [<Literal>]
    let internal XBFXPAYLOAD = "X-BFX-PAYLOAD"
    [<Literal>]
    let internal XBFXSIGNATURE = "X-BFX-SIGNATURE"

    let privateQuery(key: string, secret : string, url: string, parameters) : string = 
        let result = Http.RequestString(url , headers = [ (XBFXAPIKEY, key); (XBFXPAYLOAD, paramsToJsonToBase64 parameters); 
                                                          (XBFXSIGNATURE, Sign((paramsToJsonToBase64 parameters), secret)) ], 
                                                          customizeHttpRequest = addDecompression) 
        result

    let PrivateQuery(key: string, secret : string, url: string, parameters: seq<string * obj>) : string = 
        try
            let parameters' = Seq.append parameters [("nonce", getNonce.ToString("D") :> obj)]
            let resultData = privateQuery(key, secret, url, parameters')
            resultData
        with
        | _ as ex -> raise(ex)

    let Query<'T>(url) = let data = Http.RequestString(url)
                         JsonConvert.DeserializeObject<'T>(data)

    [<Literal>]
    let private EnglishCultureName = "English";

    type CultureHelper=
        static member GetEnglishCulture() =
                    let cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    // get culture by it's english name 
                    let culture = cultures.FirstOrDefault(fun c -> c.EnglishName.Equals(EnglishCultureName, StringComparison.InvariantCultureIgnoreCase))
                    culture
[<AutoOpen>]
module Constants = 
  [<Literal>]
  let TypeMarket = "market"
  [<Literal>]
  let TypeLimit = "limit"
  [<Literal>]
  let TypeStop = "stop"
  [<Literal>]
  let TypeTrailingStop = "trailing-stop"
  [<Literal>]
  let TypeFillOrKill = "fill-or-kill"
  [<Literal>]
  let TypeExchangeMarket = "exchange market"
  [<Literal>]
  let TypeExchangeLimit = "exchange limit"
  [<Literal>]
  let TypeExchangeStop = "exchange stop"
  [<Literal>]
  let TypeExchangeTrailingStop = "exchange trailing-stop"
  [<Literal>]
  let TypeExchangeFillOrKill = "exchange fill-or-kill"

module WebApi =
    open Utils
    open Model
    open CryptCurrency.Common.Helpers
    open CryptCurrency.Common.Retry
    open CryptCurrency.Common.StringEx
    open System.Net.Http
    open System.Threading
    open System.IO

    type BitFinexOrderSide=
    |  Sell
    |  Buy


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
//--------------------------------------------------------------------------------------------------------
        abstract AsyncGetWalletBalances: unit -> Async<List<BitFinexWalletBalance>>
        abstract AsyncGetActiveOrders: unit -> Async<List<BitFinexOrderStatus>>
        abstract AsyncGetAccountInfos: unit -> Async<List<BitFinexAccountInfo>>
        abstract AsyncGetMarginInfos: unit -> Async<List<BitFinexMargin>>
        abstract AsyncNewOrder: Order * BitFinexOrderSide * string -> Async<BitFinexOrderStatus>
        abstract AsyncCancelOrder: int -> Async<BitFinexOrderStatus>
        abstract AsyncCancelAllOrder: unit -> Async<BitFinexResponse>
        abstract AsyncGetOrderStatus: int -> Async<BitFinexOrderStatus>
        abstract AsyncGetMyTrades: PairClass * double option * int option -> Async<List<BitFinexMyTrade>>
        

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

    let private AsyncWebRequestGetString (url:string, token:CancellationToken, webtimeout: int) = 
                                    async {
                                            use handler = new HttpClientHandler()
                                            handler.AutomaticDecompression <- DecompressionMethods.Deflate ||| DecompressionMethods.GZip
                                            use httpClient = new System.Net.Http.HttpClient(handler)
                                            httpClient.Timeout <- new TimeSpan(0,0,0,0, webtimeout)
                                            use! response = httpClient.GetAsync(url, token) |> Async.AwaitTask
                                            response.EnsureSuccessStatusCode () |> ignore
                                            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                                            return content
                                         }

    let private AsyncPrivateQuery(url:string, parameters: seq<string * obj>, apiKey:string, apiSecret:string, webtimeout: int) =
                                    async {
                                            use handler = new HttpClientHandler()
                                            handler.AutomaticDecompression <- DecompressionMethods.Deflate ||| DecompressionMethods.GZip

                                            use httpClient = new System.Net.Http.HttpClient(handler)
                                            httpClient.Timeout <- TimeSpan.FromMilliseconds(float webtimeout)

                                            httpClient.DefaultRequestHeaders.Add(XBFXAPIKEY, apiKey)
                                            httpClient.DefaultRequestHeaders.Add(XBFXPAYLOAD, paramsToJsonToBase64(parameters))
                                            httpClient.DefaultRequestHeaders.Add(XBFXSIGNATURE, Sign((paramsToJsonToBase64(parameters)), apiSecret))

                                            use queryString = new StringContent(JsonConvert.SerializeObject(parameters, Formatting.None))
                                            use! response = httpClient.PostAsync(new Uri(url), queryString ) |> Async.AwaitTask
                                            response.EnsureSuccessStatusCode() |> ignore
                                            return! response.Content.ReadAsStringAsync() |> Async.AwaitTask
                                        }

    let private PrivateQuery<'T>(url:string, parameters: seq<string * obj>, 
                                 apiKey:string, apiSecret:string, 
                                 webtimeout: int): Async<'T> =
                                 async{
                                    try
                                        let parameters' = 
                                            let m = (Seq.length parameters)
                                            match m with
                                            | 1 -> seq { yield Seq.item 0 parameters; yield ("nonce", getNonce.ToString("D") :> obj); } 
                                            | n when n > 1 -> seq { yield Seq.item 0 parameters; yield ("nonce", getNonce.ToString("D") :> obj);  for i in 1..(Seq.length parameters) do yield (Seq.item i parameters) }
                                            | _ -> raise(Exception("parameters can't be empty sequence"))
                                        #if DEBUG
                                        printfn "PrivateQuery parameters %A" parameters'
                                        #endif
                                        let! resultData = AsyncPrivateQuery(url, parameters', apiKey, apiSecret, webtimeout)
                                        let mutable response:BitFinexResponse = null
                                        try
                                            response <- JsonConvert.DeserializeObject<BitFinexResponse>(resultData)
                                        with 
                                            | _ -> ()
                                        if response <> Unchecked.defaultof<_> && not response.IsSuccess then 
                                            return raise(Exception(response.Message))
                                        else
                                            return JsonConvert.DeserializeObject<'T>(resultData)
                                    with 
                                        | _ as ex -> return reraise' ex
                                 }

    let private GetActiveOrders(apiKey:string, apiSecret:string, webtimeout:int)=
                                async{
                                    let methods = "v1/orders"
                                    let! data = PrivateQuery<List<BitFinexOrderStatus>>(baseUrl +^ methods, seq {  yield ("request", ("/" + methods) :> obj) }, apiKey, apiSecret, webtimeout)
                                    return data
                                }
    
    let private GetWalletBalances(apiKey:string, apiSecret:string, webtimeout:int)=
                                async{
                                    let methods = "v1/balances"
                                    let! data = PrivateQuery<List<BitFinexWalletBalance>>(baseUrl +^ methods, [("request", ("/" + methods) :> obj)], apiKey, apiSecret, webtimeout)
                                    return data
                                }

    let private GetAccountInfos(apiKey:string, apiSecret:string, webtimeout:int)=
                                async{
                                    let methods = "v1/account_infos"
                                    let! data = PrivateQuery<List<BitFinexAccountInfo>>(baseUrl +^ methods, [("request", ("/" + methods) :> obj)], apiKey, apiSecret, webtimeout)
                                    return data
                                }

    let private GetMarginInfos(apiKey:string, apiSecret:string, webtimeout:int)=
                                async{
                                    let methods = "v1/margin_infos"
                                    let! data = PrivateQuery<List<BitFinexMargin>>(baseUrl +^ methods, [("request", ("/" + methods) :> obj)], apiKey, apiSecret, webtimeout)
                                    return data
                                }  
                                
    let private NewOrder(order:Order, side:BitFinexOrderSide,type':string,apiKey:string, apiSecret:string, webtimeout:int)=
                                async{
                                    if type'.IsNullOrEmpty() then nullArg "parameter type' can't be null or empty."
                                    let methods = "v1/order/new"
                                    let culture = CultureHelper.GetEnglishCulture()
                                    let! data = PrivateQuery<BitFinexOrderStatus>(baseUrl +^ methods, 
                                                                                        [("request", ("/" + methods) :> obj);
                                                                                         ("symbol", order.Pair.ToString() :> obj);
                                                                                         ("amount", Convert.ToString(order.Amount, culture.NumberFormat) :> obj);
                                                                                         ("price", Convert.ToString(order.Price, culture.NumberFormat) :> obj);
                                                                                         ("exchange", "bitfinex" :> obj);
                                                                                         ("side", side.ToString().ToLower() :> obj);
                                                                                         ("type", (sprintf "exchange %s" type').ToLower():> obj);
                                                                                        ],
                                                                                        apiKey, apiSecret, webtimeout)
                                    return data
                                }

    let private CancelOrder(orderId:int, apiKey:string, apiSecret:string, webtimeout:int)=
                                async{
                                    let methods = "v1/order/cancel"
                                    let! data = PrivateQuery<BitFinexOrderStatus>(baseUrl +^ methods, 
                                                                                        [("request", ("/" + methods) :> obj);
                                                                                         ("order_id", orderId :> obj)],
                                                                                        apiKey, apiSecret, webtimeout)
                                    return data
                                }
      
    let private CancelAllOrder(apiKey:string, apiSecret:string, webtimeout:int)=
                                async{
                                    let methods = "v1/order/cancel/all"
                                    let! data = PrivateQuery<BitFinexResponse>(baseUrl +^ methods, [("request", ("/" + methods) :> obj)], apiKey, apiSecret, webtimeout)
                                    return data
                                }

    let private GetOrderStatus(orderId:int, apiKey:string, apiSecret:string, webtimeout:int)=
                                async{
                                    let methods = "v1/order/status"
                                    let! data = PrivateQuery<BitFinexOrderStatus>(baseUrl +^ methods, 
                                                                                        [("request", ("/" + methods) :> obj);
                                                                                         ("order_id", orderId :> obj)],
                                                                                        apiKey, apiSecret, webtimeout)
                                    return data
                                }

    let private GetMyTrades(pair:PairClass, timeStamp:double,limitTrades:int,apiKey:string, apiSecret:string, webtimeout:int)=
                                async{
                                    let methods = "v1/mytrades"
                                    let mutable parameters: seq<string * obj> = null
                                    parameters <- [("request", ("/" + methods) :> obj);
                                                   ("symbol", pair.ToString().ToUpper() :> obj);
                                                   ("limit_trades", limitTrades :> obj)]
                                    
                                    if timeStamp > 0.0 then parameters <- Seq.append parameters [("timestamp", timeStamp :> obj)]

                                    let! data = PrivateQuery<List<BitFinexMyTrade>>(baseUrl +^ methods, parameters, apiKey, apiSecret, webtimeout)
                                    return data
                                }


    let inline private getWebResponseMessage(response:WebResponse) = use stream = response.GetResponseStream() 
                                                                     use reader = new StreamReader(stream) 
                                                                     let message = reader.ReadToEnd()
                                                                     message

    let private handleHttpRequestException(e:System.Net.Http.HttpRequestException) = if e.Message.Contains("400 (Bad Request)") then 
                                                                                        Seq.empty 
                                                                                     else 
                                                                                        reraise' e

    let private handleWebException(e:System.Net.WebException) = let body = getWebResponseMessage(e.Response)
                                                                if body.Contains("message") && body.Contains("Unknown") && e.Message.Contains("(400) Bad Request") then 
                                                                    Seq.empty 
                                                                else 
                                                                    reraise' e

    let inline private getSupportedPairsFrom(pairs:string[]) = pairs |> Seq.map (fun x -> let data = x.toArrayByChunkSize(3)
                                                                                          PairClass(Seq.item 0 data, Seq.item 1 data))

    let private GetSupportedPairs webtimeout = let pairs = BitFinexPair.Parse(Http.RequestString(baseUrl +^ symbols, httpMethod = Get, 
                                                                                         headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], 
                                                                                         customizeHttpRequest = addDecompression, timeout = webtimeout))
                                               getSupportedPairsFrom(pairs)

    let private AsyncGetSupportedPairs webtimeout = async{  let! pairs =  Http.AsyncRequestString(baseUrl +^ symbols, httpMethod = Get, 
                                                                                          headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], 
                                                                                          customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                            let pairs = BitFinexPair.Parse(pairs)
                                                            return getSupportedPairsFrom(pairs)  }

    let private GetSupportedPairsAsync(token:CancellationToken, webtimeout: int) = async{  
                                                    try
                                                        let! pairs =  AsyncWebRequestGetString(baseUrl +^ symbols, token, webtimeout)
                                                        let pairs = BitFinexPair.Parse(pairs)
                                                        return getSupportedPairsFrom(pairs)
                                                    with 
                                                        | :? OperationCanceledException -> return Seq.empty 
                                                    }

    let inline private getPairDetailsFrom(pairDetails:BitFinexPairDetails.Root[]) = pairDetails |> Seq.map (fun x -> { Pair = x.Pair; PricePrecision = x.PricePrecision; 
                                                                                                                       InitialMargin = x.InitialMargin; MinimumMargin = x.MinimumMargin; 
                                                                                                                       MaximumOrderSize = x.MaximumOrderSize; 
                                                                                                                       Expiration = x.Expiration; Margin = x.Margin }); 

    let private GetPairDetails webtimeout = let pairDetails = BitFinexPairDetails.Parse(Http.RequestString(baseUrl +^ "symbols_details", httpMethod = Get, 
                                                                                                   headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], 
                                                                                                   customizeHttpRequest = addDecompression, timeout = webtimeout))
                                            getPairDetailsFrom(pairDetails)

    let private AsyncGetPairDetails webtimeout = async{  let! pairDetails =  Http.AsyncRequestString(baseUrl +^ "symbols_details", httpMethod = Get, 
                                                                                             headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], 
                                                                                             customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                         let pairDetails = BitFinexPairDetails.Parse(pairDetails)
                                                         return getPairDetailsFrom(pairDetails)  }

    let private GetPairDetailsAsync(token:CancellationToken, webtimeout: int) = async {  
                                                                try
                                                                    let! pairDetailsJson =  AsyncWebRequestGetString(baseUrl +^ "symbols_details", token, webtimeout)
                                                                    let pairDetails = BitFinexPairDetails.Parse(pairDetailsJson)
                                                                    return getPairDetailsFrom(pairDetails)
                                                                with 
                                                                    | :? OperationCanceledException -> return Seq.empty 
                                                            }

    type WebApiError(code : int, msg : string, innerException:Exception) =
        inherit Exception(msg, innerException)
        member e.Code = code
        new (msg : string) = WebApiError(0, msg, null)
        new (msg : string, innerException:Exception) = WebApiError(0, msg, innerException)

    let private handleRaiseWebException(e:System.Net.WebException, werr: WebApiError) = let body = getWebResponseMessage(e.Response)
                                                                                        if body.Contains("message") && body.Contains("Unknown") && e.Message.Contains("(400) Bad Request") then 
                                                                                            raise(werr) 
                                                                                        else 
                                                                                            reraise' e
    
    let private handleRaiseHttpRequestException(e:System.Net.Http.HttpRequestException, werr: WebApiError) = if e.Message.Contains("400 (Bad Request)") then 
                                                                                                                raise(werr)
                                                                                                             else 
                                                                                                                reraise' e

    let inline private getOrderBookFrom(pair: PairClass, orderBook:BitFinexOrderBook.Root) = let bids = orderBook.Bids.Where(fun x -> x.Price > 0m && x.Amount > 0m).
                                                                                                              Select(fun b -> new Order(pair, b.Price, b.Amount, BitFinex, MarketSide.Bid, DateTime.UtcNow, 
                                                                                                                                        OrderType.Limit, ExternalExchange, TimeInForce.GoodTillCancel))
                                                                                             let asks = orderBook.Asks.Where(fun x -> x.Price > 0m && x.Amount > 0m).
                                                                                                              Select(fun b -> new Order(pair, b.Price, b.Amount, BitFinex, MarketSide.Ask, DateTime.UtcNow, 
                                                                                                                                        OrderType.Limit, ExternalExchange, TimeInForce.GoodTillCancel))
                                                                                             new OrderBook( bids, asks, BitFinex, pair, DateTime.UtcNow ) 


    let private getOrderBook (pair: PairClass) (webtimeout:int) = try
                                                                     let orderBook = BitFinexOrderBook.Parse(Http.RequestString(baseUrl +^ "book/" +^ pair.BaseCurrency +^ pair.CounterCurrency, 
                                                                                                                                customizeHttpRequest = addDecompression, timeout = webtimeout))
                                                                     getOrderBookFrom(pair,orderBook)
                                                                  with
                                                                  | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> 
                                                                        handleRaiseWebException(e, WebApiError(400, "Unknown Symbol from pair parameter", e))

    let private AsyncGetOrderBook (pair: PairClass) (webtimeout:int) = async {  
                                                                        try
                                                                           let! orderBook =  Http.AsyncRequestString(baseUrl +^ "book/" +^ pair.BaseCurrency +^ pair.CounterCurrency, httpMethod = Get, 
                                                                                                                     headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], 
                                                                                                                     customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                           let orderBook = BitFinexOrderBook.Parse(orderBook)
                                                                           return getOrderBookFrom(pair,orderBook)
                                                                        with
                                                                           | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> 
                                                                                return handleRaiseWebException(e, WebApiError(400, "Unknown Symbol from pair parameter", e))
                                                                     }

    let private GetOrderBookAsync(pair: PairClass, token:CancellationToken, webtimeout: int) = 
                                                            async{  
                                                                    try
                                                                        let! orderBook =  AsyncWebRequestGetString(baseUrl +^ "book/" +^ pair.BaseCurrency +^ pair.CounterCurrency, token, webtimeout)
                                                                        let orderBook = BitFinexOrderBook.Parse(orderBook)
                                                                        return getOrderBookFrom(pair,orderBook)
                                                                    with 
                                                                        | :? System.Net.Http.HttpRequestException as e -> 
                                                                                return handleRaiseHttpRequestException(e, WebApiError(400, "Unknown Symbol from pair parameter", e))
                                                            }

    let inline private getLendBookFrom(currency:string, landBook:BitFinexLendBook.Root) = { 
                                                                                             Currency = currency;
                                                                                             Asks = landBook.Asks |> Seq.map (fun x -> { Rate = x.Rate; Amount = x.Amount; 
                                                                                                                                  Period = x.Period; Timestamp = x.Timestamp; Frr = x.Frr }); 
                                                                                             Bids = landBook.Bids |> Seq.map (fun x -> { Rate = x.Rate; Amount = x.Amount; 
                                                                                                                                  Period = x.Period; Timestamp = x.Timestamp; Frr = x.Frr }); 
                                                                                           } 
                                      
    let private getLendBook(currency,webtimeout) =  try
                                                        let lendBook = BitFinexLendBook.Parse(Http.RequestString(baseUrl +^ "lendbook/" +^ currency, customizeHttpRequest = addDecompression, timeout = webtimeout))
                                                        getLendBookFrom(currency,lendBook)
                                                    with
                                                        | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> handleRaiseWebException(e, WebApiError(400, "Unknown Symbol from pair parameter", e))

    let private AsyncGetLendBook(currency,webtimeout) = async{ 
                                                                try
                                                                    let! lendBook =  Http.AsyncRequestString(baseUrl +^ "lendbook/" +^ currency, httpMethod = Get, 
                                                                                                             headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], 
                                                                                                             customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                    let lendBook = BitFinexLendBook.Parse(lendBook)
                                                                    return getLendBookFrom(currency,lendBook)
                                                                with
                                                                   | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> 
                                                                        return handleRaiseWebException(e, WebApiError(400, "Unknown Symbol from pair parameter", e))
                                                             }

    let private GetLendBookAsync(currency: string, token:CancellationToken, webtimeout: int) = 
                                                         async {  
                                                                    try
                                                                        let! lendBook =  AsyncWebRequestGetString(baseUrl +^ "lendbook/" +^ currency, token, webtimeout)
                                                                        let lendBook = BitFinexLendBook.Parse(lendBook)
                                                                        return  getLendBookFrom(currency,lendBook)
                                                                    with 
                                                                        | :? System.Net.Http.HttpRequestException as e -> 
                                                                            return handleRaiseHttpRequestException(e, WebApiError(400, "Unknown Symbol from pair parameter", e))
                                                         }
   
    let inline private getTickerFrom(data:BitFinexPubTicker.Root) = { Mid = data.Mid; Bid = data.Bid; Ask = data.Ask; LastPrice = data.LastPrice; 
                                                                      Low = data.Low; High = data.High; Volume = data.Volume; Timestamp = data.Timestamp; } 

    

    let private GetTicker (pair: PairClass) (webtimeout:int) =  try
                                                                    let tiker = BitFinexPubTicker.Parse(Http.RequestString(baseUrl +^ "pubticker/" +^ pair.BaseCurrency +^ pair.CounterCurrency, 
                                                                                                                           customizeHttpRequest = addDecompression, timeout = webtimeout))
                                                                    getTickerFrom(tiker)
                                                                with
                                                                    | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> 
                                                                         handleRaiseWebException(e, WebApiError(400, "Unknown Symbol from pair parameter", e)) 
    
    let private AsyncGetTicker (pair: PairClass) (webtimeout:int) = 
                                                                async{  
                                                                            try
                                                                                let! tiker =  Http.AsyncRequestString(baseUrl +^ "pubticker/" +^ pair.BaseCurrency +^ pair.CounterCurrency, httpMethod = Get, 
                                                                                                                      headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], 
                                                                                                                      customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                                let tiker = BitFinexPubTicker.Parse(tiker)
                                                                                return getTickerFrom(tiker) 
                                                                            with
                                                                                | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> 
                                                                                     return handleRaiseWebException(e, WebApiError(400, "Unknown Symbol from pair parameter", e)) 
                                                                }  
 
    let private GetTickerAsync(pair: PairClass, token:CancellationToken, webtimeout: int) = 
                                                    async {  
                                                                try
                                                                    let! tikerJson =  AsyncWebRequestGetString(baseUrl +^ "pubticker/" +^ pair.BaseCurrency +^ pair.CounterCurrency, token, webtimeout)
                                                                    let tiker = BitFinexPubTicker.Parse(tikerJson)
                                                                    return getTickerFrom(tiker) 
                                                                with 
                                                                    | :? System.Net.Http.HttpRequestException as e -> 
                                                                         return handleRaiseHttpRequestException(e, WebApiError(400, "Unknown Symbol from pair parameter", e))
                                                    }

    let inline private getStatsFrom(data:BitFinexPubStats.Root[]) = data |> Seq.map (fun x -> { Period = x.Period; Volume = x.Volume })

    let private GetStats (pair:PairClass) (webtimeout:int) = try
                                                                BitFinexPubStats.Parse(Http.RequestString(baseUrl +^ "stats/" +^ pair.BaseCurrency +^ pair.CounterCurrency, 
                                                                                                          customizeHttpRequest = addDecompression, timeout = webtimeout)) 
                                                                |> getStatsFrom
                                                             with
                                                                | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> handleWebException(e)

    let private AsyncGetStats (pair: PairClass) (webtimeout:int) = 
                                                            async{  
                                                                        try
                                                                            let! stats = Http.AsyncRequestString(baseUrl +^ "stats/" +^ pair.BaseCurrency +^ pair.CounterCurrency, httpMethod = Get, 
                                                                                                                 headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], 
                                                                                                                 customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                            let stats = BitFinexPubStats.Parse(stats)
                                                                            return getStatsFrom(stats)  
                                                                        with
                                                                            | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> return handleWebException(e)    
                                                            }

    let private GetStatsAsync(pair: PairClass, token:CancellationToken, webtimeout: int) = 
                                                    async{  
                                                            try
                                                                let! statsJson =  AsyncWebRequestGetString(baseUrl +^ "stats/" +^ pair.BaseCurrency +^ pair.CounterCurrency, token, webtimeout)
                                                                let stats = BitFinexPubStats.Parse(statsJson)
                                                                return getStatsFrom(stats)
                                                            with 
                                                                | :? OperationCanceledException -> return Seq.empty
                                                                | :? System.Net.Http.HttpRequestException as e -> return handleHttpRequestException(e)
                                                    }

    let inline private getTradesFrom(data:BitFinexPubTrades.Root[]) = data |> Seq.map (fun x -> { Timestamp = x.Timestamp; Tid = x.Tid; Price = x.Price; 
                                                                                                  Amount = x.Amount; Exchange = x.Exchange; Type = x.Type })

    let private GetTrades (pair:PairClass) (webtimeout:int) = try
                                                                 BitFinexPubTrades.Parse(Http.RequestString(baseUrl +^ "trades/" +^ pair.BaseCurrency +^ pair.CounterCurrency, 
                                                                                                            customizeHttpRequest = addDecompression, timeout = webtimeout)) 
                                                                 |> getTradesFrom
                                                              with
                                                                 | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> handleWebException(e)

    let private AsyncGetTrades (pair: PairClass) (webtimeout:int) = 
                                                                async{
                                                                       try
                                                                            let! trades = Http.AsyncRequestString(baseUrl +^ "trades/" +^ pair.BaseCurrency +^ pair.CounterCurrency, httpMethod = Get, 
                                                                                                                  headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], 
                                                                                                                  customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                            let trades = BitFinexPubTrades.Parse(trades)
                                                                            return getTradesFrom(trades)  
                                                                       with
                                                                          | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> return handleWebException(e)
                                                                }
    

    let private GetTradesAsync(pair: PairClass, token:CancellationToken, webtimeout: int) = 
                                                                async {  
                                                                        try
                                                                            let! trades =  AsyncWebRequestGetString(baseUrl +^ "trades/" +^ pair.BaseCurrency +^ pair.CounterCurrency, token, webtimeout)
                                                                            let trades = BitFinexPubTrades.Parse(trades)
                                                                            return getTradesFrom(trades)
                                                                        with 
                                                                            | :? OperationCanceledException -> return Seq.empty
                                                                            | :? System.Net.Http.HttpRequestException as e -> return handleHttpRequestException(e)
                                                                }

    let inline private getLendsFrom(data:BitFinexLends.Root[]) = data |> Seq.map (fun x -> { Rate = x.Rate; AmountLent = x.AmountLent; AmountUsed = x.AmountUsed; Timestamp = x.Timestamp })

    let private getLends(currency,webtimeout) = try
                                                    BitFinexLends.Parse(Http.RequestString(baseUrl +^ "lends/" +^ currency, customizeHttpRequest = addDecompression, timeout = webtimeout))
                                                    |> getLendsFrom
                                                with
                                                    | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> handleWebException(e)

    let private AsyncGetLends (currency: string) (webtimeout:int) = 
                                                            async{  
                                                                    try
                                                                        let! lends = Http.AsyncRequestString(baseUrl +^ "lends/" +^ currency, httpMethod = Get, 
                                                                                                             headers = [ Accept HttpContentTypes.Json; ContentType HttpContentTypes.Json ], 
                                                                                                             customizeHttpRequest = addDecompression, timeout = webtimeout)
                                                                        let lends = BitFinexLends.Parse(lends)
                                                                        return getLendsFrom(lends)
                                                                    with 
                                                                        | :? WebException as e when e.Status = WebExceptionStatus.ProtocolError -> return handleWebException(e)
                                                            }

    let private GetLendsAsync(currency: string, token:CancellationToken, webtimeout: int) = 
                                                        async{  
                                                                try
                                                                    let! lends =  AsyncWebRequestGetString(baseUrl +^ "lends/" +^ currency, token, webtimeout)
                                                                    let lends = BitFinexLends.Parse(lends)
                                                                    return getLendsFrom(lends)
                                                                with 
                                                                    | :? OperationCanceledException -> return Seq.empty 
                                                                    | :? System.Net.Http.HttpRequestException as e -> return handleHttpRequestException(e)
                                                        }

    type BitFinexApi(key:string, secret:string, ?timeout:int)=
         let apiKey = key
         let apiSecret = secret
         let webtimeout = defaultArg timeout 5000 
         let defaultRetryParams = { maxRetries = 3; waitBetweenRetries = 0 }

         do 
            if timeout.IsSome && timeout.Value <= 0 then invalidArg "timeout" "timeout must be greater than zero."
            if key.IsNullOrEmpty() then nullArg "key can't be null or empty."
            if secret.IsNullOrEmpty() then nullArg "secret can't be null or empty."

         member x.RunWithRetries f (retries:RetryParams) =
                let m = retries.maxRetries - 1
                match m with
                | 0 -> f()
                | _ -> try
                            f()
                       with
                       | _ -> x.RunWithRetries f ({ retries with maxRetries=retries.maxRetries-1 })

         member x.AsyncRunWithRetries (f : unit -> Async<_>, retries:RetryParams) : _ =
            let rec loop = function
              | 0, Some(ex) -> 
                               #if DEBUG
                               printfn "async loop Zero(0) raise"
                               #endif
                               raise ex
              | n, _ -> 
                        async { 
                                  try
                                    #if DEBUG
                                    printfn "async loop %i" n
                                    #endif
                                    return! f()
                                  with ex ->return! loop (n-1, Some(ex))
                              }
            loop(retries.maxRetries, None)

         interface IBitFinexApi with
            member x.GetSupportedPairs() = x.RunWithRetries (fun()->(GetSupportedPairs webtimeout)) defaultRetryParams
            member x.AsyncGetSupportedPairs() = x.AsyncRunWithRetries ((fun()->AsyncGetSupportedPairs webtimeout), defaultRetryParams)
            /// Expose C#-friendly asynchronous method that returns Task
            member x.GetSupportedPairsAsync() = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->AsyncGetSupportedPairs webtimeout), defaultRetryParams)) 
            /// Expose C#-friendly asynchronous method that returns Task
            /// and takes cancellation token to support cancellation...
            member x.GetSupportedPairsAsync(token:CancellationToken) = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetSupportedPairsAsync(token, webtimeout)), defaultRetryParams))
            member x.GetSupportedPairsAsync(?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                        Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetSupportedPairsAsync(deftoken, webtimeout)), defaultRetryParams))
                                                                        

            member x.GetPairDetails() = x.RunWithRetries (fun()->(GetPairDetails webtimeout)) defaultRetryParams
            member x.AsyncGetPairDetails() = x.AsyncRunWithRetries ((fun()->AsyncGetPairDetails webtimeout), defaultRetryParams)
            member x.GetPairDetailsAsync() = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->AsyncGetPairDetails(webtimeout)), defaultRetryParams))
            member x.GetPairDetailsAsync(token:CancellationToken) = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetPairDetailsAsync(token, webtimeout)), defaultRetryParams))
            member x.GetPairDetailsAsync(?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                     Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetPairDetailsAsync(deftoken, webtimeout)), defaultRetryParams))

            
            member x.GetTickers(pair:PairClass) = x.RunWithRetries (fun()->(GetTicker pair webtimeout)) defaultRetryParams
            member x.AsyncGetTickers(pair:PairClass) = x.AsyncRunWithRetries ((fun()->AsyncGetTicker pair webtimeout), defaultRetryParams)
            member x.GetTickersAsync(pair:PairClass) = Async.StartAsTask( x.AsyncRunWithRetries ((fun()->AsyncGetTicker pair webtimeout), defaultRetryParams))
            member x.GetTickersAsync(pair:PairClass, token:CancellationToken) = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetTickerAsync(pair, token, webtimeout)), defaultRetryParams))
            member x.GetTickersAsync(pair:PairClass, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                                 Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetTickerAsync(pair, deftoken, webtimeout)), defaultRetryParams))

            member x.GetStats(pair:PairClass) = x.RunWithRetries (fun()-> (GetStats pair webtimeout)) defaultRetryParams
            member x.AsyncGetStats(pair:PairClass) = x.AsyncRunWithRetries ((fun()->AsyncGetStats pair webtimeout), defaultRetryParams)
            member x.GetStatsAsync(pair:PairClass) = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->AsyncGetStats pair webtimeout), defaultRetryParams))
            member x.GetStatsAsync(pair:PairClass, token:CancellationToken) = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetStatsAsync(pair, token, webtimeout)), defaultRetryParams))
            member x.GetStatsAsync(pair:PairClass, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                               Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetStatsAsync(pair, deftoken, webtimeout)), defaultRetryParams))

            member x.GetOrderBook(pair:PairClass) = x.RunWithRetries (fun()->(getOrderBook pair webtimeout)) defaultRetryParams
            member x.AsyncGetOrderBook(pair:PairClass) = x.AsyncRunWithRetries ((fun()->AsyncGetOrderBook pair webtimeout), defaultRetryParams)
            member x.GetOrderBookAsync(pair:PairClass) = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->AsyncGetOrderBook pair webtimeout), defaultRetryParams))
            member x.GetOrderBookAsync(pair:PairClass, token:CancellationToken) = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetOrderBookAsync(pair, token, webtimeout)), defaultRetryParams))
            member x.GetOrderBookAsync(pair:PairClass, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                                   Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetOrderBookAsync(pair, deftoken, webtimeout)), defaultRetryParams))

            member x.GetTrades(pair:PairClass) = x.RunWithRetries (fun()->(GetTrades pair webtimeout)) defaultRetryParams
            member x.AsyncGetTrades(pair:PairClass) = x.AsyncRunWithRetries ((fun()->AsyncGetTrades pair webtimeout), defaultRetryParams)
            member x.GetTradesAsync(pair:PairClass) = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->AsyncGetTrades pair webtimeout), defaultRetryParams))
            member x.GetTradesAsync(pair:PairClass, token:CancellationToken) = Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetTradesAsync(pair, token, webtimeout)), defaultRetryParams))
            member x.GetTradesAsync(pair:PairClass, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                                Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetTradesAsync(pair, deftoken, webtimeout)), defaultRetryParams))

            member x.GetLends(currency:string) = if isNull currency then nullArg "currency" else x.RunWithRetries (fun()->getLends(currency,webtimeout)) defaultRetryParams
            member x.AsyncGetLends(currency:string) = if isNull currency then nullArg "currency" else x.AsyncRunWithRetries ((fun()->AsyncGetLends currency webtimeout),  defaultRetryParams) 
            member x.GetLendsAsync(currency:string) = if isNull currency then nullArg "currency" else Async.StartAsTask(x.AsyncRunWithRetries ((fun()->AsyncGetLends currency webtimeout),  defaultRetryParams))
            member x.GetLendsAsync(currency:string, token:CancellationToken)  = if isNull currency then nullArg "currency" else Async.StartAsTask( x.AsyncRunWithRetries ((fun()->GetLendsAsync(currency, token, webtimeout)),  defaultRetryParams))
            member x.GetLendsAsync(currency:string, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                                match currency with
                                                                                | null -> nullArg "currency" 
                                                                                | _ -> Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetLendsAsync(currency, deftoken, webtimeout)),  defaultRetryParams))

            member x.GetLendBook(currency:string) = if isNull currency then nullArg "currency" else x.RunWithRetries (fun()->getLendBook(currency,webtimeout)) defaultRetryParams
            member x.AsyncGetLendBook(currency:string) = if isNull currency then nullArg "currency" else x.AsyncRunWithRetries ((fun()->AsyncGetLendBook(currency, webtimeout)),  defaultRetryParams)
            member x.GetLendBookAsync(currency:string) = if isNull currency then nullArg "currency" else Async.StartAsTask(x.AsyncRunWithRetries ((fun()->AsyncGetLendBook(currency, webtimeout)),  defaultRetryParams))
            member x.GetLendBookAsync(currency:string, token:CancellationToken) = if isNull currency then nullArg "currency" else Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetLendBookAsync(currency, token, webtimeout)),  defaultRetryParams))
            member x.GetLendBookAsync(currency:string, ?token:CancellationToken) = let deftoken = defaultArg token Async.DefaultCancellationToken
                                                                                   match currency with
                                                                                   | null -> nullArg "currency" 
                                                                                   | _ -> Async.StartAsTask(x.AsyncRunWithRetries ((fun()->GetLendBookAsync(currency, deftoken, webtimeout)),  defaultRetryParams))

            member x.AsyncGetWalletBalances() = x.AsyncRunWithRetries ((fun()->GetWalletBalances(apiKey, apiSecret, webtimeout)),  defaultRetryParams)
            member x.AsyncGetActiveOrders() = x.AsyncRunWithRetries ((fun()->GetActiveOrders(apiKey, apiSecret, webtimeout)),  defaultRetryParams)
            member x.AsyncGetAccountInfos() = x.AsyncRunWithRetries ((fun()->GetAccountInfos(apiKey, apiSecret, webtimeout)),  defaultRetryParams)
            member x.AsyncGetMarginInfos() = x.AsyncRunWithRetries ((fun()->GetMarginInfos(apiKey, apiSecret, webtimeout)),  defaultRetryParams)
            member x.AsyncNewOrder(order:Order, side:BitFinexOrderSide,type':string) = if isNull type' then nullArg "type'"
                                                                                       x.AsyncRunWithRetries ((fun()->NewOrder(order, side, type', apiKey, apiSecret, webtimeout)),  defaultRetryParams)
            member x.AsyncCancelOrder(orderId:int) = x.AsyncRunWithRetries ((fun()->CancelOrder(orderId, apiKey, apiSecret, webtimeout)),  defaultRetryParams)
            member x.AsyncCancelAllOrder() = x.AsyncRunWithRetries ((fun()->CancelAllOrder(apiKey, apiSecret, webtimeout)),  defaultRetryParams)
            member x.AsyncGetOrderStatus(orderId:int) = x.AsyncRunWithRetries ((fun()->GetOrderStatus(orderId, apiKey, apiSecret, webtimeout)),  defaultRetryParams)
            member x.AsyncGetMyTrades(pair:PairClass, ?timeStamp:double,?limitTrades:int) = let timeStamp' = defaultArg timeStamp 0.0
                                                                                            let limitTrades' = defaultArg limitTrades 1000
                                                                                            x.AsyncRunWithRetries ((fun()->GetMyTrades(pair, timeStamp', limitTrades', apiKey, apiSecret, webtimeout)),  defaultRetryParams)
            

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
//------------------------------------------------------------------------------------------------------------------------------------------------
         member x.AsyncGetWalletBalances() = (x :> IBitFinexApi).AsyncGetWalletBalances()
         member x.AsyncGetActiveOrders() = (x :> IBitFinexApi).AsyncGetActiveOrders()
         member x.AsyncGetAccountInfos() = (x :> IBitFinexApi).AsyncGetAccountInfos()
         member x.AsyncGetMarginInfos() = (x :> IBitFinexApi).AsyncGetMarginInfos()
         member x.AsyncNewOrder(order:Order, side:BitFinexOrderSide, type':string) = (x :> IBitFinexApi).AsyncNewOrder(order, side, type')
         member x.AsyncCancelOrder(orderId:int) = (x :> IBitFinexApi).AsyncCancelOrder(orderId)
         member x.AsyncCancelAllOrder() = (x :> IBitFinexApi).AsyncCancelAllOrder()   
         member x.AsyncGetOrderStatus(orderId:int) = (x :> IBitFinexApi).AsyncGetOrderStatus(orderId)
         member x.AsyncGetMyTrades(pair:PairClass, ?timeStamp:double,?limitTrades:int) = (x :> IBitFinexApi).AsyncGetMyTrades(pair,timeStamp,limitTrades)
    
                                       

            

