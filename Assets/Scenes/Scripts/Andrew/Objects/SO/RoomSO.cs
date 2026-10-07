using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomSO", menuName = "Objects/RoomSO", order = 1)]
public class RoomSO : ScriptableObject
{
    public Object_ rightObject;
    public Object_ leftObject;
    public Object_ upObject;
    public Object_ downObject;
    public Object_ straightAheadObject;
}
