# WoT_S-Team
**World of Tanks: Strategy** is a turn-based strategy game based on the legendary game: [World of Tanks](https://worldoftanks.com/ "World of Tanks").


### Platforms
- Windows x64-x86

### Technologies
- UI - C# WPF (MVVM)
- AI and Network - C++

### Compiler
- MSVC++ v.143 (vs2022)
- NET Framework v. 4.7.2

### Command line (required)
- `-u` - user name
- `-p` - pass
- `-g` - game name
- `-pc` - player count (1 to 3)
- `-tc` - turn count (1 to 100)
- `-o` - is observer (0 or 1)
- `-ai` - enable artificial intelligence (0 or 1)

### Command line (optional)
- `-q` - close the game after the end of the game

Example: `-u USER -p PASS -g GAMENAME  -pc 1 -tc 45 -o 0  -ai 1 -q`

### Docker
It is possible to build and run the project in the docker (need ~27 GB of free space)
- Switch Docker to Windows Container Mode
- In the folder from the command line, build the image
Command: `docker build -t game_image:latest -m 2GB .`

- Run the container from the image with the parameters
Command: `docker run -i --name game_container game_image -u USER -p PASS -g GAMENAME -pc 1 -tc 45 -o 0  -ai 1 -q`

### Team members
- [Andrei Petrashevich](https://github.com/mrpetrashevitch)
- [Vasili Siachko](https://github.com/banany2001)
- [Artsiom Khilko](https://github.com/Artyom-Master)

### About
- This product is designed for educational purposes only.
