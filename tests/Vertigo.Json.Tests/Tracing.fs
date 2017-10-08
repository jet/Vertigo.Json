namespace Vertigo.Json

module Tracing =
    open System
    open Vertigo.Json
    open Vertigo.Json.Tracing
    open NUnit.Framework

    let deserializationShouldFail<'T> json =
        Assert.Throws<JsonDeserializationException>(fun () -> Json.deserialize<'T>(json) |> ignore)

    type InternalRecord = {
        nested: string
    }

    type OutsideRecord = {
        themember: InternalRecord
    }

    [<Test>]
    let ``Record member tracing`` () =
        let json = """{"themember":{}}"""
        let ex = deserializationShouldFail<OutsideRecord>(json)
        Assert.AreEqual("themember.nested", traceToString ex.Trace)

    type OutsideCollection = {
        thelist: InternalRecord list
    }

    [<Test>]
    let ``List item tracing`` () =
        let json = """{"thelist":[{}]}"""
        let ex = deserializationShouldFail<OutsideCollection>(json)
        Assert.AreEqual("thelist[0].nested", traceToString ex.Trace)

    type TheUnion =
    | First of string
    | Second of string

    type UnionContainer = {
        themember: TheUnion
    }

    [<Test>]
    let ``Union case tracing`` () =
        let json = """{"themember":{"Unknown": "something"}}"""
        let ex = deserializationShouldFail<UnionContainer>(json)
        Assert.AreEqual("themember", traceToString ex.Trace)
