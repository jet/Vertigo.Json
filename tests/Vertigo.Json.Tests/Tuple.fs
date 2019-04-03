namespace Vertigo.Json

module Tuple =
    open Vertigo.Json
    open Xunit

    type SimpleTuple = {
        amember: string*int
    }

    [<Fact>]
    let ``Tuple serialization to string`` () =
        let value = { SimpleTuple.amember = ("text", 4) }
        let json = Json.serializeU value
        let expected = """{"amember":["text",4]}"""
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Tuple serialization`` () =
        let value = { SimpleTuple.amember = ("text", 4) }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleTuple>(s)
        Assert.Equal(value, actual)

