namespace Vertigo.Json

module NullSafety =
    open System
    open Vertigo.Json
    open Vertigo.Json.Tracing
    open Xunit

    type SimpleString = {
        amember: string
    }

    [<Fact>]
    let ``Null is not allowed for primitive non option`` () =
        let json = """{"amember": null}"""
        let ex = Assert.Throws<JsonDeserializationException>(fun () -> Json.deserialize<SimpleString>(json) |> ignore)
        Assert.NotNull(ex)

    [<Fact>]
    let ``Missing value is not allowed for primitive non option`` () =
        let ex = Assert.Throws<JsonDeserializationException>(fun () -> Json.deserialize<SimpleString>("{}") |> ignore)
        Assert.NotNull(ex)