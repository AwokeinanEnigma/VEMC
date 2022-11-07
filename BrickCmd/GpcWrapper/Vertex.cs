namespace GpcWrapper
{
    public struct Vertex
    {
        public Vertex(double x, double y)
        {
            X = x;
            Y = y;
        }
        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "(",
                X.ToString(),
                ",",
                Y.ToString(),
                ")"
            });
        }
        public double X;
        public double Y;
    }
}
