namespace Vertigo.Json

module Tuple =
    open Vertigo.Json
    open NUnit.Framework

    type SimpleTuple = {
        amember: string*int
    }

    [<Test>]
    let ``Tuple serialization to string`` () =
        let value = { SimpleTuple.amember = ("text", 4) }
        let json = Json.serializeU value
        let expected = """{"amember":["text",4]}"""
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``Tuple serialization`` () =
        let value = { SimpleTuple.amember = ("text", 4) }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleTuple>(s)
        Assert.AreEqual(value, actual)

