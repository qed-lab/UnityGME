
# Unity General Mediation Engine (UGME)

The Unity General Mediation Engine is a Unity-based port of the General Mediation Engine, a procedural strong story interactive narrative generator. The system models its world state and action dynamics using planning problems specified in [PDDL](https://en.wikipedia.org/wiki/Planning_Domain_Definition_Language). From the planning problem, (U)GME builds a tree of world states connected by actions characters can take in the world. One of these characters is controlled by a human client and the rest are automatically controlled by the solution to the current planning problem. World states and action updates are exposed to the client through a procedurally generated interface and gameplay arises through an online expansion of the underlying tree data structure and proceeds until the planning problem is solved.

This repository contains experimental UGME code that is demonstrated via a game built atop the GME code-base.  The game is called "Knights of the Emerald Order", a point-and-click SCUMM-like adventure game.

## Installation

### Download Sources

Currently, the only way to install the UGME is through access to its source.  The command

        git clone https://github.ncsu.edu/recardon/UGME <directory>

will create a clone of the UGME master repository in `<directory>`. The directory is created if it does not yet exist.  Note that UGME has several dependencies, listed below.  These dependencies must be installed first before the above repo is built.

### Dependencies

- Mac OS X 10.11.6 (El Capitan). While in theory higher versions of the Mac OS X should be supported without problems, higher versions have not been tested.
- [Unity Game Engine](https://unity3d.com/) 5.6.1
- A Planning System

#### Installing the Unity Game Engine

Navigate to https://unity3d.com/get-unity/download/archive, download Unity version 5.6.1, and follow the installation instructions.

#### Installing a Planning System

The UGME is planner-agnostic since it operates on the basis of the PDDL representation, but requires a planner for operation.  While in general multiple planners could be supported, this repository is currently configured to work with a specific planner made available through the [Lightweight Automated Planning ToolKiT (LAPKT)](http://lapkt.org/index.php?title=Main_Page): the SIW+-then-BFSF planner, the top performer in the ad-hoc Ultra-Agile Planning Competition (currently running at [editor.planning.domains](editor.planning.domains)). The instructions in this section tell you how to install and configure the SIW+-then-BFSF planner.

##### Installing LAPKT Dependencies

This is adapted directly from the instructions listed here: http://lapkt.org/index.php?title=Download. I use the Homebrew package manager because it makes the installation process easier.


1. Install [Homebrew](https://brew.sh/), the macOS package manager.
2. Open a Terminal window and enter the following commands (ignore the comments which start after the hash sign):

        brew install gcc48
        brew install scons
        brew install boost
        brew install makedepend
        brew install flex
        brew install bison
        brew install python

3. Edit your `~/.bash_profile` file by adding the following to it.

        # Puts Homebrew packages on the path
        PATH=$PATH:/usr/local/Cellar  
        # Makes GCC4.8 and G++4.8 the default
        alias gcc='gcc-4.8'
        alias g++='g++-4.8'

4. Enter `source ~/.bash_profile` to make the changes effective immediately.


##### Installing LAPKT's SIW+-then-BFSF planner.

A copy of [LAPKT](http://lapkt.org/index.php?title=Download) is available within the UGME repository, at `<directory>/Dependencies/LAPKT-public/`.

1. Open a Terminal window, navigate to `<directory>/Dependencies/LAPKT-public/`, and enter the command: `scons`

2. Navigate to the folder `<directory>/Dependencies/LAPKT-public/external/libff`, and enter the commands:

        make clean
        make

3. Navigate to the folder `<directory>/Dependencies/LAPKT-public/planners/siw_plus-then-bfs_f-ffparser`, and enter the command: `scons`. The command will create an executable file `siw-then-bfsf` in this directory.

4. Copy the executable file `siw-then-bfsf` into `<directory>/External/planners/siw-then-bfsf/`.

## Usage

To see the UGME in action with its associated game (Knights of the Emerald Order), open the Unity game engine, and hit `File > Open Project` and then select the UGME `<directory>` folder.

Once inside the Unity IDE, click on the `Scenes/Game` scene to test the main game.  When you hit the Play button, you should see the message "System Loaded" in the Debug Log.



## Contributing

Details coming soon.

## History

- March 12, 2017: Expansion of UGME for support of adventure games
- March 19, 2015: Alpha Release of UGME
- August 15, 2014: Initial Commit

## Credits

- Justus Robertson: Principal Developer (`jjrobert` at `ncsu.edu`)
- Rogelio E. Cardona-Rivera: Developer (`recardon` at `ncsu.edu`)
- Allison G. Martinez-Arocho: Game Design
- Ian Coleman: Game Design

## License

The UGME code base (everything under the top level /Assets/ folder that is not in the /Assets/Plugins/ folder) is licensed under the MIT License (see LICENSE.md).  

The dependency LAPKT is licensed under the GNU GPL v3.0 License (see /Dependencies/LAPKT-public/LICENSE.txt).
