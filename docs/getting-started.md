---
index: 2
categoryindex: 1
category: docs
---

# Getting started

## .NET SDK (required)

Maybe it can seem daunting if you are a frontend developer with no prior .NET experience (trust us is not 😉), but to run [F#](https://dotnet.microsoft.com/en-us/languages/fsharp) and run [Fable](https://fable.io/) you need to have `dotnet-sdk` installed in your local machine.
For mac-os users with homebrew is as simple as `brew install dotnet-sdk`. We invite you to install the sdk according to your preferences, and following the proper instructions for your current architecture on the [microsoft website for .NET sdk](https://dotnet.microsoft.com/en-us/download) if is not a mac-os.

## Create a new Vite project

First you need a new Vite project:

<vpf-command npm="npm create vite@latest" bun="bun create vite"></vpf-command>

## Add the plugin

Next, you need to install the `vite-plugin-fable` package:

<vpf-command npm="npm install -D vite-plugin-fable" bun="bun install -D --trust vite-plugin-fable"></vpf-command>

It is important that the _post-install script_ of the plugin did run. The first time this runs, it can take some time.

If for some reason it didn't run, please manually invoke:

<vpf-command npm="npm --prefix ./node_modules/vite-plugin-fable/ run postinstall" bun="bun run --cwd ./node_modules/vite-plugin-fable/ postinstall"></vpf-command>

_Note: you don't need to install Fable as a dotnet tool when using this plugin._

## Update your Vite configuration

Lastly, we need to tell Vite to use our plugin:

```js
import { defineConfig } from "vite";
import fable from "vite-plugin-fable";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [fable()],
});
```

⚠️ Depending on your use-case, you may need to further tweak your configuration.  
Check out the [recipes](./recipes.md) page for more tips.

## Start Vite

We can now start the Vite dev server:

<vpf-command npm="npm run dev" bun="bunx --bun vite"></vpf-command>

You should see a bunch of logs:

```
12:32:42 PM [vite] [fable]: configResolved: Configuration: Debug
12:32:42 PM [vite] [fable]: configResolved: Entry fsproj /home/projects/your-project-folder/App.fsproj
12:32:42 PM [vite] [fable]: buildStart: Starting daemon
12:32:42 PM [vite] [fable]: buildStart: Initial project file change!
12:32:42 PM [vite] [fable]: projectChanged: dependent file /home/projects/your-project-folder/App.fsproj changed.
12:32:42 PM [vite] [fable]: compileProject: Full compile started of /home/projects/your-project-folder/App.fsproj
12:32:42 PM [vite] [fable]: compileProject: fable-library located at /home/projects/your-project-folder/node_modules/@fable-org/fable-library-js
12:32:42 PM [vite] [fable]: compileProject: about to type-checked /home/projects/your-project-folder/App.fsproj.
12:32:44 PM [vite] [fable]: compileProject: /home/projects/your-project-folder/App.fsproj was type-checked.
12:32:44 PM [vite] [fable]: compileProject: Full compile completed of /home/projects/your-project-folder/App.fsproj
```

And now we can import our code from F# in our `index.html`:

```html
<script type="module">
  import "/App.fs";
</script>
```

⚠️ We cannot use `<script type="module" src="/App.fs"></script>` because Vite won't recognize the `.fs` extension outside the module resolution.
See [vitejs/vite#9981](https://github.com/vitejs/vite/pull/9981)

[Next]({{fsdocs-next-page-link}})
