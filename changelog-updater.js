import { $ } from "bun";

const version = await $`bunx changelog --latest-release`.text();

if (version === "") {
  console.log("No new version found.");
  process.exit(0);
}

console.log(`New version found: ${version.trim()}`);
console.log("Updating package.json version...");

// Update package.json version
await $`npm version ${version.trim()}`;