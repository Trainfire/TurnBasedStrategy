using UnityEngine;
using System.Collections.Generic;

public class Effect : MonoBehaviour
{
    private List<EffectComponent> _effects;

    private void Awake()
    {
        _effects = new List<EffectComponent>();
        GetComponents(_effects);
    }

    public void Apply(GameboardHelper gameboardHelper, UnitAttackEvent unitAttackEvent)
    {
        _effects.ForEach(x => x.Apply(gameboardHelper, unitAttackEvent));
        Destroy(gameObject);
    }
}