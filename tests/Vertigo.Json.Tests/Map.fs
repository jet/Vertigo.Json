namespace Vertigo.Json

module Map =
    open System
    open Vertigo.Json
    open Xunit

    type ContainingMap = {
        amember: Map<string, string>
    }


    [<Fact>]
    let ``Map string->string serialization`` () =
        let expected = """{"amember":{"thekey":"thevalue"}}"""
        let value = { ContainingMap.amember = [("thekey", "thevalue")] |> Map.ofList }
        let json = Json.serializeU value
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Map string->string deserialization`` () =
        let json = """{"amember":{"thekey":"thevalue"}}"""
        let expected = { ContainingMap.amember = [("thekey", "thevalue")] |> Map.ofList }
        let actual = Json.deserialize<ContainingMap> json
        Assert.Equal(expected, actual)

    type MapStringObj = {
        amember: Map<string, obj>
    }

    [<Fact>]
    let ``Map string->obj serialization`` () =
        let expected = """{"amember":{"thekey":"thevalue"}}"""
        let value = { MapStringObj.amember = [("thekey", "thevalue" :> obj)] |> Map.ofList }
        let json = Json.serializeU value
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Map string->obj deserialization`` () =
        let json = """{"amember":{"thekey":"thevalue"}}"""
        let expected = { MapStringObj.amember = [("thekey", "thevalue" :> obj)] |> Map.ofList }
        let actual = Json.deserialize<MapStringObj> json
        Assert.Equal(expected, actual)

    [<Fact>]
    let ``Map string->obj(list) serialization`` () =
        let expected = """{"amember":{"thekey":["thevalue"]}}"""
        let value = { MapStringObj.amember = [("thekey", ["thevalue"] :> obj)] |> Map.ofList }
        let json = Json.serializeU value
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Map string->obj(list) deserialization`` () =
        let expectedJson = """{"amember":{"thekey":["thevalue"]}}"""
        let actual = Json.deserialize<MapStringObj> expectedJson
        let actualJson = Json.serializeU actual
        Assert.Equal(expectedJson, actualJson)

    type SimpleRecord = {
        amember: string
    }
    [<Fact>]
    let ``Map serialization of none`` () =
        let data = [|"foo", (Some "baz") :> obj; "bar", None :> obj; "bon", 10 :> obj; "things", ([|"ping", "pong"|] |> Map.ofArray) :> obj     |] |> Map.ofArray
        let json = Json.serializeU data 
        let expected = """{"bar":null,"bon":10,"foo":"baz","things":{"ping":"pong"}}"""
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Map string->obj(record) serialization`` () =
        let expected = """{"amember":{"thekey":[{"amember":"thevalue"}]}}"""
        let value = { MapStringObj.amember = [("thekey", [{SimpleRecord.amember="thevalue"}] :> obj)] |> Map.ofList }
        let json = Json.serializeU value
        Assert.Equal(expected, json)

    [<Fact>]
    let ``Map string->obj(record) deserialization`` () =
        let expectedJson = """{"amember":{"thekey":[{"amember":"thevalue"}]}}"""
        let actual = Json.deserialize<MapStringObj> expectedJson
        let actualJson = Json.serializeU actual
        Assert.Equal(expectedJson, actualJson)
