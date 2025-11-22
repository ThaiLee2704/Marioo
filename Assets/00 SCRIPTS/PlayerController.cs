using UnityEngine;
using Spine.Unity;
using System.Collections.Generic;

public enum PlayerState { Idle, Move, Jump_up, Falling, Attack, Dead}

public class PlayerController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] SkeletonAnimation _skeleton;

    [Header("Player Information")]
    [SerializeField] float _mySpeed = 0f;
    [SerializeField] float _jumpForce = 0f;

    private Rigidbody2D _myRigid2D;
    private PlayerState _myState;
    private bool _onGround;

    [Header("List Animation in State")]
    [SerializeField] private List<string> _bufferAnimation = new List<string>();

    private Dictionary<PlayerState, string> _prefixMap;
    //GIẢI THÍCH VỀ RANDOM ANIMATION ON STATE:
    /*
    Bài toán: Có rất nhiều animation trong spine,  muốn random kiểu animation trong mỗi lần thực hiện state đó
     */

    /*
    Chúng ta tạo ra 1 Dictionary có key là PlayerState và value kiểu string là các tiền tố (Prefix) của các Animation
    Chúng ta có 1 list kiểu string để hứng các Animation có tiền tố như trên
    Đầu tiên khởi tạo Dictionary gồm các key là PlayerState và value là tiền tố của các Animation trong cái PlayerState đó
    Viết 1 hàm RandomAnimationInState(PlayerState) dùng để random 1 trong những animtion của state đó
    Lấy thử trong cái State đó có cái animation nào không, nếu không có thì Log ra là không có anim và return
    Mỗi lần gọi hàm RandomAnimationInState() thì ta cần phải Clear cái List<string> để hứng value từ Dictionary
    Tạo 1 biến kiểu var để hứng hết tất cả dữ liệu Animation có trong Spine đó (dạng như 1 list)
    Chạy foreach trong list này để lấy ra những Animation nào có tiền tố trùng khớp và add nó vào List<string>
    Lúc này chúng ta chỉ cần random ở trong List<string> là được.
     */

    private void Awake()
    {
        _myRigid2D = this.GetComponent<Rigidbody2D>();

        _prefixMap = new Dictionary<PlayerState, string>()
        {
            { PlayerState.Idle, "idle" },
            { PlayerState.Move, "run" },
            { PlayerState.Jump_up, "jump" },
            { PlayerState.Falling, "falldown" },
            { PlayerState.Attack, "attack" },       //Test thử không có Animation prefix(tiền tố) là "attack"
            { PlayerState.Dead, "die" },

        };
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.OnPlayerMove();
        this.OnPlayerJump();
    }

    #region RANDOM ANIMATION IN STATE
    private void RandomAnimationInState(PlayerState state)
    {
        if (!_prefixMap.TryGetValue(state, out var prefix))
        {
            Debug.LogWarning($"State {state} have not prefix {prefix}"); //Cái state này không có Animation nào prefix(tiền tố) bắt đầu là như này cả
            return;
        }

        //Xóa đi cái List<string> của State cũ
        _bufferAnimation.Clear();

        //Lấy toàn bộ Animation từ skeleton
        var animations = _skeleton.Skeleton.Data.Animations;

        //Chạy for trong toàn bộ các Animation lấy Animation có prefix(tiền tố) như thế và add nó vào cái List _bufferAnimation
        foreach (var anim in animations)
        {
            if (anim.Name.StartsWith(prefix))
                _bufferAnimation.Add(anim.Name);
        }

        if (_bufferAnimation.Count == 0)
        {
            Debug.LogWarning($"Do not find animation originating with prefix {prefix}");
            return;
        }

        //Random animation
        string animName = _bufferAnimation[Random.Range(0, _bufferAnimation.Count)];
        _skeleton.AnimationName = animName;
    }
    #endregion

    #region CHANGE STATE
    private void OnChangeState(PlayerState newState)
    {
        if (_myState == newState)
            return;

        _myState = newState;

        switch (_myState)
        {
            case PlayerState.Idle:
                RandomAnimationInState(PlayerState.Idle);
                break;

            case PlayerState.Move:
                RandomAnimationInState(PlayerState.Move);
                break;

            case PlayerState.Jump_up:
                RandomAnimationInState(PlayerState.Jump_up);
                break;

            case PlayerState.Falling:
                RandomAnimationInState(PlayerState.Falling);
                break;

            case PlayerState.Attack:
                RandomAnimationInState(PlayerState.Attack);
                break;

            case PlayerState.Dead:
                RandomAnimationInState(PlayerState.Dead);
                break;
        }
    }
    #endregion

    #region HANDLE MOVE AND ROTATE LEFT/RIGHT DIRECTION 
    private void OnPlayerMove()
    {
        float moveX = Input.GetAxis("Horizontal");
        _myRigid2D.linearVelocity = new Vector2(moveX * _mySpeed, _myRigid2D.linearVelocityY);

        if (moveX != 0)
        {
            if (_onGround) this.OnChangeState(PlayerState.Move);
            else
            {
                if (_myRigid2D.linearVelocityY > 0) this.OnChangeState(PlayerState.Jump_up);
                else this.OnChangeState(PlayerState.Falling);
            }

            //Xoay nhân vật trái phải
            float euler = moveX > 0 ? 0f : 180f;
            _skeleton.transform.parent.eulerAngles = Vector3.up * euler;
        }
        else
        {
            if(_onGround) this.OnChangeState(PlayerState.Idle);
            else
            {
                if (_myRigid2D.linearVelocityY > 0) this.OnChangeState(PlayerState.Jump_up);
                else this.OnChangeState(PlayerState.Falling);
            }
        }
    }
    #endregion

    #region HANDLE JUMP
    void OnPlayerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_onGround)
                _myRigid2D.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }
    }
    #endregion

    #region CHECK ON GROUND
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Ground")
        {
            if(collision.transform.position.y < this.transform.position.y)
                _onGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Ground")
            _onGround =false;
    }
    #endregion
}
