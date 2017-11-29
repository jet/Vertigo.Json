﻿// --------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation 2005-2012.
// This sample code is provided "as is" without warranty of any kind.
// We disclaim all warranties, either express or implied, including the
// warranties of merchantability and fitness for a particular purpose.
//
// A simple F# portable parser for JSON data
// --------------------------------------------------------------------------------------

namespace Vertigo.FSharp.Data

open System
open System.IO
open System.ComponentModel
open System.Text
open System.Globalization
open Vertigo.FSharp.Data.Runtime

/// Specifies the formatting behaviour of JSON values
[<RequireQualifiedAccess>]
type internal JsonSaveOptions =
  /// Format (indent) the JsonValue
  | None = 0
  /// Print the JsonValue in one line in a compact way
  | DisableFormatting = 1

/// Represents a JSON value. Large numbers that do not fit in the
/// Decimal type are represented using the Float case, while
/// smaller numbers are represented as decimals to avoid precision loss.
[<RequireQualifiedAccess>]
[<StructuredFormatDisplay("{_Print}")>]
type internal JsonValue =
  | String of string
  | Number of decimal
  | Float of float
  | Record of properties:(string * JsonValue)[]
  | Array of elements:JsonValue[]
  | Boolean of bool
  | Null  

  [<EditorBrowsableAttribute(EditorBrowsableState.Never)>]
  [<CompilerMessageAttribute("This method is intended for use in generated code only.", 10001, IsHidden=true, IsError=false)>]
  member x._Print = x.ToString()

  /// Serializes the JsonValue to the specified System.IO.TextWriter.
  member x.WriteTo (w:TextWriter, saveOptions) =

    let newLine =
      if saveOptions = JsonSaveOptions.None then
        fun indentation plus ->
          w.WriteLine()
          System.String(' ', indentation + plus) |> w.Write
      else
        fun _ _ -> ()

    let propSep =
      if saveOptions = JsonSaveOptions.None then "\": "
      else "\":"

    let rec serialize indentation = function
      | Null -> w.Write "null"
      | Boolean b -> w.Write(if b then "true" else "false")
      | Number number -> w.Write number
      | Float number -> w.Write number
      | String s ->
          w.Write "\""
          JsonValue.JsonStringEncodeTo w s
          w.Write "\""
      | Record properties ->
          w.Write "{"                      
          for i = 0 to properties.Length - 1 do
            let k,v = properties.[i]
            if i > 0 then w.Write ","
            newLine indentation 2            
            w.Write "\""
            JsonValue.JsonStringEncodeTo w k
            w.Write propSep
            serialize (indentation + 2) v
          newLine indentation 0
          w.Write "}"
      | Array elements ->
          w.Write "["
          for i = 0 to elements.Length - 1 do
            if i > 0 then w.Write ","
            newLine indentation 2
            serialize (indentation + 2) elements.[i]
          if elements.Length > 0 then
            newLine indentation 0
          w.Write "]"
  
    serialize 0 x 

  // Encode characters that are not valid in JS string. The implementation is based
  // on https://github.com/mono/mono/blob/master/mcs/class/System.Web/System.Web/HttpUtility.cs
  static member internal JsonStringEncodeTo (w:TextWriter) (value:string) =
    if String.IsNullOrEmpty value then ()
    else 
      for i = 0 to value.Length - 1 do
        let c = value.[i]
        let ci = int c
        if ci >= 0 && ci <= 7 || ci = 11 || ci >= 14 && ci <= 31 then
          w.Write("\\u{0:x4}", ci) |> ignore
        else 
          match c with
          | '\b' -> w.Write "\\b"
          | '\t' -> w.Write "\\t"
          | '\n' -> w.Write "\\n"
          | '\f' -> w.Write "\\f"
          | '\r' -> w.Write "\\r"
          | '"'  -> w.Write "\\\""
          | '\\' -> w.Write "\\\\"
          | _    -> w.Write c

  member x.ToString saveOptions =
    let w = new StringWriter(CultureInfo.InvariantCulture)
    x.WriteTo(w, saveOptions)
    w.GetStringBuilder().ToString()

  override x.ToString() = x.ToString(JsonSaveOptions.None)
  
// --------------------------------------------------------------------------------------
// JSON parser
// --------------------------------------------------------------------------------------

