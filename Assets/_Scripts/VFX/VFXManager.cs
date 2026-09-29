using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace BusJam.VFX
{
    /// <summary>
    /// Quản lý toàn bộ hiệu ứng hình ảnh (Particle VFX), Game Feel & Camera Shake trong Bus Jam 3D
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        private static VFXManager instance;
        private static bool isQuitting = false;

        public static bool HasInstance => instance != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetQuitState()
        {
            isQuitting = false;
        }

        public static VFXManager Instance
        {
            get
            {
                if (isQuitting) return null;
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindAnyObjectByType<VFXManager>();
                    if (instance == null && !isQuitting)
                    {
                        GameObject obj = new GameObject("VFXManager");
                        instance = obj.AddComponent<VFXManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Cài đặt Camera Shake")]
        public bool batCameraShake = true;
        private Vector3 viTriGocCamera;
        private Tweener cameraShakeTweener;

        // Particle Systems được tạo tự động hoặc gán thủ công
        private ParticleSystem psConfettiTrai;
        private ParticleSystem psConfettiPhai;
        private ParticleSystem psExhaustSmoke;
        private ParticleSystem psStarBurst;
        private ParticleSystem psBrakeSmoke;
        private ParticleSystem psPassengerPop;

        private Material particleMaterialDefault;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
                return;
            }

            // Ghi nhớ vị trí ban đầu của Camera chính
            if (Camera.main != null)
            {
                viTriGocCamera = Camera.main.transform.position;
            }

            KhoiTaoParticleSystems();
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                viTriGocCamera = Camera.main.transform.position;
            }
        }

        /// <summary>
        /// Tìm hoặc tạo Shader/Material phù hợp cho Particle System (tương thích cả URP và Built-in)
        /// </summary>
        private Material LayMaterialParticle()
        {
            if (particleMaterialDefault != null) return particleMaterialDefault;

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            particleMaterialDefault = new Material(shader);
            return particleMaterialDefault;
        }

        private void KhoiTaoParticleSystems()
        {
            Material mat = LayMaterialParticle();

            // 1. Pháo hoa Confetti bên Trái
            psConfettiTrai = TaoParticleConfetti("Confetti_Left", new Vector3(-6.5f, 0.5f, 2f), new Vector3(-65f, 45f, 0f), mat);
            
            // 2. Pháo hoa Confetti bên Phải
            psConfettiPhai = TaoParticleConfetti("Confetti_Right", new Vector3(6.5f, 0.5f, 2f), new Vector3(-65f, -45f, 0f), mat);

            // 3. Khói xe xả khói (Bus Exhaust Smoke)
            psExhaustSmoke = TaoParticleExhaustSmoke("Bus_ExhaustSmoke", mat);

            // 4. Pháo sao vàng chúc mừng xe đầy khách (Star Burst)
            psStarBurst = TaoParticleStarBurst("Bus_StarBurst", mat);

            // 5. Khói phanh dừng xe (Brake Smoke)
            psBrakeSmoke = TaoParticleBrakeSmoke("Bus_BrakeSmoke", mat);

            // 6. Hạt Pop nảy màu khi hé lộ khách hoặc khách lên xe
            psPassengerPop = TaoParticlePassengerPop("Passenger_Pop", mat);

            // Kích hoạt GameObject trở lại sau khi đã cấu hình xong toàn bộ duration và module
            if (psConfettiTrai != null) psConfettiTrai.gameObject.SetActive(true);
            if (psConfettiPhai != null) psConfettiPhai.gameObject.SetActive(true);
            if (psExhaustSmoke != null) psExhaustSmoke.gameObject.SetActive(true);
            if (psStarBurst != null) psStarBurst.gameObject.SetActive(true);
            if (psBrakeSmoke != null) psBrakeSmoke.gameObject.SetActive(true);
            if (psPassengerPop != null) psPassengerPop.gameObject.SetActive(true);
        }

        #region TẠO RUNTIME PARTICLE SYSTEMS

        /// <summary>
        /// Tạo một ParticleSystem cơ bản (khởi tạo ở trạng thái Inactive để cho phép gán duration mà không gây warning)
        /// </summary>
        private ParticleSystem TaoParticleCoBan(string name, Material mat)
        {
            GameObject obj = new GameObject(name);
            obj.SetActive(false); // Đặt Inactive trước khi thêm ParticleSystem
            obj.transform.SetParent(transform, false);

            ParticleSystem ps = obj.AddComponent<ParticleSystem>();

            ParticleSystemRenderer psr = obj.GetComponent<ParticleSystemRenderer>();
            psr.material = mat;

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;

            return ps;
        }

        private ParticleSystem TaoParticleConfetti(string name, Vector3 pos, Vector3 rot, Material mat)
        {
            ParticleSystem ps = TaoParticleCoBan(name, mat);
            ps.transform.position = pos;
            ps.transform.rotation = Quaternion.Euler(rot);

            ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Billboard;

            var main = ps.main;
            main.duration = 2.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.8f, 3.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(12f, 18f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.55f);
            main.gravityModifier = 0.55f;
            main.startRotation3D = true;
            main.startRotationX = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startRotationY = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

            // Bảng màu rực rỡ phong phú chuẩn Confetti
            var col = main.startColor;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.2f, 0.35f), 0f),    // Đỏ neon
                    new GradientColorKey(new Color(1f, 0.85f, 0.1f), 0.25f), // Vàng kim
                    new GradientColorKey(new Color(0.1f, 0.9f, 0.4f), 0.5f),  // Xanh lá
                    new GradientColorKey(new Color(0.1f, 0.75f, 1f), 0.75f), // Xanh cyan
                    new GradientColorKey(new Color(0.85f, 0.25f, 1f), 1f)    // Tím hồng
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(grad);

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0.0f, 120, 160),
                new ParticleSystem.Burst(0.4f, 80, 100)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.3f;

            var rotOverLifetime = ps.rotationOverLifetime;
            rotOverLifetime.enabled = true;
            rotOverLifetime.x = new ParticleSystem.MinMaxCurve(180f * Mathf.Deg2Rad, 360f * Mathf.Deg2Rad);
            rotOverLifetime.y = new ParticleSystem.MinMaxCurve(180f * Mathf.Deg2Rad, 360f * Mathf.Deg2Rad);
            rotOverLifetime.z = new ParticleSystem.MinMaxCurve(90f * Mathf.Deg2Rad, 270f * Mathf.Deg2Rad);

            return ps;
        }

        private ParticleSystem TaoParticleExhaustSmoke(string name, Material mat)
        {
            ParticleSystem ps = TaoParticleCoBan(name, mat);

            var main = ps.main;
            main.duration = 1.0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
            main.gravityModifier = -0.08f; // Bốc nhẹ lên trời
            main.startColor = new Color(0.85f, 0.85f, 0.9f, 0.65f);

            var emission = ps.emission;
            emission.rateOverTime = 25;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f;
            shape.radius = 0.15f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.4f);
            curve.AddKey(1f, 1.6f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var colOverLife = ps.colorOverLifetime;
            colOverLife.enabled = true;
            Gradient alphaGrad = new Gradient();
            alphaGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0.4f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLife.color = alphaGrad;

            return ps;
        }

        private ParticleSystem TaoParticleStarBurst(string name, Material mat)
        {
            ParticleSystem ps = TaoParticleCoBan(name, mat);

            var main = ps.main;
            main.duration = 0.6f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.75f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.gravityModifier = 0.2f;

            Gradient starGrad = new Gradient();
            starGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.95f, 0.2f), 0f), new GradientColorKey(new Color(1f, 0.6f, 0f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(starGrad);

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25, 35) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1.2f);
            curve.AddKey(1f, 0f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, curve);

            return ps;
        }

        private ParticleSystem TaoParticleBrakeSmoke(string name, Material mat)
        {
            ParticleSystem ps = TaoParticleCoBan(name, mat);

            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            main.startColor = new Color(0.9f, 0.9f, 0.95f, 0.55f);

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 16, 22) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.5f);
            curve.AddKey(1f, 1.4f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, curve);

            return ps;
        }

        private ParticleSystem TaoParticlePassengerPop(string name, Material mat)
        {
            ParticleSystem ps = TaoParticleCoBan(name, mat);

            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.35f);
            main.gravityModifier = 0.3f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 18, 25) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;

            return ps;
        }

        #endregion

        #region PUBLIC API PHÁT HIỆU ỨNG (VFX CALLS)

        /// <summary>
        /// Bắn pháo hoa Confetti chúc mừng chiến thắng ngập tràn màn hình
        /// </summary>
        public void PlayConfettiVictory()
        {
            if (psConfettiTrai != null)
            {
                psConfettiTrai.Stop();
                psConfettiTrai.Play();
            }

            if (psConfettiPhai != null)
            {
                psConfettiPhai.Stop();
                psConfettiPhai.Play();
            }

            // Rung màn hình mạnh chúc mừng chiến thắng
            CameraShake(0.35f, 0.16f);
            Debug.Log("[VFXManager] Đã bắn pháo hoa Confetti mừng chiến thắng!");
        }

        /// <summary>
        /// Phun cụm khói xe sau đuôi khi xe bus tăng tốc khởi hành
        /// </summary>
        public void PlayBusExhaustSmoke(Vector3 viTriDuoiXe, Vector3 huongXe)
        {
            if (psExhaustSmoke != null)
            {
                psExhaustSmoke.transform.position = viTriDuoiXe;
                // Khói xả ngược hướng xe chạy
                psExhaustSmoke.transform.rotation = Quaternion.LookRotation(-huongXe + Vector3.up * 0.2f);
                psExhaustSmoke.Stop();
                psExhaustSmoke.Play();
            }
        }

        /// <summary>
        /// Phun khói phanh xe khi xe bus vừa phanh dừng lại ở bến
        /// </summary>
        public void PlayBrakeSmoke(Vector3 viTriBanhXe)
        {
            if (psBrakeSmoke != null)
            {
                psBrakeSmoke.transform.position = viTriBanhXe;
                psBrakeSmoke.Stop();
                psBrakeSmoke.Play();
            }

            // Rung nhẹ khi xe phanh dừng lại
            CameraShake(0.12f, 0.05f);
        }

        /// <summary>
        /// Bắn chùm sao vàng lấp lánh trên nóc xe khi xe đầy 3 khách
        /// </summary>
        public void PlayStarBurst(Vector3 viTriTrenXe)
        {
            if (psStarBurst != null)
            {
                psStarBurst.transform.position = viTriTrenXe + Vector3.up * 1.5f;
                psStarBurst.Stop();
                psStarBurst.Play();
            }

            // Rung nhẹ phấn khích khi xe đón đủ khách
            CameraShake(0.18f, 0.08f);
        }

        /// <summary>
        /// Bắn các hạt nảy màu khi hé lộ khách ẩn hoặc khi khách nhảy
        /// </summary>
        public void PlayPassengerPop(Vector3 viTri, Color mauHat)
        {
            if (psPassengerPop != null)
            {
                psPassengerPop.transform.position = viTri + Vector3.up * 0.8f;
                var main = psPassengerPop.main;
                main.startColor = mauHat;
                psPassengerPop.Stop();
                psPassengerPop.Play();
            }
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            if (cameraShakeTweener != null && cameraShakeTweener.IsActive())
            {
                cameraShakeTweener.Kill();
            }
        }

        /// <summary>
        /// Rung nhẹ Camera chính và tự động hoàn trả về vị trí gốc chính xác 100%
        /// </summary>
        public void CameraShake(float duration = 0.2f, float strength = 0.1f)
        {
            if (!batCameraShake || Camera.main == null) return;

            Camera cam = Camera.main;

            if (viTriGocCamera == Vector3.zero)
            {
                viTriGocCamera = cam.transform.position;
            }

            if (cameraShakeTweener != null && cameraShakeTweener.IsActive())
            {
                cameraShakeTweener.Kill();
                cam.transform.position = viTriGocCamera;
            }

            cameraShakeTweener = cam.transform.DOShakePosition(duration, strength, 14, 90f, false, true)
                .OnComplete(() =>
                {
                    if (cam != null) cam.transform.position = viTriGocCamera;
                });
        }

        /// <summary>
        /// Hiệu ứng lắc lư cảnh báo khi người chơi bấm vào nhân vật đang bị chặn
        /// </summary>
        public void PlayKhachBiChanWobble(Transform target)
        {
            if (target == null) return;

            target.DOKill();
            target.rotation = Quaternion.identity; // Đưa về góc xoay thẳng chuẩn ngay lập tức, không để lệch góc khi spam click
            target.DOPunchRotation(new Vector3(0, 20f, 0), 0.22f, 10, 1f)
                .OnComplete(() =>
                {
                    if (target != null) target.rotation = Quaternion.identity;
                });
            CameraShake(0.08f, 0.04f);
        }

        #endregion
    }
}
