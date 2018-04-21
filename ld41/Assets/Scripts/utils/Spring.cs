namespace utils
{
	public class Spring
	{
		
		// Properties
		// =====================================================================

		public float springConstant = 0.015f;
		public float damping = 0.07f;
		public float velocity;
		public float acceleration;

		private float _springPosition;
		private float _neutralPosition = 0f;

		// Actions
		// =====================================================================

		public float Simulate ()
		{
			float force = springConstant * (_springPosition - _neutralPosition)
						  + velocity * damping;

			acceleration = -force;
			_springPosition += velocity;
			velocity += acceleration;

			return _springPosition;
		}

		public void ApplyForceStartingAtPosition (float force, float position)
		{
			acceleration = 0f;
			_springPosition = position;
			velocity = force;
		}

		public void ApplyAdditiveForce (float force)
		{
			velocity += force;
		}

	}
}
