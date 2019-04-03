namespace Vertigo.Json

module Array =
    open System
    open Vertigo.Json
    open Xunit

    type StringArray = {
        amember: string array
    }

    type StringArrayOption = {
        amember: string array option
    }

    type IntArray = {
        amember: int array
    }

    [<Fact>]
    let ``Array of strings serialization to string`` () =
        let expected = """{"amember":["some","text"]}"""
        let value = { StringArray.amember = [|"some"; "text"|] }
        let json = Json.serializeU value
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Array of strings serialization`` () =
        let value = { StringArray.amember = [|"some"; "text"|] }
        let json = Json.serialize(value)
        let actual = Json.deserialize<StringArray>(json)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Array of ints serialization`` () =
        let value = { IntArray.amember = [|3; 4|] }
        let json = Json.serialize(value)
        let actual = Json.deserialize<IntArray>(json)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Array of strings option none to string`` () =
        let expected = """{"amember":null}"""
        let value = { StringArrayOption.amember = None }
        let config = JsonConfig.create (OptionBehaviour.NullForNone)
        let json = Json.serializeUX config  value
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Array of strings option none from string`` () =
        let json = """{"amember":null}"""
        let expected = { StringArrayOption.amember = None }
        let actual = Json.deserialize<StringArrayOption> json
        Assert.Equal(expected, actual)

    [<Fact>]
    let ``Array missing from string`` () =
        Assert.Throws<JsonDeserializationException>(fun () -> Json.deserialize<StringArray> "{}" |> ignore) |> ignore
