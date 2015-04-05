module http2.tests.hpack.``Decoding With Huffman tree``

open FsUnit
open NUnit.Framework
open http2.hpack
open http2.hpack.compression

let simpleTree = 
    buildHuffmanTree [|
            (int 'a'), [|true|];
            (int 'b'), [|false|]
        |]

[<Test>]
let ``Decode single true bit``() =
    let data = [true]
    let result = decodeWithTree simpleTree data
    result |> should equal "a"

[<Test>]
let ``Decode single false bit``() =
    let data = [false]
    let result = decodeWithTree simpleTree data
    result |> should equal "b"

[<Test>]
let ``Decode multiple bits``() =
    let data = [false; true; false; false]
    let result = decodeWithTree simpleTree data
    result |> should equal "babb"

[<Test>]
let ``Decode string using http2 huffman table``() =
    let data = [
            true; false; false; true; true; true;
            false; false; true; false; true;
            true; false; true; false; false; false;
            true; false; true; false; false; false;
            false; false; true; true; true
        ]
    let result = decompress data
    result |> should equal "hello"