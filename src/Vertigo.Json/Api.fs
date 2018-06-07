namespace Vertigo.Json

module Json =
    open Vertigo.Json
    open Vertigo.FSharp.Data
    open System.IO
    open System.Text

    open System
    open System.Reflection
    open Reflection
    open Internals

    let serializeX (config: JsonConfig) (objekt: obj): string =
        let json = Internals.Serialize.Root config (objekt.GetType()) objekt
        json.ToString()

    let serializeToBytesX (config: JsonConfig) (objekt: obj): byte array =
        let json = serializeX config objekt
        Encoding.UTF8.GetBytes json


    let serializeUX (config: JsonConfig) (objekt: obj): string =
        let json = Internals.Serialize.Root config (objekt.GetType()) objekt
        json.ToString(JsonSaveOptions.DisableFormatting)
    
    let deserializeX<'T> (config: JsonConfig) (json: string): 'T =
        let value = JsonValue.Parse(json)
        (Internals.Deserialize.Root config typeof<'T> value) :?> 'T

    let tryDeserializeX<'T> config json : Choice<'T, exn> =
        try
            Choice1Of2 <|  deserializeX<'T> config json
        with
        | ex -> 
            Choice2Of2 ex

    let deserializeFromBytesX<'T> (config: JsonConfig) (data: byte array) : 'T =
        let json = Encoding.UTF8.GetString data
        deserializeX<'T> config json

    let tryDeserializeFromBytesX<'T> config data : Choice<'T, exn> = 
        try 
            Choice1Of2 <| deserializeFromBytesX config data
        with
        | ex ->
            Choice2Of2 ex

    let jsonFieldsX<'T> (config: JsonConfig) =
        let t = typeof<'T>
        match t with
        | t when isRecord t -> ()
        | _ -> failwith "Can not get json fields from any type other then record"
        let props: PropertyInfo [] = getRecordFields(t)
        let fields = props |> Array.map jsonFieldName
        fields

    let serialize (objekt: obj) = serializeX JsonConfig.defaultConfig objekt
    let serializeToBytes (objekt: obj) = serializeToBytesX JsonConfig.defaultConfig objekt
    let serializeU (objekt: obj) = serializeUX JsonConfig.defaultConfig objekt
    let deserialize<'T> (json: string) = deserializeX<'T> JsonConfig.defaultConfig json
    let tryDeserialize<'T> (json: string) = tryDeserializeX<'T> JsonConfig.defaultConfig json 
    let deserializeFromBytes<'T> (data: byte array) = deserializeFromBytesX<'T> JsonConfig.defaultConfig data
    let tryDeserializeFromBytes<'T> (data: byte array) = tryDeserializeFromBytesX<'T> JsonConfig.defaultConfig data
    let jsonFields<'T> () = jsonFieldsX<'T> JsonConfig.defaultConfig

    type IJsonSerializer<'T> =
        abstract member serialize: 'T -> string
        abstract member serializeToBytes: 'T -> byte array
        abstract member serializeU: 'T -> string
        abstract member deserialize: string -> 'T
        abstract member deserializeFromBytes: byte array -> 'T
        abstract member jsonFields: unit -> string array

    let serializer<'T> (config: JsonConfig) =
        { new IJsonSerializer<'T> with
            member x.serialize objekt = serializeX config objekt
            member x.serializeToBytes objekt = serializeToBytesX config objekt
            member x.serializeU objekt = serializeUX config objekt
            member x.deserialize json = deserializeX config json
            member x.deserializeFromBytes data = deserializeFromBytesX config data
            member x.jsonFields () = jsonFieldsX config
        }
