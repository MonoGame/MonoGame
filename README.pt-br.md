# MonoGame

MonoGame é um framework .NET simples e poderoso feito para criar jogos para desktop PCs, consoles de videogame, e dispositivos móveis usando a linguagem de programação C#. MonoGame vem sendo usado de modo bem sucedido para criar jogos como [Streets of Rage 4](https://store.steampowered.com/app/985890/Streets_of_Rage_4/), [Carrion](https://store.steampowered.com/app/953490/CARRION/), [Celeste](https://store.steampowered.com/app/504230/Celeste/), [Stardew Valley](https://store.steampowered.com/app/413150/Stardew_Valley/), e [muitos outros](https://www.monogame.net/showcase/).

MonoGame é uma reimplementação de código aberto do [Microsoft's XNA Framework](https://msdn.microsoft.com/en-us/library/bb200104.aspx), que foi descontinuado.

[![Junte-se a nós em https://discord.gg/monogame](https://img.shields.io/discord/355231098122272778?color=%237289DA&label=MonoGame&logo=discord&logoColor=white)](https://discord.gg/monogame)

* [Status de Compilação](#build-status)
* [Plataformas Suportadas](#supported-platforms)
* [Suporte e Contribuições](#support-and-contributions)
* [Código Fonte](#source-code)
* [Links Úteis](#helpful-links)
* [Licença](#license)

## Status de Compilação

Usamos [GitHub Actions](https://github.com/MonoGame/MonoGame/actions) para automatizar a construção e distribuição de pacotes das últimas alterações do MonoGame. Nós também contamos com um [servidor de compilação](http://teamcity.monogame.net/?guest=1) para fazer testes de modo a evitar regressões.  A tabela abaixo mostra o status atual de construção do ramo de desenvolvimento de nome ```develop```.

| Nome                            | Status                                                                                                                                                                                         |
|:------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Compilação                      | [![Compilação](https://github.com/MonoGame/MonoGame/actions/workflows/main.yml/badge.svg?branch=develop)](https://github.com/MonoGame/MonoGame/actions/workflows/main.yml)                          |
| Testes do Windows               | [![Status de Compilação](http://teamcity.monogame.net/app/rest/builds/buildType:MonoGame_TestWindows/statusIcon)](http://teamcity.monogame.net/viewType.html?buildTypeId=MonoGame_TestWindows&guest=1) |
| Testes do Mac                   | [![Status de Compilação](http://teamcity.monogame.net/app/rest/builds/buildType:MonoGame_TestMac/statusIcon)](http://teamcity.monogame.net/viewType.html?buildTypeId=MonoGame_TestMac&guest=1)         |

## Plataformas Suportadas

Nós oferecemos suporte a uma crescente lista de como computadores desktop, dispositivos móveis e consoles de videogame.  Se houver alguma plataforma que não oferecemos suporte, por favor [faça uma solicitação](https://github.com/MonoGame/MonoGame/issues) ou [venha nos ajudar](CONTRIBUTING.md) a implementá-la.

* Computadores Desktop
  * Windows 8.1 e superiores (OpenGL & DirectX)
  * Windows Store Apps (UWP)
  * Linux (OpenGL)
  * macOS 10.15 e superiores (OpenGL)
* Dispositivos Móveis e Tablets
  * Android 6.0 e superiores (OpenGL)
  * iPhone/iPad 10.0 e superiores (OpenGL)
* Consoles (apenas para desenvolvedores registrados)
  * PlayStation 4
  * PlayStation 5
  * Xbox One (ambos UWP e XDK)
  * Nintendo Switch
  * Google Stadia

## Suporte e Contribuição

Se você acha que encontrou um bug ou deseja solicitar um novo recurso, use nosso [rastreador de solicitações](https://github.com/MonoGame/MonoGame/issues). Antes de abrir uma nova solicitação, por favor, procure para ver se seu problema já não foi reportado.  Tente ser o mais detalhado possível em sua solicitação.

Se você precisa de ajuda com o uso do MonoGame ou tem alguma outra questão, sugerimos que faça uma postagem no [fórum da comunidade](http://community.monogame.net).  Por favor, não use o rastreador de solicitações do GitHub para solicitações de suporte pessoal.

Se você estiver interessado em contribuir com correções ou novos recursos para o MonoGame, por favor leia nosso [guia do colaborador](CONTRIBUTING.md) primeiro.

### Assinatura

Caso queira contribuir com o projeto financeiramente, considere nos apoiar com uma assinatura mensal com o preço de um café.

O dinheiro é usado para hospedagem, novos equipamentos e, caso um número suficiente de pessoas assinem, para a contratação de um desenvolvedor dedicado.

Existem várias opções de doação em nossa [Página de Doações](http://www.monogame.net/donate/).

## Código Fonte

O código fonte completo está disponível aqui no GitHub:

* Clone o código fonte: `git clone https://github.com/MonoGame/MonoGame.git`
* Configure os submódulos: `git submodule update --init`
* Abra a solução da plataforma alvo para construir o framework do jogo.
* Abra a solução de Ferramentas da plataforma de desenvolvimento para construir as ferramentas de pipeline e de conteúdo.

Os pré-requisitos para construção a partir do código fonte podem ser encontrados no arquivo [Requisitos](REQUIREMENTS.md).

Uma divisão de alto nível dos componentes do framework:

* O framework do jogo pode ser encontrado em [MonoGame.Framework](MonoGame.Framework).
* O pipeline de conteúdo está localizado em [MonoGame.Framework.Content.Pipeline](MonoGame.Framework.Content.Pipeline).
* Os modelos de projetos estão em [Modelos](Templates).
* Veja [Testes](Tests) para testes unitários do framework.
* Veja [Ferramentas/Testes](Tools/MonoGame.Tools.Tests) para os testes do pipeline de conteúdo e outras ferramentas..
* [mgcb](Tools/MonoGame.Content.Builder) é a ferramenta de linha de comando para processamento de conteúdo.
* [mgfxc](Tools/MonoGame.Effect.Compiler) é a ferramenta de compilar efeitos através da linha de comando.
* O [Editor MGCB](Tools/MonoGame.Content.Builder.Editor) é uma interface gráfica para o processamento de conteúdo.

## Links Úteis

* O site oficial é [monogame.net](http://www.monogame.net).
* Nosso [rastreador de solicitações](https://github.com/MonoGame/MonoGame/issues) está no GitHub.
* Use os [fóruns da comunidade](http://community.monogame.net/) para perguntas e suporte.
* Você pode [entrar nosso servidor Discord](https://discord.gg/monogame) e conversar ao vivo com os principais desenvolvedores e os demais usuários.
* A [documentação oficial](http://www.monogame.net/documentation/) está no nosso website.
* Baixe aqui os [pacotes](http://www.monogame.net/downloads/) de lançamento e desenvolvimento.
* Siga [@MonoGameTeam](https://twitter.com/monogameteam) no X/Twitter.

## Licença

O projeto MonoGame está sob a [Licença Pública da Microsoft](https://opensource.org/licenses/MS-PL) com a exceção de algumas partes do código.  Consulte o arquivo [LICENSE.txt](LICENSE.txt) para mais detalhes.  As bibliotecas de terceiros usadas pelo MonoGame são regidas por suas próprias licenças. Consulte essas bibliotecas para obter detalhes sobre as licenças que elas usam.


