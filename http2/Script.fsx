﻿#r @"..\packages\Rx-Interfaces.2.2.5\lib\net45\System.Reactive.Interfaces.dll"
#r @"..\packages\Rx-Core.2.2.5\lib\net45\System.Reactive.Core.dll"
#r @"..\packages\Rx-Linq.2.2.5\lib\net45\System.Reactive.Linq.dll"
#r @"..\packages\FSharp.Control.Reactive.3.1.1\lib\net40\FSharp.Control.Reactive.dll"


#load "huffmancodes.fs"
open http2
#load "hpack.fs"
open http2
open http2.hpack.data



let data = [| 0b00001010uy |]

let r = decodeInteger 5 data


//open System.Security.Cryptography.X509Certificates
//
//// Define your library scripting code here
//let cert = new X509Certificate("e:\dev.pfx", "Test123")
//
//let server = http2.ssl.createServer 8888 cert
//
//server.incomingConnections 
//|> Observable.add (fun s -> printfn "New Connection")
//
//let asdf = 
//    server.incomingConnections 
//    |> Observable.map (fun ssl -> 
//        http2.ssl.processHttp2Connection ssl)
//
//
//server.listener.Stop()