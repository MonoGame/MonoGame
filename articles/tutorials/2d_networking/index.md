# MonoGame 2D Advanced Series - Networking and Leaderboards

Following the [MonoGame 2D Beginner Tutorial](https://docs.monogame.net/articles/tutorials/building_2d_games/), this guide expands your single-player game into a multiplayer experience with online leaderboards.

## 1. Choosing a Networking Library
While MonoGame does not have a built-in high-level networking API, the community relies on robust, free, open-source libraries. For this tutorial, we will use **LiteNetLib**. It is reliable, free, and handles UDP packet delivery, fragmentation, and channels efficiently.
*Reason for use:* It abstracts the low-level socket programming, allowing us to focus on game logic while remaining completely free for commercial and non-commercial use.

## 2. Basic Multiplayer Setup
### Server/Client Architecture
We will use a listen-server model (where one player acts as the host and also plays the game).
- **Host (Server):** Runs the game simulation and accepts connections.
- **Client:** Connects to the host, sends inputs, and receives game state updates.

### Implementation Steps
1. Initialize `NetManager` on both Host and Client.
2. Define `INetEventListener` to handle `OnPeerConnected`, `OnNetworkReceive`, etc.
3. Start the Host on port `9050`.
4. Connect the Client to the Host's IP.

## 3. Networked Gameplay
### State Synchronization
For a simple 2D game, **State Synchronization** is often easiest.
- The host sends the positions and states of all entities 20-30 times a second.
- Clients receive these packets and update their local representations.

### Interpolation and Smoothing
Because network packets arrive irregularly, directly setting a remote player's position will cause jitter.
```csharp
// Simple Lerp for smoothing remote player movement
Vector2 targetPosition = receivedPacket.Position;
remotePlayer.Position = Vector2.Lerp(remotePlayer.Position, targetPosition, 0.2f);
```

## 4. Creating a Basic Lobby Screen
Before gameplay, players need a place to connect.
1. **Main Menu:** Buttons for "Host Game" and "Join Game".
2. **Host Screen:** Displays the local IP address and a "Start Game" button.
3. **Join Screen:** A text input for the Host's IP address and a "Connect" button.
4. **Lobby:** Displays connected players. Once the Host clicks "Start", a network packet is broadcast to all clients to transition to the `GameplayScreen`.

## 5. Adding a Free Leaderboard
To track high scores, we can use **Microsoft PlayFab** or **Firebase**. Both offer generous free tiers suitable for indie games.
### Using PlayFab (Example)
1. Create a free PlayFab account and set up a new Title.
2. Use the PlayFab SDK or simple HTTP REST API calls to submit scores.
```csharp
// Example REST call to update leaderboard
var request = new UpdatePlayerStatisticsRequest {
    Statistics = new List<StatisticUpdate> {
        new StatisticUpdate { StatisticName = "HighScore", Value = player.Score }
    }
};
PlayFabClientAPI.UpdatePlayerStatistics(request, OnSuccess, OnError);
```
3. Fetch the leaderboard using `GetLeaderboard` and display it on a `LeaderboardScreen`.

## 6. Advanced Considerations
As you expand your game, you will need to research:
- **Lag Compensation:** Rewinding server state to accurately register hits for players with high ping.
- **Client-Side Prediction:** Allowing the local player to move immediately without waiting for server confirmation, then reconciling if the server disagrees.
- **Proxies & NAT Punchthrough:** If players are behind strict firewalls, you may need a relay server or NAT punchthrough techniques (LiteNetLib has some built-in NAT punchthrough support).

## 7. Further Reading
- [LiteNetLib GitHub Repository](https://github.com/RevenantX/LiteNetLib)
- [Game Networking Resources (gafferongames.com)](https://www.gafferongames.com/categories/networked-physics/)
- [PlayFab Documentation](https://docs.microsoft.com/en-us/gaming/playfab/)
