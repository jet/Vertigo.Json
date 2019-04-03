// --------------------------------------------------------------------------------------
// Builds the documentation from `.fsx` and `.md` files in the 'docsrc/content' directory
// (the generated documentation is stored in the 'docsrc/output' directory)
// --------------------------------------------------------------------------------------

#load "./.fake/generate.fsx/intellisense.fsx"

// Binaries that have XML documentation (in a corresponding generated XML file)
// Any binary output / copied to bin/projectName/projectName.dll will
// automatically be added as a binary to generate API docs for.
// for binaries output to root bin folder please add the filename only to the 
// referenceBinaries list below in order to generate documentation for the binaries.
// (This is the original behaviour of ProjectScaffold prior to multi project support)
let referenceBinaries = []
// Web site location for the generated documentation
let website = "/Vertigo.Json"

let githubLink = "https://github.com/jet/Vertigo.Json"

// Specify more information about your project
let info =
  [ "project-name", "Vertigo.Json"
    "project-author", "Jet.com"
    "project-summary", "F# JSON (de)serialization library"
    "project-github", githubLink
    "project-nuget", "http://nuget.org/packages/Vertigo.Json" ]

// --------------------------------------------------------------------------------------
// For typical project, no changes are needed below
// --------------------------------------------------------------------------------------

#I "../.././packages/netcorebuild/Fake.IO.FileSystem/lib/netstandard2.0/"
#I "../.././packages/netcorebuild/Fake.Core.Target/lib/netstandard2.0/"
#I "../.././packages/netcorebuild/Fake.Core.Trace/lib/netstandard2.0/"

#r @"../../packages/netcorebuild/Fake.Core.FakeVar/lib/netstandard2.0/Fake.Core.FakeVar.dll"
#r @"../../packages/netcorebuild/Fake.Core.Environment/lib/netstandard2.0/Fake.Core.Environment.dll"
#r @"../../packages/netcorebuild/Fake.Core.Context/lib/netstandard2.0/Fake.Core.Context.dll"

#r "Fake.IO.FileSystem.dll"
#r "Fake.Core.Trace.dll"
#r "Fake.Core.Target.dll"

#I "../.././packages/netcorebuild/FSharp.Formatting/lib/net40/"
//#r "FSharp.Compiler.Service.dll"
#r "FSharp.Literate.dll"
#r "FSharp.MetadataFormat.dll"
#r "RazorEngine.dll"
//#r "FSharp.CodeFormat.dll"
#r "FSharp.Markdown.dll"

open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core
open System.IO
open FSharp.Literate
open FSharp.MetadataFormat
open Fake.IO.FileSystemOperators

// When called from 'build.fsx', use the public project URL as <root>
// otherwise, use the current 'output' directory.
#if RELEASE
let root = website
#else
let root = "file://" + (__SOURCE_DIRECTORY__ @@ "../../docs")
#endif

// Paths with template/source/output locations
let bin        = __SOURCE_DIRECTORY__ @@ "../../bin"
let content    = __SOURCE_DIRECTORY__ @@ "../content"
let output     = __SOURCE_DIRECTORY__ @@ "../../docs"
let files      = __SOURCE_DIRECTORY__ @@ "../files"
let templates  = __SOURCE_DIRECTORY__ @@ "templates"
let formatting = __SOURCE_DIRECTORY__ @@ "../.././packages/netcorebuild/FSharp.Formatting/"
let docTemplate = "docpage.cshtml"

// Where to look for *.csproj templates (in this order)
let layoutRootsAll = new System.Collections.Generic.Dictionary<string, string list>()
layoutRootsAll.Add("en",[ templates; formatting @@ "templates"
                          formatting @@ "templates/reference" ])
DirectoryInfo.getSubDirectories (DirectoryInfo.ofPath templates)
|> Seq.iter (fun d ->
                let name = d.Name
                if name.Length = 2 || name.Length = 3 then
                    layoutRootsAll.Add(
                            name, [templates @@ name
                                   formatting @@ "templates"
                                   formatting @@ "templates/reference" ]))

// Copy static files and CSS + JS from F# Formatting
let copyFiles () =
  Shell.copyRecursive files output true |> Trace.logItems "Copying file: "
  Directory.ensure (output @@ "content")
  Shell.copyRecursive (formatting @@ "styles") (output @@ "content") true 
    |> Trace.logItems "Copying styles and scripts: "

let binaries =
    let manuallyAdded = 
        referenceBinaries 
        |> List.map (fun b -> bin @@ b)
    
    let conventionBased = 
        DirectoryInfo.ofPath bin 
        |> DirectoryInfo.getSubDirectories
        |> Array.map (fun d -> d.FullName @@ (sprintf "%s.dll" d.Name))
        |> List.ofArray

    conventionBased @ manuallyAdded

let libDirs =
    let conventionBasedbinDirs =
        DirectoryInfo.ofPath bin 
        |> DirectoryInfo.getSubDirectories
        |> Array.map (fun d -> d.FullName)
        |> List.ofArray

    conventionBasedbinDirs @ [bin]

// Build API reference from XML comments
let buildReference () =
  Shell.cleanDir (output @@ "reference")
  MetadataFormat.Generate
    ( binaries, output @@ "reference", layoutRootsAll.["en"],
      parameters = ("root", root)::info,
      sourceRepo = githubLink @@ "tree/master",
      sourceFolder = __SOURCE_DIRECTORY__ @@ ".." @@ "..",
      publicOnly = true,libDirs = libDirs )

// Build documentation from `fsx` and `md` files in `docs/content`
let buildDocumentation () =

  // First, process files which are placed in the content root directory.

  Literate.ProcessDirectory
    ( content, docTemplate, output, replacements = ("root", root)::info,
      layoutRoots = layoutRootsAll.["en"],
      generateAnchors = true,
      processRecursive = false)

  // And then process files which are placed in the sub directories
  // (some sub directories might be for specific language).

  let subdirs = Directory.EnumerateDirectories(content, "*", SearchOption.TopDirectoryOnly)
  for dir in subdirs do
    let dirname = (new DirectoryInfo(dir)).Name
    let layoutRoots =
        // Check whether this directory name is for specific language
        let key = layoutRootsAll.Keys
                  |> Seq.tryFind (fun i -> i = dirname)
        match key with
        | Some lang -> layoutRootsAll.[lang]
        | None -> layoutRootsAll.["en"] // "en" is the default language

    Literate.ProcessDirectory
      ( dir, docTemplate, output @@ dirname, replacements = ("root", root)::info,
        layoutRoots = layoutRoots,
        generateAnchors = true )

// Generate
copyFiles()
#if HELP
buildDocumentation()
#endif
#if REFERENCE
buildReference()
#endif
