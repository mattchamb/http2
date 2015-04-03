namespace http2

module ssl =
    open System.Net.Sockets
    open System.Net.Security
    open System.Net
    open System.Net.Http
    open System.Security.Authentication
    open System.IO
    open FSharp.Control.Reactive
    open Builders
    open System.Security.Cryptography.X509Certificates
    open System

    type SslServer = {
        listener: TcpListener;
        incomingConnections: IObservable<SslStream>
    }
    
    let acceptConnection (listener: TcpListener) (cert: X509Certificate) =
        let client = listener.AcceptTcpClient()
        let stream = client.GetStream()
        let sslStream = new SslStream(stream, false)
        sslStream.AuthenticateAsServer(cert, false, SslProtocols.Default, true)
        sslStream

    let createServer port cert = 
        let listener = new TcpListener(IPAddress.Any, port)
        let incomingConnections = observe {
            while true do
                yield acceptConnection listener cert
        }
        {listener = listener; 
        incomingConnections = incomingConnections}

    type Http2Request = {
        Method: HttpMethod
    }

    let processHttp2Connection (sslStream: SslStream) : IObservable<Http2Request> =
        observe {
            yield {Method = HttpMethod.Get}
        }

