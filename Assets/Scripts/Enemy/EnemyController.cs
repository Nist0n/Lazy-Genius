using UnityEngine;
using UnityEngine.AI;
using Core;
using Enemy.States;
using SaveSystem;

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
        public Animator Anim { get; private set; } // Optional
        public Transform PlayerTransform { get; private set; }

        [Header("Combat Runtime Overrides")]
        [SerializeField] private string defaultAttackAnimState = "Attack";
        [SerializeField] private string defaultChaseAnimState = "Chase";
        private string _attackAnimState;
        private string _chaseAnimState;
        private float _damageMultiplier = 1f;
        private float _attackSpeedMultiplier = 1f;

        public float EffectiveAttackDamage
        {
            get
            {
                float baseDamage;
                if (enemyConfig) baseDamage = enemyConfig.AttackDamage;
                else baseDamage = 10f;
                
                return baseDamage * Mathf.Max(0f, _damageMultiplier);
            }
        }

        public float EffectiveAttackCooldown
        {
            get
            {
                float baseCooldown;
                if (enemyConfig) baseCooldown = enemyConfig.AttackCooldown;
                else baseCooldown = 1f;
                
                float speed = Mathf.Max(0.0001f, _attackSpeedMultiplier);
                
                return baseCooldown / speed;
            }
        }

        public float EffectiveProjectileDamage
        {
            get
            {
                if (!enemyConfig) return EffectiveAttackDamage;
                
                float baseDamage;
                if (enemyConfig.ProjectileDamage > 0f) baseDamage = enemyConfig.ProjectileDamage;
                else baseDamage = enemyConfig.AttackDamage;
                
                return baseDamage * Mathf.Max(0f, _damageMultiplier);
            }
        }

        public string AttackAnimState => string.IsNullOrWhiteSpace(_attackAnimState) ? defaultAttackAnimState : _attackAnimState;
        public string ChaseAnimState => string.IsNullOrWhiteSpace(_chaseAnimState) ? defaultChaseAnimState : _chaseAnimState;
        public EnemyStateMachine StateMachine { get; private set; }
        public EnemyIdleState IdleState { get; private set; }
        public EnemyDeathState DeathState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; protected set; }
        public EnemyGetHitState GetHitState { get; private set; }
        public EnemyRangedCombatState RangedCombatState { get; protected set; }
        public EnemyAvoidState AvoidState { get; private set; }
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
        
        private int _playerSearchCooldown;

        private void OnEnable()
        {
            Health.OnDamageTaken += GetHit;
        }

        private void OnDisable()
        {
            Health.OnDamageTaken -= GetHit;
        }

        private void Awake()
        {
            Health = GetComponent<EnemyHealth>();
            Rb = GetComponent<Rigidbody>();
            Agent = GetComponent<NavMeshAgent>();
            Anim = GetComponentInChildren<Animator>();

            _attackAnimState = defaultAttackAnimState;
            _chaseAnimState = defaultChaseAnimState;

            StepsSource.enabled = false;

            float maxHp;
            if (enemyConfig && enemyConfig.MaxHealth > 0f) maxHp = enemyConfig.MaxHealth;
            else maxHp = 100f;
            Health.Initialize(maxHp);

            if (enemyConfig)
            {
                Agent.speed = enemyConfig.MoveSpeed;
            }

            IsPeacefulModeEnabled = CharacterManager.Instance && CharacterManager.Instance.HasActiveCharacter && CharacterManager.Instance.ActiveCharacter != null && CharacterManager.Instance.ActiveCharacter.PeacefulModeEnabled;
            
            TryFindPlayer();
            StateMachine = new EnemyStateMachine();
        }

        private void Start()
        {
            IdleState = new EnemyIdleState(this, StateMachine, enemyConfig);
            ChaseState = new EnemyChaseState(this, StateMachine, enemyConfig);
            DeathState = new EnemyDeathState(this, StateMachine, enemyConfig);
            GetHitState = new EnemyGetHitState(this, StateMachine, enemyConfig);
            AvoidState = new EnemyAvoidState(this, StateMachine, enemyConfig);
            
            CreateCombatStates();
            
            StateMachine.Initialize(IdleState);
        }
        
        public void ResetCombatOverrides()
        {
            _damageMultiplier = 1f;
            _attackSpeedMultiplier = 1f;
            _attackAnimState = defaultAttackAnimState;
            _chaseAnimState = defaultChaseAnimState;
        }
        
        private void TryFindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj)
            {
                PlayerTransform = playerObj.transform;
            }
        }

        public void ApplyCombatOverrides(float damageMultiplier, float attackSpeedMultiplier, string attackAnimState, string chaseAnimState)
        {
            _damageMultiplier = Mathf.Max(0f, damageMultiplier);
            _attackSpeedMultiplier = Mathf.Max(0.0001f, attackSpeedMultiplier);
            _attackAnimState = attackAnimState;
            _chaseAnimState = chaseAnimState;
        }

        protected virtual void CreateCombatStates()
        {
            AttackState = new EnemyAttackState(this, StateMachine, enemyConfig);
            RangedCombatState = null;
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
            
            if (StateMachine.CurrentState != null)
            {
                StateMachine.CurrentState.LogicUpdate();
                currentStateName = StateMachine.CurrentState.GetType().Name;
            }
        }

        private void FixedUpdate()
        {
            if (StateMachine.CurrentState != null)
            {
                StateMachine.CurrentState.PhysicsUpdate();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            StateMachine.CurrentState?.OnCollisionEnter(collision);
        }

        public void OnDeath()
        {
            StateMachine.ChangeState(DeathState);
        }

        public bool CanSeePlayer()
        {
            if (!PlayerTransform) return false;
            
            float detectionRadius;
            if (enemyConfig) detectionRadius = enemyConfig.DetectionRadius;
            else detectionRadius = 10f;
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
            
            float fov;
            if (enemyConfig) fov = enemyConfig.FieldOfView;
            else fov = 110f;
            
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
                    
                    Debug.DrawLine(forwardOrigin, target, Color.red);

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

        private void GetHit(DamageInfo damageInfo)
        {
            StateMachine.ChangeState(GetHitState);
        }

        public virtual EnemyState GetInitialEngageState()
        {
            if (IsPeacefulModeEnabled)
            {
                if (ShouldAvoidByLowHealth) return AvoidState;
            }
            
            return ChaseState;
        }

        public virtual EnemyState GetPostHitState(float distanceToPlayer)
        {
            if (IsPeacefulModeEnabled)
            {
                if (ShouldAvoidByLowHealth) return AvoidState;
                
                return IdleState;
            }

            float attackRange;
            if (enemyConfig) attackRange = enemyConfig.AttackRange;
            else attackRange = 4f;

            if (distanceToPlayer > attackRange) return ChaseState;
            
            return AttackState;
        }
    }
}
