using System;
using UnityEngine;
using UnityEngine.AI;
using Core;
using Enemy.States;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private EnemyConfig enemyConfig;
        
        [Header("Debug Info")]
        [SerializeField] private string currentStateName;
        
        public Rigidbody Rb { get; private set; }
        public AudioSource SoundSource;
        public AudioSource StepsSource;
        public NavMeshAgent Agent { get; private set; }
        public EnemyHealth Health { get; private set; }
        public Animator Anim { get; private set; } // Optional
        public Transform PlayerTransform { get; private set; }
        
        public EnemyStateMachine StateMachine { get; private set; }
        
        public EnemyIdleState IdleState { get; private set; }
        public EnemyDeathState DeathState { get; private set; }
        
        private int _playerSearchCooldown;
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        public EnemyGetHitState GetHitState { get; private set; }

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

            StepsSource.enabled = false;

            float maxHp;
            if (enemyConfig && enemyConfig.MaxHealth > 0f) maxHp = enemyConfig.MaxHealth;
            else maxHp = 100f;
            Health.Initialize(maxHp);

            if (enemyConfig)
            {
                Agent.speed = enemyConfig.MoveSpeed;
            }
            
            TryFindPlayer();
            StateMachine = new EnemyStateMachine();
        }
        
        private void TryFindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj)
            {
                PlayerTransform = playerObj.transform;
            }
        }

        private void Start()
        {
            IdleState = new EnemyIdleState(this, StateMachine, enemyConfig);
            ChaseState = new EnemyChaseState(this, StateMachine, enemyConfig);
            AttackState = new EnemyAttackState(this, StateMachine, enemyConfig);
            DeathState = new EnemyDeathState(this, StateMachine, enemyConfig);
            GetHitState = new EnemyGetHitState(this, StateMachine, enemyConfig);
            
            StateMachine.Initialize(IdleState);
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

        public void Attack ()=> AttackState.Attack();

        private void GetHit(DamageInfo damageInfo)
        {
            Debug.Log("GetHit");
            StateMachine.ChangeState(GetHitState);
        }
    }
}
