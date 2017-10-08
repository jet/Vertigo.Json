namespace Vertigo.Json

open System
open System.Text

type EnumValue =
    | String = 0
    | Number = 1

type JsonProperty (propertyName: string) =
    inherit Attribute()
    member val public PropertyName: string = propertyName with get, set
    member val public CaseKey: string = null with get, set
    member val public CaseValue: string = null with get, set
    member val public Case: string = null with get, set
    member val public Raw: bool = false with get, set
    member val public Converter: Type = null with get, set
    member val public EnumValue: EnumValue = EnumValue.String with get, set
    member val public DefaultValue: obj = null with get, set
    member val public DateTimeFormat: string = null with get, set
    new () = JsonProperty(null)
with
    static member Default = JsonProperty()


type Location =
| Property of string
| Item of int

type Trace = Location list

module Tracing =
    let traceToString (trace: Trace) =
        match trace.Length with
        | 0 -> "JSON root"
        | _ ->
            let value = new StringBuilder()
            trace |> List.iteri (fun index location ->
                match location with
                | Property theProperty ->
                    if index <> 0 then value.Append "." |> ignore
                    value.Append theProperty |> ignore
                | Item item -> item |> sprintf "[%i]" |> value.Append |> ignore
            )
            value.ToString()

type JsonDeserializationException(trace: Trace, msg: string) =
    inherit Exception(msg)
    member e.Trace = trace

type OptionBehaviour =
| NullForNone
| FieldMissing

type JsonConfig = {
    option: OptionBehaviour
}
with
    static member create(?option) =
        {
            JsonConfig.option = defaultArg option OptionBehaviour.NullForNone
        }
    static member defaultConfig = JsonConfig.create()
    