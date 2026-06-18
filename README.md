<p align="center">
    <img src="Images/Zeyadstrap-full-dark.png#gh-dark-mode-only" width="380" alt="Zeyadstrap">
    <img src="Images/Zeyadstrap-full-light.png#gh-light-mode-only" width="380" alt="Zeyadstrap">
</p>

<div align="center">

[![License][shield-repo-license]][repo-license]
[![Downloads][shield-repo-releases]][repo-releases]
[![Version][shield-repo-latest]][repo-latest]

</div>

----

Zeyadstrap is a fork of [Bloxstrap](https://github.com/bloxstraplabs/bloxstrap).
Yeah thats basically it what more do you need.

This is a fork that will also almost never update and only will update when the sun explodes.

oh yea btw Zeyadstrap is only on Windows, go try out [Appleblox](https://github.com/AppleBlox/appleblox) if your on MacOS.

(this is vibecoded because idk how to code in cs. I'll learn it later though trust me)

## Features

- Discord RPC so your friends can see what your playing
- Custom roblox mods that DONT interact with the ROBLOX executable binary
- UI icon color picker
- Server location loop up, I think, I dunno
- Graphics and UI related Fast Flags that might work
- Custom bootstrapper appearance thingy ma thing

## Installing

Download the [latest release of Zeyadstrap](https://github.com/zeyadmused888/Zeyadstrap/releases/latest)!! Extract it first and run it!

You may also need the [.NET 6 Desktop Runtime](https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=x64&rid=win11-x64&apphost_version=6.0.36&gui=true). If its missing. Download it.

Windows smartscreen WILL warn you and trust me THIS ISNT MALWARE IM NOT EVEN JOKING just click, "Read more" or somthing like that then click, "Run Anyway"

## FAQ

**Is this malware?**

NO, I JUST SAID ITS NOT. LOOK AT THE SOURCE CODE!!

**Can using this get me banned from Roblox?**

No, it wont ban you since it doesn't interact with ROBLOX's binary and code. For more information check out Zeyadstrap's explanation: https://github.com/zeyadmused888/Zeyadstrap 

**Is this official Zeyadstrap?**

No, read the repo name.

## Building

Clone the repository with submodules, then build with .NET:

```powershell
git submodule update --init --recursive
dotnet build Bloxstrap.sln
```

To publish an executable:

```powershell
dotnet publish Bloxstrap\Bloxstrap.csproj -c Release -r win-x64 --self-contained false
```

The published executable will be `Zeyadstrap.exe` in:

```text
Bloxstrap\bin\Release\net6.0-windows\win-x64\publish\
```

## Credits

Zeyadstrap is based on [Bloxstrap](https://github.com/bloxstraplabs/bloxstrap). Zeyadstrap uses the [WPF UI](https://github.com/lepoco/wpfui) library and maintains a fork at [bloxstraplabs/wpfui](https://github.com/bloxstraplabs/wpfui).

## License

This project follows the license included in this repository.

[shield-repo-license]:  https://img.shields.io/github/license/zeyadmused888/Zeyadstrap
[shield-repo-releases]: https://img.shields.io/github/downloads/zeyadmused888/Zeyadstrap/latest/total?color=981bfe
[shield-repo-latest]:   https://img.shields.io/github/v/release/zeyadmused888/Zeyadstrap?color=7a39fb

[repo-license]:  https://github.com/zeyadmused888/Zeyadstrap/blob/main/LICENSE
[repo-releases]: https://github.com/zeyadmused888/Zeyadstrap/releases
[repo-latest]:   https://github.com/zeyadmused888/Zeyadstrap/releases/latest
