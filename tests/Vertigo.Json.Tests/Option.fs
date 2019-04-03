namespace Vertigo.Json

module Option = 
    open Vertigo.Json
    open Xunit

    type OptionInt = {
        amember: int option
    }

    type Container = {
        nested: OptionInt option
    }

    [<Fact>]
    let ``Option none primitive`` () =
        let value = { OptionInt.amember = None }
        let s = Json.serialize value
        let actual = Json.deserialize<OptionInt>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Option none from null`` () =
        let value = { OptionInt.amember = None }
        let s = Json.serialize value
        let json = """{"amember": null}"""
        let actual = Json.deserialize<OptionInt>(json)
        Assert.True(actual.amember.IsNone);

    [<Fact>]
    let ``None serialized into null`` () =
        let value = { OptionInt.amember = None }
        let config = JsonConfig.create (OptionBehaviour.NullForNone)
        let actual = Json.serializeUX config  value
        Assert.Equal("""{"amember":null}""", actual)

    [<Fact>]
    let ``Option some primitive`` () =
        let value = { OptionInt.amember = Some 5 }
        let s = Json.serialize(value)
        let actual = Json.deserialize<OptionInt>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Option none record`` () =
        let value = { Container.nested = None }
        let s = Json.serialize value
        let actual = Json.deserialize<Container>(s)
        Assert.Equal(value, actual)

    [<Fact>]
    let ``Option some record`` () =
        let value = { Container.nested = Some { OptionInt.amember = None } }
        let s = Json.serialize value
        let actual = Json.deserialize<Container>(s)
        Assert.Equal(value, actual)
        