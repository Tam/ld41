namespace utils
{
	public static class MathHelpers
	{
		public static float Map (
			float value,
			float leftMin,
			float leftMax,
			float rightMin,
			float rightMax
		) {
			return rightMin
				   + (value - leftMin) 
				   * (rightMax - rightMin) 
				   / (leftMax - leftMin);
		}
	}
}
