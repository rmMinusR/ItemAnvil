using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temperature : ItemInstanceProperty
{
    public const float ROOM_TEMP = 70;
    
    [Min(0)] public float temperature = ROOM_TEMP;
    [Min(0)] public float heatLoss = 1;

    public override bool ShouldTick => true;
    public override void Tick()
    {
        temperature = Mathf.MoveTowards(temperature, ROOM_TEMP, heatLoss);
    }

    protected override TooltipEntry GetTooltipEntry()
    {
        throw new System.NotImplementedException();
    }
}
