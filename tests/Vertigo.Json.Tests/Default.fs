namespace Vertigo.Json

module Default =
    open System
    open Vertigo.Json
    open Xunit

    type SimpleRecord = {
        [<JsonProperty(PropertyName="stringmember", DefaultValue = "Default")>]
        StringMember: string
    }

    [<Fact>]
    let ``Record deserialization from string`` () =
        let expected = { SimpleRecord.StringMember = "Default" }
        let actual = Json.deserialize<SimpleRecord>("{}")
        Assert.Equal(expected, actual)
