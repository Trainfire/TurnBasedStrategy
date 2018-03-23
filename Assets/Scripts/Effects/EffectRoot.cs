using UnityEngine;
using System.Collections.Generic;

public class EffectRoot : MonoBehaviour
{
    private List<EffectBase> _effects;

    private void Awake()
    {
        _effects = new List<EffectBase>();
        GetComponents(_effects);
    }

    public void Apply(GameboardHelper gameboardHelper, UnitAttackEvent unitAttackEvent)
    {
        _effects.ForEach(x => x.Apply(gameboardHelper, unitAttackEvent));
        Destroy(gameObject);
    }
}