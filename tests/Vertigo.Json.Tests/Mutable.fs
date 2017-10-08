namespace Vertigo.Json

module Mutable =
    open System
    open Vertigo.Json
    open NUnit.Framework

    type SimpleRecord = {
        mutable stringmember: string
    }

    [<Test>]
    let ``Record with mutable serialization to string`` () =
        let value = { SimpleRecord.stringmember = "text" }
        let json = Json.serializeU value
        Assert.AreEqual("""{"stringmember":"text"}""", json)
        value.stringmember <- "other value"
        let json = Json.serializeU value
        Assert.AreEqual("""{"stringmember":"other value"}""", json)

    [<Test>]
    let ``Record with mutable serialization-deserialization`` () =
        let value = { SimpleRecord.stringmember = "text" }
        let json = Json.serialize(value)
        let actual = Json.deserialize<SimpleRecord>(json)
        Assert.AreEqual(value, actual)
