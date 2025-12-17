using System;

namespace Plants;

public class PlantStats
{
    public float Idratazione  = 1.0f;
    public float Salute  = 1.0f;
    public float Ossigeno  = 1.0f;    
    public float ResistenzaTemp  = 0.0f; // se + resiste meglio al caldo, se - al freddo
    public float ResistenzaPar = 0.0f; // se + resiste meglio al caldo, se - al freddo
    public float Metabolismo  = 1.0f;  // quanto in fretta cresce


    public int Foglie = 50;
    public float DropRateFoglie  = 0.01f; // percentuale di foglie che cadono 
    
}