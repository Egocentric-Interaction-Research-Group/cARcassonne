# AI agent player implementation
This document indicates how the cARcassonne AI agent is connected to the existing code base in the cARcassonne repository, and changes that have been made to this code base to accommodate the functionality needed by the AI agent.
For information about the actual AI agent code and how the AI functions, see the following document from the training environment:
https://github.com/edvinbrus1/Training-Environment-Carcassonne/blob/main/Docs/ai_agent.md

## Playing with the AI agent
It is very easy to start a game as the AI agent player. This is simply done by executing the project in Unity (pressing Play) and, before pressing the *Start game*-button, clicking on the Player object in the scene hierarchy and ticking the *controlledByAI* box. Pressing *Start game* after this will let the AI agent control your player, and it will play a game by itself by continuously placing tiles and meeples. If one wants to do several tests with the AI agent playing, it is suggested to change the *controlledByAI*-variable in the *PlayerScript* to true, as this will result in the AI agent being the default setting when starting the scene.
Currently, the AI agent is not tested in multiplayer neither against human players or other AI agents, as the multiplayer functionality is non-functioning at this time. It is also not possible to play several AI agents, nor a human and an AI agent, on the same computer.
To change the agent behaviour itself, a new agent would have to be trained. To do this, please refer to the documentation above and the repository in which it resides, which is the training environment. Once a new model (*.onnx) is trained, it can replace the current model which can be found in the *Behaviour Parameters* in the *AI* prefab under the name *Model*.

## Code structure of the AI agent
The AI agent is split into three separate classes. The *AIDecisionMaker*-class decides when the AI agent should and should not act, as well as makes static decisions for the AI agent, such as drawing a tile when the turn starts. The *CarcassonneAgent*-class is the actual agent, which makes decisions based on the current observations and the previous training it has had. Neither of these two classes interact with the code base at all. They only interact with one another and the third class, *AIWrapper*, which implements the *InterfaceAIWrapper*-interface to connect the AI agent to the game code. This means that all observations and decisions made by the AI agent are filtered through the *AIWrapper*-class, which contains the logic to make the decided AI agent actions happen in the code, as well as gathering the information from the code that the AI agent needs to observe.

### The AIWrapper class
The *AIWrapper*-class contains the methods needed by the AI agent to function. This includes actions the AI agent can take and variables it needs to observe. The interface methods are commented to indicate how they are used by the AI agent. The methods returning informational data used by the AI agent to observe its environment, upon which it bases its decisions, are simple. They return the data that the AI agents need to observe to understand the game board.
Most of the methods are self explanatory due to their names and have additional comments in the code if needed. The *Reset*-method is not used at all in the real environment currently, as there is currently no scenario where the AI agent should reset the game automatically. Some may however be less intuitive. Two of these are the methods for placing a tile or a meeple:
´´´
        public void PlaceTile(int x, int z)
        {
            controller.iTileAimX = x;
            controller.iTileAimZ = z;
            controller.ConfirmPlacementRPC();
        }

        public void PlaceMeeple(Direction meepleDirection)
        {
            float meepleX = 0.000f;
            float meepleZ = 0.000f;
            
            //If clause only changes X if it is east or west.
            if (meepleDirection == Direction.EAST)
            {
                meepleX = 0.011f;
            }
            else if (meepleDirection == Direction.WEST)
            {
                meepleX = -0.011f;
            }
            
            //If clause only changes Z if it is north or south
            if (meepleDirection == Direction.NORTH)
            {
                meepleZ = 0.011f;
            }
            else if (meepleDirection == Direction.SOUTH)
            {
                meepleZ = -0.011f;
            }

            controller.meepleControllerScript.aiMeepleX = meepleX;
            controller.meepleControllerScript.aiMeepleZ = meepleZ;
            controller.ConfirmPlacementRPC();
        }
´´´

