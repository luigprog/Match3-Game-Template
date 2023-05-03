public class Counter2D
{
    private int i;
    private int j;
    private int iTarget;
    private int jTarget;
    private bool invertDimensions;

    public int I { get { return i; } }
    public int J { get { return j; } }

    public Counter2D(int iTarget, int jTarget)
    {
        this.iTarget = iTarget;
        this.jTarget = jTarget;
    }

    public void InvertDimensions(bool invertDimensions)
    {
        this.invertDimensions = invertDimensions;
    }

    public void Reset()
    {
        i = 0;
        j = 0;
    }

    public bool Completed()
    {
        return i > iTarget || j > jTarget;
    }

    public bool SecondaryDimensionCompleted()
    {
        return (!invertDimensions && i >= iTarget) ||
               (invertDimensions && j >= jTarget);
    }

    public void Advance()
    {
        if (!invertDimensions)
        {
            i++;
            if (i > iTarget)
            {
                j++;
                if (j <= jTarget)
                {
                    i = 0;
                }
            }
        }
        else
        {
            j++;
            if (j > jTarget)
            {
                i++;
                if (i <= iTarget)
                {
                    j = 0;
                }
            }
        }
    }
}