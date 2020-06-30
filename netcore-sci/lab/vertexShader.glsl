// input variables from buffers
attribute vec3 aPos;
attribute vec3 aNormal;

// input variables from code
uniform vec3 uBBox;
uniform mat4 uModel;
uniform mat4 uProjection;
uniform mat4 uView;

// variables flowing through next pipeline ( fragment shader )
varying vec3 FragPos;
varying vec3 VecPos;
varying vec3 Normal;

void main() {
  vec3 mPos = aPos - uBBox / 2;
  gl_Position = uProjection * uView * uModel * vec4(mPos, 1.0);

  FragPos = vec3(uProjection * uView * uModel * vec4(mPos, 1.0));
  VecPos = mPos;
  Normal = normalize(vec3(uModel * vec4(aNormal, 1.0)));
}