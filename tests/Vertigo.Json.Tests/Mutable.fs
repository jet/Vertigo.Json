namespace Vertigo.Json

module Mutable =
    open System
    open Vertigo.Json
    open Xunit

    type SimpleRecord = {
        mutable stringmember: string
    }

    [<Fact>]
    let ``Record with mutable serialization to string`` () =
        let value = { SimpleRecord.stringmember = "text" }
        let json = Json.serializeU value
        Assert.Equal("""{"stringmember":"text"}""", json)
        value.stringmember <- "other value"
        let json = Json.serializeU value
        Assert.Equal("""{"stringmember":"other value"}""", json)

    [<Fact>]
    let ``Record with mutable serialization-deserialization`` () =
        let value = { SimpleRecord.stringmember = "text" }
        let json = Json.serialize(value)
        let actual = Json.deserialize<SimpleRecord>(json)
        Assert.Equal(value, actual)
