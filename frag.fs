#version 330 core
out vec4 FragColor;
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

uniform vec3 albedo;


uniform vec3 camPos;
uniform int temp;
uniform samplerCube cubeMapTex; 
const float PI = 3.14159265359;
const float metallic = 1.0f;
const float roughness =1.0f;
float chiGGX(float v)
{
    return v > 0 ? 1.0 : 0.0;
}
float GGX_Distribution(vec3 n, vec3 h, float alpha)
{
    float NoH = dot(n,h);
    float alpha2 = alpha * alpha;
    float NoH2 = NoH * NoH;
    float den = NoH2 * alpha2 + (1 - NoH2);
    return (chiGGX(NoH) * alpha2) / ( PI * den * den );
}
float chiGGX1(float v)
{
    return v > 0 ? 1.0 : 0.0;
}
float GGX_PartialGeometryTerm(vec3 v, vec3 n, vec3 h, float alpha)
{
    float VoH2 = max(dot(v,h),0.0);
    float chi = chiGGX1( VoH2 / max(dot(v,n),0.0) );
    VoH2 = VoH2 * VoH2;
    float tan2 = ( 1 - VoH2 ) / VoH2;
    return (chi * 2) / ( 1 + sqrt( 1 + alpha * alpha * tan2 ) );
}

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}


float RadicalInverse_VdC(uint bits) 
{
     bits = (bits << 16u) | (bits >> 16u);
     bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
     bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
     bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
     bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
     return float(bits) * 2.3283064365386963e-10; 
}

vec2 Hammersley(uint i, uint N)
{
	return vec2(float(i)/float(N), RadicalInverse_VdC(i));
}

vec3 ImportanceSampleGGX(vec2 Xi, vec3 N, float roughness)
{
	float a = roughness*roughness;

	float phi = 2.0 * PI * Xi.x;
	float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a*a - 1.0) * Xi.y));
	float sinTheta = sqrt(1.0 - cosTheta*cosTheta);
	
	vec3 H;
	H.x = cos(phi) * sinTheta;
	H.y = sin(phi) * sinTheta;
	H.z = cosTheta;
	vec3 up          = abs(N.z) < 0.999 ? vec3(0.0, 0.0, 1.0) : vec3(1.0, 0.0, 0.0);
	vec3 tangent   = normalize(cross(up, N));
	vec3 bitangent = cross(N, tangent);
	
	vec3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
	return normalize(sampleVec);
}

void main()
{		
    vec3 N = normalize(Normal);
    vec3 V = normalize(camPos - WorldPos);  
   
  
vec3 specular=vec3(0.0f);
    vec3 I= -V;
    vec3 R = reflect(I,N);
       vec3 envColor=texture(cubeMapTex,N).rgb;
  vec3 F0 = albedo;
  vec3 L ;
vec3 kD;
vec3 kS=vec3(0.0f);
const uint SAMPLE_COUNT = 64u;
for(uint i =0u; i<SAMPLE_COUNT;++i)
{  
     vec2 Xi = Hammersley(i, SAMPLE_COUNT);
        vec3 SampleVector = ImportanceSampleGGX(Xi, N, roughness);
      
        vec3 H = normalize(V + SampleVector);
       R= reflect(I,H);
        float cosT=clamp(dot(SampleVector, N), 0.0, 1.0);
        float sinT = sqrt( 1 - cosT * cosT);
       
        float D = GGX_Distribution(N, H, roughness);   
        float G   =  GGX_PartialGeometryTerm(V,N,H,roughness)*GGX_PartialGeometryTerm(SampleVector,N,H,roughness);    
        vec3 F    = fresnelSchlick(clamp(dot(H, V), 0.0, 1.0), F0);


        vec3 numerator    = texture(cubeMapTex,SampleVector).rgb*D* G * F *sinT; 
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, H), 0.0)  + 0.05; 
        specular += numerator/denominator;
        
        kS += F;
 }
    kS = kS/SAMPLE_COUNT;
    specular = specular/SAMPLE_COUNT;
       
   
    vec3 color =  specular; 
  
    color = pow(color, vec3(1.0/2.2)); 
    FragColor = vec4(color*5, 1.0);
}