namespace Vertigo.Json

module Tracing =
    open System
    open Vertigo.Json
    open Vertigo.Json.Tracing
    open Xunit

    let deserializationShouldFail<'T> json =
        Assert.Throws<JsonDeserializationException>(fun () -> Json.deserialize<'T>(json) |> ignore)

    type InternalRecord = {
        nested: string
    }

    type OutsideRecord = {
        themember: InternalRecord
    }

    [<Fact>]
    let ``Record member tracing`` () =
        let json = """{"themember":{}}"""
        let ex = deserializationShouldFail<OutsideRecord>(json)
        Assert.Equal("themember.nested", traceToString ex.Trace)

    type OutsideCollection = {
        thelist: InternalRecord list
    }

    [<Fact>]
    let ``List item tracing`` () =
        let json = """{"thelist":[{}]}"""
        let ex = deserializationShouldFail<OutsideCollection>(json)
        Assert.Equal("thelist[0].nested", traceToString ex.Trace)

    type TheUnion =
    | First of string
    | Second of string

    type UnionContainer = {
        themember: TheUnion
    }

    [<Fact>]
    let ``Union case tracing`` () =
        let json = """{"themember":{"Unknown": "something"}}"""
        let ex = deserializationShouldFail<UnionContainer>(json)
        Assert.Equal("themember", traceToString ex.Trace)
