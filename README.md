"# F# library for BitFinex Exchange Api https://docs.bitfinex.com/docs/public-endpoints" 
   <br>.net core 2.1
   <br>FSharp.Core 4.5.2
   <br>FSharp.Data 3.0.0-rc
   <br>F# 4.5
   <br>MSTests

   IBitFinexApi
   BitFinexApi(key:string, secret:string, ?timeout:int)
   Sample Using:
	let api = BitFinexApi(apiKey,apiSecret, 5000) //5000 ms -> 5 seconds timeout for web request
        let pairs = api.GetSupportedPairs()
        let pairs = (api :> IBitFinexApi).GetSupportedPairs()
