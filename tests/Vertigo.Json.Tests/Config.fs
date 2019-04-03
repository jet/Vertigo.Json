namespace Vertigo.Json

module Config =
    open System
    open Vertigo.Json
    open Vertigo.Json.Tracing
    open Xunit

    let deserializationShouldFail<'T> config json =
        Assert.Throws<JsonDeserializationException>(fun () -> Json.deserializeX<'T> config json |> ignore)

    type TheRecord = {
        themember: string option
    }

    [<Fact>]
    let ``Deserialize - Option value member can't be missing`` () =
        let json = """{}"""
        let config = JsonConfig.create(option = OptionBehaviour.NullForNone)
        let ex = deserializationShouldFail<TheRecord> config json
        Assert.Equal("themember", traceToString ex.Trace)

    [<Fact>]
    let ``Deserialize - Option value member ommitted`` () =
        let json = """{}"""
        let expected = {TheRecord.themember = None}
        let config = JsonConfig.create(option = OptionBehaviour.FieldMissing)
        let actual = Json.deserializeX<TheRecord> config json
        Assert.Equal(expected, actual)

    [<Fact>]
    let ``Deserialize - Option value member present as null`` () =
        let json = """{"themember":null}"""
        let expected = {TheRecord.themember = None}
        let config = JsonConfig.create(option = OptionBehaviour.NullForNone)
        let actual = Json.deserializeX<TheRecord> config json
        Assert.Equal(expected, actual)

    [<Fact>]
    let ``Serialize - None value member as null`` () =
        let config = JsonConfig.create(option = OptionBehaviour.NullForNone)
        let json = Json.serializeUX config { TheRecord.themember = None }
        Assert.Equal("""{"themember":null}""", json)

    [<Fact>]
    let ``Serialize - None value member ommitted`` () =
        let config = JsonConfig.create(option = OptionBehaviour.FieldMissing)
        let json = Json.serializeUX config { TheRecord.themember = None }
        Assert.Equal("""{}""", json)
