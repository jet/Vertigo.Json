namespace Vertigo.Json

module Array =
    open System
    open Vertigo.Json
    open NUnit.Framework

    type StringArray = {
        amember: string array
    }

    type StringArrayOption = {
        amember: string array option
    }

    type IntArray = {
        amember: int array
    }

    [<Test>]
    let ``Array of strings serialization to string`` () =
        let expected = """{"amember":["some","text"]}"""
        let value = { StringArray.amember = [|"some"; "text"|] }
        let json = Json.serializeU value
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``Array of strings serialization`` () =
        let value = { StringArray.amember = [|"some"; "text"|] }
        let json = Json.serialize(value)
        let actual = Json.deserialize<StringArray>(json)
        Assert.AreEqual(value, actual)

    [<Test>]
    let ``Array of ints serialization`` () =
        let value = { IntArray.amember = [|3; 4|] }
        let json = Json.serialize(value)
        let actual = Json.deserialize<IntArray>(json)
        Assert.AreEqual(value, actual)

    [<Test>]
    let ``Array of strings option none to string`` () =
        let expected = """{"amember":null}"""
        let value = { StringArrayOption.amember = None }
        let config = JsonConfig.create (OptionBehaviour.NullForNone)
        let json = Json.serializeUX config  value
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``Array of strings option none from string`` () =
        let json = """{"amember":null}"""
        let expected = { StringArrayOption.amember = None }
        let actual = Json.deserialize<StringArrayOption> json
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Array missing from string`` () =
        Assert.Throws<JsonDeserializationException>(fun () -> Json.deserialize<StringArray> "{}" |> ignore) |> ignore
