Player1 Hosts a LAN Game:
 Creates a shared and local game state using the BaseBuilder.Engine.Logic.WorldGen package
 Creates a ServerGameConnection and allows the OS to specify a port and passes the shared game state to it
 Creates a new SharedGameLogic passing nothing to it (alternatively, load some configuration options such as expected latency and pass those to it)
 Creates a new LocalGameLogic passing nothing to it (alternatively, load some configuration options such as camera speed and pass those to it)
 Creates a new game screen and passes the shared game logic, local game logic, shared game state, local game state, and game connection to it.

Player1 moves his camera around:
 The game screen regularly calls LocalGameLogic.Update
 LocalGameLogic.Update polls the keyboard and mouse state. When appropriate, modifies the LocalGameState which contains the Camera
  * Notice that NO TIME IS PASSING during any of the above function calls, but the local game state is changing. This is because only
    the game connection is allowed to simulate time passing (which is handled in SharedGameLogic). To add confusion, the local game
    state will often request the real delta time passed in order to do things like move the camera. However, that will not have any
    relation to the shared game state server time.

Player1 orders an entity to move
 The game screen regularly calls LocalGameLogic.Update
 
  LocalGameLogic.Update polls the keyboard and mouse state. When appropriate, the local game logic notes that the player had a unit selected and then
  right clicked somewhere which results in ordering that unit to move. The local game logic would then queue that order onto the local game logic.
  Note that the LocalGameLogic can only modify the LocalGameState. It is up to the GameConnection to convert those local orders to shared orders
  and sync them as appropriate. Once issued, an order cannot be cancelled except by a new order.
 
 The game screen regularly calls GameConnection.Update
 
  GameConnection.Update will occassionally call SharedGameLogic.SimulateTimePassing when it can be certain all player local orders for some period of time
  have been acquired for all players and all players agree on those orders. It will furthermore require that all players agree on the amount of time
  to simulate passing. Since player1 has issued a move order, all players (in this case, just Player1) will take that shared order and move it to
  the appropriates unit UnitOrders. When in SharedGameLogic.SimulateTimePassing UnitOrders are given the opportunity to effect the game state, so long
  as they effect it in a consistent way.

Player2 connects:
 Uses NetworkingUtils to find the LAN game.
 Creates a new SharedGameLogic passing nothing to it
 Creates a new LocalGameLogic passing nothing to it
 
 Create a ClientGameConnection to the specified game passing the SharedGameLogic and LocalGameLogic to it
 Waits for the ClientGameConnection to finish, which returns a SharedGameState
   Player1 (the host) issues a local order to pause the game and display a popup saying "Player connecting..."
   As describes above, that local order will eventually propagate to the shared game state
   Player1 will send Player2 the entire SharedGameState NOT including Player2 (so it matches everyone elses state)
   Player1 will issue a local order to add Player2 to the game and resume the game.
   
    Player2 will take part in ticking the game locally at this point, but will continue to wait until he has been added to the game
    (which won't happen until Player1's local order propagates). This is why we need the SharedGameLogic and LocalGameLogic
  
 Creates a LocalGameState, passing nothing to it
 Creates a new game screen and passes the shared game logic, local game logic, shared game state, local game state, and game connection to it.