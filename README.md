# MonoGame (Net namespace fork)

## This Fork

My fork of MonoGame implements the Net namespace in order to compile networked games/apps that were built for XNA on any platform. I wanted to port my kart-racing game "Seaside Racing" to MonoGame and I decided to reimplement the Net namespace instead of rewriting the networking code in the game. The main reason to use the Net namespace for a new project is that the original API is very elegant, does player management and handles lobby-to-game transitions. The implementation supports multiple backends and it should be straightforward to add support for new platforms. At the moment, the default backend uses the [Lidgren](https://github.com/lidgren/lidgren-network-gen3) library for low level networking tasks and the backend will work on any platform that supports standard .NET sockets.

### TODO
* ~~Master server and NAT introduction for PlayerMatch/Ranked support (can only handle Local and SystemLink/LAN atm)~~
* ~~Numerous small bug fixes~~
* ~~IPEndPoint abstraction~~
* ~~Only connect to end points provided by the host~~
* Clean up host specific state and responsability of internal messages vs Session/NetworkMachine

### Future work
* Host migration
* Cheat protection

## MonoGame

One framework for creating powerful cross-platform games.  The spiritual successor to XNA with 1000's of titles shipped across desktop, mobile, and console platforms.  [MonoGame](http://www.monogame.net/) is a fully managed .NET open source game framework without any black boxes.  Create, develop and distribute your games your way.

## License

The MonoGame project is under the [Microsoft Public License](https://opensource.org/licenses/MS-PL) except for a few portions of the code.  See the [LICENSE.txt](LICENSE.txt) file for more details.  Third-party libraries used by MonoGame are under their own licenses.  Please refer to those libraries for details on the license they use.
