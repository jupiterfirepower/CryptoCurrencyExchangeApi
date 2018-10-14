namespace CryptCurrency.Common

open System
open System.Runtime.CompilerServices

[<Extension>]
module StringEx =
        type String with
        [<CompiledName("IsNullOrEmpty")>]
        [<Extension>]
        member public x.IsNullOrEmpty() = String.IsNullOrEmpty(x)
        [<CompiledName("IsNullOrWhiteSpace")>]
        [<Extension>]
        member public x.IsNullOrWhiteSpace() = String.IsNullOrWhiteSpace(x)
        [<CompiledName("toArrayByChunkSize")>]
        [<Extension>]
        member public x.toArrayByChunkSize(size : int) = x |> Seq.toArray |> Array.chunkBySize size |> Array.map (fun x -> new string(x))
        //[<CompiledName("toSeqByChunkSize")>]
        [<Extension>]
        member public x.toSeqByChunkSize(size : int) = x |> Seq.chunkBySize size |> Seq.map (fun x -> new string(x))

module Retry =
    open System.Threading

    type RetryParams = {
        maxRetries : int; waitBetweenRetries : int
        }

    let defaultRetryParams = { maxRetries = 3; waitBetweenRetries = 0 }

    type RetryMonad<'a> = RetryParams -> 'a
    let rm<'a> (f : RetryParams -> 'a) : RetryMonad<'a> = f

    let internal retryFunc<'a> (f : RetryMonad<'a>) =
        rm (fun retryParams -> 
            let rec execWithRetry f i e =
                match i with
                | n when n = retryParams.maxRetries -> raise e
                | _ -> 
                    try
                        f retryParams
                    with 
                    | e -> Thread.Sleep(retryParams.waitBetweenRetries); execWithRetry f (i + 1) e
            execWithRetry f 0 (Exception())
            ) 

    type RetryBuilder() =
        
        member this.Bind (p : RetryMonad<'a>, f : 'a -> RetryMonad<'b>)  =
            rm (fun retryParams -> 
                let value = retryFunc p retryParams
                f value retryParams                
            )

        member this.Return (x : 'a) = fun defaultRetryParams -> x

        member this.Run(m : RetryMonad<'a>) = m

        member this.Delay(f : unit -> RetryMonad<'a>) = f ()

    let retry = RetryBuilder()

module Helpers=
    open System.Reflection

    let clone (e : #exn) =
        let bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        use m = new System.IO.MemoryStream()
        bf.Serialize(m, e)
        m.Position <- 0L
        bf.Deserialize m :?> exn

    let remoteStackTraceField =
        let getField name = typeof<System.Exception>.GetField(name, BindingFlags.Instance ||| BindingFlags.NonPublic)
        match getField "remote_stack_trace" with
        | null ->
            match getField "_remoteStackTraceString" with
            | null -> failwith "a piece of unreliable code has just failed."
            | f -> f
        | f -> f

    let inline reraise' (e : #exn) =
        // clone the exception to avoid mutation side-effects
        let e' = clone e
        remoteStackTraceField.SetValue(e', e'.StackTrace + System.Environment.NewLine)
        raise e'

module DataContracts =

    open SupportedCurrency
    open System.Collections.Generic
    open System.Linq
    open System.Collections.Concurrent
    open StringEx
 
    [<Serializable>]
    [<StructuralEquality;StructuralComparison>]
    type Pair =
            { BaseCurrency : string; 
              CounterCurrency : string } 
        with
        override x.ToString() = sprintf "%s/%s" x.BaseCurrency x.CounterCurrency
        member x.Clone() = { x with Pair.BaseCurrency = x.BaseCurrency }
        static member GetPair(baseCurrency : string, counterCurrency : string) : Pair = { Pair.BaseCurrency = baseCurrency.ToUpper(); Pair.CounterCurrency = counterCurrency.ToUpper() }
        static member GetPair(symbol : string) : Pair = let delimiter = [|'/';':';'_';'-'|]
                                                        match symbol with
                                                        | null -> Pair.GetPair(Unknown, Unknown) 
                                                        | x when x.Split delimiter |> Array.length = 2 
                                                            -> let data = x.Split delimiter 
                                                               Pair.GetPair(data.[0], data.[1])
                                                        | x when x.Length = 6 -> Pair.GetPair(x.Substring(0, 3), x.Substring(3, 3)) 
                                                        | _ -> Pair.GetPair(Unknown, Unknown)
    
        static member Undefined : Pair = Pair.GetPair(Unknown, Unknown)


    [<Serializable>]
    [<CustomEquality;CustomComparison>]
    type PairRecord =
        private
            { BaseCurrency : string; 
              CounterCurrency : string } 
        with
        override x.ToString() = sprintf "%s/%s" x.BaseCurrency x.CounterCurrency
        override x.GetHashCode() = hash (x.BaseCurrency, x.CounterCurrency)
        override x.Equals(b) =
                    match b with
                    | :? PairRecord as p -> (x.BaseCurrency, x.CounterCurrency) = (p.BaseCurrency, p.CounterCurrency)
                    | _ -> false
        member x.BCurrency = x.BaseCurrency.ToUpper()
        member x.CCurrency = x.CounterCurrency.ToUpper()
        
        member x.Clone() = PairRecord.GetPair(x.BaseCurrency, x.CounterCurrency)
        static member GetPair(baseCurrency : string, counterCurrency : string) : PairRecord = { BaseCurrency = baseCurrency.ToUpper(); CounterCurrency = counterCurrency.ToUpper() }
        static member GetPair(symbol : string) : PairRecord = let delimiter = [|'/';':';'_';'-'|]
                                                              match symbol with
                                                                | null -> PairRecord.GetPair(Unknown, Unknown) 
                                                                | x when x.Split delimiter |> Array.length = 2 
                                                                    -> let data = x.Split delimiter 
                                                                       PairRecord.GetPair(data.[0], data.[1])
                                                                | x when x.Length = 6 -> PairRecord.GetPair(x.Substring(0, 3), x.Substring(3, 3)) 
                                                                | _ -> PairRecord.GetPair(Unknown, Unknown)
    
        static member Undefined : PairRecord = PairRecord.GetPair(Unknown, Unknown)
        interface System.IEquatable<PairRecord> with 
            member x.Equals(p) = x.BaseCurrency.Equals(p.BaseCurrency) && x.CounterCurrency.Equals(p.CounterCurrency);
        interface System.IComparable<PairRecord> with 
            member x.CompareTo(p) = match p with
                                    | _ when (x.BaseCurrency, x.CounterCurrency) = (p.BaseCurrency, p.CounterCurrency) -> 0
                                    | _ -> -1

    [<Serializable>]
    type PairClass(baseCurrency : string, counterCurrency : string) = 
         let mutable bCurrency = baseCurrency.ToUpper()
         let mutable cCurrency = counterCurrency.ToUpper()

         //[<DefaultValue>]val mutable private xBaseCurrency: string
         //[<DefaultValue>]val mutable private xCounterCurrency: string

         override x.ToString() = sprintf "%s/%s" bCurrency cCurrency
         override x.GetHashCode() = hash (bCurrency, cCurrency)
         override x.Equals(b) =
                    match b with
                    | :? PairClass as p -> (bCurrency, cCurrency) = (p.BaseCurrency, p.CounterCurrency)
                    | _ -> false
         member x.BaseCurrency = bCurrency
         member x.CounterCurrency = cCurrency
         member x.Clone() = new PairClass(bCurrency, cCurrency)
         static member GetPair(baseCurrency : string, counterCurrency : string) = new PairClass(baseCurrency, counterCurrency)
         static member GetPair(symbol : string) = let delimiter = [|'/';':';'_';'-'|]
                                                  match symbol with
                                                  | null -> PairClass.GetPair(Unknown, Unknown) 
                                                  | x when (x.Split delimiter).Length = 2 
                                                             -> let data = x.Split delimiter 
                                                                PairClass.GetPair(data.[0], data.[1])
                                                  | x when x.Length = 6 -> PairClass.GetPair(x.Substring(0, 3), x.Substring(3, 3)) 
                                                  | _ -> PairClass.GetPair(Unknown, Unknown)              
    
         static member Undefined : PairClass = PairClass.GetPair(Unknown, Unknown)
         interface System.IEquatable<PairClass> with 
            member x.Equals(p) = x.BaseCurrency.Equals(p.BaseCurrency) && x.CounterCurrency.Equals(p.CounterCurrency);
         interface System.IComparable<PairClass> with 
            member x.CompareTo(p) = match p with
                                    | _ when (x.BaseCurrency, x.CounterCurrency) = (p.BaseCurrency, p.CounterCurrency) -> 0
                                    | _ -> -1
         new (baseCurrency : string) = PairClass(baseCurrency, Unknown)

    type MarketSide =
    | Ask = 0
    | Bid = 1

    type OrderType =
    | Market = 0
    | Limit = 1
    | Pegged = 4

    type TimeInForce = 
    | GoodTillCancel = 0
    | ImmediateOrCancel = 1
    | FillOrKill = 2

    type OrderStatus =
    | PendingNew = 0
    | New = 1 
    | PartiallyFilled = 2
    | Filled = 3
    | DoneForDay = 4
    | Canceled = 5
    | PendingCancel = 6
    | Stopped = 7
    | Rejected = 8
    | Suspended = 9
    | Calculated = 10
    | Expired = 11
    | AcceptedForBidding = 12
    | PendingReplace = 13
    | Replaced = 14
    | Unknown = 100// TO BE REMOVED

    [<Literal>]
    let Undefined = "Undefined"
    [<Literal>]
    let InCryptEx = "InCryptEx";
    [<Literal>]
    let BitStamp = "BitStamp";
    [<Literal>]
    let Btce = "Btce";
    [<Literal>]
    let Cryptsy = "Cryptsy";
    [<Literal>]
    let IB = "IB";
    [<Literal>]
    let AnxBtc = "AnxBtc";
    [<Literal>]
    let Bittrex = "Bittrex";
    [<Literal>]
    let Kraken = "Kraken";
    [<Literal>]
    let Bter = "Bter";
    [<Literal>]
    let BitFinex = "BitFinex";
    [<Literal>]
    let Poloniex = "Poloniex";
    [<Literal>]
    let Cex = "Cex";
    [<Literal>]
    let Coinfloor = "Coinfloor";
    [<Literal>]
    let RockTrading = "RockTrading";
    [<Literal>]
    let ItBit = "ItBit";
    [<Literal>]
    let BtcChina = "BtcChina";
    [<Literal>]
    let ExternalExchange = "ExternalExchange";
    
    type IQuoteBase =
        abstract MarketSide : MarketSide with get
        abstract Price : decimal with get
        abstract Amount : decimal
        abstract Time : DateTime
        abstract Exchange : string

    type System.Decimal with 
        member x.AlmostEquals(p : decimal, precision : int) = 
                        let epsilon = Math.Pow(10.0, float -precision)
                        Math.Abs(x - p) <= decimal epsilon
    
    [<Serializable>]
    type Order(pair : PairClass, price : decimal, amount : decimal, exchange : string, marketSide : MarketSide, 
               time : DateTime, orderType : OrderType, sourceSystemCode : string, 
               orderId : string, mmsId : string, parentId : string, timeInForce : TimeInForce, ?orderStatus : OrderStatus) =
        [<DefaultValue>]
        val mutable private _price : decimal
        [<DefaultValue>]
        val mutable private _amount : decimal
        [<DefaultValue>]
        val mutable private _pair : PairClass
        [<DefaultValue>]
        val mutable private _orderId : string
        [<DefaultValue>]
        val mutable private _orderType : OrderType
        [<DefaultValue>]
        val mutable private _orderStatus : OrderStatus
        // special for inner use
        [<DefaultValue>]
        val mutable private _initialized : bool

        do 
            if price < 0m then invalidArg "price" "Price can't be less then zero."
            if amount < 0m then invalidArg "amount" "Amount can't be less then zero."
            if exchange.IsNullOrEmpty() then nullArg "Name of the Exchange can't be null or empty."
            if sourceSystemCode.IsNullOrEmpty() then nullArg "SourceSystemCode can't be null or empty."

        member x.Pair = if x._initialized <> Unchecked.defaultof<_> then x._pair else pair
        member x.Id = if x._initialized <> Unchecked.defaultof<_> then x._orderId else orderId 
        member x.OrderType = if x._initialized <> Unchecked.defaultof<_> then x._orderType else orderType

        member x.OrderStatus = if x._initialized <> Unchecked.defaultof<_> 
                               then x._orderStatus 
                               else match orderStatus with
                                    | Some status -> status
                                    | None -> defaultArg orderStatus (OrderStatus.Unknown)

        member x.SourceSystemCode = sourceSystemCode
        member x.MmsId = mmsId
        member x.ParentId = parentId
        member x.TimeInForce = timeInForce
        member x.TotalPrice with get() = amount * price
        member x.MarketSide = (x :> IQuoteBase).MarketSide
        member x.Price = (x :> IQuoteBase).Price
        member x.Amount = (x :> IQuoteBase).Amount
        member x.Time = (x :> IQuoteBase).Time
        member x.Exchange with get() = (x :> IQuoteBase).Exchange
        member val Underlying = new List<Order>()

        member x.Liquidity = if x.MarketSide = MarketSide.Ask then x.Amount else x.TotalPrice

        member x.UpdateOrderId(orderId : string) : Order = Order(x, orderId)
        member x.UpdateAmount(newAmount : decimal) : Order = Order(x, newAmount)
        member x.UpdatePrice(newPrice : decimal) : Order = Order(x, newPrice, true)
        member x.UpdatePair(pair : PairClass) : Order = Order(x, pair)
        member x.UpdateOrderStatus(orderStatus : OrderStatus) : Order = Order(x, orderStatus)
        member x.ToMarketOrder() : Order = Order(x, OrderType.Market)
        member x.TransformPrice(coefficient : decimal, fix : decimal) : Order = Order(x, x.Price + x.Price * coefficient + fix, true)
        
        static member CreateNewMMSOrderId(exchangeName : string) = sprintf "%A%A%A" exchangeName (DateTime.UtcNow.ToString("s")) (Guid.NewGuid())

        override x.ToString() = sprintf "%A%A" decimal amount

        interface IQuoteBase with
                 member x.MarketSide = marketSide
                 member x.Price = if x._initialized <> Unchecked.defaultof<_> then x._price else price
                 member x.Amount = if x._initialized <> Unchecked.defaultof<_> then x._amount else amount
                 member x.Time = time
                 member x.Exchange with get() = exchange

        member x.Equals(o:Order) = match o with
                                    | _ when (not(x.MmsId.IsNullOrEmpty()) && (x.MmsId = o.MmsId)) || 
                                             (not(x.Id.IsNullOrEmpty()) && (x.Id = o.Id)) || 
                                             (x.MmsId.IsNullOrEmpty() && (x.Price.AlmostEquals(o.Price, 8) &&
                                               x.Amount.AlmostEquals(o.Amount, 8) && x.MarketSide = o.MarketSide )) -> true
                                    | _ -> false

        override x.Equals(o : obj) = match o with
                                     | null -> false
                                     | :? Order as other -> x.Equals(other)
                                     | _ -> invalidArg "o : obj" "not a Order"

        override x.GetHashCode() = hash x

        interface IComparer<IQuoteBase> with
                member x.Compare(n : IQuoteBase, m: IQuoteBase) = n.Price.CompareTo(m.Price)

        interface IComparable with
                member x.CompareTo(p : obj) = match p with
                                              | null -> 1
                                              | :? IQuoteBase as other -> (x :> IComparer<IQuoteBase>).Compare(x, other)
                                              | _ -> invalidArg "obj" "not a Order"

        interface IEquatable<Order> with
                member x.Equals(o) = x.Equals(o)
                
        interface IComparable<Order> with
                member x.CompareTo(p) = match p with
                                         | _ when x.Equals(p) -> 0
                                         | _ -> -1

        new(pair : PairClass, price : decimal, amount : decimal, exchange : string, marketSide : MarketSide, 
            time : DateTime, orderType : OrderType, sourceSystemCode : string, timeInForce : TimeInForce) = Order(pair, price, amount, exchange, marketSide, time, orderType, sourceSystemCode, timeInForce)
        new(order : Order) = Order(order.Pair, order.Price, order.Amount, order.Exchange, order.MarketSide, order.Time, order.OrderType, order.SourceSystemCode, order.Id, order.MmsId, order.ParentId, order.TimeInForce)

        new(order : Order, orderId : string) as self = new Order(order) then
                                                       self._initialized <- true
                                                       self._orderId <- orderId
                                                       order.Underlying |> Seq.iter (fun x -> self.Underlying.Add(x))

        new(order : Order, newAmount : decimal) as self = new Order(order) then
                                                          self._initialized <- true  
                                                          self._amount <- newAmount
                                                          order.Underlying |> Seq.iter (fun x -> self.Underlying.Add(x))

        new(order : Order, newPrice : decimal, ignored : bool) as self = new Order(order) then
                                                                         self._initialized <- true  
                                                                         self._price <- newPrice
                                                                         self._amount <- order.Amount
                                                                         order.Underlying |> Seq.iter (fun x -> self.Underlying.Add(x))

        new(order : Order, pair : PairClass) as self = new Order(order) then
                                                       self._initialized <- true
                                                       self._pair <- pair
                                                       order.Underlying |> Seq.iter (fun x -> self.Underlying.Add(x))

        new(order : Order, orderType : OrderType) as self = new Order(order) then
                                                     self._initialized <- true
                                                     self._orderType <- orderType
                                                     order.Underlying |> Seq.iter (fun x -> self.Underlying.Add(x))

        new(order : Order, orderStatus : OrderStatus) as self = new Order(order) then
                                                         self._initialized <- true
                                                         self._orderStatus <- orderStatus
                                                         order.Underlying |> Seq.iter (fun x -> self.Underlying.Add(x))

        type OrderBook(bids : seq<Order>, asks : seq<Order>, exchange : string, pair : PairClass, time : DateTime) =
             member x.Asks = seq<Order> { for it in asks.OrderByDescending(fun x -> x.Price) do yield it }
             member x.Bids = seq<Order> { for it in bids.OrderBy(fun x -> x.Price) -> it }
             member x.Pair = pair
             member x.Exchange = exchange
             member x.Time = time
             member x.IsStored = false
 