namespace Vertigo.Json

module Converters =
    open System
    open Vertigo.Json
    open NUnit.Framework

    type DecimalRecord = {
        [<JsonProperty(Converter=typeof<Converters.StringToDecimal>)>]
        amember: decimal
    }

    [<Test>]
    let ``Decimal serialization to string`` () =
        let actual = Json.serializeU { DecimalRecord.amember = 10m }
        Assert.AreEqual("""{"amember":"10"}""", actual)

    [<Test>]
    let ``Decimal deserialization from string`` () =
        let expected = { DecimalRecord.amember = 10m }
        let actual = Json.deserialize<DecimalRecord>("""{"amember":"10"}""")
        Assert.AreEqual(expected, actual)

    type IntRecord = {
        [<JsonProperty(Converter=typeof<Converters.StringToInt>)>]
        amember: int
    }

    [<Test>]
    let ``Int serialization to string`` () =
        let actual = Json.serializeU { IntRecord.amember = 10 }
        Assert.AreEqual("""{"amember":"10"}""", actual)

    [<Test>]
    let ``Int deserialization from string`` () =
        let expected = { IntRecord.amember = 10 }
        let actual = Json.deserialize<IntRecord>("""{"amember":"10"}""")
        Assert.AreEqual(expected, actual)

    type DateTimeRecord = {
        [<JsonProperty(Converter=typeof<Converters.DateTimeToEpoch>)>]
        amember: DateTime
    }

    [<Test>]
    let ``DateTime serialization to epoch time`` () =
        let time = new DateTime(2017, 5, 28, 22, 51, 52)
        let actual = Json.serializeU { DateTimeRecord.amember = time }
        Assert.AreEqual("""{"amember":1496011912}""", actual)

    [<Test>]
    let ``DateTime deserialization from epoch time`` () =
        let time = new DateTime(2017, 5, 28, 22, 51, 52)
        let expected = { DateTimeRecord.amember = time }
        let actual = Json.deserialize<DateTimeRecord>("""{"amember":1496011912}""")
        Assert.AreEqual(expected, actual)

    type DateTimeOffsetRecord = {
        [<JsonProperty(Converter=typeof<Converters.DateTimeOffsetToEpoch>)>]
        amember: DateTimeOffset
    }

    [<Test>]
    let ``DateTimeOffset serialization to epoch time`` () =
        let time = new DateTimeOffset(2017, 5, 28, 22, 51, 52, TimeSpan(0L))
        let actual = Json.serializeU { DateTimeOffsetRecord.amember = time }
        Assert.AreEqual("""{"amember":1496011912}""", actual)

    [<Test>]
    let ``DateTimeOffset deserialization from epoch time`` () =
        let time = new DateTimeOffset(2017, 5, 28, 22, 51, 52, TimeSpan(0L))
        let expected = { DateTimeOffsetRecord.amember = time }
        let actual = Json.deserialize<DateTimeOffsetRecord>("""{"amember":1496011912}""")
        Assert.AreEqual(expected, actual)
