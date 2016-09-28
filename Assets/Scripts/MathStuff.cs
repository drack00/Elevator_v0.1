using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MathStuff
{
	public static void ImpulseForce (Rigidbody rigidbody, Vector3 force) {
		rigidbody.AddForce (-1 * rigidbody.velocity, ForceMode.VelocityChange);
		rigidbody.AddForce (force, ForceMode.VelocityChange);
	}
	public static void ImpulseTorque (Rigidbody rigidbody, Vector3 torque) {
		rigidbody.AddTorque (-1 * rigidbody.angularVelocity, ForceMode.VelocityChange);
		rigidbody.AddTorque (torque, ForceMode.VelocityChange);
	}
	public static Transform GetClosestTransform (Vector3 reference, Transform[] transforms){
		Transform tMin = null;
		float minDist = Mathf.Infinity;
		foreach (Transform transform in transforms) {
			float dist = Vector3.Distance (reference, transform.position);
			if (dist < minDist) {
				tMin = transform;
				minDist = dist;
			}
		}
		return tMin;
	}
	public static Vector3 GetClosestPosition (Vector3 reference, Vector3[] positions) {
		Vector3 tMin = Vector3.zero;
		float minDist = Mathf.Infinity;
		foreach (Vector3 position in positions) {
			float dist = Vector3.Distance (reference, position);
			if (dist < minDist) {
				tMin = position;
				minDist = dist;
			}
		}
		return tMin;
	}
	public static Transform GetClosestTransform (Quaternion reference, Transform[] transforms){
		Transform tMin = null;
		float minDist = Mathf.Infinity;
		foreach (Transform transform in transforms) {
			float dist = Quaternion.Angle (reference, transform.rotation);
			if (dist < minDist) {
				tMin = transform;
				minDist = dist;
			}
		}
		return tMin;
	}
	public static Vector3 GetClosestRotation (Quaternion reference, Vector3[] rotations) {
		Quaternion[] _rotations = new Quaternion[rotations.Length];
		for (int i = 0; i < _rotations.Length; i++) {
			_rotations [i] = Quaternion.Euler (rotations [i]);
		}
		return (GetClosestRotation (reference, _rotations)).eulerAngles;
	}
	public static Quaternion GetClosestRotation (Quaternion reference, Quaternion[] rotations) {
		Quaternion tMin = Quaternion.identity;
		float minDist = Mathf.Infinity;
		foreach (Quaternion rotation in rotations) {
			float dist = Quaternion.Angle (reference, rotation);
			if (dist < minDist) {
				tMin = rotation;
				minDist = dist;
			}
		}
		return tMin;
	}
	public static float SignedAngleBetween (Vector2 a, Vector2 b)
	{
		float angle = Vector2.Angle (a, b);
		float sign = Mathf.Sign (Vector2.Dot (a, b));

		float signedAngle = angle * sign;

		return signedAngle * Mathf.Deg2Rad;
	}
	public static float SignedAngleBetween (Vector3 a, Vector3 b, Vector3 n)
	{
		float angle = Vector3.Angle (a, b);
		float sign = Mathf.Sign (Vector3.Dot (n, Vector3.Cross (a, b)));

		float signedAngle = angle * sign;

		return signedAngle * Mathf.Deg2Rad;
	}
	public static float FullAngleBetween (Vector2 a, Vector2 b)
	{
		float signedAngle = SignedAngleBetween (a, b);

		float angle360 = (signedAngle + 180) % 360;

		return angle360 * Mathf.Deg2Rad;
	}
	public static float FullAngleBetween (Vector3 a, Vector3 b, Vector3 n)
	{
		float signedAngle = SignedAngleBetween (a, b, n);

		float angle360 = (signedAngle + 180) % 360;

		return angle360 * Mathf.Deg2Rad;
	}

	//check if 2 shapes overlap
	public static List<Vector3[]> OverlapShapes (GameObject stencil, GameObject fill) {
		//cache all fill and stencil points
		List<Vector2> fillPoints = new List<Vector2> ();
		for (int i = 0; i < fill.GetComponent<PolygonCollider2D> ().points.Length; i++) {
			fillPoints.Add (fill.transform.TransformPoint (fill.GetComponent<PolygonCollider2D> ().points [i]));
		}
		List<Vector2> stencilPoints = new List<Vector2> ();
		for (int i = 0; i < stencil.GetComponent<PolygonCollider2D> ().points.Length; i++) {
			stencilPoints.Add (stencil.transform.TransformPoint (stencil.GetComponent<PolygonCollider2D> ().points [i]));
		}

		//find all fill/stencil intersections and draw overlap polygons
		List<List<Vector2>> _match = new List<List<Vector2>> ();
		for (int i = 0; i < fillPoints.Count; i++) {
			//get points being considered
			Vector2 entryStart = fillPoints [i];
			Vector2 entryEnd = i < fillPoints.Count - 1 ? fillPoints [i + 1] : fillPoints [0];

			//get valid entry points between start and end
			RaycastHit2D[] entryHits = Physics2D.LinecastAll(entryStart, entryEnd);
			for (int j = 0; j < entryHits.Length; j++) {
				RaycastHit2D entryHit = entryHits [j];

				//add valid entry points to list
				if (entryHit.collider.gameObject == stencil && !Mathf.Approximately (entryHit.fraction, Mathf.Epsilon)) {
					//new polygon
					_match.Add (new List<Vector2> ());
					_match [_match.Count - 1].Add (entryHit.point);
				}
			}

			//iterate through valid entry points
			for (int j = 0; j < _match.Count; j++) {
				//continue until the polygon is closed
				int entryIndex = i;
				while (_match[j].Count < 2 || entryIndex != i) {
					//iterate through fill points
					for (int k = 1; k < fillPoints.Count; k++) {
						//get index relative to entry
						int _index = entryIndex + k;
						if (_index >= fillPoints.Count)
							_index -= fillPoints.Count;

						//get points being considered
						Vector2 exitEnd = fillPoints [_index];

						//check for exits before the next corner
						bool foundExit = false;
						RaycastHit2D[] exitHits = Physics2D.LinecastAll (exitEnd, _match [_match.Count - 1] [_match [_match.Count - 1].Count - 1]);
						for (int l = exitHits.Length - 1; l >= 0; l--) {
							RaycastHit2D exitHit = exitHits [l];

							//is this point valid
							if (exitHit.collider.gameObject == stencil && !Mathf.Approximately (exitHit.fraction, Mathf.Epsilon)) {
								//add valid exit point to polygon
								_match [j].Add (exitHit.point);

								//end the exit loop
								foundExit = true;
								break;
							}
						}

						//end the exit loop
						if (foundExit)
							break;

						//add the exit point to the polygon
						if (PolyContainsPoint(stencilPoints, exitEnd))
							_match [j].Add (exitEnd);
					}
						
					//find exit on stencil
					for (int k = 0; k < stencilPoints.Count; k++) {
						//get points being considered
						Vector2 traceStart = stencilPoints [k];
						Vector2 traceEnd = k < stencilPoints.Count - 1 ? stencilPoints [k + 1] : stencilPoints [0];

						//check for entrys before the next corner
						bool foundEntry = false;
						RaycastHit2D[] _entryHits = Physics2D.LinecastAll (traceStart, traceEnd);
						for (int l = 0; l < _entryHits.Length; l++) {
							RaycastHit2D _entryHit = _entryHits [l];

							//is this point valid, and equal to the last point
							if (_entryHit.collider.gameObject == fill && !Mathf.Approximately (_entryHit.fraction, Mathf.Epsilon) && _entryHit.point == _match [j] [_match [j].Count - 1]) {
								//this is the last point on the other shape
								entryIndex = k;

								//end the entry loop
								foundEntry = true;
								break;
							}
						}

						//end the entry loop
						if (foundEntry)
							break;
					}
					//iterate through stencil points
					for (int k = 1; k < stencilPoints.Count; k++) {
						//get index relative to entry
						int _index = entryIndex + k;
						if (_index >= stencilPoints.Count)
							_index -= stencilPoints.Count;

						//get points being considered
						Vector2 exitEnd = stencilPoints [_index];

						//check for exits before the next corner
						bool foundExit = false;
						RaycastHit2D[] exitHits = Physics2D.LinecastAll (exitEnd, _match [_match.Count - 1] [_match [_match.Count - 1].Count - 1]);
						for (int l = exitHits.Length - 1; l >= 0; l--) {
							RaycastHit2D exitHit = exitHits [l];

							//is this point valid
							if (exitHit.collider.gameObject == fill && !Mathf.Approximately (exitHit.fraction, Mathf.Epsilon)) {
								//add valid exit points to polygon
								_match [j].Add (exitHit.point);

								//end the exit loop
								foundExit = true;
								break;
							}
						}

						//end the exit loop
						if (foundExit)
							break;

						//add the exit point to the polygon
						if (PolyContainsPoint(fillPoints, exitEnd))
							_match [j].Add (exitEnd);
					}
				
					//find exit on fill
					for (int k = 0; k < fillPoints.Count; k++) {
						//get points being considered
						Vector2 traceStart = fillPoints [k];
						Vector2 traceEnd = k < fillPoints.Count - 1 ? fillPoints [k + 1] : fillPoints [0];

						//check for entrys before the next corner
						bool foundEntry = false;
						RaycastHit2D[] _entryHits = Physics2D.LinecastAll (traceStart, traceEnd);
						for (int l = 0; l < _entryHits.Length; l++) {
							RaycastHit2D _entryHit = _entryHits [l];

							//is this point valid, and equal to the last point
							if (_entryHit.collider.gameObject == stencil && !Mathf.Approximately (_entryHit.fraction, Mathf.Epsilon) && _entryHit.point == _match [j] [_match [j].Count - 1]) {
								//this is the last point on the other shape
								entryIndex = k;

								//end the entry loop
								foundEntry = true;
								break;
							}
						}

						//end the entry loop
						if (foundEntry)
							break;
					}
				}

				//remove duplicate points
				for (int k = 0; k < _match[j].Count; k++) {
					for (int l = _match[j].Count - 1; l > k; l--) {
						if (_match [j] [k] == _match [j] [l])
							_match [j].RemoveAt (l);
					}
				}
			}
		}
			
		//contingency for instances with no crossover
		if (_match.Count < 1) {
			//solitary polygon
			_match.Add (new List<Vector2> ());

			//for instances when the fill is larger than the stencil
			foreach (Vector2 point in stencilPoints) {
				if (PolyContainsPoint(fillPoints, point))
					_match [_match.Count - 1].Add (point);
			}

			//for instances when the stencil is larger than the fill
			if(_match[_match.Count - 1].Count < 1) {
				foreach (Vector2 point in fillPoints) {
					if (PolyContainsPoint(stencilPoints, point))
						_match [_match.Count - 1].Add (point);
				}
			}
		} else {
			//remove duplicate polygons
			for (int i = 0; i < _match.Count; i++) {
				for (int j = _match.Count - 1; j > i; j--) {
					if (SameShape (_match [i], _match [j]))
						_match.RemoveAt (j);
				}
			}
		}

		//convert into collection
		List<Vector3[]> match = new List<Vector3[]>();
		foreach (List<Vector2> _poly in _match) {
			Vector3[] poly = new Vector3[_poly.Count];
			for (int i = 0; i < poly.Length; i++) {
				poly [i] = new Vector3 (_poly [i].x, _poly [i].y, 0.0f);
			}
			match.Add (poly);
		}

		//return collection
		return match;
	} 

	public static bool SameShape (List<Vector2> a, List<Vector2> b) {
		if (a.Count == b.Count) {
			for (int i = 0; i < a.Count; i++) {
				for (int j = 0; j < b.Count; j++) {
					bool sameShape = true;

					for (int k = 0; k < b.Count; k++) {
						int _indexA = i + k;
						while (_indexA >= a.Count)
							_indexA -= a.Count;

						int _indexB = j + k;
						while (_indexB >= b.Count)
							_indexB -= b.Count;

						Vector2 pointA = a [_indexA];
						Vector2 pointB = b [_indexB];

						if (!(Mathf.Approximately (pointA.x, pointB.x) && Mathf.Approximately (pointA.y, pointB.y)))
							sameShape = false;
					}

					if (sameShape)
						return true;
				}
			}
		}

		return false;
	}

	public static bool LineOfSight (Vector2 start, Vector2 end, GameObject filter){
		bool forwardsHit = true;
		RaycastHit2D[] traceForward = Physics2D.LinecastAll (start, end, LayerMask.GetMask (LayerMask.LayerToName (filter.layer)));
		foreach (RaycastHit2D hit in traceForward) {
			if (hit.collider.gameObject == filter) {
				forwardsHit = Mathf.Approximately (Mathf.Epsilon, hit.fraction);
				break;
			}
		}
		bool backwardsHit = true;
		RaycastHit2D[] traceBackward = Physics2D.LinecastAll (end, start, LayerMask.GetMask (LayerMask.LayerToName (filter.layer)));
		foreach (RaycastHit2D hit in traceBackward) {
			if (hit.collider.gameObject == filter) {
				backwardsHit = Mathf.Approximately (Mathf.Epsilon, hit.fraction);
				break;
			}
		}

		return forwardsHit && backwardsHit;
	}
	public static bool PolyContainsPoint (List<Vector2> polyPoints, Vector2 p){
		Vector2[] _polyPoints = new Vector2[polyPoints.Count];
		for (int i = 0; i < _polyPoints.Length; i++) {
			_polyPoints [i] = polyPoints [i];
		}
		return PolyContainsPoint (_polyPoints, p);
	}
	public static bool PolyContainsPoint (Vector2[] polyPoints, Vector2 p){
		int j = polyPoints.Length - 1;
		bool inside = false;
		for (int i = 0; i < polyPoints.Length; j = i++) {
			if (((polyPoints [i].y <= p.y && p.y <= polyPoints [j].y) || (polyPoints [j].y <= p.y && p.y <= polyPoints [i].y)) &&
			   (p.x < (polyPoints [j].x - polyPoints [i].x) * (p.y - polyPoints [i].y) / (polyPoints [j].y - polyPoints [i].y) + polyPoints [i].x))
				inside = !inside;
		}
		return inside;
	}

	public static float SurfaceArea (Vector2[] vertices) {
		float result = 0.0f;
		int j = 0;
		for(int i = vertices.Length - 1; j < vertices.Length; i = j++) {
			result += Vector3.Cross (vertices [j], vertices [i]).magnitude;
		}
		return result;
	}

	public static bool IsBetween (Vector2 a, Vector2 b, Vector2 c){
		float crossProduct = (c.y - a.y) * (b.x - a.x) - (c.x - a.x) * (b.y - a.y);
		if (Mathf.Abs (crossProduct) > Mathf.Epsilon)
			return false;

		float dotProduct = (c.x - a.x) * (b.x - a.x) + (c.y - a.y) * (b.y - a.y);
		if (dotProduct < 0.0f)
			return false;

		float squaredLengthBA = Mathf.Pow (b.x - a.x, 2) + Mathf.Pow (b.y - a.y, 2);
		if (dotProduct > squaredLengthBA)
			return false;

		return true;
	}

	public static float IrregularSurfaceArea (List<Vector2> points) {
		Vector2[] _points = new Vector2[points.Count];
		for (int i = 0; i < _points.Length; i++) {
			_points [i] = points [i];
		}

		return IrregularSurfaceArea (_points);
	}
	public static float IrregularSurfaceArea (Vector2[] points) {
		float temp = 0.0f;
		for (int i = 0; i < points.Length; i++) {
			Vector2 start = points [i];
			Vector2 end = i < points.Length - 1 ? points [i + 1] : points [0];

			float mulA = start.x * end.y;
			float mulB = end.x * start.y;

			temp += (mulA - mulB);
		}
		temp *= 0.5f;
		return Mathf.Abs (temp);
	}

	public static Vector3 RotateAroundPivot (Vector3 point, Vector3 pivot, Quaternion rotation){
		return rotation * (point - pivot) + pivot;
	}
}