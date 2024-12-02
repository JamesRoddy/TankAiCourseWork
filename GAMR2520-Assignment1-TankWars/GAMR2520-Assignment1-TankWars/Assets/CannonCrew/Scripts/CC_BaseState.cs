using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState : MonoBehaviour
{
    public abstract Type Entry();
    public abstract Type Update();
    public abstract Type Exit();




}
