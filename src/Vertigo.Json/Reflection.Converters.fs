namespace Vertigo.Json

module Converters =
    open System
    open Vertigo.FSharp.Data

    type IConverter =
        abstract member JsonType: unit -> Type
        abstract member toJson: obj -> obj
        abstract member fromJson: obj -> obj

    type StringToInt() =
        interface IConverter with
            member x.JsonType () = (fun u -> typeof<string>) ()
            member x.toJson value = (fun (v: obj) -> (v :?> int).ToString() :> obj) value
            member x.fromJson value = (fun (v: obj) -> (int (v :?> string)) :> obj) value

    type StringToInt64() =
        interface IConverter with
            member x.JsonType () = (fun u -> typeof<string>) ()
            member x.toJson value = (fun (v: obj) -> (v :?> int64).ToString() :> obj) value
            member x.fromJson value = (fun (v: obj) -> (int64 (v :?> string)) :> obj) value

    type StringToFloat() =
        interface IConverter with
            member x.JsonType () = (fun u -> typeof<string>) ()
            member x.toJson value = (fun (v: obj) -> (v :?> float).ToString() :> obj) value
            member x.fromJson value = (fun (v: obj) -> (float (v :?> string)) :> obj) value

    type StringToDecimal() =
        interface IConverter with
            member x.JsonType () = (fun u -> typeof<string>) ()
            member x.toJson value = (fun (v: obj) -> (v :?> decimal).ToString() :> obj) value
            member x.fromJson value = (fun (v: obj) -> (decimal (v :?> string)) :> obj) value

    type DateTimeToEpoch() =
        interface IConverter with
            member x.JsonType () = (fun u -> typeof<int64>) ()
            member x.toJson value = (fun (v: obj) -> int64(((v :?> DateTime) - DateTime(1970, 1, 1)).TotalSeconds) :> obj) value
            member x.fromJson value = (fun (v: obj) -> DateTime(1970, 1, 1).Add(TimeSpan.FromSeconds(float(v :?> int64))) :> obj) value

    type DateTimeOffsetToEpoch() =
        interface IConverter with
            member x.JsonType () = (fun u -> typeof<int64>) ()
            member x.toJson value = (fun (v: obj) -> int64(((v :?> DateTimeOffset) - DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan(0L))).TotalSeconds) :> obj) value
            member x.fromJson value = (fun (v: obj) -> DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan(0L)).Add(TimeSpan.FromSeconds(float(v :?> int64))) :> obj) value
