using UnityEngine;
using UnityEngine.UI;

namespace _TheGame._Scripts.Helpers
{
    [RequireComponent(typeof(ParticleSystem))]
    public class UiParticleSystem : MaskableGraphic
    {
        public Texture ParticleTexture;
        public Sprite ParticleSprite;

        private ParticleSystem _particleSystem;
        private ParticleSystem.Particle[] _particles;
        private UIVertex[] _quad = new UIVertex[4];
        private Vector4 _imageUV = Vector4.zero;
        private ParticleSystem.MainModule _mainModule;

        public override Texture mainTexture
        {
            get
            {
                if (ParticleTexture)
                {
                    return ParticleTexture;
                }
                if (ParticleSprite)
                {
                    return ParticleSprite.texture;
                }
                return Texture2D.whiteTexture;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _particleSystem = GetComponent<ParticleSystem>();
            _mainModule = _particleSystem.main;

            _particles = new ParticleSystem.Particle[_mainModule.maxParticles];

            if (ParticleSprite)
            {
                _imageUV = new Vector4(
                    ParticleSprite.rect.x / ParticleSprite.texture.width,
                    ParticleSprite.rect.y / ParticleSprite.texture.height,
                    ParticleSprite.rect.width / ParticleSprite.texture.width,
                    ParticleSprite.rect.height / ParticleSprite.texture.height
                );
            }
            else
            {
                _imageUV = new Vector4(0, 0, 1, 1);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            int particleCount = _particleSystem.GetParticles(_particles);

            for (int i = 0; i < particleCount; i++)
            {
                ParticleSystem.Particle particle = _particles[i];
                Vector2 position = (Vector2)particle.position;
                float rotation = -particle.rotation * Mathf.Deg2Rad;
                Color32 color = particle.GetCurrentColor(_particleSystem);
                float size = particle.GetCurrentSize(_particleSystem) * 0.5f;

                Vector2 bl = new Vector2(-size, -size);
                Vector2 tl = new Vector2(-size, size);
                Vector2 tr = new Vector2(size, size);
                Vector2 br = new Vector2(size, -size);

                bl = RotateAroundZero(bl, rotation) + position;
                tl = RotateAroundZero(tl, rotation) + position;
                tr = RotateAroundZero(tr, rotation) + position;
                br = RotateAroundZero(br, rotation) + position;

                _quad[0] = UIVertex.simpleVert;
                _quad[0].color = color;
                _quad[0].position = bl;
                _quad[0].uv0 = new Vector2(_imageUV.x, _imageUV.y);

                _quad[1] = UIVertex.simpleVert;
                _quad[1].color = color;
                _quad[1].position = tl;
                _quad[1].uv0 = new Vector2(_imageUV.x, _imageUV.y + _imageUV.w);

                _quad[2] = UIVertex.simpleVert;
                _quad[2].color = color;
                _quad[2].position = tr;
                _quad[2].uv0 = new Vector2(_imageUV.x + _imageUV.z, _imageUV.y + _imageUV.w);

                _quad[3] = UIVertex.simpleVert;
                _quad[3].color = color;
                _quad[3].position = br;
                _quad[3].uv0 = new Vector2(_imageUV.x + _imageUV.z, _imageUV.y);

                vh.AddUIVertexQuad(_quad);
            }
        }

        private Vector2 RotateAroundZero(Vector2 v, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        void Update()
        {
            SetVerticesDirty();
        }
    }
}