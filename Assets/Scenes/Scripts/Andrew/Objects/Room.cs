using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : Object_
{
    public Object_ rightObject;
    public Object_ leftObject;
    public Object_ upObject;
    public Object_ downObject;
    public Object_ straightAheadObject;
    public RoomSO roomSO;

    public void Start()
    {
        if (roomSO != null)
        {
            rightObject = roomSO.rightObject;
            leftObject = roomSO.leftObject;
            upObject = roomSO.upObject;
            downObject = roomSO.downObject;
            straightAheadObject = roomSO.straightAheadObject;
        }
    }

    public Object_ GetPart(ConcentrationKey key)
    {
        switch (key)
        {
            case ConcentrationKey.Right: return rightObject;
            case ConcentrationKey.Left: return leftObject;
            case ConcentrationKey.Up: return upObject;
            case ConcentrationKey.Down: return downObject;
            case ConcentrationKey.Straight_Ahead: return straightAheadObject;
            default: return null;
        }
    }
}
