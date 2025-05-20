import { $ } from "bun";

// https://www.npmjs.com/package/keep-a-changelog#cli

const version = await $`bunx changelog --latest-release`.text();

const packageVersion = await $`npm info vite-plugin-fable version`.text();

if (version === packageVersion) {
  process.exit(0);
}

// Update package.json version
await $`npm version ${version.trim()}`;