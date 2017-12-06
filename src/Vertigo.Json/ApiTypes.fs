namespace Vertigo.Json

open System
open System.Text

/// Enum representations in JSON
type EnumValue =
    /// Represent Enum value with string
    | String = 0
    /// Represent Enum value with number
    | Number = 1

/// Attribute type for customizing serialization to/from JSON 
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
    member val public IsFallback : bool = false with get, set
    new () = JsonProperty(null)
with
    static member Default = JsonProperty()

/// Represent location in [Trace]
type Location =
| Property of string
| Item of int

/// Represents place in JSON structure
type Trace = Location list

/// Operations on [Trace]
module Tracing =
    /// Converts [Trace] into string in JSON path format
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

/// Exception that is thrown on errors during deserialization
type JsonDeserializationException(trace: Trace, msg: string) =
    inherit Exception(msg)
    member e.Trace = trace

/// Serialization behaviours for Option members 
type OptionBehaviour =
| NullForNone
| FieldMissing

/// Represents configuration for JSON serializer
type JsonConfig = {
    option: OptionBehaviour
}
with
    static member create(?option) =
        {
            JsonConfig.option = defaultArg option OptionBehaviour.NullForNone
        }
    static member defaultConfig = JsonConfig.create()
    