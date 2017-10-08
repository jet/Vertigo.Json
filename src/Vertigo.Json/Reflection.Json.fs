namespace Vertigo.Json

/// [omit]
module Reflection =
    open System
    open System.Globalization
    open System.Collections
    open System.Collections.Generic
    open System.Collections.Concurrent
    open System.Reflection
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Reflection    

    let cache (f:'a -> 'b) =
        let f = new Func<_,_>(f)
        let cache = new ConcurrentDictionary<'a, 'b>()
        fun k -> cache.GetOrAdd(k, f)

    type FSharpType
        with 
            static member IsOption (t: Type): bool =
                t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>

            static member GetOptionType (t: Type): Type =
                let types = t.GetGenericArguments()
                types.[0]

            static member IsList (t: Type) =
                t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<List<_>>

            static member IsArray (t: Type) =
                t.IsArray

            static member IsMap (t: Type) =
                t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Map<_,_>>

            static member GetListItemType (t: Type) =
                let types = t.GetGenericArguments()
                types.[0]

            static member GetMapKeyType (t: Type) =
                let types = t.GetGenericArguments()
                types.[0]

            static member GetMapValueType (t: Type) =
                let types = t.GetGenericArguments()
                types.[1]

            static member GetListType (itemType: Type) =
                typedefof<List<_>>.MakeGenericType([| itemType |])

            static member GetListConstructor (t: Type) =
                t.GetMethod ("Cons")

            static member GetListEmpty (t: Type) =
                t.GetProperty("Empty")

    let getRecordFields: Type -> PropertyInfo [] = FSharpType.GetRecordFields |> cache
    let getUnionCases: Type -> UnionCaseInfo [] = FSharpType.GetUnionCases |> cache
    let getTupleElements: Type -> Type [] = FSharpType.GetTupleElements |> cache
    let getOptionType: Type -> Type = FSharpType.GetOptionType |> cache
    let getListItemType: Type -> Type = FSharpType.GetListItemType |> cache
    let getMapValueType: Type -> Type = FSharpType.GetMapValueType |> cache
    let getListType: Type -> Type = FSharpType.GetListType |> cache
    let getListConstructor: Type -> MethodInfo = FSharpType.GetListConstructor |> cache
    let getListEmpty: Type -> PropertyInfo = FSharpType.GetListEmpty |> cache
    let isTuple: Type -> bool = FSharpType.IsTuple |> cache
    let isList: Type -> bool = FSharpType.IsList |> cache
    let isArray: Type -> bool = FSharpType.IsArray |> cache
    let isMap: Type -> bool = FSharpType.IsMap |> cache
    let isRecord: Type -> bool = FSharpType.IsRecord |> cache
    let isUnion: Type -> bool = FSharpType.IsUnion |> cache
    let isOption: Type -> bool = FSharpType.IsOption |> cache

    type FSharpValue
        with 
            static member OptionUnwrap (t: Type) (o: obj): obj option =
                let _, fields = FSharpValue.GetUnionFields(o, t)
                match fields.Length with
                | 1 -> Some fields.[0]
                | _ -> None

            static member OptionNone (t: Type): obj =
                let cases = getUnionCases t
                FSharpValue.MakeUnion(cases.[0], Array.empty)        

            static member OptionSome (t: Type) (value: obj): obj =
                let cases = getUnionCases t
                FSharpValue.MakeUnion(cases.[1], [| value |])

            static member KvpKey (value: obj): obj =
                let prop = value.GetType().GetProperty("Key")
                prop.GetValue(value, null)

            static member KvpValue (value: obj): obj =
                let prop = value.GetType().GetProperty("Value")
                prop.GetValue(value, null)

            static member MakeList (itemType: Type) (items: obj list) =
                let listType = getListType itemType
                let cons = getListConstructor listType
                let add item list = cons.Invoke (null, [| item; list |])
                let list = (getListEmpty listType).GetValue(null)
                list |> List.foldBack add items               

            static member MakeMap (mapType: Type) (items: (string*obj) list) =
                let cons = mapType.GetConstructors().[0]
                let keyType = FSharpType.GetMapKeyType mapType
                let valueType = FSharpType.GetMapValueType mapType
                let tupleType = FSharpType.MakeTupleType [|keyType; valueType|]
                let listItems = items |> List.map (fun item -> FSharpValue.MakeTuple([|fst item; snd item|], tupleType))
                let thelist = FSharpValue.MakeList tupleType listItems
                let theMap = cons.Invoke([|thelist|])
                theMap

