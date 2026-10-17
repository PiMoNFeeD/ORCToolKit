# ORCToolKit
A C# tool that can do a bunch of stuff with Hexane Engine's Resident Evil: Operation Raccoon City files. Everything is based on manually reverse-engineering the game and it's assets.

Created to make modding RE:ORC easier, and improve my C# skills =) (this is the first big app I made on C#).

# Capabilities
Consider this a future roadmap:
| Feature | ORCToolKit (PC files) | ORCToolKit (console files) | Other public tools (PC files) | Other public tools (console files)
|:----------|:----------:|:----------:|:----------:|:----------:|
| Editing uncompressed SSG archives (i.e. csv archives)                                   | ✔️ | ✔️ | ✔️ | ❓ |
| Editing compressed SSG archives (i.e. texture and model archives)                       | ✔️ | ❌ (unknown compression) | ✔️ | ❌ |
| Editing SSG archives that are both compressed and uncompressed (i.e. cutscene archives) | ✔️ | ❌ (unknown compression) | ❌ | ❌ |
| Localization editing                                                                    | ✔️ | ✔️ | ❌ | ❌ |
| SWB2 database editing (things like character stats or level layouts)                    | ✔️ | ✔️ | ❌ | ❌ |
| Model viewer                                                                            | ❌ | ❌ | ✔️ (see below) | ❓ |
| Animation viewer                                                                        | ❌ | ❌ | ✔️ (see below) | ❓ |

If you would like to view models or animations, check out my Blender script for that [here](https://residentevilmodding.boards.net/post/147414). It supports a much wider range of models than any other existing tool, and it's the only tool that can read ORC's animations. I plan on integrating it's functionality into this tool in the future.