As can be seen in the code, these two methods explicitly sets values of variables in the *GameControllerScript*-class and the *MeepleControllerScript*-class respectively. These variables are directly tied to where the object will be placed, and are normally set by a human player physically moving the tile or the meeple. Due to some tight connections between physical movement and the variables in the code, some additional code is needed in order for this explicit setting of variables to have an effect. This has been implemented in the *GameControllerScript*-class alongside the regular code used for human players, and is discussed further in the *Code changes*section.

### Workarounds
The *AIWrapper*-class and some functions created for the AI agent to call currnetly contain workarounds for some AI-specific problems in the normal code. These can safely be removed if the issues in the underlying game code are fixed to correctly incorporate the needed functionaly for the AI agent behavior. The clearest example of this is the *FreeCurrentMeeple*-method in the *AIWrapper*-class. Here, a meeple is explicitly freed. This only happens when the AI agent tries to confirm the current meeple placement, but the meeple neither gets placed or returns, which in code is represented by the phase remaining the same rather than going to the next or previous phase.
This occurs due to a bug where the game does not correctly free a meeple when the placement is legal in itself, but some other meeple already occupies the road or city from another tile. A human player can simply move the meeple off the tile and return it, but the AI agent has no choice to place a meeple outside of the tile, and thus the meeple is freed explicitly.
Similar workarounds, also relating to meeples, can be found in the changes in the *GameControllerScript* described below. As the game code is very strictly bound to the movement and positions of the meeple objects, the game meeple has to be explicitly moved to the physical position that represents the AI agent's choice before attempting to place it. This could be removed if the strong bond between physical placement and logical placement is broken in future updates.

## Code changes
The game code remains virtually intact. No changes have been made to the existing code that would affect human players. Very minor changes, for example changing the *iTileAimX* and *iTileAimZ* variables in the *GameControllerScript* from private to public, have taken place, but mainly the existing code has been extended in order to incorporate the needed AI agent functionality.

### GameControllerScript
The *GameControllerScript* has mainly had two changes. The first is the placement of the tiles and meeples, mainly relating to the *ConfirmPlacement*-method and its RPC counterpart. In the RPC call, a check is made if the calling agent is human or AI controlled. If it is an AI agent, it instead calls *ConfirmPlacementAI* for all players, which takes the earlier mentioned explicitly set variables as input arguments. This method properly sets up the tile or meeple without requiring the normal physical movement from human players, and then calls the normally used *ConfirmPlacement* method. This method has received only a very minor change, resulting in the raycasting of a tile only being used if it is a human player, as an AI agent does not physically move the tile.
The other notable change in this script is the *UpdateBoundary*-method, which is only used by the AI agent to update its limited allowed space for placing tiles. This simply sets a boundary of how far off the board the AI agent can move its tile before being reset to the center of the board. This limit is currently one tile away from the furthest placed tile in each direction, which is the maximum distance from the center in that direction at which a tile can be legally placed.
###PlayerScript
The *PlayerScript* has also received some minor changes, such as changing the score to public for rewards and adding a boolean for whether or not the player is controlled by an AI agent or not. It also contains functionality for returning the number of meeples left and total meeples, used for observations and normalization.
Additionally, the *PlayerScript* instantiates, if the *controlledByAI* boolean is true, the AI-prefab that contains the actual AI agent. This instantiation is what starts the process of an AI agent playing the game, as the AI-prefab contains the *AIDecisionMaker* and *CarcassonneAgent* scripts, which in turn creates the *AIWrapper*.

## The AI Prefab
The AI prefab object is what contains an AI agent during gameplay. This object is instantiated by the *PlayerScript* if the *controlledByAI* variables is true when the *Start Game* button is pressed. The prefab is placed as a child to the *Player* object in the hierarchy.
This AI Prefab currently contains a trained model to place tiles. This model can be replaced by any other trained model that matches the current *CarcassonneAgent* script by dragging it into the *Model* variable in the *Behaviour Parameters* in the AI prefab. Note that the other variables on the AI prefab do not need to be changed as they are only used for training, which should be done in the separate training environment.
