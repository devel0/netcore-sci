varying vec3 FragPos;
varying vec3 VecPos;
varying vec3 Normal;
uniform vec3 LightPos;
// DECLAREGLFRAG

void main() {
  float y = VecPos.y;
  // float c = cos(atan(VecPos.x, VecPos.z) * 20.0 + uTime * 40.0 + y * 50.0);
  // float s = sin(-atan(VecPos.z, VecPos.x) * 20.0 - uTime * 20.0 - y * 30.0);
  
  vec3 objectColor = vec3(1, 0, 0);
  //objectColor = objectColor * (1.0 - uDisco) + discoColor * uDisco;

  float ambientStrength = 0.3;
  vec3 lightColor = vec3(1.0, 1.0, 1.0);
  vec3 lightPos = LightPos;// vec3(0, 0, 0); //uMaxY * 2.0, uMaxY * 2.0, uMaxY * 2.0);
  vec3 ambient = ambientStrength * lightColor;

  vec3 norm = normalize(Normal);
  vec3 lightDir = normalize(lightPos - FragPos);

  float diff = max(dot(norm, lightDir), 0.0);
  vec3 diffuse = diff * lightColor;

  vec3 result = (ambient + diffuse) * objectColor;
  gl_FragColor = vec4(result, 1.0);
}