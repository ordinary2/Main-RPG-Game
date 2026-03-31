using UnityEngine;

public class Enemy_Slime : Enemy, ICounterable
{
    public bool canBeCountered { get => canBeStunned; }
    public Enemy_SlimeDeadState slimeDeadState { get; set; }
    
    [Header("Slime specifics")]
    [SerializeField] private GameObject slimeToCreatePrefab;
    [SerializeField] private int amountOfSlimesToCreate = 2;
    [SerializeField] private Vector2 newSlimeVelocity;
    
    [SerializeField] private bool hasRecoveryAnimation = true;
        protected override void Awake()
        {
            base.Awake();
    
            idleState = new Enemy_IdleState(this, stateMachine, "idle");
            moveState = new Enemy_MoveState(this, stateMachine, "move");
            attackState = new Enemy_AttackState(this, stateMachine, "attack");
            battleState = new Enemy_BattleState(this, stateMachine, "battle");
            stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");
            slimeDeadState = new Enemy_SlimeDeadState(this, stateMachine, "idle");
            
            anim.SetBool("hasStunRecovery", hasRecoveryAnimation);
        }
    
        protected override void Start()
        {
            base.Start();
            
            stateMachine.Initialize(idleState);
        }

        public override void EntityDeath()
        {
            stateMachine.ChangeState(slimeDeadState);
        }

        public void HandleCounter()
        {
            if (canBeCountered == false)
                return;
            
            stateMachine.ChangeState(stunnedState);
        }

        public void CreateSlimeOnDeath()
        {
            if(slimeToCreatePrefab == null)
                return;

            for (int i = 0; i < amountOfSlimesToCreate; i++)
            {
                GameObject newSlime = Instantiate(slimeToCreatePrefab, transform.position, Quaternion.identity);
                Enemy_Slime slimeScript = newSlime.GetComponent<Enemy_Slime>();

                slimeScript.stats.AdjustStatSetup(stats.resources, stats.offense, stats.defense, .6f, 1.2f);
                slimeScript.ApplyRespawnVelocity();
                slimeScript.StartBattleStateCheck(player);
            }
        }

        public void ApplyRespawnVelocity()
        {
            Vector2 velocity = new Vector2(stunnedVelocity.x * Random.Range(-1f, 1f), stunnedVelocity.y * Random.Range(1f, 2f));
        }

        public void StartBattleStateCheck(Transform player)
        {
            TryEnterBattleState(player);
            InvokeRepeating(nameof(ReEnterBattleState), 0, .3f);
        }

        private void ReEnterBattleState()
        {
            if (stateMachine.currentState == battleState || stateMachine.currentState == attackState)
            {
                CancelInvoke(nameof(ReEnterBattleState));
                return;
            }
            
            stateMachine.ChangeState(battleState);
        }

        /*public void SetupSlime(Vector2 velocity, Entity_Stats newStats)
        {
            float xVelocity = velocity.x * Random.Range(-2, 2);
            float yVelocity = velocity.y * Random.Range(1, 2);
            
            rb.linearVelocity = new Vector2(xVelocity, yVelocity);
            
            stats.AdjustStatSetup(stats.resources, stats.offense, stats.defense, .6f, 1.2f);
        }*/
}
