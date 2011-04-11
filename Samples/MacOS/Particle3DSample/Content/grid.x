xof 0303txt 0032
template Vector {
 <3d82ab5e-62da-11cf-ab39-0020af71e433>
 FLOAT x;
 FLOAT y;
 FLOAT z;
}

template MeshFace {
 <3d82ab5f-62da-11cf-ab39-0020af71e433>
 DWORD nFaceVertexIndices;
 array DWORD faceVertexIndices[nFaceVertexIndices];
}

template Mesh {
 <3d82ab44-62da-11cf-ab39-0020af71e433>
 DWORD nVertices;
 array Vector vertices[nVertices];
 DWORD nFaces;
 array MeshFace faces[nFaces];
 [...]
}

template Coords2d {
 <f6f23f44-7686-11cf-8f52-0040333594a3>
 FLOAT u;
 FLOAT v;
}

template MeshTextureCoords {
 <f6f23f40-7686-11cf-8f52-0040333594a3>
 DWORD nTextureCoords;
 array Coords2d textureCoords[nTextureCoords];
}


Mesh {
 4;
 640.00000;0.000000;-640.00000;,
 -640.00000;0.000000;-640.00000;,
 -640.00000;0.000000;640.00000;,
 640.00000;0.000000;640.00000;;
 2;
 3;0,1,2;,
 3;0,2,3;;

 MeshTextureCoords {
  4;
  0.000000;0.000000;,
  0.000000;32.000000;,
  32.000000;32.000000;,
  32.000000;0.000000;;
 }

 MeshMaterialList {
  1;
  2;
  0,
  0;

  Material {
   0.800000;0.800000;0.800000;1.000000;;
   1.000000;
   0.000000;0.000000;0.000000;;
   0.000000;0.000000;0.000000;;

   TextureFilename {
    "checker.bmp";
   }
  }
 }
}