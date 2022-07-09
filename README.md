# TharBot
A discord bot in Discord.NET, originally made to learn using APIs and databases, now I make new functionality for fun.

Uses Lavalink (https://github.com/freyacodes/Lavalink) for its music modules.
This was built with roughly a month's experience in programming, so it's probably a mess for anyone else, but if you for some reason want to use it yourself:

```
1. Replace the "Token" value in appsettings.json with your discord bot token
2. Create an account with MongoDB Atlas OR install mongoDB and use localhost as the connection string in the next step
3. Replace the "MongoDB ConnectionString" value in appsettings with your own connection string
4. Replace all the API token values with your own API tokens, if you want the functionality for those commands (imgur, anime, reddit, weather and pollution)
5. Rename "appsettings.json" to "appsettings.development.json" (Or remove the ".development" on line 19 in src/Discord Bot/TharBot/TharBot/program.cs
6. Replace the emotes in "Handlers\EmoteHandler.cs" with your own emotes for the various actions (game functions will not work otherwise)
7. Get lavalink from https://github.com/freyacodes/Lavalink, and run it concurrently with the bot
8. Build and run the TharBot project
9. Type the "th.help" command in the discord server you added the bot to
```
