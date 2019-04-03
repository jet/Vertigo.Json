namespace Vertigo.Json

module Union =
    open Vertigo.Json
    open Xunit

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

    [<Fact>]
    let ``Union serialization to string`` () =
        let value = { TheRecord.amember = StringCase "The text" }
        let json = Json.serializeU value
        let expected = """{"amember":{"StringCase":"The text"}}"""
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Union deserialization`` () =
        let value = { TheRecord.amember = StringCase "The text" }
        let json = Json.serialize(value)
        let actual = Json.deserialize<TheRecord>(json)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Union deserialization fallback`` () =
        let value = """{"amember": {"NonExistCase": "hello"}, "bmember": {"NumCase": 22}}"""
        let deserialized = Json.deserialize<TheRecordWithFallback> value
        match deserialized.amember with
        | Fallback x -> Assert.Equal(x.ToString(), "hello")
        | _ -> failwith "Not correctly matching fallback"

        match deserialized.bmember with
        | NumCase x -> Assert.Equal(x, 22)
        | _ -> failwith "Case not matching"

    [<Fact>]
    let ``Union multifield serialization`` () =
        let value = { TheRecord.amember = MultifieldCase ("The text", 5) }
        let json = Json.serialize(value)
        let actual = Json.deserialize<TheRecord>(json)
        Assert.Equal(value, actual)

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

    [<Fact>]
    let ``Union annoteted serialization to string`` () =
        let value = { TheAnnotatedRecord.amember = StringCase "The text" }
        let json = Json.serializeU value
        let expected = """{"amember":{"key":"StringCase","value":"The text"}}"""
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Union annoteted deserialization`` () =
        let expected = { TheAnnotatedRecord.amember = StringCase "The text" }
        let json = """{"amember":{"key":"StringCase","value":"The text"}}"""
        let actual = Json.deserialize<TheAnnotatedRecord>(json)
        Assert.Equal(expected, actual)

    [<Fact>]
    let ``Union annoteted serialization to string nested record`` () =
        let value = { TheAnnotatedRecord.amember = RecordCase {TheNestedRecord.amember = "The text"} }
        let json = Json.serializeU value
        let expected = """{"amember":{"key":"RecordCase","value":{"amember":"The text"}}}"""
        Assert.Equal(expected, json)
