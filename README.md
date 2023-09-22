# Rambles

I wrote Rambles because I wanted a simple static content generator like Hexo, but didn't want to learn its specifics or stray outside of the .NET environment.

## How it works

 - The framework parses all `.md` files in the source directory as markdown documents with YAML-like metadata (see: [RambleParser.cs](TheLeftExit.Rambles/RambleParser.cs)).
 - You provide the logic to aggregate parsed documents and generate a set of pages/files based on those documents (see: [RambleRenderer.cs](TheLeftExit.Rambles.Demo/RambleRenderer.cs)).
 - The frameworks dumps all generated documents (and other files from the source directory) to the target directory.

You can find a demo project that uses Rambles at [TheLeftExit.Rambles.Demo](TheLeftExit.Rambles.Demo), and its output at [theleftexit.net/Rambles](https://theleftexit.net/Rambles/).