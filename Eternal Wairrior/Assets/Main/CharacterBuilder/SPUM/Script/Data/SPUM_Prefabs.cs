using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
public enum PlayerState
{
    IDLE,
    MOVE,
    ATTACK,
    DAMAGED,
    DEBUFF,
    DEATH,
    OTHER,
}
public class SPUM_Prefabs : MonoBehaviour
{
    public float _version;
    public bool EditChk;
    public string _code;
    public Animator _anim;
    private AnimatorOverrideController OverrideController;

    public string UnitType;
    public List<SpumPackage> spumPackages = new List<SpumPackage>();
    public List<PreviewMatchingElement> ImageElement = new();
    public List<SPUM_AnimationData> SpumAnimationData = new();
    public Dictionary<string, List<AnimationClip>> StateAnimationPairs = new();
    public List<AnimationClip> IDLE_List = new();
    public List<AnimationClip> MOVE_List = new();
    public List<AnimationClip> ATTACK_List = new();
    public List<AnimationClip> DAMAGED_List = new();
    public List<AnimationClip> DEBUFF_List = new();
    public List<AnimationClip> DEATH_List = new();
    public List<AnimationClip> OTHER_List = new();

    private void Awake()
    {
        StateAnimationPairs = new Dictionary<string, List<AnimationClip>>();
        OverrideControllerInit();
        PopulateAnimationLists();
    }
    public void OverrideControllerInit()
    {
        if (_anim == null)
        {
            Debug.LogError("Animator component is missing!");
            return;
        }

        OverrideController = new AnimatorOverrideController();
        OverrideController.runtimeAnimatorController = _anim.runtimeAnimatorController;

        if (_anim.runtimeAnimatorController == null)
        {
            Debug.LogError("RuntimeAnimatorController is missing!");
            return;
        }

        AnimationClip[] clips = _anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            OverrideController[clip.name] = clip;
        }

        _anim.runtimeAnimatorController = OverrideController;

        foreach (PlayerState state in System.Enum.GetValues(typeof(PlayerState)))
        {
            var stateText = state.ToString();
            if (!StateAnimationPairs.ContainsKey(stateText))
            {
                StateAnimationPairs[stateText] = new List<AnimationClip>();
            }

            switch (state)
            {
                case PlayerState.IDLE:
                    StateAnimationPairs[stateText] = IDLE_List;
                    break;
                case PlayerState.MOVE:
                    StateAnimationPairs[stateText] = MOVE_List;
                    break;
                case PlayerState.ATTACK:
                    StateAnimationPairs[stateText] = ATTACK_List;
                    break;
                case PlayerState.DAMAGED:
                    StateAnimationPairs[stateText] = DAMAGED_List;
                    break;
                case PlayerState.DEBUFF:
                    StateAnimationPairs[stateText] = DEBUFF_List;
                    break;
                case PlayerState.DEATH:
                    StateAnimationPairs[stateText] = DEATH_List;
                    break;
                case PlayerState.OTHER:
                    StateAnimationPairs[stateText] = OTHER_List;
                    break;
            }
        }
    }
    public bool allListsHaveItemsExist()
    {
        List<List<AnimationClip>> allLists = new List<List<AnimationClip>>()
        {
            IDLE_List, MOVE_List, ATTACK_List, DAMAGED_List, DEBUFF_List, DEATH_List, OTHER_List
        };

        return allLists.All(list => list.Count > 0);
    }
    [ContextMenu("PopulateAnimationLists")]
    public void PopulateAnimationLists()
    {
        IDLE_List = new();
        MOVE_List = new();
        ATTACK_List = new();
        DAMAGED_List = new();
        DEBUFF_List = new();
        DEATH_List = new();
        OTHER_List = new();

        var groupedClips = spumPackages
        .SelectMany(package => package.SpumAnimationData)
        .Where(spumClip => spumClip.HasData &&
                        spumClip.UnitType.Equals(UnitType) &&
                        spumClip.index > -1)
        .GroupBy(spumClip => spumClip.StateType)
        .ToDictionary(
            group => group.Key,
            group => group.OrderBy(clip => clip.index).ToList()
        );
        foreach (var kvp in groupedClips)
        {
            var stateType = kvp.Key;
            var orderedClips = kvp.Value;
            switch (stateType)
            {
                case "IDLE":
                    IDLE_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "MOVE":
                    MOVE_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "ATTACK":
                    ATTACK_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "DAMAGED":
                    DAMAGED_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "DEBUFF":
                    DEBUFF_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "DEATH":
                    DEATH_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "OTHER":
                    OTHER_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
            }
        }

    }
    public void PlayAnimation(PlayerState PlayState, int index)
    {
        string stateKey = PlayState.ToString();

        if (!StateAnimationPairs.ContainsKey(stateKey))
        {
            Debug.LogWarning($"Animation state {stateKey} not found in StateAnimationPairs. Initializing empty list.");
            StateAnimationPairs[stateKey] = new List<AnimationClip>();
            return;
        }

        var animations = StateAnimationPairs[stateKey];
        if (animations == null || animations.Count == 0 || index >= animations.Count)
        {
            Debug.LogWarning($"No animations available for state {stateKey} at index {index}");
            return;
        }

        Animator animator = _anim;
        OverrideController[stateKey] = animations[index];

        bool isMove = stateKey.Contains("MOVE");
        bool isDebuff = stateKey.Contains("DEBUFF");
        bool isDeath = stateKey.Contains("DEATH");

        animator.SetBool("1_Move", isMove);
        animator.SetBool("5_Debuff", isDebuff);
        animator.SetBool("isDeath", isDeath);

        if (!isMove && !isDebuff)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    bool isTrigger = parameter.name.ToUpper().Contains(stateKey.ToUpper());
                    if (isTrigger)
                    {
                        animator.SetTrigger(parameter.name);
                    }
                }
            }
        }
    }
    AnimationClip LoadAnimationClip(string clipPath)
    {
        AnimationClip clip = Resources.Load<AnimationClip>(clipPath.Replace(".anim", ""));

        if (clip == null)
        {
            Debug.LogWarning($"Failed to load animation clip '{clipPath}'.");
        }

        return clip;
    }
}
