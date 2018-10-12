"# F# library for BitFinex Exchange Api https://docs.bitfinex.com/docs/public-endpoints" 
   <br>.net core 2.1
   <br>FSharp.Core 4.5.2
   <br>FSharp.Data 3.0.0-rc
   <br>F# 4.5
   <br>MSTests

   <br>IBitFinexApi
   <br>BitFinexApi(key:string, secret:string, ?timeout:int)
   <br>Sample Using:
<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;let api = BitFinexApi(apiKey,apiSecret, 5000) //5000 ms -> 5 seconds timeout for web request
<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;let pairs = api.GetSupportedPairs()
<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;let pairs = (api :> IBitFinexApi).GetSupportedPairs()
<br>See unit tests for details
