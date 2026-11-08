namespace AvaloniaSilkEffects.Backgrounds;

internal static class LatentShaders
{
    internal const string Mesh = """
        #version 330 core
        
        uniform float u_time;
        
        uniform vec4 u_colors[10];
        uniform float u_colorsCount;
        
        uniform float u_distortion;
        uniform float u_swirl;
        uniform float u_grainMixer;
        uniform float u_grainOverlay;
        
        in vec2 v_objectUV;
        out vec4 fragColor;
        
        
        #define TWO_PI 6.28318530718
        #define PI 3.14159265358979323846
        
        
        vec2 rotate(vec2 uv, float th) {
          return mat2(cos(th), sin(th), -sin(th), cos(th)) * uv;
        }
        
        
          float hash21(vec2 p) {
            p = fract(p * vec2(0.3183099, 0.3678794)) + 0.1;
            p += dot(p, p + 19.19);
            return fract(p.x * p.y);
          }
        
        
        float valueNoise(vec2 st) {
          vec2 i = floor(st);
          vec2 f = fract(st);
          float a = hash21(i);
          float b = hash21(i + vec2(1.0, 0.0));
          float c = hash21(i + vec2(0.0, 1.0));
          float d = hash21(i + vec2(1.0, 1.0));
          vec2 u = f * f * (3.0 - 2.0 * f);
          float x1 = mix(a, b, u.x);
          float x2 = mix(c, d, u.x);
          return mix(x1, x2, u.y);
        }
        
        float noise(vec2 n, vec2 seedOffset) {
          return valueNoise(n + seedOffset);
        }
        
        vec2 getPosition(int i, float t) {
          float a = float(i) * .37;
          float b = .6 + fract(float(i) / 3.) * .9;
          float c = .8 + fract(float(i + 1) / 4.);
        
          float x = sin(t * b + a);
          float y = cos(t * c + a * 1.5);
        
          return .5 + .5 * vec2(x, y);
        }
        
        void main() {
          vec2 uv = v_objectUV;
          uv += .5;
          vec2 grainUV = uv * 1000.;
        
          float grain = noise(grainUV, vec2(0.));
          float mixerGrain = .4 * u_grainMixer * (grain - .5);
        
          const float firstFrameOffset = 41.5;
          float t = .5 * (u_time + firstFrameOffset);
        
          float radius = smoothstep(0., 1., length(uv - .5));
          float center = 1. - radius;
          for (float i = 1.; i <= 2.; i++) {
            uv.x += u_distortion * center / i * sin(t + i * .4 * smoothstep(.0, 1., uv.y)) * cos(.2 * t + i * 2.4 * smoothstep(.0, 1., uv.y));
            uv.y += u_distortion * center / i * cos(t + i * 2. * smoothstep(.0, 1., uv.x));
          }
        
          vec2 uvRotated = uv;
          uvRotated -= vec2(.5);
          float angle = 3. * u_swirl * radius;
          uvRotated = rotate(uvRotated, -angle);
          uvRotated += vec2(.5);
        
          vec3 color = vec3(0.);
          float opacity = 0.;
          float totalWeight = 0.;
        
          for (int i = 0; i < 10; i++) {
            if (i >= int(u_colorsCount)) break;
        
            vec2 pos = getPosition(i, t) + mixerGrain;
            vec3 colorFraction = u_colors[i].rgb * u_colors[i].a;
            float opacityFraction = u_colors[i].a;
        
            float dist = length(uvRotated - pos);
        
            dist = pow(dist, 3.5);
            float weight = 1. / (dist + 1e-3);
            color += colorFraction * weight;
            opacity += opacityFraction * weight;
            totalWeight += weight;
          }
        
          color /= max(1e-4, totalWeight);
          opacity /= max(1e-4, totalWeight);
        
          float grainOverlay = valueNoise(rotate(grainUV, 1.) + vec2(3.));
          grainOverlay = mix(grainOverlay, valueNoise(rotate(grainUV, 2.) + vec2(-1.)), .5);
          grainOverlay = pow(grainOverlay, 1.3);
        
          float grainOverlayV = grainOverlay * 2. - 1.;
          vec3 grainOverlayColor = vec3(step(0., grainOverlayV));
          float grainOverlayStrength = u_grainOverlay * abs(grainOverlayV);
          grainOverlayStrength = pow(grainOverlayStrength, .8);
          color = mix(color, grainOverlayColor, .35 * grainOverlayStrength);
        
          opacity += .5 * grainOverlayStrength;
          opacity = clamp(opacity, 0., 1.);
        
          fragColor = vec4(color, opacity);
        }
        """;
    internal const string Dither = """
        #version 330 core
        
        uniform float u_time;
        
        uniform vec2 u_resolution;
        uniform float u_pixelRatio;
        uniform float u_originX;
        uniform float u_originY;
        uniform float u_worldWidth;
        uniform float u_worldHeight;
        uniform float u_fit;
        uniform float u_scale;
        uniform float u_rotation;
        uniform float u_offsetX;
        uniform float u_offsetY;
        
        uniform float u_pxSize;
        uniform vec4 u_colorBack;
        uniform vec4 u_colorFront;
        uniform float u_shape;
        uniform float u_type;
        
        out vec4 fragColor;
        
        
        vec3 permute(vec3 x) { return mod(((x * 34.0) + 1.0) * x, 289.0); }
        float snoise(vec2 v) {
          const vec4 C = vec4(0.211324865405187, 0.366025403784439,
            -0.577350269189626, 0.024390243902439);
          vec2 i = floor(v + dot(v, C.yy));
          vec2 x0 = v - i + dot(i, C.xx);
          vec2 i1;
          i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
          vec4 x12 = x0.xyxy + C.xxzz;
          x12.xy -= i1;
          i = mod(i, 289.0);
          vec3 p = permute(permute(i.y + vec3(0.0, i1.y, 1.0))
            + i.x + vec3(0.0, i1.x, 1.0));
          vec3 m = max(0.5 - vec3(dot(x0, x0), dot(x12.xy, x12.xy),
              dot(x12.zw, x12.zw)), 0.0);
          m = m * m;
          m = m * m;
          vec3 x = 2.0 * fract(p * C.www) - 1.0;
          vec3 h = abs(x) - 0.5;
          vec3 ox = floor(x + 0.5);
          vec3 a0 = x - ox;
          m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);
          vec3 g;
          g.x = a0.x * x0.x + h.x * x0.y;
          g.yz = a0.yz * x12.xz + h.yz * x12.yw;
          return 130.0 * dot(m, g);
        }
        
        
        #define TWO_PI 6.28318530718
        #define PI 3.14159265358979323846
        
        
          float hash11(float p) {
            p = fract(p * 0.3183099) + 0.1;
            p *= p + 19.19;
            return fract(p * p);
          }
        
        
          float hash21(vec2 p) {
            p = fract(p * vec2(0.3183099, 0.3678794)) + 0.1;
            p += dot(p, p + 19.19);
            return fract(p.x * p.y);
          }
        
        
        float getSimplexNoise(vec2 uv, float t) {
          float noise = .5 * snoise(uv - vec2(0., .3 * t));
          noise += .5 * snoise(2. * uv + vec2(0., .32 * t));
        
          return noise;
        }
        
        const int bayer2x2[4] = int[4](0, 2, 3, 1);
        const int bayer4x4[16] = int[16](
        0, 8, 2, 10,
        12, 4, 14, 6,
        3, 11, 1, 9,
        15, 7, 13, 5
        );
        
        const int bayer8x8[64] = int[64](
        0, 32, 8, 40, 2, 34, 10, 42,
        48, 16, 56, 24, 50, 18, 58, 26,
        12, 44, 4, 36, 14, 46, 6, 38,
        60, 28, 52, 20, 62, 30, 54, 22,
        3, 35, 11, 43, 1, 33, 9, 41,
        51, 19, 59, 27, 49, 17, 57, 25,
        15, 47, 7, 39, 13, 45, 5, 37,
        63, 31, 55, 23, 61, 29, 53, 21
        );
        
        float getBayerValue(vec2 uv, int size) {
          ivec2 pos = ivec2(fract(uv / float(size)) * float(size));
          int index = pos.y * size + pos.x;
        
          if (size == 2) {
            return float(bayer2x2[index]) / 4.0;
          } else if (size == 4) {
            return float(bayer4x4[index]) / 16.0;
          } else if (size == 8) {
            return float(bayer8x8[index]) / 64.0;
          }
          return 0.0;
        }
        
        
        void main() {
          float t = .5 * u_time;
        
          float pxSize = u_pxSize * u_pixelRatio;
          vec2 pxSizeUV = gl_FragCoord.xy - .5 * u_resolution;
          pxSizeUV /= pxSize;
          vec2 canvasPixelizedUV = (floor(pxSizeUV) + .5) * pxSize;
          vec2 normalizedUV = canvasPixelizedUV / u_resolution;
        
          vec2 ditheringNoiseUV = canvasPixelizedUV;
          vec2 shapeUV = normalizedUV;
        
          vec2 boxOrigin = vec2(.5 - u_originX, u_originY - .5);
          vec2 givenBoxSize = vec2(u_worldWidth, u_worldHeight);
          givenBoxSize = max(givenBoxSize, vec2(1.)) * u_pixelRatio;
          float r = u_rotation * PI / 180.;
          mat2 graphicRotation = mat2(cos(r), sin(r), -sin(r), cos(r));
          vec2 graphicOffset = vec2(-u_offsetX, u_offsetY);
        
          float patternBoxRatio = givenBoxSize.x / givenBoxSize.y;
          vec2 boxSize = vec2(
          (u_worldWidth == 0.) ? u_resolution.x : givenBoxSize.x,
          (u_worldHeight == 0.) ? u_resolution.y : givenBoxSize.y
          );
          
          if (u_shape > 3.5) {
            vec2 objectBoxSize = vec2(0.);
            // fit = none
            objectBoxSize.x = min(boxSize.x, boxSize.y);
            if (u_fit == 1.) { // fit = contain
              objectBoxSize.x = min(u_resolution.x, u_resolution.y);
            } else if (u_fit == 2.) { // fit = cover
              objectBoxSize.x = max(u_resolution.x, u_resolution.y);
            }
            objectBoxSize.y = objectBoxSize.x;
            vec2 objectWorldScale = u_resolution.xy / objectBoxSize;
        
            shapeUV *= objectWorldScale;
            shapeUV += boxOrigin * (objectWorldScale - 1.);
            shapeUV += vec2(-u_offsetX, u_offsetY);
            shapeUV /= u_scale;
            shapeUV = graphicRotation * shapeUV;
          } else {
            vec2 patternBoxSize = vec2(0.);
            // fit = none
            patternBoxSize.x = patternBoxRatio * min(boxSize.x / patternBoxRatio, boxSize.y);
            float patternWorldNoFitBoxWidth = patternBoxSize.x;
            if (u_fit == 1.) { // fit = contain
              patternBoxSize.x = patternBoxRatio * min(u_resolution.x / patternBoxRatio, u_resolution.y);
            } else if (u_fit == 2.) { // fit = cover
              patternBoxSize.x = patternBoxRatio * max(u_resolution.x / patternBoxRatio, u_resolution.y);
            }
            patternBoxSize.y = patternBoxSize.x / patternBoxRatio;
            vec2 patternWorldScale = u_resolution.xy / patternBoxSize;
        
            shapeUV += vec2(-u_offsetX, u_offsetY) / patternWorldScale;
            shapeUV += boxOrigin;
            shapeUV -= boxOrigin / patternWorldScale;
            shapeUV *= u_resolution.xy;
            shapeUV /= u_pixelRatio;
            if (u_fit > 0.) {
              shapeUV *= (patternWorldNoFitBoxWidth / patternBoxSize.x);
            }
            shapeUV /= u_scale;
            shapeUV = graphicRotation * shapeUV;
            shapeUV += boxOrigin / patternWorldScale;
            shapeUV -= boxOrigin;
            shapeUV += .5;
          }
        
          float shape = 0.;
          if (u_shape < 1.5) {
            // Simplex noise
            shapeUV *= .001;
        
            shape = 0.5 + 0.5 * getSimplexNoise(shapeUV, t);
            shape = smoothstep(0.3, 0.9, shape);
        
          } else if (u_shape < 2.5) {
            // Warp
            shapeUV *= .003;
        
            for (float i = 1.0; i < 6.0; i++) {
              shapeUV.x += 0.6 / i * cos(i * 2.5 * shapeUV.y + t);
              shapeUV.y += 0.6 / i * cos(i * 1.5 * shapeUV.x + t);
            }
        
            shape = .15 / max(0.001, abs(sin(t - shapeUV.y - shapeUV.x)));
            shape = smoothstep(0.02, 1., shape);
        
          } else if (u_shape < 3.5) {
            // Dots
            shapeUV *= .05;
        
            float stripeIdx = floor(2. * shapeUV.x / TWO_PI);
            float rand = hash11(stripeIdx * 10.);
            rand = sign(rand - .5) * pow(.1 + abs(rand), .4);
            shape = sin(shapeUV.x) * cos(shapeUV.y - 5. * rand * t);
            shape = pow(abs(shape), 6.);
        
          } else if (u_shape < 4.5) {
            // Sine wave
            shapeUV *= 4.;
        
            float wave = cos(.5 * shapeUV.x - 2. * t) * sin(1.5 * shapeUV.x + t) * (.75 + .25 * cos(3. * t));
            shape = 1. - smoothstep(-1., 1., shapeUV.y + wave);
        
          } else if (u_shape < 5.5) {
            // Ripple
        
            float dist = length(shapeUV);
            float waves = sin(pow(dist, 1.7) * 7. - 3. * t) * .5 + .5;
            shape = waves;
        
          } else if (u_shape < 6.5) {
            // Swirl
        
            float l = length(shapeUV);
            float angle = 6. * atan(shapeUV.y, shapeUV.x) + 4. * t;
            float twist = 1.2;
            float offset = 1. / pow(max(l, 1e-6), twist) + angle / TWO_PI;
            float mid = smoothstep(0., 1., pow(l, twist));
            shape = mix(0., fract(offset), mid);
        
          } else {
            // Sphere
            shapeUV *= 2.;
        
            float d = 1. - pow(length(shapeUV), 2.);
            vec3 pos = vec3(shapeUV, sqrt(max(0., d)));
            vec3 lightPos = normalize(vec3(cos(1.5 * t), .8, sin(1.25 * t)));
            shape = .5 + .5 * dot(lightPos, pos);
            shape *= step(0., d);
          }
        
        
          int type = int(floor(u_type));
          float dithering = 0.0;
        
          switch (type) {
            case 1: {
              dithering = step(hash21(ditheringNoiseUV), shape);
            } break;
            case 2:
            dithering = getBayerValue(pxSizeUV, 2);
            break;
            case 3:
            dithering = getBayerValue(pxSizeUV, 4);
            break;
            default :
            dithering = getBayerValue(pxSizeUV, 8);
            break;
          }
        
          dithering -= .5;
          float res = step(.5, shape + dithering);
        
          vec3 fgColor = u_colorFront.rgb * u_colorFront.a;
          float fgOpacity = u_colorFront.a;
          vec3 bgColor = u_colorBack.rgb * u_colorBack.a;
          float bgOpacity = u_colorBack.a;
        
          vec3 color = fgColor * res;
          float opacity = fgOpacity * res;
        
          color += bgColor * (1. - opacity);
          opacity += bgOpacity * (1. - opacity);
        
          fragColor = vec4(color, opacity);
        }
        """;
}
