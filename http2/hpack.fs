namespace http2

open System
open System.Collections.Generic

module hpack =
    
    type IndexingAction =
        | Incremental
        | NonIndexed

    type LiteralHeaderKind =
        | IndexedName of IndexingAction * int * string
        | NewName of IndexingAction * string * string

    type HeaderRepresentation = 
        | LiteralHeader of LiteralHeaderKind
        | IndexedHeader of int

    type EncodedHeaderBlock = HeaderRepresentation list

    type HeaderTableEntry = {
        Name: string;
        Value: string
    }

    let (|Static|Dynamic|Error|) index = 
        if index <= 0 then
            Error
        else if index <= 61 then
            Static
        else
            Dynamic
        

    let staticHeaderTable =
        [
            1, {Name = ":authority"; Value = ""};
            2, {Name = ":method"; Value = "GET"};
            3, {Name = ":method"; Value = "POST"};
            4, {Name = ":path"; Value = "/"};
            5, {Name = ":path"; Value = "/index.html"};
            6, {Name = ":scheme"; Value = "http"};
            7, {Name = ":scheme"; Value = "https"};
            8, {Name = ":status"; Value = "200"};
            9, {Name = ":status"; Value = "204"};
            10, {Name = ":status"; Value = "206"};
            11, {Name = ":status"; Value = "304"};                                
            12, {Name = ":status"; Value = "400"};
            13, {Name = ":status"; Value = "404"};                                 
            14, {Name = ":status"; Value = "500"};
            15, {Name = "accept-charset"; Value = ""};
            16, {Name = "accept-encoding"; Value = ""};
            17, {Name = "accept-language"; Value = ""};
            18, {Name = "accept-ranges"; Value = ""};               
            19, {Name = "accept"; Value = ""};
            20, {Name = "access-control-allow-origin"; Value = ""};
            21, {Name = "age"; Value = ""};
            22, {Name = "allow"; Value = ""};
            23, {Name = "authorization"; Value = ""};
            24, {Name = "cache-control"; Value = ""};
            25, {Name = "content-disposition"; Value = ""};
            26, {Name = "content-encoding"; Value = ""};
            27, {Name = "content-language"; Value = ""};
            28, {Name = "content-length"; Value = ""};
            29, {Name = "content-location"; Value = ""};
            30, {Name = "content-range"; Value = ""};
            31, {Name = "content-type"; Value = ""};
            32, {Name = "cookie"; Value = ""};
            33, {Name = "date"; Value = ""};
            34, {Name = "etag"; Value = ""};
            35, {Name = "expect"; Value = ""};
            36, {Name = "expires"; Value = ""};
            37, {Name = "from"; Value = ""};
            38, {Name = "host"; Value = ""};
            39, {Name = "if-match"; Value = ""};
            40, {Name = "if-modified-since"; Value = ""};
            41, {Name = "if-none-match"; Value = ""};
            42, {Name = "if-range"; Value = ""};
            43, {Name = "if-unmodified-since"; Value = ""};
            44, {Name = "last-modified"; Value = ""};
            45, {Name = "link"; Value = ""};
            46, {Name = "location"; Value = ""};
            47, {Name = "max-forwards"; Value = ""};
            48, {Name = "proxy-authenticate"; Value = ""};
            49, {Name = "proxy-authorization"; Value = ""};
            50, {Name = "range"; Value = ""};
            51, {Name = "referer"; Value = ""};
            52, {Name = "refresh"; Value = ""};
            53, {Name = "retry-after"; Value = ""};
            54, {Name = "server"; Value = ""};
            55, {Name = "set-cookie"; Value = ""};
            56, {Name = "strict-transport-security"; Value = ""};
            57, {Name = "transfer-encoding"; Value = ""};
            58, {Name = "user-agent"; Value = ""};
            59, {Name = "vary"; Value = ""};
            60, {Name = "via"; Value = ""};
            61, {Name = "www-authenticate"; Value = ""};
        ]
        |> Map.ofList

    type DynamicHeaderTable = {
        entries: (string * string) list
    }
        
    let decodeHeaderList (headers: HeaderRepresentation list) =
        let decodeHeader header = 
            match header with
            | LiteralHeader data -> 
                match data with
                | IndexedName(_, tblIdx, v) -> 
                    match tblIdx with
                    | Static -> {Name = ""; Value = v}
                    | Dynamic -> {Name = ""; Value = v}
                    | Error -> failwith ""
                | NewName(_, n, v) -> {Name = n; Value = v}

            | IndexedHeader index -> 
                let header = staticHeaderTable.TryFind index
                match header with
                | None -> failwith "Could not find indexed value in static table. TODO: lookup in dynamic table."
                | Some h -> h
        headers
        |> List.map decodeHeader
