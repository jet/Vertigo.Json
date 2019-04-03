namespace Vertigo.Json

module Enum =
    open Vertigo.Json
    open Xunit
    
    type Counting =
    | first = 1
    | second = 2
    | third = 3

    type SimpleEnum = {
        amember: Counting
    }

    [<Fact>]
    let ``Enum member serialization`` () =
        let value = { SimpleEnum.amember = Counting.second }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleEnum>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Enum deserialized from string`` () =
        let value = { SimpleEnum.amember = Counting.second }
        let json = """{"amember": "second"}"""
        let actual = Json.deserialize<SimpleEnum>(json)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Enum serialized to string`` () =
        let value = { SimpleEnum.amember = Counting.second }
        let json = Json.serializeU value
        let expected = """{"amember":"second"}"""
        Assert.Equal(expected, json)

    type SimpleEnumInteger = {
        [<JsonProperty(EnumValue = EnumValue.Number)>]
        amember: Counting
    }

    [<Fact>]
    let ``Enum deserialized from integer`` () =
        let value = { SimpleEnumInteger.amember = Counting.second }
        let json = """{"amember":2}"""
        let actual = Json.deserialize<SimpleEnumInteger>(json)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Enum serialized to integer`` () =
        let value = { SimpleEnumInteger.amember = Counting.second }
        let json = Json.serializeU value
        let expected = """{"amember":2}"""
        Assert.Equal(expected, json)
