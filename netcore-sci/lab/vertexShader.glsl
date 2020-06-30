// input variables
attribute vec3 aPos;
attribute vec3 aNormal;

// variables controlled by code
//uniform mat4 uModelTr;
uniform mat4 uModel;
uniform mat4 uProjection;
uniform mat4 uView;

// variables flowing through next pipeline ( fragment shader )
varying vec3 FragPos;
varying vec3 VecPos;
varying vec3 Normal;

void main() {
  gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);

  FragPos = vec3(uModel * vec4(aPos, 1.0));
  VecPos = aPos;
  Normal = normalize(vec3(uModel * vec4(aNormal, 1.0)));
}