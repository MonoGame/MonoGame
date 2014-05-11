"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core required");

var $jsilmg = JSIL.DeclareAssembly("MonoGame.Web");

var $sdlasms = new JSIL.AssemblyCollection({
    2: "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
    4: "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
});

JSIL.DeclareNamespace("MonoGame");
JSIL.DeclareNamespace("MonoGame.Web");

JSIL.ImplementExternals("MonoGame.Web.JSAPI", function ($interfaceBuilder) {
    var $ = $interfaceBuilder;

    $.Method({ Static: false, Public: true, Virtual: true }, "Test",
      new JSIL.MethodSignature($.String, [], []),
      function Test() {
          return "hello";
      }
    );
});
