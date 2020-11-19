NOTICE: SUPPORT FOR THIS PROJECT ENDED ON 18 November 2020

This projected was owned and maintained by Jet.com (Walmart). This project has reached its end of life and Walmart no longer supports this project.

We will no longer be monitoring the issues for this project or reviewing pull requests. You are free to continue using this project under the license terms or forks of this project at your own risk. This project is no longer subject to Jet.com/Walmart's bug bounty program or other security monitoring.


## Actions you can take

We recommend you take the following action:

  * Review any configuration files used for build automation and make appropriate updates to remove or replace this project
  * Notify other members of your team and/or organization of this change
  * Notify your security team to help you evaluate alternative options

## Forking and transition of ownership

For [security reasons](https://www.theregister.co.uk/2018/11/26/npm_repo_bitcoin_stealer/), Walmart does not transfer the ownership of our primary repos on Github or other platforms to other individuals/organizations. Further, we do not transfer ownership of packages for public package management systems.

If you would like to fork this package and continue development, you should choose a new name for the project and create your own packages, build automation, etc.

Please review the licensing terms of this project, which continue to be in effect even after decommission.

ORIGINAL README BELOW

----------------------

# Vertigo.Json

**Vertigo.Json** is F# JSON serialization library based on Reflection.

Here's an example of Vertigo.Json usage:

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

- @precisemath
