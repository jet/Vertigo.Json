# Vertigo.Json

**Vertigo.Json** is F# JSON serialization library based on Reflection.


Here's example of Vertigo.Json usage:

```fsharp
open Vertigo.Json

// Your F# type
type YourType = {
	member1: string
	member2: int
}

let data: YourType = { member1 = "some value"; member2 = 42}

// serialize data into JSON
let json = Json.serialize data

// deserialize from JSON to F# type
let data = Json.deserialize<YourType> json
```
## Installing

Vertigo is available on [Nuget](https://www.nuget.org/packages/Vertigo.Json/), and requires no dependencies outside of NUnit. 

While Vertigo is targeted to .NET Core, it is tested against the official .NET release and Mono to ensure compatibility. 


## Building
To build Vertigo, you will need [.NET Core 2.0](https://www.microsoft.com/net/download/) or later installed.

After that, to build you can enter the following into the command line: 

```
> dotnet restore
> dotnet build
```

## Testing

Vertigo uses the NUnit library for unit testing.  To run unit tests, execute the following in the command line: 

```
> dotnet test
```

## Maintainer(s)

- [@tombert](https://github.com/tombert)