type private JsonParser(jsonText:string, cultureInfo, tolerateErrors) =

    let cultureInfo = defaultArg cultureInfo CultureInfo.InvariantCulture

    let mutable i = 0
    let s = jsonText

    // Helper functions
    let skipWhitespace() =
      while i < s.Length && Char.IsWhiteSpace s.[i] do
        i <- i + 1
    let decimalSeparator = cultureInfo.NumberFormat.NumberDecimalSeparator.[0]
    let isNumChar c =
      Char.IsDigit c || c=decimalSeparator || c='e' || c='E' || c='+' || c='-'
    let throw() =
      let msg =
        sprintf
          "Invalid JSON starting at character %d, snippet = \n----\n%s\n-----\njson = \n------\n%s\n-------" 
          i (jsonText.[(max 0 (i-10))..(min (jsonText.Length-1) (i+10))]) (if jsonText.Length > 1000 then jsonText.Substring(0, 1000) else jsonText)
      failwith msg
    let ensure cond =
      if not cond then throw()

    let buf = new StringBuilder() // pre-allocate string buffer

    // Recursive descent parser for JSON that uses global mutable index
    let rec parseValue() =
        skipWhitespace()
        ensure(i < s.Length)
        match s.[i] with
        | '"' -> JsonValue.String(parseString())
        | '-' -> parseNum()
        | c when Char.IsDigit(c) -> parseNum()
        | '{' -> parseObject()
        | '[' -> parseArray()
        | 't' -> parseLiteral("true", JsonValue.Boolean true)
        | 'f' -> parseLiteral("false", JsonValue.Boolean false)
        | 'n' -> parseLiteral("null", JsonValue.Null)
        | _ -> throw()

    and parseRootValue() =
        skipWhitespace()
        if i = s.Length 
        then
            JsonValue.Null
        else
            ensure(i < s.Length)
            match s.[i] with
            | '{' -> parseObject()
            | '[' -> parseArray()
            | _ -> throw()

    and parseString() =
        ensure(i < s.Length && s.[i] = '"')
        i <- i + 1
        while i < s.Length && s.[i] <> '"' do
            if s.[i] = '\\' then
                ensure(i+1 < s.Length)
                match s.[i+1] with
                | 'b' -> buf.Append('\b') |> ignore
                | 'f' -> buf.Append('\f') |> ignore
                | 'n' -> buf.Append('\n') |> ignore
                | 't' -> buf.Append('\t') |> ignore
                | 'r' -> buf.Append('\r') |> ignore
                | '\\' -> buf.Append('\\') |> ignore
                | '/' -> buf.Append('/') |> ignore
                | '"' -> buf.Append('"') |> ignore
                | 'u' ->
                    ensure(i+5 < s.Length)
                    let hexdigit d =
                        if d >= '0' && d <= '9' then int32 d - int32 '0'
                        elif d >= 'a' && d <= 'f' then int32 d - int32 'a' + 10
                        elif d >= 'A' && d <= 'F' then int32 d - int32 'A' + 10
                        else failwith "hexdigit"
                    let unicodeChar (s:string) =
                        if s.Length <> 4 then failwith "unicodeChar";
                        char (hexdigit s.[0] * 4096 + hexdigit s.[1] * 256 + hexdigit s.[2] * 16 + hexdigit s.[3])
                    let ch = unicodeChar (s.Substring(i+2, 4)) // TODO: range
                    buf.Append(ch) |> ignore
                    i <- i + 4  // the \ and u will also be skipped past further below
                | 'U' ->
                    ensure(i+9 < s.Length)
                    let unicodeChar (s:string) =
                        if s.Length <> 8 then failwith "unicodeChar";
                        if s.[0..1] <> "00" then failwith "unicodeChar";     // only code points U+010000 to U+10FFFF supported
                                                                             // for coversion to UTF16 surrogate pairs
                        // used http://en.wikipedia.org/wiki/UTF-16#Code_points_U.2B010000_to_U.2B10FFFF as a guide below
                        let codePoint = System.UInt32.Parse(s, NumberStyles.HexNumber) - 0x010000u
                        let HIGH_TEN_BIT_MASK = 0xFFC00u                     // 1111|1111|1100|0000|0000
                        let LOW_TEN_BIT_MASK = 0x003FFu                      // 0000|0000|0011|1111|1111
                        let leadSurrogate = (codePoint &&& HIGH_TEN_BIT_MASK >>> 10) + 0xD800u
                        let trailSurrogate = (codePoint &&& LOW_TEN_BIT_MASK) + 0xDC00u                        
                        char leadSurrogate, char trailSurrogate

                    let lead, trail = unicodeChar (s.Substring(i+2, 8))
                    buf.Append(lead) |> ignore
                    buf.Append(trail) |> ignore
                    i <- i + 8  // the \ and u will also be skipped past further below
                | _ -> throw()
                i <- i + 2  // skip past \ and next char
            else
                buf.Append(s.[i]) |> ignore
                i <- i + 1
        ensure(i < s.Length && s.[i] = '"')
        i <- i + 1
        let str = buf.ToString()
        buf.Clear() |> ignore
        str

    and parseNum() =
        let start = i
        while i < s.Length && isNumChar(s.[i]) do
            i <- i + 1
        let len = i - start
        let sub = s.Substring(start,len)
        match TextConversions.AsDecimal cultureInfo sub with
        | Some x -> JsonValue.Number x
        | _ ->
            match TextConversions.AsFloat [| |] (*useNoneForMissingValues*)false cultureInfo sub with
            | Some x -> JsonValue.Float x
            | _ -> throw()

