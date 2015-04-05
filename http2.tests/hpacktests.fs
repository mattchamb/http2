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

    let expected = Node (Leaf (Simple '\000'), ErrorLeaf)

    tree |> should equal expected

[<Test>]
let ``Byte val of 256 is treated as EOS``() = 
    let table = [|
        256, [|true|]
    |]
    let tree = buildHuffmanTree table

    let expected = Node (Leaf EOS, ErrorLeaf)

    tree |> should equal expected

[<Test>]
let ``Byte value out of range throws exception``() = 
    let table = [|
        500, [|false; false|];
    |]

    (fun () -> buildHuffmanTree table |> ignore) |> should throw typeof<System.Exception>

[<Test>]
let ``Single false code goes to right branch``() = 
    let table = [|
        0, [|false|]
    |]
    let tree = buildHuffmanTree table

    let expected = Node (ErrorLeaf, Leaf (Simple '\000'))

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
                 Leaf (Simple '0'))
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

    
[<Test>]
let ``Negative values throw exception``() = 
    let table = [|
        -1, [|false|];
    |]
        (fun () -> buildHuffmanTree table |> ignore) |> should throw typeof<System.Exception>

[<Test>]
let ``Values over 256 throw exception``() = 
    let table = [|
        257, [|false|];
    |]
        (fun () -> buildHuffmanTree table |> ignore) |> should throw typeof<System.Exception>