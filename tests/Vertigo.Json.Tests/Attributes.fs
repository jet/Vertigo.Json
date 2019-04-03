namespace Vertigo.Json

module Attributes =
    open System
    open Vertigo.Json
    open Xunit

    type SimpleRecord = {
        [<JsonProperty(PropertyName="intmember")>]
        IntMember: int
        [<JsonProperty(PropertyName="stringmember")>]
        StringMember: string
    }

    [<Fact>]
    let ``Record serialization to string`` () =
        let expected = """{"intmember":3,"stringmember":"text"}"""
        let value = { SimpleRecord.IntMember = 3; StringMember = "text" }
        let json = Json.serializeU value
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Record deserialization from string`` () =
        let json = """{"intmember":3,"stringmember":"text"}"""
        let expected = { SimpleRecord.IntMember = 3; StringMember = "text" }
        let actual = Json.deserialize<SimpleRecord>(json)
        Assert.Equal(expected, actual)

    type TheUnion =
    | [<JsonProperty(Case="stringcase")>] StringCase of string
    | [<JsonProperty(Case="multifieldcase")>] MultifieldCase of string*int

    type TheRecord = {
        amember: TheUnion
    }

    [<Fact>]
    let ``Union serialization to string`` () =
        let value = { TheRecord.amember = StringCase "The text" }
        let json = Json.serializeU value
        let expected = """{"amember":{"stringcase":"The text"}}"""
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Union deserialization from string`` () =
        let expected = { TheRecord.amember = StringCase "The text" }
        let json = """{"amember":{"stringcase":"The text"}}"""
        let actual = Json.deserialize<TheRecord>(json)
        Assert.Equal(expected, actual)

    type ShortJsonProperty = {
        [<JsonProperty("intmember")>]
        IntMember: int
    }

    [<Fact>]
    let ``Record - short form JsonProperty form serialization to string`` () =
        let expected = """{"intmember":3}"""
        let value = { ShortJsonProperty.IntMember = 3 }
        let json = Json.serializeU value
        Assert.Equal(expected, json)

    type MixedJsonProperty = {
        [<JsonProperty("intmember", DefaultValue = 3)>]
        IntMember: int
    }

    [<Fact>]
    let ``Record - mixed form JsonProperty form serialization to string`` () =
        let expected = """{"intmember":3}"""
        let value = { ShortJsonProperty.IntMember = 3 }
        let json = Json.serializeU value
        Assert.Equal(expected, json)

    type CustomFormatDateTime = {
        [<JsonProperty(DateTimeFormat="yyyy-MM-ddTHH:mm:ss")>]
        amember: DateTime
    }

    [<Fact>]
    let ``DateTime member serialization custom format`` () =
        let expected = """{"amember":"2017-05-28T22:51:52"}"""
        let therec = Json.deserialize<CustomFormatDateTime>(expected)
        let actual = Json.serializeU(therec)
        Assert.Equal(expected, actual)

