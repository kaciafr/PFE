using System;
using System.Collections.Generic;
using UnityEngine;

namespace Goblfin
{
	public enum TypeOfResolution
	{
		Water,
		Reparation,
		None,
	}

	public static class ResolveRegistry
	{
		private static readonly Dictionary<TypeOfResolution, HashSet<ResolutionSource>> Sources = new();
		private static readonly HashSet<ResolutionSource> Empty = new();

		public static IReadOnlyCollection<ResolutionSource> GetResolve(TypeOfResolution type)
			=> Sources.TryGetValue(type, out var set) ? set : Empty;

		public static void Register(ResolutionSource source)
		{
			if (!Sources.TryGetValue(source.Type, out var set))
				Sources[source.Type] = set = new HashSet<ResolutionSource>();
			set.Add(source);
		}

		public static void Unregister(ResolutionSource source)
		{
			if (Sources.TryGetValue(source.Type, out var set))
				set.Remove(source);
		}

		public static TypeOfResolution WhatForWhat(TypeOfProblem problem) => problem switch
		{
			TypeOfProblem.Burn => TypeOfResolution.Water,
			TypeOfProblem.Broke => TypeOfResolution.Reparation,
			_ => TypeOfResolution.None,
		};

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset() => Sources.Clear();
	}
}