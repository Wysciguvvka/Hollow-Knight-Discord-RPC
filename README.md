
# Hollow Knight Discord Rich Presence Mod
Hollow Knight Discord Rich Presence Mod (or Discord Custom Status) mod adds custom Hollow Knight In-Game Status to your discord activity.\
This modification is for Hollow Knight __1.5__ or above.\
[Preview](https://i.ibb.co/qDj1b3T/presence.png)

## Feedback/Bug Reports/Suggestions

If you have any feedback/suggestion or you've found a bug, reach me on\
Discord: __@Wysciguwka#8823__\
GitHub: [Wysciguvvka](https://www.github.com/Wysciguvvka)


## Installation
1. Install the [modding api](https://github.com/hk-modding/api) if you didn't do it yet.
2. Download the latest Rich Presence Mod release from [here] or compile it by yourself.
3. Extract the .zip file and put __HollowKnightDiscordRPC__ in your mods folder 
(Usually):

``` 
Windows:	C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\Mods\
Linux:		~/.local/share/Steam/steamapps/common/Hollow Knight/hollow_knight_Data/Managed/Mods/
Mac:		~/Library/Application Support/Steam/steamapps/common/Hollow Knight/hollow_knight.app/hollow_knight_Data/Resources/Data/Managed/Mods/
```
or:
```
{Your Hollow Knight installation}/hollow_knight_Data/Managed/Mods/
```
4. Put the Plugins folder in:
(if you compiled the code by yourself, download [Discord GameSDK](https://discord.com/developers/docs/game-sdk/sdk-starter-guide) Put x86 and x86_64 folders into Plugins)
```
{Your Hollow Knight installation}/hollow_knight_Data/
```
5. Launch Hollow Knight and enjoy your custom Discord In-Game status
## How To Use

Launch Hollow Knight after installation.\
If you opened Discord after launching Hollow Knight, it might take a few seconds to display custom status.
The mod will not work with the Discord browser client.\
If you want to customize the mod, then navigate to Settings > Mods > Discord Rich Presence.


## Compilation

To build the mod by yourself:
1. Clone the repository
2. From Hollow_Knight_Data folder copy:
```
Assembly-CSharp.dll
UnityEngine.CoreModule.dll
UnityEngine.dll
UnityEngine.TextRenderingModule.dll
UnityEngine.UI.dll
UnityEngine.UIModule.dll
```
to `HollowKnightDiscordRPC/references` folder.
3. Build the solution using an IDE or `dotnet build`.
4. The result will be in `bin/Release/HollowKnightDiscordRPC.dll (or bin/debug/)`

## Authors

- [Wysciguvvka](https://www.github.com/Wysciguvvka)


## Appendix

That was my first time coding in C#, so the code might not be excellent.\
I will appreciate any code reviews/feedback/whatever

