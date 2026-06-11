<p align="center">
<img width="512" height="512" alt="shitpost" src="https://github.com/user-attachments/assets/164dd9c7-28a8-4639-8988-35a935374bc7" />
</p>

This is all just the skeleton from the C# documentation, changed for Neurotrauma.
It should be noted that I don't know what I am doing yet!

Google Doc with codebase can be found [here](https://docs.google.com/document/d/1AJDsnBTOf8GZjzLwk0Dm6FDzZSp3U4sCgUFjvv2a_uo/edit?tab=t.0#heading=h.w01ukn2d7564)

C# Documentation (for Barotrauma) can be found [here](https://evilfactory.github.io/LuaCsForBarotrauma/cs-docs/baro-server/html/index.html)

- Namespace: _Neurotrauma_
  
- PascalCase is recommended. Following C# conventions.

- Download the required refs from [here](https://github.com/evilfactory/LuaCsForBarotrauma/releases/download/latest/luacsforbarotrauma_refs.zip)

- If you're going to be running this locally, change the LocalMods folder directory in **Build.props**. IDK if there is a better way to do this without GitHub constantly changing it.

- Linux / OSX projects have been disabled, as this is complex enough as-is right now.

- Download Visual Studio (Not Visual Code Studio!!!) with the .NET addon, then open the Neurotrauma.sln file using it to have the entire project visible and easily navigatable.

- If you want to test, you should go to the top side of the screen and under 'Build' hit 'Rebuild Solution'; this will re-generate the entire LocalMod. You can then Launch Barotrauma via Visual Studio or the normal way.

- Everything within the **_Assets_** folder gets copied into LocalMods alongside the C# code, already compiled.
