namespace Vertigo.Json

module List =
    open System
    open Vertigo.Json
    open NUnit.Framework

    type StringList = {
        amember: string list
    }

    type StringListOption = {
        amember: string list option
    }

    type IntList = {
        amember: int list
    }

    [<Test>]
    let ``List of strings serialization to string`` () =
        let expected = """{"amember":["some","text"]}"""
        let value = { StringList.amember = ["some"; "text"] }
        let json = Json.serializeU value
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``List of strings serialization`` () =
        let value = { StringList.amember = ["some"; "text"] }
        let json = Json.serialize(value)
        let actual = Json.deserialize<StringList>(json)
        Assert.AreEqual(value, actual)

    [<Test>]
    let ``List of ints serialization`` () =
        let value = { IntList.amember = [3; 4] }
        let json = Json.serialize(value)
        let actual = Json.deserialize<IntList>(json)
        Assert.AreEqual(value, actual)

    [<Test>]
    let ``List of strings option none to string`` () =
        let expected = """{"amember":null}"""
        let value = { StringListOption.amember = None }
        let json = Json.serializeU value
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``List of strings option none from string`` () =
        let json = """{"amember":null}"""
        let expected = { StringListOption.amember = None }
        let actual = Json.deserialize<StringListOption> json
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``List missing from string`` () =
        Assert.Throws<JsonDeserializationException>(fun () -> Json.deserialize<StringList> "{}" |> ignore) |> ignore

    type Simple = {
        amember: string
    }

    [<Test>]
    let ``List on root level serialization`` () =
        let expected = """[{"amember":"bla"}]"""
        let data = [{ Simple.amember = "bla" }]
        let actual = Json.serializeU data
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``List on root level deserialization`` () =
        let json = """[{"amember":"bla"}]"""
        let expected = { Simple.amember = "bla" }
        let actual = Json.deserialize<Simple list> json
        Assert.AreEqual(1, actual.Length)
        Assert.AreEqual(expected, actual.[0])

    [<Test>]
    let ``List on root level deserialization from empty string`` () =
        let json = "[]"
        let actual = Json.deserialize<Simple list> json
        Assert.AreEqual(0, actual.Length)