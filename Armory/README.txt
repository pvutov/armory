This program is an honest attempt to expose the "hidden" WRD armory values in a user-friendly way. Pull requests are welcome.

------------------------------------------------

Instead of showing all of the data that the modding suite provides, I've chosen to highlight 
the parts that are relevant to the player. There are bound to be errors. I may forget to include
an important stat. I may present an NDF variable as something it is not, because I do not understand
the entirety of the NDF binary. If you catch any of these errors or omissions, please inform me.

Many of the data transformations I'm doing are described in NDF_Documentation.txt. If curious about the source of a field not mentioned in the documentation, look in UnitDatabase.cs. 

------------------------------------------------

If you're aware of important variables that I'm not exposing, please point them out: When it comes to writing this tool, merely identifying the gameplay-relevant parts of the NDF is half the battle.

If you're aware of good presentation changes to existing fields, point them out too. A trivial example of a conversion that is already in the tool is the addition of "m" to some variables that are provably in meters. A less trivial and even more useful conversion is that the weapon range stats are multiplied by 175/13000 to convert them from internal units to the meters that players are familiar with.