//    and parseNum() =
//        let start = i
//        while i < s.Length && isNumChar(s.[i]) do
//            i <- i + 1
//        let len = i - start
//        let sub = s.Substring(start,len)
//        let mutable x = 0L
//        if Int64.TryParse(sub, NumberStyles.Integer, cultureInfo, &x) then
//            JsonValue.Number (decimal x)
//        else
//        let mutable x = 0m
//        if Decimal.TryParse(sub, NumberStyles.Number ||| NumberStyles.AllowCurrencySymbol, cultureInfo, &x) then
//            JsonValue.Number x
//        else
//            let mutable x = 0.0
//            if Double.TryParse(sub, NumberStyles.Float, cultureInfo, &x) then
//                JsonValue.Float x
//            else
//                throw()

    and parsePair() =
        let key = parseString()
        skipWhitespace()
        ensure(i < s.Length && s.[i] = ':')
        i <- i + 1
        skipWhitespace()
        key, parseValue()

    and parseEllipsis() =
        let mutable openingBrace = false
        if i < s.Length && s.[i] = '{' then
            openingBrace <- true
            i <- i + 1
            skipWhitespace()
        while i < s.Length && s.[i] = '.' do
            i <- i + 1
            skipWhitespace()
        if openingBrace && i < s.Length && s.[i] = '}' then
            i <- i + 1
            skipWhitespace()

    and parseObject() =
        ensure(i < s.Length && s.[i] = '{')
        i <- i + 1
        skipWhitespace()
        let pairs = ResizeArray<_>()
        if i < s.Length && s.[i] = '"' then
            pairs.Add(parsePair())
            skipWhitespace()
            while i < s.Length && s.[i] = ',' do
                i <- i + 1
                skipWhitespace()
                if tolerateErrors && s.[i] = '}' then
                    () // tolerate a trailing comma, even though is not valid json
                else
                    pairs.Add(parsePair())
                    skipWhitespace()
        if tolerateErrors && i < s.Length && s.[i] <> '}' then
            parseEllipsis() // tolerate ... or {...}
        ensure(i < s.Length && s.[i] = '}')
        i <- i + 1
        JsonValue.Record(pairs.ToArray())

    and parseArray() =
        ensure(i < s.Length && s.[i] = '[')
        i <- i + 1
        skipWhitespace()
        let vals = ResizeArray<_>()
        if i < s.Length && s.[i] <> ']' then
            vals.Add(parseValue())
            skipWhitespace()
            while i < s.Length && s.[i] = ',' do
                i <- i + 1
                skipWhitespace()
                vals.Add(parseValue())
                skipWhitespace()
        if tolerateErrors && i < s.Length && s.[i] <> ']' then
            parseEllipsis() // tolerate ... or {...}
        ensure(i < s.Length && s.[i] = ']')
        i <- i + 1
        JsonValue.Array(vals.ToArray())

    and parseLiteral(expected, r) =
        ensure(i+expected.Length < s.Length)
        for j in 0 .. expected.Length - 1 do
            ensure(s.[i+j] = expected.[j])
        i <- i + expected.Length
        r

    // Start by parsing the top-level value
    member x.Parse() =
        let value = parseRootValue()
        skipWhitespace()
        if i <> s.Length then
            throw()
        value

    member x.ParseMultiple() =
        seq {
            while i <> s.Length do
                yield parseRootValue()
                skipWhitespace()
        }

type JsonValue with

    /// Parses the specified JSON string
    static member Parse(text, ?cultureInfo) =
        JsonParser(text, cultureInfo, false).Parse()
