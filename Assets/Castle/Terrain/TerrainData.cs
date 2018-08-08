using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TerrainGeneratorData {

    public readonly float[,] heighmap;

    public TerrainGeneratorData( float[,] heighmap ) {
        this.heighmap = heighmap;
    }

    public TerrainGeneratorData( int resolution ) {
        heighmap = new float[resolution, resolution];
    }
}

