namespace Vertigo.Json

module Raw =
    open System
    open Vertigo.FSharp.Data
    open Vertigo.Json
    open NUnit.Framework

    type RecordWithRaw = {
        [<JsonProperty(Raw=true)>]
        rawmember: string
    }

    [<Test>]
    let ``Raw value serialization to string`` () =
        let expected = """{"rawmember":{"first":"value"}}"""
        let value = { RecordWithRaw.rawmember = """{"first":"value"}""" }
        let json = Json.serializeU value
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``Raw value deserialization from string`` () =
        let json = """{"rawmember":{"first":"value"}}"""
        let actual = Json.deserialize<RecordWithRaw> json
        Assert.AreEqual("""{"first":"value"}""", actual.rawmember)

    [<Test>]
    let ``Raw value serialization to string from non-empty string`` () =
        let expected = """{"rawmember":"the string"}"""
        let value = { RecordWithRaw.rawmember = "the string" }
        let json = Json.serializeU value
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``Raw value serialization to string from empty string`` () =
        let expected = """{"rawmember":""}"""
        let value = { RecordWithRaw.rawmember = "" }
        let json = Json.serializeU value
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``Raw value deserialization from non-empty string`` () =
        let json = """{"rawmember":"the string"}"""
        let expected = { RecordWithRaw.rawmember = "the string" }
        let actual = Json.deserialize<RecordWithRaw> json
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Raw value deserialization from empty string`` () =
        let json = """{"rawmember":""}"""
        let expected = { RecordWithRaw.rawmember = "" }
        let actual = Json.deserialize<RecordWithRaw> json
        Assert.AreEqual(expected, actual)

    type RecordWithRawOption = {
        [<JsonProperty(Raw=true)>]
        rawmember: string option
    }

    [<Test>]
    let ``Raw option some value serialization to string`` () =
        let expected = """{"rawmember":{"first":"value"}}"""
        let value = { RecordWithRawOption.rawmember = Some """{"first":"value"}""" }
        let json = Json.serializeU value
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``Raw option none serialization to string`` () =
        let value = { RecordWithRawOption.rawmember = None }
        let json = Json.serializeU value
        Assert.AreEqual("""{"rawmember":null}""", json)

    [<Test>]
    let ``Raw option some value deserialization from string`` () =
        let json = """{"rawmember":{"first":"value"}}"""
        let actual = Json.deserialize<RecordWithRawOption> json
        Assert.AreEqual(Some """{"first":"value"}""", actual.rawmember)

    [<Test>]
    let ``Raw option none deserialization from string`` () =
        let actual = Json.deserialize<RecordWithRawOption> """{"rawmember":null}"""
        Assert.AreEqual(None, actual.rawmember)
