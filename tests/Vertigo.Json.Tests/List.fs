namespace Vertigo.Json

module List =
    open System
    open Vertigo.Json
    open Xunit

    type StringList = {
        amember: string list
    }

    type StringListOption = {
        amember: string list option
    }

    type IntList = {
        amember: int list
    }

    [<Fact>]
    let ``List of strings serialization to string`` () =
        let expected = """{"amember":["some","text"]}"""
        let value = { StringList.amember = ["some"; "text"] }
        let json = Json.serializeU value
        Assert.Equal(expected, json)

    [<Fact>]
    let ``List of strings serialization`` () =
        let value = { StringList.amember = ["some"; "text"] }
        let json = Json.serialize(value)
        let actual = Json.deserialize<StringList>(json)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``List of ints serialization`` () =
        let value = { IntList.amember = [3; 4] }
        let json = Json.serialize(value)
        let actual = Json.deserialize<IntList>(json)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``List of strings option none to string`` () =
        let expected = """{"amember":null}"""
        let value = { StringListOption.amember = None }
        let config = JsonConfig.create (OptionBehaviour.NullForNone)
        let json = Json.serializeUX config  value
        Assert.Equal(expected, json)

    [<Fact>]
    let ``List of strings option none from string`` () =
        let json = """{"amember":null}"""
        let expected = { StringListOption.amember = None }
        let actual = Json.deserialize<StringListOption> json
        Assert.Equal(expected, actual)

    [<Fact>]
    let ``List missing from string`` () =
        Assert.Throws<JsonDeserializationException>(fun () -> Json.deserialize<StringList> "{}" |> ignore) |> ignore

    type Simple = {
        amember: string
    }

    [<Fact>]
    let ``List on root level serialization`` () =
        let expected = """[{"amember":"bla"}]"""
        let data = [{ Simple.amember = "bla" }]
        let actual = Json.serializeU data
        Assert.Equal(expected, actual)

    [<Fact>]
    let ``List on root level deserialization`` () =
        let json = """[{"amember":"bla"}]"""
        let expected = { Simple.amember = "bla" }
        let actual = Json.deserialize<Simple list> json
        Assert.Equal(1, actual.Length)
        Assert.Equal(expected, actual.[0])

    [<Fact>]
    let ``List on root level deserialization from empty string`` () =
        let json = "[]"
        let actual = Json.deserialize<Simple list> json
        Assert.Equal(0, actual.Length)
