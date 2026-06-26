<p align="center">
<img width="512" height="512" alt="NTCS" src="https://github.com/user-attachments/assets/2a5ad40e-df70-42fd-9f27-e17ef4b86fc2" />
</p>

<h1>Neurotrauma C#</h1>

A complete re-write of Neurotrauma in C# (with a dash of Lua).
- Better performance, less stuttering and efficent memory usage.
- Brand new QOL features.
- Bug fixes.
- New additions.
- Full addon compatibility.

<h3>Links</h3>

<hr>

Google Doc with codebase can be found [here](https://docs.google.com/document/d/1AJDsnBTOf8GZjzLwk0Dm6FDzZSp3U4sCgUFjvv2a_uo/edit?tab=t.0#heading=h.w01ukn2d7564)

C# Documentation (for Barotrauma) can be found [here](https://luatrauma.github.io/Luatrauma.Docs/api/cs/client/html/)

Lua Documentation (for Barotrauma) can be found [here](https://evilfactory.github.io/LuaCsForBarotrauma/lua-docs/)

NT Repository (Lua version) can be found [here](https://github.com/OlegBSTU/Neurotrauma)

<h3>Code & Requirements</h3>

<hr>

- Namespace: _Neurotrauma_
  
- PascalCase is recommended. Following C# conventions.

- Download the required refs from [here](https://github.com/evilfactory/LuaCsForBarotrauma/releases/download/latest/luacsforbarotrauma_refs.zip)

- If you're going to be running this locally, change the LocalMods folder directory in **Build.props**. IDK if there is a better way to do this without GitHub constantly changing it.

- Linux / OSX projects have been disabled, as this is complex enough as-is right now.

- Download Visual Studio (Not Visual Code Studio!!!) with the .NET addon, then open the Neurotrauma.sln file using it to have the entire project visible and easily navigatable.

- If you want to test, you should go to the top side of the screen and under 'Build' hit 'Rebuild Solution'; this will re-generate the entire LocalMod. You can then Launch Barotrauma via Visual Studio or the normal way.

- Everything within the **_Assets_** folder gets copied into LocalMods alongside the C# code, already compiled.

<h3>Credits</h3>

<hr>

- Tina/Mannatu: For making the original NT mod in Lua.

- Heelge: For maintaing/updating the NT Fork.
