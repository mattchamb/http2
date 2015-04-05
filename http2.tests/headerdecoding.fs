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

    r |> should equal {Name = "www-authenticate"; Value = ""};
