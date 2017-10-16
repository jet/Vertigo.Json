namespace Vertigo.Json

module internal JsonConversions =
    open System
    open Vertigo.FSharp.Data
    open Vertigo.FSharp.Data.Runtime
    open Tracing

    let fail trace typeName value =
        raise <| JsonDeserializationException(trace, sprintf "Expected to be %s, saw: %A" typeName value)

    let AsString (trace: Trace) (value: JsonValue) =
        match value with
        | JsonValue.String s -> s
        | _ -> fail trace "string" value

    let AsInteger (trace: Trace) (value: JsonValue) =
        match value with
        | JsonValue.Number n -> int n
        | JsonValue.Float n -> int n
        | _ -> fail trace "number" value

    let AsInteger64 (trace: Trace) (value: JsonValue) =
        match value with
        | JsonValue.Number n -> int64 n
        | JsonValue.Float n -> int64 n
        | _ -> fail trace "number" value

    let AsDecimal (trace: Trace) (value: JsonValue) =
        match value with
        | JsonValue.Number n -> n
        | JsonValue.Float n -> decimal n
        | _ -> fail trace "number" value

    let AsFloat (trace: Trace) (value: JsonValue) =
        match value with
        | JsonValue.Float n -> n
        | JsonValue.Number n -> float n
        | _ -> fail trace "number" value

    let AsBoolean (trace: Trace) (value: JsonValue) =
        match value with
        | JsonValue.Boolean b -> b
        | _ -> fail trace "boolean" value

    let AsDateTime cultureInfo (trace: Trace) (value: JsonValue) =
        match value with
        | JsonValue.String s -> 
            let value = TextConversions.AsDateTime cultureInfo s
            match value with
            | Some value -> value
            | None -> fail trace "string containing datetime" value
        | _ -> fail trace "string" value

    let AsDateTimeOffset cultureInfo (trace: Trace) (value: JsonValue) =
        match value with
        | JsonValue.String s -> 
            let value = TextConversions.AsDateTimeOffset cultureInfo s
            match value with
            | Some value -> value
            | None -> fail trace "string containing DateTimeOffset" value
        | _ -> fail trace "string" value

    let AsGuid (trace: Trace) (value: JsonValue) =
        match value with
        | JsonValue.String s -> 
            let value = TextConversions.AsGuid s
            match value with
            | Some value -> value
            | None -> fail trace "string containing guid" value
        | _ -> fail trace "string" value

    let AsArray (trace: Trace) (value: JsonValue) =
        match value with
        | JsonValue.Array arr -> arr
        | _ -> fail trace "array" value
