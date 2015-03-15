"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core required");

var $jsilmg = JSIL.DeclareAssembly("MonoGame.Web");

var $sdlasms = new JSIL.AssemblyCollection({
    2: "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
    4: "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
});

var gameState = {
    callbackObj: null,
    timeout: null,
    callback: null,
    canvas: null,
    gl: null
};

JSIL.DeclareNamespace("MonoGame");
JSIL.DeclareNamespace("MonoGame.Web");

JSIL.ImplementExternals("MonoGame.Web.JSAPI", function ($interfaceBuilder) {
    var $ = $interfaceBuilder;

    var $mgasm = JSIL.GetAssembly("MonoGame.Framework");

    $.Method({ Static: false, Public: true, Virtual: false }, "GamePlatformStartRunLoop",
      new JSIL.MethodSignature(null, [$mgasm.TypeRef("Microsoft.Xna.Framework.IHasCallback")], []),
      function (callbackObj) {
          gameState.callbackObj = callbackObj;
          gameState.callback = function() {
              gameState.callbackObj.Callback();
              gameState.timeout = window.setTimeout(gameState.callback, 1);
          };
          gameState.timeout = window.setTimeout(gameState.callback, 1);
      }
    );

    $.Method({ Static: false, Public: true, Virtual: false }, "GraphicsDevicePlatformSetup",
      new JSIL.MethodSignature(null, [], []),
      function() {
          var canvas = document.createElement("canvas");
          canvas.style.display = "block";
          canvas.style.position = "absolute";
          canvas.style.left = "0px";
          canvas.style.top = "0px";
          canvas.style.width = "100%";
          canvas.style.height = "100%";
          canvas.id = "gameCanvas";
          document.body.appendChild(canvas);

          gameState.canvas = canvas;
          gameState.canvas.width = "640";
          gameState.canvas.height = "480";

          try {
              gameState.gl = canvas.getContext("webgl") || canvas.getContext("experimental-webgl");
          } catch (e) {
          }

          if (!gameState.gl) {
              alert("Unable to initialize WebGL.  Your browser may not support it.");
              gameState.gl = null;

              throw "Unable to initialize WebGL!";
          }
      }
    );

    $.Method({ Static: false, Public: true, Virtual: false }, "GraphicsDevicePlatformClear",
      new JSIL.MethodSignature(null, [], []),
      function () {
          // TODO Finish this
          gameState.gl.clearColor(1, 0, 0, 1);
          gameState.gl.clear(gameState.gl.COLOR_BUFFER_BIT | gameState.gl.DEPTH_BUFFER_BIT);
      }
    );
    
});
