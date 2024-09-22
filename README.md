<!-- PROJECT LOGO -->

<div align="center">
  <a href="https://github.com/murra133/MagicTape">
    <img src="art\logo.png" alt="Logo" width="200">
  </a>
  <p align="center">
    A multi-user collaborative environment for Rhino
    <br />
    ·
    <a href="https://github.com/murra133/MagicTape/issues">Report Bug</a>
    ·
    <a href="https://github.com/murra133/MagicTape/issues">Request Feature</a>
  </p>
</div>

# Magic Tape

Magic Tape is a library written around the [Reekon T1](https://www.reekon.tools/t1-tomahawk) using an unofficial Bluetooth wrapper to communicate with it.

# How it works

The [Reekon T1](https://www.reekon.tools/t1-tomahawk) is attached to a mobile phone with the iPhone app installed.
The iPhone app and Tape measure combo provide real world coordinates relative to given Grid Coordinates.

An intermediary server recieves and forwards any sent messages.

A Rhino Plugin acts as the reciever node and caches all of the incoming data, displaying current and previous measurements.

# 🧑‍💻 👩‍💻 Get Hacking

## Prerequisites for Rhino Plugin

- [.NET Core 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Rhino 8.11+](https://www.rhino3d.com/download/) (win/mac)
- XCode
- npm
- An iPhone per Tape
- A Reekon T1 Measuring Tape

## Debugging the Rhino Plugin

1. Clone this Repo
2. Open the `src/desktop/` (the Rhino Plugin) in VSCode and hit debug
3. Run the `MagicTape` command

## Debugging the Phone App

1. Open `src/phone/` (the mobile app) in VSCode
2. Hit Debug
3. Open the app on your device
4. Turn the Tape Measure On
5. Connect Via Bluetooth

## Setting up the Server

- AWS API Gateway Websocket for Server Connections
- A DynamoDB database to host connections.
- Serverless Lambda functions to route messages to connected components

See src/Server for functions

# Collaborators

- Brian Murray
- Callum Sykes
- James Coleman
- Sergey Pigach
- Soomeen Hahm
- Poorva Joshi

# Acknowledgments

Big thanks to AEC Tech LA 2024 for arranging this event! This project has been a great collaboration of several great minds. Please check out other hackathon projects and future hackathon events hosted by AECTech.

# License

We use the MIT License, see LICENSE
