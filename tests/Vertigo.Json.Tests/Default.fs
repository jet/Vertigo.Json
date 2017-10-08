namespace Vertigo.Json

module Default =
    open System
    open Vertigo.Json
    open NUnit.Framework

    type SimpleRecord = {
        [<JsonProperty(PropertyName="stringmember", DefaultValue = "Default")>]
        StringMember: string
    }

    [<Test>]
    let ``Record deserialization from string`` () =
        let expected = { SimpleRecord.StringMember = "Default" }
        let actual = Json.deserialize<SimpleRecord>("{}")
        Assert.AreEqual(expected, actual)
