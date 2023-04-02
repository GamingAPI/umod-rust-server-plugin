# UMod GamingAPI Plugin
The rust server plugin utilizing UMod for GamingAPI. 

## Configuration

The plugin is setup for you to control which events you want your server to save, certain services can require you to enable certain ones in order to function, but that is up to the individual service.

```json
{
  "If true, when a player writes in the chat the event will be saved": true,
  "If true, when a player farms a resource the event will be saved": true,
  "If true, when a player respawns the event will be saved": true,
  "If true, when a player disconnects the event will be saved": true,
  "If true, when a player connects the event will be saved": true,
  "If true, when the server wipes the event will be saved": true,
  "If true, when a player crafts an item the event will be saved": true,
  "If true, when a player is banned the event will be saved": true,
  "If true, when a player is reported the event will be saved": true,
  "If true, when a server command is run the event will be saved": true,
  "If true, when a player hits another player the event will be saved": true,
  "If true, when a player picks up an item the event will be saved": true,
  "If true, when a player loots an item the event will be saved": true
}
```