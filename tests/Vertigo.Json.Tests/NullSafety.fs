namespace Vertigo.Json

module NullSafety =
    open System
    open Vertigo.Json
    open Vertigo.Json.Tracing
    open NUnit.Framework

    type SimpleString = {
        amember: string
    }

    [<Test>]
    let ``Null is not allowed for primitive non option`` () =
        let json = """{"amember": null}"""
        let ex = Assert.Throws<JsonDeserializationException>(fun () -> Json.deserialize<SimpleString>(json) |> ignore)
        Assert.IsNotNull(ex)

    [<Test>]
    let ``Missing value is not allowed for primitive non option`` () =
        let ex = Assert.Throws<JsonDeserializationException>(fun () -> Json.deserialize<SimpleString>("{}") |> ignore)
        Assert.IsNotNull(ex)