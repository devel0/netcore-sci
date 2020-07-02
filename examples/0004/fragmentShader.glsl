// variables coming from vertex shader
varying vec3 FragPos;
varying vec3 VecPos;
varying vec3 Normal;

// variables coming from code
uniform float uMaxY;
uniform float uMinY;
uniform float uTime;
uniform float uDisco;

// DECLAREGLFRAG

void main() {
  float y = (VecPos.y - uMinY) / (uMaxY - uMinY);
  float c = cos(atan(VecPos.x, VecPos.z) * 20.0 + uTime * 40.0 + y * 50.0);
  float s = sin(-atan(VecPos.z, VecPos.x) * 20.0 - uTime * 20.0 - y * 30.0);

  vec3 discoColor =
      vec3(0.5 + abs(0.5 - y) * cos(uTime * 10.0),
           0.25 + (smoothstep(0.3, 0.8, y) * (0.5 - c / 4.0)),
           0.25 + abs((smoothstep(0.1, 0.4, y) * (0.5 - s / 4.0))));

  vec3 objectColor = vec3((1.0 - y), 0.40 + y / 4.0, y * 0.75 + 0.25);
  objectColor = objectColor * (1.0 - uDisco) + discoColor * uDisco;

  float ambientStrength = 0.3;
  vec3 lightColor = vec3(1.0, 1.0, 1.0);
  vec3 lightPos = vec3(uMaxY * 2.0, uMaxY * 2.0, uMaxY * 2.0);
  vec3 ambient = ambientStrength * lightColor;

  vec3 norm = normalize(Normal);
  vec3 lightDir = normalize(lightPos - FragPos);

  float diff = max(dot(norm, lightDir), 0.0);
  vec3 diffuse = diff * lightColor;

  vec3 result = (ambient + diffuse) * objectColor;
  gl_FragColor = vec4(result, 1.0);
}
