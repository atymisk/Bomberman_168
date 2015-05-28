(full screen this)

1. Setting up the database to accept logins, you have two options:

	Regardless of the options below, you need to have the references attached in your version of the solution: System.Data AND MySql.Data
	You must also have the C# MySQL connector, which if you need to download and install the link is here: https://dev.mysql.com/downloads/connector/net/6.9.html

	a. Pseudo-Database interactions
		-In the "ServerSystem" Folder open the ClientServer solution and open up the Server.cs file.
		-There is a class called "Settings" in the near top of the code and there is only one variable, database
		-switch to false if you want a Pseudo-Database intereaction or True if you wish to use a functioning MySQL Database

	b. fully functioning MySQL database
		-Make sure the database bool variable in the server.cs file is set to true for this to work
		-Setup your MySQL database to have the schema named "bmdb" and the table "main"
			-Schema needs to have the following columns and types
				1. username - VARCHAR(12)//the number doesn't entirely matter, cannot be null
				2. pass - VARCHAR(12)//cannot be null
				3. wins - INT
				4. games - INT
		-enter the ip address of your database in the server.cs file in either under the "IP" class in the string "mySQL" OR
		 directly in the "DatabaseHandler" class under the server variable

	After following the steps above, you should have a database or pseudo-database interactions with the game

2. Setting up a game with two players:
	-open the unity project and select the scene "LogInScreen" and run it
	-If you are using your own MySQL database, proceed to the register page to create logins and follow the onscreen instructions

	-Build and run to get a second game client running to test this

	-If you create a login successfully, you will be immediately transported to the Game Select scene
		-Whenever login is successful you will be taken to the Game Select
	-Create a Game will bring up a popup with an input field
		-enter a name and you will be brought to a lobby
		-The game will not start if there is not at least two players ready'd up
	-Second client should use the search feature to find the game you made and if found, it will bring up a popup for you to join it
	-When both players are ready, the game will start.

	**If a player disconnects, the game treats it as a death. So with only two players it will result in a gameover and 
	  bring the remaining player back to the lobby

3. Running two or more Sessions at once
	-Build and run at minimum of three client while having the unity editor open and running
	**currently our game doesn't really account for having one login logged in at one given instance, basically the same login can be used over and over
	-have two clients create a game session and name it separately from each other because it will make sure that all games are unique.
	-have the other two join each one, 1 for 1 and the other for the other.
		-once ready'd up a game will start.
		-Games are separate and run on their own.