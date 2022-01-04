
# Hollow Knight Discord Rich Presence Mod
Hollow Knight Discord Rich Presence Mod (or Discord Custom Status mod) adds custom Hollow Knight In-Game Status to your Discord activity.\
This modification is for Hollow Knight __1.5__.\
\
![Preview](/Previews/presence.png)

## Feedback/Bug Reports/Suggestions

If you have any feedback/suggestions or you've found a bug, please reach me on\
Discord: __@Wysciguwka#8823__


## Installation
1. Install the [modding api](https://github.com/hk-modding/api) if you haven't installed it.
2. Download the latest mod release from [here](https://github.com/Wysciguvvka/Hollow-Knight-Discord-RPC/releases) or build it yourself.
3. Extract the .zip file and copy the __DiscordRPC__ folder into the mods folder
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
4. Launch Hollow Knight and enjoy your custom Discord In-Game status
## How To Use

Launch Hollow Knight after installation.\
If you opened Discord after launching Hollow Knight, it might take a few seconds to display custom status.
The mod will not work with the Discord browser client.\
If you want to customize the mod, then navigate to Settings > Mods > Rich Presence.\
Keep in mind that Discord GameSDK has a rate limit of one update per 15 seconds, so it may take a while to update your status.


## How to build

To build the mod by yourself:
1. Clone the repository.
2. From the `Hollow_Knight_Data` folder in your Hollow Knight installation, copy:
```
Assembly-CSharp.dll
UnityEngine.CoreModule.dll
UnityEngine.dll
UnityEngine.TextRenderingModule.dll
UnityEngine.UI.dll
UnityEngine.UIModule.dll
```
into the `HollowKnightDiscordRPC/references` folder.\
3. Download [Discord GameSDK](https://discord.com/developers/docs/game-sdk/sdk-starter-guide) and copy the `x86` and `x86_64` folders into `Assets/Plugins`\
4. Build the solution.\
5. The result will be in `bin/Release/HollowKnightDiscordRPC.dll (or bin/debug/)`

## Mod Preview

###### In Menu Status:
![Menu](/Previews/menu.png)
###### Classic Mode:
![Classic Mode](/Previews/presence.png)
###### Steel Soul Mode:
![Steel Soul Mode](/Previews/steel.png)
###### Godseeker Mode:
![Godseeker Mode](/Previews/godseeker.png)
###### Pantheons:
![Customization](/Previews/pantheon.png)
###### Boss Health:
![Customization](/Previews/bosshp.png)
###### Customization:
![Customization](/Previews/others.png)
###### Stag Travel Status:
![Stag Travel](/Previews/travel.png)
###### Hide Everything option:
![Hide Everything](/Previews/hide.png)
###### Settings:
![Settings](/Previews/settings.jpg)

## Authors

- [Wysciguvvka](https://www.github.com/Wysciguvvka)

#### Disclaimer

That was my first time coding in C#, so the code might not be excellent.\
I will appreciate any code reviews/feedback/whatever

