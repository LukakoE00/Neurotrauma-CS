<p align="center">
<img width="512" height="512" alt="NTCS" src="https://github.com/user-attachments/assets/2a5ad40e-df70-42fd-9f27-e17ef4b86fc2" />
</p>

<h1>Neurotrauma C#</h1>

A complete re-write of Neurotrauma in C# (with a dash of Lua).
- Better performance, less stuttering and efficent memory usage.
- Brand new QOL features.
- Bug fixes.
- New additions.

Prior releases can be found under "Releases". To revert back download the zip and extract it into local mods.

<h3>Links</h3>

<hr>
C# Documentation (for Barotrauma) can be found [here](https://luatrauma.github.io/Luatrauma.Docs/api/cs/client/html/)

Lua Documentation (for Barotrauma) can be found [here](https://evilfactory.github.io/LuaCsForBarotrauma/lua-docs/)

NT C# Template Repository can be found [here](https://github.com/BigEvilGreenBean/Neurotrauma-CS-Addon-Template)

<h3>Code & Requirements</h3>

<hr>

- Namespace: _Neurotrauma_
  
- PascalCase is recommended. Following C# conventions.

- If you're going to be running this locally, change the LocalMods folder directory in **Build.props**. IDK if there is a better way to do this without GitHub constantly changing it.

- Download Visual Studio (Not Visual Code Studio!!!) with the .NET addon, then open the Neurotrauma.sln file using it to have the entire project visible and easily navigatable.

- If you want to test, you should go to the top side of the screen and under 'Build' hit 'Rebuild Solution'; this will re-generate the entire LocalMod. You can then Launch Barotrauma via Visual Studio or the normal way.

- Everything within the **_Assets_** folder gets copied into LocalMods alongside the C# code, already compiled.

<h3>Contributions/Pull Requests</h3>

<hr>

NT C# and NT in general is open source (obviously) and encourages others to report issues or fix those issues themselves. There is no strict guideline to making a pull request or reporting an issue, however please include details regarding the version of NT, what addons were in use and what the fix does. Any vibe coded pull request will not be accepted, don't waste your time making it.

<h3>Credits</h3>

<hr>

- Tina/Mannatu: For making the original NT mod in Lua.

- Heelge: For maintaing/updating the NT Fork.

- Lukako: For porting/making XML, writing Afflictions, writing Item Methods, making Config. + Misc files

- Cookie: For writing Priority system, Human Update Template, Stat Template, writing Item Method system. + Misc files

- Finka: For Writing Dynamic Items system. + Misc files

- BigEvilGreenBean: For writing Human Update system, writing Helper Functions, NT types. + Misc files

- Walter: Helped in spirit 🙏
