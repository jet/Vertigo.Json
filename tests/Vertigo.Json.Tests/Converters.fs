namespace Vertigo.Json

module Converters =
    open System
    open Vertigo.Json
    open Xunit

    type DecimalRecord = {
        [<JsonProperty(Converter=typeof<Converters.StringToDecimal>)>]
        amember: decimal
    }

    [<Fact>]
    let ``Decimal serialization to string`` () =
        let actual = Json.serializeU { DecimalRecord.amember = 10m }
        Assert.Equal("""{"amember":"10"}""", actual)

    [<Fact>]
    let ``Decimal deserialization from string`` () =
        let expected = { DecimalRecord.amember = 10m }
        let actual = Json.deserialize<DecimalRecord>("""{"amember":"10"}""")
        Assert.Equal(expected, actual)

    type IntRecord = {
        [<JsonProperty(Converter=typeof<Converters.StringToInt>)>]
        amember: int
    }

    [<Fact>]
    let ``Int serialization to string`` () =
        let actual = Json.serializeU { IntRecord.amember = 10 }
        Assert.Equal("""{"amember":"10"}""", actual)

    [<Fact>]
    let ``Int deserialization from string`` () =
        let expected = { IntRecord.amember = 10 }
        let actual = Json.deserialize<IntRecord>("""{"amember":"10"}""")
        Assert.Equal(expected, actual)

    type DateTimeRecord = {
        [<JsonProperty(Converter=typeof<Converters.DateTimeToEpoch>)>]
        amember: DateTime
    }

    [<Fact>]
    let ``DateTime serialization to epoch time`` () =
        let time = new DateTime(2017, 5, 28, 22, 51, 52)
        let actual = Json.serializeU { DateTimeRecord.amember = time }
        Assert.Equal("""{"amember":1496011912}""", actual)

    [<Fact>]
    let ``DateTime deserialization from epoch time`` () =
        let time = new DateTime(2017, 5, 28, 22, 51, 52)
        let expected = { DateTimeRecord.amember = time }
        let actual = Json.deserialize<DateTimeRecord>("""{"amember":1496011912}""")
        Assert.Equal(expected, actual)

    type DateTimeOffsetRecord = {
        [<JsonProperty(Converter=typeof<Converters.DateTimeOffsetToEpoch>)>]
        amember: DateTimeOffset
    }

    [<Fact>]
    let ``DateTimeOffset serialization to epoch time`` () =
        let time = new DateTimeOffset(2017, 5, 28, 22, 51, 52, TimeSpan(0L))
        let actual = Json.serializeU { DateTimeOffsetRecord.amember = time }
        Assert.Equal("""{"amember":1496011912}""", actual)

    [<Fact>]
    let ``DateTimeOffset deserialization from epoch time`` () =
        let time = new DateTimeOffset(2017, 5, 28, 22, 51, 52, TimeSpan(0L))
        let expected = { DateTimeOffsetRecord.amember = time }
        let actual = Json.deserialize<DateTimeOffsetRecord>("""{"amember":1496011912}""")
        Assert.Equal(expected, actual)
