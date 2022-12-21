using Spine.Unity;
using UnityEngine;

public class CharacterSpine : MonoBehaviour
{
    public SkeletonAnimation skeltonAnimation;
    public AnimationReferenceAsset[] animClips;

    public enum AnimState
    {
        Animation,
        Attack,
        Dead,
        Idle,
        Walk,

    }
    private AnimState _AnimState;

    private string _CurrentAnimation;
    private bool isEndAnim = true;

    private Rigidbody2D rig;
    private float xx, yy;

    private void Awake() => rig = GetComponent<Rigidbody2D>();

    private void Update()
    {
        if (isEndAnim == false)
            return;

        xx = Input.GetAxisRaw("Horizontal");
        yy = Input.GetAxisRaw("Vertical");
        bool attack = Input.GetKeyDown("space");
        bool die = Input.GetKeyDown("q");
        if (attack == true)
        {
            _AnimState = AnimState.Attack;
            // isEndAnim = false;
        }
            
        else if (die == true)
        {
            _AnimState = AnimState.Dead;
            // isEndAnim = false;
        }
            
        else if (xx != 0f)
        {
            _AnimState = AnimState.Walk;
            // TODO: rotate back
        }
        else
        {
            _AnimState = AnimState.Idle;
        }

        SetCurrentAnimation(_AnimState);
    }
    private void FixedUpdate()
    {
        rig.velocity = new Vector2(xx * 300 * Time.deltaTime, yy * 300 * Time.deltaTime);
        rig.gravityScale = 1;
    }

    private void _AsyncAnimation(AnimationReferenceAsset animClip, bool loop, float timeScale)
    {
        if (animClip.name.Equals(_CurrentAnimation))
        {
            return;
        }

        if (animClip.name.Equals("attack") || animClip.name.Equals("dead"))
        {
            isEndAnim = false;
        }
        

        skeltonAnimation.state.SetAnimation(0, animClip, loop).TimeScale = timeScale;
        skeltonAnimation.timeScale = timeScale;
        skeltonAnimation.loop = loop;
        skeltonAnimation.state.Complete += EndAnimationCb;

        _CurrentAnimation = animClip.name;
    }

    private void SetCurrentAnimation(AnimState _state)
    {
        _AsyncAnimation(animClips[(int)_state], true, 1f); 
    }

    private void EndAnimationCb(Spine.TrackEntry te)
    {
        isEndAnim = true;
    }
}
