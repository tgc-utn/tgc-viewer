xof 0303txt 0032
template ColorRGBA {
 <35ff44e0-6c7c-11cf-8f52-0040333594a3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
 FLOAT alpha;
}

template ColorRGB {
 <d3e16e81-7835-11cf-8f52-0040333594a3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
}

template Material {
 <3d82ab4d-62da-11cf-ab39-0020af71e433>
 ColorRGBA faceColor;
 FLOAT power;
 ColorRGB specularColor;
 ColorRGB emissiveColor;
 [...]
}

template TextureFilename {
 <a42790e1-7810-11cf-8f52-0040333594a3>
 STRING filename;
}

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

template MeshNormals {
 <f6f23f43-7686-11cf-8f52-0040333594a3>
 DWORD nNormals;
 array Vector normals[nNormals];
 DWORD nFaceNormals;
 array MeshFace faceNormals[nFaceNormals];
}

template MeshMaterialList {
 <f6f23f42-7686-11cf-8f52-0040333594a3>
 DWORD nMaterials;
 DWORD nFaceIndexes;
 array DWORD faceIndexes[nFaceIndexes];
 [Material <3d82ab4d-62da-11cf-ab39-0020af71e433>]
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


Material PDX07_-_Default {
 1.000000;1.000000;1.000000;1.000000;;
 3.200000;
 0.000000;0.000000;0.000000;;
 0.000000;0.000000;0.000000;;

 TextureFilename {
  "discor.jpg";
 }
}

Mesh Box01 {
 24;
 -2.993547;-4.583536;0.447885;,
 2.993547;-4.583536;0.447885;,
 -2.993547;5.747107;0.447884;,
 2.993547;5.747107;0.447884;,
 -2.993547;-5.747107;-0.447884;,
 2.993547;-5.747107;-0.447884;,
 -2.993547;5.747107;-0.447885;,
 2.993547;5.747107;-0.447885;,
 2.993547;-5.747107;-0.447884;,
 -2.993547;-4.583536;0.447885;,
 -2.993547;-5.747107;-0.447884;,
 2.993547;-4.583536;0.447885;,
 2.993547;5.747107;-0.447885;,
 2.993547;-4.583536;0.447885;,
 2.993547;-5.747107;-0.447884;,
 2.993547;5.747107;0.447884;,
 -2.993547;5.747107;-0.447885;,
 2.993547;5.747107;0.447884;,
 2.993547;5.747107;-0.447885;,
 -2.993547;5.747107;0.447884;,
 -2.993547;-5.747107;-0.447884;,
 -2.993547;5.747107;0.447884;,
 -2.993547;5.747107;-0.447885;,
 -2.993547;-4.583536;0.447885;;
 12;
 3;3,0,1;,
 3;0,3,2;,
 3;7,4,6;,
 3;4,7,5;,
 3;8,9,10;,
 3;9,8,11;,
 3;12,13,14;,
 3;13,12,15;,
 3;16,17,18;,
 3;17,16,19;,
 3;20,21,22;,
 3;21,20,23;;

 MeshNormals  {
  6;
  0.000000;0.000000;1.000000;,
  0.000000;-0.000000;-1.000000;,
  0.000000;-0.610016;0.792389;,
  1.000000;0.000000;0.000000;,
  0.000000;1.000000;-0.000000;,
  -1.000000;0.000000;0.000000;;
  12;
  3;0,0,0;,
  3;0,0,0;,
  3;1,1,1;,
  3;1,1,1;,
  3;2,2,2;,
  3;2,2,2;,
  3;3,3,3;,
  3;3,3,3;,
  3;4,4,4;,
  3;4,4,4;,
  3;5,5,5;,
  3;5,5,5;;
 }

 MeshMaterialList  {
  1;
  12;
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0;
  { PDX07_-_Default }
 }

 MeshTextureCoords  {
  24;
  0.005033;0.624769;,
  0.005033;0.282823;,
  0.985455;0.624769;,
  0.983756;0.303209;,
  0.985455;0.994967;,
  0.985455;0.653021;,
  0.005033;0.994967;,
  0.005033;0.653021;,
  0.573233;0.203410;,
  0.005033;0.254571;,
  0.005033;0.203410;,
  0.573233;0.254571;,
  0.985455;0.007549;,
  0.005033;0.058710;,
  0.005033;0.007549;,
  0.985455;0.058710;,
  0.586123;0.079482;,
  0.017922;0.130643;,
  0.017922;0.079482;,
  0.586123;0.130643;,
  0.985455;0.138123;,
  0.005033;0.189284;,
  0.005033;0.138123;,
  0.985455;0.189284;;
 }
}