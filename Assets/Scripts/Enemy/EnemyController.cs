using Core;
using Enemy.States;
using SaveSystem;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyHealth))]
    public abstract class EnemyController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private EnemyConfig enemyConfig;
        public EnemyConfig EnemyConfig => enemyConfig;

        [Header("Debug Info")]
        [SerializeField] private string currentStateName;

        public Rigidbody Rb { get; private set; }
        public AudioSource SoundSource;
        public AudioSource StepsSource;
        public NavMeshAgent Agent { get; private set; }
        public EnemyHealth Health { get; private set; }
        public Animator Anim { get; private set; }
        public Transform PlayerTransform { get; private set; }

        [Header("Combat Runtime Overrides")]
        [SerializeField] private string defaultAttackAnimState = "Attack";
        protected string AttackAnimOverride;
        protected float DamageMultiplier = 1f;
        protected float AttackSpeedMultiplier = 1f;

        private EnemyFightStateMachine _fightStateMachine;
        private int _playerSearchCooldown;
        private float _getHitEnterTime;

        public EnemyFightStateMachine FightStateMachine => _fightStateMachine;

        public float EffectiveAttackDamage
        {
            get
            {
                float baseDamage = enemyConfig ? enemyConfig.AttackDamage : 10f;
                return baseDamage * Mathf.Max(0f, DamageMultiplier);
            }
        }

        public float EffectiveAttackCooldown
        {
            get
            {
                float baseCooldown = enemyConfig ? enemyConfig.AttackCooldown : 1f;
                float speed = Mathf.Max(0.0001f, AttackSpeedMultiplier);
                return baseCooldown / speed;
            }
        }

        public string AttackAnimState =>
            string.IsNullOrWhiteSpace(AttackAnimOverride) ? defaultAttackAnimState : AttackAnimOverride;

        public bool IsPeacefulModeEnabled { get; private set; }

        public bool ShouldAvoidByLowHealth
        {
            get
            {
                if (!Health) return false;
                if (Health.MaxHealth <= 0f) return false;
                return Health.CurrentHealth <= Health.MaxHealth * 0.25f;
            }
        }

        protected abstract EnemyFightStateMachine CreateFightStateMachine();
        protected abstract void StopCombatRoutines();

        private void OnEnable()
        {
            Health.OnDamageTaken += OnDamageTaken;
        }

        private void OnDisable()
        {
            Health.OnDamageTaken -= OnDamageTaken;
        }

        private void Awake()
        {
            Health = GetComponent<EnemyHealth>();
            Rb = GetComponent<Rigidbody>();
            Agent = GetComponent<NavMeshAgent>();
            Anim = GetComponentInChildren<Animator>();

            AttackAnimOverride = defaultAttackAnimState;
            StepsSource.enabled = false;

            float maxHp = enemyConfig && enemyConfig.MaxHealth > 0f ? enemyConfig.MaxHealth : 100f;
            Health.Initialize(maxHp);

            if (enemyConfig)
            {
                Agent.speed = enemyConfig.MoveSpeed;
            }

            IsPeacefulModeEnabled = CharacterManager.Instance
                                    && CharacterManager.Instance.HasActiveCharacter
                                    && CharacterManager.Instance.ActiveCharacter != null
                                    && CharacterManager.Instance.ActiveCharacter.PeacefulModeEnabled;

            TryFindPlayer();
        }

        private void Start()
        {
            SetFightStateMachine(CreateFightStateMachine());
        }

        public void SetFightStateMachine(EnemyFightStateMachine fightStateMachine)
        {
            _fightStateMachine?.CurrentState?.Exit();
            StopCombatRoutines();
            _fightStateMachine = fightStateMachine;
            _fightStateMachine.Initialize();
        }

        public virtual void ResetCombatOverrides()
        {
            DamageMultiplier = 1f;
            AttackSpeedMultiplier = 1f;
            AttackAnimOverride = defaultAttackAnimState;
        }

        public virtual void ApplyCombatOverrides(
            float damageMultiplier,
            float attackSpeedMultiplier,
            string attackAnimState,
            string chaseAnimState = null)
        {
            DamageMultiplier = Mathf.Max(0f, damageMultiplier);
            AttackSpeedMultiplier = Mathf.Max(0.0001f, attackSpeedMultiplier);
            AttackAnimOverride = attackAnimState;
        }

        private void Update()
        {
            if (!PlayerTransform)
            {
                _playerSearchCooldown--;
                if (_playerSearchCooldown <= 0)
                {
                    _playerSearchCooldown = 30;
                    TryFindPlayer();
                }
            }

            if (_fightStateMachine?.CurrentState != null)
            {
                _fightStateMachine.CurrentState.LogicUpdate();
                currentStateName = _fightStateMachine.CurrentState.GetType().Name;
            }
        }

        private void FixedUpdate()
        {
            _fightStateMachine?.CurrentState?.PhysicsUpdate();
        }

        private void OnCollisionEnter(Collision collision)
        {
            _fightStateMachine?.CurrentState?.OnCollisionEnter(collision);
        }

        public void OnDeath()
        {
            _fightStateMachine.ChangeState(_fightStateMachine.CreateDeathState());
        }

        private void OnDamageTaken(DamageInfo damageInfo)
        {
            _fightStateMachine.ChangeState(_fightStateMachine.CreateGetHitState());
        }

        private void TryFindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj)
            {
                PlayerTransform = playerObj.transform;
            }
        }

        public bool CanSeePlayer()
        {
            if (!PlayerTransform) return false;

            float detectionRadius = enemyConfig ? enemyConfig.DetectionRadius : 10f;
            float distance = Vector3.Distance(transform.position, PlayerTransform.position);
            if (distance > detectionRadius)
            {
                return false;
            }

            Vector3 directionToPlayer = (PlayerTransform.position - transform.position).normalized;
            Vector3 planeDirection = directionToPlayer;
            planeDirection.y = 0;
            if (planeDirection == Vector3.zero) planeDirection = transform.forward;

            Vector3 flatForward = transform.forward;
            flatForward.y = 0;
            if (flatForward == Vector3.zero) flatForward = Vector3.forward;

            float angle = Vector3.Angle(flatForward, planeDirection);
            float fov = enemyConfig ? enemyConfig.FieldOfView : 110f;

            if (angle > fov * 0.5f)
            {
                return false;
            }

            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 target = PlayerTransform.position + Vector3.up * 1.0f;
            Vector3 rayDirection = target - origin;

            if (Physics.Raycast(origin, rayDirection, out RaycastHit hit, distance + 1.0f, Physics.AllLayers, QueryTriggerInteraction.Collide))
            {
                if (hit.transform == PlayerTransform || hit.transform.root == PlayerTransform.root || hit.transform.CompareTag("Player"))
                {
                    return true;
                }

                if (hit.transform.IsChildOf(transform))
                {
                    Vector3 forwardOrigin = origin + transform.forward * 0.5f;
                    Vector3 fwdDirection = target - forwardOrigin;

                    if (Physics.Raycast(forwardOrigin, fwdDirection, out RaycastHit hit2, distance + 1.0f, Physics.AllLayers, QueryTriggerInteraction.Collide))
                    {
                        if (hit2.transform == PlayerTransform || hit2.transform.CompareTag("Player"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public float GetDistanceToPlayer()
        {
            if (!PlayerTransform) return float.MaxValue;
            return Vector3.Distance(transform.position, PlayerTransform.position);
        }

        public float GetFlatDistanceToPlayer()
        {
            if (!PlayerTransform) return float.MaxValue;
            Vector3 toPlayer = PlayerTransform.position - transform.position;
            Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
            return flatDir.magnitude;
        }

        public float GetAttackRange() => enemyConfig ? enemyConfig.AttackRange : 4f;

        public float GetDetectionRadius() => enemyConfig ? enemyConfig.DetectionRadius : 10f;

        public bool ShouldEnterAvoidFromIdle() =>
            IsPeacefulModeEnabled && ShouldAvoidByLowHealth && PlayerTransform;

        public bool ShouldEngageFromIdle() =>
            !IsPeacefulModeEnabled && PlayerTransform && CanSeePlayer();

        public virtual bool ShouldChaseAfterHit()
        {
            if (IsPeacefulModeEnabled) return false;
            return GetDistanceToPlayer() > GetAttackRange();
        }

        public virtual bool ShouldCombatAfterHit()
        {
            if (IsPeacefulModeEnabled) return false;
            return GetDistanceToPlayer() <= GetAttackRange();
        }

        public bool ShouldFleeAfterHit() =>
            IsPeacefulModeEnabled && ShouldAvoidByLowHealth;

        public bool ShouldReturnIdleAfterHit() =>
            IsPeacefulModeEnabled && !ShouldAvoidByLowHealth;

        public virtual bool ShouldAttackFromChase() => false;

        public bool IsGetHitRecoveryComplete()
        {
            float cooldown = enemyConfig ? enemyConfig.GetHitCooldown : 0.5f;
            return Time.time >= _getHitEnterTime + cooldown;
        }

        public void EnterIdle()
        {
            if (Agent) Agent.enabled = true;
            if (Agent && Agent.isOnNavMesh) Agent.isStopped = true;
            if (Anim) Anim.Play("Idle");
        }

        public void ExitIdle()
        {
            if (Agent && Agent.isOnNavMesh) Agent.isStopped = false;
        }

        public void EnterChase()
        {
            StepsSource.enabled = true;
            if (Agent) Agent.enabled = true;
            if (Agent && Agent.isOnNavMesh) Agent.isStopped = false;
            if (Anim) Anim.Play("Chase");
        }

        public void ExitChase()
        {
            StepsSource.enabled = false;
            if (Agent && Agent.isOnNavMesh) Agent.isStopped = true;
        }

        public void UpdateChaseMovement()
        {
            if (!PlayerTransform || !Agent) return;
            Agent.SetDestination(PlayerTransform.position);
        }

        public bool ShouldLosePlayerFromChase()
        {
            return GetDistanceToPlayer() > GetDetectionRadius() * 1.5f;
        }

        public void EnterGetHit()
        {
            if (Agent) Agent.enabled = false;
            if (Anim) Anim.Play("GetHit");
            _getHitEnterTime = Time.time;
        }

        public void EnterDeath()
        {
            if (Agent) Agent.enabled = false;
            if (Anim) Anim.Play("Death");
        }

        public void EnterAvoid()
        {
            if (Agent) Agent.enabled = true;
            if (Agent && Agent.isOnNavMesh) Agent.isStopped = false;
        }

        public void ExitAvoid()
        {
            if (Agent && Agent.isOnNavMesh) Agent.isStopped = false;
        }

        public bool ShouldStopAvoiding()
        {
            if (!PlayerTransform) return true;
            return !IsPeacefulModeEnabled || !ShouldAvoidByLowHealth;
        }

        public void UpdateAvoidMovement()
        {
            if (!PlayerTransform || !Agent || !Agent.isOnNavMesh || !enemyConfig)
            {
                return;
            }

            LookAtPlayerFlat(5f);

            Vector3 toPlayer = PlayerTransform.position - transform.position;
            Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
            float distance = flatDir.magnitude;

            float min = Mathf.Max(0.1f, enemyConfig.PreferredMinDistance);
            float max = Mathf.Max(min + 0.1f, enemyConfig.PreferredMaxDistance);
            float runAway = Mathf.Max(0.1f, enemyConfig.RunAwayDistance);

            Vector3 enemyPos = transform.position;
            Vector3 playerPos = PlayerTransform.position;
            Vector3 dirToPlayer = (playerPos - enemyPos).normalized;
            Vector3 dirAwayFromPlayer = -dirToPlayer;

            float desiredDistance = min + (max - min) * 0.5f;

            if (distance >= desiredDistance && distance >= runAway)
            {
                Agent.isStopped = true;
                if (Anim) Anim.Play("Idle");
                return;
            }

            float moveDistance = Mathf.Clamp(desiredDistance - distance, 2f, desiredDistance);
            Vector3 targetPos = enemyPos + dirAwayFromPlayer * moveDistance;

            if (Anim) Anim.Play("Chase");
            Agent.isStopped = false;
            Agent.SetDestination(targetPos);
        }

        protected void LookAtPlayerFlat(float rotationSpeed)
        {
            if (!PlayerTransform) return;

            Vector3 toPlayer = PlayerTransform.position - transform.position;
            Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (flatDir.sqrMagnitude < 0.001f) return;

            Quaternion lookRotation = Quaternion.LookRotation(flatDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
