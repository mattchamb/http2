module http2.tests.hpack.``Variable Length Integers``

open FsUnit
open NUnit.Framework
open http2.hpack.data

[<Test>]
let ``asdf``() =
    let a, _ = decodeInteger 5 [ 0b00001010uy ]
    a |> should equal 10UL

[<Test>]
let ``asd2f``() =
    let a, _ = decodeInteger 5 [ 0b00011111uy; 0b10011010uy; 0b00001010uy ]
    a |> should equal 1337UL
    
[<Test>]
let ``asd22f``() =
    let a, _ = decodeInteger 8 [ 0b00101010uy ]
    a |> should equal 42UL
    
[<Test>]
let ``asd223f``() =
    let a, b = decodeInteger 8 [ 0b00101010uy; 0b10011010uy; 0b00001010uy ]
    a |> should equal 42UL
    b |> should equal [0b10011010uy; 0b00001010uy]