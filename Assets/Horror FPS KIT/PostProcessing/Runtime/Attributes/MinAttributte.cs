namespace UnityEngine.PostProcessing
{
    public sealed class MinAttributte : PropertyAttribute
    {
        public readonly float min;

        public MinAttributte(float min)
        {
            this.min = min;
        }
    }
}
