namespace Vertigo.Json

module Option = 
    open Vertigo.Json
    open NUnit.Framework

    type OptionInt = {
        amember: int option
    }

    type Container = {
        nested: OptionInt option
    }

    [<Test>]
    let ``Option none primitive`` () =
        let value = { OptionInt.amember = None }
        let s = Json.serialize value
        let actual = Json.deserialize<OptionInt>(s)
        Assert.AreEqual(value, actual)

    [<Test>]
    let ``Option none from null`` () =
        let value = { OptionInt.amember = None }
        let s = Json.serialize value
        let json = """{"amember": null}"""
        let actual = Json.deserialize<OptionInt>(json)
        Assert.True(actual.amember.IsNone);

    [<Test>]
    let ``None serialized into null`` () =
        let value = { OptionInt.amember = None }
        let actual = Json.serializeU value
        Assert.AreEqual("""{"amember":null}""", actual)

    [<Test>]
    let ``Option some primitive`` () =
        let value = { OptionInt.amember = Some 5 }
        let s = Json.serialize(value)
        let actual = Json.deserialize<OptionInt>(s)
        Assert.AreEqual(value, actual)

    [<Test>]
    let ``Option none record`` () =
        let value = { Container.nested = None }
        let s = Json.serialize value
        let actual = Json.deserialize<Container>(s)
        Assert.AreEqual(value, actual)

    [<Test>]
    let ``Option some record`` () =
        let value = { Container.nested = Some { OptionInt.amember = None } }
        let s = Json.serialize value
        let actual = Json.deserialize<Container>(s)
        Assert.AreEqual(value, actual)
        
