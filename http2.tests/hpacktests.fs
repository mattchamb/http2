module http2.tests.hpack.``Huffman Tree Building``

open FsUnit
open NUnit.Framework
open http2.hpack
open http2.hpack.compression

[<Test>]
let ``Empty table returns ErrorLeaf as both branches``() = 
    let table = [||]

    let tree = buildHuffmanTree table

    let expected = Node(ErrorLeaf, ErrorLeaf)

    tree |> should equal expected

[<Test>]
let ``Single true code goes to left branch``() = 
    let table = [|
        0, [|true|]
    |]
    let tree = buildHuffmanTree table

    let expected = Node (Leaf (Simple 0uy), ErrorLeaf)

    tree |> should equal expected

[<Test>]
let ``Single false code goes to right branch``() = 
    let table = [|
        0, [|false|]
    |]
    let tree = buildHuffmanTree table

    let expected = Node (ErrorLeaf, Leaf (Simple 0uy))

    tree |> should equal expected

[<Test>]
let ``Two false codes create right branches``() = 
    let table = [|
        48, [|false; false|];
    |]
    let tree = buildHuffmanTree table

    let expected = 
        Node 
            (ErrorLeaf, 
            Node 
                (ErrorLeaf,
                 Leaf (Simple 48uy))
            )

    tree |> should equal expected

[<Test>]
let ``Ambiguous pattern throws exception``() = 
    let table = [|
        0, [|false|];
        1, [|false; true|];
    |]
        (fun () -> buildHuffmanTree table |> ignore) |> should throw typeof<System.Exception>

[<Test>]
let ``Duplicate patterns throws exception``() = 
    let table = [|
        47, [|false; false|];
        48, [|false; false|];
    |]

    (fun () -> buildHuffmanTree table |> ignore) |> should throw typeof<System.Exception>