/// [omit]
module Internals =
    open System
    open System.Text
    open System.Globalization
    open System.Collections
    open System.Collections.Generic
    open System.Linq
    open System.Collections.Concurrent
    open System.Reflection
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Reflection    
    open Vertigo.FSharp.Data

    open Reflection
    open Tracing

    let getFieldJsonFieldName (p: PropertyInfo) (a: JsonProperty) =
        match a.PropertyName with
        | null -> p.Name
        | propertyName -> a.PropertyName

    let getCaseJsonFieldName (c: UnionCaseInfo) (a: JsonProperty) =
        match a.Case with
        | null -> c.Name
        | Case -> a.Case

    let getDateTimeFormat (attr: JsonProperty): string =
        match attr.DateTimeFormat with
        | null -> "yyyy-MM-ddTHH:mm:ss.fffffffK"
        | format -> format

    let GetTypeJsonAttribute (t: Type) =
        let jsonProps = t.GetCustomAttributes(typeof<JsonProperty>, false)
        match jsonProps.Length with
        | 1 -> (jsonProps.[0]) :?> JsonProperty
        | _ -> JsonProperty.Default 

    let GetFieldJsonAttribute (p: PropertyInfo) =
        let jsonProps = p.GetCustomAttributes(typeof<JsonProperty>, false)
        match jsonProps.Length with
        | 1 -> (jsonProps.[0]) :?> JsonProperty
        | _ -> JsonProperty.Default 

    let GetCaseJsonAttribute (c: UnionCaseInfo) =
        let jsonProps = c.GetCustomAttributes typeof<JsonProperty>
        match jsonProps.Length with
        | 1 -> (jsonProps.[0]) :?> JsonProperty
        | _ -> JsonProperty.Default

    let Converter (t: Type): Converters.IConverter =
        let cons = t.GetConstructors().[0]
        let converter = cons.Invoke([||])
        converter :?> Converters.IConverter

    let getTypeJsonAttribute: Type -> JsonProperty = GetTypeJsonAttribute |> cache
    let getFieldJsonAttribute: PropertyInfo -> JsonProperty = GetFieldJsonAttribute |> cache
    let getCaseJsonAttribute: UnionCaseInfo -> JsonProperty = GetCaseJsonAttribute |> cache
    let getConverter: Type -> Converters.IConverter = Converter |> cache

    let jsonFieldName (prop: PropertyInfo) =
        let attr = getFieldJsonAttribute prop
        getFieldJsonFieldName prop attr

    type JsonField = string * JsonValue

    type Serialize = class end
        with
            static member Enum (config: JsonConfig) (t: Type) (value: obj) (attr: JsonProperty): JsonValue =
                match attr.EnumValue with
                | EnumValue.Number ->
                    let index = decimal (value :?> int)
                    JsonValue.Number index
                | EnumValue.String ->
                    let valueString = Enum.GetName(t, value)
                    JsonValue.String valueString

            static member ApplyConverter (t: Type) (value: obj) (attr: JsonProperty): (Type*obj) =
                match attr.Converter with
                | null -> (t, value)
                | converterType ->
                    let converter = getConverter converterType
                    let valueType = converter.JsonType ()
                    let value = converter.toJson value
                    (valueType, value)

            static member NonOption (config: JsonConfig) (t: Type) (value: obj) (attr: JsonProperty): JsonValue =
                match attr.Raw with
                | false ->
                    let t, value = Serialize.ApplyConverter t value attr
                    match t with
                    | t when t = typeof<string> -> JsonValue.String (value :?> string)
                    | t when t = typeof<decimal> -> JsonValue.Number (value :?> decimal)
                    | t when t = typeof<float> -> JsonValue.Float (value :?> float)
                    | t when t = typeof<int> -> JsonValue.Number (decimal (value :?> int))
                    | t when t = typeof<int64> -> JsonValue.Number (decimal (value :?> int64))
                    | t when t = typeof<bool> -> JsonValue.Boolean (value :?> bool)
                    | t when t = typeof<Guid> -> JsonValue.String ((value :?> Guid).ToString())
                    | t when t = typeof<DateTime> -> JsonValue.String ((value :?> DateTime).ToString(getDateTimeFormat attr))
                    | t when t = typeof<DateTimeOffset> -> JsonValue.String ((value :?> DateTimeOffset).ToString(getDateTimeFormat attr))
                    | t when t.IsEnum -> Serialize.Enum config t value attr
                    | t when isTuple t -> Serialize.Enumerable config (FSharpValue.GetTupleFields value)
                    | t when isList t -> Serialize.Enumerable config (value :?> IEnumerable)
                    | t when isArray t -> Serialize.Enumerable config (value :?> IEnumerable)
                    | t when isMap t -> Serialize.Map config (value :?> IEnumerable)
                    | t when isRecord t -> Serialize.Record config t value
                    | t when isUnion t -> Serialize.Union config t value
                    | _ -> failwith <| sprintf "Unknown type: Type=%s" t.Name
                | true ->
                    let raw = value :?> string
                    if raw.StartsWith "{" then
                        JsonValue.Parse raw
                    else
                        JsonValue.String raw
                        
            static member Option (config: JsonConfig) (t: Type) (o: obj) (attr: JsonProperty): JsonValue option =
                match t with
                |  t when isOption t ->
                    let unwrapedO = FSharpValue.OptionUnwrap t o
                    match unwrapedO with
                    | Some someO -> Some (Serialize.NonOption config (getOptionType t) someO attr)
                    | None -> 
                        match config.option with
                        | NullForNone -> Some JsonValue.Null
                        | FieldMissing -> None
                | _ -> Some (Serialize.NonOption config t o attr)

            static member Property (config: JsonConfig) (o: obj) (prop: PropertyInfo): JsonField option =
                let attr = getFieldJsonAttribute prop
                let propValue = prop.GetValue(o, Array.empty)
                let jsonValue = Serialize.Option config prop.PropertyType propValue attr
                match jsonValue with
                | Some jsonValue -> Some (getFieldJsonFieldName prop attr, jsonValue)
                | None -> None

            static member Enumerable (config: JsonConfig) (objs: IEnumerable): JsonValue =
                let items =
                    objs.Cast<Object>()
                    |> Seq.map (fun o -> Serialize.Option config (o.GetType()) o JsonProperty.Default)
                    |> Seq.map (fun jvalue ->
                        match jvalue with
                        | Some jvalue -> jvalue
                        | None -> JsonValue.Null)
                    |> Array.ofSeq
                JsonValue.Array items

            static member Map (config: JsonConfig) (kvps: IEnumerable): JsonValue =
                let properties =
                    kvps.Cast<Object>()
                    |> Seq.map (fun o ->
                        let key = FSharpValue.KvpKey o :?> string
                        let value = FSharpValue.KvpValue o
                        let jvalue = Serialize.Option config (value.GetType()) value JsonProperty.Default
                        match jvalue with
                        | Some jvalue -> (key, jvalue)
                        | None -> (key, JsonValue.Null)
                    )
                    |> Array.ofSeq
                JsonValue.Record properties

            static member Record (config: JsonConfig) (t: Type) (o: obj): JsonValue =
                let props: PropertyInfo array = getRecordFields(t)
                let fields = props |> Array.map (Serialize.Property config o) |> Array.choose id
                JsonValue.Record fields

            static member Union (config: JsonConfig) (t: Type) (o: obj): JsonValue =
                let caseInfo, values = FSharpValue.GetUnionFields(o, t)
                let attr = getCaseJsonAttribute caseInfo
                let typeAttr = getTypeJsonAttribute t
                let jValue =
                    match values.Length with
                    | 1 ->
                        let caseValue = values.[0]
                        Serialize.NonOption config (caseValue.GetType()) caseValue attr
                    | _ ->
                        Serialize.Enumerable config values
                let theCase = getCaseJsonFieldName caseInfo attr
                match typeAttr.CaseKey, typeAttr.CaseValue with
                | caseKey, caseValue when caseKey <> null && caseValue <> null ->
                    JsonValue.Record [| (caseKey, JsonValue.String theCase); (caseValue, jValue) |]
                | _ -> JsonValue.Record [| (theCase, jValue) |]


            static member Root (config: JsonConfig) (t: Type) (value: obj): JsonValue =
                match t with
                | t when isList t -> Serialize.Enumerable config (value :?> IEnumerable)
                | t when isArray t -> Serialize.Enumerable config (value :?> IEnumerable)
                | t when isMap t -> Serialize.Map config (value :?> IEnumerable)
                | t when isRecord t -> Serialize.Record config t value
                | t when isUnion t -> Serialize.Union config t value
                | t -> failwith (sprintf "Object must be one of following types: Record, Union, Map, Array, List. The type is: %s" (t.ToString()))
    
    let failDeserialization (trace: Trace) (message: string) =
        let message = trace |> traceToString |> sprintf "%s. Location: %s." message
        raise (new JsonDeserializationException(trace, message))
                
    type Deserialize = class end
        with
            static member Enum (trace: Trace) (t: Type) (attr: JsonProperty) (value: JsonValue): obj =
                match attr.EnumValue with
                | EnumValue.Number ->
                    let index = JsonConversions.AsInteger trace value
                    Enum.ToObject(t, index)
                | EnumValue.String ->
                    let valueStr = JsonConversions.AsString trace value
                    Enum.Parse(t, valueStr)

            static member JsonType (t: Type) (attr: JsonProperty): Type =
                match attr.Converter with
                | null -> t
                | converterType -> (getConverter converterType).JsonType ()

            static member ConvertJson (attr: JsonProperty) (value: obj): obj =
                match attr.Converter with
                | null -> value
                | converterType -> (getConverter converterType).fromJson value

            static member getValueType (trace: Trace) (value: JsonValue): Type =
                match value with
                | JsonValue.String _ -> typeof<string>
                | JsonValue.Number _ -> typeof<decimal>
                | JsonValue.Float _ -> typeof<float>
                | JsonValue.Boolean _ -> typeof<bool>
                | JsonValue.Array _ -> getListType typeof<obj>
                | JsonValue.Record _ -> typeof<Map<string, obj>>

            static member NonOption (config: JsonConfig) (trace: Trace) (t: Type) (attr: JsonProperty) (value: JsonValue): obj =
                match attr.Raw with
                | false ->
                    let t = Deserialize.JsonType t attr
                    let t =
                        match t with
                        | t when t = typeof<obj> -> Deserialize.getValueType trace value
                        | t -> t 
                    let jsonValue =
                        match t with
                        | t when t = typeof<string> -> JsonConversions.AsString trace value :> obj
                        | t when t = typeof<decimal> -> JsonConversions.AsDecimal trace value :> obj
                        | t when t = typeof<float> -> JsonConversions.AsFloat trace value :> obj
                        | t when t = typeof<int> -> JsonConversions.AsInteger trace value :> obj
                        | t when t = typeof<int64> -> JsonConversions.AsInteger64 trace value :> obj
                        | t when t = typeof<bool> -> JsonConversions.AsBoolean trace value :> obj
                        | t when t = typeof<DateTime> -> JsonConversions.AsDateTime CultureInfo.InvariantCulture trace value :> obj
                        | t when t = typeof<DateTimeOffset> -> JsonConversions.AsDateTimeOffset CultureInfo.InvariantCulture trace value :> obj
                        | t when t = typeof<Guid> -> JsonConversions.AsGuid trace value :> obj
                        | t when t.IsEnum -> Deserialize.Enum trace t attr value
                        | t when isTuple t -> Deserialize.Tuple config trace t value
                        | t when isList t -> Deserialize.List config trace t value
                        | t when isArray t -> Deserialize.Array config trace t value
                        | t when isMap t -> Deserialize.Map config trace t value
                        | t when isRecord t -> Deserialize.Record config trace t value
                        | t when isUnion t -> Deserialize.Union config trace t value
                        | _ -> t.Name |> sprintf "Unknown type %s" |> failDeserialization trace
                    Deserialize.ConvertJson attr jsonValue
                | true ->
                    match value with
                    | JsonValue.String value -> value :> obj
                    | _ -> value.ToString(JsonSaveOptions.DisableFormatting) :> obj
                    

            static member Option (config: JsonConfig) (trace: Trace) (t: Type) (attr: JsonProperty) (value: JsonValue option): obj =
                match t with
                | t when isOption t ->
                    match value with
                    | Some fieldValue ->
                        match fieldValue with
                        | JsonValue.Null ->
                            FSharpValue.OptionNone t
                        | _ ->
                            let value = Deserialize.NonOption config trace (getOptionType t) attr fieldValue
                            FSharpValue.OptionSome t value
                    | None ->
                        match config.option with
                        | NullForNone ->
                            failDeserialization trace "Value is missing for optional field and NullForNone policy is used"
                        | FieldMissing ->
                            FSharpValue.OptionNone t
                | _ ->
                    match value with
                    | Some fieldValue ->
                        Deserialize.NonOption config trace t attr fieldValue
                    | None ->
                        match attr.DefaultValue with
                        | null -> failDeserialization trace "Missing non optional"
                        | defaultValue -> defaultValue

            static member List (config: JsonConfig) (trace: Trace) (t: Type) (value: JsonValue): obj =
                let itemType = getListItemType t
                match value with
                | JsonValue.Array values ->
                    let arrayValues =
                        values |> Array.mapi (fun index value ->
                            let itemTrace = trace @ [Location.Item index]
                            Deserialize.Option config itemTrace itemType JsonProperty.Default (Some value)
                        )
                    arrayValues |> List.ofSeq |> FSharpValue.MakeList itemType
                | _ -> failDeserialization trace "Can't parse list from json"

            static member Map (config: JsonConfig) (trace: Trace) (t: Type) (value: JsonValue): obj =
                let itemType = getMapValueType t
                match value with
                | JsonValue.Record fields ->
                    fields 
                    |> Array.map (fun field ->
                        let itemName = fst field
                        let itemValue = snd field
                        let itemType =
                            match itemType with
                            | t when t = typeof<obj> -> itemType
                            | _ -> itemType
                        let itemTrace = trace @ [Location.Property itemName]
                        let itemValueObj = Deserialize.Option config itemTrace itemType JsonProperty.Default (Some itemValue)
                        (itemName, itemValueObj)
                    )
                    |> List.ofArray |> FSharpValue.MakeMap t
                | _ -> failDeserialization trace "Can't parse map from json"

            static member Array (config: JsonConfig) (trace: Trace) (t: Type) (value: JsonValue): obj =
                let itemType = t.GetElementType()
                match value with
                | JsonValue.Array values ->
                    let arrayValues =
                        values |> Array.mapi (fun index value ->
                            let itemTrace = trace @ [Location.Item index]
                            Deserialize.Option config itemTrace itemType JsonProperty.Default (Some value)
                        )
                    let arr = Array.CreateInstance(itemType, arrayValues.Length)
                    arrayValues |> Array.iteri (fun i v -> arr.SetValue(v, i))
                    arr :> obj
                | _ -> failDeserialization trace "Can't parse array from json"

            static member TupleItems (config: JsonConfig) (trace: Trace) (types: Type[]) (value: JsonValue): obj[] =
                match value with
                | JsonValue.Array values ->
                    if types.Length <> values.Length then
                        failDeserialization trace "Number of values does not match expected"
                    let tupleValues = (Array.zip types values) |> Array.mapi (fun index (t, value) ->
                        let itemTrace = trace @ [Location.Item index]
                        Deserialize.Option config itemTrace t JsonProperty.Default (Some value)
                    )
                    tupleValues
                | _ -> failDeserialization trace "Can't parse tuple from json"

            static member Tuple (config: JsonConfig) (trace: Trace) (t: Type) (value: JsonValue): obj =
                let types = getTupleElements t
                let tupleValues = Deserialize.TupleItems config trace types value
                FSharpValue.MakeTuple (tupleValues, t)

            static member Record (config: JsonConfig) (trace: Trace) (t: Type) (value: JsonValue): obj =
                match value with
                | JsonValue.Record fields ->
                    let props: PropertyInfo array = getRecordFields t
                    let propValues = 
                        props 
                        |> Array.map (fun prop ->
                            let attr = getFieldJsonAttribute prop
                            let name = getFieldJsonFieldName prop attr
                            let field = fields |> Seq.tryFind (fun f -> fst f = name)
                            let fieldValue = field |> Option.map snd
                            let propertyTrace = trace @ [Location.Property name]
                            Deserialize.Option config propertyTrace prop.PropertyType attr fieldValue
                        )
                    FSharpValue.MakeRecord(t, propValues)
                | _ -> failDeserialization trace "Can't parse record from json"

            static member Union (config: JsonConfig) (trace: Trace) (t: Type) (value: JsonValue): obj =
                let typeAttr = getTypeJsonAttribute t
                match value with
                | JsonValue.Record fields ->
                    let fieldName, fieldValue =
                        match typeAttr.CaseKey, typeAttr.CaseValue with
                        | caseKey, caseValue when caseKey <> null && caseValue <> null ->
                            if fields.Length <> 2 then
                                failDeserialization trace (sprintf "Can't parse union from record with %i fields" fields.Length)
                            let fieldName = fields |> Seq.tryFind (fun f -> fst f = caseKey)
                            let fieldName =
                                match fieldName with
                                | Some fieldName -> fieldName
                                | None -> failDeserialization trace (sprintf "Can't find union case for key: %s" caseKey)
                            let fieldValue = fields |> Seq.tryFind (fun f -> fst f = caseValue)
                            let fieldValue =
                                match fieldValue with
                                | Some fieldValue -> fieldValue
                                | None -> failDeserialization trace (sprintf "Can't find union case for value: %s" caseValue)
                            let caseNameTrace = trace @ [fieldName |> fst |> Location.Property]
                            let caseName = JsonConversions.AsString caseNameTrace (snd fieldName)
                            (caseName, snd fieldValue)
                        | _ ->
                            if fields.Length <> 1 then
                                failDeserialization trace (sprintf "Can't parse union from record with  %i fields" fields.Length)
                            let field = fields.[0]
                            field

                    let caseTrace = trace @ [Location.Property fieldName]
                    let caseInfo = t |> getUnionCases |> Array.tryFind (fun c -> getCaseJsonFieldName c (getCaseJsonAttribute c) = fieldName)
                    let caseInfo =
                        match caseInfo with
                        | Some caseInfo -> caseInfo
                        | None -> failDeserialization trace (sprintf "Unknown union case: %s" fieldName)
                    let attr = getCaseJsonAttribute caseInfo
                    let props: PropertyInfo array = caseInfo.GetFields()
                    let propsValues =
                        match props.Length with
                        | 1 ->
                            let propValue = Deserialize.Option config caseTrace props.[0].PropertyType attr (Some fieldValue)
                            [| propValue |]
                        | _ ->
                            let propsTypes = props |> Array.map (fun p -> p.PropertyType)
                            Deserialize.TupleItems config caseTrace propsTypes fieldValue
                    FSharpValue.MakeUnion (caseInfo, propsValues)
                | _ -> failDeserialization trace "Can't parse union from json"

            static member Root (config: JsonConfig) (t: Type) (value: JsonValue): obj =
                match t with
                | t when isList t -> Deserialize.List config [] t value
                | t when isArray t -> Deserialize.Array config [] t value
                | t when isMap t -> Deserialize.Map config [] t value
                | t when isRecord t -> Deserialize.Record config [] t value
                | t when isUnion t -> Deserialize.Union config [] t value
                | _ -> failDeserialization [] "Json should be either object or array"          