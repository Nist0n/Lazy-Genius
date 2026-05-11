using System;
using System.Collections;
using Core;
using Enemy.Boss.Projectiles;
using Player.Projectiles;
using UnityEngine;
using UnityEngine.AI;
using Enemy.Boss.States;
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
        
        public event Action<float> OnHealthChanged;

        public BossStateMachine StateMachine { get; private set; }
        public Transform PlayerTransform => _playerTransform;
        public NavMeshAgent Agent => _agent;

        public BossIdleState IdleState { get; private set; }
        public BossChaseState ChaseState { get; private set; }
        public BossBasicRangedAttackState BasicAttackState { get; private set; }
        public BossSuppressiveFireState SuppressiveFireState { get; private set; }
        public BossRocketBarrageState RocketBarrageState { get; private set; }
        public BossOverheatState OverheatState { get; private set; }
        public BossEnragedState EnragedState { get; private set; }
        public BossDeathState DeathState { get; private set; }

        public bool IsDead { get; private set; }
        public bool IsPeacefulModeEnabled { get; private set; }
        public bool IsEnraged { get; private set; }
        public bool EnragedPending { get; set; }
        public float LastRocketBarrageTime { get; set; } = -999f;
        public float GetHealth() => _currentHealth;
        public float GetMaxHealth()
        {
            if (config) return config.MaxHealth;
            else return 0f;
        }

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
            StateMachine = new BossStateMachine();
        }

        private void PickRandomWeaponAndElement()
        {
            if (weaponProfiles != null && weaponProfiles.Length > 0)
            {
                _weaponProfile = weaponProfiles[Random.Range(0, weaponProfiles.Length)];
                Debug.Log(_weaponProfile);
            }

            if (elementProfiles != null && elementProfiles.Length > 0)
            {
                _elementProfile = elementProfiles[Random.Range(0, elementProfiles.Length)];
                Debug.Log(_elementProfile);
            }
        }

        public GameObject GetBasicProjectilePrefab()
        {
            if (_elementProfile && _elementProfile.BasicProjectilePrefab)
            {
                return _elementProfile.BasicProjectilePrefab;
            }

            if (config) return config.BasicProjectilePrefab;
            else return null;
        }

        public GameObject GetSuppressiveProjectilePrefab()
        {
            if (_elementProfile && _elementProfile.SuppressiveProjectilePrefab)
            {
                return _elementProfile.SuppressiveProjectilePrefab;
            }

            if (config) return config.SuppressiveProjectilePrefab;
            else return null;
        }

        public float GetEffectiveBasicDamage()
        {
            float baseDamage;
            if (config) baseDamage = config.BasicAttackDamage;
            else baseDamage = 0f;
            
            float mul;
            if (_weaponProfile) mul = _weaponProfile.DamageMultiplier;
            else mul = 1f;
            
            return baseDamage * mul;
        }

        public float GetEffectiveSuppressiveDamage()
        {
            float baseDamage;
            if (config) baseDamage = config.SuppressiveDamage;
            else baseDamage = 0f;
            
            float mul;
            if (_weaponProfile) mul = _weaponProfile.DamageMultiplier;
            else mul = 1f;
            
            return baseDamage * mul;
        }

        public float GetEffectiveBasicAimDelay()
        {
            float baseDelay;
            if (config)
                if (IsEnraged) baseDelay = config.EnragedBasicAimDelay;
                else baseDelay = config.BasicAimDelay;
            else baseDelay = 0.5f;
            
            float speed;
            if (_weaponProfile) speed = _weaponProfile.AttackSpeedMultiplier;
            else speed = 1f;
            
            return baseDelay / speed;
        }

        public float GetEffectiveBasicInterval()
        {
            float baseInterval;
            if (config) baseInterval = config.BasicInterval;
            else baseInterval = 0.45f;
            
            float speed;
            if (_weaponProfile) speed = _weaponProfile.AttackSpeedMultiplier;
            else speed = 1f;
            
            return baseInterval / speed;
        }

        public float GetEffectiveSuppressiveShotInterval()
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

        public void PlayBossChaseAnimation()
        {
            string state;
            if (_weaponProfile) state = _weaponProfile.ChaseAnimState;
            else state = "Chase";
            PlayAnimation(state);
        }

        public void PlayBossBasicAttackAnimation()
        {
            string state;
            if (_weaponProfile) state = _weaponProfile.BasicAttackAnimState;
            else state = "Attack";
            PlayAnimation(state);
        }

        public void PlayBossSuppressiveAttackAnimation()
        {
            string state;
            if (_weaponProfile) state = _weaponProfile.SuppressiveAttackAnimState;
            else state = "Attack";
            PlayAnimation(state);
        }

        private void Start()
        {
            IdleState = new BossIdleState(this, StateMachine, config);
            ChaseState = new BossChaseState(this, StateMachine, config);
            BasicAttackState = new BossBasicRangedAttackState(this, StateMachine, config);
            SuppressiveFireState = new BossSuppressiveFireState(this, StateMachine, config);
            RocketBarrageState = new BossRocketBarrageState(this, StateMachine, config);
            OverheatState = new BossOverheatState(this, StateMachine, config);
            EnragedState = new BossEnragedState(this, StateMachine, config);
            DeathState = new BossDeathState(this, StateMachine, config);

            StateMachine.Initialize(IdleState);
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

            if (!IsEnraged && _currentHealth > 0f && _currentHealth <= GetMaxHealth() * config.EnragedThresholdNormalized)
            {
                EnragedPending = true;
            }

            StateMachine.CurrentState?.LogicUpdate();
            currentStateName = StateMachine.CurrentState?.GetType().Name ?? string.Empty;
        }

        public void TakeDamage(float damage, DamageInfo damageInfo)
        {
            if (IsDead)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            
            OnHealthChanged?.Invoke(_currentHealth);
            
            StateMachine.ChangeState(ChaseState);
            
            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        public bool CanSeePlayer()
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

        public void LookAtPlayer(float rotationSpeed = 7f)
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

        public float DistanceToPlayer()
        {
            if (!_playerTransform)
            {
                return float.MaxValue;
            }

            return Vector3.Distance(transform.position, _playerTransform.position);
        }

        public bool IsRocketReady()
        {
            return Time.time >= LastRocketBarrageTime + config.RocketCooldown;
        }

        public void SetMovementEnabled(bool enabled)
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

        public void MoveTo(Vector3 destination)
        {
            if (!_agent || !_agent.enabled || !_agent.isOnNavMesh)
            {
                return;
            }

            _agent.SetDestination(destination);
        }

        public void PlayAnimation(string stateName)
        {
            if (_animator && !string.IsNullOrWhiteSpace(stateName))
            {
                _animator.Play(stateName);
            }
        }

        public void FireProjectile(GameObject projectilePrefab, float damage, Vector3 direction, float upwardBias = 0f)
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

        public void FireProjectileAtTarget(GameObject projectilePrefab, float damage, Vector3 targetPosition, float upwardBias = 0f)
        {
            Vector3 origin = GetMuzzlePosition();
            Vector3 direction = targetPosition - origin;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = transform.forward;
            }

            FireProjectile(projectilePrefab, damage, direction, upwardBias);
        }

        public Vector3 GetMuzzlePosition()
        {
            if (muzzleTransform) return muzzleTransform.position;
            return transform.position + Vector3.up * 1.5f;
        }

        public void FireRocketAt(Vector3 targetPosition)
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
                return;
            }
        }

        public void SpawnRocketTelegraph(Vector3 position)
        {
            if (!config || !config.RocketTelegraphPrefab)
            {
                return;
            }

            GameObject marker = Instantiate(config.RocketTelegraphPrefab, position, Quaternion.identity);
            Destroy(marker, config.RocketTelegraphDuration + 0.2f);
        }

        public void ApplyEnragedModifiers()
        {
            if (IsEnraged)
            {
                return;
            }

            IsEnraged = true;
            if (_agent)
            {
                _agent.speed = config.MoveSpeed * config.EnragedMoveSpeedMultiplier;
            }
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
            EnragedPending = false;
            StateMachine.ChangeState(DeathState);
        }

        public void StartSelfDestruct()
        {
            if (_deathSequenceStarted)
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
