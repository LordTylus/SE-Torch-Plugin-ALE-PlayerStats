### Introduction
Often I found myself in the position to have a quick overview of players of a certain faction. Like their Entity-IDs or Steam-IDs as well as the date the server last had contact with them. 

So this plugin does exactly that. It adds a command where you can list all players your world has ever seen and allow for some filtering and sorting. 

Also checking and adjusting Players Reputation with certain factions is a bit unpleasant to do in space master and takes long. especially if you want to do big changes that affect a whole faction. 

For that the plugin offers commands too.

### Commands
- !listplayers [-online] [-players] [-faction=&lt;FactionTag&gt;] [-orderby=&lt;name|date|faction|pcu|blocks&gt;]
 - Lists all Players. If -online is used, only online Players are shown
 - if -players is used, all NPCs are ignored
 - if -npcs is used, all Players are ignored
 - if -faction=&lt;Tag&gt; is used, only players of said faction are used. To see players without faction just leave the tag empty.
 - if -orderby=&lt;name|date|faction|pcu|blocks&gt; is used players are ordered ascending by name, faction tag or date. 
 - if -filter=0lt;name|date|faction|pcu|blocks&gt;&lt;&lt;, &lt;=, &gt;, &gt;=, =, !=&gt;&lt;Attribute&gt; is used the list is filtered accordintly.

A combination of several -orderby and -filter is possible to do multiplayer sorting and fine filtering. 

You can combine multiple of parameters like -players -online -orderby=faction

## Commands for Reputation

- !reputation list player &lt;playername&gt;
 - Lists the Reputation one Player has with every Faction on the server.
- !reputation list faction &lt;factionTag&gt;
 - Lists the Reputation one Faction has with every Player on the server.
- !reputation list factions &lt;factionTag&gt;
 - Lists the Reputation one Faction has with every Faction on the server.
- !reputation change player &lt;playername&gt; &lt;factionTag&gt; &lt;reputationDelta&gt;
 - Adds the given reputation between the given player and faction. It can be negative to remove reputation.
- !reputation change faction &lt;factionTag1&gt; &lt;factionTag2&gt; &lt;reputationDelta&gt;
 - Adds the given reputation of all players from faction 1 to faction 2. It can be negative to remove reputation.
- !reputation set player &lt;playername&gt; &lt;factionTag&gt; &lt;reputation&gt;
 - Sets the given reputation between the given player and faction.
- !reputation set playerallfactions &lt;playername&gt; &lt;reputation&gt; 
 - Sets the given reputation between the given player and all factions.
- !reputation set faction &lt;factionTag1&gt; &lt;factionTag2&gt; &lt;reputation&gt;
 - Sets the given reputation of all players from faction 1 to faction 2.
- !reputation set factionallplayers &lt;factionTag&gt; &lt;reputation&gt;
 - Sets the given reputation of all players with passed on faction.

### Examples

- !listplayers -filter=blocks&lt;500 -filter=pcu&gt;2000 -orderby=faction -orderby=name -players
- !listplayers -filter=blocks&lt;500 -npcs
- !listplayers -online -filter=blocks&gt;1000
- !listplayers -filter=date&gt;=10 
 - Only shows people who have logged in within the last 10 days. Alternatively you can use &lt;10 which will only show people offline for longer than 10 days. 

### Github
[See here](https://github.com/LordTylus/SE-Torch-Plugin-ALE-PlayerStats)