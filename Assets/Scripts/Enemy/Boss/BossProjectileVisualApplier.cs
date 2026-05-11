using UnityEngine;

namespace Enemy.Boss
{
    public static class BossProjectileVisualApplier
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public static void Apply(GameObject projectileRoot, BossElementProfile element, BossWeaponProfile weapon)
        {
            if (!projectileRoot || !element || !weapon)
            {
                return;
            }

            switch (weapon.ElementPresentation)
            {
                case BossElementPresentationMode.TintProjectileColor:
                    TintRenderersAndParticles(projectileRoot.transform, element.ElementColor);
                    break;

                case BossElementPresentationMode.AttachParticlesOnProjectile:
                    if (element.ProjectileFollowParticlePrefab)
                    {
                        GameObject fx = Object.Instantiate(
                            element.ProjectileFollowParticlePrefab,
                            projectileRoot.transform,
                            false);
                        fx.transform.localPosition = Vector3.zero;
                        fx.transform.localRotation = Quaternion.identity;
                    }

                    break;
            }
        }

        private static void TintRenderersAndParticles(Transform root, Color color)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (!r || r is ParticleSystemRenderer)
                {
                    continue;
                }

                Material[] mats = r.materials;
                for (int m = 0; m < mats.Length; m++)
                {
                    Material mat = mats[m];
                    if (!mat)
                    {
                        continue;
                    }

                    if (mat.HasProperty(BaseColorId))
                    {
                        mat.SetColor(BaseColorId, color);
                    }
                    else if (mat.HasProperty(ColorId))
                    {
                        mat.SetColor(ColorId, color);
                    }
                }
            }

            var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem ps = particleSystems[i];
                if (!ps)
                {
                    continue;
                }

                var main = ps.main;
                main.startColor = color;
            }
        }
    }
}
