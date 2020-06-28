attribute vec3 aPos;
attribute vec3 aNormal;
uniform mat4 uModel;
uniform mat4 uProjection;
uniform mat4 uView;

varying vec3 FragPos;
varying vec3 VecPos;
varying vec3 Normal;
uniform float uTime;
uniform float uDisco;
void main() {
  float discoScale = sin(uTime * 10.0) / 10.0;
  float distortionX = 1.0 + uDisco * cos(uTime * 20.0) / 10.0;

  float scale = 1.0 + uDisco * discoScale;

  vec3 scaledPos = aPos;
  scaledPos.x = scaledPos.x * distortionX;

  scaledPos *= scale;
  gl_Position = uProjection * uView * uModel * vec4(scaledPos, 1.0);
  FragPos = vec3(uModel * vec4(aPos, 1.0));
  VecPos = aPos;
  Normal = normalize(vec3(uModel * vec4(aNormal, 1.0)));
}