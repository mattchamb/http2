module http2.tests.hpack.``Variable Length Integers``

open FsUnit
open NUnit.Framework
open http2.hpack.data

[<Test>]
let ``asdf``() =
    let a, b = decodeInteger 5 [ 0b00001010uy ]
    a |> should equal 10UL
    b |> should equal []

[<Test>]
let ``asd2f``() =
    let a, b = decodeInteger 5 [ 0b00011111uy; 0b10011010uy; 0b00001010uy ]
    a |> should equal 1337UL
    b |> should equal []
    
[<Test>]
let ``asd22f``() =
    let a, b = decodeInteger 8 [ 0b00101010uy ]
    a |> should equal 42UL
    b |> should equal []
    
[<Test>]
let ``asd223f``() =
    let a, b = decodeInteger 8 [ 0b00101010uy; 0b10011010uy; 0b00001010uy ]
    a |> should equal 42UL
    b |> should equal [0b10011010uy; 0b00001010uy]

[<Test>]
let ``asd2233f``() =
    let a, b = decodeInteger 8 [ 0b11111111uy; 0b00011010uy; 0b00001010uy ]
    a |> should equal 281UL
    b |> should equal [0b00001010uy]

[<Test>]
let ``Try decoding something``() =
    let data = [
        0x8cuy; 0xf1uy; 0xe3uy; 0xc2uy; 
        0xe5uy; 0xf2uy; 0x3auy; 0x6buy; 
        0xa0uy; 0xabuy; 0x90uy; 0xf4uy;
        0xffuy
    ]
    let s, r = decodeString data
    s |> should equal "www.example.com"
    r |> should equal []


