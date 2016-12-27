# MonoGame (Net namespace fork)

## This Fork

My fork of MonoGame implements the Net namespace in order to compile networked games/apps that were built for XNA on any platform. I wanted to port my kart-racing game "Seaside Racing" to MonoGame and I decided to reimplement the Net namespace instead of rewriting the networking code in the game. The main reason to use the Net namespace for a new project is that the original API is very elegant, handles player management and lobby-to-game transitions. The implementation supports multiple backends and it should be straightforward to add support for new platforms. At the moment, the default backend uses the [Lidgren](https://github.com/lidgren/lidgren-network-gen3) library for low level networking tasks and the backend will work on any platform that supports standard .NET sockets.

### Implementation details
Since a user can choose to use any network topology for their game, the internal implementation uses a peer-to-peer topology as it is the most general case. If the user wants a client/server topology, all traffic can be routed through the designated host or any other peer. This is inline with the original XNA implementation.

The peer that starts a game using the NetworkSession.Create() method becomes the host of the game. The host of the game broadcasts its existence to the master server (if NetworkSessionType is PlayerMatch or Ranked as opposed to Local or SystemLink) and responds to discovery requests. Any peer that connects to a host using the NetworkSession.Join() method becomes an ordinary peer. In the future, host migration could make it possible for ordinary peers to become hosts, but it is not implemented yet.

The master server is responsible for introducing (ie. finding the endpoint and performing NAT punchthrough) a peer to a host of an online game. Once connected to a host, the peer is introduced to the other peers already in the game by the host. Peers only allow introductions/connections from other peers whose endpoints are specified in an allowlist and only allow changes to the allowlist from the host. Once fully connected, the signed in gamers of the peer are able to join. This procedure ensures that the peers in a game are fully connected to each other in order to support true a peer-to-peer topology.

There are 2 caveats of this setup that users must be aware of:
* A peer cannot be behind the same router as the master server since the master server must know the external ip of the peer
* A peer cannot be behind the same router as the host since the host must know the external ip of the peer

In practice, the only problem is that gamers behind the same router will not be able to connect to each other if any of them are designated the host.

### Future work
* Clean up responsibility of internal messages vs Session
* Host migration
* Cheat protection

## MonoGame

One framework for creating powerful cross-platform games.  The spiritual successor to XNA with 1000's of titles shipped across desktop, mobile, and console platforms.  [MonoGame](http://www.monogame.net/) is a fully managed .NET open source game framework without any black boxes.  Create, develop and distribute your games your way.

## License

The MonoGame project is under the [Microsoft Public License](https://opensource.org/licenses/MS-PL) except for a few portions of the code.  See the [LICENSE.txt](LICENSE.txt) file for more details.  Third-party libraries used by MonoGame are under their own licenses.  Please refer to those libraries for details on the license they use.
