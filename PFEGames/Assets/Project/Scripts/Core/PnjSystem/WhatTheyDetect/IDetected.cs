using UnityEngine;

namespace Goblfin.PnjSystem.WhatTheyDetect
{
	public interface IDetected
	{
		Transform transform { get; }
		float speed { get; }
	}
}