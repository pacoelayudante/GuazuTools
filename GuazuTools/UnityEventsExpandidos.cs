using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UnityEventFloat : UnityEvent<float> {}
[System.Serializable]
public class UnityEventBool : UnityEvent<bool> { }
[System.Serializable]
public class UnityEventInt : UnityEvent<int> { }
