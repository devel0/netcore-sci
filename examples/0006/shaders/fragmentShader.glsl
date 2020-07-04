// variables coming from vertex shader
varying vec3 FragPos;
varying vec3 VecPos;
varying vec3 Normal;

// variables coming from code
uniform float Amb;
uniform vec3 ObjCol;
uniform vec3 LightPos;

// DECLAREGLFRAG

void main() {
  vec3 objectColor = ObjCol;
  vec3 lightColor = vec3(1.0, 1.0, 1.0);

  vec3 ambient = Amb * lightColor;

  vec3 norm = normalize(Normal);
  vec3 lightDir = normalize(LightPos - FragPos);

  float diff = max(dot(norm, lightDir), 0.0);
  vec3 diffuse = diff * lightColor;

  vec3 result = (ambient + diffuse) * objectColor;
  gl_FragColor = vec4(result, 1.0);
}