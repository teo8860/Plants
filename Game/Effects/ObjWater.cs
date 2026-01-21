using Raylib_CSharp.Colors;

namespace Plants;


public class ObjWater : ObjParticle
{
    public ObjWater()
    {
        this.defaultData = new();
        this.defaultData.radius = 8.5f;
        this.defaultData.color = Color.Blue;
        
        this.defaultData.gravity_min.X = -0.2f;
        this.defaultData.gravity_max.X = 0.2f;

        this.defaultData.gravity_min.Y = -0.2f;
        this.defaultData.gravity_max.Y = -0.2f;
    }

}