using System;
using Gameplay.Player.Domain.Combat;
using UnityEngine;


namespace Gameplay.Player.Presentation.Combat
{
	[Serializable]
	public struct DiceShotFaceDescriptor
	{
		[field: SerializeField] public DiceFace         Face    { get; private set; }
		[field: SerializeField] public ParticleSystem[] ShotVfx { get; private set; }
	}
}