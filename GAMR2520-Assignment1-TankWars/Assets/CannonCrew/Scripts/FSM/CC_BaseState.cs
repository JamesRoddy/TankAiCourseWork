using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseST 
{
// the base state class is an abstact class emanign that it cannot be defined but can be inherited from as a template for other classes 
// this allows for mutliple states to be defined with these core three methods but have their own unquie take on  and functionality define dthrough polymorphism 
// where these functions can be redefined by the child classes of the BaseST class to have theeir own implmentation this allows for more flexibility in terms of code reuse 
// as due to all the states inheriting from this class they can all be interpeted as the BaseST parent class alllowing for a plug and play apsect to be applied to the state machine
// making itterative testing and development  of states much easier 
    public abstract Type Entry();
    public abstract Type Update();
    public abstract Type Exit();




}
