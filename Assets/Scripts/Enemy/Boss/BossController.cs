using System;
using System.Collections;
using Core;
using Enemy.Boss.Projectiles;
using Player.Projectiles;
using UnityEngine;
using UnityEngine.AI;
using SaveSystem;
using Random = UnityEngine.Random;

namespace Enemy.Boss
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class BossController : MonoBehaviour, IDamageable
    {
        [SerializeField] private BossConfig config;
        [SerializeField] private string currentStateName;

        [Header("Optional Transforms")]
        [SerializeField] private Transform muzzleTransform;
        [SerializeField] private Transform rocketSpawnTransform;

        [Header("Weapon & element")]
        [SerializeField] private BossWeaponProfile[] weaponProfiles;
        [SerializeField] private BossElementProfile[] elementProfiles;

        private BossWeaponProfile _weaponProfile;
        private BossElementProfile _elementProfile;

        private Transform _playerTransform;
        private NavMeshAgent _agent;
        private Animator _animator;
        private float _currentHealth;
        private int _playerSearchCooldown;
        private bool _deathSequenceStarted;

        private BossFightStateMachine _fightStateMachine;
        private float _enragedIntroStartTime;
        private bool _aggroedByPlayer;

        private Coroutine _basicAttackRoutine;
        private Coroutine _suppressiveFireRoutine;
        private Coroutine _rocketBarrageRoutine;
        private Coroutine _enragedIntroRoutine;

        public event Action<float> OnHealthChanged;

        public BossConfig Config => config;
        public Transform PlayerTransform => _playerTransform;
        public bool IsDead { get; private set; }
        public bool IsPeacefulModeEnabled { get; private set; }
        public bool IsEnraged { get; private set; }
        public bool IsCombatAllowed => !IsPeacefulModeEnabled || _aggroedByPlayer;
        public float LastRocketBarrageTime { get; set; } = -999f;

        public float GetHealth() => _currentHealth;

        public float GetMaxHealth() => config ? config.MaxHealth : 0f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();

            IsPeacefulModeEnabled = CharacterManager.Instance && CharacterManager.Instance.HasActiveCharacter && CharacterManager.Instance.ActiveCharacter != null && CharacterManager.Instance.ActiveCharacter.PeacefulModeEnabled;

            if (config)
            {
                _currentHealth = config.MaxHealth;
                _agent.speed = config.MoveSpeed;
            }

            TryFindPlayer();
            PickRandomWeaponAndElement();
        }

        private void Start()
        {
            SetFightStateMachine(new BossFightStateMachine(this));
        }

        private void SetFightStateMachine(BossFightStateMachine fightStateMachine)
        {
            _fightStateMachine?.CurrentState?.Exit();
            StopAllCombatRoutines();
            _fightStateMachine = fightStateMachine;
            _fightStateMachine.Initialize();
        }

        private void StopAllCombatRoutines()
        {
            StopBasicAttackRoutine();
            StopSuppressiveFireRoutine();
            StopRocketBarrageRoutine();
        }

        private void PickRandomWeaponAndElement()
        {
            if (weaponProfiles != null && weaponProfiles.Length > 0)
            {
                _weaponProfile = weaponProfiles[Random.Range(0, weaponProfiles.Length)];
            }

            if (elementProfiles != null && elementProfiles.Length > 0)
            {
                _elementProfile = elementProfiles[Random.Range(0, elementProfiles.Length)];
            }
        }

        private GameObject GetBasicProjectilePrefab()
        {
            if (_elementProfile && _elementProfile.BasicProjectilePrefab)
            {
                return _elementProfile.BasicProjectilePrefab;
            }

            if (config) return config.BasicProjectilePrefab;

            return null;
        }

        private GameObject GetSuppressiveProjectilePrefab()
        {
            if (_elementProfile && _elementProfile.SuppressiveProjectilePrefab)
            {
                return _elementProfile.SuppressiveProjectilePrefab;
            }

            if (config) return config.SuppressiveProjectilePrefab;
            
            return null;
        }

        private float GetEffectiveBasicDamage()
        {
            float baseDamage;
            if (config) baseDamage = config.BasicAttackDamage;
            else baseDamage = 0f;
            
            float mul;
            if (_weaponProfile) mul = _weaponProfile.DamageMultiplier;
            else mul = 1f;
            
            return baseDamage * mul;
        }

        private float GetEffectiveSuppressiveDamage()
        {
            float baseDamage;
            if (config) baseDamage = config.SuppressiveDamage;
            else baseDamage = 0f;
            
            float mul;
            if (_weaponProfile) mul = _weaponProfile.DamageMultiplier;
            else mul = 1f;
            
            return baseDamage * mul;
        }

        private float GetEffectiveBasicAimDelay()
        {
            float baseDelay;
            if (config)
            {
                if (IsEnraged) baseDelay = config.EnragedBasicAimDelay;
                else baseDelay = config.BasicAimDelay;
            }
            else
            {
                baseDelay = 0.5f;
            }

            float speed;
            if (_weaponProfile) speed = _weaponProfile.AttackSpeedMultiplier;
            else speed = 1f;
            
            return baseDelay / speed;
        }

        private float GetEffectiveBasicInterval()
        {
            float baseInterval;
            if (config) baseInterval = config.BasicInterval;
            else baseInterval = 0.45f;
            
            float speed;
            if (_weaponProfile) speed = _weaponProfile.AttackSpeedMultiplier;
            else speed = 1f;
            
            return baseInterval / speed;
        }

        private float GetEffectiveSuppressiveShotInterval()
        {
            float sps;
            if (config) sps = config.SuppressiveShotsPerSecond;
            else sps = 8f;
            
            float baseInterval = 1f / Mathf.Max(1f, sps);
            float speed;
            if (_weaponProfile) speed = _weaponProfile.AttackSpeedMultiplier;
            else speed = 1f;
            
            return baseInterval / speed;
        }

        private void PlayBossChaseAnimation()
        {
            string state;
            if (_weaponProfile) state = _weaponProfile.ChaseAnimState;
            else state = "Chase";
            
            PlayAnimation(state);
        }

        private void PlayBossBasicAttackAnimation()
        {
            string state;
            if (_weaponProfile) state = _weaponProfile.BasicAttackAnimState;
            else state = "Attack";
            
            PlayAnimation(state);
        }

        private void PlayBossSuppressiveAttackAnimation()
        {
            string state;
            if (_weaponProfile) state = _weaponProfile.SuppressiveAttackAnimState;
            else state = "Attack";
            
            PlayAnimation(state);
        }

        private void Update()
        {
            if (!_playerTransform)
            {
                _playerSearchCooldown--;
                if (_playerSearchCooldown <= 0)
                {
                    _playerSearchCooldown = 30;
                    TryFindPlayer();
                }
            }

            if (IsDead)
            {
                return;
            }

            TryEnterEnragedPhase();

            _fightStateMachine?.CurrentState?.LogicUpdate();
            currentStateName = _fightStateMachine?.CurrentState?.GetType().Name ?? string.Empty;
        }

        private void TryEnterEnragedPhase()
        {
            if (!config || IsEnraged || IsDead || _currentHealth <= 0f)
            {
                return;
            }

            if (_currentHealth > GetMaxHealth() * config.EnragedThresholdNormalized)
            {
                return;
            }

            if (_fightStateMachine is BossEnragedFightStateMachine)
            {
                return;
            }

            SetFightStateMachine(new BossEnragedFightStateMachine(this));
        }

        public void TakeDamage(float damage, DamageInfo damageInfo)
        {
            if (IsDead)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            OnHealthChanged?.Invoke(_currentHealth);

            TryEnterEnragedPhase();

            if (_currentHealth <= 0f)
            {
                Die();
                return;
            }

            if (IsPeacefulModeEnabled)
            {
                if (IsDamageFromPlayer(damageInfo))
                {
                    _aggroedByPlayer = true;
                    _fightStateMachine.ChangeState(_fightStateMachine.CreateChaseState());
                }

                return;
            }

            _fightStateMachine.ChangeState(_fightStateMachine.CreateChaseState());
        }

        private static bool IsDamageFromPlayer(DamageInfo damageInfo)
        {
            if (!damageInfo.SourceObject)
            {
                return false;
            }

            if (damageInfo.SourceObject.CompareTag("Player"))
            {
                return true;
            }

            return damageInfo.SourceObject.transform.root.CompareTag("Player");
        }

        private bool CanSeePlayer()
        {
            if (!_playerTransform || !config)
            {
                return false;
            }

            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            if (distance > config.DetectionRadius)
            {
                return false;
            }

            Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
            Vector3 flatDirection = directionToPlayer;
            flatDirection.y = 0f;
            if (flatDirection == Vector3.zero)
            {
                flatDirection = transform.forward;
            }

            Vector3 flatForward = transform.forward;
            flatForward.y = 0f;
            if (flatForward == Vector3.zero)
            {
                flatForward = Vector3.forward;
            }

            float angle = Vector3.Angle(flatForward, flatDirection);
            if (angle > config.FieldOfView * 0.5f)
            {
                return false;
            }

            Vector3 origin = transform.position + Vector3.up * 1.8f;
            Vector3 target = _playerTransform.position + Vector3.up * 1f;
            Vector3 rayDirection = target - origin;

            if (Physics.Raycast(origin, rayDirection, out RaycastHit hit, distance + 1f, Physics.AllLayers, QueryTriggerInteraction.Collide))
            {
                return hit.transform == _playerTransform || hit.transform.root == _playerTransform.root || hit.transform.CompareTag("Player");
            }

            return false;
        }

        private void LookAtPlayer(float rotationSpeed = 7f)
        {
            if (!_playerTransform)
            {
                return;
            }

            Vector3 toPlayer = _playerTransform.position - transform.position;
            Vector3 flatDirection = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (flatDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion lookRotation = Quaternion.LookRotation(flatDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        private float DistanceToPlayer()
        {
            if (!_playerTransform)
            {
                return float.MaxValue;
            }

            return Vector3.Distance(transform.position, _playerTransform.position);
        }

        public bool IsRocketReady()
        {
            return config && Time.time >= LastRocketBarrageTime + config.RocketCooldown;
        }

        private void SetMovementEnabled(bool enabled)
        {
            if (!_agent)
            {
                return;
            }

            _agent.enabled = true;
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = !enabled;
            }
        }

        private void MoveTo(Vector3 destination)
        {
            if (!_agent || !_agent.enabled || !_agent.isOnNavMesh)
            {
                return;
            }

            _agent.SetDestination(destination);
        }

        private void PlayAnimation(string stateName)
        {
            if (_animator && !string.IsNullOrWhiteSpace(stateName))
            {
                _animator.Play(stateName);
            }
        }

        private void FireProjectile(GameObject projectilePrefab, float damage, Vector3 direction, float upwardBias = 0f)
        {
            if (!projectilePrefab)
            {
                return;
            }

            Vector3 origin;
            if (muzzleTransform) origin = muzzleTransform.position;
            else origin = transform.position + Vector3.up * 1.5f;
            
            Vector3 shotDirection = (direction + Vector3.up * upwardBias).normalized;
            if (shotDirection.sqrMagnitude < 0.001f)
            {
                shotDirection = transform.forward;
            }

            Quaternion rotation = Quaternion.LookRotation(shotDirection);
            GameObject projectile = Instantiate(projectilePrefab, origin, rotation);

            RangedProjectile rangedProjectile = projectile.GetComponent<RangedProjectile>();
            if (rangedProjectile)
            {
                rangedProjectile.Initialize(damage, gameObject);
            }

            if (_weaponProfile && _elementProfile)
            {
                BossProjectileVisualApplier.Apply(projectile, _elementProfile, _weaponProfile);
            }
        }

        private void FireProjectileAtTarget(GameObject projectilePrefab, float damage, Vector3 targetPosition, float upwardBias = 0f)
        {
            Vector3 origin = GetMuzzlePosition();
            Vector3 direction = targetPosition - origin;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = transform.forward;
            }

            FireProjectile(projectilePrefab, damage, direction, upwardBias);
        }

        private Vector3 GetMuzzlePosition()
        {
            if (muzzleTransform) return muzzleTransform.position;
            return transform.position + Vector3.up * 1.5f;
        }

        private void FireRocketAt(Vector3 targetPosition)
        {
            if (!config || !config.RocketProjectilePrefab)
            {
                return;
            }

            Vector3 spawn;
            if (rocketSpawnTransform) spawn = rocketSpawnTransform.position;
            else spawn = transform.position + Vector3.up * 2f;
            
            Vector3 toTarget = targetPosition - spawn;
            if (toTarget.sqrMagnitude < 0.001f)
            {
                toTarget = transform.forward;
            }

            Quaternion rotation = Quaternion.LookRotation(toTarget.normalized);
            GameObject rocket = Instantiate(config.RocketProjectilePrefab, spawn, rotation);

            BossRocketProjectile rocketProjectile = rocket.GetComponent<BossRocketProjectile>();
            if (rocketProjectile)
            {
                rocketProjectile.Initialize(
                    targetPosition,
                    config.RocketSpeed,
                    config.RocketDamage,
                    config.RocketExplosionRadius,
                    config.RocketLifetime,
                    gameObject);
            }
        }

        private void SpawnRocketTelegraph(Vector3 position)
        {
            if (!config || !config.RocketTelegraphPrefab)
            {
                return;
            }

            GameObject marker = Instantiate(config.RocketTelegraphPrefab, position, Quaternion.identity);
            Destroy(marker, config.RocketTelegraphDuration + 0.2f);
        }

        public void BeginEnragedPhase()
        {
            if (IsEnraged)
            {
                return;
            }

            IsEnraged = true;
            if (_agent && config)
            {
                _agent.speed = config.MoveSpeed * config.EnragedMoveSpeedMultiplier;
            }
        }

        public void EnterIdle()
        {
            SetMovementEnabled(false);
            PlayAnimation("Idle");
        }

        public bool ShouldChaseFromIdle()
        {
            if (!IsCombatAllowed || !PlayerTransform || !config || !CanSeePlayer())
            {
                return false;
            }

            float distance = DistanceToPlayer();
            return distance > config.ChaseMinDistance && distance < config.DetectionRadius;
        }

        public bool ShouldBasicAttackFromIdle()
        {
            if (!IsCombatAllowed || !PlayerTransform || !config || !CanSeePlayer())
            {
                return false;
            }

            float distance = DistanceToPlayer();
            return distance >= config.CombatMinDistance && distance <= config.CombatMaxDistance;
        }

        public void EnterChase()
        {
            SetMovementEnabled(true);
            PlayBossChaseAnimation();
        }

        public void ExitChase()
        {
            SetMovementEnabled(false);
        }

        public void UpdateChase()
        {
            if (!PlayerTransform)
            {
                return;
            }

            LookAtPlayer();
            MoveTo(PlayerTransform.position);
        }

        public bool ShouldReturnIdleFromChase()
        {
            return config && DistanceToPlayer() > config.DetectionRadius;
        }

        public bool CanAttackFromChase()
        {
            if (!config || !CanSeePlayer())
            {
                return false;
            }

            float distance = DistanceToPlayer();
            return distance >= config.CombatMinDistance && distance <= config.CombatMaxDistance;
        }

        public void EnterBasicAttack()
        {
            SetMovementEnabled(false);
            PlayBossBasicAttackAnimation();
            StopBasicAttackRoutine();
            _basicAttackRoutine = StartCoroutine(BasicAttackRoutine());
        }

        public void ExitBasicAttack()
        {
            StopBasicAttackRoutine();
        }

        private void StopBasicAttackRoutine()
        {
            if (_basicAttackRoutine != null)
            {
                StopCoroutine(_basicAttackRoutine);
                _basicAttackRoutine = null;
            }
        }

        public void UpdateBasicAttackFacing()
        {
            LookAtPlayer();
        }

        private IEnumerator BasicAttackRoutine()
        {
            if (!config)
            {
                yield break;
            }

            int minShots = Mathf.Max(1, config.BasicShotCountRange.x);
            int maxShots = Mathf.Max(minShots, config.BasicShotCountRange.y);
            int shotCount = Random.Range(minShots, maxShots + 1);

            for (int i = 0; i < shotCount; i++)
            {
                if (IsDead || !PlayerTransform)
                {
                    _fightStateMachine.ChangeState(_fightStateMachine.CreateIdleState());
                    yield break;
                }

                yield return new WaitForSeconds(GetEffectiveBasicAimDelay());

                Vector3 target = PlayerTransform.position + Vector3.up;
                GameObject prefab = GetBasicProjectilePrefab();
                FireProjectileAtTarget(prefab, GetEffectiveBasicDamage(), target);

                yield return new WaitForSeconds(GetEffectiveBasicInterval());
            }

            _fightStateMachine.ChangeState(_fightStateMachine.CreateChaseState());
        }

        public void EnterSuppressiveFire()
        {
            SetMovementEnabled(false);
            PlayBossSuppressiveAttackAnimation();
            StopSuppressiveFireRoutine();
            _suppressiveFireRoutine = StartCoroutine(SuppressiveFireRoutine());
        }

        public void ExitSuppressiveFire()
        {
            StopSuppressiveFireRoutine();
        }

        private void StopSuppressiveFireRoutine()
        {
            if (_suppressiveFireRoutine != null)
            {
                StopCoroutine(_suppressiveFireRoutine);
                _suppressiveFireRoutine = null;
            }
        }

        private IEnumerator SuppressiveFireRoutine()
        {
            if (!config)
            {
                yield break;
            }

            float duration = config.SuppressiveDuration + (IsEnraged ? config.EnragedSuppressiveBonusDuration : 0f);
            float shotInterval = GetEffectiveSuppressiveShotInterval();
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (IsDead || !PlayerTransform)
                {
                    _fightStateMachine.ChangeState(_fightStateMachine.CreateIdleState());
                    yield break;
                }

                if (!CanSeePlayer())
                {
                    _fightStateMachine.ChangeState(_fightStateMachine.CreateChaseState());
                    yield break;
                }

                LookAtPlayer();
                Vector3 muzzleOrigin = GetMuzzlePosition();
                Vector3 baseDirection = (PlayerTransform.position + Vector3.up - muzzleOrigin).normalized;
                Vector3 spreadDirection = Quaternion.Euler(
                    Random.Range(-config.SuppressiveSpreadAngle, config.SuppressiveSpreadAngle),
                    Random.Range(-config.SuppressiveSpreadAngle, config.SuppressiveSpreadAngle),
                    0f) * baseDirection;

                GameObject prefab = GetSuppressiveProjectilePrefab();
                FireProjectile(prefab, GetEffectiveSuppressiveDamage(), spreadDirection);
                yield return new WaitForSeconds(shotInterval);
                elapsed += shotInterval;
            }

            _fightStateMachine.ChangeState(_fightStateMachine.CreateOverheatState());
        }

        public void EnterRocketBarrage()
        {
            SetMovementEnabled(false);
            PlayAnimation("Attack");
            StopRocketBarrageRoutine();
            _rocketBarrageRoutine = StartCoroutine(RocketBarrageRoutine());
        }

        public void ExitRocketBarrage()
        {
            StopRocketBarrageRoutine();
        }

        private void StopRocketBarrageRoutine()
        {
            if (_rocketBarrageRoutine != null)
            {
                StopCoroutine(_rocketBarrageRoutine);
                _rocketBarrageRoutine = null;
            }
        }

        private IEnumerator RocketBarrageRoutine()
        {
            if (!config || !PlayerTransform)
            {
                _fightStateMachine.ChangeState(_fightStateMachine.CreateIdleState());
                yield break;
            }

            int min = Mathf.Max(1, config.RocketCountRange.x);
            int max = Mathf.Max(min, config.RocketCountRange.y);
            int count = Random.Range(min, max + 1);
            if (IsEnraged)
            {
                count += Mathf.Max(0, config.EnragedExtraRockets);
            }

            Vector3 playerCenter = PlayerTransform.position;
            Vector3[] targets = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                Vector2 rnd = Random.insideUnitCircle * config.RocketAreaRadius;
                targets[i] = playerCenter + new Vector3(rnd.x, 0f, rnd.y);
                SpawnRocketTelegraph(targets[i]);
            }

            yield return new WaitForSeconds(config.RocketTelegraphDuration);

            for (int i = 0; i < targets.Length; i++)
            {
                FireRocketAt(targets[i]);
                yield return new WaitForSeconds(0.12f);
            }

            LastRocketBarrageTime = Time.time;
            _fightStateMachine.ChangeState(_fightStateMachine.CreateOverheatState());
        }

        public void EnterOverheat()
        {
            SetMovementEnabled(false);
            PlayAnimation("Idle");
        }

        public float GetOverheatDuration()
        {
            if (!config)
            {
                return 3f;
            }

            if (IsEnraged)
            {
                return config.EnragedOverheatDuration;
            }

            return Random.Range(config.OverheatDurationRange.x, config.OverheatDurationRange.y);
        }

        public bool IsOverheatComplete(float enterTime, float duration) => Time.time >= enterTime + duration;

        public void EnterEnragedIntro()
        {
            SetMovementEnabled(false);
            PlayAnimation("Enraged");
            _enragedIntroStartTime = Time.time;
        }

        public bool IsEnragedIntroComplete()
        {
            float duration;
            if (config) duration = config.EnragedStateDuration;
            else duration = 0.8f;
            
            return Time.time >= _enragedIntroStartTime + duration;
        }

        public void EnterDeath()
        {
            SetMovementEnabled(false);
            if (_agent)
            {
                _agent.enabled = false;
            }

            PlayAnimation("Death");
            StartSelfDestruct();
        }

        private void TryFindPlayer()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject)
            {
                _playerTransform = playerObject.transform;
            }
        }

        private void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            _fightStateMachine.ChangeState(_fightStateMachine.CreateDeathState());
        }

        private void StartSelfDestruct()
        {
            if (_deathSequenceStarted || !config)
            {
                return;
            }

            _deathSequenceStarted = true;
            StartCoroutine(SelfDestructCoroutine());
        }

        private IEnumerator SelfDestructCoroutine()
        {
            yield return new WaitForSeconds(config.SelfDestructDelay);
            Destroy(gameObject);
        }
    }
}
