namespace Vertigo.Json

module Union =
    open Vertigo.Json
    open NUnit.Framework

    type TheUnion =
    | StringCase of string
    | MultifieldCase of string*int

    type TheUnionWithFallback =
    | NumCase of int
    | [<JsonProperty(IsFallback = true)>] Fallback of obj

    type TheRecord = {
        amember: TheUnion
    }

    type TheRecordWithFallback = {
        amember : TheUnionWithFallback
        bmember : TheUnionWithFallback
    }

    [<Test>]
    let ``Union serialization to string`` () =
        let value = { TheRecord.amember = StringCase "The text" }
        let json = Json.serializeU value
        let expected = """{"amember":{"StringCase":"The text"}}"""
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``Union deserialization`` () =
        let value = { TheRecord.amember = StringCase "The text" }
        let json = Json.serialize(value)
        let actual = Json.deserialize<TheRecord>(json)
        Assert.AreEqual(value, actual)

    [<Test>]
    let ``Union deserialization fallback`` () =
        let value = """{"amember": {"NonExistCase": "hello"}, "bmember": {"NumCase": 22}}"""
        let deserialized = Json.deserialize<TheRecordWithFallback> value
        match deserialized.amember with
        | Fallback x -> Assert.AreEqual(x.ToString(), "hello")
        | _ -> failwith "Not correctly matching fallback"

        match deserialized.bmember with
        | NumCase x -> Assert.AreEqual(x, 22)
        | _ -> failwith "Case not matching"

    [<Test>]
    let ``Union multifield serialization`` () =
        let value = { TheRecord.amember = MultifieldCase ("The text", 5) }
        let json = Json.serialize(value)
        let actual = Json.deserialize<TheRecord>(json)
        Assert.AreEqual(value, actual)

    type TheNestedRecord = {
        amember: string
    }

    [<JsonProperty(CaseKey="key", CaseValue="value")>]
    type TheAnnotatedUnion =
    | StringCase of string
    | MultifieldCase of string*int
    | RecordCase of TheNestedRecord

    type TheAnnotatedRecord = {
        amember: TheAnnotatedUnion
    }

    [<Test>]
    let ``Union annoteted serialization to string`` () =
        let value = { TheAnnotatedRecord.amember = StringCase "The text" }
        let json = Json.serializeU value
        let expected = """{"amember":{"key":"StringCase","value":"The text"}}"""
        Assert.AreEqual(expected, json)

    [<Test>]
    let ``Union annoteted deserialization`` () =
        let expected = { TheAnnotatedRecord.amember = StringCase "The text" }
        let json = """{"amember":{"key":"StringCase","value":"The text"}}"""
        let actual = Json.deserialize<TheAnnotatedRecord>(json)
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Union annoteted serialization to string nested record`` () =
        let value = { TheAnnotatedRecord.amember = RecordCase {TheNestedRecord.amember = "The text"} }
        let json = Json.serializeU value
        let expected = """{"amember":{"key":"RecordCase","value":{"amember":"The text"}}}"""
        Assert.AreEqual(expected, json)
