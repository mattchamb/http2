namespace http2

open System
open System.Collections.Generic

module hpack =

    module compression =

        type EncodedChar =
            | Simple of byte
            | EOS
        
        type TreeNode =
            | Leaf of char
            | Node of TreeNode * TreeNode
            | ErrorLeaf

        let buildHuffmanTree (table: bool array array) =
            let rec buildNode depth =
                ErrorLeaf

            buildNode 0

        let huffmanTree = Node (Leaf 'a', Leaf 'a')

        let rec decode data treeNode result =
                match data, treeNode with
                | _, ErrorLeaf -> failwith ""
                | [], Node _ -> failwith ""
                | [], Leaf c -> c :: result
                | _, Leaf c -> decode data treeNode (c :: result)
                | bit :: rest, Node (left, right) -> 
                    match bit with
                    | true -> decode rest left result
                    | false -> decode rest right result

        let decompress data =
            let resultData = decode data huffmanTree []
            new String(resultData |> List.rev |> List.toArray)

    
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
        [|
            {Name = ":authority"; Value = ""};
            {Name = ":method"; Value = "GET"};
            {Name = ":method"; Value = "POST"};
            {Name = ":path"; Value = "/"};
            {Name = ":path"; Value = "/index.html"};
            {Name = ":scheme"; Value = "http"};
            {Name = ":scheme"; Value = "https"};
            {Name = ":status"; Value = "200"};
            {Name = ":status"; Value = "204"};
            {Name = ":status"; Value = "206"};
            {Name = ":status"; Value = "304"};                                
            {Name = ":status"; Value = "400"};
            {Name = ":status"; Value = "404"};                                 
            {Name = ":status"; Value = "500"};
            {Name = "accept-charset"; Value = ""};
            {Name = "accept-encoding"; Value = ""};
            {Name = "accept-language"; Value = ""};
            {Name = "accept-ranges"; Value = ""};               
            {Name = "accept"; Value = ""};
            {Name = "access-control-allow-origin"; Value = ""};
            {Name = "age"; Value = ""};
            {Name = "allow"; Value = ""};
            {Name = "authorization"; Value = ""};
            {Name = "cache-control"; Value = ""};
            {Name = "content-disposition"; Value = ""};
            {Name = "content-encoding"; Value = ""};
            {Name = "content-language"; Value = ""};
            {Name = "content-length"; Value = ""};
            {Name = "content-location"; Value = ""};
            {Name = "content-range"; Value = ""};
            {Name = "content-type"; Value = ""};
            {Name = "cookie"; Value = ""};
            {Name = "date"; Value = ""};
            {Name = "etag"; Value = ""};
            {Name = "expect"; Value = ""};
            {Name = "expires"; Value = ""};
            {Name = "from"; Value = ""};
            {Name = "host"; Value = ""};
            {Name = "if-match"; Value = ""};
            {Name = "if-modified-since"; Value = ""};
            {Name = "if-none-match"; Value = ""};
            {Name = "if-range"; Value = ""};
            {Name = "if-unmodified-since"; Value = ""};
            {Name = "last-modified"; Value = ""};
            {Name = "link"; Value = ""};
            {Name = "location"; Value = ""};
            {Name = "max-forwards"; Value = ""};
            {Name = "proxy-authenticate"; Value = ""};
            {Name = "proxy-authorization"; Value = ""};
            {Name = "range"; Value = ""};
            {Name = "referer"; Value = ""};
            {Name = "refresh"; Value = ""};
            {Name = "retry-after"; Value = ""};
            {Name = "server"; Value = ""};
            {Name = "set-cookie"; Value = ""};
            {Name = "strict-transport-security"; Value = ""};
            {Name = "transfer-encoding"; Value = ""};
            {Name = "user-agent"; Value = ""};
            {Name = "vary"; Value = ""};
            {Name = "via"; Value = ""};
            {Name = "www-authenticate"; Value = ""};
        |]

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
                if index < 0 then
                    failwith "Invalid index is a negative number."
                else if index < staticHeaderTable.Length then
                    staticHeaderTable.[index]
                else 
                    failwith "Could not find indexed value in static table. TODO: lookup in dynamic table."
        headers
        |> List.map decodeHeader
