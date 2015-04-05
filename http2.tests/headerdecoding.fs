module http2.tests.hpack.``Decoding Header Key Value pairs``

open FsUnit
open NUnit.Framework
open http2.hpack

let emptyDynamicTable = { entries = [] }

[<Test>]
let ``Accessing first static indexed header``() =
    let header = IndexedHeader 1
    let r, _ = processHeader header emptyDynamicTable

    r |> should equal {Name = ":authority"; Value = ""}

[<Test>]
let ``Accessing last static indexed header``() =
    let header = IndexedHeader 61
    let r, _ = processHeader header emptyDynamicTable

    r |> should equal {Name = "www-authenticate"; Value = ""}

[<Test>]
let ``Accessing static indexed header with indexed name returns new value``() =
    let header = 
        LiteralHeader (
            IndexedName (
                NonIndexing, 1, "NewValue"))
    let r, _ = processHeader header emptyDynamicTable

    r |> should equal {Name = ":authority"; Value = "NewValue"}

[<Test>]
let ``Processing non indexed header returns new name and value``() =
    let header = 
        LiteralHeader (
            NewName (
                NonIndexing, "NewName", "NewValue"))
    let r, _ = processHeader header emptyDynamicTable

    r |> should equal {Name = "NewName"; Value = "NewValue"}

[<Test>]
let ``Processing header with NonIndexing set does not change dynamic table``() =
    let header = 
        LiteralHeader (
            NewName (
                NonIndexing, "NewName", "NewValue"))
    let _, newTable = processHeader header emptyDynamicTable

    newTable |> should equal emptyDynamicTable

//[<Test>]
//let ``Processing header with Incremental indexing updates dynamic table``() =
//    let header = 
//        LiteralHeader (
//            NewName (
//                Incremental, "NewName", "NewValue"))
//    let _, newTable = processHeader header emptyDynamicTable
//
//    newTable |> should not' (equal emptyDynamicTable)
