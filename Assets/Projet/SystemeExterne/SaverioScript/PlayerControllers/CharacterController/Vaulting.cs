using System.Linq;
using CharacterController.Script.PlayerControllers.Utilities;
using Script.PlayerControllers;
using UnityEngine;

namespace CharacterController.Script.PlayerControllers
{
	public static class Vaulting
	{
		public static void TryVault(CharacterSetup characterSetup, float currentSpeed)
		{
			var data = characterSetup.CharacterData;

			var maxScanDistance = data.vaultAnimations.Max(a => a.forwardCheckDistance);
			var maxScanHeight = data.vaultAnimations.Max(a => a.maxObstacleHeight);

			if (!TryDetectObstacle(characterSetup, maxScanDistance, maxScanHeight, out var topPoint,
				    out var obstacleHeight))
				return;


			var selected = SelectAnimData(currentSpeed, obstacleHeight, data);
			if (selected == null)
				return;

			characterSetup.GoTo(new VaultState(topPoint, selected));
		}

		private static bool TryDetectObstacle(CharacterSetup characterSetup, float forwardDistance, float maxScanHeight,
			out Vector3 topPoint, out float obstacleHeight)
		{
			topPoint = Vector3.zero;
			obstacleHeight = 0f;

			var pos = characterSetup.transform;
			var data = characterSetup.CharacterData;
			var origin = pos.position + Vector3.zero * data.vaultOrigin;

			if (!RaycastChecker.Check(origin, pos.forward, forwardDistance, GameLayers.Vaultable, out var forwardHit,
				    new Color(0, 1, 0)))
				return false;

			var topCheckOrigin = forwardHit.point + pos.forward * 0.1f + Vector3.up * maxScanHeight;
			if (!RaycastChecker.Check(topCheckOrigin, Vector3.down, maxScanHeight, GameLayers.Vaultable, out var topHit,
				    Color.yellow))
				return false;

			topPoint = topHit.point;
			obstacleHeight = topHit.point.y - pos.position.y;
			return true;
		}


		private static VaultAnimData SelectAnimData(float speed, float obstacleHeight, CharacterData data)
		{
			foreach (var entry in data.vaultAnimations)
			{
				var heightMatches = obstacleHeight >= entry.minObstacleHeight &&
				                    obstacleHeight <= entry.maxObstacleHeight;
				var speedMatches = speed >= entry.speedMin;

				if (heightMatches && speedMatches)
					return entry;
			}

			return null;
		}
	}
}