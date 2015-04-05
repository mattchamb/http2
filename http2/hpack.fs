namespace http2

open System
open System.Collections.Generic

module hpack =

    module compression =

        type HuffmanPattern = int * (bool array)

        type EncodedChar =
            | Simple of char
            | EOS
        
        type TreeNode =
            | Leaf of EncodedChar
            | Node of TreeNode * TreeNode
            | ErrorLeaf

        let isValidTable (table: HuffmanPattern array) =
            let uniq = 
                table
                |> Seq.distinctBy snd
                |> Seq.length
            let hasOutOfRange =
                table
                |> Seq.map fst
                |> Seq.exists (fun n -> n < 0 || n > 256)
            uniq = table.Length && not hasOutOfRange

        let buildHuffmanTree table =
            
            if not <| isValidTable table then
                failwith "The provided huffman encoding table is invalid."

            let longEnough depth (values: 'a array) = 
                depth < values.Length

            let rec buildBranch depth (codes: HuffmanPattern array) = 
                match codes.Length with
                | 0 -> ErrorLeaf
                | 1 ->  
                    let ch, data = codes.[0]
                    if data.Length - 1 = depth then
                        match ch with
                        | 256 -> Leaf EOS
                        | _ -> Simple (char ch) |> Leaf
                    else 
                        let child = buildBranch (depth + 1) codes
                        if data.[depth] then
                            Node (child, ErrorLeaf)
                        else 
                            Node (ErrorLeaf, child)

                | _ -> 
                    let err = 
                        codes
                        |> Seq.exists 
                            (fun (_, arr) -> arr.Length - 1 = depth)
                    if err then
                        failwith "The encoding table contains an ambiguous encoding."

                    buildNode (depth + 1) codes
            and buildNode depth candidates =
                let left, right =
                    candidates
                    |> Array.filter (fun (_, d) -> longEnough depth d)
                    |> Array.partition (fun (_, d) -> d.[depth])
                let leftBranch = buildBranch depth left
                let rightBranch = buildBranch depth right
                Node (leftBranch, rightBranch)

            buildNode 0 table

        let httpHuffmanTable = buildHuffmanTree huffmancodes.table


        let decodeWithTree tree data =
            let stepToNextNode bit currentNode =
                match bit, currentNode with
                | _, ErrorLeaf -> failwith "Encoutered a bit string that does not match a valid pattern in the given encoding."
                | _, Leaf _ -> failwith "Cannot keep decoding a bit string from a Leaf node"
                | true, Node (left, _) -> left
                | false, Node (_, right) -> right

            let doDecode (res: EncodedChar list, node: TreeNode) (bit: bool) =
                let nextNode = stepToNextNode bit node
                match nextNode with
                | ErrorLeaf -> failwith ""
                | Node _ -> res, nextNode
                | Leaf ch -> (ch :: res), tree //return tree because we have reached a leaf and can start again
                
            let decodedData, _ = 
                data
                |> List.fold doDecode ([], tree)
            let resultData =
                decodedData
                |> List.rev
                |> Seq.takeWhile 
                    (fun encodedChar -> 
                        match encodedChar with
                        | Simple _ -> true
                        | EOS -> false)
                |> Seq.map
                    (fun encCh ->
                        match encCh with
                        | Simple ch -> ch
                        | EOS -> failwith "Encountered EOS after it should have been filtered out...")
                |> Seq.toArray
            new String(resultData)

        let decompress data =
            decodeWithTree httpHuffmanTable data
        
    type HeaderTableEntry = {
        Name: string;
        Value: string
    }

    /// http://http2.github.io/http2-spec/compression.html#static.table.definition
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

    let lookupStaticTable idx =
        // In the spec, indexes start at 1
        staticHeaderTable.[idx - 1]

    type DynamicHeaderTable = {
        entries: HeaderTableEntry list
    }

    let lookup table index =
        let len = table.entries.Length
        match len with
        | 0 -> None
        | _ when index >= len -> None
        | _ -> Some (List.nth table.entries index)

    type IndexingAction =
        | Incremental
        | NonIndexing

    type LiteralHeaderKind =
        | IndexedName of IndexingAction * int * string
        | NewName of IndexingAction * string * string

    type HeaderRepresentation = 
        | LiteralHeader of LiteralHeaderKind
        | IndexedHeader of int

    type EncodedHeaderBlock = HeaderRepresentation list

    let (|Static|Dynamic|Error|) index = 
        if index <= 0 then
            Error
        else if index <= 61 then
            Static
        else
            Dynamic

    let decodeLiteralHeader header dynamicTable =
        match header with
        | IndexedName(idxAction, tblIdx, v) -> 
            match tblIdx with
            | Static -> 
                idxAction, { lookupStaticTable tblIdx with Value = v }
            | Dynamic -> 
                let r = lookup dynamicTable tblIdx
                match r with
                | None -> failwith ""
                | Some hdr -> idxAction, hdr
            | Error -> 
                failwith ""
        | NewName(idxAction, n, v) -> 
            idxAction, {Name = n; Value = v}

    let decodeHeaderWithTable header dynamicTable = 
            match header with
            | LiteralHeader data -> 
                let idxAction, decodedHdr = decodeLiteralHeader data dynamicTable
                Some idxAction, decodedHdr
            | IndexedHeader tblIdx -> 
                if tblIdx < 0 then
                    failwith "Invalid index: negative number."
                else if tblIdx <= staticHeaderTable.Length then
                    None, lookupStaticTable tblIdx
                else 
                    let header = lookup dynamicTable tblIdx
                    match header with
                    | None -> failwith ""
                    | Some hdr -> None, hdr
                    

    let updateDynamicTable header table =
        table
    
    let processHeader header dynamicTable =
        let updateAction, headerResult = decodeHeaderWithTable header dynamicTable
        match updateAction with
        | Some action -> 
            let updatedTable = updateDynamicTable headerResult dynamicTable
            headerResult, updatedTable
        | None -> 
            headerResult, dynamicTable
