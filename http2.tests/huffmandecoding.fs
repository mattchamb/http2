module http2.tests.hpack.``Decoding With Huffman tree``

open System
open FsUnit
open NUnit.Framework
open http2.hpack
open http2.hpack.compression
open http2.hpack.data

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

[<Test>]
let ``Try decoding something else``() =
    let tests = [
        ("f1 e3 c2 e5 f2 3a 6b a0 ab 90 f4 ff", "www.example.com");
        ("a8 eb 10 64 9c bf", "no-cache");
        ("25 a8 49 e9 5b a9 7d 7f", "custom-key");
        ("25 a8 49 e9 5b b8 e8 b4 bf", "custom-value");
        ("64 02", "302");
        ("ae c3 77 1a 4b", "private");
        ("d0 7a be 94 10 54 d4 44 a8 20 05 95 04 0b 81 66 e0 82 a6 2d 1b ff", "Mon, 21 Oct 2013 20:13:21 GMT");
        ("9d 29 ad 17 18 63 c7 8f 0b 97 c8 e9 ae 82 ae 43 d3", "https://www.example.com");
        ("9b d9 ab", "gzip");
        ("94 e7 82 1d d7 f2 e6 c7 b3 35 df df cd 5b 39 60 d5 af 27 08 7f 36 72 c1 ab 27 0f b5 29 1f 95 87 31 60 65 c0 03 ed 4e e5 b1 06 3d 50 07", "foo=ASDJKHQKBZXOQWEOPIUAXQWEOIU; max-age=3600; version=1")
    ]
    tests
    |> List.map (fun (d, ex) -> 
        let r = 
            d.Split(' ')
            |> Array.map (fun s -> Convert.ToByte(s, 16))
        r, ex)
    |> List.iter (fun (data, expected) ->
        let result = decompress (toBitArray <| List.ofArray data)
        result |> should equal expected
    )
