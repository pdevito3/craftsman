# Craftsman

Craftsman is the workhorse behind the [Wrapt](https://wrapt.dev) framework and provides a suite of CLI commands for quickly scaffolding out new files and projects for your .NET Web APIs with simple CLI commands and configuration files.

<p>
    <a href="https://github.com/pdevito3/craftsman/releases"><img src="https://img.shields.io/nuget/v/craftsman.svg" alt="Latest Release"></a>   
    <a href="https://github.com/pdevito3/craftsman/blob/master/LICENSE.txt"><img src ="https://img.shields.io/github/license/mashape/apistatus.svg?maxAge=2592000" alt="License"></a>
  <a href="https://discord.gg/TBq2rVkSEj" target="\_parent">
    <img alt="" src="https://img.shields.io/badge/Discord-Wrapt-%235865F2" />
  </a>
</p>


------

## Documentation

For all the documentation on how to use Craftsman, visit [wrapt.dev](https://wrapt.dev).

## Upcoming Features in v0.17

The last few releases have brought a ton of new features and while the backend is progressing nicely, there are still a few features I want to add before getting to a stable v1. 

I'm planning on doing some more heavy front end work for this upcoming release and then I'll probably do a heavy dog food project to find practical gaps. I have some of the upcoming items below for the next release and this is certainly not an exhaustive list of everything, but I want to be open about what's on the horizon. 🌅

Have a request for something you don't see below? Join [our discord](https://discord.gg/TBq2rVkSEj) and let's talk about it!

- ✅ Users and Roles managed in each boundary (AuthN still separate)

- ✅ New `Email` Value Object

- ✅ Functional Tests use Docker DB and has other cleanup items

- ✅ NextJS template (still a WIP, but you can find [the sandbox for things here](https://github.com/pdevito3/next-template-wrapt-sand))

  - ✅ OIDC Auth support
  - ✅ List View
  - ✅ Add entity form
  - ✅ Edit entity form
  - ✅ Delete entity
  - ✅ Custom form components with [Mantine](https://mantine.dev) and [TailwindCSS](https://tailwindcss.com/)
  - ✅ Light/Dark Mode
  - ✅ Responsive
  - ✅ Entity Scaffolding with Craftsman

  - ✅ Permissions Integration
  
  - lots more...

🚧 Json Schema or C# classes for easier file scaffolding



### Some v0.16 highlights

✅ Testing Optimizations

✅ Common Value Object Scaffolding

✅ Auth Server rewrite with Keycloak

✅ Move permission guards to feature

✅ Migrate `Automapper` to `Mapster`



### Some v0.15 highlights

✅ Updated CLI command structure

✅ OpenTelemetry & Jaeger Tracing

✅ Built in Domain Event support (with unit test scaffolding)

✅ Moved to `Program.cs` only format

✅ Added repository & unit of work abstractions for better testing and validation

✅ SmartEnum property scaffolding support



### Some v0.14 highlights

✅ Duende BFF scaffolding

✅ React scaffolding

✅ Dockerfile and Docker Compose scaffolding



### Some v0.13 highlights

✅ DDD promoted entities (private setters, factory methods, associated fakers)

✅ Huge permissions upgrade. Significantly simplified setup which resulted in a new library ([HeimGuard](https://github.com/pdevito3/heimguard)) that can be used in any .NET project.

✅ New `register:producer` command

✅ Added soft delete capability

✅ Added Shared Kernel



### Some v0.12 highlights

✅ .NET 6 Scaffolding

✅ Docker utility updates for integration tests using [Fluent Docker](https://github.com/mariotoffia/FluentDocker) 🐳

✅  `add:feature` enhancement to add more than just ad-hoc features

✅ `new:example` command to generate example projects with associated templates

✅ Auth Server Scaffolding (In-Memory)

✅ Auditable entities



## Support

If Wrapt and Craftsman are saving you time and helping your projects, consider [sponsoring me on Github](https://github.com/sponsors/pdevito3) to support ongoing Wrapt development and make it even better!

## Contributing

Time is of the essence. Before developing a Pull Request I recommend opening a new [topic for discussion](https://github.com/pdevito3/craftsman/discussions). I also haven't had enough PR interest to take the time and put together a `contributing.md`, but if you are interested, I will definitely put together a detailed writeup.

## Contact Me

Sometimes Github notifications get lost in the shuffle. If you file an issue and don't hear from me in 24-48 hours feel free to ping me on [twitter](https://twitter.com/pdevito3) or Discord (pdevito3#4244). We also have [own discord channel](https://discord.gg/TBq2rVkSEj) now for easy contact with me and larger community discussions!
