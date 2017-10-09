# Vertigo.Json

**Vertigo.Json** is F# JSON serialization library based on Reflection.

See [the home page](http://jet.github.io/Vertigo.Json) for details.

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

## Maintainer(s)

- [@vsapronov](https://github.com/vsapronov)
