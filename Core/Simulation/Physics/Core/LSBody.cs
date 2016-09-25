﻿//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace Lockstep
{
	[System.Serializable]
	public partial class LSBody : MonoBehaviour
	{
		#region Core deterministic variables

		[SerializeField] //For inspector debugging
		internal Vector2d _position;
		[SerializeField]
		internal Vector2d _rotation = Vector2d.up;
		[SerializeField, FixedNumber]
		internal long _heightPos;
		[SerializeField]
		public Vector2d _velocity;

		#endregion

		#region Lockstep variables

		private bool _forwardNeedsSet = false;

		private bool ForwardNeedsSet {
			get { return _forwardNeedsSet; }
			set { _forwardNeedsSet = value; }
		}




		private Vector2d _forward;

		public Vector2d Forward {
			get {
				return Rotation.ToDirection ();
			}
			set {
				Rotation = value.ToRotation ();
			}
		}

		[Lockstep]
		public bool PositionChanged { get; set; }

		[Lockstep]
		public Vector2d Position {
			get {
				return _position;
			}
			set {
				_position = value;
				this.PositionChanged = true;
			}
		}

		private bool _rotationChanged;

		[Lockstep]
		public bool RotationChanged {
			get {
				return _rotationChanged;
			}
			set {
				if (value)
					ForwardNeedsSet = true;
				_rotationChanged = value;
			}
		}


		[Lockstep]
		public Vector2d Rotation {
			get {
				return _rotation;
			}
			set {
				_rotation = value;
				this.RotationChanged = true;
			}
		}

		[Lockstep]
		public bool HeightPosChanged { get; set; }

		[Lockstep]
		public long HeightPos {
			get { return _heightPos; }
			set {
				_heightPos = value;
				this.HeightPosChanged = true;
			}
		}

		[Lockstep]
		public bool VelocityChanged { get; set; }

		[Lockstep]
		public Vector2d Velocity {
			get { return _velocity; }
			set {
				_velocity = value;
				VelocityChanged = true;
			}
		}

		public Vector2d LastPosition { get; private set; }

		internal uint RaycastVersion { get; set; }

		#endregion

		internal Vector3 _visualPosition;

		public Vector3 VisualPosition { get { return _visualPosition; } }

		public LSAgent Agent { get; private set; }

		public long FastRadius { get; private set; }

		public bool PositionChangedBuffer { get; private set; }
		//D
		public bool SetPositionBuffer { get; private set; }
		//ND


		public bool RotationChangedBuffer { get; private set; }
		//D
		private bool SetRotationBuffer { get; set; }
		//ND

		bool SetVisualPosition { get; set; }

		bool SetVisualRotation { get; set; }

		private bool Setted { get; set; }


		public long VelocityFastMagnitude { get; private set; }


		public bool Active {get; private set;}

		private void AddChild (LSBody_ child)
		{
			if (Children == null)
				Children = new FastBucket<LSBody_> ();
			Children.Add (child);
		}

		private void RemoveChild (LSBody_ child)
		{
			Children.Remove (child);
		}

		private FastBucket<LSBody_> Children;
		public Vector2d[] RealPoints;
		public Vector2d[] Edges;
		public Vector2d[] EdgeNorms;


		public long XMin { get; private set; }

		public long XMax { get; private set; }

		public long YMin { get; private set; }

		public long YMax { get; private set; }

		public int PastGridXMin;
		public int PastGridXMax;
		public int PastGridYMin;
		public int PastGridYMax;

		public long HeightMin { get; private set; }

		public long HeightMax { get; private set; }

		public delegate void CollisionFunction (LSBody_ other);

		private Dictionary<int,CollisionPair> _collisionPairs;
		private HashSet<int> _collisionPairHolders;

		internal Dictionary<int,CollisionPair> CollisionPairs {
			get {
				return _collisionPairs.IsNotNull () ? _collisionPairs : (_collisionPairs = new Dictionary<int, CollisionPair> ());
			}
		}

		internal HashSet<int> CollisionPairHolders {
			get {
				return _collisionPairHolders ?? (_collisionPairHolders = new HashSet<int> ());
			}
		}

		internal void NotifyContact (LSBody_ other, bool isColliding, bool isChanged)
		{
			if (isColliding) {
				if (isChanged) {
					if (onContactEnter.IsNotNull ())
						onContactEnter (other);
				}
				if (onContact != null)
					onContact (other);

			} else {
				if (isChanged) {
					if (onContactExit != null)
						onContactExit (other);
				}
			}
		}

		public event CollisionFunction onContact;
		public event CollisionFunction onContactEnter;
		public event CollisionFunction onContactExit;

		public int ID { get; private set; }

		private int _dynamicID = -1;

		internal int DynamicID { get { return _dynamicID; } set { _dynamicID = value; } }

		public int Priority { get; set; }

		#region Serialized

		[SerializeField]
		protected ColliderType _shape = ColliderType.None;

		public ColliderType Shape { get { return _shape; } }

		[SerializeField]
		private bool _isTrigger;

		public bool IsTrigger { get { return _isTrigger; } }

		[SerializeField]
		private int _layer;

		public int Layer { get { return _layer; } }

		[SerializeField,FixedNumber]
		private long _halfWidth = FixedMath.Half;

		public long HalfWidth { get { return _halfWidth; } }

		[SerializeField,FixedNumber]
		public long _halfHeight = FixedMath.Half;

		public long HalfHeight { get { return _halfHeight; } }

		[SerializeField,FixedNumber]
		protected long _radius = FixedMath.Half;

		public long Radius { get { return _radius; } }

		[SerializeField]
		protected bool _immovable;

		public bool Immovable { get; private set; }

		[SerializeField]
		private int _basePriority;

		public int BasePriority { get { return _basePriority; } }

		[SerializeField]
		private Vector2d[] _vertices;

		public Vector2d[] Vertices { get { return _vertices; } }

		[SerializeField, FixedNumber]
		private long _height = FixedMath.One;

		public long Height { get {return _height;} }


		[SerializeField]
		private Transform _positionalTransform;

		public Transform PositionalTransform { get ; set; }


		[SerializeField]
		private Transform _rotationalTransform;

		public Vector3 _rotationOffset;

		public Transform RotationalTransform { get; set; }


		#endregion

		#region Runtime Values

		private bool _canSetVisualPosition;

		public bool CanSetVisualPosition {
			get {
				return _canSetVisualPosition;
			}
			set {
				_canSetVisualPosition = value && PositionalTransform != null;
			}
		}

		private bool _canSetVisualRotation;

		public bool CanSetVisualRotation {
			get {
				return _canSetVisualRotation && RotationalTransform != null;
			}
			set {
				_canSetVisualRotation = value;
			}
		}

		public Vector3d Position3d {
			get {
				return this.Position.ToVector3d (this.HeightPos);
			}
		}

		private Vector2d[] RotatedPoints;


		#endregion

	}

}