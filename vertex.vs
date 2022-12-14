#version 330 core
    layout (location = 0) in vec3 COD;
    layout (location = 1) in vec2 uv;
    layout (location = 2) in vec3 normal;
    out vec3 COD1;
     out vec3 Normal;
      out vec3 WorldPos;
      out vec2 TexCoords;
   
    uniform mat4 view;
    uniform mat4 projection;
    uniform mat4 model;

    
    void main()
    {
    TexCoords=uv;
    Normal=normal;
   WorldPos =COD;
COD1=COD*0.01;
       gl_Position = model*projection*view*vec4(COD1, 1.0);
    }