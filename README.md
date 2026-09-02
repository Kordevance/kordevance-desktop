<p align="center">
  <img src="Kori/Assets/Images/logo-128.png" width="120" alt="Kori logo" />
</p>

<h1 align="center">Kori</h1>

<p align="center">Your personal planner, on the desktop.</p>

<p align="center">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white" />
  <img alt="Avalonia UI" src="https://img.shields.io/badge/UI-Avalonia-6C3483" />
  <img alt="Platforms" src="https://img.shields.io/badge/platforms-macOS%20%7C%20Windows-lightgrey" />
  <img alt="License" src="https://img.shields.io/badge/license-all%20rights%20reserved-lightgrey" />
</p>

## What is this

Kori is the desktop client for the Kori assistant. It's the app you actually open, look at, and click around in: pairing your device to a gateway, picking a profile, chatting, wiring up connectors and model providers, and setting goals.

This repo is just the client. It doesn't run any domain logic itself, it talks over HTTP to your own Kori gateway (a separate service you host) and renders everything on top with a native-feeling cross platform UI.

## How it fits together

A quick mental model before you dive into the code:

- **Gateway** is the server you own and run somewhere. It holds your profiles, your model providers, your connectors, your chats, your goals. Kori (this app) never stores any of that on its own.
- **Device pairing** is how this app first meets your gateway. You point Kori at your gateway's address, enter a pairing code generated on the gateway side, and from then on this device is trusted.
- **Profiles** let more than one person (or persona) share a gateway without mixing up chats, providers, or goals.
- **Providers** are the LLM providers you've configured on your gateway (OpenAI, Anthropic, a local model, whatever you've set up), plus which one handles which job through model assignments (primary, triage, discovery).
- **Connectors** are the extra services your gateway can reach out to on your behalf.
- **Goals** and **Chat** are the day to day surfaces you actually spend time in.

## Getting started

You'll need:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A running Kori gateway somewhere you can reach (this repo won't help you stand one up, that lives elsewhere)

Clone it, restore, run:

```bash
git clone git@github.com:Kordevance/kordevance-desktop.git
cd kordevance-desktop
dotnet run --project Kori/Kori.csproj
```

First launch will ask for your gateway's address and a pairing code. Grab the code from your gateway and you're in.

## Contributing

This is early and still moving fast, so expect some rough edges. If you find a bug or have an idea, open an issue and we'll take a look.

## License

There's no open source license attached to this repository, all rights are reserved. The code is here so you can read it, learn from it, and build and run your own copy locally to see how it works. What you can't do is redistribute it, rebrand it, ship a modified version, or use it commercially without asking first.

If you just want to use Kori day to day, grab a signed build from the releases page instead of building from source, those are the ones we actually support.
