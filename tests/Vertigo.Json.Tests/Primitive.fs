namespace Vertigo.Json

module Primitive = 

    open System
    open Vertigo.Json
    open Xunit

    type SimpleString = {
        amember: string
    }

    type SimpleDecimal = {
        amember: decimal
    }

    type SimpleBool = {
        amember: bool
    }

    type SimpleInt = {
        amember: int
    }

    type SimpleInt64 = {
        amember: int64
    }

    type SimpleFloat = {
        amember: float
    }

    type SimpleGuid = {
        amember: Guid
    }

    type SimpleDateTime = {
        amember: DateTime
    }

    type SimpleDateTimeOffset = {
        amember: DateTimeOffset
    }

    [<Fact>]
    let ``String member serialization`` () =
        let value = { SimpleString.amember = "Some string" }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleString>(s)
        Assert.Equal(value, actual)
        
    [<Fact>]
    let ``Decimal member serialization`` () =
        let value = { SimpleDecimal.amember = 100M }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleDecimal>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Bool member serialization`` () =
        let value = { SimpleBool.amember = true }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleBool>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Int member serialization`` () =
        let value = { SimpleInt.amember = 13 }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleInt>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Int64 member serialization`` () =
        let value = { SimpleInt64.amember = 13L }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleInt64>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Float member serialization`` () =
        let value = { SimpleFloat.amember = 13.0 }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleFloat>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Guid member serialization`` () =
        let value = { SimpleGuid.amember = Guid.NewGuid() }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleGuid>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``DateTime member serialization without ticks`` () =
        let now = DateTime.Now
        let dateTime = now.AddTicks( - (now.Ticks % TimeSpan.TicksPerSecond));
        let value = { SimpleDateTime.amember = dateTime }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleDateTime>(s)
        Assert.Equal(value, actual)

//    [<Fact>]
//    let ``DateTime member serialization Newtonsoft format`` () =
//        let expected = """{"amember":"2017-05-28T22:51:52.1236713-04:00"}"""
//        let therec = Json.deserialize<SimpleDateTime>(expected)
//        let actual = Json.serializeU(therec)
//        Assert.Equal(expected, actual)

    [<Fact>]
    let ``DateTimeOffset member serialization`` () =
        let now = DateTimeOffset.UtcNow
        let dateTime = now.AddTicks( - (now.Ticks % TimeSpan.TicksPerSecond));
        let value = { SimpleDateTimeOffset.amember = dateTime }
        let s = Json.serialize(value)
        let actual = Json.deserialize<SimpleDateTimeOffset>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``DateTimeOffset member serialization Newtonsoft format`` () =
        let expected = """{"amember":"2017-05-28T22:51:52.1236713-04:00"}"""
        let therec = Json.deserialize<SimpleDateTimeOffset>(expected)
        let actual = Json.serializeU(therec)
        Assert.Equal(expected, actual)

    [<Fact>]
    let ``DateTimeOffset member deserialization offset`` () =
        let expected = new DateTimeOffset(2017, 5, 28, 22, 51, 52, new TimeSpan(0L))
        let json = """{"amember":"2017-05-28T22:51:52"}"""
        let therec = Json.deserialize<SimpleDateTimeOffset>(json)
        Assert.Equal(expected, therec.amember)

//    type SimpleTimeSpan = {
//        amember: TimeSpan
//    }
//
//    [<Fact>]
//    let ``TimeSpan member serialization`` () =
//        let value = { SimpleTimeSpan.amember = TimeSpan(1, 3, 4, 5) }
//        let s = Json.serializeToString(value)
//        let actual = Json.deserializeFromStringT<SimpleTimeSpan>(s)
//        Assert.Equal(value, actual)
