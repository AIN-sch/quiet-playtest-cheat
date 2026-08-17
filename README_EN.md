# QUIET Cheat (QUIET Playtest)

> 🗓️ **Game version checked: 2026-08-17**
>
> If the game updated and this didn't, it means **I stopped playing**, or **I forgot to upload**.
> So, **don't nag me**. A real man stands on his own — figure out how to update it yourself.

> ⚠️ **Disclaimer**
>
> This plugin is for **learning and research purposes only** (BepInEx plugin development, Unity IL2CPP reverse engineering, game network mechanics research).
> It must **not** be used for commercial purposes, to disrupt other players' experience, or in violation of any game/platform terms of service.
>
> - Any consequences arising from using, modifying, or redistributing this plugin (bans, losses, disputes, etc.) are **entirely the user's own responsibility**
> - The author is not liable for any direct or indirect damages, nor for any legal consequences
> - Do not use it in contexts that require fairness (tournaments, leaderboards, etc.)
> - Please retain this notice when redistributing
>
> Downloading and using this means you agree to the above. **Your actions, your problem.**

A BepInEx 6 (IL2CPP) plugin. Drop the compiled `QUIETCheat.dll` into
`<game folder>\BepInEx\plugins\`, restart the game, press **INS** to toggle the menu (on by default).

Source is open. Take it, mod it, do whatever you want with it.

## Features

- **ESP**: monsters/grabbables show Chinese name + distance (red for monsters, yellow for items). Works for host and client.
- **God mode + infinite stamina**: refills every frame (host only).
- **One-click collect**: suck all grabbables into your bag (host only).
- **One-click vacuum**: pull physical items to your feet, with a configurable cap and mission-item priority (host only).
- **Noclip / fly**: disable collision and move directly (host only).
- **Freeze monsters**: skip monster AI frames, keeps them in place (host only).
- **No alert / phase lock**: zeroes the alert gauge every frame, monsters stay at the starting phase (host only).

**Host gets every feature; clients only get ESP** — everything else is greyed out.

## Known Bugs

- **ESP is buggy.** Yes, I've seen it. I just don't want to fix it — small game, not worth it.
- There may be other bugs here and there. Also not fixing those.

## My Attitude

Small game, not worth much effort. I'm lazy.
Use it if it works; if not, fork it and fix it yourself.

## Modding / Forks

Source lives in `src/`. Open **`QUIETCheat.sln`** directly in Visual Studio and go to town.
Building requires the game installed (the csproj references assemblies in `BepInEx\interop\`, which exist after BepInEx has run the game once); run `dotnet build -c Release`.

Use it however you like, distribute it however you like.

## Tech Discussion

QQ group: **1102821216**
When joining, you **must** state where you came from (e.g. "saw it on GitHub"), otherwise your request won't be approved — for the group's safety.

## Build

```
dotnet build -c Release
```

Needs dotnet SDK (6+). Run the game through BepInEx once first so the game assemblies are generated in `BepInEx\interop\`, or the build will fail.

## Changelog

- **v1.0.0**: Initial release
