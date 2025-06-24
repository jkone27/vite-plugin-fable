---
index: 1
categoryindex: 1
category: docs
---

# vite-plugin-fable

<style>img { max-width: min(100%, 400px); display: block; margin-inline: auto; }</style>

![vite-plugin-fable logo](./img/logo.png)

> This project is up for adoption. I'm looking for eager people to maintain this.<br>
> Please open a [discussion](https://github.com/fable-compiler/vite-plugin-fable/discussions) if you are interested!

## Introduction

When diving into Vite, I found myself having a friendly debate with what the [get started with Vite](https://fable.io/docs/getting-started/javascript.html#browser) guide suggests.  
It's purely a matter of taste, and I mean no disrespect to the authors.

If you peek at the latest Fable docs, you'll notice this snippet at the end:

    dotnet fable watch --run npx vite

Now, that's where my preferences raise an eyebrow.
For nearly everything else in Vite, whether it's JSX, TypeScript, Sass, or Markdown, I find myself typing `npm run dev`. (<small>or even `bun run dev`, cheers to [Bun](https://twitter.com/i/status/1701702174810747346) for that!</small>)  
You know, the command that summons `vite`, the proclaimed [Next Generation Frontend Tooling](https://vitejs.dev/).

I'm of the opinion (_brace yourselves, hot take incoming_) that integrating Fable with frontend development should align as closely as possible with the broader ecosystem. Vite is a star in the frontend universe, with a user base dwarfing that of F# developers. It makes sense to harmonize with their flow, not the other way around.

    bun run dev

I absolutely recognize and respect the legacy of Fable. It's a veteran in the scene, predating Vite, so I get the historical reasons for its current approach.  
But that doesn't mean I can't cheer for evolution and a bit of change, right?

## Community Templates 🚧 

### Feliz

Some [Feliz](https://fable-hub.github.io/Feliz/) templates are already using this plugin, ✅

* [feliz-vite (react)](https://github.com/jkone27/feliz-vite) 🪐: a template that combines Feliz react DSL and this awesome plugin as well as vitest and react testing library fable bindings, for extra awesme experience.

* [vite-feliz-solid](https://github.com/jkone27/feliz-vite-solid) 🌐: a template that combines Feliz JSX react dsl with solidjs, vitest, jsdom and vite.

### Express.js (WIP) ⚠️
* [vite-fable-server](https://github.com/jkone27/vite-fable-server) 🍇: a simple example combining the plugin with express.js (and glutinum bindings) as well as vite plugin server, for a simple backend application. ⚠️ tests not working atm. 

**NOTE**: Want to add to this list? please open an issue on our github repo, we are glad to add more examples for users.

(*) please note we do **not support such templates**. At the time of wrigint, they are just a way to showcase this plugin functionality or to help document the plugin usage.

[Next]({{fsdocs-next-page-link}})
